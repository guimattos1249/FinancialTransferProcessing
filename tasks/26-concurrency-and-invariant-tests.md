# Tarefa 26 — Validar invariantes financeiros sob concorrência

**Status:** Adiado  
**Fase:** Qualidade pós-core  
**Depende de:** Tarefas 16 e 25

## Problema

A estratégia de lock só é confiável se for validada com corridas reais e múltiplos consumidores.

## Objetivo

Criar uma suíte dedicada a concorrência e invariantes contra PostgreSQL e RabbitMQ reais.

## Implementação esperada

- Executar 200 transferências simultâneas do mesmo payer com saldo para apenas parte delas.
- Cobrir múltiplos payers, mesmo payee, transferências cruzadas e mensagens duplicadas.
- Rodar com mais de uma instância lógica do consumidor.
- Verificar conservação global, saldo não negativo, atomicidade, idempotência e estados terminais.
- Repetir cenários suficientes para aumentar a chance de expor races sem tornar a suíte aleatória.

## Critérios de aceite

- Nenhuma execução viola os invariantes definidos no README.
- Quantidade/volume de Completed corresponde exatamente ao saldo disponível.
- Duplicatas não causam débito adicional.
- Deadlocks/conflitos são recuperados dentro do limite e não deixam `Pending` sem justificativa.

