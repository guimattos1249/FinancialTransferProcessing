# Financial Transfer Processing

Sistema de processamento assíncrono de transferências financeiras desenvolvido com **.NET e Microsoft Azure**, explorando conceitos de arquitetura distribuída, mensageria, serverless, concorrência, idempotência, observabilidade e performance.

O projeto simula um fluxo de transferência inspirado no funcionamento de sistemas de pagamento instantâneo: a API recebe uma solicitação de transferência e responde imediatamente, enquanto a liquidação financeira acontece de forma assíncrona através de **Azure Service Bus** e **Azure Functions**.

## 🎯 Objetivo

O principal objetivo deste projeto não é implementar um CRUD financeiro, mas explorar problemas comuns em sistemas distribuídos e aplicações financeiras:

- processamento assíncrono;
- concorrência sobre recursos compartilhados;
- consistência financeira;
- idempotência;
- processamento duplicado de mensagens;
- retries e tratamento de falhas;
- Dead Letter Queue;
- escalabilidade;
- observabilidade;
- throughput e latência;
- orquestração de workflows.

Um dos principais cenários abordados é o processamento de múltiplas transferências simultâneas sobre uma mesma conta.

Se uma conta possui saldo de **R$ 1.000,00** e recebe simultaneamente 200 solicitações que, somadas, ultrapassam esse valor, o sistema deve garantir que:

- nenhum dinheiro seja criado;
- nenhum dinheiro desapareça;
- o saldo nunca fique negativo;
- cada transferência seja liquidada no máximo uma vez.

---

# 🏗️ Arquitetura

```text
                         ┌─────────────────────────┐
                         │     ASP.NET Core API    │
                         │                         │
POST /transfers ────────►│ Validate Request        │
                         │ Create Pending Transfer │
                         └────────────┬────────────┘
                                      │
                                      │ Publish
                                      ▼
                         ┌─────────────────────────┐
                         │    Azure Service Bus    │
                         │                         │
                         │     transfers queue     │
                         └────────────┬────────────┘
                                      │
                                      │ ServiceBusTrigger
                                      ▼
                         ┌─────────────────────────┐
                         │     Azure Function      │
                         │                         │
                         │   Process Transfer      │
                         │   Check Balance         │
                         │   Handle Concurrency    │
                         │   Ensure Idempotency    │
                         └────────────┬────────────┘
                                      │
                                      ▼
                         ┌─────────────────────────┐
                         │       Database          │
                         │                         │
                         │ Accounts                │
                         │ Transfers               │
                         │ Processed Messages      │
                         └─────────────────────────┘
```

A aplicação é dividida em dois principais entrypoints:

```text
ASP.NET Core API
        │
        ▼
Application

Azure Functions
        │
        ▼
Application
```

As regras de negócio permanecem independentes da infraestrutura utilizada para executar o processamento.

---

# 🔄 Fluxo de uma transferência

O cliente envia:

```http
POST /transfers
```

```json
{
  "payerId": "acc-1",
  "payeeId": "acc-2",
  "amount": 2500,
  "idempotencyKey": "abc-123"
}
```

Valores monetários são representados em **centavos**, evitando problemas relacionados a operações de ponto flutuante.

A API:

1. valida a requisição;
2. verifica a chave de idempotência;
3. cria a transferência como `Pending`;
4. publica uma mensagem no Azure Service Bus;
5. responde imediatamente ao cliente.

```http
202 Accepted
```

```json
{
  "id": "9c1f8b2e-...",
  "status": "pending"
}
```

A API **não aguarda a liquidação financeira**.

---

# 📨 Azure Service Bus

O **Azure Service Bus** é responsável por desacoplar o recebimento da transferência de sua liquidação.

```text
API
 │
 │ TransactionRequested
 ▼
Azure Service Bus
 │
 │ ServiceBusTrigger
 ▼
Azure Function
```

Exemplo conceitual da mensagem:

```json
{
  "messageId": "f32bb87a-...",
  "transferId": "9c1f8b2e-...",
  "occurredAt": "2026-08-12T15:00:00Z"
}
```

A mensagem contém apenas as informações necessárias para identificar e processar a transferência.

---

# ⚡ Azure Functions

A liquidação é executada por uma **Azure Function** acionada através de um `ServiceBusTrigger`.

```text
ProcessTransferFunction
        │
        ▼
Get Transfer
        │
        ▼
Already processed?
   │            │
  Yes           No
   │            │
 Ignore         ▼
            Lock / concurrency control
                │
                ▼
          Check payer balance
              /       \
             /         \
       Sufficient    Insufficient
           │              │
           ▼              ▼
       Debit payer       Failed
       Credit payee
       Completed
```

A Function funciona apenas como um entrypoint para o processamento.

As regras financeiras permanecem na camada de aplicação/domínio.

---

# 💰 Estados da transferência

Uma transferência possui três estados:

```text
                       balance available
                ┌────────────────────────────► Completed
                │
Pending ────────┤
                │
                └────────────────────────────► Failed
                      insufficient funds
```

Uma transferência que chegou a `Completed` ou `Failed` nunca poderá ser processada novamente.

---

# 🔐 Idempotência

Toda transferência possui uma:

```text
idempotencyKey
```

Exemplo:

```json
{
  "idempotencyKey": "payment-order-928192"
}
```

Caso o cliente envie a mesma solicitação várias vezes:

```text
Request
Request
Request
Request
Request
```

apenas **uma transferência financeira poderá existir**.

Todas as solicitações seguintes retornam a transferência originalmente associada àquela chave.

A idempotência também é aplicada ao processamento das mensagens para impedir que uma mensagem entregue novamente pelo Service Bus resulte em uma segunda movimentação financeira.

---

# ⚔️ Concorrência

Um dos principais desafios do projeto é garantir consistência quando múltiplas transferências são processadas simultaneamente.

Exemplo:

```text
Account A

Balance: R$ 1.000

200 transferências simultâneas
            │
            ▼
      Azure Functions
      ↙  ↓  ↓  ↓  ↘
     T1 T2 T3 ... T200
            │
            ▼
        Account A
```

Mesmo com várias Functions concorrentes, o sistema deve garantir:

```text
Balance >= 0
```

em todos os cenários.

Serão estudadas estratégias como:

- transações no banco de dados;
- optimistic concurrency;
- pessimistic locking;
- isolamento transacional;
- serialização do processamento por conta.

O objetivo também é comparar os impactos dessas abordagens sobre **consistência e performance**.

---

# ♻️ Retry e tratamento de falhas

Falhas transitórias podem ocorrer durante o processamento.

Exemplos:

```text
Database unavailable
Network timeout
External provider unavailable
Temporary infrastructure failure
```

Nesses casos, uma mensagem pode ser processada novamente.

Por isso:

```text
Retry
   +
Idempotency
```

são tratados como conceitos complementares.

---

# ☠️ Dead Letter Queue

Mensagens que não conseguem ser processadas após sucessivas tentativas são direcionadas para uma **Dead Letter Queue (DLQ)**.

```text
Service Bus

Message
   │
   ▼
Function
   │
   X
 Retry
   │
   X
 Retry
   │
   X
 Retry
   │
   ▼
Dead Letter Queue
```

Isso evita retries infinitos e permite investigação ou reprocessamento controlado.

---

# 🧾 Reconciliação financeira

Além do processamento das transferências, o projeto possui uma rotina de **reconciliação financeira**.

Uma Azure Function executada por `TimerTrigger` analisa as operações de determinado período.

```text
TimerTrigger
     │
     ▼
ReconciliationFunction
     │
     ├── Total processed
     ├── Total failed
     ├── Financial volume
     └── Possible inconsistencies
```

Essa rotina permite explorar Azure Functions também no contexto de **jobs e cargas agendadas**.

---

# 🔀 Azure Logic Apps

O projeto utiliza **Azure Logic Apps** para orquestrar o workflow de reconciliação.

```text
Scheduled Trigger
       │
       ▼
   Logic App
       │
       ▼
Start Reconciliation
       │
       ▼
Azure Function
       │
       ▼
Result
    /       \
   /         \
Success    Divergence
  │            │
  ▼            ▼
Finish       Alert
```

A Logic App é responsável pela **orquestração do processo**, enquanto regras de negócio e processamento financeiro permanecem implementados em C#.

---

# 📊 Observabilidade

O projeto utiliza **Application Insights** para acompanhar o comportamento da aplicação.

Algumas métricas relevantes:

```text
Request latency

Messages processed / second

Function execution time

Failed transactions

Retry count

Dead-letter messages

Queue length

Database latency
```

Também são utilizados logs estruturados e correlation IDs para acompanhar uma transferência entre diferentes componentes.

Exemplo:

```text
HTTP Request
     │
CorrelationId
     │
     ▼
Service Bus Message
     │
CorrelationId
     │
     ▼
Azure Function
     │
CorrelationId
     ▼
Database
```

Isso permite rastrear uma operação de ponta a ponta.

---

# 🚀 Performance

O projeto inclui testes de carga utilizando **k6**.

Os testes avaliam principalmente:

- throughput;
- latência;
- concorrência;
- taxa de erros;
- comportamento sob alta carga.

Cenários planejados:

```text
100 concurrent requests

500 concurrent requests

1.000 transfers

10.000 transfers

200 transfers from the same payer
```

As principais métricas observadas serão:

```text
Requests / second

Transfers / second

p50 latency

p95 latency

p99 latency

Failure rate

Processing latency
```

Também serão comparados diferentes mecanismos de controle de concorrência.

---

# 🛡️ Invariantes

Independentemente do volume de requisições ou número de Functions executando simultaneamente, algumas regras nunca podem ser violadas.

### Conservation

A soma total dos saldos deve permanecer constante.

```text
Total Balance Before
        =
Total Balance After
```

### Non-negative balance

Nenhuma conta pode possuir saldo negativo.

```text
Balance >= 0
```

### Idempotency

A mesma chave de idempotência nunca pode resultar em múltiplos débitos.

### Atomicity

Uma transferência deve executar:

```text
Debit payer
+
Credit payee
+
Complete transfer
```

como uma única operação lógica.

Caso alguma etapa falhe, nenhuma movimentação parcial deve permanecer.

### Non-blocking API

O endpoint de criação da transferência não aguarda a liquidação.

```text
POST /transfers

        ↓

202 Accepted
```

O processamento ocorre posteriormente.

---

# 🧪 Testes

O projeto possui diferentes categorias de testes.

```text
Unit Tests
    │
    └── Domain rules

Integration Tests
    │
    ├── Database
    ├── Repository
    └── Transfer processing

Concurrency Tests
    │
    ├── simultaneous transfers
    ├── overdraft protection
    └── idempotency

Load Tests
    │
    └── k6
```

Um dos principais cenários consiste em enviar **200 transferências simultaneamente** sobre uma conta cujo saldo suporta apenas parte delas.

O resultado precisa continuar respeitando todos os invariantes financeiros.

---

# 🗂️ Estrutura

```text
src/

├── FinancialProcessing.Api
│
├── FinancialProcessing.Application
│   ├── Accounts
│   ├── Transfers
│   ├── Reconciliation
│   └── Contracts
│
├── FinancialProcessing.Domain
│   ├── Entities
│   ├── Enums
│   └── Exceptions
│
├── FinancialProcessing.Infrastructure
│   ├── Persistence
│   ├── Messaging
│   └── Repositories
│
└── FinancialProcessing.Functions
    ├── ProcessTransfer
    └── Reconciliation


tests/

├── FinancialProcessing.UnitTests
├── FinancialProcessing.IntegrationTests
├── FinancialProcessing.ConcurrencyTests
└── LoadTests
```

---

# 🛠️ Tecnologias

### Backend

- C#
- .NET
- ASP.NET Core
- Entity Framework Core

### Azure

- Azure Functions
- Azure Service Bus
- Azure Logic Apps
- Application Insights

### Database

- PostgreSQL

### Infrastructure

- Docker
- Docker Compose

### Tests

- xUnit
- k6

---

# 🗺️ Roadmap

## Phase 1 — Core

- [ ] Criar estrutura da solution
- [ ] Implementar `Account`
- [ ] Implementar `Transfer`
- [ ] Criar API REST
- [ ] Persistir transferências como `Pending`
- [ ] Implementar idempotência

## Phase 2 — Async Processing

- [ ] Configurar Azure Service Bus
- [ ] Publicar `TransactionRequested`
- [ ] Criar `ProcessTransferFunction`
- [ ] Implementar liquidação
- [ ] Implementar controle de concorrência
- [ ] Implementar retry
- [ ] Configurar Dead Letter Queue

## Phase 3 — Reconciliation

- [ ] Criar rotina de reconciliação
- [ ] Criar Azure Function com `TimerTrigger`
- [ ] Implementar Azure Logic App
- [ ] Criar fluxo de tratamento de divergências

## Phase 4 — Observability

- [ ] Configurar Application Insights
- [ ] Implementar structured logging
- [ ] Implementar correlation ID
- [ ] Criar métricas de processamento

## Phase 5 — Performance

- [ ] Criar testes com k6
- [ ] Testar transferências concorrentes
- [ ] Medir throughput
- [ ] Medir p50 / p95 / p99
- [ ] Comparar estratégias de concorrência
- [ ] Documentar resultados

---

# 📚 Conceitos explorados

Este projeto foi desenvolvido como estudo prático de:

- Distributed Systems
- Event-Driven Architecture
- Asynchronous Processing
- Serverless Computing
- Message Queues
- Financial Transactions
- Idempotency
- Concurrency Control
- Database Transactions
- Retry Strategies
- Dead Letter Queues
- Observability
- Distributed Tracing
- Performance Testing
- Workflow Orchestration
- Clean Architecture

---

# 📌 Status

🚧 **Em desenvolvimento**

O projeto está sendo construído incrementalmente, com foco não apenas na implementação das funcionalidades, mas também na análise das decisões arquiteturais, trade-offs de consistência e comportamento da aplicação sob concorrência e alta carga.