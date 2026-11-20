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
| 1 | **Fazenda** | 278 | 0 | Bancada | **feita** (61 pesquisas) |
| 2 | Construção (paredes, portas, pisos) | 557 | 80 | Bancada | a fazer |
| 3 | Mobília e lazer | 179 | 41 | Bancada (Coro 4) | a fazer |
| 4 | Produção e indústria | 79 | 19 | Bancada (Silêncio 6, Forja 4) | a fazer |
| 5 | Logística (armazenamento, canos, reciclagem) | 92 | 11 | Bancada | a fazer |
| 6 | Energia | 45 | 16 | Bancada (Velho Mundo 2) | a fazer |
| 7 | Água, clima e higiene (DBH + temperatura) | 83 | 10 | Bancada (Velho Mundo 3) | a fazer |
| 8 | Cozinha e bebidas | 109 | 38 | Bancada | a fazer |
| 9 | Vestuário | 108 | 29 | Bancada (Forja 7) | a fazer |
| 10 | Medicina e farmácia (próteses, implantes, cirurgias, drogas) | 507 | 332 | **Hélice 23**, Bancada 13 | a fazer |
| 11 | Armas e munição | 1.099 | 380 | Bancada 16, Velho Mundo 8, Silêncio 5, Forja 4 | a fazer |
| 12 | Armaduras | 40 | 3 | Bancada 6, Forja 4 | a fazer |
| 13 | Defesa (torretas, armadilhas, trincheiras) | 115 | 8 | Bancada 18, **Forja 13**, Velho Mundo 9 | a fazer |
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

**Aplicado em 2026-11-19 (proposta aprovada):** `Patches/EQ_InicioSemPesquisa.xml` tira as tags de início da `PlayerColony` (0 pesquisas concluídas; os memes ainda dão as deles). O kit livre está em `kits/pesquisa/Fundamentos.json` (campo `kit`, 36 defs). As outras 161 coisas livres foram para a árvore provisória **Fundamentos** (13 pesquisas baratas: Carpintaria, Alvenaria, Pisos, Mobília, Arte, Armazenamento, Higiene, Cozinha, Roupas, Fumo, Armas, Fortificação, Alcionita). O comando `inicio` confere tudo (`concluídas agora: 0`, `kit livre 36/36`, `Fundamentos travados 161`). Ficaram de fora porque já têm outra trava: munição (precisa da arma), cirurgias (precisam do implante), itens de ideologia (precisam do preceito), drones e ARCs, e recursos feitos em bancada.

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
- Ideia de blocos:
  - Paredes e portas, por era: madeira → pedra → aço → plastaço/autoportas.
  - Pisos, por material: chão batido → madeira → pedra → tapete → metal/estéril.
  - Arquitetura medieval e heráldica.

**3. Mobília e lazer (179).**
- Mods principais: VFE (mobília básica, moderna e artesanal), Medieval 2 (oficinas complexas), Empire (decorações nobres, órgãos, trono), Door Mat e Alpha Biomes.
- Absorve: Mobília básica, Mobília moderna, Mobília artesanal, Descarte de lixo, Luzes avançadas, TV de tubo, TV de tela plana, Harpa, Cravo, Piano, Decorações nobres, Órgãos de tubos e Trono estélico.
- Atenção: Eletricidade, Refinação de biocombustível e Perfuração profunda caíram aqui só por arrasto. São pesquisas-base e devem ficar como nós de referência.

**4. Produção e indústria (79).**
- Conteúdo: bancadas (fabricação, usinagem, manufatura VFE), escâneres, bomba de umidade, multi-analisador e autópsias.
- Absorve: Fabricação, Manufatura, Escaneadoras, Multi-analisador, Autópsias, Produção de drogas, Fabricação de cerveja e Vinho.
- Atenção: cerveja e vinho talvez fiquem melhor em Cozinha e bebidas.

**5. Logística (92).**
- Conteúdo: armazenamento (Neat, Adaptive, LWM), reciclagem (VRecycling) e canos.
- Absorve: Reciclagem básica e Reciclagem complexa, mais o armazenamento que hoje está em Mobília complexa e Usinagem.

**6. Energia (45).**
- Conteúdo: VQE Generator (ARCs), VFE Power e Core.
- Absorve: Baterias, Baterias avançadas, Painéis solares, Geotérmica, Maremotriz, Moinho, Geradores industriais, Nuclear, Fontes de energia avançadas e Gerador toxificante.

**7. Água, clima e higiene (83).**
- Conteúdo: todo o Dubs Bad Hygiene, mais condicionadores e refrigeradores.
- Absorve: as 22 pesquisas do DBH (sistema de água, esgoto, poços, aquecimento, saunas, piscinas etc.), Condicionamento de ar e Refrigerador primitivo.

**8. Cozinha e bebidas (109).**
- Conteúdo: VCE (inclusive confeitaria), VBE (inclusive cafés e chás) e pasta nutritiva.
- Absorve: Pasta nutritiva, Refeição de sobrevivência, Carne-seca, Expresso, Destilaria, Mixologia, Enlatamento, Condimentos, Confeitaria, Frituras, Grelhados, Sopas e Apicultura (a parte que sobrou: hidromel e pão doce).

**9. Vestuário (108).**
- Conteúdo: chapéus, roupas, vestuário nobre e utilitários (cintos).
- Absorve: Vestuário complexo, Vestuário nobre, Vestuário real, Visor eletrônico, Cintos de escudo, Cintos bomba de fumaça e Fermentação de suor.
- Atenção: 7 pesquisas hoje ficam no ramo da Forja.

**10. Medicina e farmácia (507).**
- Conteúdo: EPOE (181), Royalty (implantes), cirurgias, remédios e drogas (VFE Medieval 2 alquimia, penoxicilina, suco-turbo).
- Absorve: Próteses, Biônica, Biônica avançada, Fiação cerebral, Metabolismo artificial, Fatores de cura, Análise molecular, as pesquisas EPOE (Implantes de glitterworld, Chip MA-AI, IA de persona, Reparo sintético etc.), Procedimentos de fertilidade, Produção de medicamentos, Alquimia, Produção de suco-turbo, Penoxicilina, Despertar e Psicoide.
- A maior parte é ramo **Hélice**.

**11. Armas e munição (1.099).**
- Conteúdo: CE + Armory + Guns, VWE (+ Frontier, Laser, Coilguns, Makeshift, Bioferrite) e Medieval 2.
- Absorve: Armas de fogo, Contragolpe, Operação a gás, Precisão, Morteiros, Lança-granadas, Armas pesadas, Pólvora, Munição avançada, Optoeletrônica, Arcos, Lâminas longas, Mecha de fogo, Feixe, Laser e Munição de carga.

**12. Armaduras (40).**
- Conteúdo: armaduras CE e Royalty, colete de carga e escudos.
- Absorve: Placas, À prova de balas, Fuzileiro, Reconhecimento, Catafrata, Aparatos de salto e Janízaro.

**13. Defesa (115).**
- Conteúdo: torretas (VFE Security, CE, Pirates, Deserters), trincheiras e IEDs.
- Absorve: Torretas de disparo, Torretas pesadas, Torreta de urânio, Canhão de espuma, Enxame de foguetes, Trincheiras, Railgun, EMP, Repulsor, Cerco, Defesas ocultas e Demolição remota.
- Também absorve o armamento de caixão de guerra (Pirates).

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
- a pesquisa original que esvaziar some sozinha.
