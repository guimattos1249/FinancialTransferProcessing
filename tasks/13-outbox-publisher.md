# Tarefa 13 — Publicar o outbox com publisher confirms

**Status:** Backlog  
**Fase:** Processamento assíncrono  
**Depende de:** Tarefas 11 e 12

## Problema

As mensagens ficam seguras no banco, mas ainda não chegam ao RabbitMQ.

## Objetivo

Criar um publicador em background que entregue lotes do outbox e marque confirmação somente após o broker confirmar.

## Implementação esperada

- Buscar lotes pendentes com intervalo, tamanho e paralelismo configuráveis.
- Publicar mensagens persistentes com tipo, versão, message ID, correlation ID e content type nos headers/properties.
- Usar publisher confirms e só então preencher `PublishedAt`.
- Registrar tentativas e aplicar atraso progressivo para falhas transitórias.
- Evitar publicação concorrente do mesmo registro por múltiplas instâncias, usando lease/lock apropriado no PostgreSQL.

## Critérios de aceite

- Falha antes do confirm mantém a mensagem disponível para nova tentativa.
- Reinício do processo não perde mensagens.
- Duas instâncias não publicam deliberadamente o mesmo lote; duplicatas residuais continuam toleradas pelo consumidor.
- Métricas e logs identificam lote, mensagem, tentativa e resultado.

