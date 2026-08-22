# Tarefa 8 — Consultar transferência por ID - DONE

**Status:** Pronto  
**Fase:** Core  
**Depende de:** Tarefa 7

## Problema

O cliente recebe uma transferência `Pending`, mas ainda não consegue acompanhar sua liquidação sem consultar diretamente o banco.

## Objetivo

Criar `GET /api/v1/Transfer/{id}` para retornar o estado atual e os dados públicos da transferência.

## Implementação esperada

- Criar caso de uso, contrato de resposta e consulta no repository com `AsNoTracking`.
- Retornar ID, payer, payee, valor, status, datas de criação/processamento e motivo de falha quando aplicável.
- Retornar HTTP 404 para transferência inexistente.
- Não expor a entidade de domínio diretamente.
- Documentar respostas 200, 400 e 404 no OpenAPI.

## Critérios de aceite

- Transferências `Pending`, `Completed` e `Failed` são representadas corretamente.
- `failureReason` só possui valor para uma falha e `processedAt` só após um estado terminal.
- Identificador inválido é rejeitado e um ID inexistente retorna 404.
- Consulta usa `AsNoTracking` e propaga o `CancellationToken` recebido.

