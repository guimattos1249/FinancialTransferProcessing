# Tarefa 27 — Medir carga e comparar estratégias de concorrência

**Status:** Adiado  
**Fase:** Qualidade pós-core  
**Depende de:** Tarefas 20 e 26

## Problema

O projeto ainda não possui números reproduzíveis de capacidade, latência ou efeito da contenção por conta.

## Objetivo

Criar cenários k6 e um procedimento de benchmark para medir API e processamento assíncrono.

## Implementação esperada

- Criar cenários de 100/500 clientes, 1.000/10.000 transferências e 200 transferências do mesmo payer.
- Separar latência de aceitação HTTP da latência até estado terminal.
- Medir RPS, transfers/s, p50/p95/p99, erros, retries, DLQ e backlog.
- Parametrizar URL, duração, credenciais, massa e thresholds.
- Comparar ao menos a estratégia adotada com uma alternativa controlada, sem comprometer o código de produção.

## Critérios de aceite

- Scripts não dependem de IDs fixos e preparam/limpam massa de forma segura.
- Thresholds objetivos fazem o teste falhar quando há regressão relevante.
- Resultado registra hardware, configuração, versões e número de Workers.
- Relatório documenta gargalos e decisão de concorrência com evidências.

