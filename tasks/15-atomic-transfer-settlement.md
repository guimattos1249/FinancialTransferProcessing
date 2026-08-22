# Tarefa 15 — Liquidar transferência de forma atômica e idempotente

**Status:** Backlog  
**Fase:** Processamento assíncrono  
**Depende de:** Tarefa 14

## Problema

O consumidor ainda não movimenta saldo nem conclui a transferência com garantias transacionais.

## Objetivo

Implementar o caso de uso que debita payer, credita payee e finaliza a transferência em uma única transação.

## Implementação esperada

- Carregar a transferência e as duas contas para atualização.
- Usar `ProcessedMessage` como inbox durável por `MessageId` na mesma transação financeira.
- Ignorar com sucesso mensagens já processadas e transferências em estado terminal.
- Em saldo insuficiente, marcar a transferência `Failed` com motivo estável, sem alterar saldos.
- Em saldo suficiente, debitar, creditar e marcar `Completed` atomicamente.
- Classificar falhas de negócio e infraestrutura para orientar o consumidor.

## Critérios de aceite

- Completed conserva a soma dos saldos e registra `ProcessedAt`.
- Saldo insuficiente não altera nenhuma conta e termina como Failed.
- Exceção em qualquer etapa reverte toda a unidade de trabalho, inclusive inbox.
- Redelivery do mesmo `MessageId` não movimenta saldo novamente.

