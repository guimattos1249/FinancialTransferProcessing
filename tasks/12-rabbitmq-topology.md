# Tarefa 12 — Configurar RabbitMQ, contratos e topologia

**Status:** Backlog  
**Fase:** Processamento assíncrono  
**Depende de:** Tarefa 10

## Problema

Não existe broker configurado nem uma topologia reproduzível para transportar `TransferRequested`.

## Objetivo

Adicionar a infraestrutura RabbitMQ e declarar a topologia durável da aplicação.

## Implementação esperada

- Adicionar RabbitMQ ao Docker Compose com health check, volume e credenciais configuráveis.
- Criar opções fortemente tipadas e validação no startup.
- Declarar exchange `financial-transfers`, routing key `transfer.requested` e fila quorum `transfer-processing`.
- Preparar exchanges/filas de retry e `transfer-processing.dlq` sem implementar ainda a política de retentativa.
- Centralizar nomes e configuração; não espalhar literais pelo código.

## Critérios de aceite

- A topologia pode ser declarada repetidamente sem erro.
- Exchange, filas e bindings sobrevivem ao reinício do broker.
- Configuração inválida falha cedo com mensagem clara.
- O ambiente local sobe com banco, API e RabbitMQ saudáveis.

