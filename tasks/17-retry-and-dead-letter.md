# Tarefa 17 — Implementar retry progressivo e Dead Letter Queue

**Status:** Backlog  
**Fase:** Processamento assíncrono  
**Depende de:** Tarefas 14 e 15

## Problema

Falhas transitórias e mensagens inválidas ainda não possuem destinos distintos, podendo causar perda ou loop infinito.

## Objetivo

Implementar retentativas com atraso e encaminhar mensagens irrecuperáveis ou esgotadas para a DLQ.

## Implementação esperada

- Classificar sucesso, falha de negócio, falha transitória e falha permanente.
- Configurar filas de retry com TTL/dead-letter routing e atrasos progressivos limitados.
- Transportar número de tentativa e metadados originais sem alterar `MessageId`.
- Enviar para DLQ após o limite ou imediatamente em envelope irrecuperável.
- Registrar motivo final e fornecer procedimento documentado de inspeção/reprocessamento.

## Critérios de aceite

- Saldo insuficiente termina como `Failed` e recebe ack sem retry.
- Indisponibilidade temporária retorna à fila após atraso observável.
- Tentativas nunca excedem a configuração.
- Mensagem na DLQ preserva payload, IDs, erro e contagem de tentativas.

