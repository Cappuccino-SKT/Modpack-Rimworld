# Status

_Só o estado atual e os próximos passos. Manter curto (até ~60 linhas). Detalhe do que já foi feito: `HISTORICO.md` (buscar, não ler inteiro). Visão geral do projeto: `RESUMO.md`._

## Onde estamos (2026-11-20)

- **Base pronta e testada:** 4 potências e 7 facções menores (com os Fungobi), ideologias do usuário, xenos próprios, Fama/Infâmia, cerimônia, cargos, permissões, bases, serviços. Hegemony é a camada de estratégia.
- **Concluídas:** Etapas 1, 2, 3 (3a–3e), 4 (4a, 4b sem Rimesis, 4c em parte), 5 (5a–5g), 8 (quase tudo), 11 (11a–11d), **10d** (menos a otimização do Hegemony), **10e** (2 blocos), **6a** (narrador principal), 10a (só a ferramenta `perf`).
- **Últimos blocos (2026-11-19):** frentes nas invasões do Hegemony e escolha de lado (10e); investigação de estrutura (`Structures.cs`: ordem "investigar estrutura", ruínas do Velho Mundo ensinam pesquisa); narrador (`Narrator.cs`); tensão entre potências, conselho de guerra, guerra declarada e frentes de fronteira (`WarDiplomacy.cs`).
- **Mundo fixo e fronteiras vivas (em andamento, tudo em `MUNDO.md`):**
  - Decidido: América do Norte + México (sem Caribe) num mundo de 25%; biomas fixos com os 31 de superfície; marcos, ruínas e sítios variam por partida; 3 cidades por potência (1 capital), menores com 1; regiões da 3.7 do `MUNDO.md` aprovadas; território = Hegemony + regras nossas.
  - **M1, M2, M2b, F1, M3a e M3b feitos: o mundo fixo é jogável.** 16 cidades em lugares fixos (`Defs/World/EQ_MundoFixo.xml`, editável), marcos, mutadores e estradas refeitos por partida (`FixedWorld.cs`), com semente sorteada por partida (`fixedSeed`).
  - Preset `EcosDaQueda` (escala e latitudes reais, 31/31 biomas, rios, nomes) **dentro do mod**: `Mods/EcosDaQueda/Worldbuilder/EcosDaQueda/`. Pintura editável em `mundo/pintura.tsv`. Os 6 cenários do Ecos abrem direto nele (`WB_StartInWorld`).
  - **F2 feito: a fronteira cresce.** No mundo fixo o território nasce com raio 1,5 e cresce 1 tile a cada 3–5 dias até o teto (raio natural do Hegemony × facção × capital 1,3); cidade no teto faz a facção fundar outra a 6–9 tiles (`Frontier.cs`, `Defs/World/EQ_Fronteira.xml`, valores propostos). Detalhe em `MUNDO.md` › F2.
  - **F3 feito: cada tile rende.** Por dia, o território rende para a cidade mais próxima da facção pelo bioma (perfis do Hegemony, e os nossos para as 12 zonas da Queda): comida, materiais, prosperidade (`TerritoryYield.cs`, `Defs/World/EQ_RendimentoTerritorio.xml`). Marcos do território o Hegemony já dava. Detalhe em `MUNDO.md` › F3.
  - **F4 feito: fronteira do jogador.** A colônia (o Hegemony não tem estado para ela; usa a riqueza do mapa) nasce com raio 2 e cresce como as NPCs; botão **"Plantar marco de fronteira"** na colônia (mapa-múndi; 150 de prata, encostado na borda, até 3 por colônia) puxa o território para um lado; colônia no teto recebe carta com lugar sugerido (fundar perto dá +1 tile); o território rende um **tributo em drop pod** a cada 5 dias (`PlayerFrontier.cs`, `Defs/World/EQ_FronteiraJogador.xml`, postfix em `TerritorySolver.Sources`). Detalhe em `MUNDO.md` › F4.
  - **Worldbuilder** (`ferny.worldbuilder`, 3522102833) está no ModsConfig do usuário e no `modpack/`. **The Earth [Odyssey]** (3556550577) fica fora (traz facções próprias); só para refazer o preset: `-Include allyjamy.terraearthodysseyv.wb`.
- **N1 feito: fundação da nação** (`NACAO.md` › Resultado do N1; `Nation.cs`, `Defs/World/EQ_Nacao.xml`). Com o Hegemony, em qualquer mundo: sem nação não há território, F4, cerimônia nem invasão de conquista contra o jogador. Botão **"Proclamar líder"** no trono (sala do trono, 3 adultos livres) → carta (esperar / comprar parte dos vizinhos / desistir) → revolta dos **Povos da Região** em 1–2 dias (pontos × 1,3) → vitória funda a nação (nome, F4, cerimônia); derrota (líder morto/capturado, 6 h na sala do trono) → 15 dias e +20%. Vassalo de save antigo conta como fundado.
- **N1b feito: ideologia clássica até a fundação** (`NACAO.md` › Resultado do N1b; `NationFaith.cs`). Cenários do Ecos pulam a tela de ideologia: o jogador começa com uma clássica só dele (o modo clássico global fica desligado, as NPC seguem com as delas). Na vitória da revolta ela vira fluida, abre a escolha de estrutura + 1 meme, o líder proclamado volta à função e a 1ª reforma fica liberada.
- **N2 feito: conselho** (`NACAO.md` › Resultado do N2; `Council.cs`, `Defs/World/EQ_Conselho.xml`). Botão "Conselho" no trono depois da fundação: Embaixador (Social), General (Tiro/Corpo a corpo), Conselheiro (Intelectual), bônus pela perícia (0 na 4, cheio na 15) em boa vontade, tensão da patrona, cerimônia/dízimos, exércitos, revolta, fronteira, tributo e marcos; pensamentos membro/preterido; troca com espera de 3 dias.
- **Testes:** depois do N2 rodou o lote `conselho`, `nacao`, `nacao-fe`, `fronteira-jogador`, `tensao`, `cerimonia-coro`, `comando`, `obrigacoes`, `promocao` (9, sem erros). Regressão completa de 62 roteiros (2026-11-20, com F4, N1 e N1b): 59 sem erros; `dados-antigos` (XML do editor de pesquisa trocado no meio do lote), `faccoes-menores` e `nao-catalogados` (intermitentes de terceiros) passaram ao rodar de novo. O executor começa com a nação fundada (`nation lock` liga a trava; `nation classic` simula o começo clássico).

## Próximo passo (escolher um; um bloco por sessão)

1. **N2b — governo pela ideologia, 1º corte** (recomendado; `NACAO.md` › 4 e `nacao/memes.md`): grupos 1 e 2 (República com 3 pastas, Primazia humana com o Ministro da Indústria etc.), sobre o conselho do N2. Ou **F5** (exército toma fronteira).
2. **F5 — exército toma fronteira**: vitória numa frente de fronteira (`WarDiplomacy.cs`, `Fronts.cs`) passa tiles do perdedor para o vencedor (ex.: marco/posto de ocupação como fonte de território, como os marcos do F4). Depois: **F6** (medição de jogo longo). Pendências do F4: cidades do jogador sem mapa (tomadas/vassalas, com estado no Hegemony) ainda ficam fora da regra; marco é instantâneo (sem caravana de agrimensores).
3. Sem depender do usuário: traduzir o texto da peça de cenário do Worldbuilder ("Start in world") no mod de tradução; lições de investigação para estruturas de outros mods; medição de jogo longo (10a) para a otimização do Hegemony.

## Precisa do usuário (✱)

- **Nação (`NACAO.md` › 5):** o N1 seguiu as propostas de ✱1–✱5 (ajustar em `EQ_Nacao.xml` se quiser); bônus do conselho feitos com a proposta, sem a 4ª cadeira (Intendente?) — ajustar em `EQ_Conselho.xml`, quais sugestões extras entram; **o que cada meme acrescenta ao governo** (3.7) e se a ideologia clássica vale em todo cenário (✱9; hoje só nos do Ecos, `classicStartAllScenarios` em `EQ_Nacao.xml`) e se a reforma liberada na fundação fica (`freeReformOnFounding`).
- **Mundo (`MUNDO.md` › 7):** valores do F4 propostos em `Defs/World/EQ_FronteiraJogador.xml` (raio 2, marco 150 de prata/raio 1,5→2,5/até 3, tributo a cada 5 dias × 0,6). Os do F2 estão com a proposta (cidade nova a 6 tiles, 1 tile a cada 3–5 dias, capital ×1,3, até 6 cidades por potência): ajustar em `Defs/World/EQ_Fronteira.xml` se quiser; onde o jogador começa (tile livre ou fixo por cenário).
- **3f Pesquisa por manual:** planejar a nova árvore (editor em `http://127.0.0.1:8765/pesquisa`, `python kits/kits.py`; "Gerar Defs" → `Defs/Pesquisa/EQ_Pesquisa.xml`). Árvore **Fazenda** ativa (`kits/pesquisa/Fazenda.json`, 61 pesquisas, mescla de Cultivo/Agricultura/Animais): Lavoura (as 230 plantas: Alimentícia, Souvenir, Recurso com Medicamento e Drogas), Agricultura (mobília, solo, manejo, ferramentas), Pecuária (abrigo, alimentação, abate, criação, produtos). Sementes: `EQSeedRecipes` (87 extrair, 108 propagar). Levantamentos: comandos `agro` e `animais`. Pesquisa original esvaziada pelo editor some da tela (`EQHiddenResearch`, jogo e YART; 11 hoje). Plano das árvores para todo o modpack: **`ARVORES.md`** (22 árvores, mapa em `kits/arvores-mapa.tsv`; decidir munição, pisos, cirurgias e o que fica livre antes de montar). **Início com pouca coisa liberada:** 0 pesquisas no início (`EQ_InicioSemPesquisa.xml`), kit livre de 36 coisas, o resto livre na árvore provisória **Fundamentos** (13 pesquisas); comando `inicio` confere.
- **Editor de kits/unidades:** nenhuma unidade pronta nem coleção ligada ainda.
- Seamless World (10f): descrever o bug. Mod de desempenho (10c): aprovar simplificações depois de medir. Colmeia Erguida: ideologia (`.rid`). Narrador 6b (variações, retrato, maldições do VFE Pirates). Etapa 7 (QoL). Rimesis. Character Creator (tirar quando avisar). Xeno do líder da Forja.

## Como testar

- `scripts/run-test.ps1 -Script tests/x.txt` (`-Build` compila; `-Batch a.txt,b.txt`; `-Show '<regex>'`; `-Include pkg1,pkg2` só no perfil de teste). Regressão: `scripts/regress.ps1`.
- O `newgame` aceita `cov=0.25 seed=X preset=Nome_Com_Sublinhado`. Comandos do mundo e do território no `TESTES.md`.

## Pendências técnicas

- **Hospitality + "Saídos do abrigo":** com o começo no abrigo selado (VQE Ancients) as visitas falham ("Trying to spawn visitors, but parms couldn't be resolved", dia 2–3). Investigar ponto de chegada no mapa do abrigo.
- `frente-hegemony` falhou na regressão completa 2 vezes (M3b, F2) e passou na do F3; intermitente.

- Intermitentes raros de terceiros (Alpha Memes, Alpha Genes, VIE, Faction Territories 2, VPE/CE); lista no `HISTORICO.md`.
- Worldbuilder: 1 erro cosmético dele no log (`Frame.CompleteConstruction transpiler`), sem efeito.
- Reino do Lodo com poucos defensores; lojas com roupas genéricas; preços sem balanço; 13 pesquisas do VFE Security caem nos ramos só pelo nível.

## A conferir pelo usuário (só visual)

- **N2:** botão "Conselho" no trono (só depois da fundação; antes, desabilitado com o motivo), janela com as 3 cadeiras, menu de candidatos ordenado pela perícia, aviso de espera, mensagem de cadeira vazia; pensamentos no humor.
- **N1b:** jogo novo num cenário do Ecos pula a tela de ideologia (aba Ideologia mostra a clássica); na vitória da revolta, a janela de memes (estrutura, depois 1 meme; "Voltar" fecha e ela reabre em alguns segundos), a carta "A fé da nação" e a reforma liberada na aba Ideologia.
- **N1:** botão "Proclamar líder" no trono (ícone, motivo quando desabilitado, menu dos candidatos); carta da proclamação com as 3 opções; carta da revolta; janela de nome na vitória; recusa na aba Reputação › Cerimônia antes da fundação.
- **F4:** botão "Plantar marco de fronteira" na colônia (mapa-múndi), mira no tile com o motivo da recusa, ícone do marco e o texto de inspeção; cartas "Tributo do território" e "Bordas longe demais".

- **F3:** descrições das zonas da Queda na janela de terreno/economia da cidade no Hegemony (em português).
- **F2:** mensagem "X fundou uma cidade nova" e as fronteiras crescendo no mapa-múndi (camada do Hegemony).
- **M3b:** jogo novo com um cenário do Ecos deve pular a tela de parâmetros do mundo e ir para a escolha do tile no mapa da América do Norte; a tela de mundos do Worldbuilder (outros cenários) mostra o preset "EcosDaQueda" com miniatura.

- Blocos de 2026-11-17 a 19 (lista detalhada no `HISTORICO.md`): cartas e telas da 3a–3e, 4b, 4c, 5a–5g, 11a–11d; exércitos no mapa-múndi, frentes de trincheira (3 opções "Entrar na frente…"), cartas de guerra, tensão e conselho de guerra; botão "investigar estrutura" (ícone azulado), barra e mensagem; descrição nova da Central de Atendimento.
- Se um def novo não aparecer: apagar o cache do Missile Girl (`TESTES.md`).
