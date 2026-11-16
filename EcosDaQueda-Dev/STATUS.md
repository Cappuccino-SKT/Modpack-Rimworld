# Status

_Só o estado atual e os próximos passos. Manter curto (até ~60 linhas). Detalhe do que já foi feito: `HISTORICO.md` (buscar, não ler inteiro)._

## Onde estamos (2026-11-16)

- **Base pronta e testada:** 4 potências (Silêncio, Hélice, Forja, Coro) e 4 facções menores (Fora de Linha, Não Catalogados, Feira Andante, Sindicato do Saque), todas com ideologia do usuário (`.rid`), xenotipos próprios, relações fixas, Fama/Infâmia, cerimônia de submissão, líder e 7 cargos por potência.
- **Mundo só com as nossas facções** (sem Império/Desertores); Hegemony é a camada de estratégia.
- **Plano em etapas:** `PLANO.md`. Etapa 1 (travas do início: xenos, cenários, narrador provisório) feita. **Etapa 2 em andamento:** feitos arsenal por facção (A), armadura-caixão da Forja (C), guarda-roupa e comércio por facção, **psycasts do Coro por cargo (G)**, **questão androide (H)** e **permissões por cargo (B)**: 25 permissões da Royalty (6–7 por potência: entregas, trabalhadores, ajuda militar com as nossas unidades, recall orbital da Forja, liteira do Coro), 1 ponto por cargo, uso pago com favor. No H, fabricar sem o cargo técnico licenciado da Forja é pirataria (Infâmia); androide desperto na colônia dá Infâmia na Forja e boa vontade nos Fora de Linha. No G, cada cargo licencia um caminho do VPE (Protetor → … → Arconte → Hemosage); sem o cargo o colono não destrava, a cerimônia dá de graça, perder o cargo revoga.
- **Vassalagem:** só o Coro dá nível psíquico; a Forja dá vínculo mecanoide.

## Próximo passo

Blocos da etapa 2 ainda abertos, na ordem sugerida: **K** (bases por potência); depois D, E, F, I, L, M, N, O. A escolha do próximo bloco é do usuário.

## Não commitado

Nada (blocos G, H e B commitados em 2026-11-16; sem push).

## Como testar

- Sempre `-Profile full` (o perfil lean quebra em jogo novo: os `.rid` pedem preceitos de mods que ele não carrega).
- Regressão inteira em uma chamada: `scripts/regress.ps1` (uma linha por teste). Um teste só: `scripts/run-test.ps1` (imprime resumo; relatório completo em `tests/out/`).

## Pendências técnicas

- Perfil lean do executor quebrado (ampliar ou aposentar).
- `xenogen` marca os androides dos Fora de Linha como PROBLEMA (`faccoes-menores`, 80; às vezes 1 aldeão androide em `faccoes-menores2`) e a regressão conta `problemas=0` como falha: ajustar as duas checagens.
- Erros de terceiros não resolvidos: Warbolter ao largar arma; `Invalid pawn assigned to Bloodletter otherworldly role` (Vanilla Ideology Expanded, intermitente em `fase3`); causa raiz da falha de `Book.GenerateBook` (hoje só contornada).
- Lojas ainda vendem roupas comuns genéricas; preços e quantidades do comércio sem balanceamento; equilíbrio de laser/bobina com o CE e do cavaleiro lacrado (320 pontos).
- Facção rebelde do Hegemony, ícones das facções menores, bug Facial Animation / Seamless World, traduções restantes.
- Itens "(decidir)" do `PLANO.md`.

## A conferir pelo usuário (só visual)

- Início de jogo com o modo desenvolvedor **desligado**: menu de xenos, cenários e narrador; Prepare Carefully.
- Aba Permissões do líder (botão "Escolher permissão") e o menu de ajuda real de cada potência; textos das 25 permissões.
- Mensagens da questão androide (pirataria, unidade registrada, desperto) ao fabricar e ao despertar.
- Aba Psicasts do VPE com caminhos licenciados: botão travado com o motivo "licença do Coro (cargo)" e mensagem ao receber o caminho na cerimônia.
- Aba Reputação (lealdade, líder, favor, sucessor, botão "Cerimônia"), cena da cerimônia no mapa, aba de títulos do líder.
- Visual das unidades (roupas, armas, xenos, mechs da Forja) e a janela de comércio de cada facção.
- Aba Ideologia das 8 facções; nomes de assentamento.
- A fazer pelo usuário: xeno do líder da Forja, arte do retrato do narrador.
