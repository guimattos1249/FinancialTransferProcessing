# Tarefa 18 — Fechar o ciclo confiável das mensagens

**Status:** Backlog  
**Fase:** Processamento assíncrono  
**Depende de:** Tarefas 13 e 17

## Problema

Publicador e consumidor precisam continuar corretos durante perda de conexão, cancelamento e encerramento dos processos.

## Objetivo

Concluir ack/nack, recovery, backpressure e graceful shutdown da API/publicador e do Worker.

## Implementação esperada

- Emitir ack somente após commit de sucesso ou resultado de negócio terminal persistido.
- Não dar ack em cancelamento ou falha antes do commit.
- Recuperar conexão/canal e redeclarar topologia após queda do RabbitMQ.
- No shutdown, interromper novas entregas, aguardar operações em andamento por prazo configurável e requeue do restante.
- Aplicar readiness degradada quando broker/banco impedirem processamento e manter liveness significativa.

## Critérios de aceite

- Reinício forçado do Worker durante liquidação não causa débito duplicado nem perda.
- Queda e retorno do RabbitMQ retomam publicação e consumo sem reiniciar toda a stack.
- Limites de prefetch/concorrência impedem crescimento ilimitado em memória.
- Encerramento respeita o timeout e deixa mensagens não confirmadas disponíveis.

