# Tarefa 20 — Aplicar rate limiting distribuído

**Status:** Backlog  
**Fase:** Redis  
**Depende de:** Tarefa 19

## Problema

Uma instância ou cliente pode saturar a criação/consulta de transferências, e limites locais divergem quando a API escala horizontalmente.

## Objetivo

Implementar limites distribuídos por cliente nas rotas públicas usando Redis.

## Implementação esperada

- Definir chave confiável de partição (credencial/cliente; IP somente como fallback explícito).
- Configurar políticas separadas para POST de transferência e GET de status.
- Usar operação atômica no Redis, limites/janelas configuráveis e expiração obrigatória.
- Retornar HTTP 429 com `Retry-After` e headers de quota coerentes.
- Definir e documentar comportamento fail-open/fail-closed por endpoint quando Redis falhar.

## Critérios de aceite

- Duas instâncias compartilham o mesmo contador e não multiplicam a quota.
- Requisições dentro do limite continuam inalteradas; excedentes retornam 429.
- Chaves não crescem indefinidamente.
- Concorrência, expiração e indisponibilidade do Redis possuem comportamento definido e observável.

