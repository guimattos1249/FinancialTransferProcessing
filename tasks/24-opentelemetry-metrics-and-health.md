# Tarefa 24 — Adicionar OpenTelemetry, métricas e health checks

**Status:** Backlog  
**Fase:** Observabilidade  
**Depende de:** Tarefas 18, 20, 22 e 23

## Problema

Logs isolados não mostram latência distribuída, capacidade, backlog e saúde operacional do sistema.

## Objetivo

Instrumentar API, PostgreSQL, Redis, RabbitMQ, outbox, Worker e reconciliação com padrões OpenTelemetry.

## Implementação esperada

- Configurar traces e métricas com exportador OTLP configurável e comportamento seguro quando ausente.
- Propagar contexto W3C nos headers das mensagens e criar spans de producer/consumer.
- Medir latência HTTP, publicação, processamento, retries, falhas, DLQ, cache e reconciliação.
- Expor liveness e readiness distintas para API e Worker.
- Evitar atributos de alta cardinalidade como IDs em métricas; mantê-los apenas em traces/logs.

## Critérios de aceite

- Um trace conecta POST, outbox, publish, consume e liquidação.
- Métricas permitem calcular throughput e p50/p95/p99 sem cardinalidade descontrolada.
- Readiness reflete dependências críticas e liveness não provoca restart por falha externa transitória.
- Desabilitar exportação não quebra a aplicação.

