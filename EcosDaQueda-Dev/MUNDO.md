# Mundo fixo e fronteiras vivas (desenho, 2026-11-19)

_Ideia do usuário de 2026-11-19, elaborada. Junta o **10b** (mundo fixo) com um sistema novo de **fronteiras vivas**. Substitui a pergunta antiga do 10b ("quanto varia entre partidas"): **não varia**. Decisões em aberto no fim (✱)._

## 1. A ideia (palavras do usuário, organizadas)

- **Mapa-múndi fixo:** a **América do Norte** tirada de um mapa da Terra, encaixada num mundo de **cobertura 25%**. Fixos: o mapa, os biomas e a posição inicial dos assentamentos das facções.
- **Biomas fixos:** a posição de cada bioma é sempre a mesma, e o mundo tem que ter **todos os biomas disponíveis no modpack** (vanilla, DLCs, Alpha Biomes, VE e o resto). A América do Norte dá a base (clima por latitude); os biomas de mod entram por pintura nas regiões que combinam com eles (ex.: biomas de cinzas e radiação nas zonas da Queda).
- **Varia por partida:** marcos do mundo, ruínas e construções exploráveis (sítios do VQE Ancients, abrigos, ruínas, os 10 marcos nossos e as zonas que forem sítios). O resto é fixo.
- **Início:** cada **potência** começa com **3 assentamentos, um deles a capital** (decidido); cada **facção menor** começa com **1**.
- **Fronteiras vivas** (para todas as facções, inclusive o jogador):
  - a fronteira (área ocupada) nasce em volta do assentamento e **cresce aos poucos**;
  - quando a fronteira fica **longe demais** de um assentamento, nasce um **assentamento novo** (as bordas ficaram longe demais para serem protegidas);
  - **cada tile da fronteira dá um bônus** ao assentamento dono (é o incentivo para crescer);
  - o **jogador** tem mais controle: escolhe para onde a fronteira cresce;
  - os **exércitos** (10d) expandem a fronteira e tomam a fronteira do inimigo. É a camada de estratégia.

## 2. O que já existe para reaproveitar

| Peça | Onde | Serve para |
|---|---|---|
| Mapa da Terra pronto | **[Worldbuilder] The Earth [Odyssey]** (Workshop 3556550577; 1.6/1.7; biomas e marcos da Odyssey; versões com e sem facções). Versão sem Odyssey: 3534543546 | Fonte dos biomas, relevo, rios e costas da América do Norte |
| Editor de mundo | **Worldbuilder** (Workshop 3522102833; 1.6/1.7; pintar tiles, salvar e distribuir mundos prontos; "não adicionar no meio do jogo") | Montar, salvar e carregar o nosso mundo fixo |
| Território | **Hegemony**: território projetado por cada assentamento (enfraquece com a distância, cresce com riqueza e prédios; `territoryDecayReduction`), tiles disputados, contagem de tiles, leitura por API (`GetTerritoryClaims`, `TerritoryRevision`) | Base da fronteira; já desenhado no mapa-múndi |
| Fundação de cidades | **Hegemony**: expedições de fronteira (IA e jogador; construtores, materiais, rota) | O "assentamento novo" quando a fronteira se afasta |
| Bônus por terreno | **Hegemony**: `BiomeEffectDef` (por bioma) e `SiteEffectDef` (marco, mutador, relevo) dão comida, materiais, componentes, defesa… ao assentamento | O "bônus por tile" (já tem unidade e economia) |
| Postos avançados | **Hegemony** aceita território de postos (até 3 tiles por posto) e **Vanilla Outposts** está no modpack | Controle do jogador sobre a direção |
| Exércitos e frentes | Nosso: `Forces.cs`, `Fronts.cs`, `WarDiplomacy.cs` (10d/10e) | Tomar fronteira à força |

## 3. Arquitetura proposta (recomendação)

**O Hegemony continua sendo o motor do território; nós controlamos o ritmo e as regras por cima.** Ter dois sistemas de território no mesmo mapa (o dele e um nosso) daria conflito visual, de economia e de desempenho. O plano:

1. **Fronteira que cresce aos poucos:** em vez de o território aparecer inteiro em volta da cidade, ele começa pequeno e cresce com o tempo, a riqueza e o nível (ajustando o alcance do Hegemony por Harmony ou pelos prédios com `territoryDecayReduction`). _Precisa de um protótipo: confirmar no código do Hegemony como o alcance é calculado e se dá para limitá-lo por cidade (bloco F1)._
2. **Cidade nova pela distância:** um componente nosso olha, por facção, os tiles da fronteira mais distantes de qualquer cidade; passando de X tiles (✱), agenda a fundação de uma cidade ali (para as NPCs, pela expedição do Hegemony ou criando o assentamento direto, que ele adota; para o jogador, vira uma oferta/missão).
3. **Bônus por tile:** `BiomeEffectDef`/`SiteEffectDef` nossos para os biomas e as zonas da Queda (4b), mais um rendimento por tile controlado entregue ao assentamento mais próximo do dono (comida, materiais, prata, pesquisa), com teto para não explodir.
4. **Controle do jogador:** "marcos de fronteira" (postos avançados) que o jogador planta num tile vizinho da sua fronteira para puxar o crescimento naquela direção; o resto cresce sozinho.
5. **Exército e fronteira:** vitória numa frente de fronteira (10d) passa tiles do perdedor para o vencedor (por postos ocupados, ou pela regra nossa de alcance); conquista de cidade já existe.

**Alternativa (B):** território próprio tile a tile, com o do Hegemony desligado. Dá controle total, mas é muito mais trabalho, briga com a economia do Hegemony (que conta tiles) e pesa no desempenho. Só se o protótipo F1 mostrar que o Hegemony não deixa controlar o crescimento.

### Resultado do F1 (2026-11-19, código descompilado + medição no executor, roteiro `tests/territorio.txt`, comando `territory`)

- **Fórmula** (`FactionTerritories2.TerritorySolver`): força de uma cidade num tile = √prosperidade × 2^(−distância × 1,5 × m ÷ 2); o tile é dela enquanto a força ≥ 10 (`claimThreshold`), até 40 tiles (`claimRange`). Raio ≈ 2·log₂(√P ÷ 10) ÷ (1,5·m). `m` vem de `TerritorySolver.SettlementDecay(SettlementState)` (prédios com `territoryDecayReduction`); configurações globais: `distancePower` 1,5, `claimThreshold` 10, `claimRange` 40.
- **Medição (mundo aleatório 30%, jogo novo):** dia 0 sem território (calcula nos primeiros dias); dia 3 ≈ 6.200 tiles; dia 8 ≈ 6.330. Cidade típica de potência: prosperidade 25–45 mil → raio ≈ 5,5 tiles, 55–90 tiles por cidade; menores (Colmeia, Não Catalogados): prosperidade 2–8 mil → raio ≈ 3–4. O jogador também tem território (59 tiles). Em 5 dias o território cresceu só 1–2% (o raio segue o log da prosperidade).
- **Conclusão:** o território **nasce quase inteiro** e depois quase não cresce. O "nasce pequeno e cresce aos poucos" é nosso: postfix em `SettlementDecay` com `m = max(1, raio_natural ÷ raio_permitido)`, onde o raio permitido começa em ~1,5 tile e cresce com o tempo até um teto; quando a cidade bate no teto com pressão (raio natural maior), entra a regra da cidade nova. **Postos de fronteira nativos** (`FrontierOutposts`, regra `outposts.enabled`/`outposts.claimRange`) dão ao jogador o controle de direção.
- **Risco para o F2:** o Hegemony recalcula o território por revisão (hash de prosperidade, prédios, bônus). Mudar só o nosso multiplicador talvez não dispare o recálculo; o F2 começa confirmando isso.
- No mundo aleatório as facções têm 7–17 cidades cada; no mundo fixo serão 3 por potência e 1 por menor, então o espaço livre para crescer é muito maior.

## 4. Como encaixar a América do Norte em 25%

O mundo do RimWorld é uma esfera de tiles hexagonais; "cobertura 25%" gera um quarto da esfera. O plano:
1. Instalar Worldbuilder + The Earth [Odyssey] e confirmar que carregam no 1.7/Waylith com o modpack (bloco M1).
2. Ler o formato de mundo salvo do Worldbuilder (bioma, relevo, elevação, temperatura, rios, estradas, marcos por tile).
3. Script de conversão (`scripts/mundo-america.py`): recorta a América do Norte (latitude/longitude) do mundo da Terra e reprojeta no mundo 25% (escala para caber; cada tile novo pega o bioma/relevo do tile de origem mais próximo; rios e costas redesenhados).
4. Retoque à mão no editor (Worldbuilder), salvar como **mundo do Ecos da Queda** dentro do nosso mod; os cenários próprios apontam para ele.
5. Posições fixas: capitais e cidades iniciais das potências, cidade de cada menor, os 10 marcos nossos, zonas da Queda. Os nomes de líderes e detalhes pequenos podem variar.

**Achados do M1 (2026-11-19, só leitura dos arquivos; ainda não testado no jogo):**
- Baixados pela SteamCMD (pasta `steamcmd/steamapps/workshop/content/294100/`):
  - Worldbuilder (`ferny.worldbuilder`, 3522102833, 64 MB). Tem **só a pasta `1.6`**: a DLL é do 1.6 e precisa ser testada no Waylith.
  - The Earth [Odyssey] (`allyjamy.terraearthodysseyv.wb`, 3556550577, 47 MB), que depende do Worldbuilder.

  Ainda **não** foram copiados para `Mods/` nem ativados.
- Um mundo do Worldbuilder é uma pasta `Worldbuilder/<nome>/` com dois arquivos:
  - `Preset.xml`: nome, tipo de planeta, listas de biomas e marcos, opções `saveFactions`/`saveTerrain`/`saveBases`, ideologias `.rid` e **`savedSettlementsData`** (tile, facção e nome de cada assentamento).
  - `TerrainData.xml`: dados por tile, em base64+deflate: `tileBiome`, `tileElevation`, `tileHilliness`, `tileTemperature`, `tileRainfall`, `tileSwampiness`, `tileFeature`, rios, estradas, `tileMutator*` (marcos) e `tilePollution`.

  Os nomes são os mesmos do save do mundo vanilla (`WorldGrid.ExposeData`), então o conversor pode ler os dados com a mesma lógica do jogo.
- O Earth usa IDs de tile até ~352 mil (mundo grande). Os IDs dependem da cobertura, então o conversor casa os tiles pela **posição na esfera** (latitude/longitude), não pelo ID.
- Vêm duas versões: `Earth` (com facções do autor) e `Earth [No Factions] oddy`. Vamos partir da **sem facções** e pôr as nossas no preset.

**Teste de carga do M1 (2026-11-19, executor, roteiro `tests/mundo-m1.txt` com `-Include ferny.worldbuilder,allyjamy.terraearthodysseyv.wb`):** os dois mods **carregam no 1.7/Waylith** e o jogo novo roda sem erro do nosso lado. Único erro do Worldbuilder: `Frame.CompleteConstruction transpiler did not find GenSpawn.Spawn call` (5×; personalização de construções, que não usamos). Copiados para `Mods/3522102833` e `Mods/3556550577`; **não estão no ModsConfig do usuário** (só no perfil de teste, via `-Include`).

**Biomas do modpack (comando `biomes`, tabela em `tests/out/biomes.tsv`):** 40 BiomeDefs. **31 de superfície**, que o mundo fixo tem que ter:
- vanilla (14): Matagal Árido, Floresta Boreal, Pântano Frio, Deserto, Deserto Extremo, Geleira, Lago, Oceano, Gelo Marinho, Floresta Temperada, Pântano Temperado, Floresta Tropical, Pântano Tropical, Tundra;
- Odyssey (5): Planície Glacial, Floresta Brilhante, Pastagem, Campo de Lava, Terra Cicatrizada;
- Alpha Biomes (12): selva infestada de feraliscos, cemitério de galatross, superorganismo gelatinoso, prados idílicos, intrusão mecanoide, manguezal miasmático, selva micótica, floresta ocular, lagos de propano, conflagração piroclástica, penhascos abandonados, poços de piche.

Ficam de fora (não são de superfície): Underground, Labyrinth, MetalHell, Undercave (mapas de bolso/subterrâneo), Orbit e Space (camada orbital da Odyssey), os 2 planos de bolso do Alpha Genes e o complexo ancestral do VQE Ancients (sítio). Num mundo aleatório de 30%, 4 dos 31 não apareceram, e os 4 são tropicais (Floresta Tropical, Pântano Tropical, selva de feraliscos, manguezal): **para ter os tropicais, o recorte precisa ir até o México, a América Central e o Caribe** (ou pintá-los na Flórida/Golfo). Os biomas de mod que não existem na Terra real entram por pintura: os estranhos (ocular, gelatinoso, micótica, intrusão mecanoide, cemitério de galatross, piche, propano, piroclástica) viram as **zonas da Queda** (4b) e as regiões das facções que combinam com eles.

Riscos: o Worldbuilder é recente e grande (65 MB); pode brigar com mods de mundo do modpack (Geological Landforms, World Map Enhanced, Hegemony na geração) e com o motor Waylith. O 25% reduz tiles e pesa menos (bom para o desempenho e para o Seamless World, 10f).

### Resultado do M2 (2026-11-19): conversor pronto, preset v1

- **Geometria:** com cobertura 25% o jogo centra a calota em **45°N, 0°** (raio 45°; `WorldGrid.CalculateViewCenterAndAngle`) e o tile tem o **mesmo tamanho** em qualquer cobertura (subdivisões 10). A América do Norte cabe **em escala real (1:1)**.
- **O mapa da Terra do mod não usa coordenadas reais:** a América do Norte dele está ~11° ao sul e ~12–18° a leste (`mundo/terra-origem.png`). O conversor põe o ponto (35, −102) do mapa dele no centro da calota; como o centro é 45°N, **as latitudes do mundo novo ficam as reais** (estações e duração do dia corretas).
- **Encaixe otimizado na grade real de 25%** (que é serrilhada, feita de triângulos inteiros): escala 1, centro (35, −102) → 99,8% da terra (México com peso 3) dentro. Fica de fora só a pontinha do Yucatán (na borda) e gelo polar distorcido acima de 80° no mapa do mod.
- **Resultado** (`mundo/preview-v1.png`, preset em `mundo/preset-v1/`): 79.889 tiles; 14 biomas (Oceano 39.294, Floresta Boreal 12.669, Tundra 6.339, Floresta Temperada 6.277, Deserto 4.977, Matagal Árido 3.966, Geleira 2.572, Pastagem 1.987, Floresta Tropical 1.245, Pântano Temperado 251, Lago 248, Terra Cicatrizada 48, Campo de Lava 13, Gelo Marinho 3). Jogo novo com o preset sai **idêntico** (roteiro `mundo-m2c.txt`).
- **Faltam 17 biomas, que entram por pintura (M2b):** Pântano Frio, Deserto Extremo, Pântano Tropical, Planície Glacial, Floresta Brilhante e os 12 do Alpha Biomes.
- **O preset v1 sai sem:** rios, estradas, nomes de regiões, marcos e mutadores (o terreno mudou; os marcos variam por partida). Rios e estradas precisam ser refeitos (M2b). Sem cidades no preset, o jogo sorteia assentamentos antes de aplicar o terreno (podem cair na água): resolvido no M3 com as posições fixas.
- **Como refazer:** `run-test.ps1 -Include ferny.worldbuilder,allyjamy.terraearthodysseyv.wb` com `tests/mundo-m2a.txt` (abre a Terra 100%, despeja a região em `tests/out/terra-america.tsv`), depois `mundo-m2b.txt` (25%, `worldconvert ... 35 -102 1 EcosDaQueda`; o preset vai para `test-profile/full/Worldbuilder/EcosDaQueda`) e `mundo-m2c.txt` (confere). Prévia da origem: `python scripts/mundo-preview.py`.

### Resultado do M2b (2026-11-19): preset v2 com os 31 biomas e rios

- **Pintura** (`mundo/pintura.tsv`, comando `worldpaint`): 21 regras (bioma, lat, lon, raio, sobre: terra/água/frio/quente/costa), borda com ruído Perlin. Os biomas estranhos do Alpha Biomes viraram **zonas da Queda** em lugares reais ligados às facções: intrusão mecanoide no cinturão da ferrugem (Forja), poços de piche em La Brea (Hélice), lagos de propano nas areias de Alberta, conflagração piroclástica em Yellowstone (Silêncio), penhascos no Grand Canyon, cemitério de galatross nas Badlands, floresta ocular nos Apalaches (Não Catalogados), superorganismo gelatinoso na Luisiana (Reino do Lodo), manguezal miasmático nos Everglades, selva de feraliscos em Chiapas, selva micótica em Veracruz (Fungobi), prados idílicos na Virgínia (Coro). Os de clima: Pântano Frio na Baía de Hudson, Deserto Extremo no Vale da Morte e em Sonora, Pântano Tropical em Tabasco, Planície Glacial no Ártico, Floresta Brilhante na costa do Pacífico, Gelo Marinho no Oceano Ártico.
- **Rios e nomes** refeitos pelos passos do jogo (`worldrivers`: `WorldGenStep_Rivers` e `WorldGenStep_Features`): ~1.330 tiles com rio seguindo o relevo real (Mississippi, Missouri, Mackenzie…), ~49 nomes de regiões.
- **Resultado** (`mundo/preview-v2.png`, preset em `mundo/preset-v2/`): **31 de 31 biomas de superfície**; jogo novo com o preset sai igual, com rios (roteiro `mundo-m2c.txt`).
- **Falta:** estradas (dependem das cidades fixas, M3); a pontinha do Yucatán fica na borda da calota.

### Resultado do M3a (2026-11-19): cidades fixas

- **Como funciona:** `FixedWorld.cs` + `Defs/World/EQ_MundoFixo.xml` (`EcosDaQueda.FixedWorldDef`: `presetName`, `minSpacing`, `searchRadius`, lista de cidades com facção, nome, lat/lon do mundo novo). Postfix em `World.FinalizeInit` (prioridade última, depois do Worldbuilder aplicar o terreno), só quando o preset carregado tem o nome do Def. Sem Worldbuilder ou com outro preset, nada muda.
- **Passos:** (1) as facções da lista perdem as cidades sorteadas e ganham as fixas, no tile livre mais perto do ponto (terra, `canBuildBase`, não intransponível, a ≥ 4 tiles de outra); (2) cidades de outras facções (insetos) só são realocadas se caírem em tile ruim; (3) limpa mutadores e marcos velhos; (4) marcos (`WorldGenStep_Landmarks`), mutadores (`WorldGenStep_Mutators.AddMutatorsFromTile`) e estradas (`WorldGenStep_Roads`) refeitos com a semente da partida.
- **Achado:** o preset troca biomas mas **não limpa os mutadores** sorteados para o terreno aleatório (~59 mil tiles); um "bosque" de planta que o bioma novo não tem quebra o Faction Territories 2 (`TileMutatorWorker_PlantGrove.GetLabel` NRE, simulação da cidade parada). Resolvido no passo 3.
- **Resultado** (roteiro `mundo-m3.txt`, `mundo/preview-m3.png`): 16 cidades nas regiões da seção 6, nenhuma na água; 178 marcos; ~870 tiles de estrada; no dia 3 o Hegemony dá ~950 tiles de território (Forja 279, Coro 248, Hélice 166, insetos 120, Silêncio 61, menores 4–26). **Las Vegas só pega 4 tiles** (deserto) e o Silêncio ~20 por cidade: ver no F2.
- **Falta no M3:** cenários próprios usando o mundo; Worldbuilder + The Earth no modpack e o preset dentro do nosso mod (pasta `Worldbuilder/`); capital com algum efeito (hoje é só a primeira da lista).

### Resultado do M3b (2026-11-19): mundo fixo jogável

- **Preset dentro do mod:** `Mods/EcosDaQueda/Worldbuilder/EcosDaQueda/` (Preset.xml, TerrainData.xml, Thumbnail.png). O Worldbuilder lê a pasta `Worldbuilder/` de cada pasta carregada dos mods ativos (`WorldPresetManager.GetAllPresets`). É a cópia que vale; `mundo/preset-v2/` fica como histórico. Ao refazer o preset (`mundo-m2b.txt`), copiar o resultado de `test-profile/full/Worldbuilder/EcosDaQueda` para lá (preset de mesmo nome na pasta do usuário ganha da do mod e mistura caminhos).
- **Modpack:** só o **Worldbuilder** entrou (ModsConfig do usuário e `modpack/`, logo depois do VEF; `loadAfter` no nosso About). **O The Earth fica fora:** traz facções, culturas e genes próprios e só serve para refazer o preset (`-Include allyjamy.terraearthodysseyv.wb`).
- **Cenários:** os 6 cenários do Ecos têm a peça `WB_StartInWorld` (`worldPresetName` EcosDaQueda, `MayRequire="ferny.worldbuilder"`). O Worldbuilder pula a tela de parâmetros do mundo e gera direto do preset: o jogador vai da escolha do cenário/narrador para a escolha do tile (✱ conferir na tela).
- **Semente:** nesse caminho o Worldbuilder usa sempre a semente gravada no preset (`EcosDaQueda`), então marcos, ruínas, estradas e mapas sairiam iguais em toda partida. `FixedWorldDef.fixedSeed`: quando o mundo chega com ela, `FixedWorld` sorteia outra antes de refazer marcos/mutadores/estradas (relatório `semente_nova=`). Semente dada pelo executor (`seed=`) fica como veio.
- **Executor:** `newgame <cenário>` sem `preset=` segue o caminho do cenário (preset, cobertura e semente do preset). Roteiro `mundo-m3b.txt` (na regressão): 16 cidades, 178 marcos, ~870 tiles de estrada, 0 erros.
- **Fica para depois:** capital com efeito (melhor junto do F2, que mexe no multiplicador de território por cidade); texto da peça de cenário vem em inglês do Worldbuilder ("Start in world: EcosDaQueda").

### Resultado do F2 (2026-11-19): fronteira que cresce

- **Recálculo confirmado no código:** a assinatura que decide se o Hegemony refaz o território (`TerritorySolver.Signature`) inclui o `decayMultiplier` de cada cidade; mudar só o nosso multiplicador refaz (checagem a cada 5 s reais). A distância do Hegemony é **custo de caminho** (`Edge`: `movementDifficulty` do bioma + morro 0,5/1, montanha 2, intransponível 10; estrada reduz), não tiles: por isso Las Vegas, entre montanhas, pega poucos tiles.
- **Como funciona** (`Frontier.cs`, `Defs/World/EQ_Fronteira.xml`, postfix em `SettlementDecay` no módulo do Hegemony): cada cidade de NPC tem raio permitido R (começa em 1,5, +1 tile a cada 5 dias se pobre até 3 dias se rica; progresso salvo, em degraus de 1 tile para o Hegemony só recalcular quando muda) e teto C = raio natural do Hegemony × facção × capital (1,3). O multiplicador vira m = 2·log2(√P ÷ 10) ÷ (1,5 · min(R, C)). Só no mundo fixo (`WorldComponent_EQFrontier.active`, ligado pelo `FixedWorld`, salvo no jogo). Jogador fica como o Hegemony faz (F4).
- **Capital com efeito:** teto ×1,3 (as 4 capitais marcadas pelo `FixedWorld`). **Facções:** Sindicato teto ×2 (deserto e montanha), Silêncio ×1,4 (Rochosas), Não Catalogados não fundam cidades.
- **Cidade nova:** cidade no teto → a facção funda outra a 6–9 tiles, na própria terra ou em terra de ninguém (nunca na de outra facção), a ≥ 4 tiles de qualquer assentamento, preferindo estrada; 1 a cada 20 dias por facção; até 6 cidades (potências) ou 2 (menores do mundo fixo). Nome pelo gerador da facção; mensagem para o jogador.
- **Medição** (roteiro `fronteira.txt`, comandos `frontier` e `frontierskip`): dia 2 ≈ 100–120 tiles no total (antes ~950); +12 dias ≈ 850; +40 dias ≈ 1.300, com 10–11 cidades novas (potências com 5). Las Vegas: 1 tile no dia 2, ~14 por cidade no fim.
- **Achado à parte:** com "Saídos do abrigo" (começo no abrigo selado do VQE Ancients), o Hospitality falha ao trazer visitas ("parms couldn't be resolved", dia 2–3), também sem o F2. O roteiro usa "Andarilhos".

## 5. Blocos (ordem sugerida)

| Bloco | O que entrega | Depende de |
|---|---|---|
| ~~**M1**~~ | Baixar e testar Worldbuilder + The Earth [Odyssey] no perfil de teste; ler o formato; lista de biomas (**feito 2026-11-19**) | — |
| ~~**M2**~~ | Conversor América do Norte → 25%; primeiro mundo gerado e carregado no executor (**feito 2026-11-19**, preset v1) | M1 |
| ~~**M2b**~~ | Pintar os 17 biomas que faltam, refazer rios e nomes (**feito 2026-11-19**, preset v2; estradas vão para o M3) | M2 |
| ~~**M3**~~ | (**M3a e M3b feitos 2026-11-19**: cidades fixas, marcos, mutadores, estradas; preset no mod, Worldbuilder no modpack, cenários abrem no mundo) Mundo do Ecos: posições fixas das facções (✱ mapa de regiões), marcos, zonas; cenários usam o mundo; geração aleatória desligada | M2, ✱ regiões |
| ~~**F1**~~ | Como o território do Hegemony cresce e se dá para controlar por cidade (**feito 2026-11-19**: dá, pelo multiplicador por cidade; caminho A confirmado) | — |
| ~~**F2**~~ | Fronteira que cresce aos poucos + cidade nova pela distância (NPCs) (**feito 2026-11-19**, com capital) | F1 |
| **F3** | Bônus por tile (bioma, zona da Queda, marcos) | F1 |
| **F4** | Fronteira do jogador: marcos de fronteira, direção, cidade nova do jogador | F2 |
| **F5** | Exército toma fronteira (frentes de fronteira passam tiles) | F2, 10d |
| **F6** | Medição de desempenho em jogo longo no mundo novo (10a) e ajustes | tudo |

## 6. Proposta de regiões (para o usuário ajustar)

| Facção | Região | Por quê (lore) | Cidades iniciais |
|---|---|---|---|
| **Irmandade da Forja** | Grandes Lagos / cinturão da ferrugem | fábricas, aço, linhas de montagem: o "berço do recall" | capital em Detroit; Pittsburgh; Chicago |
| **Concílio da Hélice** | Costa Oeste | laboratórios, marketing e assinatura: genética como produto | capital no Vale do Silício; Los Angeles; Seattle |
| **O Coro Ascendente** | Costa Leste | corte, capital simbólica, impostos e cerimônias | capital em Washington; Nova York; Boston |
| **Ordem do Silêncio Perpétuo** | Montanhas Rochosas e o norte | isolada; bunkers e laboratórios fechados (Cheyenne Mountain, Los Alamos) | capital sob a montanha (Colorado); Los Alamos; Yellowknife |
| **Sindicato do Saque** | Deserto do sudoeste | estradas, água como poder (Primazia da Água), saque de comboios | Las Vegas |
| **Reino do Lodo** | Pântanos do Golfo | mutantes no lodo, "matéria-prima" | Nova Orleans |
| **Não Catalogados** | Apalaches (florestas fechadas) | criaturas que a Ordem quer catalogar, longe das cidades | um ninho nas montanhas |
| **Colmeia Erguida** | Sul quente (Texas, ou Yucatán se o México entrar) | insetos, calor, terra aberta | uma colmeia |
| **Feira Andante** | estradas do Meio-Oeste | já é móvel (8 feiras que viajam); não precisa de lugar fixo | — |
| **Fora de Linha** | dentro das terras da Forja | não tem assentamento (células escondidas) | — |
| **Fungobi** | ruínas de cidades destruídas | facção oculta | — |

O jogador começa num tile livre, como hoje (a confirmar). As potências ficam em cantos opostos: Forja e Coro vizinhos no nordeste (são neutros entre si), Silêncio entre todos pelo meio-oeste montanhoso (inimigo do Coro e da Forja), Hélice isolada a oeste (rival da Forja).

## 7. Decisões do usuário (✱)

**Decidido em 2026-11-19:** 3 assentamentos por potência (1 capital); motor do território = **Hegemony + regras nossas** (caminho A); regiões propostas por mim (seção 6), o usuário ajusta; **pode baixar** Worldbuilder e The Earth [Odyssey]. **Proposta de regiões aprovada** (seção 6). **Recorte: EUA, Canadá e México (até o Yucatán); o Caribe fica de fora.** Floresta e pântano tropicais vêm do sul do México; os tropicais do Alpha Biomes (selva de feraliscos, manguezal miasmático) são pintados ali. **Biomas fixos e todos os do modpack presentes; marcos, ruínas e sítios exploráveis variam por partida.**

Em aberto:

1. Ajustes na proposta de regiões (seção 6).
5. **Distância para cidade nova** e **ritmo** do crescimento (proposta: cidade nova quando a borda passa de 6 tiles da cidade mais próxima; fronteira cresce 1 tile a cada 3–5 dias, mais rápido com riqueza).
6. **Onde o jogador começa:** tile livre à escolha (como hoje) ou lugares fixos por cenário.
