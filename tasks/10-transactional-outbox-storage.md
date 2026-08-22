# Tarefa 10 — Modelar e persistir o transactional outbox

**Status:** Pronto  
**Fase:** Processamento assíncrono  
**Depende de:** Tarefa 7

## Problema

Publicar diretamente no RabbitMQ após salvar a transferência cria uma janela em que o banco confirma e a mensagem se perde.

## Objetivo

Criar o modelo persistente de outbox e o contrato versionado `TransferRequested`.

## Implementação esperada

- Criar entidade/tabela de outbox com `MessageId`, tipo, versão, payload, ocorrência, tentativas, próxima tentativa, publicação e último erro.
- Definir envelope contendo `messageId`, `transferId`, `occurredAt`, `correlationId` e versão do schema.
- Criar configuração EF, repository e migration com índices para buscar mensagens publicáveis.
- Definir tamanho máximo e política de serialização do payload.
- Impedir alteração do identificador e do conteúdo após a criação.

## Critérios de aceite

- A migration sobe e desce em PostgreSQL limpo.
- Mensagens pendentes podem ser consultadas por lote e em ordem estável.
- Mensagens publicadas deixam de ser selecionadas.
- Contrato possui teste de serialização compatível e datas em UTC.

