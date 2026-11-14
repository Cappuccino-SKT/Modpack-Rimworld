# CLAUDE.md

Esta pasta é uma instalação do **RimWorld 1.7** (não-Steam, motor Waylith) onde estamos desenvolvendo o mod **Ecos da Queda** (`Mods/EcosDaQueda`, packageId `ecosdaqueda.core`).

## Antes de qualquer coisa

Leia **`EcosDaQueda-Dev/STATUS.md`** (onde paramos e próximos passos). Depois, conforme a tarefa:
- `EcosDaQueda-Dev/DESIGN.md` — visão, facções, sistemas planejados.
- `EcosDaQueda-Dev/TECNICO.md` — como o jogo/Hegemony/VPE funcionam, packageIds, convenções.
- `EcosDaQueda-Dev/TESTES.md` — como testar, ruído conhecido do log, limitações.

Ao terminar uma etapa, **atualize `STATUS.md`** (e DESIGN/TECNICO se algo mudou).

## Regras do projeto

- Conversar com o usuário em **português (PT-BR)**. Textos dos Defs também em PT-BR.
- Tom do mod: **satírico, inspirado em Fallout, usando a lore do RimWorld** (nada da história de Fallout). Todas as facções são moralmente cinzas, cada uma com uma parte horrível.
- Reaproveitar mecânicas dos mods instalados em vez de reinventar. Dependências opcionais sempre com `MayRequire="<packageId>"`.
- Prefixo `EQ_` em todos os defNames.
- Não editar mods de terceiros em `Mods/<id>`; corrigir por patch/def no nosso mod.
- Os mods **Hearts of RimWorld** e **Rimworld Exploration Mode** estão desativados de propósito. A camada de estratégia é o **Hegemony**.

## Testes automáticos (use sempre antes de pedir teste ao usuário)

O mod `Mods/EcosDaQuedaDebug` (packageId `ecosdaqueda.debug`) tem um executor de testes: o jogo abre **sem janela**, num perfil isolado (não toca nos saves/config do usuário), executa um roteiro de comandos, grava um relatório e fecha. Um teste em jogo novo leva ~40 s.

```
powershell -ExecutionPolicy Bypass -File EcosDaQueda-Dev/scripts/run-test.ps1 -Script EcosDaQueda-Dev/tests/fase2.txt -Build
```

- `-Build` compila os 3 projetos C# antes. `-Profile full -Save <nome>` usa todos os mods e um save do usuário. `-Window` mostra a janela.
- Roteiros em `EcosDaQueda-Dev/tests/*.txt`; relatórios em `EcosDaQueda-Dev/tests/out/`. Comandos: ver `EcosDaQueda-Dev/TESTES.md`.
- O RimWorld precisa estar fechado. Ao criar um sistema novo, adicione comandos ao `EQDebug.cs`/`TestRunner.cs` e um roteiro de teste.
- O mesmo mod põe ações no menu de debug do jogo (modo desenvolvedor), categoria "Ecos da Queda".

## Testes (resumo)

- Validar XML: `powershell -ExecutionPolicy Bypass -File EcosDaQueda-Dev/scripts/validate-xml.ps1`
- Feche o jogo antes de editar `ModsConfig.xml`.
- Abrir o jogo, esperar o menu, depois: `powershell -ExecutionPolicy Bypass -File EcosDaQueda-Dev/scripts/check-log.ps1`
- `-quicktest` e controle de tela **não funcionam** neste ambiente; só o que for **visual** (janelas, abas, ícones) precisa do usuário. Lógica se testa com o executor acima.
- Log: `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Logs\Player.log`
