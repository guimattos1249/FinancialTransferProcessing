# Tarefa 7 — Criar transferência pendente com idempotência - DONE

## Problema

A API ainda não possui o fluxo principal do sistema: receber uma solicitação de transferência e persistir a intenção para processamento assíncrono.

A criação da solicitação não deve movimentar o saldo imediatamente e requisições repetidas não podem gerar transferências duplicadas.

## Objetivo

Criar uma transferência entre duas contas existentes, garantindo idempotência e persistindo-a inicialmente com status `Pending`.

## Endpoint

```http
POST /api/v1/Transfer
Idempotency-Key: payment-123
```

## Request

```json
{
  "payerId": "01900000-0000-0000-0000-000000000001",
  "payeeId": "01900000-0000-0000-0000-000000000002",
  "amountInCents": 5000
}
```

## Resposta

```http
202 Accepted
```

```json
{
  "id": "01900000-0000-0000-0000-000000000003",
  "payerId": "01900000-0000-0000-0000-000000000001",
  "payeeId": "01900000-0000-0000-0000-000000000002",
  "amountInCents": 5000,
  "status": "Pending"
}
```

## Implementação esperada

- Criar contratos de leitura e escrita para `Transfer`.
- Criar `ICreateTransferUseCase` e `CreateTransferUseCase`.
- Criar request, response e validator.
- Criar o repository de transferências com Entity Framework.
- Registrar repository e use case na injeção de dependência.
- Criar `TransferController`.
- Obter a chave pelo header `Idempotency-Key`.
- Persistir a transferência com status `Pending`.
- Salvar por meio de `IUnitOfWork`.
- Retornar HTTP 202.
- Não debitar nem creditar contas neste endpoint.

## Validações

- Payer e payee devem ser diferentes.
- Payer e payee não podem ser `Guid.Empty`.
- O valor deve ser maior que zero.
- A chave de idempotência é obrigatória.
- A chave deve respeitar o tamanho máximo definido pelo domínio.
- As duas contas devem existir.

## Comportamento idempotente

Antes de criar uma transferência, consultar pela chave de idempotência:

- Se a chave não existir, criar uma nova transferência.
- Se a chave existir com payer, payee e valor iguais, retornar a transferência existente.
- Se a chave existir com dados diferentes, retornar HTTP 409 Conflict.

O índice único do banco deve permanecer como garantia final contra concorrência. A aplicação deve tratar a violação desse índice porque duas requisições simultâneas podem concluir a consulta inicial antes que uma delas persista a transferência.

## Critérios de aceite

- Uma solicitação válida cria uma transferência com status `Pending`.
- O endpoint retorna HTTP 202.
- Nenhum saldo é alterado durante a criação.
- Payer ou payee inexistente resulta em HTTP 404.
- Payer igual ao payee resulta em HTTP 422.
- Valor inválido resulta em HTTP 400 ou 422, conforme a convenção adotada pela API.
- Repetir a mesma solicitação com a mesma chave retorna a transferência original.
- Reutilizar a chave com dados diferentes retorna HTTP 409.
- Requisições concorrentes não criam transferências duplicadas.
- Todas as operações propagam `CancellationToken`.
- A solução compila sem erros ou avisos.
