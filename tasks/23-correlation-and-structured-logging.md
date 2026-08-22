# Tarefa 23 — Propagar correlation ID e logs estruturados

**Status:** Backlog  
**Fase:** Observabilidade  
**Depende de:** Tarefas 13 e 14

## Problema

Não é possível seguir uma operação da requisição HTTP até o outbox, RabbitMQ e Worker de forma consistente.

## Objetivo

Padronizar correlação e logging estruturado ponta a ponta.

## Implementação esperada

- Aceitar/validar `X-Correlation-ID` ou gerar um valor e devolvê-lo na resposta.
- Persistir e transportar correlation ID no outbox e nos headers RabbitMQ.
- Criar scopes de log com `CorrelationId`, `MessageId`, `TransferId` e tentativa.
- Padronizar eventos e níveis para API, publicador, consumidor, retry e reconciliação.
- Aplicar redaction; não registrar credenciais, payload integral ou dados desnecessários.

## Critérios de aceite

- Um mesmo correlation ID aparece em toda a jornada de uma transferência.
- IDs inválidos/excessivos são rejeitados ou substituídos segundo política documentada.
- Logs são consultáveis por campos, não dependem de parsing de texto livre.
- Exceções preservam contexto sem duplicar stack traces em todas as camadas.

