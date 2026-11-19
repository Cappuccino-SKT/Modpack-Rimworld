# Status

_Só o estado atual e os próximos passos. Manter curto (até ~60 linhas). Detalhe do que já foi feito: `HISTORICO.md` (buscar, não ler inteiro)._

## Onde estamos (2026-11-19)

- **Base pronta e testada:** 4 potências (Silêncio, Hélice, Forja, Coro) e 6 facções menores (Fora de Linha, Não Catalogados, Feira Andante, Sindicato do Saque, Reino do Lodo, Colmeia Erguida), ideologias do usuário, xenos próprios, Fama/Infâmia, cerimônia, líder e cargos, permissões, bases, serviços. Mundo só com as nossas facções; Hegemony é a camada de estratégia.
- **Etapas 1, 2 e 3 concluídas.** Etapa 3 (tecnologia): 3a árvore em 6 abas e trava fora da bancada; 3b licenças por cargo; 3c engenharia reversa; 3d dados antigos (manual e disco do Velho Mundo); **3e (2026-11-18)**: dízimo tecnológico da Forja (dados antigos + componentes → mech de trabalho no cargo 2, título de veículo seminovo no cargo 3) e auditoria `tech buildaudit` (0 de 59 prédios espaciais escapam da trava, então não precisou patch de móveis/energia).
- **Etapas 4 e 5 (2026-11-18):** 4a sementes; 4b zonas da Queda e trincheiras nos cercos (sem o Rimesis); 4c resultados de ritual; 5a eventos por potência; 5b Fora de Linha fase 2; 5c Não Catalogados fase 2; 5d Fungobi; 5e eventos das menores; 5f eventos sem dono; 5g VQE em PT-BR e missões da patrona. Detalhe no `HISTORICO.md`.
- **Etapa 11a (2026-11-18):** obrigações de cargo: 2 por potência (cargos 1–2 e 3–4), colono emprestado que volta sozinho, 3 faltas = rebaixamento de um cargo.
- **Etapa 11b (2026-11-18):** encargos do 3º cargo em diante (Linha 7-B da Forja, filial da Hélice, sala de leitura do Silêncio, temporada da corte do Coro): metas por quinzena, relatório, renúncia e faltas; aparece na aba Reputação.
- **Etapa 11d (2026-11-19):** recusar promoções (botão na aba Reputação: fica no cargo e nas obrigações, favor continua acumulando) e encargo cumprido apaga uma falta.
- **Etapa 10d, 1ª parte (2026-11-19):** tropas das potências (`Forces.cs`): exércitos no mapa-múndi (força, moral, tesouro e manutenção), guarnições abstratas, mobilização pelas guerras do Hegemony, batalha abstrata (vitória com guerra formal = toma o assentamento pelo Hegemony), chamada às armas do vassalo (colono, soldo ou recusa = falta). Roteiro `tropas.txt`.
- **Etapa 11c (2026-11-19):** do 5º cargo, a patrona oferece o comando do exército (alvo, postura, soldo, recuo no mapa-múndi); vitória apaga falta, derrota/recusa contam falta. Roteiro `comando.txt`.
- **Etapa 10e, 1º bloco (2026-11-19):** frente de trincheira (`Fronts.cs`): exército comandado no modo "batalha jogada" (ou da patrona, 35%) abre uma frente diante do alvo; caravana entra num mapa com trincheiras do Trenches, dois lados, onda e avanço; sem visita, decide sozinha em 3 dias. Roteiro `frente.txt`.
- **Etapa 10e, 2º bloco (2026-11-19):** frentes também nas invasões do próprio Hegemony entre NPCs (evento `InvasionArrived`, um lado nosso, 50%, até 2 abertas; fecham com a invasão; a escaramuça não muda a cidade). Em toda frente a caravana escolhe apoiar atacante, defensor ou ficar de fora (Fama/boa vontade do apoiado; Infâmia e hostilidade do outro; favor só se for a patrona). Roteiro `frente-hegemony.txt`.
- **Etapa 10a (ferramenta):** comando `perf` e linha de base em `tecnico/desempenho.md` (jogo novo ≈ 2400 tiques/s sem desenho; nada pesa ainda).
- **Etapa 8 (parte):** peso do líder nos grupos das potências 1 → 0,15; executor conta erros de carregamento do nosso mod; correção do VEE (predadores por bioma) em `ThirdPartyFixes.cs`.
- **Compatibilidade 1.7:** `mods/compat-1.7.md` (91 mods declaram 1.7; 29 só até 1.6, todos carregam e passam nos testes).
- **Etapa 8 (resto, 2026-11-18):** perfil lean aposentado; trava da volta ao menu no lote resolvida (era o autosave sem janela); spam do Hegemony no mundo novo (~7.400 erros, estourava o log) corrigido; livros de pesquisa voltaram a sortear projeto (abas da 3a); traduções oficiais PT-BR da Anomaly e da Odyssey consertadas na carga (tomos e livro-mapa). Novo roteiro `livros.txt`.
- **Testes (2026-11-19):** regressão em lote, 50 testes, 25,9 min; 49 sem erros e 1 intermitente do Alpha Memes (corrigido depois por patch e conferido em 3 roteiros).

## Não commitado

Nada: commit e push de 2026-11-19 (main, mod/ecos-da-queda, -debug e -traducao, inclusive a tradução do VMemes). Resumo geral do projeto: `RESUMO.md`.

## Próximo passo

**10e concluído (2 blocos).** Próximo sem depender do usuário: a **Etapa 6/7** só no que não exige escolha (ver `PLANO.md`). Ideia em aberto: ligar a escaramuça das frentes do Hegemony à batalha dele (`TryContributeExternalForce`), se o usuário quiser. O resto do 10d (otimizar o Hegemony) espera a medição de jogo longo (10a). O **3f** espera o planejamento da nova árvore com o usuário. Balanço das tropas e das frentes só jogando. Roteiro completo: `PLANO.md` › Roteiro.

## Precisa do usuário (✱)

- **3f Pesquisa por manual (ideia nova, 2026-11-18):** toda pesquisa trancada até ter o manual; regras decididas em `PLANO.md` › Etapa 3 › 3f. Falta planejar com o usuário a **nova árvore de pesquisa** (a atual será desfeita) antes de começar.

- **Seamless World (10f):** descrever o bug que fez desativá-lo.
- **Mundo fixo (10b):** quanto varia entre partidas; mapa à mão ou semente escolhida.
- **Mod de desempenho (10c):** aprovar cada simplificação (depois de medir jogo longo; um save longo dele ajudaria: `run-test -Save <nome>` com `tests/perf.txt`).
- **Colmeia Erguida:** a ideologia (`.rid`); hoje provisória.
- Narrador (Etapa 6), qualidade de vida (Etapa 7), Rimesis (resto do 4b: ligar ao Silêncio ou remover): escolhas listadas no `PLANO.md`.
- Balanço de custos da tecnologia e campanha curta (3e): só jogando.
- Character Creator é temporário: tirar quando o usuário avisar.
- **Editor de kits (2026-11-18):** `python kits/kits.py` abre o editor no navegador (sem o jogo): coleções de kits fixos com variações sorteadas por peso, ícones de 2.766 itens, importa os bonecos salvos no Character Creator; botão "Gerar Defs" escreve `Defs/Kits/EQ_Kits.xml`. O usuário monta as coleções e liga cada uma às unidades no editor; depois conferir com `kittest`/`kit`. Aba **Unidades**: o usuário cria as unidades de cada facção (papel, pontos, xeno, grupos em que aparece, coleções de kit; rascunho ou pronta) e pode trocar as antigas nos grupos. Ainda **nenhuma unidade pronta nem coleção ligada**.

## Como testar

- Sempre `-Profile full`. Regressão: `scripts/regress.ps1` (lote; `-Build` compila). Um teste: `scripts/run-test.ps1 -Script tests/x.txt -Profile full`. Vários numa abertura: `-Batch a.txt,b.txt`.

## Pendências técnicas


- Intermitentes de terceiros (raros; o log agora não corta mais, então a próxima ocorrência traz a pilha): nome da ideologia gerada (`r_ideoName` unresolvable, 1 vez no `mercados`), ideologia removida (Alpha Memes), MeatBurst do Alpha Genes, `BestowingCeremony`, VIE "Invalid pawn role", "ran out of path nodes", pawn de missão chegando fora do mapa (EdgeWalkIn), `AccessViolation` do Faction Territories 2, capa Eltex do VPE sem valores do CE (`permissoes`).
- Teste e outra sessão ao mesmo tempo: o perfil `test-profile/full` é um só; duas sessões testando juntas dão "Já há um teste rodando no perfil" (usar `-Slot n`). O jogo normal do usuário aberto não bloqueia mais (2026-11-18); só divide o Player.log.
- Reino do Lodo: brutas sem roupa e bases com poucos defensores (orçamento do CBF). Lojas com roupas genéricas; preços sem balanço; laser/bobina com CE.
- 13 pesquisas do VFE Security caem nos ramos só pelo nível.

## A conferir pelo usuário (só visual)

- Blocos de 2026-11-17/18 (cartas, telas e itens novos): 4b receitas "analisar … (zona)" na bancada de engenharia reversa e trincheiras/arame num cerco da Forja; 3d manual e disco; 3e dízimo tecnológico; 4c resultados de ritual; 5a eventos de potência; 5b rede e Exposição; 5c expedição e contrato; 5e greve, liquidação, colmeia, matéria-prima; 5d horda e ruínas Fungobi; 5f eventos sem dono; 11a obrigações e rebaixamento; 11b cartas de encargo/relatório e a linha "Encargo" na aba Reputação; 11d botão "Recusar promoções" na aba Reputação; 10d/11c/10e exércitos no mapa-múndi (e os 5 botões do exército sob comando), a frente de trincheira (ícone, carta, mapa com trincheiras e os dois lados lutando; as 3 opções "Entrar na frente…" no menu da caravana; frente aberta por invasão do Hegemony) (ícone de caravana na cor da potência, texto ao selecionar), cartas "Guerra" e "Chamada às armas", "Em guerra" na aba Reputação. Detalhe de cada um no `HISTORICO.md`.
- Se um def novo não aparecer no jogo depois de atualizar: apagar o cache do Missile Girl (ver TESTES.md).
- 3c: bancada de engenharia reversa, conta "fazer engenharia reversa" e mensagens. 3b: botão "Licenças" e janela; motivo da trava. 3a: tela de pesquisa com 6 abas.
- Itens antigos (lista completa no fim do `HISTORICO.md`): início sem modo dev, bases, permissões, serviços, marcos, Colmeia, Linha Exótica, veículos, bichos de lixão, androides, psycasts, "Saídos do abrigo", aba Reputação, unidades e comércio, ideologias.
- A fazer pelo usuário: xeno do líder da Forja, arte do retrato do narrador.
