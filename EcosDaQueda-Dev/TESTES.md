# Testes

## Testes automáticos (executor sem janela)

`scripts/run-test.ps1 -Script tests/<roteiro>.txt [-Build] [-Profile lean|full] [-Save <nome>] [-Window] [-TimeoutSec N]`

- **Saída resumida por padrão:** só as linhas `!!`, `PROBLEMAS` e `RESULTADO` (mais as que casarem com `-Show '<regex>'`); `-Full` imprime o relatório inteiro. O relatório completo fica sempre em `tests/out/`: consultar com busca, não despejar na conversa.
- **Regressão inteira:** `scripts/regress.ps1 [-Build] [-Tests a,b] [-Long]` roda os roteiros no perfil full e imprime uma linha por teste (`-Long` inclui `mundo`, ~20 min; rodar em segundo plano).
- Abre `RimWorld.exe -Headless -savedatafolder=<perfil isolado> -eqtest=<roteiro> -eqout=<relatório>`; o mod `EcosDaQuedaDebug` (`TestRunner.cs`) executa os comandos e fecha o jogo.
- Perfil **lean** (padrão): Harmony, DLCs, VEF, VIE Memes, Alpha Memes, VPE, Jaeger Framework, Hegemony, Ecos da Queda e o mod de debug. Jogo novo pronto em ~25 s. Perfil **full**: todos os mods ativos do usuário.
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
| `give <ThingDef> <qtd>` ; `prisoner` | cria itens na área de casa; cria um prisioneiro |
| `serum <n>` | um colono toma n doses do Soro |
| `cecheck [armas\|melee\|roupas\|veiculos]` | compatibilidade com o Combat Extended: lista por mod o que não tem verbo CE, ToolCE, Bulk ou projétil CE (roteiro `tests/ce-compat.txt`, perfil full) |
| `movingbases` | assentamentos móveis (VEF MovingBase): dono, tile, parada/andando, estoque, se aceita negociar |
| `factions` | lista todas as facções do mundo: def, mod de origem, oculta, assentamentos, tecnologia, relação com o jogador |
| `faction <fac>` ; `raid <fac> [pontos]` | descreve qualquer facção (líder, assentamentos, ideologia, relações fixas); força um ataque dela à colônia e conta quem veio |
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
- `android [status]` ; `android fab [awake]` ; `android spawn` ; `android awaken` ; `android recruit`: questão androide — licença do líder, boa vontade com a Forja e os Fora de Linha, Fama/Infâmia da Forja; `fab` fabrica numa estação de criação real (comum ou já desperto); `spawn` gera um androide comum da colônia e `awaken` o desperta; `recruit` traz um desperto dos Fora de Linha. Teste `tests/androides.txt` (também com licença: `join forja` + `psy title 2 forja`).
- `psy [show]` ; `psy title <n> [facção]` ; `psy grant` ; `psy gen <PawnKind> [n]`: caminhos psíquicos licenciados (VPE) do líder — caminhos destravados, pontos, e quais licenças ele pode usar (+/-); `title` põe o cargo n do Coro (0 tira; a lealdade repõe o cargo dela na hora seguinte); `grant` destrava os licenciados; `gen` conta os caminhos de n unidades geradas. Teste `tests/psy-coro.txt` (cerimônia, promoção, rebaixamento, saída da lealdade com `join none`).
- `kit <PawnKind|fac:nome> [n]`: gera n unidades e resume armas, quantas vieram sem arma, munição de reserva (CE) e roupas; acusa PROBLEMAS se mais da metade vier desarmada. Testes `tests/arsenal.txt`, `tests/arsenal-forja.txt`.
- `weapontags [roupas]`: todas as armas (ou roupas) com mod, nível tecnológico, valor e etiquetas de sorteio. Teste `tests/weapontags.txt`.
- `trader [TraderKindDef]`: gera o estoque (sem argumento, de todos os `EQ_*`) e resume armas, roupas, munição, itens de maior valor e os geradores tirados da cópia. `tradeinfo`: itens comerciáveis por mod, categoria e etiqueta. Teste `tests/comercio.txt`.
- Ruído conhecido de terceiros (intermitente): `Invalid pawn assigned to ... role` (Vanilla Ideology Expanded com ideologia sorteada do jogador).
