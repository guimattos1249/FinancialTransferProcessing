# Tarefa 21 — Implementar reconciliação financeira e checkpoint

**Status:** Backlog  
**Fase:** Reconciliação  
**Depende de:** Tarefa 15

## Problema

Não existe verificação posterior que detecte transferências presas, registros inconsistentes ou janelas não analisadas.

## Objetivo

Criar um caso de uso de reconciliação por período, com resultado reproduzível e checkpoint durável.

## Implementação esperada

- Modelar execução/checkpoint com início, fim, status e resumo.
- Calcular total processado, total falho, volume concluído e transferências pendentes acima do SLA.
- Detectar inconsistências possíveis entre status, `ProcessedAt` e registros de inbox.
- Processar janelas em UTC, com limites inclusivos/exclusivos sem lacuna ou sobreposição.
- Persistir checkpoint somente após sucesso e impedir duas execuções sobre a mesma janela.

## Critérios de aceite

- Reinício retoma da última janela concluída.
- Nova execução sobre o mesmo período produz o mesmo resultado financeiro.
- Falha parcial não avança checkpoint.
- Consultas são paginadas e não carregam todo o histórico em memória.

