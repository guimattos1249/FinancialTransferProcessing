# Tarefa 11 — Gravar transferência e evento atomicamente

**Status:** Backlog  
**Fase:** Processamento assíncrono  
**Depende de:** Tarefa 10

## Problema

A transferência `Pending` ainda pode ser salva sem o comando que dará início à liquidação.

## Objetivo

Alterar a criação para persistir `Transfer` e `TransferRequested` no outbox na mesma transação do banco.

## Implementação esperada

- Criar a mensagem do outbox somente para uma nova transferência.
- Salvar transferência e mensagem em um único `SaveChanges`/commit.
- Preservar o comportamento de replay da chave de idempotência sem duplicar mensagens.
- Propagar o correlation ID recebido pela API para transferência e envelope.
- Tratar rollback e violações concorrentes sem deixar registros órfãos.

## Critérios de aceite

- Nova solicitação gera exatamente uma transferência e uma mensagem relacionada.
- Repetição idempotente não cria novo item de outbox.
- Falha forçada na gravação de qualquer registro não persiste o outro.
- Corridas pela mesma chave resultam em uma transferência e uma mensagem.

