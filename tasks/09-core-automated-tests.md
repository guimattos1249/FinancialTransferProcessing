# Tarefa 9 — Criar fundação de testes e cobrir o core

**Status:** Adiado  
**Fase:** Qualidade pós-core  
**Depende de:** Tarefa 28

> Esta tarefa está deliberadamente adiada até a conclusão do core funcional e não bloqueia as tarefas de implementação.

## Problema

As regras já implementadas de conta, transferência, validação e idempotência não possuem uma rede de segurança automatizada.

## Objetivo

Adicionar projetos xUnit para testes unitários e de integração do core existente.

## Implementação esperada

- Criar `FinancialTransferProcessing.UnitTests` e `FinancialTransferProcessing.IntegrationTests` na solution.
- Cobrir débito, crédito, overflow, estados de transferência e validações de domínio.
- Cobrir criação e consulta de conta, criação idempotente de transferência e mapeamento de erros HTTP.
- Executar integração contra PostgreSQL real isolado, preferencialmente com Testcontainers.
- Fornecer builders/fixtures sem compartilhar estado mutável entre testes.

## Critérios de aceite

- Repetição idempotente, conflito de chave e corrida pela mesma chave possuem testes.
- Migrações são aplicadas no ambiente de integração.
- Testes são determinísticos, paralelizáveis quando seguro e não dependem do banco do desenvolvedor.
- `dotnet test` executa toda a suíte sem falhas ou testes ignorados.

