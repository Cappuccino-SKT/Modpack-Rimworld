# Status

_Só o estado atual e os próximos passos. Manter curto (até ~60 linhas). Detalhe do que já foi feito: `HISTORICO.md` (buscar, não ler inteiro). Visão geral do projeto: `RESUMO.md`._

## Onde estamos (2026-11-19, noite; tudo commitado e no GitHub)

- **Base pronta e testada:** 4 potências e 7 facções menores (com os Fungobi), ideologias do usuário, xenos próprios, Fama/Infâmia, cerimônia, cargos, permissões, bases, serviços. Hegemony é a camada de estratégia.
- **Concluídas:** Etapas 1, 2, 3 (3a–3e), 4 (4a, 4b sem Rimesis, 4c em parte), 5 (5a–5g), 8 (quase tudo), 11 (11a–11d), **10d** (menos a otimização do Hegemony), **10e** (2 blocos), **6a** (narrador principal), 10a (só a ferramenta `perf`).
- **Últimos blocos (2026-11-19):** frentes nas invasões do Hegemony e escolha de lado (10e); investigação de estrutura (`Structures.cs`: ordem "investigar estrutura", ruínas do Velho Mundo ensinam pesquisa); narrador (`Narrator.cs`); tensão entre potências, conselho de guerra, guerra declarada e frentes de fronteira (`WarDiplomacy.cs`).
- **Mundo fixo e fronteiras vivas (em andamento, tudo em `MUNDO.md`):**
  - Decidido: América do Norte + México (sem Caribe) num mundo de 25%; biomas fixos com os 31 de superfície; marcos, ruínas e sítios variam por partida; 3 cidades por potência (1 capital), menores com 1; regiões da seção 6 do `MUNDO.md` aprovadas; território = Hegemony + regras nossas.
  - **M1, M2, M2b, F1, M3a e M3b feitos: o mundo fixo é jogável.** 16 cidades em lugares fixos (`Defs/World/EQ_MundoFixo.xml`, editável), marcos, mutadores e estradas refeitos por partida (`FixedWorld.cs`), com semente sorteada por partida (`fixedSeed`).
  - Preset `EcosDaQueda` (escala e latitudes reais, 31/31 biomas, rios, nomes) **dentro do mod**: `Mods/EcosDaQueda/Worldbuilder/EcosDaQueda/`. Pintura editável em `mundo/pintura.tsv`. Os 6 cenários do Ecos abrem direto nele (`WB_StartInWorld`).
  - **Worldbuilder** (`ferny.worldbuilder`, 3522102833) está no ModsConfig do usuário e no `modpack/`. **The Earth [Odyssey]** (3556550577) fica fora (traz facções próprias); só para refazer o preset: `-Include allyjamy.terraearthodysseyv.wb`.
- **Testes:** regressão de 55 roteiros sem erros (2026-11-19, já com o Worldbuilder carregado; 3 intermitentes de terceiros passaram na repetição). `mundo-m3b.txt` entrou na regressão.

## Próximo passo (escolher um; um bloco por sessão)

1. **F2 — fronteira que cresce** (recomendado; no mundo fixo Las Vegas só pega 4 tiles e o Silêncio ~20 por cidade: entender junto): postfix em `TerritorySolver.SettlementDecay` (raio permitido cresce até um teto) + cidade nova pela distância + **capital com efeito** (raio maior; hoje é só a primeira da lista). Primeiro confirmar se o Hegemony recalcula o território quando só o nosso multiplicador muda (ver `MUNDO.md` › F1).
2. Sem depender do usuário: traduzir o texto da peça de cenário do Worldbuilder ("Start in world") no mod de tradução; lições de investigação para estruturas de outros mods; medição de jogo longo (10a) para a otimização do Hegemony.

## Precisa do usuário (✱)

- **Mundo (`MUNDO.md` › 7):** ritmo do crescimento e distância da cidade nova (proposta: 6 tiles; 1 tile a cada 3–5 dias); onde o jogador começa (tile livre ou fixo por cenário).
- **3f Pesquisa por manual:** planejar a nova árvore (editor em `http://127.0.0.1:8765/pesquisa`, `python kits/kits.py`; "Gerar Defs" → `Defs/Pesquisa/EQ_Pesquisa.xml`; ainda nenhuma árvore do usuário; pendente esconder pesquisa original vazia).
- **Editor de kits/unidades:** nenhuma unidade pronta nem coleção ligada ainda.
- Seamless World (10f): descrever o bug. Mod de desempenho (10c): aprovar simplificações depois de medir. Colmeia Erguida: ideologia (`.rid`). Narrador 6b (variações, retrato, maldições do VFE Pirates). Etapa 7 (QoL). Rimesis. Character Creator (tirar quando avisar). Xeno do líder da Forja.

## Como testar

- `scripts/run-test.ps1 -Script tests/x.txt` (`-Build` compila; `-Batch a.txt,b.txt`; `-Show '<regex>'`; `-Include pkg1,pkg2` só no perfil de teste). Regressão: `scripts/regress.ps1`.
- O `newgame` aceita `cov=0.25 seed=X preset=Nome_Com_Sublinhado`. Comandos do mundo e do território no `TESTES.md`.

## Pendências técnicas

- Intermitentes raros de terceiros (Alpha Memes, Alpha Genes, VIE, Faction Territories 2, VPE/CE); lista no `HISTORICO.md`.
- Worldbuilder: 1 erro cosmético dele no log (`Frame.CompleteConstruction transpiler`), sem efeito.
- Reino do Lodo com poucos defensores; lojas com roupas genéricas; preços sem balanço; 13 pesquisas do VFE Security caem nos ramos só pelo nível.

## A conferir pelo usuário (só visual)

- **M3b:** jogo novo com um cenário do Ecos deve pular a tela de parâmetros do mundo e ir para a escolha do tile no mapa da América do Norte; a tela de mundos do Worldbuilder (outros cenários) mostra o preset "EcosDaQueda" com miniatura.

- Blocos de 2026-11-17 a 19 (lista detalhada no `HISTORICO.md`): cartas e telas da 3a–3e, 4b, 4c, 5a–5g, 11a–11d; exércitos no mapa-múndi, frentes de trincheira (3 opções "Entrar na frente…"), cartas de guerra, tensão e conselho de guerra; botão "investigar estrutura" (ícone azulado), barra e mensagem; descrição nova da Central de Atendimento.
- Se um def novo não aparecer: apagar o cache do Missile Girl (`TESTES.md`).
