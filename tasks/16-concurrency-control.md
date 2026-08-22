# Tarefa 16 — Garantir saldos consistentes sob concorrência

**Status:** Backlog  
**Fase:** Processamento assíncrono  
**Depende de:** Tarefa 15

## Problema

Duas ou mais liquidações simultâneas sobre a mesma conta podem ler o mesmo saldo e permitir gasto duplicado ou perder atualizações.

## Objetivo

Adotar e documentar uma estratégia de concorrência no PostgreSQL que preserve os invariantes financeiros com múltiplos Workers.

## Implementação esperada

- Avaliar o token `Version` existente e locking pessimista/isolamento para a operação financeira.
- Definir ordem determinística de lock das duas contas para evitar deadlocks em transferências cruzadas.
- Tratar conflito de concorrência e deadlock como falha transitória com limite de tentativas.
- Garantir que consultas de liquidação não usem entidades stale nem tracking indevido.
- Registrar a decisão e seus trade-offs em ADR.

## Critérios de aceite

- Concorrência sobre o mesmo payer nunca produz saldo negativo.
- Transferências A→B e B→A concorrentes não entram em deadlock permanente.
- Não há lost update em payer nem payee.
- Os testes demonstram conservação de saldo com mais de uma instância de processamento.

