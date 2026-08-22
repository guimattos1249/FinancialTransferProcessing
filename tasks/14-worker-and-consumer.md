# Tarefa 14 — Criar Worker e consumidor de transferências

**Status:** Backlog  
**Fase:** Processamento assíncrono  
**Depende de:** Tarefa 12

## Problema

Não existe processo consumidor para executar as transferências publicadas.

## Objetivo

Adicionar `FinancialTransferProcessing.Worker` e consumir `TransferRequested` com ack manual.

## Implementação esperada

- Criar projeto Worker com referências e injeção de dependência coerentes com as camadas existentes.
- Manter conexão/canal de longa duração e criar escopo por entrega.
- Configurar prefetch e limite de concorrência.
- Desserializar e validar envelope, rejeitando versão/tipo inválidos segundo política explícita.
- Encaminhar mensagens válidas a um contrato de processamento da Application.
- Adicionar o Worker à solution, Dockerfile/Compose e health checks aplicáveis.

## Critérios de aceite

- Uma mensagem válida chama o processador uma vez por entrega.
- Ack não ocorre antes do retorno bem-sucedido do processador.
- Payload inválido não derruba o loop de consumo e recebe destino conhecido.
- O número de entregas simultâneas respeita a configuração.

