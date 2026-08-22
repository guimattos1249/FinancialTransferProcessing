# Tarefa 22 — Agendar reconciliação e tratar divergências

**Status:** Backlog  
**Fase:** Reconciliação  
**Depende de:** Tarefa 21

## Problema

O caso de uso de reconciliação precisa ser orquestrado periodicamente e tornar divergências acionáveis.

## Objetivo

Adicionar serviço hospedado no Worker para executar reconciliações sem sobreposição e registrar alertas.

## Implementação esperada

- Configurar intervalo, atraso de segurança, tamanho da janela e timeout.
- Usar lock distribuído/durável para impedir execução simultânea em múltiplos Workers.
- Respeitar cancelamento e concluir/liberar o lock com segurança.
- Emitir log, métrica e registro persistente para divergências, com detalhes suficientes para investigação.
- Prever extensão para publicação futura de evento sem misturar orquestração e regra de aplicação.

## Critérios de aceite

- Apenas uma instância processa cada janela.
- Execuções demoradas não se sobrepõem.
- Divergência fica identificada por execução, período e tipo.
- Reinício e cancelamento não criam lacunas silenciosas.

