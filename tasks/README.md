# Kanban do projeto

Este diretório contém o backlog executável para concluir o Financial Transfer Processing. Cada cartão deve ser implementado na ordem de suas dependências, e não apenas pela numeração.

## Quadro

### Concluído

| Tarefa | Entrega |
| --- | --- |
| [07](07-create-pending-transfer.md) | Criar transferência pendente com idempotência |
| [10](10-transactional-outbox-storage.md) | Modelar e persistir o transactional outbox |

### Pronto para iniciar

| Tarefa | Entrega | Depende de |
| --- | --- | --- |
| [08](08-get-transfer-by-id.md) | Consultar transferência por ID | 07 |

### Backlog — processamento assíncrono

| Tarefa | Entrega | Depende de |
| --- | --- | --- |
| [11](11-atomic-transfer-and-outbox.md) | Gravar transferência e evento atomicamente | 10 |
| [12](12-rabbitmq-topology.md) | Configurar RabbitMQ, contratos e topologia | 10 |
| [13](13-outbox-publisher.md) | Publicar o outbox com publisher confirms | 11, 12 |
| [14](14-worker-and-consumer.md) | Criar Worker e consumidor de transferências | 12 |
| [15](15-atomic-transfer-settlement.md) | Liquidar transferência de forma atômica e idempotente | 14 |
| [16](16-concurrency-control.md) | Garantir saldos consistentes sob concorrência | 15 |
| [17](17-retry-and-dead-letter.md) | Implementar retry progressivo e DLQ | 14, 15 |
| [18](18-reliable-message-lifecycle.md) | Fechar ack, recovery e graceful shutdown | 13, 17 |

### Backlog — Redis e reconciliação

| Tarefa | Entrega | Depende de |
| --- | --- | --- |
| [19](19-transfer-status-cache.md) | Cachear consultas de status com Redis | 08, 15 |
| [20](20-distributed-rate-limiting.md) | Aplicar rate limiting distribuído | 19 |
| [21](21-reconciliation-use-case.md) | Implementar reconciliação e checkpoint | 15 |
| [22](22-reconciliation-scheduler.md) | Agendar reconciliação e tratar divergências | 21 |

### Backlog — observabilidade e encerramento do core

| Tarefa | Entrega | Depende de |
| --- | --- | --- |
| [23](23-correlation-and-structured-logging.md) | Propagar correlação e logs estruturados | 13, 14 |
| [24](24-opentelemetry-metrics-and-health.md) | Adicionar traces, métricas e health checks | 18, 20, 22, 23 |
| [28](28-production-readiness.md) | Fechar containers, CI e documentação operacional | 24 |

### Adiado — qualidade pós-core

Estas tarefas serão retomadas após a conclusão funcional do projeto e não bloqueiam o caminho crítico atual.

| Tarefa | Entrega | Depende de |
| --- | --- | --- |
| [09](09-core-automated-tests.md) | Criar fundação de testes e cobrir o core | 28 |
| [25](25-end-to-end-integration-tests.md) | Cobrir o fluxo completo com testes de integração | 09 |
| [26](26-concurrency-and-invariant-tests.md) | Validar invariantes financeiros sob concorrência | 16, 25 |
| [27](27-k6-load-and-performance-tests.md) | Medir carga e comparar estratégias | 20, 26 |

## Política do quadro

- Manter no máximo uma tarefa de implementação em andamento por pessoa.
- Mover um cartão para pronto somente quando todas as dependências estiverem concluídas.
- Decisões técnicas relevantes devem ser registradas no próprio cartão ou em ADR.
- Ao concluir um cartão, executar sua verificação funcional específica.
- Não considerar uma tarefa concluída com avisos novos ou migrações não validadas.
- A automação de qualidade está deliberadamente adiada e não bloqueia a conclusão dos cartões do core.

## Definição de pronto

- Critérios de aceite do cartão atendidos.
- `CancellationToken` propagado em toda operação assíncrona aplicável.
- Configurações externas validadas no startup e documentadas.
- Logs não expõem dados sensíveis nem payload financeiro desnecessário.
- Solução compila sem erros e sem novos avisos.
- Docker Compose e documentação atualizados quando a entrega altera a operação local.
- Arquivos novos ou alterados permanecem com finais de linha CRLF.

## Caminho crítico

```text
10 -> 11 -> 13 -> 18 -> 24 -> 28
       \-> 12 -> 14 -> 15 -> 16 --/
                         \-> 21 -> 22 -> 24

Após o core: 09 -> 25 -> 26 -> 27
```
