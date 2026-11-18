# Mods de conteúdo, visual e interface

Mods que só adicionam itens/construções/animais, ou mudam visual e interface. Descrição curta: para que serve e o que adiciona. Pasta = `Mods/<id>`. Mecânicas e frameworks estão em [mecanicas.md](mecanicas.md) e [`frameworks/`](frameworks/).

## Armas e armaduras

| Mod | packageId | O que adiciona | Notas para o Ecos |
|---|---|---|---|
| Vanilla Weapons Expanded - Laser | `VanillaExpanded.VWEL` | Armas laser ultratec (poucas fabricáveis), tag `LaserGun` | Usado nas PawnKinds do Silêncio/Hélice |
| VWE - Coilguns | `VanillaExpanded.VWEC` | Rifles, revólver e lança gauss (Spacer, penetração total); `ExpandableProjectileDef` | Tag `Coilgun` |
| VWE - Makeshift | `VanillaExpanded.VWEMS` | Armas improvisadas (pistola, SMG, rifle, LMG, escopeta), estilo Rust/Fallout | Tom perfeito para saqueadores pós-Queda; tag `MakeshiftGun` |
| VWE - Bioferrite | `VanillaExpanded.VWEBF` | Armas de bioferrita (Anomaly) com habilidades que gastam cargas | Tag `VWEB_BioferriteWeapon`; Silêncio |
| VWE - Frontier (2454918139) — **novo** | `VanillaExpanded.VWEFT` | Armas estilo faroeste (revólveres, rifles de alavanca) com pesquisa própria; liga com VFE Settlers | Patch CE já existe; boa opção para facções "pioneiras" |
| Vanilla Weapons Expanded (1814383360) — **novo** | `VanillaExpanded.VWE` | 30+ armas de todos os níveis (revólver pesado, escopeta de trincheira, fuzil de batalha, antimaterial, armas de carga, arcos e bestas com flechas especiais), traços de arma (`WeaponTraitDef`); tags genéricas (`SpacerGun`, `IndustrialGunAdvanced`, `GunHeavy`, `NeolithicRanged*`). 1.7; patch CE no próprio CE | Entra sozinho em PawnKinds com tags genéricas (os nossos arsenais quase só usam tags próprias e dos módulos VWE). **Proposta:** antimaterial e armas de carga na Forja; arcos compostos e bestas nos Não Catalogados e na Colmeia |
| Combat Extended Armors / Armory / Guns | `CETeam.CombatExtended*` | Roupas, armas e equipamentos feitos para o CE (Armory: 93 itens) | Só existem com CE |

## Móveis e construções

| Mod | O que adiciona |
|---|---|
| Vanilla Furniture Expanded (`vanillaexpanded.vfecore`) | 30+ móveis de todos os níveis (mesas, cadeiras, iluminação, decoração), estilos |
| VFE - Architect | Pisos, cercas, paredes decorativas organizados em grupos de menu |
| VFE - Farming | Hidropônicos (início e fim de jogo), pesquisa e aba própria |
| VFE - Medical Module | Camas médicas, mesas vinculáveis, paredes estéreis com faixa colorida |
| VFE - Power | Geradores novos com progressão tecnológica de energia, aba de pesquisa própria |
| VFE - Production | Estações de produção de fim de jogo (oficinas avançadas) |
| [sbz] Neat Storage (3416243474) | 27 móveis de armazenamento com visual que muda pelo conteúdo (via [Adaptive Storage](frameworks/adaptive-storage.md)) |
| Door Mat (`dracoix.doormat.r12a`) | Capachos que limpam os pés (menos sujeira dentro) |
| Adaptive Simple Storage (3297307747) — **novo** | 21 caixotes, barris, fardos e paletes (via [Adaptive Storage](frameworks/adaptive-storage.md)); visual de depósito de sucata para as bases do Sindicato e da Feira |
| LWM's Adaptive Deep Storage (3373064575) — **novo** | 16 móveis de armazenamento profundo (prateleiras, cofre, armário de armas, palheiro, caçamba) |
| Adaptive Primitive Storage (3400037215) — **novo** | 14 depósitos primitivos (cestos, potes de barro, celeiro, adega, varal de carne, tenda); bons para Reino do Lodo, Não Catalogados e Colmeia nas plantas do CBF |
| VFE - Security (1845154007) — **novo** | Defesas e artilharia de todos os níveis (detalhes em [mecânicas](mecanicas.md)) |
| Trenches | Trincheiras e abrigos (ver mecânicas) |

## Animais e natureza

| Mod | O que adiciona | Notas para o Ecos |
|---|---|---|
| Vanilla Animals Expanded — Waste Animals (`vanillaexpanded.vaewaste`) | 11 animais de biomas poluídos (corpos, sons, produtos); usa VEF AnimalBehaviours e MVCF | **Feito (bloco L):** bichos nos ataques do Sindicato e do Reino do Lodo |
| Vanilla Plants Expanded (+ More Plants, Mushrooms) | Frutas, legumes, cogumelos (detalhes em mecânicas) | — |

## Visual e animação (precisam de compatibilidade só se fizermos armas/apparel com gráfico próprio)

| Mod | O que faz | Compat |
|---|---|---|
| [NL] Facial Animation - WIP (`nals.facialanimation`) | Rostos animados (piscar, emoções, comer) com 18 tipos de Def de rosto | Cabeças/xenótipos modded podem precisar de `HeadTypeDef` próprio do mod; **desativado no modpack** (bug a corrigir depois) |
| Show Me Your Hands (`mlie.showmeyourhands`) | Desenha as mãos do pawn na arma | Posições automáticas pela textura; `WHands.ClutterHandsTDef` para ajustar arma específica |
| Show Me Your Tools (3738350131) — **novo** | Cada trabalho mostra uma ferramenta animada (picareta, machado, solda…) com sons e detritos | `JobEffects.JobToolDef` liga `JobDef` → ferramenta; JobDefs novos nossos ficam sem ferramenta até ganharem um |
| Dubs Apparel Tweaks | Esconde chapéu dentro de casa/na cama, cabelo sob chapéu, velocidade de vestir | — |
| World Map Enhanced (Continued) | Texturas melhores para o mapa-múndi (inclui biomas do Odyssey) | — |
| Clean Textures (2865361569) — **novo** | 220+ texturas de terreno mais limpas (vanilla, Alpha Biomes, VPE Plants…); só texturas, carrega no fim; 180 MB | — |

## Interface e qualidade de vida

| Mod | O que faz |
|---|---|
| DragSelect | Arrastar para selecionar vários itens em comércio/caravana e prioridades de trabalho |
| Dubs Mint Menus | Listas com busca para contas, cirurgias, plantas, arquiteto e pesquisa |
| EdB Prepare Carefully | Editor completo dos colonos iniciais com presets |
| Map Preview | Prévia do mapa local no mapa-múndi antes de escolher |
| Yet Another Research Tree (YART) | Árvore de pesquisa em grafo, fila por canal; as 6 abas próprias da Etapa 3a aparecem nela |
| Clean Research Sort | Ordena automaticamente a árvore de pesquisa vanilla |
| Smart Speed | Velocidade ultra sem modo dev e multiplicador de eventos |
| Cherry Picker | Remove seletivamente conteúdo de mods (sem quebrar referências) — útil para tirar itens que não combinam com o tom |
| Allow Tool | Ordens em massa (permitir/proibir, colher, transporte urgente) |
| Character Creator (`astryl.ModernCC`, 3762126187) | **Temporário:** editor de pawns que o usuário usa para montar as roupas das facções; contorna a trava de xenos da etapa 1 (não travar). Sair do modpack quando ele avisar |

## Desempenho e depuração

| Mod | Uso |
|---|---|
| Dubs Performance Analyzer | Mede o custo de cada parte do jogo e de cada patch Harmony — usar para perfilar o nosso C# |
| Missile Girl - Performance Mod | Otimizações gerais; deve ficar no fim da lista |
| XML Extensions | Além das funções de framework, mostra stack trace de erros de XML (ver guia) |

## Nossos mods

| Mod | packageId | Branch |
|---|---|---|
| Ecos da Queda | `ecosdaqueda.core` | `mod/ecos-da-queda` |
| Ecos da Queda - Tradução PT-BR | `ecosdaqueda.traducao` | `mod/ecos-da-queda-traducao` |
| Ecos da Queda - Ferramentas de Debug | `ecosdaqueda.debug` | `mod/ecos-da-queda-debug` |
