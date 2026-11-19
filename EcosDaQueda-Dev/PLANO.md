# Plano de desenvolvimento em etapas

_Criado em 2026-11-15 a partir da lista do usuário. Cada etapa termina com teste no executor e atualização do STATUS.md. A ordem pode mudar; o que está marcado **(decidir)** depende de resposta do usuário._

## Resumo

| Etapa | O que entrega | Tamanho | Depende de |
|---|---|---|---|
| 0 | Ideologias da Feira e do Sindicato; vassalagem sem nível psíquico indevido | feito | — |
| 1 | Travas: só os nossos xenos na criação, cenários próprios, narradores de fora ocultos | **feito** | — |
| 2 | Utilidade para os mods Vanilla Expanded de conteúdo | **feito** | 1 (auditoria) |
| 3 | Sistema de tecnologia próprio (sucata, dados antigos, licenças das potências) | **feito** (3a–3e) | 2 (saber o que cada potência "possui") |
| 4 | Utilidade para os outros mods | médio | 2, 3 |
| 5 | Eventos do mundo + missões das facções menores | grande | 2, 3 |
| 6 | Narrador próprio | médio | 5 (ele agenda os eventos) |
| 7 | Qualidade de vida no estilo HSK | médio | independente |
| 8 | Pendências antigas | pequeno | independente |
| 9 | Polimento, balanceamento e lançamento | médio | todas |
| 10 | Estratégia, desempenho e mundo fixo (lista do usuário de 2026-11-17) | grande | 3 (fecha a tecnologia); 10a mede antes de mexer |
| 11 | Cargos com obrigações reais: quanto mais alto o cargo, mais a potência cobra (comandar exército na guerra, administrar coisas da potência) — pedido do usuário de 2026-11-18 | grande | 5a (eventos), 10d (tropas do Hegemony) |

## Roteiro do que falta (revisto em 2026-11-17)

Um bloco por sessão. Estimativa: **~25 blocos**. Ordem recomendada (cada linha é um bloco; ✱ = depende de decisão ou material do usuário):

**Etapa 3 — tecnologia (4 blocos).** Hoje tudo fora da bancada fica fechado num jogo real, então esta etapa vem primeiro.
1. ~~**3b Licenças**~~ (feito): cada cargo das 4 potências licencia projetos do ramo dela; compra de projeto com favor na aba Reputação (preço por custo/nível); licença perdida = pirataria ao fabricar (Infâmia, como drones/androides); solução para techprints (a potência vende ou a licença dispensa).
2. ~~**3c Engenharia reversa**~~ (feito; **investigação de estrutura** acrescentada em 2026-11-19: o mesmo para construções, no lugar, inclusive ruínas do Velho Mundo): bancada própria (arte do Vanilla Recycling); desmontar item dá 15–30% do projeto que o fabrica; patente de potência sem licença dá Infâmia.
3. ~~**3d Dados antigos**~~ (feito): manual técnico e disco do Velho Mundo em abrigos do VQE Ancients, ruínas, marcos nossos e na Feira; liberam o projeto e dão 40–50%.
4. ~~**3e Fechamento**~~ (feito, 2026-11-18): **dízimo tecnológico** da Forja (1 dado antigo + 4 componentes → mech de trabalho, cargo 2; 2 dados + 1 componente espacial → título de veículo seminovo, cargo 3); auditoria `tech buildaudit`: dos 59 prédios espaciais construíveis, **0 escapam** da trava (todos já pedem projeto de ramo de potência), então móveis e energia não precisaram de patch. Ficam para o jogo do usuário: balanço dos custos e campanha curta ✱.
4b. **3f Pesquisa por manual** (ideia do usuário de 2026-11-18; ~3 blocos, ver seção da Etapa 3): toda pesquisa nasce trancada e só abre com o manual técnico dela; manuais por comércio, exploração, missões e engenharia reversa (que ensina o item, não o projeto inteiro). Decisões ✱ listadas lá antes de começar.

**Etapa 8 — pendências técnicas (1 bloco, pode vir antes, entre etapas).**
5. ~~Executor: erros de carregamento no `RESULTADO`, `problemas=0`/androides; bases com vários líderes; perfil lean aposentado; trava da volta ao menu no lote (autosave sem janela); spam do Hegemony no mundo novo; livros de pesquisa; traduções oficiais quebradas (Anomaly, Odyssey)~~ (feitos em 2026-11-18). Sobram intermitentes de terceiros raros (lista no STATUS), tratados quando reaparecerem com o log agora completo.

**Etapa 5 — eventos do mundo (7 blocos).**
6. ~~**5a Eventos por potência**~~ (feito, 2026-11-18, `PowerEvents.cs`; gatilho por guerras do Hegemony feito no 10d: chamada às armas): auditoria da Hélice, inspeção de garantia da Forja, turnê do Coro, chamada de artigos do Silêncio (gatilhos: reputação, cargo, guerras do Hegemony).
7. ~~**5b Fora de Linha, fase 2**~~ (feito, 2026-11-18, `ForaDeLinha.cs`, `EQ_ForaDeLinha.xml`): Exposição perante a Forja (0–100, aviso em 50, reação em 100), dados de diagnóstico (caem de mechs e técnicos da Forja), ofertas da rede (contrabando, apagar registro, sabotagem) e trair a célula (proposta da Forja). Falta: a Forja exigir a entrega do androide desperto (recall) e mercado negro de licenças.
8. ~~**5c Não Catalogados, fase 2**~~ (feito, 2026-11-18, `EQ_NaoCatalogados.xml`): expedição de coleta do Silêncio (colono emprestado, bioferrita e fragmento; 40% de contra-ataque dos Não Catalogados em 3 dias) e contrato de contenção oferecido pelos Não Catalogados mesmo hostis (tributo em carne ou prisioneiro; o Silêncio cobra Infâmia). Falta: missão de defesa/resgate (fica para a 5g, com as missões).
9. ~~**5d Fungobi**~~ (feito, 2026-11-18, `Fungobi.cs`): facção oculta sempre hostil, horda (incidente de ameaça grande, dia 12+) e ruínas de assentamentos destruídos com moradores Fungobi (70%).
10. ~~**5e Facções menores**~~ (feito, 2026-11-18, `EQ_MinorEvents.xml`): greve do Sindicato (trégua real de 20–30 dias sem ataques dele, mesmo sendo inimigo permanente), liquidação da Feira (lotes fechados), recolhimento de colmeia pela Hélice (contra a Colmeia Erguida, com contra-ataque) e o Reino do Lodo exigindo "matéria-prima" (prisioneiro ou carne; recusar = 70% de ataque).
11. ~~**5f Eventos sem dono**~~ (feito, 2026-11-18, `WorldEvents.cs`): tempestade de cinzas, abrigo que se abre, comboio perdido (o Sindicato às vezes vem buscar), recall mecanoide.
12. ~~**5g Missões dos mods com texto nosso**~~ (feito, 2026-11-18): tradução dos VQE (sessão própria, `Mods/EcosDaQuedaTraducao`) e quem pede = líder da potência patrona do `EQ_Missions.xml` enquanto ela não for hostil (`QuestAskers.cs`); as versões "sem pedinte" (rádio interceptado) continuam.

**Colmeia Erguida (1 bloco) ✱** — assim que o usuário entregar a ideologia (`.rid`).
13. Ligar a ideologia, história, relações fixas, parte horrível, descrição.

**Etapa 4 — outros mods (3 blocos).**
14. ~~**4a Mercados**~~ (feito, 2026-11-18): órgãos já eram vendidos (Hélice, Feira e outros); sementes na Feira (loja e caravana) e no dízimo do Coro (tipo `Category`). **Água ficou de fora:** a garrafa do Dubs só existe com a sede ligada, e ela está desligada no modpack (decidir ✱ se liga a sede).
15. ~~**4b Mundo**~~ (feito sem o Rimesis, 2026-11-18, `Zones.cs`, `Sieges.cs`): 8 zonas da Queda (recurso exclusivo → 12% de projeto do ramo na bancada de engenharia reversa); trincheiras e arame nos cercos; Layered Walls já age sozinho. Falta: Rimesis ✱. Proposta original: Alpha Biomes como zonas da Queda com recurso exclusivo para tecnologia; Trenches e Layered Walls nos cercos; Rimesis ✱ (ligar ao Silêncio como campeões nêmesis ou remover).
16. ~~**4c Rituais**~~ (feito em parte, 2026-11-18, `RitualOutcomes.cs`): 4 resultados anexáveis de ritual, um por potência (fama, favor, boa vontade e itens). Falta: rituais completos próprios das ideologias das potências pelo Custom Ritual Framework (só fazem sentido para quem joga com essas ideologias ✱) e Blessed Conception.

**Etapa 6 — narrador (2 blocos) ✱** (variações a decidir; retrato precisa de arte).
17. ~~**6a Narrador principal**~~ (feito, 2026-11-19, `Narrator.cs`): atacante pela Infâmia com as potências hostis, Sindicato no começo, fim de jogo segurado até cargo ou pesquisa espacial. Falta ✱: maldições do VFE Pirates como dificuldade (junto com as variações).
18. **6b Variações** (clássico, calmo, caótico) e textos.

**Etapa 7 — qualidade de vida estilo HSK (2 blocos) ✱** (escolher o que vale).
19–20. Preferir mods prontos para 1.7 e CE; o resto por patch nosso.

**Etapa 10 — estratégia, desempenho e mundo fixo (~9 blocos; lista do usuário de 2026-11-17).** Detalhe na seção da Etapa 10. Ordem sugerida: 10a logo depois do 3e; 10b antes da Etapa 5 (os eventos usam o mundo); 10c–10e junto ou logo depois da 5a (guerras); 10f por último, antes da Etapa 9.

**Etapa 11 — cargos com obrigações (~4 blocos; pedido do usuário de 2026-11-18).** Detalhe na seção da Etapa 11. 11a pode vir logo (só usa sistemas prontos); 11c depende do 10d.

**Etapa 9 — polimento e lançamento (4–5 blocos).**
21. **Balanceamento:** preços e quantidades do comércio, laser/bobina no CE, cavaleiro lacrado, custos da pesquisa.
22. **Identidade visual:** ícones das facções menores e das ideologias, roupas próprias das lojas ✱ (Character Creator; tirar do modpack depois), xeno do líder da Forja ✱.
23. **Tradução:** ~50 mods sem PT-BR, marcos do VEE, VFE Medieval 2 (resto).
24. **Campanha de teste:** o usuário joga uma campanha; consertos do que aparecer (inclui a conferência visual acumulada do STATUS).
25. **Lançamento ✱:** descrição, preview, README, versão; decidir se publica (Steam/GitHub) e como.

**Decisões que dependem do usuário:** ideologia da Colmeia; Rimesis (usar ou remover); variações do narrador e arte do retrato; quais itens de QoL; xeno do líder da Forja; quando tirar o Character Creator; se e onde publicar o mod.

## Etapa 0 — feito em 2026-11-15

- Ideologias **A Liquidação Eterna** (Feira Andante) e **O Dissídio Coletivo** (Sindicato do Saque), dos `.rid` do usuário.
- Cerimônia de vassalagem: o nível psíquico que todas as potências davam vinha do Vanilla Psycasts Expanded (1 nível em qualquer outorga). Agora **só o Coro** dá (1 nível na submissão, +1 por cargo); a **Forja instala um vínculo mecanoide** no líder; Silêncio e Hélice não dão nada.

## Etapa 1 — Travas e base (pequena)

1. **Xenos na criação do jogo:** a lista de xenotipos da tela de colonos, o editor de xenotipo personalizado e o Prepare Carefully mostram só os `EQ_Xeno_*` permitidos. Os xenos de fora continuam existindo como definição (os genes deles são usados pelos nossos), só não são escolhíveis.
2. **Cenários próprios**, cada um com os xenos que combinam com aquele início. Proposta **(decidir)**:
   - *Saídos do abrigo* (padrão): Humano Antigo, Vagante.
   - *Andarilhos das terras devastadas*: Vagante, Vagante Esquelético.
   - *Dissidentes de uma potência* (um por potência): o padrão dela (Graxeiro / Corista) ou Vagante, já com Infâmia naquela potência.
   - *Lote Descontinuado*: fugitivos da Hélice.
   Elites (não hereditários) nunca são escolhíveis: só se ganha em jogo (Soro, cargos, serviços).
   Os cenários do jogo e dos mods ficam ocultos.
3. **Narradores de fora ocultos** (`listVisible`): Cassandra, Phoebe, Randy, Casper (VFE Pirates), Basilicus (VPE), Ariadne (VFE Empire), Damocles (VFE Deserters), Empress Evil (VFE Insectoids 2), Maynard (VFE Medieval 2). Entra um narrador nosso provisório (cópia do ritmo da Cassandra) até a etapa 6.
4. **Auditoria automática de uso de mods:** comando de debug que conta, por mod, quantos itens aparecem em unidades, estoques de comércio, bases, recompensas e pesquisas das nossas facções. É a lista definitiva para as etapas 2 e 4 (a lista abaixo é preliminar, feita pelas referências do nosso mod).

**Feita em 2026-11-15** (ver STATUS.md). Ficou como proposto; os cenários de dissidentes são 4 e o da Hélice é o do Lote Descontinuado. Pendente: conferência visual e arte do narrador.

## Etapa 2 — Utilidade para os Vanilla Expanded (grande; fazer em blocos)

**Resultado da auditoria (`modusage`, 2026-11-15):** armas, roupas, comida e próteses dos mods **já aparecem** nas nossas facções, mas só porque compartilham etiquetas genéricas do jogo (qualquer facção industrial recebe). O que falta não é aparecer, é **identidade**: cada potência com o arsenal dela e as outras sem. Sem uso nenhum: construções do VFE Architect, Farming e Production, Trenches, VQE Drone Factory, VRE Starjack e Insector, e as pesquisas, missões e eventos de quase todos os mods.

Preliminar: o que hoje **não tem ligação** com as nossas facções e sistemas, e a proposta de uso.

| Bloco | Mods | Situação hoje | Proposta |
|---|---|---|---|
| A. Arsenal | VWE Laser, Coilguns, Bioferrite, Makeshift, Frontier; CE Guns/Armory/Armors | quase sem referência (1–2 itens) | arsenal por facção: Makeshift → Sindicato e Vagantes; Frontier → Feira e colonos; Laser → Hélice; Coilguns → Forja; Bioferrite → Silêncio; armas psíquicas/nobres → Coro |
| B. Império e Desertores | VFE Empire, VFE Deserters | facções removidas; títulos, permissões e tramas sem uso | roupas e armas nobres → Coro; **permissões por cargo** das 4 potências usando o sistema de permissões; contrabando e "tramas" dos Desertores → base da fase 2 dos Fora de Linha (Exposição contra a Forja) |
| C. Piratas | VFE Pirates | facções removidas | **armaduras-caixão (warcaskets) → Cavaleiros da Forja**; maldições como modificadores de dificuldade do nosso narrador |
| D. Medieval | VFE Medieval 2 | só a classe da feira móvel | torneios e vinho → Coro (corte aristocrática); armas medievais → Não Catalogados e tribos de Vagantes |
| E. Insetos | VFE Insectoids 2, VRE Insector | só a história | ligação com a Hélice ("colmeia sem recall" é produto dela): missões de recolhimento, genes de inseto como mercadoria |
| F. Missões | VQE Ancients, Drone Factory, The Generator | sem ligação com a história | Ancients = abrigos do mundo antigo (origem do jogador e fonte de dados antigos, etapa 3); Drone Factory → Forja; The Generator → início do jogo saído do abrigo |
| G. Psíquico | VPE, VPE Hemosage | 3 referências | caminhos de psycast por cargo do Coro; Hemosage → elite do Coro (Sangue Maestro) |
| H. Androides | VRE Android | Fora de Linha | "questão androide" da Forja: fabricar androide exige licença dela; androide desperto na colônia gera Infâmia na Forja e Fama nos Fora de Linha |
| I. Anomalia | Vanilla Anomaly Expanded – Insanity | sem ligação | sanidade como custo dos serviços e da pesquisa do Silêncio |
| J. Comida e bebida | VCooking, Bakery, VBrewing, Coffees and Teas, Nutrient Paste, VPlants (+More, Mushrooms) | sem ligação | estoque típico por facção (Feira vende de tudo; Coro compra vinho e chá; Hélice vende pasta "premium"; Não Catalogados e cogumelos), itens de dízimo e de estipêndio |
| K. Móveis e energia | VFurniture Core, Architect, Farming, Medical, Power, Production | sem ligação | estilo das bases por potência (Custom Base Framework) e desbloqueio por tecnologia licenciada (etapa 3) |
| L. Sucata e bichos | Vanilla Recycling, VAE Waste Animals, Vanilla Traits | sem ligação | Recycling = fonte de tecnologia por sucata (etapa 3) e identidade do Sindicato; animais de lixo nos biomas poluídos e nos ataques do Sindicato |
| M. Veículos | VVE, VVE Upgrades | 2 referências | caravanas motorizadas da Feira; blindados só com licença da Forja |
| N. Mundo | V Landmarks Expanded | sem ligação | marcos com nome e história nossos |
| O. Genes | VRE Starjack, Phytokin, Fungoid, Saurid, Lycanthrope | 0 a 2 genes usados | conferir se algum xeno nosso deve usá-los; se não, só como mercadoria do Soro/Hélice |

**Andamento (2026-11-17, bloco O):** feito — conferido: nossos xenos já usam genes do Fungoid (Fungobi, Engenheiro-Primaz), Lycanthrope (Bruto/Bruta, Fungobi, Lote Descontinuado, Engenheiro-Primaz), Saurid (Vagante Esquelético, Lote Descontinuado) e Phytokin (Sangue Maestro); só o Starjack ficou sem uso, e nenhum xeno precisou mudar. O resto vira mercadoria: **Linha Exótica da Hélice**, pacotes de genes montados só com genes desses mods e do VRE Insector (reserva de 71 genes), na loja (2–4 pacotes, 1–2 genes) e na caravana (1–2, 1 gene). Não feito: Soro com genes exóticos, colmeia humanoide com os xenos de inseto do usuário (etapa 5). **Etapa 2 concluída.**

**Andamento (2026-11-17, bloco N):** feito — 10 marcos próprios (`Defs/World/EQ_Landmarks.xml`) com nome sorteado em PT-BR e história na descrição, reaproveitando mutadores do Odyssey e do VEE: cratera da Queda (+ destroços mecanoides), campo da Última Ofensiva (superfície bombardeada), fazenda-modelo da Linha Robusta (berço do Reino do Lodo), poços da Primeira Escuta (Silêncio), cicatriz do Projeto Jardim (Hélice), pátio de recall (Forja), laboratório da colmeia sem recall, plataforma da Evacuação (Império/Coro), guarnição esquecida e refinaria em greve (Sindicato). Saem ~10% dos marcos do mundo. Não feito: tradução dos 59 marcos do VEE (seguem em inglês), marcos como fonte de dados antigos (etapa 3), efeito de jogo por facção. Próximo: O.

**Andamento (2026-11-17, bloco M):** feito — blindado (veículo de combate) montado sem o cargo cavaleiro da garantia da Forja é pirataria (Forja -10); títulos de posse que viram veículo ao usar: Feira vende lata-velha e seminovo (usados: peças a 35–95%, tanque baixo), Forja vende blindado registrado (só com o cargo). Descoberta: o Vehicle Framework deixa desligados na versão oficial os veículos em ataques de NPC e o comércio de veículos, então "caravanas motorizadas" de NPC e blindados nos ataques da Forja ficam impossíveis por enquanto. Não feito: veículos como tecnologia licenciada (etapa 3). Próximos: N, O.

**Andamento (2026-11-17, bloco L):** feito — bichos de lixão (VAE Waste Animals) nos ataques do Sindicato ("piquete com bichos") e do Reino do Lodo ("estouro do chiqueiro") e nas caravanas do Reino; o Sindicato carrega lixo e pacotes do Vanilla Recycling (vira saque); traços do Vanilla Traits nos líderes das potências e nos cargos altos do Sindicato; a Feira compra lixo. Os biomas poluídos já recebem os bichos de lixão pelo próprio mod (`pollutionWildAnimals`). Não feito: sucata como fonte de tecnologia (etapa 3), bases do Sindicato com prédios de reciclagem. Próximos: M, N, O.

**Andamento (2026-11-17, bloco I):** feito — sanidade do Insanity como custo do Silêncio: pesquisar como membro cobra sanidade (6%/dia a 100% de velocidade) e quem perdeu sanidade pesquisa mais rápido (até +60%); a Consulta ao Arquivo cobra 15% do colono mais estudado; serviço novo "retiro de silêncio" devolve 50% ao mais abalado e deixa-o sem voz por 5 dias. Não feito: "corrupção" que fortalece rituais (DESIGN, ideias futuras), efeito na pesquisa de anomalia (o mod já mexe nela). Próximos: L, M, N, O.

**Andamento (2026-11-17, bloco F):** feito — missões com potência patrona (Ancients → Silêncio; Drone Factory e The Generator → Forja: Fama e favor), pirataria de drones sem licença da Forja, e "Saídos do abrigo" começando no abrigo selado do Ancients com peças de gerador ARC. Não feito: dados antigos como fonte de tecnologia (etapa 3), quem pede as missões (continua "qualquer líder de facção" dos mods), tradução dos textos das missões. Próximos: I, L, M, N, O.

**Andamento (2026-11-16, bloco E):** feito — recall de colmeia (Fama/favor na Hélice) e núcleos de colmeia, quitina e geleia no comércio da Hélice. Não feito: missão de recolhimento própria e genes do VRE Insector como mercadoria. Próximos: F, I, L, M, N, O.

**Andamento (2026-11-16, bloco D):** feito — arsenal medieval dos Não Catalogados e vinho/torneio no Coro. O VFE Medieval 2 não tem torneio próprio na 1.6 (o torneio é serviço nosso). Não feito: tribos de Vagantes (não existem tribos no mundo hoje), tradução do VFE Medieval 2. Próximos: E, F, I, L, M, N, O.

**Andamento (2026-11-16, bloco K):** feitas as bases das 4 potências (plano do CBF por potência, prédios do CBF, guarnição nossa). Fica para a etapa 3: desbloqueio de móveis e energia por tecnologia licenciada; peças desenhadas por nós no editor do CBF. Blocos da etapa 2 que faltam: D, E, F, I, L, M, N, O.

**Andamento (2026-11-16, bloco B):** feitas as permissões por cargo das 4 potências (25, sistema da Royalty). Fica para depois: contrabando e tramas dos Desertores (fase 2 dos Fora de Linha). Próximo sugerido: **K**.

**Andamento (2026-11-16, bloco H):** feito — licença de fabricação (cargo técnico licenciado da Forja; sem ele, pirataria com Infâmia) e androide desperto na colônia (Infâmia na Forja, boa vontade nos Fora de Linha). Fica para depois: missão de recall do desperto. Próximos sugeridos: **B**, **K**.

**Andamento (2026-11-16, bloco G):** feito — cada cargo do Coro licencia um caminho do VPE (trava, cerimônia dá de graça, perda revoga) e o Maestro Supremo usa o Hemosage. Próximos sugeridos: **H**, **B**, **K**.

**Andamento (2026-11-16):** feitos também **roupas por facção** e **comércio por facção** (blocos A e J, e parte de F, H, M: avental do VQE Ancients, peças de androide e de veículo à venda na Forja). Próximos sugeridos: **G** (psycasts do Coro por cargo), **H** (regras da questão androide), **B** (permissões por cargo), **K** (bases por potência).

**Andamento (2026-11-15):** blocos **A** (arsenal) e **C** (armadura-caixão na Forja) feitos; o bloco B já teve as armas nobres do VFE Empire dadas ao Coro. Falta em A: roupas/armaduras e estoque de comércio por facção. Próximos sugeridos: **J+A (estoque de comércio e roupas)**, depois **G** (psycasts do Coro), **H** (androides) e **B** (permissões por cargo).

Ordem sugerida dos blocos: **A → C → B → G → H** (dão identidade às potências) e depois o resto.

## Etapa 3 — Sistema de tecnologia próprio (grande)

Ideia central: num mundo pós-apocalíptico ninguém "inventa" do zero; **recupera, copia ou recebe licença**.

**Decidido com o usuário (2026-11-17):**
- **Teto da bancada: industrial básico.** Neolítico, medieval e o industrial comum (eletricidade, usinagem, microeletrônica, armas de fogo, hidroponia etc.) se pesquisam normalmente. O resto exige um dos caminhos abaixo.
- **Árvore de pesquisa própria do mod:** abas nossas no lugar das abas soltas de cada mod (Bancada, Velho Mundo, Silêncio, Hélice, Forja, Coro), com posições geradas por script a partir dos pré-requisitos.
- **Engenharia reversa dá % do projeto** (ex.: 15–30% por item, conforme o valor); vários itens para liberar. Usa Vanilla Recycling.
- **Licença por cargo ou compra com favor:** o cargo licencia de graça certos projetos (como os psycasts do Coro); quem não tem o cargo pode comprar caro com favor.
- **Licença perdida vira pirataria:** o projeto continua conhecido; fabricar sem licença dá Infâmia (como drones e androides da Forja).
- **Classificação proposta dos 279 projetos:** `tecnico/pesquisa-classificacao.md` (gerada por `scripts/research-classify.py`; lista crua por `tests/research.txt` → `tests/out/research.tsv`). Bancada 119, Velho Mundo 21 (sem dono: só sucata/dados), Silêncio 52 (Anomaly, sensores, arquivo), Hélice 35 (genética, EPOE, implantes), Forja 45 (mecanoides, veículos de combate, armaduras, armas de energia, nave), Coro 7 (eltex, itens nobres; os psycasts já são licenciados por cargo). Aprovada pelo usuário em 2026-11-17.

**Blocos:**
- **3a — Trava e árvore: feito (2026-11-17).** 6 abas próprias (a Anomaly fica na dela), trava fora da bancada com motivo (vanilla e YART), pesquisa atual sem acesso para sozinha. Tech Advancing só mexe no custo; YART e Clean Research Sort refazem o layout e respeitam a trava.
- **3b — Licenças: feito (2026-11-17).** Cargo mínimo por nível tecnológico (medieval 1º cargo, industrial 2º, Anomaly 3º, espacial 4º, ultra 6º; exceções no XML); membros compram licenças acima do cargo com favor (botão "Licenças" na aba Reputação; vale enquanto for membro); licença dá acesso e **dispensa techprints**; fabricar/construir com projeto de potência sem licença = pirataria (boa vontade -4, 1×/dia por potência). Não-membros só chegam às patentes por sucata/dados (3c/3d). **(decidir)** potências venderem licença a não-membros (prata + Fama alta)?
- **3c — Engenharia reversa: feito (2026-11-17).** Pesquisa e bancada próprias (depois da reciclagem básica do VRE); desmontar item ou prédio minificado dá 15–30% do custo do projeto que o fabrica (valor e vida do item; material, comestíveis e itens < 25 de prata não ensinam); prefere o projeto travado; sem os pré-requisitos para em 99% e libera o resto na bancada; patente de potência sem licença dá Infâmia +2. **(decidir)** devolver parte do material ao desmontar?
- **3d — Dados antigos: feito (2026-11-17).** Manual técnico (até industrial, 40%) e disco do Velho Mundo (espacial+, 50%), cada um ligado a um projeto fora da bancada sorteado ao nascer (Velho Mundo mais provável); lido por um colono, libera o projeto e rende a fração (teto de 99% sem pré-requisitos); patente sem licença dá Infâmia. Aparecem em abrigos do VQE Ancients (90%, 1–3), ruínas vanilla fora da colônia (40%, 1–2), nos 10 marcos nossos (60%, 1–2) e na Feira (vende e compra). **(decidir)** dados de patente vendidos pela própria potência? esquemas vanilla liberarem projeto travado?
- **3f — Pesquisa por manual (ideia do usuário de 2026-11-18; a fazer).** Muda a base da etapa 3.
  - **Lore:** o mundo já tinha toda a tecnologia; a Queda a perdeu. O jogador não inventa, **reaprende**.
  - **Regra:** **todas** as pesquisas começam trancadas, inclusive as da bancada (hoje as 119 da bancada se pesquisam livremente). Para destrancar uma pesquisa, é preciso o **manual técnico dela**. Com o manual, o projeto fica disponível e se pesquisa normalmente na bancada (o manual destranca, não dá %).
  - **Como conseguir manuais:**
    - **Comércio:** outras facções vendem (a Feira principalmente; cada potência vende os do ramo dela). É o caminho principal das **pesquisas básicas**.
    - **Exploração:** ruínas, abrigos do Ancients, marcos nossos, zonas da Queda (o que a 3d já faz, agora com o manual destrancando em vez de dar %).
    - **Missão dirigida:** "o manual de X está em tal lugar" (sítio no mapa-mundi com o manual garantido).
    - **Missões especiais das potências:** caminho das tecnologias **mais avançadas e específicas** (espacial/ultra, patentes das potências); recompensa pelo cargo ou pelas obrigações (Etapa 11).
    - **Engenharia reversa:** vale principalmente para **equipamento**. Destruir um item ensina a **fabricar aquele item** (receita liberada sozinha), **não o projeto inteiro**. O projeto só destranca quando o jogador tiver feito engenharia reversa de uma **porcentagem dos itens ligados a ele**.
  - **O que muda no que já existe:** a 3a (trava fora da bancada) vira trava de tudo; a 3c deixa de dar 15–30% e passa a liberar receita por item + contagem por projeto; a 3d deixa de dar 40–50% e passa a destrancar; o disco do Velho Mundo vira o manual das tecnologias espaciais/ultra (ou some, a decidir). As licenças (3b) continuam como permissão legal (pirataria); falta decidir se a licença também entrega o manual.
  - **Decidido pelo usuário (2026-11-18):**
    1. Início do jogo: vêm destrancadas **as pesquisas do cenário** (cada cenário define a sua lista).
    2. Engenharia reversa: **50% dos itens** ligados ao projeto destrancam o projeto inteiro; projeto **sem item** só se destranca por manual.
    3. O manual **não é consumido**: fica com o jogador e pode ser vendido depois.
    4. Licença de cargo dá **só a permissão legal**, não o manual.
    5. O manual destranca a pesquisa **mesmo sem os pré-requisitos** (ela espera a cadeia para ser pesquisada).
    6. **A árvore de pesquisa atual (abas da 3a) vai ser desfeita e remontada** de acordo com o modpack; o desenho da nova árvore **ainda não foi planejado ✱** e vem antes (ou junto) do primeiro bloco do 3f. A classificação dos projetos por ramo (`tecnico/pesquisa-classificacao.md`) também pode mudar.
  - **Blocos sugeridos:** (1) trava total + manual destrancando + pesquisas iniciais por cenário; (2) engenharia reversa por item com contagem por projeto; (3) fontes: estoque de manuais por facção, missão dirigida e missões especiais das potências.

## Etapa 4 — Utilidade para os outros mods (média)

| Mod | Proposta |
|---|---|
| Alpha Biomes | biomas como regiões com história (zonas da Queda); recursos exclusivos como insumo de tecnologia |
| Rimesis | conferir o que faz no nosso mundo e ligar ao Silêncio ou remover **(decidir)** |
| EPOE Forked | próteses avançadas como licença da Hélice |
| Dubs Bad Hygiene | água como recurso de poder do Sindicato (meme Primazia da Água) e exigência de cargo do Coro |
| Harvest Organs Post Mortem | mercado de órgãos da Feira e da Hélice |
| Trenches, Layered Wall Destruction | bases e cercos das potências |
| Custom Base Framework | plantas de base próprias por potência |
| Custom Ritual Framework, Blessed Conception | rituais das nossas ideologias com resultado próprio |
| SeedsPlease, Smart Farming | sementes como mercadoria da Feira e item de dízimo |
| Geological Landforms, World Map Enhanced | sem ação (apoio) |
| Simple Sidearms | **(proposta, auditoria 2026-11-17)** arma secundária nas PawnKinds de elite |
| VBE Coffees and Teas, Nutrient Paste | **(proposta)** café/chá no estipêndio de uma potência; pasta nutritiva como ração dos abrigos/Fora de Linha |
| VFE Pirates (maldições), Deserters (contrabando) | **(proposta)** maldição como pacto do Coro/Silêncio; categoria de contrabando para itens de patente |
| Tech Advancing | **(proposta)** conferir convivência com as licenças por cargo (3b) |
| Show Me Your Tools | **(proposta)** ferramenta animada para JobDefs novos nossos |
| Hospitality (novo) | **(proposta)** hospedar membros de potência dá Fama; recrutar hóspede de potência é aliciamento (Infâmia); líder e cargos não recrutáveis; conferir a trava de xenos |
| VFE Security (novo) | **(proposta)** classificar as 13 pesquisas `VFES_*` nos ramos (artilharia/railgun/EMP → Forja, cerco medieval → Coro/Não Catalogados); defesas nas bases do CBF e nos cercos; artilharia da Forja em retaliação |
| Vanilla Weapons Expanded (novo) | **(proposta)** antimaterial e armas de carga no arsenal da Forja; arcos compostos e bestas nos Não Catalogados e na Colmeia |
| Adaptive Simple/Deep/Primitive Storage (novos) | **(proposta)** depósitos nas plantas de base do CBF |
| Realistic Rooms Rewritten (novo) | **(proposta)** conferir as exigências de cômodo dos cargos |

Tabela de todos os mods (uso hoje e proposta): `mods/encaixe.md`.

## Etapa 5 — Eventos do mundo (grande)

- Eventos por potência, disparados por reputação, cargo e guerras do Hegemony (ex.: auditoria da Hélice, inspeção de garantia da Forja, turnê do Coro, chamada de artigos do Silêncio).
- **Fase 2 das facções menores:** Exposição dos Fora de Linha contra a Forja, sabotagem, expedições de coleta do Silêncio nos ninhos, greve do Sindicato (trégua temporária), liquidação da Feira.
- **Fungobi** (zumbis do mundo, xenos `EQ_Xeno_Fungobi*` prontos, sem facção): hordas em eventos e moradores de cidades destruídas; a pele rende fio do diabo, então caçar zumbi dá lucro.
- Eventos de mundo sem dono: tempestade de cinzas, abrigo que se abre, comboio perdido, recall mecanoide.
- Reaproveitar os eventos e missões dos mods (VQE, Anomaly, Insectoids, **Vanilla Events Expanded**: 28 eventos `VEE_*`) com texto e gatilho nossos. **(proposta)** batalha espacial/queda de nave → destroços da Queda; eventos psíquicos → Coro; viajante traidor → espião do Sindicato; noite longa/era do gelo → tempestade de cinzas.

## Etapa 6 — Narrador próprio (média)

- Um narrador principal que conhece os nossos sistemas: escolhe ameaça pela reputação com cada potência, usa o Sindicato no começo, e segura os eventos de fim de jogo até o jogador ter cargo ou tecnologia.
- Variações **(decidir)**: clássico (ritmo crescente), calmo (construção) e caótico.
- Nomes e retratos no tom do mod; o retrato precisa de arte (o usuário fornece ou usamos um provisório).

## Etapa 7 — Qualidade de vida no estilo HSK (média)

O que o HSK faz e ainda não temos **(decidir quais valem)**:
- pilhas maiores e receitas em lote;
- aba de trabalho detalhada e prioridades por tarefa (já temos Complex Jobs);
- HUD com necessidades e saúde no retrato do colono;
- tabela de colonos/animais/prisioneiros personalizável;
- luz de parede, mais opções de planejamento e de zonas;
- qualidade mínima na construção, reconstrução automática;
- ferramentas de trabalho com bônus (já temos Show Me Your Tools só visual);
- textos de pesquisa e de bancada organizados por categoria (ligado à etapa 3).

Caminho: preferir mods prontos compatíveis com 1.7 e CE; o que não existir, fazer por patch no nosso mod.

## Etapa 10 — Estratégia, desempenho e mundo fixo (grande; lista do usuário de 2026-11-17)

Origem: `Desktop/Coisas que o modpack precisa t er.txt`. Regra do projeto: não editar o Hegemony nem o Seamless World; tudo por API pública, Harmony e Defs nossos (módulo `Mods/Hegemony` do Ecos da Queda ou mod novo).

- **10a — Medição (1 bloco, primeiro).** _Ferramenta feita (2026-11-18): `perf start`/`perf report` e `tests/perf.txt`; linha de base em `tecnico/desempenho.md`. Falta medir jogo longo com guerras do Hegemony._ Comando `perf` no executor: ticks por segundo e custo por sistema num jogo longo (perfil full, 15–30 dias), com e sem o Hegemony, e quanto custam os tiques do mundo (expedições, territórios, caravanas). Resultado em `tecnico/desempenho.md`; ele decide o que entra no 10c e no 10d. Ferramenta de apoio: Dubs Performance Analyzer (com janela, pelo usuário).
- **10b — Mundo fixo + fronteiras vivas (redesenhado em 2026-11-19: ver `MUNDO.md`).** América do Norte num mundo 25% (Worldbuilder + The Earth), posições iniciais fixas, fronteiras que crescem e geram cidades novas, bônus por tile, controle do jogador, exércitos tomando fronteira; blocos M1–M3 e F1–F6. Texto antigo abaixo.
- **10b (texto antigo) — Mundo fixo com a história (1–2 blocos).** Sempre o mesmo planeta: semente, tamanho, cobertura e relevo fixos; capitais das potências, terras das facções menores e os 10 marcos nossos sempre nas mesmas regiões; variam só detalhes (nomes de líderes, bases menores, alguns marcos). Cenários próprios apontam para esse mundo. Facilita a história, o Hegemony (fronteiras previsíveis) e o Seamless World. **(decidir)** quanto varia entre partidas; mapa desenhado à mão ou semente escolhida.
- **10c — Mod de desempenho (1–2 blocos).** Mod novo (`mod/ecos-desempenho`, worktree em `Mods/EcosDesempenho`): regras simplificadas conforme o 10a (ex.: limite de pawns do mundo, tiques mais espaçados de sistemas caros, limpeza de corpos e sujeira longe da colônia). Cada mudança com o custo de imersão listado para o usuário aprovar ✱.
- **10d — Hegemony aperfeiçoado (2–3 blocos).** _1ª parte feita (2026-11-19, `Forces.cs`): exércitos das potências como objetos do mundo (força, moral, tesouro e manutenção, guarnições abstratas), mobilização pelas guerras do Hegemony, batalha abstrata com conquista por `TryTransferSettlement`, chamada às armas do vassalo. Batalha jogada feita no 10e. Resto feito (2026-11-19, `WarDiplomacy.cs`): tensão entre potências → incidente → conselho de guerra do vassalo → guerra declarada pelo Hegemony; frentes de fronteira entre territórios em guerra. Falta só a otimização do Hegemony (depende do 10a com jogo longo)._ Otimizar o que o 10a apontar (por Harmony, sem editar o mod); mecânicas novas pela API pública (API 4/5: leitura do estado, agendamento de invasões, forças enviadas): **alocação de tropas** (exércitos e guarnições como objetos do mundo, com custo e manutenção); cada exército pode ser resolvido de forma abstrata no mapa-mundi ou levado a uma batalha real, simulada ou jogada (decisão do usuário de 2026-11-18; o jogador comanda forças da potência nos cargos altos, Etapa 11), frentes de guerra entre territórios, papel do jogador como vassalo (enviar tropas, receber pedidos da potência).
- **10e — Batalhas de trincheira (1–2 blocos).** _1º bloco feito (2026-11-19, `Fronts.cs`): frente de trincheira quando um exército nosso chega ao alvo (comandado no modo jogado, ou da patrona por sorte); mapa com trincheiras, dois lados, onda e avanço; resultado vale como a batalha do exército. 2º bloco feito (2026-11-19): frentes também nas invasões do próprio Hegemony entre NPCs e escolha de lado na entrada (Fama/boa vontade do apoiado, Infâmia e hostilidade do outro). Falta: balanço (jogando); a escaramuça das frentes do Hegemony não muda o resultado da invasão (dá para ligar por `TryContributeExternalForce` mais tarde, se o usuário quiser)._ Quando territórios de duas nações em guerra se encostam, abre-se uma **frente de trincheira**: sítio no mapa-mundi com mapa gerado de trincheiras (mod Trenches) e as duas facções lutando em ondas; o jogador pode apoiar um lado (Fama e favor), ficar de fora ou saquear os mortos; o resultado empurra a fronteira no Hegemony. Usa o 10d (tropas) e a 5a (eventos de guerra).
- **10f — Seamless World (objetivo final; pesquisa + blocos).** Hoje desativado por bug ✱ (o usuário descreve o bug). Depois do 10a–10c: religar no perfil de teste, medir, listar o que quebra (nossos sistemas de mapa: bases, marcos, dados antigos, cerimônia) e o que é viável; decidir com o usuário.

## Etapa 11 — Cargos com obrigações reais (grande; pedido do usuário de 2026-11-18)

Hoje o cargo só dá coisas (permissões, licenças, serviços, caminhos do VPE) e cobra pouco (dízimo). A ideia é o cargo **pesar**: quanto mais alto, mais a potência exige, com prazo, e falhar custa (favor, Fama; no máximo, rebaixamento). Cada obrigação tem a cara da potência, e cumprir bem dá favor e acelera a promoção.

- **Escada de obrigações por nível.** Cargos 1–2: tarefas pequenas (entregas, abrigar um enviado, emprestar um colono por alguns dias). Cargos 3–4: **gerenciar coisas da potência** — um posto avançado/entreposto perto da colônia (Outposts/Hegemony), a cota de produção da Forja, o dízimo de uma região, a fiscalização de piratas de tecnologia. Cargos 5+: **comandar** — liderar um exército da potência numa guerra do Hegemony, defender uma base aliada sitiada, decidir o destino de prisioneiros de guerra, votar/arbitrar disputas internas (sucessão do líder da potência).
- **Formato.** Cada obrigação é uma carta-missão (reaproveita `PowerEventDef`/missões do bloco F) com prazo, custo de não cumprir e recompensa; o executor ganha comandos para forçar e conferir. Recusar ou falhar várias vezes = rebaixamento de um cargo (perde o que ele licencia). Promoção recusável: a cerimônia de promoção ganha a opção "recusar".
- **Por potência (rascunho, decidir ✱):** Silêncio — conduzir uma expedição de coleta aos ninhos, guardar uma contenção, "arquivar" (silenciar) uma testemunha; Hélice — gerir uma filial (meta de lucro), cobrar devedores, testar um produto nos próprios colonos; Forja — cota de componentes/mechs, comandar o recall de um assentamento pirata, manter a guarnição de uma fábrica; Coro — organizar o torneio/festival da corte, comandar a guarda numa guerra, escolher quem vai para o "treinamento vocal".
- **Exército na guerra (cargos altos).** Depende do 10d: tropas como objetos do mundo, alocadas de forma abstrata ou levadas a uma batalha real (simulada ou jogada). O jogador recebe o comando de uma força da potência (tamanho pelo cargo), escolhe alvo/postura no mapa-mundi, paga a manutenção ou perde moral; vitória empurra a fronteira e dá muito favor; derrota pesa no cargo. Pode virar batalha jogável (10e, trincheiras) se o jogador for junto.
- **Feito (2026-11-19): 11c** — comando de exército do 5º cargo em diante (`Forces.cs`: oferta, alvo, postura, soldo, recuo; vitória apaga falta, derrota/deserção/recusa contam falta). Falta: levar o exército a uma batalha jogada (10e).
- **Feito (2026-11-19): 11d** — recusar promoções (botão na aba Reputação; fica no cargo e nas obrigações, favor continua acumulando) e encargo cumprido apaga uma falta. Textos revisados junto.
- **Feito (2026-11-18): 11b** — encargos (um por potência, do 3º cargo em diante): metas por quinzena, relatório, renúncia, faltas (`Encargos.cs`, `EQ_Encargos.xml`). Ficou de fora: posto avançado como objeto no mapa-mundi (pode vir com o 10d) e cobrar devedores/fiscalizar piratas.
- **Feito (2026-11-18): 11a** — 8 obrigações (2 por potência: cargos 1–2 e 3–4), colono emprestado, faltas e rebaixamento (`PowerEvents.cs`, `EQ_Obligations.xml`).
- **Blocos:** 11a escada de obrigações simples dos cargos 1–4 com o que já existe (cartas, entregas, serviços, emprestar colono); 11b administrar algo da potência (posto avançado/cota) com estado salvo e relatório periódico; 11c comandar exército (com 10d); 11d rebaixamento, recusa de promoção, textos e balanceamento.
- **Decidido pelo usuário (2026-11-18):**
  - **Punição máxima = rebaixamento de cargo** (nunca expulsão por falhar obrigação). Antes disso: perda de favor e Fama.
  - **O jogador pode recusar a promoção** (fica no cargo atual e nas obrigações dele).
  - **Exército nos dois modos:** alocar de forma abstrata pelo mundo (mapa-mundi, resolução simulada) **e** usar numa batalha real, simulada ou jogada (o jogador escolhe; ligado ao 10e). Vem junto com a melhoria do sistema de guerra e política (10d).

## Etapa 8 — Pendências antigas

- Erro do Warbolter ao gerar pawn fora do mapa.
- Erro intermitente ao largar arma do Combat Extended (`tried to drop CE_Gun_… which it didn't contain`).
- Facção rebelde do Hegemony com o FactionDef da Hélice.
- Xeno do líder da Forja (usuário vai criar).
- Ícones próprios das facções menores e das ideologias.
- Facial Animation e RimExodus Seamless World desativados por bug.
- Tradução dos ~50 mods sem PT-BR.
- Conferência visual acumulada (ver STATUS.md).
