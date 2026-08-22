# Tarefa 25 — Cobrir o fluxo completo com testes de integração

**Status:** Adiado  
**Fase:** Qualidade pós-core  
**Depende de:** Tarefa 9

> Esta tarefa está deliberadamente adiada até a conclusão do core funcional e não bloqueia a prontidão operacional inicial.

## Problema

Testes de unidade não comprovam as garantias obtidas pela combinação de PostgreSQL, RabbitMQ, Redis, API e Worker.

## Objetivo

Criar uma suíte end-to-end isolada que exercite a jornada real da transferência.

## Implementação esperada

- Subir dependências descartáveis com Testcontainers e aplicar migrations.
- Exercitar criação → outbox → RabbitMQ → Worker → consulta terminal.
- Cobrir sucesso, saldo insuficiente, replay HTTP, redelivery RabbitMQ, retry e DLQ.
- Testar queda do broker/Worker em pontos controlados e posterior recuperação.
- Coletar diagnóstico útil em falha sem depender de sleeps fixos longos.

## Critérios de aceite

- Cada cenário começa com estado limpo e termina verificando banco e broker.
- Testes comprovam ausência de movimentação parcial e duplicada.
- Suíte é repetível localmente e em CI.
- Timeouts são explícitos e falhas mostram o estágio da jornada que não concluiu.

