# Status

_Só o estado atual e os próximos passos. Manter curto (até ~60 linhas). Detalhe do que já foi feito: `HISTORICO.md` (buscar, não ler inteiro)._

## Onde estamos (2026-11-18)

- **Base pronta e testada:** 4 potências (Silêncio, Hélice, Forja, Coro) e 6 facções menores (Fora de Linha, Não Catalogados, Feira Andante, Sindicato do Saque, Reino do Lodo, Colmeia Erguida), ideologias do usuário, xenos próprios, Fama/Infâmia, cerimônia, líder e cargos, permissões, bases, serviços. Mundo só com as nossas facções; Hegemony é a camada de estratégia.
- **Etapas 1, 2 e 3 concluídas.** Etapa 3 (tecnologia): 3a árvore em 6 abas e trava fora da bancada; 3b licenças por cargo; 3c engenharia reversa; 3d dados antigos (manual e disco do Velho Mundo); **3e (2026-11-18)**: dízimo tecnológico da Forja (dados antigos + componentes → mech de trabalho no cargo 2, título de veículo seminovo no cargo 3) e auditoria `tech buildaudit` (0 de 59 prédios espaciais escapam da trava, então não precisou patch de móveis/energia).
- **Etapa 5a (2026-11-18):** eventos por potência em carta de escolha: auditoria genética da Hélice, inspeção de garantia da Forja, turnê do Coro, chamada de artigos do Silêncio (pagar, ceder, recusar; ignorar 3 dias = recusa).
- **Etapa 5d (2026-11-18):** Fungobi no jogo: facção oculta hostil, horda (incidente) e ruínas de assentamentos destruídos com moradores Fungobi.
- **Etapa 5f (2026-11-18):** eventos sem dono: tempestade de cinzas, abrigo que se abre (moradores entram com manual), comboio perdido (o Sindicato pode vir buscar), recall mecanoide.
- **Etapa 4a (2026-11-18):** sementes na Feira e como dízimo do Coro (órgãos já eram vendidos).
- **Etapa 4c (parte, 2026-11-18):** 4 resultados de ritual escolhíveis, um por potência (fama, favor, itens).
- **Etapa 11a (2026-11-18):** obrigações de cargo: 2 por potência (cargos 1–2 e 3–4), colono emprestado que volta sozinho, 3 faltas = rebaixamento de um cargo.
- **Etapa 5b (2026-11-18):** Fora de Linha, fase 2: Exposição perante a Forja, dados de diagnóstico como moeda, ofertas da rede (contrabando, apagar registro, sabotagem) e a proposta da Forja de trair a célula.
- **Etapa 5c (2026-11-18):** Não Catalogados, fase 2: expedição de coleta do Silêncio (com contra-ataque) e contrato de contenção com a Carne.
- **Etapa 5e (2026-11-18):** facções menores: greve do Sindicato com trégua real, liquidação da Feira, recolhimento de colmeia da Hélice, matéria-prima do Reino do Lodo.
- **Etapa 4b (2026-11-18, sem o Rimesis):** 8 zonas da Queda (recurso exclusivo do bioma do Alpha Biomes → 12% de projeto do ramo na bancada de engenharia reversa) e trincheiras/arame do Trenches nos cercos das potências e do Reino do Lodo. Corrigidos no caminho: mech como construtor de cerco (erro a cada tique) e os 6 mutadores de animais do VEE.
- **Etapa 5g (2026-11-18):** textos dos 3 VQE em PT-BR (`Mods/EcosDaQuedaTraducao`) e missões pedidas pela potência patrona (Silêncio: Ancients; Forja: drones e ARC) enquanto ela não for hostil (`QuestAskers.cs`).
- **Etapa 10a (ferramenta):** comando `perf` e linha de base em `tecnico/desempenho.md` (jogo novo ≈ 2400 tiques/s sem desenho; nada pesa ainda).
- **Etapa 8 (parte):** peso do líder nos grupos das potências 1 → 0,15; executor conta erros de carregamento do nosso mod; correção do VEE (predadores por bioma) em `ThirdPartyFixes.cs`.
- **Compatibilidade 1.7:** `mods/compat-1.7.md` (91 mods declaram 1.7; 29 só até 1.6, todos carregam e passam nos testes).
- **Etapa 8 (resto, 2026-11-18):** perfil lean aposentado; trava da volta ao menu no lote resolvida (era o autosave sem janela); spam do Hegemony no mundo novo (~7.400 erros, estourava o log) corrigido; livros de pesquisa voltaram a sortear projeto (abas da 3a); traduções oficiais PT-BR da Anomaly e da Odyssey consertadas na carga (tomos e livro-mapa). Novo roteiro `livros.txt`.
- **Testes:** regressão em lote, 44 testes, **24,6 min** sem nenhuma trava (antes 52 min); 43 sem erros, 1 intermitente de terceiros (nome da ideologia, abaixo).

## Não commitado

Bloco 4b (`Zones.cs`, `Sieges.cs`, `ThirdPartyFixes.cs`, `EQ_Zones.xml`, `EQ_Sieges.xml`, debug `WorldDebug.cs`, `tests/zonas-cercos.txt`); 5g (`QuestAskers.cs`, `asker`, `tests/pedintes.txt`); traduções da outra sessão (`Mods/EcosDaQuedaTraducao`, `traducao/`); Etapa 8: `ThirdPartyFixes.cs` (Hegemony, regras de tradução), `Research.cs` (livros), `HarmonyPatches.cs`; debug `TestRunner.cs` (autosave, vigia, log), `EQDebug.cs` (`books`); `scripts/run-test.ps1`, `regress.ps1`, `tests/livros.txt`; docs (STATUS, HISTORICO, PLANO, TESTES, CLAUDE.md, `mods/encaixe.md`).

## Próximo passo

**Etapa 8 concluída.** Próximo bloco que não depende do usuário: **11b administrar algo da potência** (posto avançado/cota com estado salvo e relatório periódico; ver `PLANO.md` › Etapa 11). Roteiro completo: `PLANO.md` › Roteiro.

## Precisa do usuário (✱)

- **Seamless World (10f):** descrever o bug que fez desativá-lo.
- **Mundo fixo (10b):** quanto varia entre partidas; mapa à mão ou semente escolhida.
- **Mod de desempenho (10c):** aprovar cada simplificação (depois de medir jogo longo; um save longo dele ajudaria: `run-test -Save <nome>` com `tests/perf.txt`).
- **Colmeia Erguida:** a ideologia (`.rid`); hoje provisória.
- Narrador (Etapa 6), qualidade de vida (Etapa 7), Rimesis (resto do 4b: ligar ao Silêncio ou remover): escolhas listadas no `PLANO.md`.
- Balanço de custos da tecnologia e campanha curta (3e): só jogando.
- Character Creator é temporário: tirar quando o usuário avisar.

## Como testar

- Sempre `-Profile full`. Regressão: `scripts/regress.ps1` (lote; `-Build` compila). Um teste: `scripts/run-test.ps1 -Script tests/x.txt -Profile full`. Vários numa abertura: `-Batch a.txt,b.txt`.

## Pendências técnicas

- Intermitentes de terceiros (raros; o log agora não corta mais, então a próxima ocorrência traz a pilha): nome da ideologia gerada (`r_ideoName` unresolvable, 1 vez no `mercados`), ideologia removida (Alpha Memes), MeatBurst do Alpha Genes, `BestowingCeremony`, VIE "Invalid pawn role", "ran out of path nodes", pawn de missão chegando fora do mapa (EdgeWalkIn), `AccessViolation` do Faction Territories 2.
- Teste e outra sessão ao mesmo tempo: o perfil `test-profile/full` é um só; duas sessões testando juntas dão "O RimWorld já está aberto".
- Reino do Lodo: brutas sem roupa e bases com poucos defensores (orçamento do CBF). Lojas com roupas genéricas; preços sem balanço; laser/bobina com CE.
- 13 pesquisas do VFE Security caem nos ramos só pelo nível.

## A conferir pelo usuário (só visual)

- Blocos de 2026-11-17/18 (cartas, telas e itens novos): 4b receitas "analisar … (zona)" na bancada de engenharia reversa e trincheiras/arame num cerco da Forja; 3d manual e disco; 3e dízimo tecnológico; 4c resultados de ritual; 5a eventos de potência; 5b rede e Exposição; 5c expedição e contrato; 5e greve, liquidação, colmeia, matéria-prima; 5d horda e ruínas Fungobi; 5f eventos sem dono; 11a obrigações e rebaixamento. Detalhe de cada um no `HISTORICO.md`.
- Se um def novo não aparecer no jogo depois de atualizar: apagar o cache do Missile Girl (ver TESTES.md).
- 3c: bancada de engenharia reversa, conta "fazer engenharia reversa" e mensagens. 3b: botão "Licenças" e janela; motivo da trava. 3a: tela de pesquisa com 6 abas.
- Itens antigos (lista completa no fim do `HISTORICO.md`): início sem modo dev, bases, permissões, serviços, marcos, Colmeia, Linha Exótica, veículos, bichos de lixão, androides, psycasts, "Saídos do abrigo", aba Reputação, unidades e comércio, ideologias.
- A fazer pelo usuário: xeno do líder da Forja, arte do retrato do narrador.
