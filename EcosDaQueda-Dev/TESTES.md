# Testes

## Testes automáticos (executor sem janela)

`scripts/run-test.ps1 -Script tests/<roteiro>.txt [-Build] [-Profile full] [-Save <nome>] [-Window] [-TimeoutSec N]`

- **Saída resumida por padrão:** só as linhas `!!`, `PROBLEMAS` e `RESULTADO` (mais as que casarem com `-Show '<regex>'`); `-Full` imprime o relatório inteiro. O relatório completo fica sempre em `tests/out/`: consultar com busca, não despejar na conversa.
- **Regressão inteira:** `scripts/regress.ps1 [-Build] [-Tests a,b] [-Long] [-Parallel N] [-Single]` roda os roteiros no perfil full e imprime uma linha por teste (`-Long` inclui `mundo`, ~20 min; rodar em segundo plano).
  - **Em lote (padrão, 2026-11-18):** o jogo abre uma vez só (`run-test.ps1 -Batch a.txt,b.txt` → `-eqbatch=<lista>` no executor) e roda os roteiros em sequência, voltando ao menu principal entre eles (o jogo anterior é descartado; erros ao fechar contam no teste que fechou). Economiza ~80 s de carga por teste. O tempo de cada linha é o do roteiro (sem a carga). **Vigia externo** (`run-test.ps1`): se um roteiro passa de `-TimeoutSec` (900 s; `mundo` 2400) + 120 s, ou o jogo fecha no meio, o relatório ganha `!! TIMEOUT EXTERNO`/`# RESULTADO: TRAVOU` e o lote reabre o jogo no roteiro seguinte (na 1ª regressão em lote o jogo congelou no 5º teste sem rodar quadros, e o timeout interno não dispara nesse caso). A volta ao menu entre testes **travava às vezes** (resolvido em 2026-11-18: era o autosave, cujo evento longo nunca roda sem janela; o executor desliga o autosave e, na volta ao menu, libera evento parado há 300 quadros; o vigia interno grava `# vigia:` no relatório a cada 30 s parado no menu ou no fechamento, e o contador do log é zerado a cada teste). Por segurança, continua assim: o executor grava o RESULTADO **antes** de voltar ao menu, e o vigia, ao ver um relatório completo parado por 90 s sem o próximo começar, mata o jogo e reabre no roteiro seguinte sem contar falha (linha `# (a volta ao menu travou…)`). Regressão inteira (33 testes) em 2026-11-18: **37 min** (antes 75). `-StallSec N` também mata se o relatório ficar N s sem linha nova. O log do Unity de cada abertura fica em `tests/out/lote-<perfil>-<n>.log` (cópia do `Player.log` do LocalLow). Teste sem relatório nenhum roda de novo sozinho no fim. `-Single` volta ao modo antigo (um jogo por teste).
  - **Decisão do usuário (2026-11-18): rodar só 1 jogo por vez** (a RAM fica no limite: ~4 GB livres com um jogo aberto). O `-Parallel` existe, mas não usar.
  - **`-Parallel 2`:** dois jogos ao mesmo tempo, cada um com seu perfil (`test-profile/full-1`, `full-2`, via `run-test.ps1 -Slot n`). Cada jogo usa ~2–2,7 GB e a máquina tem 12 GB: só usar com o resto fechado. O `Player.log` é um só para os dois jogos (o Waylith ignora `-logFile`): o resumo de ERROR/WARN do log fica misturado; os relatórios de cada teste não.
  - **Compilar com jogo aberto:** o Waylith trava as DLLs carregadas; o `-Build` (via `scripts/build-mods.ps1`) move a DLL travada para `test-profile/dll-trash` antes de compilar. O jogo aberto segue com a antiga; o próximo que abrir pega a nova.
  - **Jogo do usuário aberto (ex.: Character Creator):** não bloqueia o teste (perfis separados). Só um teste por perfil (`-Slot n` para outro). Memória: jogo do usuário + 1 teste cabe; jogo do usuário + `-Parallel 2`, não. O resumo do Player.log pode trazer linhas do jogo do usuário (o run-test avisa); o relatório do teste não.
  - Só `PROBLEMA` em maiúsculas conta como falha (`problemas=0` do `xenogen` é contagem). O `xenogen` aceita os androides `VREA_Android*` (Fora de Linha).
- Abre `RimWorld.exe -Headless -savedatafolder=<perfil isolado> -eqtest=<roteiro> -eqout=<relatório>`; o mod `EcosDaQuedaDebug` (`TestRunner.cs`) executa os comandos e fecha o jogo.
- Perfil único **full** (padrão): todos os mods ativos do usuário + o mod de debug. O perfil **lean** (só os mods essenciais) foi aposentado em 2026-11-18: as ideologias dos `.rid` dependem de memes/precepts de muitos mods e ele quebrava.
- Perfis ficam em `test-profile/` (descartável). Relatórios em `tests/out/<roteiro>.<perfil>.result.txt`. Linhas `!! LOG ERROR` são erros do log durante o teste; a última linha diz `RESULTADO: SEM ERROS` ou quantos erros.
- O executor desliga as janelas forçadas do mod (`EQLeader.SuppressDialogs`); `join <fac>` ainda força a vassalagem pelo caminho antigo (o líder recebe o 1º cargo).
- O roteiro começa com `newgame` (cria mundo + colônia, cenário padrão) ou `load <save>` (use `-Save <nome>` para copiar o save do usuário).
- Ruído ignorado pelo executor: "Could not find player faction" (Hegemony na criação do mundo), "Bad string pass when reading rule" (tradução PT-BR), "AsyncGPUReadback failed" e "Error in ParallelForEach … Combat Extended" (Combat Extended sem placa de vídeo; com muitos mods novos o perfil full sem janela acusa centenas destes: use `-Window` para a prova real).

| Comando | Faz |
|---|---|
| `wait 2500` / `wait 3h` / `wait 2d` | avança o tempo (ticks, horas ou dias de jogo) |
| `dump` | estado completo: colonos, lealdade e, por potência, relações, reputação, ideologia e preceitos (`~` = preceito invisível) |
| `rep <fac> fame/infamy <valor>` ; `rep <fac> add <fama> <infâmia>` | define ou soma reputação (add propaga às outras) |
| `goodwill <fac> <delta>` | muda a boa vontade pelo caminho normal (testa o patch de reputação) |
| `hostile <fac>` ; `peace <fac>` | torna a potência hostil / neutra com o jogador |
| `kill <fac>` | cria um membro e o mata com um colono como autor |
| `join <fac>/none/real` | força a lealdade (real = volta a ler do Hegemony) |
| `due stipend/tithe` | faz o estipêndio/dízimo vencer na próxima hora |
| `allegiance` ; `services` ; `service <defName>` | mostra lealdade; lista/usa serviços |
| `ceremony <fac>` ; `ceremony cancel` | solicita / cancela a cerimônia de submissão (mostra o motivo se bloqueada) |
| `quests` ; `quest <trecho>` ; `accept <trecho>` | lista missões; mostra o texto; aceita uma missão |
| `ceremony await [ticks]` ; `ceremony status` ; `ceremony start` | roda até o emissário ficar pronto; mostra a etapa; inicia o ritual com todos os colonos |
| `favor <n>` ; `favor event <tipo> [n]` | dá favor ao líder; simula um evento temático (ResearchFinished, SerumUsed, XenogermImplanted, TradeSilver, QualityCrafted, RitualFinished) |
| `leader show/set <i>/other/successor/kill/unassign` ; `renounce` | líder: mostra candidatos, nomeia, troca, define sucessor, mata, destitui; rompe o juramento |
| `transexport <pasta> all\|<ids>` | exporta (inglês) todo texto traduzível dos mods ativos de terceiros; ver TECNICO.md › Tradução. Rodar com `-Profile full -Exclude ecosdaqueda.traducao` |
| `deftext <TipoDeDef> <defName>` ; `language` ; `genegen <filtro>` ; `transerrors <filtro>` | tradução: label/descrição + de qual arquivo veio a injeção; idioma ativo; genes gerados por modelo; erros de carga/injeção |
| `letters` ; `choose <trecho do título> <nº da opção>` | lista cartas (com opções) e escolhe uma opção |
| `books [n]` | gera n livros de cada tipo com pesquisa (projetos sorteados, livros sem projeto) e confere regras de texto sem palavra-chave (RulePackDefs, projetos); lista as regras da tradução consertadas/removidas na carga |
| `give <ThingDef> <qtd>` ; `prisoner` | cria itens na área de casa; cria um prisioneiro |
| `serum <n>` | um colono toma n doses do Soro |
| `cecheck [armas\|melee\|roupas\|veiculos]` | compatibilidade com o Combat Extended: lista por mod o que não tem verbo CE, ToolCE, Bulk ou projétil CE (roteiro `tests/ce-compat.txt`, perfil full) |
| `movingbases` | assentamentos móveis (VEF MovingBase): dono, tile, parada/andando, estoque, se aceita negociar |
| `research <arquivo>` | grava todos os projetos de pesquisa (TSV: mod, nível, aba, custo, pré-requisitos, o que liberam) no arquivo; resumo por nível na saída (roteiro `tests/research.txt`) |
| `tech status` ; `tech try\|grant\|revoke\|finish\|current <projeto>` ; `tech work [pontos]` | Etapa 3: por ramo (total, concluídos, com acesso, podem começar, abas), projetos livres com pré-requisito travado, sobreposições; testa/libera/revoga acesso; conclui; define a pesquisa atual; simula pesquisa do 1º colono (roteiro `tests/pesquisa.txt`) |
| `tech license <projeto>` ; `tech buy <projeto>` ; `tech licenses` ; `tech craft <RecipeDef\|ThingDef>` | 3b: cargo exigido, licença (Title/Purchased/None), preço em favor, techprints; compra com favor; resumo por ramo; simula fabricar/construir e mostra a pirataria (roteiro `tests/licencas.txt`) |
| `tech datapool` ; `tech data <ThingDef>[:<projeto>]` ; `tech dataspawn <marcador>[:n]` | 3d: projetos que cada item de dados pode sortear, por ramo, e regras de geração; cria o item (projeto sorteado ou forçado) e o colono o lê (acesso, progresso, Infâmia); põe n marcadores perto do colono e roda a geração de dados neste mapa (sem sorteio de chance) (roteiro `tests/dados-antigos.txt`) |
| `tech buildaudit [arquivo]` ; `count mechs` | 3e: prédios construíveis espaciais+ sem projeto de ramo de potência (escapam da trava), tabela em `buildaudit.tsv`; mechs do jogador no mapa (roteiro `tests/fechamento-tech.txt`) |
| `fungobi` ; `fungobi horde [pontos]` ; `fungobi ruin` | 5d: estado da facção oculta; dispara a horda no mapa; gera a ruína de um assentamento destruído ao lado (moradores forçados), conta e descarta o mapa (roteiro `tests/fungobi.txt`) |
| `incident <IncidentDef>` ; `convoyclaim` | dispara um incidente no mapa (forçado); 5f: o próximo comboio perdido tem o dono vindo buscar (roteiro `tests/eventos-mundo.txt`) |
| `ritualoutcome [<def> [escala]]` | 4c: lista os resultados anexáveis de ritual nossos ou aplica um como se o ritual tivesse terminado (escala 1 = excelente, 0.5 = bom) (roteiro `tests/rituais.txt`) |
| `promo [refuse \| accept]` | 11d: recusa de promoção (liga/desliga), se o jogo acha que deveria haver cerimônia, último pedido e missões (roteiro `tests/promocao.txt`) |
| `encargo [offer \| report \| end [penalize] \| progress <FavorEventType> <n> [qualidade] \| skip <dias>]` | 11b: estado do encargo (período, andamento, faltas, oferta pendente); `offer` força a oferta, `report` fecha o período agora, `progress` simula eventos (ex.: `QualityCrafted 3 Good`), `skip` adianta o período (roteiro `tests/encargos.txt`) |
| `forces [war a b \| hegwar a b \| peace a b \| raise <potência> [inimigo] [n] \| step [n] \| arrive \| daily [n] \| call <potência> \| support <força> <moral> \| treasury <potência> <valor> \| garrison <facção> <n> \| log]` | 10d: guerras (Hegemony + forçadas), exércitos (força, moral, apoio, alvo, tiles), tesouros, última batalha. `war` força uma guerra sem o Hegemony, `hegwar` declara pelo Hegemony, `raise` levanta exército (n = força fixa, sem gastar tesouro), `step` avança n tiles, `arrive` põe no alvo e luta, `daily` roda n dias de renda/manutenção/mobilização, `call` manda a chamada às armas, `log` mostra os 30 últimos registros. 11c: `offer` (oferece o comando do exército da patrona), `posture <normal\|agressiva\|cautelosa>`, `pay`, `target [n]` (sem n lista), `retreat`, `unpaid` (zera o soldo). 10e: `frontchance <0..1>` (chance de frente da patrona; roteiros antigos usam 0), `played [off]` (batalha jogada no exército comandado), `front [visit \| expire \| kill attacker\|defender \| check \| side attacker\|defender\|none]` (gera o mapa como se uma caravana chegasse, vence o prazo, mata um lado, verifica o fim, toma partido como no menu da caravana). 10e 2º bloco: `hegfrontchance <0..1>`, `invasion <atacante> <defensor> [n]` (simula o evento `InvasionArrived` do Hegemony contra um assentamento do defensor), `invasion resolve attacker\|defender\|none` (simula o `InvasionResolved`). Membro de verdade só depois de `psy title <n> <potência>` (o `join` deixa a cerimônia pendente) |
| `forces tension [a b [valor]] \| tensionday [n] \| council calm\|inflame \| border [a b]` | 10d (resto): tensões entre potências (define e dispara os limiares 50/75/100), roda n dias de tensão e fronteira, responde ao conselho de guerra, abre frente de fronteira (sem par: em todas as guerras) |
| `newgame [cenário] [narrador] cov=0.25 seed=X preset=Nome_Com_Sublinhado` | Opções do mundo: cobertura, semente e preset do Worldbuilder (sublinhado vira espaço). Sem `preset=`, um cenário com "começar no mundo X" do Worldbuilder (os nossos, desde o M3b) usa o preset dele com a cobertura e a semente gravadas nele, como no jogo |
| `worlddump arquivo` / `worldgrid arquivo` / `worldconvert arquivo cLat cLon escala preset` | Conversor do mundo fixo (M2): despeja os tiles da região América (num jogo da Terra), a grade lat/lon do mundo atual, e converte para o mundo atual (25%), com prévia `.ppm` e preset salvo. Roteiros `mundo-m2a/b/c.txt` |
| `worldpaint arquivo` / `worldrivers [semente]` / `worldpreview arquivo.ppm` / `worldsave nome` | M2b: pinta biomas pela tabela (`mundo/pintura.tsv`), refaz rios e nomes de regiões, gera prévia e salva o preset do Worldbuilder |
| `frontier` / `frontierskip <dias>` | F2: por cidade, raio permitido R, teto (capital com `*`) e multiplicador do Hegemony; cidades novas. `frontierskip` avança o crescimento e as cidades novas n dias sem passar o tempo (depois `wait 1d` para o Hegemony recalcular). Roteiro `fronteira.txt` |
| `playertiles` / `frontiermark` / `frontiermarkage <dias>` / `tributenow` | F4: tiles do jogador; planta um marco de graça no tile válido mais longe da colônia; envelhece os marcos (demarcação, raio); um dia de rendimento do jogador e entrega do tributo na hora. O `frontier` mostra a colônia (riqueza, R, teto, marcos), os marcos e a sugestão de cidade. Roteiro `fronteira-jogador.txt` |
| `nation [show\|lock\|classic\|faith\|throne\|proclaim [i]\|bribe\|giveup\|revolt\|win\|clear\|lose [throne]\|retry\|found\|invasion]` | N1: estado da nação (etapa, falhas, espera, pontos, revoltosos, ideologias). O executor começa com a nação **fundada** (roteiros antigos); `lock` liga a trava. `throne` ergue uma sala do trono 5×5 com trono; `proclaim` proclama o candidato i (mostra a carta e as opções); `bribe` dá a prata e suborna; `revolt` traz a revolta na hora; `win` mata os revoltosos (vitória); `clear` mata os que sobraram; `lose` mata o líder, `lose throne` põe um revoltoso na sala do trono no limite das 6 h; `retry` zera a espera; `found` funda direto; `invasion` pergunta ao Hegemony se uma invasão de conquista contra a colônia passa. N1b: `classic` troca a ideologia do jogador por uma clássica (o executor não passa pela tela de ideologia, onde o patch faz isso nos cenários do Ecos); na vitória ela vira fluida com memes sorteados (sem janela); `faith` mostra o resultado (memes, líder devolvido, pontos de reforma). Roteiros `nacao.txt` e `nacao-fe.txt` |
| `council [show\|recruit [n]\|sit <cadeira> [i]\|leader <cadeira>\|empty <cadeira>\|skill <cadeira> <nível>\|cooldown\|down <cadeira>\|goodwill\|thoughts]` | N2: conselho (cadeiras `embaixador`, `general`, `conselheiro`) com perícia, % do bônus e fatores nos sistemas. `recruit` cria n colonos; `sit` senta o candidato i livre; `leader` tenta pôr o líder (recusa; nomeia um líder se não houver); `skill` muda as perícias da cadeira (o valor mostrado inclui os genes); `cooldown` zera as esperas; `down` derruba o conselheiro; `goodwill` pede +10 com uma potência e mostra o que entrou; `thoughts` mostra membro/preterido. Roteiro `conselho.txt` |
| `yield` / `yieldnow` | F3: rendimento do território por facção e pelas 2 maiores cidades (o que um dia entrega) com o estoque no Hegemony; `yieldnow` entrega um dia na hora. Roteiro `rendimento.txt` |
| `worldfixed` | Mundo fixo (M3): relatório da aplicação (cidades removidas/postas, marcos, mutadores, estradas) e as cidades por facção com tile, lat/lon, bioma e estrada. Mostra `semente_nova=` quando a semente fixa do preset foi trocada. Roteiros `mundo-m3.txt` e `mundo-m3b.txt` (cenário) |
| `territory` | Fronteiras vivas (F1): território do Hegemony por facção (tiles, cidades, distância máxima até a cidade, prosperidade e raio natural das 4 primeiras cidades). O território só existe depois dos primeiros dias (roteiro `territorio.txt`, ~4 min) |
| `worldinfo` / `biomes [arquivo]` | Mundo fixo (10b): tiles, cobertura, semente, assentamentos e se o Worldbuilder está ativo (com os presets); `biomes` grava a tabela de todos os BiomeDefs (mod, habitável, tiles no mundo atual). O Worldbuilder está no ModsConfig desde o M3b; o The Earth só no perfil de teste: `run-test.ps1 -Include allyjamy.terraearthodysseyv.wb` |
| `investigate [spawn <ThingDef> \| designate \| apply \| research <projeto> 0\|1]` | Investigação de estrutura: cria a estrutura perto da colônia, marca a ordem (os colonos fazem sozinhos), aplica direto, zera toda a pesquisa (0) ou conclui um projeto (1). Sem argumento: lições, fontes, designações, Energia elétrica e último resultado |
| `narrator [pick n \| canfire <IncidentDef>]` | 6a: estado do narrador (fim de jogo segurado/liberado, pontos e teto, pesos dos hostis); `pick` simula n escolhas de atacante (1000 pontos); `canfire` testa um incidente. Só com `newgame <cenário> EQ_Storyteller_Central` (o padrão do executor é a Cassandra) |
| `obligations [return]` | 11a: membro, cargo, faltas por potência, colonos emprestados (e quando voltam) e colonos no mapa; `return` antecipa a volta dos emprestados (roteiro `tests/obrigacoes.txt`; as obrigações são `PowerEventDef`, use `event`/`choose`) |
| `exposure [add N, drop]` | 5b: Exposição perante a Forja e relação com Forja e Fora de Linha; soma Exposição; `drop` gera e mata um técnico da Forja e conta os dados de diagnóstico que caíram (roteiro `tests/fora-de-linha.txt`) |
| `retaliation` ; `lastretaliation` | 5c: o próximo contra-ataque sorteado acontece com certeza; mostra o último agendado (roteiro `tests/nao-catalogados.txt`) |
| `truce <facção>` | 5e: se a facção está em trégua e se o sorteio de ataques ainda a aceita (roteiro `tests/menores-eventos.txt`) |
| `events` ; `event <PowerEventDef>` | 5a: eventos de potência e o motivo de cada um não poder aparecer; força a carta (ignora o intervalo) e lista as opções com o que falta (responder com `choose <parte do título> <n>`) (roteiro `tests/eventos-potencias.txt`) |
| `perf start` ; `perf report [arquivo]` | 10a: mede ms por tique do jogo, do mundo, do mapa e de cada componente de mod; ver `tecnico/desempenho.md` (roteiro `tests/perf.txt`, fora da regressão) |
| `tech rsources` ; `tech reverse <ThingDef>[:vida%]` ; `tech finishprereqs <projeto>` | 3c: itens que ensinam algo por ramo e o que fica fora do filtro da receita; cria o item (minificado se for prédio), confere o filtro e desmonta como a bancada (projeto, fração, progresso, acesso, Infâmia); conclui os pré-requisitos (roteiro `tests/engenharia-reversa.txt`) |
| `factions` | lista todas as facções do mundo: def, mod de origem, oculta, assentamentos, tecnologia, relação com o jogador |
| `faction <fac>` ; `raid <fac> [pontos] [bichos]` | descreve qualquer facção (líder, assentamentos, ideologia, relações fixas); força um ataque dela à colônia e conta quem veio e quantos animais têm lorde/dever; `bichos` usa só os grupos de combate com animais |
| `pawngen <PawnKind>` ou `pawngen fac:<nome> [n]` | gera n pawns (padrão 20) e conta traços (e se o traço forçado veio em todos: `faltou_traco`) e itens do inventário |
| `xenogen <PawnKind>` ou `xenogen fac:<nome> [n]` | gera n pawns (padrão 20) e conta xenotipo, padrão por baixo (endogenes) e gênero; marca PROBLEMA se elite sem padrão ou xeno de fora |
| `retaliate <fac>` ; `gift <fac>` ; `tribute <fac>` | força retaliação, presente, tributo |
| `signature <fac> [run]` | diagnóstico dos incidentes-assinatura |
| `incident <IncidentDef>` ; `save <nome>` ; `timeout <seg>` ; `quit` | utilidades |

`<fac>` = `silencio`, `helice`, `forja`, `coro` (ou o defName). Roteiros existentes: `smoke`, `ideo`, `fase2`, `fase2b`, `retaliacao`, `assinatura`, `fase3` (cerimônia, promoção, transferência, sucessão), `fase3-silencio`, `save-test` e `save-migracao` (estes dois com `-Profile full -Save Test`).

**Não coberto pelo executor:** tudo que é visual (aba Reputação, ícones, menus) e o contrato de contenção (precisa de entidade numa plataforma).

## Menu de debug do jogo

Com o modo desenvolvedor ligado (Opções), o menu de debug ganha a categoria **Ecos da Queda**: Fama/Infâmia +50, zerar reputação, forçar lealdade, vencer estipêndio/dízimo, tornar hostil/neutra, forçar retaliação/presente/tributo, matar membro, dar Soro/prata, criar prisioneiro, zerar recargas, relatório no log.

## Testes manuais

## Fluxo

1. Editar o mod em `Mods/EcosDaQueda`.
2. Validar XML: `scripts/validate-xml.ps1`.
3. **Fechar o jogo** antes de mexer em `ModsConfig.xml` (o jogo sobrescreve ao sair).
4. Abrir o jogo (`RimWorld.exe`) e esperar o menu. Carregamento com ~97 mods leva 1–3 min.
5. Ler o log: `scripts/check-log.ps1` (resume ERROR/WARN filtrando ruído conhecido).
6. Testes que exigem interface (criar mundo, ver aba Facções) **são feitos pelo usuário**, depois lê-se o log.

## Limitações conhecidas do ambiente

- **`RimWorld.exe -quicktest` não funciona** com esta lista de mods: o patch `VREAndroids.GeneUtility_GenesInOrder_Patch` acessa `Find.WindowStack`, que é nulo no quicktest → a geração de facções aborta e o log enche de NullReference. Num jogo criado pelo menu isso não ocorre.
- **Controle de tela (computer-use) não funciona no jogo**: o motor Waylith ignora cliques simulados. Não usar `open_application` para o RimWorld (abre instâncias extras).
- Só uma instância do jogo por vez (mods como Missile Girl travam o arquivo de log).

## Log

`%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Logs\Player.log` (anterior: `Player-prev.log`). Formato: `AAAA-MM-DD HH:MM:SS.mmm  NIVEL [Categoria] mensagem`, seguido de stack trace.

## Ruído conhecido (não é do nosso mod)

| Mensagem | Origem | Situação |
|---|---|---|
| `Knowledge data was missing key ...` | conceitos novos de mods | inofensivo |
| `GAGARIN: Cache ...` | Missile Girl | inofensivo |
| `[Vulkan] ... layer manifest` | driver | inofensivo |
| `Persistent allocation ... not freed before shutdown` | motor ao fechar | inofensivo |
| `[UnityStubs] hideFlags ... Stub!` | camada de compatibilidade | inofensivo |
| `Translation data for language Portuguese Brazilian has N errors` | traduções | inofensivo |
| `Could not find player faction` em `Rimesis...DoTend` | Rimesis, durante criação do mundo | inofensivo |
| `[Hegemony] Initial insect hive is preserved...` | Hegemony | aviso |
| `KeyNotFoundException` em `RimworldExploration.VisibilityManager` | Exploration Mode | **mod desativado** |
| `[VFED] ... Gun_ChargeLance_Unique ... contraband` | VFE Deserters | aviso |
| `Config error in Hygiene/Bladder: description has trailing whitespace` | Dubs Bad Hygiene | inofensivo (espaço sobrando no texto do mod) |
| `Faction_N is referenced but is not deep-saved` / `Could not resolve reference ... Faction_N` (curParent `CombatExtended.FactionStrengthTracker`) e `Thing_HumanN` (curParent `VanillaTraitsExpanded.TraitsManager`) | CE e Vanilla Traits Expanded guardando referências a expedições do Hegemony que já acabaram | inofensivo |
| `penAmount or armorAmount are zero for <Sharp/Blunt> on <roupa>` | CE: roupa sem nenhuma armadura atingida (ex.: headset do Biotech, capa Eltex do VPE); o dano segue igual | inofensivo; o executor ignora (2026-11-19) |
| `BoundMap(,)` / `CropVertical ... was not cached` | CE sem janela (texturas não lidas) | só no executor; ignorado |
| `A hidden ritual precept was missing, adding: ...` | nosso `exactPrecepts` (corrigido em 2026-11-14); pode aparecer uma última vez em saves antigos | o jogo recoloca sozinho |

## Histórico de problemas resolvidos

- Replace Stuff original (1.5) quebrava ao carregar → trocado por **Replace Stuff - Continued** (3526354009).
- Cherry Picker original (removido da Steam) → port 1.6 (3521312241), mesmo packageId.
- Faltavam dependências: Jaeger's Utility Framework (3825311621) e Vehicle Framework (3014915404).
- `VPE_Puppeteer` não existe → trocado por `VPE_Empath`.
- Nomes de assentamento esgotavam (Hegemony cria muitas cidades) → listas ampliadas com pesos e numeração.

- `tests/cerimonia-helice.txt`, `cerimonia-forja.txt`, `cerimonia-coro.txt` (full): submissão completa em cada potência; `leader show` mostra `nívelPsíquico` e `vínculoMecanoide` do líder.
- `newgame [ScenarioDef] [StorytellerDef]`: sem argumentos usa Crashlanded + Cassandra (como antes).
- `start`: narradores e cenários visíveis/ocultos, cenário e narrador em uso, xenos permitidos, xenos dos colonos, e o menu de xenotipos depois do filtro. Testes `tests/inicio.txt`, `tests/inicio-forja.txt`.
- `modusage [todos|<packageId>]`: por mod, quantos defs dele as nossas facções usam (unidades, armas e roupas por etiqueta, estoque de comércio, genes, memes, preceitos) e quanto conteúdo ele tem. Teste `tests/modusage.txt`.
- `leather <XenotypeDef>`: gera um pawn com o xeno, mata, carneia e lista o que rendeu (e os genes de couro). Teste `tests/fungobi.txt`.
- `tests/reino-lodo.txt`: facção, xenos (`xenogen`), armas (`kit`), ataques de 300 e 1500 pontos e a base medieval (`base`). `tests/ideo.txt` agora inclui o Reino do Lodo.
- `hive [ThingDef]` ; `hive nature [ThingDef]`: cria a colmeia (padrão `Hive`) e a mata com um colono como autor (ou sem autor, que não deve contar); mostra Fama da Hélice, favor do líder e o total de colmeias destruídas. Teste `tests/insetos.txt`.
- `trader <TraderKindDef> <filtro>`: além do resumo, lista os itens do estoque cujo defName contém o filtro.
- `count <ThingDef>`: quantidade no mapa inteiro, na área doméstica e cápsulas ainda caindo (`give` só conta a área doméstica, e estipêndio e permissões costumam cair fora dela).
- `base <fac>`: gera o mapa do assentamento livre mais próximo da facção, mostra o plano do CBF previsto (`Angriffsvorschau.Plan`), riqueza, paredes, torres, os prédios mais comuns, defensores por PawnKind e veículos; depois descarta o mapa. Teste `tests/bases.txt` (~70 s). O número de defensores varia com a riqueza sorteada.
- `permit [list]` ; `permit buy <defName>` ; `permit use <defName>`: permissões por cargo — título, favor, pontos, permissões do líder e quais estão disponíveis (+/-) e pessoas da potência no mapa; `buy` compra como na aba Permissões; `use` usa pelo menu de ajuda real (opção do `GetRoyalAidOptions` + alvo perto do líder). Teste `tests/permissoes.txt` (Forja: aço, aprendizes, cavaleiros, recall; Coro: tenores → sopranos, liteira). Reforços chegam de nave: contar só depois de ~2 h.
- `sanity` ; `sanity set <0–1>` ; `sanity research [dias]`: sanidade (Insanity) dos colonos, multiplicadores, traços, velocidade de pesquisa e voz em caução; define a de todos; simula N dias de pesquisa do colono mais estudado (mostra velocidade, sanidade antes/depois e progresso). Teste `tests/sanidade.txt`.
- `patron <QuestScriptDef>`: cria uma missão crua do tipo, aceita e conclui com sucesso; mostra o patrono, Fama dele e favor do líder. `drone [ThingDef]`: choca um drone do Drone Factory (padrão: o primeiro item com `CompDroneHatcher`) e mostra licença, boa vontade com a Forja e contadores. Teste `tests/missoes.txt` (cenário `EQ_Scenario_Abrigo`: confere o abrigo selado com `count`).
- `landmarks`: bloco N — total de marcos do mundo e os nossos (`EQ_Marco_*`): validade, quantos saíram, 5 nomes sorteados; PROBLEMA se inválido, sem descrição ou nome com regra não resolvida. Teste `tests/marcos.txt`.
- `zone list` ; `zone <zona>` (sufixo, ex. `Ossario`, ou defName): bloco 4b — receitas de zona (bioma, recurso, ramo, projetos possíveis, se estão na bancada); aplica uma análise com o primeiro colono (projeto, progresso, acesso, Infâmia). `siege <facção> place|real|count`: planta o cerco num ponto longe dos colonos e conta por def (desfaz depois); ataque real com estratégia de cerco; o que a facção tem no mapa (plantas, obras, prontos). Teste `tests/zonas-cercos.txt`.
- `asker <QuestScriptDef> [n]`: 5g — gera n missões, conta pelo texto se quem pede é a patrona, outra facção ou ninguém, e as remove; `trocas` = quantas vezes o nosso patch escolheu a patrona. Teste `tests/pedintes.txt` (inclui patrona hostil = sorteio original).
- `vehicle [status]` ; `vehicle build <VehicleDef>` ; `vehicle title <item>`: bloco M — `build` monta a planta pronta com um colono (`Frame.CompleteConstruction`) e mostra se é de combate, licença, boa vontade com a Forja e contadores; `title` usa um título de posse (ou mostra o bloqueio) e descreve o veículo entregue (vida média das peças, tanque). Teste `tests/veiculos.txt`.
- `android [status]` ; `android fab [awake]` ; `android spawn` ; `android awaken` ; `android recruit`: questão androide — licença do líder, boa vontade com a Forja e os Fora de Linha, Fama/Infâmia da Forja; `fab` fabrica numa estação de criação real (comum ou já desperto); `spawn` gera um androide comum da colônia e `awaken` o desperta; `recruit` traz um desperto dos Fora de Linha. Teste `tests/androides.txt` (também com licença: `join forja` + `psy title 2 forja`).
- `psy [show]` ; `psy title <n> [facção]` ; `psy grant` ; `psy gen <PawnKind> [n]`: caminhos psíquicos licenciados (VPE) do líder — caminhos destravados, pontos, e quais licenças ele pode usar (+/-); `title` põe o cargo n do Coro (0 tira; a lealdade repõe o cargo dela na hora seguinte); `grant` destrava os licenciados; `gen` conta os caminhos de n unidades geradas. Teste `tests/psy-coro.txt` (cerimônia, promoção, rebaixamento, saída da lealdade com `join none`).
- `kit <PawnKind|fac:nome> [n]`: gera n unidades e resume armas, quantas vieram sem arma, munição de reserva (CE) e roupas; acusa PROBLEMAS se mais da metade vier desarmada. Testes `tests/arsenal.txt`, `tests/arsenal-forja.txt`.
- `kitcatalog [arquivo]`: exporta o catálogo do editor de kits (`kits/kitcatalog.json`: roupas, armas, itens, materiais, xenos, unidades e facções). Roteiro `tests/kitcatalog.txt` (ou `python kits/kits.py catalog`, que também extrai os ícones).
- `roster <FactionDef> [UnitRosterDef]`: mostra os grupos da facção (quem vem em cada um e o peso), líderes e membro básico, e gera uma vez cada unidade do editor (`EQ_U_*`) conferindo xeno e kit. Com o 2º argumento, aplica antes um elenco de teste (`EQ_Elenco_Teste`, do mod de debug, `applyOnStartup` false). Teste `tests/kits.txt`.
- `kittest <KitCollectionDef> <PawnKind> [n]`: liga a coleção à unidade só nesta sessão, gera n pawns pelo gerador normal e confere se cada um saiu com as roupas e a arma da variação sorteada. Teste `tests/kits.txt` (usa `EQ_Kit_Teste`, do mod de debug, para não depender das coleções do usuário).
- `pesquisa`: confere as pesquisas do editor de árvore carregadas (cada desbloqueio passou a exigir o projeto; aba, ramo, posição, pré-requisitos) e aplica um projeto de teste criado só na memória a Bed, MealSimple, Plant_Rice, WoodPlankFloor e Anesthetize (e restaura no fim). Confere a **coerência planta × produto** (item liberado por uma árvore cujo ingrediente só vem de planta que a cadeia dele não libera; ignora lenha e receitas alternativas) e as absorvidas (`absorvidas: N, ainda exigidas 0, visíveis 0` e o que cada nó absorveu). Roteiro `tests/arvore-pesquisa.txt`. (O roteiro `tests/pesquisa.txt` é outro: ramos e licenças.)
- `agro`: levanta o que serve para cultivar (fora as plantas) e grava `tests/out/agro.tsv` (roteiro `tests/agro.txt`).
- `animais`: idem para animais (camas, curral, comida, abate, cirurgias em animais), `tests/out/animais.tsv` (roteiro `tests/animais.txt`).
- `inicio`: pesquisas que cada cenário do mod já começa concluídas (facção, cenário, memes → `tests/out/inicio.tsv`) e, no jogo atual, confere 0 concluídas, o kit livre (`kits/pesquisa/Fundamentos.json`, campo `kit`) disponível e a árvore Fundamentos travada. Roteiro `tests/inicio-pesquisa.txt` (o `tests/inicio.txt` é outro: cenário e narrador).
- `weapontags [roupas]`: todas as armas (ou roupas) com mod, nível tecnológico, valor e etiquetas de sorteio. Teste `tests/weapontags.txt`.
- `trader [TraderKindDef]`: gera o estoque (sem argumento, de todos os `EQ_*`) e resume armas, roupas, munição, itens de maior valor e os geradores tirados da cópia. `tradeinfo`: itens comerciáveis por mod, categoria e etiqueta. Teste `tests/comercio.txt`.
- Ruído conhecido de terceiros (intermitente): `Invalid pawn assigned to ... role` (Vanilla Ideology Expanded com ideologia sorteada do jogador).

## Cache de XML do Missile Girl (GAGARIN)

O mod de desempenho Missile Girl guarda o XML já montado (`<perfil>/MissileGirl/Cache`). Se o cache foi montado quando um tipo C# nosso ainda não existia, os defs que dependem dele **somem sem erro** nas aberturas seguintes (o XML não mudou, então o cache não é refeito). O `run-test.ps1` apaga o cache do perfil de teste quando alguma DLL nossa é mais nova que ele. **No jogo do usuário** pode acontecer o mesmo depois de atualizar o mod: se um def novo não aparecer, apagar `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\MissileGirl\Cache`.
