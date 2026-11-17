# Status

_Só o estado atual e os próximos passos. Manter curto (até ~60 linhas). Detalhe do que já foi feito: `HISTORICO.md` (buscar, não ler inteiro)._

## Onde estamos (2026-11-16)

- **Base pronta e testada:** 4 potências (Silêncio, Hélice, Forja, Coro) e 5 facções menores (Fora de Linha, Não Catalogados, Feira Andante, Sindicato do Saque, **Reino do Lodo**: mutantes medievais, xenos Bruto/Bruta e ideologia "A Esteira Sagrada" do usuário), todas com ideologia do usuário (`.rid`), xenotipos próprios, relações fixas, Fama/Infâmia, cerimônia de submissão, líder e 7 cargos por potência.
- **Mundo só com as nossas facções** (sem Império/Desertores); Hegemony é a camada de estratégia.
- **Plano em etapas:** `PLANO.md`. Etapa 1 (travas do início: xenos, cenários, narrador provisório) feita. **Etapa 2 em andamento:** feitos arsenal por facção (A), armadura-caixão da Forja (C), guarda-roupa e comércio por facção, **psycasts do Coro por cargo (G)**, **questão androide (H)**, **permissões por cargo (B)**, **bases por potência (K)**, **medieval (D)** e **insetos (E)**: toda colmeia destruída pelo jogador é um "recall" (Fama na Hélice e favor se o líder for vassalo dela); a Hélice vende núcleos de colmeia (pherocores), quitina e geleia, e compra os núcleos de volta. No D, arsenal próprio dos Não Catalogados com armas do VFE Medieval 2; o Coro recebe vinho no estipêndio, aceita vinho no dízimo e tem o serviço "torneio de corte" (prata + vinho). No K, plano próprio do Custom Base Framework para cada potência (arquivo-bunker, filial da Hélice, forja-fábrica, corte do Coro) com guarnição nossa. No B, 25 permissões da Royalty (6–7 por potência: entregas, trabalhadores, ajuda militar com as nossas unidades, recall orbital da Forja, liteira do Coro), 1 ponto por cargo, uso pago com favor. No H, fabricar sem o cargo técnico licenciado da Forja é pirataria (Infâmia); androide desperto na colônia dá Infâmia na Forja e boa vontade nos Fora de Linha. No G, cada cargo licencia um caminho do VPE (Protetor → … → Arconte → Hemosage); sem o cargo o colono não destrava, a cerimônia dá de graça, perder o cargo revoga.
- **Vassalagem:** só o Coro dá nível psíquico; a Forja dá vínculo mecanoide.

## Próximo passo

Blocos da etapa 2 ainda abertos, na ordem sugerida: F, I, L, M, N, O (etapa 2); depois a etapa 3 (tecnologia licenciada). A escolha do próximo bloco é do usuário.

## Não commitado

Character Creator no modpack e facção Reino do Lodo (mutantes). Blocos G, H, B, K, D, E e a tradução do VFE Medieval 2 já estão no GitHub.

## Como testar

- Sempre `-Profile full` (o perfil lean quebra em jogo novo: os `.rid` pedem preceitos de mods que ele não carrega).
- Regressão inteira em uma chamada: `scripts/regress.ps1` (uma linha por teste). Um teste só: `scripts/run-test.ps1` (imprime resumo; relatório completo em `tests/out/`).

## Pendências técnicas

- Bases: o grupo de assentamento de cada potência tem o líder com peso 1, então bases ricas saem com vários líderes (ex.: 4 engenheiros-primazes); baixar ou tirar o líder do grupo `Settlement`. O número de defensores varia com a riqueza sorteada da base (orçamento do CBF).
- **Character Creator** (`astryl.ModernCC`, instalado em 2026-11-16): editor completo de personagens, inclusive genes, na criação de colonos e com F12 a qualquer hora. Contorna a trava de xenos da etapa 1 (que cobre a tela do jogo e o Prepare Carefully). Decidir: aceitar como ferramenta do jogador ou travar a aba de genes na criação.
- Reino do Lodo: as brutas saem sem roupa (orçamento baixo + preceito de nudez feminina obrigatória da ideologia); bases medievais com poucos defensores humanos (orçamento do CBF).
- Fungobi (4 xenos do usuário em `Xenotypes/`, fúngicos e estéreis): ainda não usados; perguntar a que facção pertencem.
- Executor: o `RESULTADO` não conta erros de **carregamento de Defs** (ex.: referência cruzada não resolvida); eles só aparecem na lista ERROR/WARN do resumo. Conferir essa lista ou fazer o executor contar.
- Perfil lean do executor quebrado (ampliar ou aposentar).
- `xenogen` marca os androides dos Fora de Linha como PROBLEMA (`faccoes-menores`, 80; às vezes 1 aldeão androide em `faccoes-menores2`) e a regressão conta `problemas=0` como falha: ajustar as duas checagens.
- **Intermitente, investigar:** `Faction <potência> contains ideo <ideologia> which was removed!` junto de `Error while notifying ideos of a HistoryEvent` (Alpha Memes, pawn morto de velhice na geração). Apareceu 2 vezes em ~25 rodadas em 2026-11-16 (`insetos` com o Coro, `cerimonia-forja` com a Hélice); repetindo os mesmos testes, some. Não vem da tradução (sem "Bad string pass" no log) nem do nosso código de ideologia (ele só copia o `.rid`, não remove). Em `fase3` reapareceu o intermitente do Vanilla Ideology Expanded (`Invalid pawn assigned to <cargo> role`).
- Erros de terceiros não resolvidos: Warbolter ao largar arma; `Invalid pawn assigned to Bloodletter otherworldly role` (Vanilla Ideology Expanded, intermitente em `fase3`); causa raiz da falha de `Book.GenerateBook` (hoje só contornada).
- Lojas ainda vendem roupas comuns genéricas; preços e quantidades do comércio sem balanceamento; equilíbrio de laser/bobina com o CE e do cavaleiro lacrado (320 pontos).
- VFE Medieval 2 traduzido (1464/1540; faltam só geradores de nomes vikings/medievais e 3 "Mote", de propósito: as listas de palavras do mod ficam em inglês e misturariam idiomas).
- Facção rebelde do Hegemony, ícones das facções menores, bug Facial Animation / Seamless World, traduções restantes.
- Itens "(decidir)" do `PLANO.md`.

## A conferir pelo usuário (só visual)

- Início de jogo com o modo desenvolvedor **desligado**: menu de xenos, cenários e narrador; Prepare Carefully.
- Visual das bases (visitar ou atacar uma de cada potência): prédios, muralha, decoração.
- Aba Permissões do líder (botão "Escolher permissão") e o menu de ajuda real de cada potência; textos das 25 permissões.
- Mensagens da questão androide (pirataria, unidade registrada, desperto) ao fabricar e ao despertar.
- Aba Psicasts do VPE com caminhos licenciados: botão travado com o motivo "licença do Coro (cargo)" e mensagem ao receber o caminho na cerimônia.
- Aba Reputação (lealdade, líder, favor, sucessor, botão "Cerimônia"), cena da cerimônia no mapa, aba de títulos do líder.
- Visual das unidades (roupas, armas, xenos, mechs da Forja) e a janela de comércio de cada facção.
- Aba Ideologia das 8 facções; nomes de assentamento.
- A fazer pelo usuário: xeno do líder da Forja, arte do retrato do narrador.
