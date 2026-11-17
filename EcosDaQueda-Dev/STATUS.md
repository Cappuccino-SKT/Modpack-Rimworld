# Status

_Só o estado atual e os próximos passos. Manter curto (até ~60 linhas). Detalhe do que já foi feito: `HISTORICO.md` (buscar, não ler inteiro)._

## Onde estamos (2026-11-17)

- **Base pronta e testada:** 4 potências (Silêncio, Hélice, Forja, Coro) e 5 facções menores (Fora de Linha, Não Catalogados, Feira Andante, Sindicato do Saque, **Reino do Lodo**: mutantes medievais, xenos Bruto/Bruta e ideologia "A Esteira Sagrada" do usuário), todas com ideologia do usuário (`.rid`), xenotipos próprios, relações fixas, Fama/Infâmia, cerimônia de submissão, líder e 7 cargos por potência.
- **Mundo só com as nossas facções** (sem Império/Desertores); Hegemony é a camada de estratégia.
- **Plano em etapas:** `PLANO.md`. Etapa 1 (travas do início: xenos, cenários, narrador provisório) feita. **Etapa 2 concluída** (2026-11-17). No **O (genes)**: nossos xenos já usavam genes de Fungoid, Lycanthrope, Saurid e Phytokin (só o Starjack sem uso); o resto virou a **Linha Exótica da Hélice**, pacotes de genes só com genes desses mods e do Insector (71 genes) na loja e na caravana dela (`StockGenerator_EQGenepacks`). Feitos também arsenal por facção (A), armadura-caixão da Forja (C), guarda-roupa e comércio por facção, **psycasts do Coro por cargo (G)**, **questão androide (H)**, **permissões por cargo (B)**, **bases por potência (K)**, **medieval (D)**, **insetos (E)**, **missões (F)**, **anomalia (I)**, **sucata e bichos (L)**, **veículos (M)** e **marcos do mundo (N)**. No N: 10 marcos próprios com nome sorteado e história (cratera da Queda, campo da Última Ofensiva, fazenda-modelo da Linha Robusta, poços da Primeira Escuta, cicatriz do Projeto Jardim, pátio de recall, laboratório da colmeia sem recall, plataforma da Evacuação, guarnição esquecida, refinaria em greve), ~10% dos marcos do mundo; os 59 do VEE seguem em inglês. No M: blindado montado sem o cargo cavaleiro da garantia da Forja é pirataria (Forja -10); a Feira vende títulos de posse de veículos usados (lata-velha, seminovo: peças gastas, tanque baixo) e a Forja vende o blindado registrado (só com o cargo). O Vehicle Framework deixa desligados os veículos em ataques de NPC e o comércio de veículos, por isso o título de posse. No L: ataques do Sindicato podem vir com bichos de lixão do VAE Waste Animals ("piquete com bichos") e os do Reino do Lodo com javalis e búfalos de lixão ("estouro do chiqueiro"); o Sindicato carrega lixo e pacotes do Vanilla Recycling (vira saque); traços do Vanilla Traits nos líderes (arquivista-mor Acadêmico, diretor Magnata, primaz Viciado em trabalho, maestro Esnobe) e nos cargos altos do Sindicato; a Feira compra lixo. No I (Insanity): membros do Silêncio pagam a pesquisa em sanidade (6%/dia a 100% de velocidade) e pesquisam mais rápido quanto menos sãos (até +60%); a Consulta ao Arquivo cobra 15% do colono mais estudado; serviço novo "retiro de silêncio" (+50% de sanidade ao mais abalado, 5 dias sem voz). No F: missões dos VQE dão Fama (e favor, se for a potência do líder) a uma patrona (Ancients → Silêncio; Drone Factory e The Generator → Forja); drone fabricado sem o cargo técnico licenciado da Forja é pirataria; "Saídos do abrigo" começa dentro do abrigo selado do Ancients, com peças de gerador ARC. No E: toda colmeia destruída pelo jogador é um "recall" (Fama na Hélice e favor se o líder for vassalo dela); a Hélice vende núcleos de colmeia (pherocores), quitina e geleia, e compra os núcleos de volta. No D, arsenal próprio dos Não Catalogados com armas do VFE Medieval 2; o Coro recebe vinho no estipêndio, aceita vinho no dízimo e tem o serviço "torneio de corte" (prata + vinho). No K, plano próprio do Custom Base Framework para cada potência (arquivo-bunker, filial da Hélice, forja-fábrica, corte do Coro) com guarnição nossa. No B, 25 permissões da Royalty (6–7 por potência: entregas, trabalhadores, ajuda militar com as nossas unidades, recall orbital da Forja, liteira do Coro), 1 ponto por cargo, uso pago com favor. No H, fabricar sem o cargo técnico licenciado da Forja é pirataria (Infâmia); androide desperto na colônia dá Infâmia na Forja e boa vontade nos Fora de Linha. No G, cada cargo licencia um caminho do VPE (Protetor → … → Arconte → Hemosage); sem o cargo o colono não destrava, a cerimônia dá de graça, perder o cargo revoga.
- **Fungobi (zumbis do mundo):** 4 xenos do usuário (`EQ_Xeno_Fungobi`, `FungobiAtirador`, `FungobiAcido`, `FungobiLider`), estéreis, sem facção; gene próprio `EQ_Gene_PeleFioDoDiabo` (carneado rende ~94 fio do diabo, no lugar do couro de pano do Alpha Genes). Ainda **não aparecem no jogo**: vão para eventos e cidades destruídas (etapa 5).
- **Vassalagem:** só o Coro dá nível psíquico; a Forja dá vínculo mecanoide.

## Próximo passo

**Etapa 3 (tecnologia própria)**: decisões e classificação aprovadas (`PLANO.md` › Etapa 3; `tecnico/pesquisa-classificacao.md`). **3a feito**: árvore em 6 abas próprias, trava fora da bancada. **3b feito** (2026-11-17): o cargo licencia os projetos do ramo da potência por nível (medieval 1º cargo, industrial 2º, Anomalia 3º, espacial 4º, ultra 6º); membros compram as licenças de cima com favor (botão "Licenças" na aba Reputação; valem enquanto for membro); a licença abre a pesquisa e dispensa techprints; fabricar ou construir com projeto de potência sem licença é pirataria (boa vontade -4, uma vez por dia por potência). Num jogo real, Velho Mundo e as patentes das potências de que você não é membro seguem fechados até o 3c/3d. **Próximo: bloco 3c (engenharia reversa).** Roteiro completo do que falta: `PLANO.md` › Roteiro.

**A Colmeia Erguida** (2026-11-17): facção menor neolítica com os 3 xenos de inseto do usuário (rainha líder, soldado, operário; + batedor com arco), 12 vilas no mundo, teste `colmeia`. **Provisório:** ideologia sorteada "O Enxame (provisória)" e descrição curta. **Aguardando o usuário montar a ideologia** (`.rid`); depois: ligar em `EQ_IdeoTemplates.xml`, escrever a história (DESIGN, descrições), relações fixas e parte horrível.

## Não commitado

Bloco 3b: mod (`Licenses.cs`, `Research.cs`, `MainTabWindow_Reputation.cs`, `EQ_Research.xml`), debug (`EQDebug.cs`) e raiz (docs, `tests/licencas.txt`, `regress.ps1`).

## Como testar

- Sempre `-Profile full` (o perfil lean quebra em jogo novo: os `.rid` pedem preceitos de mods que ele não carrega).
- Regressão inteira em uma chamada: `scripts/regress.ps1` (uma linha por teste). Um teste só: `scripts/run-test.ps1` (imprime resumo; relatório completo em `tests/out/`).

## Pendências técnicas

- Bases: o grupo de assentamento de cada potência tem o líder com peso 1, então bases ricas saem com vários líderes (ex.: 4 engenheiros-primazes); baixar ou tirar o líder do grupo `Settlement`. O número de defensores varia com a riqueza sorteada da base (orçamento do CBF).
- **Character Creator** (`astryl.ModernCC`, 2026-11-16) é **temporário**: o usuário o usa para montar as roupas das facções. Tirar do modpack quando ele avisar (contorna a trava de xenos da etapa 1; não travar).
- Reino do Lodo: as brutas saem sem roupa (orçamento baixo + preceito de nudez feminina obrigatória da ideologia); bases medievais com poucos defensores humanos (orçamento do CBF).
- Executor: o `RESULTADO` não conta erros de **carregamento de Defs** (ex.: referência cruzada não resolvida); eles só aparecem na lista ERROR/WARN do resumo. Conferir essa lista ou fazer o executor contar.
- Perfil lean do executor quebrado (ampliar ou aposentar).
- `xenogen` marca os androides dos Fora de Linha como PROBLEMA (`faccoes-menores`, 80; às vezes 1 aldeão androide em `faccoes-menores2`) e a regressão conta `problemas=0` como falha: ajustar as duas checagens.
- **Intermitente, investigar:** `Faction <potência> contains ideo <ideologia> which was removed!` junto de `Error while notifying ideos of a HistoryEvent` (Alpha Memes, pawn morto de velhice na geração). Apareceu 2 vezes em ~25 rodadas em 2026-11-16 e de novo em `ideo` na regressão de 2026-11-17 (bloco L; repetido, passou) (`insetos` com o Coro, `cerimonia-forja` com a Hélice); repetindo os mesmos testes, some. Não vem da tradução (sem "Bad string pass" no log) nem do nosso código de ideologia (ele só copia o `.rid`, não remove). Em `fase3` reapareceu o intermitente do Vanilla Ideology Expanded (`Invalid pawn assigned to <cargo> role`).
- **Novo, intermitente (2026-11-17, `faccoes-menores`):** Não Catalogado com gene de explosão de carne do Alpha Genes (`HediffComp_MeatBurst`) morre de velhice na geração (`xenogen`) e tenta soltar um Fingerspike em mapa nulo (5 erros, Vehicle Framework no meio). Não vem do bloco F. Saída: impedir morte na geração ou pular o MeatBurst sem mapa.
- **Intermitente em `sucata` (2026-11-17, regressão do 3b):** `ran out of path nodes while pathing to Thing_HumanN` (busca de caminho vanilla); repetido, passou. A ideologia removida voltou em `cerimonia-helice` na mesma regressão.
- **Intermitente em `cerimonia-helice` (2026-11-17, regressão do bloco M):** `JobDriver_BestowingCeremony` lança exceção no initAction do toil 2; repetido, passou. Investigar se voltar.
- **Intermitente em `arsenal-forja` (2026-11-17):** missão vanilla de pedintes (`QuestPart_BegForItems`/`PawnsArrivalModeWorker_EdgeWalkIn`) sem célula de entrada → "spawn out of bounds"; noutra rodada, `Error in GenStep` (NRE). Nada passa pelo nosso código.
- Erros de terceiros não resolvidos: Warbolter ao largar arma; `Invalid pawn assigned to Bloodletter otherworldly role` (Vanilla Ideology Expanded, intermitente em `fase3`); causa raiz da falha de `Book.GenerateBook` (hoje só contornada).
- Lojas ainda vendem roupas comuns genéricas; preços e quantidades do comércio sem balanceamento; equilíbrio de laser/bobina com o CE e do cavaleiro lacrado (320 pontos).
- VFE Medieval 2 traduzido (1464/1540; faltam só geradores de nomes vikings/medievais e 3 "Mote", de propósito: as listas de palavras do mod ficam em inglês e misturariam idiomas).
- Facção rebelde do Hegemony, ícones das facções menores, bug Facial Animation / Seamless World, traduções restantes.
- Itens "(decidir)" do `PLANO.md`.
- Bloco F: textos das missões dos VQE ainda em inglês (tradução pendente); quem pede a missão continua sendo "qualquer líder de facção".

## A conferir pelo usuário (só visual)

- Etapa 3b: botão "Licenças" na aba Reputação (4 botões menores à direita), janela de licenças (filtro, "Comprar (N)", estado), motivo da trava na pesquisa ("exige o cargo de …") e a mensagem de pirataria ao fabricar.
- Etapa 3a: tela de pesquisa (YART) com as 6 abas, posições, motivo da trava ("Patente da Forja: exige licença…") e a linha "Ramo:" na descrição.

- Início de jogo com o modo desenvolvedor **desligado**: menu de xenos, cenários e narrador; Prepare Carefully.
- Visual das bases (visitar ou atacar uma de cada potência): prédios, muralha, decoração.
- Aba Permissões do líder (botão "Escolher permissão") e o menu de ajuda real de cada potência; textos das 25 permissões.
- Bloco I: serviço "retiro de silêncio" no menu, mensagens de sanidade, a parte "Método do Silêncio" no detalhamento da velocidade de pesquisa e a necessidade de sanidade na aba do colono.
- Bloco N: marcos nossos no mapa-mundi (ícone, nome, dica com a história na aba Terreno) e o mapa gerado num deles (cratera da Queda com destroços, fazenda-modelo poluída).
- Colmeia Erguida: aparência dos insetoides (quitina, membros), ícone e vilas no mapa-mundi.
- Bloco O: pacotes da Linha Exótica na janela de comércio da Hélice (nome e genes do pacote).
- Bloco M: títulos de posse na janela de comércio da Feira e da Forja, botão de usar, veículo entregue perto do colono (cor aleatória, peças gastas na aba do veículo) e as mensagens de blindado pirateado/registrado ao montar um.
- Bloco L: bichos de lixão no ataque do Sindicato e na caravana do Reino do Lodo; traços nos líderes (aba do colono ao capturar).
- Mensagens da questão androide (pirataria, unidade registrada, desperto) ao fabricar e ao despertar.
- Aba Psicasts do VPE com caminhos licenciados: botão travado com o motivo "licença do Coro (cargo)" e mensagem ao receber o caminho na cerimônia.
- Início "Saídos do abrigo" no abrigo selado do VQE Ancients (colonos lá dentro, porta trancada, névoa) e mensagens de missão com patrono e de drone pirateado/registrado.
- Aba Reputação (lealdade, líder, favor, sucessor, botão "Cerimônia"), cena da cerimônia no mapa, aba de títulos do líder.
- Visual das unidades (roupas, armas, xenos, mechs da Forja) e a janela de comércio de cada facção.
- Aba Ideologia das 8 facções; nomes de assentamento.
- A fazer pelo usuário: xeno do líder da Forja, arte do retrato do narrador.
