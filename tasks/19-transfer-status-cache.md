# Tarefa 19 — Cachear consultas de status com Redis

**Status:** Backlog  
**Fase:** Redis  
**Depende de:** Tarefas 8 e 15

## Problema

Polling frequente do status pode pressionar o PostgreSQL, embora o banco deva permanecer como fonte de verdade.

## Objetivo

Adicionar Redis como cache auxiliar do endpoint de consulta de transferência.

## Implementação esperada

- Adicionar Redis ao Compose, opções tipadas, health check e abstração na Application.
- Implementar cache-aside com chave versionada e TTL configurável.
- Atualizar/invalidar o cache após transição para `Completed` ou `Failed`.
- Tratar Redis indisponível como cache miss sem impedir criação, liquidação ou consulta no banco.
- Evitar cachear respostas inexistentes por período excessivo.

## Critérios de aceite

- Segunda consulta elegível pode ser atendida pelo Redis.
- Estado terminal aparece sem servir `Pending` além da política definida.
- Queda do Redis não altera correção nem disponibilidade do fluxo financeiro.
- Chaves, TTL, hits, misses e falhas são observáveis sem incluir dados sensíveis.

