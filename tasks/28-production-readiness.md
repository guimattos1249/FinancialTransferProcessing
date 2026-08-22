# Tarefa 28 — Fechar prontidão operacional e entrega contínua

**Status:** Backlog  
**Fase:** Encerramento  
**Depende de:** Tarefa 24

## Problema

Mesmo com o fluxo funcional, o projeto não estará concluído sem build reproduzível, validação automática e instruções de operação/recuperação.

## Objetivo

Finalizar containers, CI, configuração segura e documentação para executar e avaliar o sistema completo.

## Implementação esperada

- Criar imagens multi-stage e não-root para API e Worker, com shutdown e health checks corretos.
- Completar Compose com PostgreSQL, RabbitMQ, Redis, API, Worker e observabilidade opcional.
- Adicionar CI para restore, build com warnings controlados, migrations e validação dos containers.
- Remover segredos fixos de ambientes não locais e documentar todas as variáveis.
- Atualizar README com arquitetura realizada, setup, troubleshooting, DLQ/reprocessamento, migrations e resultados de performance.
- Revisar retenção/limpeza de outbox, inbox, cache e dados de reconciliação.

## Critérios de aceite

- Uma máquina limpa executa o sistema seguindo apenas o README.
- CI bloqueia regressões de build, migrations e formatação.
- Serviços rodam sem privilégio desnecessário e não expõem credenciais padrão fora do perfil local.
- Runbooks cobrem indisponibilidade do banco/broker/Redis, mensagens na DLQ e rollback de release.
- Todos os itens funcionais do roadmap estão implementados ou explicitamente justificados como fora de escopo; a automação de qualidade permanece registrada na fase pós-core.

