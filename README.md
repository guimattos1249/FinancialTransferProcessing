# Financial Transfer Processing

Sistema de processamento assíncrono de transferências financeiras desenvolvido com **.NET, RabbitMQ, .NET Worker Service e Redis**, explorando conceitos de arquitetura distribuída, mensageria, concorrência, idempotência, observabilidade e performance.

O projeto simula um fluxo de transferência inspirado no funcionamento de sistemas de pagamento instantâneo: a API recebe uma solicitação de transferência e responde imediatamente, enquanto a liquidação financeira acontece de forma assíncrona através de uma fila durável no **RabbitMQ**, consumida por um **.NET Worker Service**. O **Redis** complementa a arquitetura em cenários de cache, idempotência de acesso rápido e rate limiting distribuído.

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
                         │        RabbitMQ         │
                         │                         │
                         │ transfer-processing q.  │
                         └────────────┬────────────┘
                                      │
                                      │ Consume / manual ack
                                      ▼
                         ┌─────────────────────────┐
                         │  .NET Worker Service    │
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

Redis atua ao lado da API e do Worker como cache distribuído e mecanismo auxiliar de rate limiting e idempotência. O banco de dados permanece como fonte de verdade para saldos, transferências e mensagens processadas.

A aplicação é dividida em dois principais entrypoints:

```text
ASP.NET Core API
        │
        ▼
Application

.NET Worker Service
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
3. cria a transferência como `Pending` e registra a mensagem no outbox na mesma transação;
4. confirma a transação no banco;
5. responde imediatamente ao cliente.

Em background, o publicador do outbox envia uma mensagem persistente ao RabbitMQ. Esse envio não faz parte do tempo de resposta da requisição.

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

# 📨 RabbitMQ

O **RabbitMQ** é responsável por desacoplar o recebimento da transferência de sua liquidação. A API publica comandos e o Worker os consome de maneira independente, permitindo que cada componente seja escalado e reiniciado sem interromper o outro.

```text
API
 │
 │ TransferRequested
 ▼
RabbitMQ exchange: financial-transfers
 │
 │ routing key: transfer.requested
 ▼
queue: transfer-processing
 │
 │ competing consumer
 ▼
.NET Worker Service
```

Exemplo conceitual da mensagem:

```json
{
  "messageId": "f32bb87a-...",
  "transferId": "9c1f8b2e-...",
  "occurredAt": "2026-08-12T15:00:00Z"
}
```

A mensagem contém apenas as informações necessárias para identificar e processar a transferência. Os dados financeiros completos são recuperados do banco pelo `transferId`, evitando que informações de saldo fiquem desatualizadas dentro da fila.

A topologia utiliza:

- exchange durável `financial-transfers` para receber os comandos;
- routing key `transfer.requested` para encaminhar o comando;
- fila durável `transfer-processing`, preferencialmente do tipo **quorum**;
- mensagens persistentes para sobreviver ao reinício do broker;
- publisher confirms para a aplicação confirmar que o RabbitMQ recebeu a publicação;
- acknowledgements manuais para remover a mensagem somente após o processamento bem-sucedido;
- prefetch e limite de concorrência configuráveis para controlar quantas transferências cada instância processa simultaneamente.

O **Transactional Outbox** elimina a janela de inconsistência entre persistir a transferência e publicar o comando. A API salva a transferência `Pending` e a mensagem do outbox na mesma transação de banco; um publicador em background entrega as mensagens pendentes ao RabbitMQ e marca cada item como publicado somente depois do publisher confirm.

---

# ⚙️ .NET Worker Service

A liquidação é executada por um **.NET Worker Service** baseado no Generic Host. O processo mantém uma conexão com o RabbitMQ, aguarda mensagens da fila `transfer-processing` e cria um escopo de injeção de dependência para cada entrega.

```text
ProcessTransferWorker
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

O Worker funciona apenas como entrypoint para o processamento. O consumidor desserializa e valida o envelope, propaga `MessageId` e `CorrelationId`, chama o caso de uso da camada de aplicação e decide o destino da mensagem:

- **ack** depois que a transação de liquidação for confirmada;
- **retry** quando ocorrer uma falha transitória;
- **dead letter** quando a mensagem exceder o limite de tentativas ou contiver um erro não recuperável.

Várias instâncias do Worker podem consumir a mesma fila como **competing consumers**. O RabbitMQ distribui as mensagens entre elas, enquanto prefetch e concorrência limitam a pressão sobre o banco. No encerramento, o Worker interrompe novas entregas, aguarda os processamentos em andamento dentro do limite configurado e devolve ao broker qualquer mensagem que não tenha recebido ack.

As regras financeiras permanecem na camada de aplicação/domínio.

---

# ⚡ Redis

O **Redis** é utilizado como componente auxiliar para rate limiting distribuído, cache do status das transferências e consultas rápidas de chaves de idempotência ou mensagens recentemente processadas. O banco de dados continua sendo a fonte de verdade, e Redis não participa do controle de saldo, do lock financeiro nem substitui o RabbitMQ.

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

A idempotência também é aplicada ao processamento das mensagens para impedir que uma mensagem entregue novamente pelo RabbitMQ resulte em uma segunda movimentação financeira.

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
   .NET Worker Services
      ↙  ↓  ↓  ↓  ↘
     T1 T2 T3 ... T200
            │
            ▼
        Account A
```

Mesmo com várias instâncias concorrentes do Worker, o sistema deve garantir:

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

Nesses casos, o Worker não envia o ack definitivo e a mensagem pode ser processada novamente.

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
RabbitMQ

Message
   │
   ▼
Worker
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

A topologia separa a fila principal, a fila de retry e a DLQ:

```text
transfer-processing
        │
        ├── transient failure ──► transfer-processing.retry.*
        │                              │
        │                              └── delay / TTL ──► transfer-processing
        │
        └── attempts exhausted ─► transfer-processing.dlq
```

O retry usa atraso progressivo e um limite de tentativas. Erros de negócio definitivos, como saldo insuficiente, atualizam a transferência para `Failed` e recebem ack; não devem ser repetidos como falhas de infraestrutura. A DLQ evita loops infinitos e permite investigação ou reprocessamento controlado.

---

# 🧾 Reconciliação financeira

Além do processamento das transferências, o projeto possui uma rotina de **reconciliação financeira**.

Um serviço hospedado no projeto Worker executa a reconciliação em intervalos configuráveis e analisa as operações de determinado período.

```text
Periodic scheduler
     │
     ▼
ReconciliationWorker
     │
     ├── Total processed
     ├── Total failed
     ├── Financial volume
     └── Possible inconsistencies
```

O agendamento respeita cancelamento, impede sobreposição de execuções e registra o checkpoint do último período reconciliado. Dessa forma, uma reinicialização do Worker não perde nem duplica silenciosamente uma janela de reconciliação.

---

# 🔀 Orquestração no Worker

O workflow de reconciliação é orquestrado em C# pelo próprio Worker, mantendo o fluxo versionado, testável e executável em qualquer ambiente.

```text
Scheduled Trigger
       │
       ▼
ReconciliationWorker
       │
       ▼
Start Reconciliation
       │
       ▼
Application Use Case
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

O Worker é responsável apenas por agendamento e **orquestração do processo**. As regras de reconciliação permanecem na camada de aplicação, e uma divergência pode gerar uma métrica, um log estruturado e um evento no RabbitMQ para integrações futuras.

---

# 📊 Observabilidade

O projeto utiliza **OpenTelemetry**, logs estruturados e correlation IDs para acompanhar o comportamento da API, do RabbitMQ e do Worker sem depender de um provedor específico de observabilidade.

Algumas métricas relevantes:

```text
Request latency

Messages processed / second

Worker processing time

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
RabbitMQ Message
     │
CorrelationId
     │
     ▼
.NET Worker Service
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

Independentemente do volume de requisições ou número de instâncias do Worker executando simultaneamente, algumas regras nunca podem ser violadas.

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

├── FinancialTransferProcessing.API
│
├── FinancialTransferProcessing.Application
│   ├── Accounts
│   ├── Transfers
│   ├── Reconciliation
│   └── Contracts
│
├── FinancialTransferProcessing.Domain
│   ├── Entities
│   ├── Enums
│   └── Exceptions
│
├── FinancialTransferProcessing.Infrastructure
│   ├── Persistence
│   ├── Messaging
│   ├── Caching
│   └── Repositories
│
└── FinancialTransferProcessing.Worker
    ├── Consumers
    │   └── ProcessTransferConsumer
    ├── Outbox
    └── Reconciliation


tests/

├── FinancialTransferProcessing.UnitTests
├── FinancialTransferProcessing.IntegrationTests
├── FinancialTransferProcessing.ConcurrencyTests
└── LoadTests
```

---

# 🛠️ Tecnologias

### Backend

- C#
- .NET
- ASP.NET Core
- Entity Framework Core

### Messaging and background processing

- RabbitMQ
- .NET Worker Service
- Transactional Outbox
- Redis
- OpenTelemetry

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

- [ ] Configurar RabbitMQ e sua topologia
- [ ] Implementar Transactional Outbox
- [ ] Publicar `TransferRequested`
- [ ] Criar `FinancialTransferProcessing.Worker`
- [ ] Criar `ProcessTransferConsumer`
- [ ] Implementar liquidação
- [ ] Implementar controle de concorrência
- [ ] Implementar retry
- [ ] Configurar Dead Letter Queue
- [ ] Configurar acknowledgements, publisher confirms e graceful shutdown
- [ ] Configurar Redis para rate limiting e cache auxiliar

## Phase 3 — Reconciliation

- [ ] Criar rotina de reconciliação
- [ ] Criar serviço agendado no Worker
- [ ] Implementar orquestração da reconciliação em C#
- [ ] Criar fluxo de tratamento de divergências

## Phase 4 — Observability

- [ ] Configurar OpenTelemetry
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
- .NET Worker Services
- Message Queues
- Competing Consumers
- Transactional Outbox
- Distributed Cache
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
