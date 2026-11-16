# Plano de desenvolvimento em etapas

_Criado em 2026-11-15 a partir da lista do usuário. Cada etapa termina com teste no executor e atualização do STATUS.md. A ordem pode mudar; o que está marcado **(decidir)** depende de resposta do usuário._

## Resumo

| Etapa | O que entrega | Tamanho | Depende de |
|---|---|---|---|
| 0 | Ideologias da Feira e do Sindicato; vassalagem sem nível psíquico indevido | feito | — |
| 1 | Travas: só os nossos xenos na criação, cenários próprios, narradores de fora ocultos | **feito** | — |
| 2 | Utilidade para os mods Vanilla Expanded de conteúdo | grande | 1 (auditoria) |
| 3 | Sistema de tecnologia próprio (sucata, dados antigos, licenças das potências) | grande | 2 (saber o que cada potência "possui") |
| 4 | Utilidade para os outros mods | médio | 2, 3 |
| 5 | Eventos do mundo + missões das facções menores | grande | 2, 3 |
| 6 | Narrador próprio | médio | 5 (ele agenda os eventos) |
| 7 | Qualidade de vida no estilo HSK | médio | independente |
| 8 | Pendências antigas | pequeno | independente |

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

**Andamento (2026-11-16, bloco H):** feito — licença de fabricação (cargo técnico licenciado da Forja; sem ele, pirataria com Infâmia) e androide desperto na colônia (Infâmia na Forja, boa vontade nos Fora de Linha). Fica para depois: missão de recall do desperto. Próximos sugeridos: **B**, **K**.

**Andamento (2026-11-16, bloco G):** feito — cada cargo do Coro licencia um caminho do VPE (trava, cerimônia dá de graça, perda revoga) e o Maestro Supremo usa o Hemosage. Próximos sugeridos: **H**, **B**, **K**.

**Andamento (2026-11-16):** feitos também **roupas por facção** e **comércio por facção** (blocos A e J, e parte de F, H, M: avental do VQE Ancients, peças de androide e de veículo à venda na Forja). Próximos sugeridos: **G** (psycasts do Coro por cargo), **H** (regras da questão androide), **B** (permissões por cargo), **K** (bases por potência).

**Andamento (2026-11-15):** blocos **A** (arsenal) e **C** (armadura-caixão na Forja) feitos; o bloco B já teve as armas nobres do VFE Empire dadas ao Coro. Falta em A: roupas/armaduras e estoque de comércio por facção. Próximos sugeridos: **J+A (estoque de comércio e roupas)**, depois **G** (psycasts do Coro), **H** (androides) e **B** (permissões por cargo).

Ordem sugerida dos blocos: **A → C → B → G → H** (dão identidade às potências) e depois o resto.

## Etapa 3 — Sistema de tecnologia próprio (grande)

Ideia central: num mundo pós-apocalíptico ninguém "inventa" do zero; **recupera, copia ou recebe licença**. Proposta **(decidir os números e o que fica em cada potência)**:

1. **Pesquisa de bancada limitada:** vai sozinha só até um teto (neolítico/medieval e o básico industrial). Daí em diante a pesquisa exige um dos três caminhos abaixo.
2. **Sucata e engenharia reversa:** desmontar um item numa bancada própria dá progresso no projeto que o fabrica (usa Vanilla Recycling).
3. **Dados antigos:** discos e manuais achados em abrigos (VQE Ancients), ruínas, marcos e no estoque da Feira; cada um libera ou acelera um projeto.
4. **Licenças das potências:** cada potência tem um ramo exclusivo, liberado por cargo (favor) e revogado se você sai ou vira inimigo:
   - **Silêncio:** Anomaly, bioferrite, pesquisa do Vazio.
   - **Hélice:** genética, xenogermes, próteses avançadas (EPOE), o Soro.
   - **Forja:** mecanoides, veículos, armaduras-caixão, armas pesadas, androides.
   - **Coro:** psycasts, eltex, itens nobres.
   Roubar tecnologia (atacar comboio, engenharia reversa de item licenciado) é possível e dá Infâmia.
5. Tech Advancing e YART já estão no modpack: conferir se ajudam ou atrapalham (o avanço automático de nível tecnológico pode quebrar o teto).

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

## Etapa 5 — Eventos do mundo (grande)

- Eventos por potência, disparados por reputação, cargo e guerras do Hegemony (ex.: auditoria da Hélice, inspeção de garantia da Forja, turnê do Coro, chamada de artigos do Silêncio).
- **Fase 2 das facções menores:** Exposição dos Fora de Linha contra a Forja, sabotagem, expedições de coleta do Silêncio nos ninhos, greve do Sindicato (trégua temporária), liquidação da Feira.
- Eventos de mundo sem dono: tempestade de cinzas, abrigo que se abre, comboio perdido, recall mecanoide.
- Reaproveitar os eventos e missões dos mods (VQE, Anomaly, Insectoids) com texto e gatilho nossos.

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

## Etapa 8 — Pendências antigas

- Erro do Warbolter ao gerar pawn fora do mapa.
- Erro intermitente ao largar arma do Combat Extended (`tried to drop CE_Gun_… which it didn't contain`).
- Perfil lean do executor de testes quebrado pelas ideologias dos `.rid` (ampliar a lista de mods dele).
- Facção rebelde do Hegemony com o FactionDef da Hélice.
- Xeno do líder da Forja (usuário vai criar).
- Ícones próprios das facções menores e das ideologias.
- Facial Animation e RimExodus Seamless World desativados por bug.
- Tradução dos ~50 mods sem PT-BR.
- Conferência visual acumulada (ver STATUS.md).
