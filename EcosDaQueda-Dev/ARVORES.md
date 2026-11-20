# Árvores de pesquisa: plano para cobrir o modpack

Base da etapa 3f, item 6 do PLANO: desfazer a árvore atual e remontá-la de acordo com o modpack.
Levantamento de 2026-11-19, feito sobre o catálogo exportado do jogo (`kits/kitcatalog.json`).

- **Conteúdo:** 3.837 coisas liberáveis, sem contar as sementes. São construções, itens, plantas, pisos e receitas/cirurgias.
- **Pesquisas originais:** 348, com as 61 da Fazenda.
- **Mapa completo:** `kits/arvores-mapa.tsv` tem uma linha por coisa: árvore proposta, tipo, def, rótulo, mod, categoria e pesquisa atual.
- **Como refazer o mapa:** rode `kits/kits.py catalog` e `python kits/tool/arvores_mapa.py --write`.

## Resumo

Legenda da tabela:
- **Sem pesq.:** quantas coisas não exigem pesquisa nenhuma hoje.
- **Ramo de hoje:** em que ramos ficam as pesquisas originais dessa árvore. O número é a quantidade de pesquisas em cada ramo.

| # | Árvore | Itens | Sem pesq. | Ramo de hoje | Situação |
|---|---|---|---|---|---|
| 1 | **Fazenda** | 278 | 0 | Bancada | **feita** (61 pesquisas; textos na lore e ligada à Produção em 2026-11-20) |
| 2 | Construção (paredes, portas, pisos) | 557 | 80 | Bancada | **feita** (17 pesquisas, 519 coisas) |
| 3 | Mobília e lazer | 179 | 41 | Bancada (Coro 4) | **feita** (15 pesquisas, 14 absorvidas) |
| 4 | Produção e indústria | 79 | 19 | Bancada (Silêncio 6, Forja 4) | **feita** (16 pesquisas, 15 absorvidas) |
| 5 | Logística (armazenamento, canos, reciclagem) | 92 | 11 | Bancada | **feita** (6 pesquisas) |
| 6 | Energia | 45 | 16 | Bancada (Velho Mundo 2) | **feita** (12 pesquisas, 11 absorvidas) |
| 7 | Água, clima e higiene (DBH + temperatura) | 83 | 10 | Bancada (Velho Mundo 3) | **feita** (16 pesquisas, 22 absorvidas) |
| 8 | Cozinha e bebidas | 109 | 38 | Bancada | **feita** (16 pesquisas, 15 absorvidas) |
| 9 | Vestuário | 108 | 29 | Bancada (Forja 7) | **feita** (7 pesquisas, 5 absorvidas) |
| 10 | Medicina e farmácia (próteses, implantes, cirurgias, drogas) | 507 | 332 | **Hélice 23**, Bancada 13 | a fazer |
| 11 | Armas e munição | 1.099 | 380 | Bancada 16, Velho Mundo 8, Silêncio 5, Forja 4 | **feita** (46 pesquisas, 372 coisas, 20 absorvidas) |
| 12 | Armaduras | 40 | 3 | Bancada 6, Forja 4 | **feita** (16 pesquisas, 60 coisas, 10 absorvidas) |
| 13 | Defesa (torretas, armadilhas, trincheiras) | 115 | 8 | Bancada 18, **Forja 13**, Velho Mundo 9 | **feita** (26 pesquisas, 169 coisas, 22 absorvidas) |
| 14 | Veículos | 40 | 0 | Bancada 6, **Forja 4** | licença da Forja |
| 15 | Mecanoides e drones | 62 | 7 | **Forja 7** | licença da Forja |
| 16 | Androides | 41 | 1 | **Forja 2** | licença da Forja |
| 17 | Genética (genes, sanguófagos, Alpha Genes) | 45 | 2 | Bancada 7, **Hélice 5** | licença da Hélice |
| 18 | Insetoides (colmeias VFE) | 36 | 0 | Bancada 2, **Hélice 2** | licença da Hélice |
| 19 | Vazio (Anomaly) | 45 | 2 | **Silêncio 27** | licença do Silêncio |
| 20 | Psíquica (eltex, conjurador) | 11 | 0 | Bancada 2, **Coro 1** | licença do Coro |
| 21 | Espaço e comunicação (Odyssey, nave, cápsulas, consoles) | 44 | 4 | **Forja 9**, Velho Mundo 4 | Velho Mundo / Forja |
| 22 | Ideologia (Alpha Memes, VIE, Ideology) | 222 | 55 | Bancada | a fazer |

Total: 3.837 coisas em 22 árvores, das quais **1.006 hoje não exigem pesquisa nenhuma**.

## Início da partida: o que já vem liberado (levantamento de 2026-11-19)

Comandos usados: `inicio` (roteiro `tests/inicio-pesquisa.txt`, grava `tests/out/inicio.tsv`) e o mapa `kits/arvores-mapa.tsv`.

**Hoje os 6 cenários começam iguais.**
- Todos têm 14 pesquisas já concluídas. Elas vêm da facção do jogador (`PlayerColony`, tags de início clássico).
- As 14: Eletricidade, Pasta nutritiva, Condicionamento de ar, Refrigerador primitivo, Lapidação, Mobília complexa, Mobília básica (VFE), Vestuário complexo e 6 do DBH (Sistema de água primitivo, moderno e avançado, Água quente, Água quente avançada, Irrigação).
- Alguns memes dão mais: Escavador e Devoção profunda dão Lapidação; Conexão com árvores dá Plantio de árvores; Alimentação de sangue dá Descanso mortal; Devoção profunda também dá Fio do diabo.

**O que fica liberado no começo, fora da Fazenda: 1.478 coisas.**
- 1.038 não exigem pesquisa nenhuma.
- 440 vêm das 14 pesquisas iniciais.
- Por árvore: Armas e munição 380 (quase tudo munição), Medicina 331 (quase tudo cirurgias), Ideologia 204, Construção 143, Mobília 94, Logística 65, Vestuário 62, Cozinha 45, Água 40, Produção 35, Energia 26, Clima 13, e o resto com menos de 10.
- Algumas coisas já ficam presas por outro caminho, sem pesquisa própria: drones (precisam da bancada de drones), ARCs (vêm da missão do gerador), munição (precisa da arma), aço e plastaço (precisam de bancada).

**Aplicado em 2026-11-19 (proposta aprovada):** `Patches/EQ_InicioSemPesquisa.xml` tira as tags de início da `PlayerColony` (0 pesquisas concluídas; os memes ainda dão as deles). O kit livre está em `kits/pesquisa/Fundamentos.json` (campo `kit`, 36 defs). As outras 161 coisas livres foram para a árvore provisória **Fundamentos** (13 pesquisas baratas: Carpintaria, Alvenaria, Pisos, Mobília, Arte, Armazenamento, Higiene, Cozinha, Roupas, Fumo, Armas, Fortificação, Alcionita). O comando `inicio` confere tudo (`concluídas agora: 0`, `kit livre 36/36`, `Fundamentos travados 161`). Em 2026-11-20 o açougue (`ButcherSpot`) entrou no kit (37) e o nó Fumo saiu de Fundamentos (baseado e psycho foram para Drogas I e II da Fazenda). Ficaram de fora porque já têm outra trava: munição (precisa da arma), cirurgias (precisam do implante), itens de ideologia (precisam do preceito), drones e ARCs, e recursos feitos em bancada.

**Proposta original:**
1. **Zerar as pesquisas iniciais.** Nenhum cenário começa com Eletricidade, água encanada etc. Opcional: um toque por cenário, como o Abrigo sabendo Eletricidade ou a Forja sabendo Metalurgia.
2. **Kit livre mínimo**, cerca de 40 coisas. Todo o resto vai para o nível I de alguma árvore, com custo baixo.
   - Dormir: local para dormir (simples, duplo, de bebê), colchonete.
   - Fogo e luz: fogueira, tocha, tocha de parede.
   - Trabalho: ponto de fabricação e **bancada de pesquisa simples** (sem ela não se pesquisa nada).
   - Construção: parede, porta simples, cerca, portão de cerca.
   - Armazenamento: local de pilha de lenha pequeno e depósito de cadáveres. Zonas de estoque não são construções e continuam livres.
   - Social e rituais: túmulo, ponto de festa, de casamento, de caravana, de ritual e de meditação.
   - Água e higiene (DBH): latrina, balde de água, poço primitivo, buraco de resíduo.
   - Comida: refeição simples e comida de bebê.
   - Armas: faca, porrete, arco curto, lanças arremessáveis, pedras de arremesso.
   - Roupas: veste tribal, roupa tribal infantil, parca.
   - Remédio: medicina natural.
3. **Tudo o mais que hoje é livre ganha pesquisa** no nível I da árvore dele (cama, mesa, cadeira, fogão, mesa de alfaiate, armários, roupas industriais, refeições boas etc.). Isso pode ser aplicado agora, numa árvore provisória "Fundamentos", ou árvore por árvore conforme forem montadas.

## Casos que pedem decisão antes de montar

1. **Munição do CE (cerca de 900 itens, 380 sem pesquisa).**
   - Cada calibre tem de 3 a 6 tipos (FMJ, AP, HP, incendiária etc.).
   - Proposta: não pôr munição à mão. A munição comum de cada calibre seria liberada junto com a primeira arma que a usa. Os tipos avançados (AP-HE, incendiária, sabot) iriam para uma pesquisa "Munição avançada" por era.
   - Isso dá para gerar automaticamente: o catálogo já sabe as munições de cada arma.
   - **Decidido (2026-11-20, usuário):** munição comum (FMJ, AP, HP, chumbo, balote…) vem com a primeira arma do calibre, mesmo que uma arma de nível menor libere munição de uma maior; a especial (incendiária, HE dos calibres de fuzil, sabot, flechette, PEM, iônica…) tem pesquisa própria por era. Aplicado na árvore Armas (abaixo).
2. **Pisos (489, quase todos tapetes e ladrilhos em várias cores).**
   - Proposta: uma pesquisa por material ou estilo, e não por cor. Por exemplo: madeira, pedra polida, tapete, tapete fino, metal e estéril.
3. **Cirurgias (367, 330 sem pesquisa).**
   - A maioria é "instalar X", que já depende de fabricar o implante.
   - Proposta: pôr na árvore só o item (a prótese ou o implante) e deixar a cirurgia livre. As exceções são cirurgias sem item, como lobotomia e cirurgia cerebral.
4. **Coisas sem pesquisa hoje (1.006).**
   - Pela regra da Fazenda, "tudo na árvore", o começo da partida fica mais pesado.
   - Decidir árvore por árvore o que continua livre. Por exemplo: ponto de dormir, fogueira, mesa de corte, roupas tribais, arcos.
5. **Ramos com licença.**
   - Quando a pesquisa original é de ramo de facção, a pesquisa nova herda o ramo. Foi o que fizemos com os ecossistemas da Fazenda, que continuam no ramo Velho Mundo.
   - Pesquisa do ramo livre (Bancada) não pode ter pré-requisito em ramo com licença. O teste `pesquisa` acusa isso.
6. **Árvores grandes.**
   - Armas, Construção e Medicina podem virar várias abas, por era ou por tipo. Ou uma aba só com blocos, como a Fazenda.
7. **Pesquisas originais sem desbloqueio (24).**
   - As da Fazenda já somem sozinhas.
   - Sobram projetos que só servem de pré-requisito: Cirurgia cerebral, Arquogenética, Bioregeneração, Pesca, Autópsias e a Engenharia reversa do mod.
   - Elas entram como nós de "conhecimento" ou somem.

## Por árvore (o que entra e o que absorve)

Para cada árvore: as pesquisas originais que ela absorve (projetos cuja maior parte do conteúdo cai nela), as categorias principais e observações. A lista completa está em `kits/arvores-mapa.tsv`.

**2. Construção (557).**
- Categorias: pisos (498) e estrutura (59).
- Mods principais: VFE Architect, Ideology, Alpha Biomes e VFE Medieval 2. O mod "?" (389) são as variantes de cor de piso geradas.
- Absorve: Portas automáticas, Fabricação de carpete, Arquitetura medieval, Heráldica e parte de Metalurgia.
- **Feita em 2026-11-20** (`kits/pesquisa/Construcao.json`), "reconstruir o que a guerra derrubou"; caso 2 decidido: **pisos por material, as cores vêm juntas**.
  - **Estrutura:** Carpintaria I, Alvenaria rústica I, Pedra lavrada II (cerca de tijolos, pontes de pedra), Ferro e grade II (cela, alambrado, reforçados do CE), Portas automáticas III, Blindagem IV (Velho Mundo).
  - **Pisos:** Rústicos I, Pedra bruta I, Ladrilho de pedra II, Tapete II (257 cores), Ladrilho fino III, Tapete fino III (63), Concreto e asfalto III, Ladrilho metálico IV, Materiais estéreis IV.
  - **Arquitetura:** Arquitetura medieval II (castelo, enxaimel, esteiras de couro), Heráldica III.
  - Absorve: Portas automáticas, Fabricação de carpete, Arquitetura medieval, Heráldica, Materiais estéreis. Carpintaria, Alvenaria e Pisos saíram de Fundamentos (8 pesquisas agora).
  - Ficam de fora: pisos e lajes de ideologia (mórbido, espinhoso, totêmico, viking; vão para Ideologia, por ora com a Cantaria), decorações nobres e porta ornamentada (Mobília/Coro), os pisos de prefab do framework e o que está no kit.

**3. Mobília e lazer (179).**
- Mods principais: VFE (mobília básica, moderna e artesanal), Medieval 2 (oficinas complexas), Empire (decorações nobres, órgãos, trono), Door Mat e Alpha Biomes.
- Absorve: Mobília básica, Mobília moderna, Mobília artesanal, Descarte de lixo, Luzes avançadas, TV de tubo, TV de tela plana, Harpa, Cravo, Piano, Decorações nobres, Órgãos de tubos e Trono estélico.
- **Feita em 2026-11-20** (`kits/pesquisa/Mobilia.json`), "um lar no fim do mundo": **Casa** (rústica I, de verdade II com o trono, moderna III com lixeiras, artesanal III com a porta ornamentada), **Luz** (luminárias II, luzes avançadas III), **Lazer** (arte e jogos I, jogos de salão II, instrumentos II, televisão de tubo III, tela plana IV/Velho Mundo) e **Nobreza/Coro** (decorações nobres II, piano de cauda III, órgão IV, trono estélico III).
- Absorve também Mobília complexa (hub: por enquanto traz 108 coisas, quase todas de armazenamento, que vão para a Logística). Mobília e Arte saíram de Fundamentos (6 pesquisas). A Fazenda (Memorial animal) exige a Mobília de verdade nova.
- O trono fica no ramo livre (a fundação da nação precisa dele).

**4. Produção e indústria (79). Feita em 2026-11-20** (`kits/pesquisa/Producao.json`, gerador no histórico).
- História: "do ferro-velho à linha de montagem" — a Queda levou o saber de fabricar; componentes são do Velho Mundo, a linha de montagem e o componente avançado da Forja, escâneres e multi-analisador do Silêncio.
- Blocos e níveis (I medieval, II industrial, III exige bancada de alta tecnologia, IV espacial): **Ferro-velho** (catação, desmanche, reciclagem complexa), **Ofícios** (cantaria, metalurgia, oficinas complexas), **Fábrica** (usinagem, eletrificação das oficinas, linha de montagem), **Componentes** (fabricação, fabricação avançada), **Laboratório e prospecção** (microeletrônica, refinação de biocombustível, perfuração profunda, multi-analisador, escâneres).
- **Absorve** (campo `absorbs`): Lapidação, Metalurgia, Oficinas complexas, Usinagem, Manufatura, Fabricação, Fabricação avançada, Microeletrônica, Refinação de biocombustível, Perfuração profunda, Multi-analisador, as 2 escaneadoras e as 2 reciclagens. A original some e tudo que dependia dela passa a depender do nó novo; por isso Cantaria (208 desbloqueios), Microeletrônica (68) e Metalurgia (44) trazem, por enquanto, conteúdo de outras árvores (pisos, munição EMP, armas), que sai quando elas forem montadas.
- Decisões: cerveja, vinho, hidromel e mosto vão para **Cozinha**; a reciclagem veio da Logística; as oficinas complexas, da Mobília. Fogões, alfaiataria, laboratórios de drogas e poções, autópsias, bancada de munição e fundição de warcasket ficam para Cozinha, Vestuário, Drogas, Medicina, Armas e Armaduras. A bancada de engenharia reversa continua no projeto próprio (exige Catação pela absorção). Fogão grande e alfaiataria grande ficam na Linha de montagem até Cozinha e Vestuário.

**5. Logística (92).**
- Conteúdo: armazenamento (Neat, Adaptive, LWM) e canos. (A reciclagem foi para a Produção, bloco Ferro-velho.)
- Absorve: o armazenamento que hoje está em Mobília complexa e Usinagem.
- **Feita em 2026-11-20** (`kits/pesquisa/Logistica.json`), "quem guarda, sobrevive": Armazenamento rústico I (saiu de Fundamentos, que fica com 5 pesquisas), caixotes e barris II, celeiro II (exige a Cantaria e o feno da Fazenda), estantes e cabides II, vitrines III, metal e cofres III (Usinagem). 80 coisas, sem absorver original (o armazenamento era arrasto de Mobília complexa, Usinagem, Metalurgia, Cantaria e Vestuário complexo).

**6. Energia (45). Feita em 2026-11-20** (`kits/pesquisa/Energia.json`).
- História: "acender as luzes de novo". Blocos: **Combustão** (Eletricidade I, geradores químicos II, industriais III, nuclear IV/Forja), **Renováveis** (vento e água II, sol e alcionita II, maré e calor da terra III, fontes avançadas IV/Velho Mundo), **Baterias** (II, avançadas IV/Velho Mundo) e **Poluição** (gerador toxificante II, bombas de poluição II).
- Absorve: Eletricidade, Baterias, Baterias avançadas, Painéis solares, Geotérmica, Maremotriz, Moinho, Geradores industriais, Nuclear, Fontes de energia avançadas e Gerador toxificante. Liga-se à Produção (refinaria, usinagem, microeletrônica).
- A Eletricidade nova é a base de Produção, Fazenda e Fundamentos (as refs foram trocadas). Por absorção ela traz, por enquanto, 32 coisas de outras árvores (luzes, aquecedor, fogão elétrico, alto-falante, batedores dos insetos...).
- A Alcionita saiu de Fundamentos. As variantes do ARC (VQE The Generator) não entram: vêm pela missão; o ARC básico vem com a Eletricidade.

**7. Água, clima e higiene (83).**
- Conteúdo: todo o Dubs Bad Hygiene, mais condicionadores e refrigeradores.
- Absorve: as 22 pesquisas do DBH (sistema de água, esgoto, poços, aquecimento, saunas, piscinas etc.), Condicionamento de ar e Refrigerador primitivo.
- **Feita em 2026-11-20** (`kits/pesquisa/Agua.json`), "água limpa é o novo ouro": **Água** (encanamento I, moderna II, industrial III, purificação III), **Esgoto** (fossa I, tratamento III), **Higiene** (rústica I, saiu de Fundamentos; banhos e sauna II com os mosaicos; banheiro de luxo III; biônicos de higiene IV/Hélice) e **Clima** (refrigerador primitivo I, água quente I, climatização II com aquecedor e ventilação, aquecimento central II, refrigeração III, combate a incêndio III). A Fazenda (Água e adubo) exige o Encanamento novo. Ficam para a árvore Espaço: água no espaço do Odyssey (nó criogênico, recuperação de água, vaso a vácuo, tanques).

**8. Cozinha e bebidas (109).**
- Conteúdo: VCE (inclusive confeitaria), VBE (inclusive cafés e chás) e pasta nutritiva.
- Absorve: Pasta nutritiva, Refeição de sobrevivência, Carne-seca, Expresso, Destilaria, Mixologia, Enlatamento, Condimentos, Confeitaria, Frituras, Grelhados, Sopas e Apicultura (a parte que sobrou: hidromel e pão doce).
- **Feita em 2026-11-20** (`kits/pesquisa/Cozinha.json`), "comida de verdade depois da pasta": **Cozinha** (fogão a lenha I, saiu de Fundamentos; carne-seca e condimentos I; grelha II; confeitaria II; molhos II; cozinha elétrica III com sopas e frituras; chocolate III; comida de prateleira IV com enlatadora e refeição embalada), **Pasta nutritiva** II e **Bebidas** (cerveja e chá I, vinho II, hidromel II, destilaria II, café e limonada II, expresso III, mixologia III). Absorve também Fabricação de cerveja e Vinho (decisão do usuário).
- Cada receita exige a planta dela na Fazenda (lúpulo, uva, mel, grãos, café, limão, açúcar, menta, especiarias, cacau, fungo); a coerência planta × produto deu 17 casos na primeira versão e 0 depois. Cigarro e charuto foram para Drogas II da Fazenda, com o tabaco.

**9. Vestuário (108).**
- Conteúdo: chapéus, roupas, vestuário nobre e utilitários (cintos).
- Absorve: Vestuário complexo, Vestuário nobre, Vestuário real, Visor eletrônico, Cintos de escudo, Cintos bomba de fumaça e Fermentação de suor.
- **Feita em 2026-11-20** (`kits/pesquisa/Vestuario.json`), "vestir o fim do mundo": **Costura** (couro e pano I, saiu de Fundamentos; alfaiataria II, exige o algodão; confecção III com a mesa grande), **Moda** (nobre II, real III) e **Equipamento** (máscaras de gás e cintos II, cintos de escudo IV/Velho Mundo). Absorve Vestuário complexo, nobre e real, Cintos de bomba de fumaça e de escudo.
- Ficam para outras árvores: armaduras e capacetes (Armaduras), comunicadores e mochilas de mecanitor (Mecanoides), visor eletrônico e aparato de salto (Armaduras), mochila tóxica, traje de rum, máscara de peste (Medicina: usa remédio de ervas). A mesa grande de alfaiataria e o fogão grande saíram da licença da Forja (eram da Manufatura).
- Atenção: 7 pesquisas hoje ficam no ramo da Forja.

**10. Medicina e farmácia (507).**
- Conteúdo: EPOE (181), Royalty (implantes), cirurgias, remédios e drogas (VFE Medieval 2 alquimia, penoxicilina, suco-turbo).
- Absorve: Próteses, Biônica, Biônica avançada, Fiação cerebral, Metabolismo artificial, Fatores de cura, Análise molecular, as pesquisas EPOE (Implantes de glitterworld, Chip MA-AI, IA de persona, Reparo sintético etc.), Procedimentos de fertilidade, Produção de medicamentos, Alquimia, Produção de suco-turbo, Penoxicilina, Despertar e Psicoide.
- A maior parte é ramo **Hélice**.

**11. Armas e munição (1.099).**
- Conteúdo: CE + Armory + Guns, VWE (+ Frontier, Laser, Coilguns, Makeshift, Bioferrite) e Medieval 2.
- Absorve: Armas de fogo, Contragolpe, Operação a gás, Precisão, Morteiros, Lança-granadas, Armas pesadas, Pólvora, Munição avançada, Optoeletrônica, Arcos, Lâminas longas, Mecha de fogo, Feixe, Laser e Munição de carga.
- **Feita em 2026-11-20** (`kits/pesquisa/Armas.json`, "do cano de encanamento ao laser do Império"), pela regra do usuário: as armas de **sucata** são as primeiras armas de fogo, **sem pré-requisito**, e cada era se divide em 4 passos por evolução (I pistolas → II armas leves: submetralhadoras, escopetas, carabinas → III fuzis → IV apoio: metralhadoras); a era seguinte só abre depois do IV da anterior. A escolha de que arma vai em cada nível ficou comigo (critério: mecanismo e época da arma, que é a ordem das pesquisas originais):
  - **Sucata** (Bancada, Medieval, 300–700): pistola, submetralhadora + escopeta, fuzil, metralhadora leve improvisados (VWE Makeshift). O I traz também a **bancada de recarga** (`AmmoBench`, que estava com Armas de fogo).
  - **Armaria** (Bancada, ação manual, 600–1.200; pede Usinagem): revólveres e pistolas antigas → escopetas e carabinas de alavanca → fuzis de ferrolho → primeiras metralhadoras (Lewis, Madsen, Boys, binóculo-rádio).
  - **Oficina automática** (Bancada, contragolpe e gás, 1.200–1.900): pistolas automáticas → submetralhadoras e escopetas automáticas → fuzis semiautomáticos → metralhadoras de apoio (LMG, BAR, DP-27, PTRS).
  - **Arsenal do Velho Mundo** (ramo Velho Mundo, 2.000–3.200; pede Microeletrônica): carabinas compactas e PDW → fuzis de assalto → fuzis de batalha e precisão → metralhadoras e antimaterial (inclui a giratória).
  - **Armas de carga** (ramo Forja, Spacer, 3.200–4.500; pede Fabricação): pistolas de carga e gauss → escopetas/submetralhadoras de carga → fuzis de carga (e a LAS-40C) → blasters, lança gauss e feixe de graser.
  - **Laser recuperado** (ramo Forja, Ultra, 5.000–7.000): pistolas → escopeta → fuzis → repetidora.
  - **Munição especial I a V**, uma por era (a I pede a lavoura "Outros I: Tinta e chama", por causa do prometeu; cada uma pede a anterior). A III absorve a Munição avançada do CE.
  - Linhas laterais: **Pólvora negra** (Bancada, pede Metalurgia: pederneira → bacamarte/arcabuz/canhão de mão → mosquetes → fuzil de muralha e morteiro de mão); **Explosivos** (granadas e pistola sinalizadora depois de Sucata III; lança-granadas e lança-chamas, foguetes e lançadores avançados no ramo Velho Mundo; munição especial de explosivos); **Arcos** (recurvo, arcos de guerra, bestas, composto; flechas especiais de fogo e veneno); **Corpo a corpo** (ferro, lâminas longas e haste, trincheira).
  - Gerado por script (no rascunho da sessão; refazer à mão pelo editor `/pesquisa`). Teste `arvore-pesquisa`: coerência planta × produto 0, absorvidas 107 (20 desta árvore), `pesquisa` e `inicio-pesquisa` sem erros.
  - **Ficaram de fora (a fazer):** munição de calibres sem arma no modpack (UNSC, Lebel, Arisaka etc.: continuam livres, mas a especial delas foi junto com a Munição avançada absorvida) e a munição de torretas, morteiros e artilharia (árvore **Defesa**); armas bioférricas (Silêncio, árvore Vazio), cajado de eltex (Psíquica), faca de caixão de guerra (Armaduras), granadas PEM/tóxica/de espuma (ficam nas pesquisas delas), estandarte heráldico e bisturi. Fundamentos continua com faca, porrete, arco curto e arremessáveis.

**12. Armaduras (40).**
- Conteúdo: armaduras CE e Royalty, colete de carga e escudos.
- Absorve: Placas, À prova de balas, Fuzileiro, Reconhecimento, Catafrata, Aparatos de salto e Janízaro.
- **Feita em 2026-11-20** (`kits/pesquisa/Armaduras.json`, "ficar vivo depois da Queda"), no mesmo modelo das Armas: 3 eras em 4 passos (cabeça → peito leve → armadura completa → pesada/escudos), a seguinte abre depois do IV.
  - **Couro e ferro** (Bancada, Medieval, pede Metalurgia): elmos → couro, gibão e escudos → couraça e couro pesado → placas (absorve Placas).
  - **Balística** (Bancada, pede Usinagem; o IV pede Microeletrônica): colete macio e equipamento de carga (mochilas, colete tático, bandoleira, óculos) → antiestilhaço (absorve À prova de balas) → compósitos → escudos balísticos, exoesqueleto e mochila de rádio.
  - **Armadura espacial** (Forja, pede Fabricação): reconhecimento e visor eletrônico → fuzileiro e corsário → janízaro → catafrata.
  - Ao lado: **Óptica I e II** (visão noturna; ramo Silêncio, como as originais), **Aparato de salto** e armadura gafanhoto (Coro) e **Caixões de guerra** (fundição; Forja).
  - Capacetes que estavam mapeados para Vestuário vieram para cá. Ficam fora: capacetes eltex (Psíquica), de mecanitor (Mecanoides), de vácuo (Espaço), renegados (Genética).

**13. Defesa (115).**
- Conteúdo: torretas (VFE Security, CE, Pirates, Deserters), trincheiras e IEDs.
- Absorve: Torretas de disparo, Torretas pesadas, Torreta de urânio, Canhão de espuma, Enxame de foguetes, Trincheiras, Railgun, EMP, Repulsor, Cerco, Defesas ocultas e Demolição remota.
- Também absorve o armamento de caixão de guerra (Pirates).
- **Feita em 2026-11-20** (`kits/pesquisa/Defesa.json`, "segurar o que é seu"), no modelo das Armas (linhas em passos, cada uma pedindo a anterior):
  - **Fortificação** (Bancada, depois de Fundamentos › Fortificação): trincheiras escoradas → arame e armadilhas → guerra de trincheira → concreto e defesas ocultas.
  - **Minas:** IEDs e mina terrestre → minas especiais (PEM, espuma, tóxica, choque; pede Microeletrônica) → demolição remota e antigrão (Forja).
  - **Torretas** (pedem Oficina automática I e Eletricidade): leves → metralhadoras montadas (com caixa de munição e carregadores) → metralhadoras pesadas (Velho Mundo) → canhões automáticos (Velho Mundo); depois **Defesas espaciais** (Forja): urânio e enxame → carga → imperiais, tesla e railgun → graser e repulsores.
  - **Cerco** (pedem Metalurgia; canhões pedem Pólvora negra IV): armas de muralha → balista e catapulta → canhões → órgão e Gatling. **Artilharia:** morteiros → canhão de campanha → obuses → morteiro automático, mais **Munição especial: Artilharia** (pede a Munição especial I das Armas, pelo prometeu).
  - **Armamento de caixão de guerra I e II** (Forja, depois de Armaduras › Caixões de guerra).
  - **Munição de torreta:** o catálogo agora exporta a munição do canhão de cada torreta (`ammo` nos prédios; `ResearchCatalog.cs` do mod de debug). A comum vem com a torreta; a especial (incendiária, fumaça, PEM, tóxica, sabot…) na Munição especial: Artilharia. Ficam de fora a munição já dada pelas Armas, a do kit livre (as lanças da balista) e a que não se fabrica (projéteis antigrão, só saque).
  - Fora: complexo incinerador pesado (bioferrita; árvore Vazio). Fundamentos ainda tem Armas improvisadas e Fortificação de campo: decidir se somem agora que Armas e Defesa estão prontas.

**14 a 16. Forja (licença).**
- **Veículos:** os 4 projetos do VVE.
- **Mecanoides e drones:** meca-tecnologia básica, padrão, elevada e ultra, Drones (VQE) e Atomizador.
- **Androides:** Tecnologia androide e Logística de neutroamina.

**17 e 18. Hélice (licença).**
- **Genética:** Xenogenética, Processador de genes, Tanques de crescimento, Descanso mortal, Alpha Genes e sanguófagos.
- **Insetoides:** as 3 tecnologias de colmeia.

**19. Vazio (Silêncio).**
- As 24 pesquisas do Anomaly: bioferrita, sérums, carniçais, contenção, rituais e armamento de vazio.

**20. Psíquica (Coro).**
- Equipamento de eltex e de conjurador.
- É pequena: pode virar um bloco dentro de outra árvore.

**21. Espaço e comunicação.**
- Odyssey (gravitech, pontes, ônibus), nave, criptossono, cápsulas de transporte, console de comunicação e baliza.
- Ramos de hoje: Velho Mundo e Forja.

**22. Ideologia (222).**
- Alpha Memes (164, quase tudo piso e estrutura de meme), VIE Memes e Ideology.
- Quase sem pesquisa hoje. Pode ficar quase toda livre ou ir para Construção e Mobília.

## Ordem sugerida

1. **Decidir os casos 1 a 4.** Munição, pisos, cirurgias e o que fica livre mudam o tamanho de tudo.
2. **Árvores de base (Bancada)**, nesta ordem: Produção → Energia → Construção → Mobília → Logística → Água/clima → Cozinha → Vestuário.
3. **Combate:** Armas e munição, Armaduras e Defesa.
4. **Medicina**, que é mista: Bancada para o básico e Hélice para biônicos.
5. **Árvores de facção:** Forja, Hélice, Silêncio, Coro e Velho Mundo. Combinar com a etapa 3f, pesquisa por manual.
6. **Ideologia e o que sobrar.**

Cada árvore pode ser montada como a Fazenda:
- um script gera o JSON a partir do mapa;
- você revisa e ajusta no editor (`/pesquisa`);
- "Gerar Defs" aplica no jogo;
- a pesquisa original que esvaziar some sozinha; a que for substituída de vez entra em `absorbs` do nó.
- Textos no tom da Produção e da Fazenda: o nome do nível diz o que é, a descrição diz o conteúdo e termina com uma frase da lore (a Queda, o Velho Mundo, as 4 potências, a Feira).
