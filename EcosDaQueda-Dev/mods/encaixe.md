# Encaixe de cada mod no modpack

Uma linha por mod da lista ([`../../modpack/mods.md`](../../modpack/mods.md)), na ordem de carga. **Uso hoje** = o que o Ecos da Queda já faz com o mod (bloco do `PLANO.md` entre parênteses). **Proposta** = ideia ainda não feita (as de peso também estão no `PLANO.md`). "Apoio" = só qualidade de vida, visual, desempenho ou dependência de outro mod; nada a fazer no Ecos. Detalhes de cada mod: [mecanicas.md](mecanicas.md), [conteudo.md](conteudo.md) e [`frameworks/`](frameworks/).

Auditado em 2026-11-17, com os 11 mods novos do mesmo dia (contagem de `MayRequire`/referências em `Mods/EcosDaQueda`). Ao pôr ou tirar mod, atualizar esta tabela.

| Mod | Papel | Uso hoje no Ecos | Proposta |
|---|---|---|---|
| Harmony | biblioteca | todo o nosso C# (patches) | — |
| Adaptive Storage Framework | dependência (Neat Storage) | — | — |
| HugsLib | dependência de outros mods | — | — |
| Jaeger's Utility Framework | dependência (Hegemony) | — | — |
| RimExodus Framework | dependência (Hegemony; Seamless World desativado) | — | — |
| Vehicle Framework | framework de veículos | indireto, pelo VVE (M) | — |
| XML Extensions | framework XML | — | opções do Ecos no menu, se um dia houver configurações |
| [FSF] Filth Vanishes With Rain And Time | apoio | — | — |
| [sbz] Neat Storage | apoio (móveis) | — | — |
| Adaptive Simple / Deep / Primitive Storage (novos) | apoio (armazenamento) | — | depósitos nas plantas de base do CBF (sucata no Sindicato, primitivos no Reino do Lodo) |
| Allow Tool | apoio | — | — |
| Cherry Picker | apoio | — | tirar conteúdo fora do tom, se aparecer |
| Vanilla Expanded Framework | framework | dependência; genes, estoque, missões dos VQE, KCSG | — |
| Alpha Memes | memes | memes nas ideologias (Factions, MinorFactions) | — |
| DragSelect | apoio | — | — |
| Dubs Apparel Tweaks | apoio | — | — |
| Dubs Bad Hygiene | necessidades da colônia | — | água como poder do Sindicato e exigência de cargo do Coro (4a) |
| Common Sense | apoio | — | — |
| Dubs Mint Menus | apoio | — | — |
| Dubs Performance Analyzer | apoio (perfilar nosso C#) | — | — |
| EdB Prepare Carefully | apoio (início) | a trava de xenos (etapa 1) convive com ele | — |
| Expanded Prosthetics and Organ Engineering - Forked | próteses | pesquisa e comércio (Research, Traders) | próteses avançadas como licença da Hélice |
| Geological Landforms | apoio (mapa) | — | — |
| Map Preview | apoio | — | — |
| Pick Up And Haul | apoio | — | — |
| Replace Stuff - Continued | apoio | — | — |
| Show Me Your Hands | apoio (visual) | — | — |
| Simple sidearms | combate | — | arma secundária nas PawnKinds de elite |
| Smart Medicine - Continued | apoio | — | — |
| Smart Speed | apoio | — | — |
| Smarter Construction | apoio | — | — |
| Ugh You Got Me | apoio | — | — |
| Snap Out! (novo) | apoio | — | — |
| Animals Logic (novo) | apoio | — | — |
| Realistic Rooms Rewritten (novo) | regra de cômodos | — | conferir exigências de cômodo dos cargos (Royalty) |
| Vanilla Animals Expanded — Waste Animals | fauna de lixão | bichos nos ataques do Sindicato e do Reino do Lodo (L) | — |
| Vanilla Anomaly Expanded - Insanity | sanidade | pesquisa do Silêncio paga em sanidade, retiro de silêncio (I) | — |
| Vanilla Brewing Expanded | bebidas | estoque de comerciante | — |
| Vanilla Brewing Expanded - Coffees and Teas | bebidas | — (apoio) | café/chá no estipêndio de alguma potência |
| Vanilla Cooking Expanded | comida | estoque de comerciante | — |
| Vanilla Cooking Expanded - Bakery | comida | — (apoio) | — |
| Vanilla Factions Expanded - Empire | facção | Império fora do mundo (WorldFactions); itens na pesquisa e comércio | — |
| Vanilla Factions Expanded - Deserters | facção | Desertores fora do mundo (WorldFactions) | contrabando como modelo da pirataria de patentes |
| Vanilla Factions Expanded - Insectoids 2 | insetos | colmeia destruída = "recall" da Hélice; núcleos no comércio (E) | — |
| Vanilla Furniture Expanded - Security (novo) | defesas e artilharia | 13 pesquisas caem nos ramos só pelo nível | classificar as pesquisas; defesas nas bases do CBF e cercos; artilharia da Forja |
| Vanilla Events Expanded (novo) | eventos | — | base dos eventos da etapa 5 com texto e gatilho nossos |
| Vanilla Furniture Expanded (+ Architect, Farming, Medical, Power, Production) | móveis e produção | — (apoio; aparecem na árvore de pesquisa reclassificada, 3a) | — |
| Vanilla Ideology Expanded - Memes and Structures | memes | memes e preceitos nas nossas ideologias (35 referências) | — |
| Vanilla Landmarks Expanded | marcos | 10 marcos próprios usam o framework de marcos (N) | — |
| Vanilla Nutrient Paste Expanded | comida | — (apoio) | pasta nutritiva como ração dos Fora de Linha/abrigos |
| Vanilla Plants Expanded (+ More Plants, Mushrooms) | plantas | estoque de comerciante | — |
| Vanilla Psycasts Expanded (+ Hemosage) | psíquico | caminhos licenciados por cargo do Coro (G) | — |
| Vanilla Quests Expanded - Ancients | missões | missões dão Fama ao Silêncio; início "Saídos do abrigo" (F) | — |
| Vanilla Quests Expanded - Drone Factory | missões | missões para a Forja; drone sem cargo = pirataria (F) | — |
| Vanilla Quests Expanded - The Generator | missões | missões para a Forja (F) | — |
| Vanilla Races Expanded - Android | raça | questão androide da Forja e dos Fora de Linha (H) | — |
| Vanilla Races Expanded (Fungoid, Genie, Highmate, Hussar, Lycanthrope, Phytokin, Pigskin, Sanguophage, Saurid, Waster, Insector) | xenos e genes | genes nos nossos xenos e na Linha Exótica da Hélice (O) | — |
| Vanilla Races Expanded - Starjack | xeno | só na Linha Exótica (pacotes de genes) | xeno de alguma facção espacial, se houver |
| Vanilla Recycling Expanded | reciclagem | carga do Sindicato (L); pré-requisito da engenharia reversa (3c) | — |
| Vanilla Traits Expanded | traços | traços dos líderes e cargos altos (L) | — |
| Hegemony - Faction Territories and Vassalage | estratégia | camada de estratégia; disposições e vetos (pasta `Mods/Hegemony`) | eventos de guerra por potência (etapa 5) |
| Vanilla Vehicles Expanded | veículos | blindado da Forja, títulos de posse da Feira (M) | — |
| Vanilla Vehicles Expanded - Upgrades | veículos | — (apoio) | — |
| Vanilla Weapons Expanded (base, novo) | armas | entra pelas tags genéricas | antimaterial/carga na Forja; arcos e bestas nos neolíticos |
| Vanilla Weapons Expanded - Laser / Coilguns / Makeshift / Bioferrite / Frontier | armas | arsenal por facção via tags (A) | — |
| Vanilla Factions Expanded - Pirates | facção | armadura-caixão (Warcasket) da Forja (C) | maldições como pacto do Coro/Silêncio |
| World Map Enhanced (Continued) | apoio (visual) | — | — |
| Vanilla Factions Expanded - Medieval 2 | medieval | arsenal dos Não Catalogados, vinho do Coro, torneio (D) | — |
| Combat Extended (+ Armors, Armory, Guns) | combate | munição e loadouts nas PawnKinds e cenários | — |
| Custom Base Framework | bases de NPC | plano de base por potência (K) | — |
| Alpha Biomes | biomas | — | zonas da Queda com recurso exclusivo (4b) |
| Alpha Genes | genes | genes e xenos (222 referências) | — |
| [FSF] Complex Jobs | apoio | — | — |
| Missile Girl - Performance Mod | apoio | — | — |
| Rimesis | nêmesis | — | ligar ao Silêncio ou remover (4b, decidir) |
| Layered Wall Destruction Fork | combate | — | paredes nossas com perfil; cercos (4b) |
| Trenches | combate | — | trincheiras nas bases e cercos (4b) |
| Tech Advancing | progressão | — | conferir convivência com as licenças por cargo (3b) |
| Yet Another Research Tree (YART) | interface de pesquisa | 6 abas próprias de pesquisa (3a) | — |
| Clean Research Sort | apoio | — | — |
| Door Mat | apoio | — | — |
| SeedsPlease: Lite Redux | sementes | — | sementes como mercadoria e dízimo (4a) |
| Harvest Organs Post Mortem Continued | órgãos | — | mercado de órgãos da Feira e da Hélice (4a) |
| Custom Ritual Framework | rituais | — | rituais próprios das ideologias (4c) |
| Smart Farming | apoio | — | — |
| Show Me Your Tools | apoio (visual) | — | ferramenta para JobDefs novos nossos |
| Blessed Conception Psychic Ritual | ritual | — | ritual do Silêncio (4c) |
| Hospitality (novo) | hóspedes e recrutamento | — | Fama por hospedar; Infâmia por aliciar membro de potência |
| Clean Textures (novo) | apoio (visual) | — | — |
| Character Creator | **temporário** | usado pelo usuário para montar roupas das facções | tirar do modpack quando ele avisar |
