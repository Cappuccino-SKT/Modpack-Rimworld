# CLAUDE.md

Esta pasta é uma instalação do **RimWorld 1.7** (não-Steam, motor Waylith) onde estamos desenvolvendo o mod **Ecos da Queda** (`Mods/EcosDaQueda`, packageId `ecosdaqueda.core`).

## Antes de qualquer coisa

Leia **`EcosDaQueda-Dev/STATUS.md`** (curto: onde paramos e próximos passos). Depois, **só o que a tarefa pedir**:
- `EcosDaQueda-Dev/PLANO.md` — plano de desenvolvimento em etapas (o que vem a seguir e em que ordem).
- `EcosDaQueda-Dev/DESIGN.md` — visão, facções, sistemas planejados.
- `EcosDaQueda-Dev/TECNICO.md` — ambiente, estrutura, packageIds e **índice** das notas por assunto em `EcosDaQueda-Dev/tecnico/` (ler só o arquivo do assunto).
- `EcosDaQueda-Dev/TESTES.md` — como testar, ruído conhecido do log, limitações.
- `EcosDaQueda-Dev/HISTORICO.md` — registro detalhado do que já foi feito. **Não ler inteiro**; buscar o termo quando precisar.

## Economia de contexto

O custo de uma sessão é (tamanho do contexto) × (número de chamadas). Por isso:
- **Um bloco do plano por sessão.** Ao fechar o bloco, deixar `STATUS.md` pronto para uma sessão nova começar dali.
- **Documentação só no fim do bloco:** reescrever `STATUS.md` (manter até ~60 linhas), acrescentar uma linha na tabela do `HISTORICO.md`, e mexer em DESIGN/`tecnico/`/TESTES/PLANO só se algo mudou de fato.
- **Saídas grandes vão para arquivo, não para a conversa:** relatórios de teste, código decompilado, listagens (`weapontags`, `tradeinfo`, `modusage`). Gravar e consultar com busca ou trecho.
- **Testes:** `run-test.ps1` imprime só o resumo (use `-Show '<regex>'` para linhas específicas, `-Full` só se precisar); a regressão é `regress.ps1`, uma chamada.
- **Agrupar:** várias leituras/edições independentes na mesma chamada; edições em lote por um script só.
- Ler arquivos grandes por trecho (busca + intervalo de linhas), não inteiros.

## Regras do projeto

- Conversar com o usuário em **português (PT-BR)**. Textos dos Defs também em PT-BR.
- Tom do mod: **satírico, inspirado em Fallout, usando a lore do RimWorld** (nada da história de Fallout). Todas as facções são moralmente cinzas, cada uma com uma parte horrível.
- Reaproveitar mecânicas dos mods instalados em vez de reinventar. Dependências opcionais sempre com `MayRequire="<packageId>"`.
- Prefixo `EQ_` em todos os defNames.
- Não editar mods de terceiros em `Mods/<id>`; corrigir por patch/def no nosso mod.
- Os mods **Hearts of RimWorld** e **Rimworld Exploration Mode** estão desativados de propósito. A camada de estratégia é o **Hegemony**.

## Versionamento (git)

Repositório `https://github.com/Cappuccino-SKT/Modpack-Rimworld` (público), na raiz do jogo.
- `main`: só o modpack (`modpack/`: lista de mods, ordem de carga, `install.ps1`), `EcosDaQueda-Dev/`, `README.md`, este arquivo. Jogo e mods de terceiros **não** são versionados (`.gitignore` em lista branca).
- Cada mod nosso é uma branch órfã `mod/<nome>` cuja raiz é a pasta do mod, montada como **worktree** em `Mods/<nome>` (`mod/ecos-da-queda`, `mod/ecos-da-queda-debug`). Commit dentro da pasta do mod → branch do mod; commit na raiz → `main`. Nunca trocar de branch na raiz.
- Mod novo: `git worktree add --orphan -b mod/<nome> Mods/<Nome>`, e adicionar em `README.md`, `modpack/mods.md` e `modpack/install.ps1`.
- Mudou a lista de mods? Atualize `modpack/` (ModsConfig.xml, workshop-ids.txt, mods.md).
- Só faça commit/push quando o usuário pedir.

## Testes automáticos (use sempre antes de pedir teste ao usuário)

O mod `Mods/EcosDaQuedaDebug` (packageId `ecosdaqueda.debug`) tem um executor de testes: o jogo abre **sem janela**, num perfil isolado (não toca nos saves/config do usuário), executa um roteiro de comandos, grava um relatório e fecha. Um teste em jogo novo leva ~40 s.

```
powershell -ExecutionPolicy Bypass -File EcosDaQueda-Dev/scripts/run-test.ps1 -Script EcosDaQueda-Dev/tests/fase2.txt -Build
```

- Use sempre `-Profile full` em jogo novo (o perfil lean está quebrado). Regressão inteira: `EcosDaQueda-Dev/scripts/regress.ps1`.
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
