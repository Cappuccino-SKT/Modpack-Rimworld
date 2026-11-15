# Notas técnicas

## Documentação dos mods do modpack

Guias de frameworks, mecânicas e conteúdo, mais referência de API gerada das DLLs: `EcosDaQueda-Dev/mods/README.md`. Ferramentas: `scripts/mods-inventario.py` (inventário) e `scripts/mods-api.py` (referência de API a partir do descompilado).

## Ambiente

- **RimWorld 1.7.5000 rev1275**, cópia **não-Steam**, em `C:\Users\Felipe\Desktop\RimWorld`.
- Todas as DLCs: Royalty, Ideology, Biotech, Anomaly, Odyssey.
- **Motor Waylith** (o 1.7 trocou Unity por motor próprio, .NET 10 + Vulkan). A API do Unity é reimplementada em `UnityStubs.dll`; há camadas de compatibilidade para Harmony e Prepatcher (`RimWorldWaylith.PrepatcherCompat.dll`). DLLs de mods compiladas para 1.6 geralmente funcionam.
- Mods baixados via **SteamCMD anônimo** (`C:\Users\Felipe\steamcmd`). Lista: `scripts/steamcmd-mods.txt` (cópia de `C:\Users\Felipe\steamcmd\mods.txt`). Download: `C:\Users\Felipe\steamcmd\steamcmd.exe +runscript C:\Users\Felipe\steamcmd\mods.txt`, depois copiar `steamapps\workshop\content\294100\*` para `Mods\`.
- Pastas de mods ficam nomeadas pelo **ID da Workshop** (ex.: `Mods\3825320105` = Hegemony). O mod do projeto é `Mods\EcosDaQueda`.
- `Mods_desativados\` guarda versões antigas substituídas (Replace Stuff original, Cherry Picker original).
- Dados do usuário: `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\` (`Config\ModsConfig.xml`, `Logs\Player.log`, `Saves\`).
- .NET SDK 8 e 10 instalados; `ilspycmd` instalado como dotnet tool global (`%USERPROFILE%\.dotnet\tools`).

## Como o jogo escolhe pastas de versão (decompilado de `Verse.ModContentPack.InitLoadFolders`)

1. Se o mod tem `LoadFolders.xml` com versões definidas: usa a entrada da versão atual (`v1.7`); senão, a **maior versão ≤ atual** listada; senão `default`.
2. Sem LoadFolders: usa a pasta `1.7` se existir; senão a **maior pasta de versão ≤ 1.7**; depois `Common/` e a raiz.
3. `supportedVersions` sem 1.7 só gera **aviso amarelo** — o mod carrega normalmente.

Por isso mods sem suporte declarado a 1.7 carregam o conteúdo 1.6 automaticamente.

## Estrutura do mod

```
Mods/EcosDaQueda/
  About/About.xml            packageId ecosdaqueda.core, supportedVersions 1.6/1.7
  LoadFolders.xml            "/" sempre; "Mods/Hegemony" só se jaeger972.factionterritories2 ativo
  Defs/Memes/EQ_Memes.xml    4 memes-assinatura (exclusionTag EQ_SignatureMeme)
  Defs/Factions/EQ_Factions.xml   base abstrata EQ_MainFactionBase + 4 FactionDefs
  Defs/PawnKinds/EQ_PawnKinds.xml 16 PawnKindDefs
  Defs/RulePacks/EQ_Namers.xml    nomes de assentamentos (com pesos p= e numeração de reserva)
  Defs/Ideo/EQ_IdeoSymbols.xml    ícones (IdeoIconDef) e cores (ColorDef) das ideologias
  1.7/Assemblies/EcosDaQueda.dll  C# (só 1.7)
  1.7/Defs/EQ_IdeoTemplates.xml   modelos fixos de ideologia (IdeoTemplateDef)
  1.7/Defs/EQ_FactionRelations.xml  matriz de relações entre as potências
  1.7/Defs/EQ_Reputation.xml      reputação (Fama/Infâmia, títulos) + aba Reputação
  Mods/Hegemony/Defs/EQ_HegemonyDispositions.xml
  Mods/Hegemony/1.7/Assemblies/EcosDaQueda.Hegemony.dll  vetos do Hegemony
  Source/EcosDaQueda/             código C# principal (.csproj, .cs)
  Source/EcosDaQueda.Hegemony/    código C# da integração com o Hegemony
  Textures/EcosDaQueda/Icons/     ícones gerados
```

Convenções: prefixo **`EQ_`** em todos os defNames; textos em PT-BR; dependências opcionais sempre com `MayRequire="<packageId>"`.

## C# — `EcosDaQueda.dll`

- Projeto: `Mods/EcosDaQueda/Source/EcosDaQueda/EcosDaQueda.csproj`, **net10.0**, referencia `Assembly-CSharp.dll`, `UnityStubs.dll`, `Waylith.dll` da raiz do jogo (Private=false) e `0Harmony.dll` do mod Harmony (`Mods/2009463077/1.7/Assemblies`, Private=false). Patches Harmony em `HarmonyPatches.cs`/`Leadership.cs`.
- Build: `cd Mods/EcosDaQueda/Source/EcosDaQueda; dotnet build -c Release` → `Mods/EcosDaQueda/1.7/Assemblies/EcosDaQueda.dll`.
- `LoadFolders.xml` › `v1.7` inclui a pasta `1.7` (DLL + `1.7/Defs` com defs que usam classes do C#). No 1.6 o C# não carrega.
- **`IdeoTemplateDef`** + **`WorldComponent_IdeoTemplates`**: no `FinalizeInit` do mundo (fim da geração do mundo e ao carregar save), aplica o modelo uma vez por facção (guarda `appliedFactionIds` no save). Aplica: nome, adjetivo, membro, títulos, descrição travada (`descriptionLocked`), `SetIcon(icon, color)`, divindades (`IdeoFoundation_Deity.SetDeities`), preceitos (cria os que faltam, troca os da mesma issue, renomeia com `SetName` + `nameLocked`, `xenotype` para PreferredXenotype) e, com `exactPrecepts`, remove preceitos/cargos/rituais não listados (nunca remove `IdeoRole_Leader`). Renomeia o líder atual da facção (nome por gênero).
- O jogo só tem uma `IdeoFoundationDef` (`Deity`), então divindades sempre funcionam.
- A FactionDef usa `requiredPreceptsOnly=true` para gerar o mínimo; o modelo completa.

### Fase 2 — relações fixas e reputação

- **`FactionRelationPresetDef`** (`1.7/Defs/EQ_FactionRelations.xml`): pares de facções com `goodwill`, `label` e `permanentEnemies`. **`WorldComponent_FactionRelations`** aplica uma vez por mundo (flag `applied` no save; saves antigos recebem na 1ª carga) escrevendo `FactionRelation.baseGoodwill` e `kind` nos dois sentidos (≤-75 hostil, ≥75 aliado). A cada 60000 ticks reimpõe hostilidade (≤-80) entre `permanentEnemies`.
- **Reputação** (`ReputationSettingsDef` em `1.7/Defs/EQ_Reputation.xml` + `WorldComponent_Reputation`): `ReputationRecord{faction, fame, infamy}` salvo no mundo. Fontes (Harmony, `HarmonyPatches.cs`): postfix em `Faction.TryAffectGoodwillWith` (Δ>0 → Fama, Δ<0 → Infâmia, entre jogador e potência rastreada), `Faction.Notify_MemberDied` (instigador do jogador: +4 humano / +1 outro) e `Faction.Notify_MemberCaptured` (+3). Propagação: potências com boa vontade ≤ -50 com o alvo ganham Fama = 0,5×Infâmia causada e Infâmia = 0,25×Fama dada; com ≥ 50, Infâmia = 0,25×Infâmia causada. Níveis por `thresholds` (30, 150) → título em matriz 3×3 (`titles`/`titleDescriptions`).
- **Efeitos da reputação** (`tiers` = 9 `ReputationTier`, ordem Fama-linha × Infâmia-coluna): `priceGain` somado a `priceGain_FactionBase` via prefix em `TradeUtility.GetPricePlayerBuy/Sell` quando `TradeSession.trader.Faction` é rastreada (não aparece no detalhamento de preço do tooltip vanilla); a cada `checkIntervalDays` (3), por facção, com cooldown `minDaysBetweenEvents` (8, `ReputationRecord.lastEventTick`): `giftChance` (não hostil → `ThingSetMakerDefOf.Reward_ItemsStandard`, valor `giftMarketValue`, cápsula + carta), `tributeChance` (não hostil → prata `tributeSilver`), `raidChance` (hostil → `IncidentDefOf.RaidEnemy` forçado com `parms.faction`, `customLetterText`). Textos das cartas no XML com `{0}` = nome da facção (`{1}` = prata no tributo).
- **Lealdade/postos** (`Allegiance.cs`, `1.7/Defs/EQ_Allegiance.xml`): `FactionAllegianceDef` (ranks com `fameRequired`, `priceGain`, `stipend` [ThingDefCountClass]; dízimo `titheOptions` com `type` Silver/Things/Prisoner/Colonist/Refuse, `count` × posto, `fame`/`infamy`/`goodwill`). `EQAllegiance.PlayerOverlordProvider` é preenchido pelo módulo Hegemony via reflexão em `FactionFealty.Overlord(Faction)` (internal). `WorldComponent_Allegiance` checa a cada 2500 ticks: muda de membro (cartas de juramento/desligamento), promoção, estipêndio (cápsula), dízimo (`ChoiceLetter_EQTithe`, `LetterDef EQ_TitheLetter`, timeout → penalidade `ignoreInfamy/ignoreGoodwill`). Itens são tirados do mapa (área de casa, não proibidos) por `EQItems`; pessoas vão para a facção como world pawns (`KeepForever`).
- **Fase 3 — cargos, líder e cerimônia** (`Allegiance.cs`, `Leadership.cs`, `1.7/Defs/EQ_Titles.xml`, `EQ_Allegiance.xml`): substitui os postos por Fama.
  - Cargos = `RoyalTitleDef` vanilla (7 por potência, tag `EQ_Title_<potência>`); as FactionDefs ganharam `royalTitleTags`, `royalFavorLabel`, `royalFavorIconPath` e a herança padrão (`royalTitleInheritanceRelations` não pode ser nulo). O sistema de títulos da Royalty é genérico por facção; só a missão de outorga (`QuestNode_Root_BestowingCeremony`) é presa ao Império (PawnKinds e `Faction.OfEmpire`). `RoyalTitleDef` premiável exige `changeHeirQuestPoints`.
  - `FactionAllegianceDef.ranks` = benefícios por título (priceGain, stipend), na ordem; mais `ceremonyLabel/Text`, `promotionLabel/Text`, `emissaryKind`, `escortKind`, `joinThings/joinPrisoner/joinLeaderHediff`, `favorSources`.
  - Cerimônia: `QuestScriptDef EQ_Ceremony` + `QuestNode_Root_EQCeremony` (cópia do nó vanilla com nossos peões; reaproveita `QuestPart_BestowingCeremony`, `LordJob_BestowingCeremony` e `RitualOutcomeEffectWorker_Bestowing`). Prefix em `RoyalTitleUtility.GenerateBestowingCeremonyQuest` troca a missão para as potências (usa `root.TestRun`; `QuestScriptDef.CanRun` devolve false aqui). Prefix em `RitualOutcomeEffectWorker_Bestowing.Apply` cobra o preço na submissão (ou recusa) e põe um `PsychicAmplifier` no inventário do emissário (o vanilla faz `First()` e destrói um).
  - `EQCeremony.Request` dá ao líder o favor do 1º título → o vanilla gera a missão. `pendingPower` até concluir. Hora cheia: se o líder tem título numa potência → `Join` (chama `EQAllegiance.Submit`).
  - Favor só do líder: prefix em `Pawn_RoyaltyTracker.GainFavor` redireciona ao líder (ou bloqueia se não for membro); `EQFavor.Run` abre a exceção. `Faction.allowRoyalFavorRewards` só fica ligado para a potência do jogador. Fontes (`EQFavor.Notify`): postfix em `ResearchManager.FinishProject`, `CompQuality.SetQuality` (contexto Colony), `LordJob_Ritual.ApplyOutcome`, `GeneUtility.ImplantXenogermItem`, `TradeDeal.TryExecute`; mais Soro, dízimo (`TitheOption.favor`) e serviços (`FactionServiceDef.favor`).
  - Líder: `EQLeader` (cargo `leaderRole` da ideologia principal do jogador; candidatos = `Precept_Role.RequirementsMet`, senão o jogo destitui). `WorldComponent_Allegiance.LeaderTick` (250 ticks): transferência (`TransferTo`, com cache `cachedTitle/cachedFavor` para líder perdido), sucessão, `Dialog_EQChooseLeader` forçado. O Hegemony também troca o líder da facção do jogador sozinho ("Faction leadership changed").
  - Módulo Hegemony: `SubmitHandler`/`SubmitBlockHandler` por reflexão em `FactionFealty.Submit/Block` (internos); postfix em `Block` barra o juramento direto do jogador às potências (`EQAllegiance.AllowSubmit` abre a exceção).
- **Serviços** (`Services.cs`, `1.7/Defs/EQ_Services.xml`): `FactionServiceDef` (faction, minRank, cooldownDays, silver, requiresHeldEntity [`Building_HoldingPlatform.HeldPawn` não humano, destruído], requiresPrisoner; efeitos things / callAid [`IncidentDefOf.RaidFriendly` forçado com `parms.faction`] / researchPoints [`ResearchManager.AddProgress` no projeto atual] / colonistThought / fame). Recargas em `WorldComponent_Services` (dicionário defName→tick). Menu por `WorldComponent_Services.OpenMenu()` (FloatMenu).
- **O Soro**: `ThingDef EQ_Soro` (ParentName `MechSerumBase`, textura do healer tingida) com `CompProperties_UseEffectSerum` → `CompUseEffect_Serum`: remove xenogenes e `genes.SetXenotype`.
- **Retaliação-assinatura**: `FactionAllegianceDef.hostileIncidents` + `hostileIncidentChance/Label/Text`; `WorldComponent_Reputation.TrySignatureIncident` roda antes dos caçadores (incidente vanilla forçado + carta nossa).
- **Aba "Reputação"**: `MainButtonDef EQ_Reputation` → `MainTabWindow_Reputation` (ícone, nome, título com tooltip, relação com o jogador e com as outras potências, barras de Fama/Infâmia).
- Harmony: `EcosDaQuedaStartup` (`[StaticConstructorOnStartup]`) faz `PatchAll`. Referência a `Mods/2009463077/1.7/Assemblies/0Harmony.dll` (Harmony 3.0, net10).
- **Módulo Hegemony** (`Source/EcosDaQueda.Hegemony` → `Mods/Hegemony/1.7/Assemblies/EcosDaQueda.Hegemony.dll`, carregado só com Hegemony ativo via LoadFolders): registra `HegemonyApi.RegisterVeto(AIPeaceOrTreaty)` vetando paz/tratados entre `permanentEnemies`. Compilar **depois** do EcosDaQueda (referencia a DLL).
- **Facções proxy do Hegemony:** expedições/exércitos ("<Facção> expedition") são facções **ocultas e temporárias** com o **mesmo FactionDef** e mesma ideologia. Todo código que procura "a facção X" deve usar `EQUtil.MainFaction(def)` / `EQUtil.Canonical(f)` e ignorar `EQUtil.IsProxy(f)` (`temporary || Hidden`). Reputação de ações contra proxies é creditada à facção principal. (Bug encontrado em 2026-11-14: o mod criava registros de reputação para as expedições.)
- **`exactPrecepts` não remove preceitos invisíveis** (`!def.visible`): são rituais internos do jogo (julgamento, execução pública, nascimento, troca de função, discurso do trono, gravinave…). Removê-los gera "A hidden ritual precept was missing, adding: …" ao carregar.
- Guerras do Hegemony são um registro próprio (`DiplomacyLedger`), separado da hostilidade vanilla; não declaramos guerras pela API (ainda).

## Mod de debug — `Mods/EcosDaQuedaDebug` (`ecosdaqueda.debug`)

- `Source/EcosDaQueda.Debug.csproj` → `1.7/Assemblies/EcosDaQueda.Debug.dll` (referencia EcosDaQueda.dll e 0Harmony; compilar por último).
- `EQDebug.cs`: operações (reputação, lealdade, serviços, itens, cartas, dump) usadas pelo executor e pelo menu. `DebugActions.cs`: `[DebugAction("Ecos da Queda", ...)]` (namespace `LudeonTK`), listas com `Dialog_DebugOptionListLister`. `TestRunner.cs`: ativo só com `-eqtest=`; postfix em `Root_Entry.Update`/`Root_Play.Update` (máquina de estados Menu → Loading → Running → Done), postfix em `Log.Error` para contar erros, `TickManager.DoSingleTick()` em blocos de 200 ms para o `wait`, `Root.Shutdown()` ao fim.
- Jogo novo pelo executor: `Root_Play.SetupForQuickTestPlay()` + `PrepForMapGen()` + `PreMapGenerate()` na cena de entrada, depois `LongEventHandler.QueueLongEvent(null, "Play", ...)`. (O `-quicktest` puro quebra; este caminho funciona.)
- Opções do motor usadas: `-Headless` (sem janela), `-savedatafolder=<pasta>` (perfil isolado: Config/, Saves/), `-NoCrashDialog`. Argumentos `chave=valor` são lidos com `GenCommandLine.TryGetCommandLineArg`.
- `IncidentWorker.CanFireNow` devolve false para vários incidentes mesmo com `parms.forced`; usar `TryExecute` direto.

## Tradução PT-BR — `Mods/EcosDaQuedaTraducao` (`ecosdaqueda.traducao`)

Traduz mods de terceiros sem PT-BR sem tocar neles. Cada mod traduzido fica em `Mods/<packageId>/Languages/PortugueseBrazilian/` e o `LoadFolders.xml` (gerado) carrega a pasta só com `IfModActive`.

Fluxo (ferramenta `scripts/traducao.py`; textos-fonte em `traducao/`, versionados na `main`):
1. `run-test.ps1 -Script tests/trans-export.txt -Profile full -Exclude ecosdaqueda.traducao` → `trans-export/<id>/DefInjected/...` (inglês; usa a mesma travessia do limpador de traduções do jogo, `DefInjectionUtility`). A tradução precisa estar **desativada** na exportação, senão sai em português.
2. `python -I traducao.py dump <ids>` → `traducao/en/<id>.tsv` (+ Keyed em inglês do mod).
3. Traduzir em `traducao/pt/<id>.tsv` ou várias partes em `traducao/pt/<id>/*.tsv` (mesmas chaves). Item de lista sem tradução fica em inglês; chave ausente fica em inglês.
4. `python -I traducao.py build` → gera o mod e confere marcadores (`{0}`, `[PAWN_nameDef]`, `<color>`, `->`; pronomes podem mudar). `todo <id>` lista o que falta (ex.: depois de update do mod).
5. `tests/traducao.txt` (perfil full) confere injeção e erros.

Armadilhas descobertas:
- **Pastas de carga são camadas**: arquivos com o mesmo caminho relativo em pastas diferentes se sobrescrevem. Por isso o build prefixa cada arquivo com o packageId (`sarg_alphagenes__Genes.xml`).
- A pasta do tipo de def é achada por `GenTypes.GetTypeInAnyAssembly`: tipos de mods fora de `Verse`/`RimWorld` precisam do **nome completo** (`AlphaGenes.AnimalGeneTemplateDef`, `VEF.Pawns.PregnancyApproachDef`). O export já gera assim.
- Genes gerados por modelo (`*GeneTemplateDef`) pegam o texto traduzido do modelo; defs `generated` não são exportados.
- **`generalRules` de pesquisa dos mods VE sem regras no XML** (VPE, VRE Android) são preenchidas por código do VEF *depois* da injeção → não dá para traduzir por DefInjected ("Object reference not set"). Ficam fora; o `todo` continua listando essas chaves (é esperado).
- **Keyed com a mesma chave do jogo** (ex.: `NotAllowed`, `Hybrid` no Vehicle Framework) ficam fora: o PT do jogo já cobre e repetir dá chave duplicada.
- `deftext` aceita nome curto ou completo do tipo; com nomes repetidos (`AbilityDef` do jogo e do VEF) usa o tipo que tem o def.
- Não usar `Get-Content`/`Set-Content` do PowerShell 5.1 em arquivos UTF-8 sem BOM: lê como ANSI e estraga os acentos.
- A referência do Workshop (coleção 3018573588, baixada com SteamCMD) é fraca (tradução automática, pt-PT, termos errados): serve só de consulta; o texto é nosso (repo público).
- Glossário (seguir a Biotech oficial): baseliner = humano padrão, xenogerm = xenogerminador, germline = linhagem germinativa, hemogen = hemogênio, deathrest = descanso mortal, luciferium = lucifério, chemfuel = combustível químico, wastepack = pacote de resíduos tóxicos, shambler = trôpego, ghoul = carniçal, psylink = psínculo, "Carriers of this gene" = "Portadores deste gene". Royalty/psíquico: psyfocus = concentração psíquica, neural heat = calor neural, psycast = poder psíquico, psycaster = conjurador, skip/skipgate = salto/portal de salto, eltex = eltex. Caminhos do VPE: cronopata, senhor da estática, conflagrador, moldador do gelo, voz da natureza, harmonista, protetor, senhor da guerra, arcotecnicista, necropata, mestre do salto, tecnomante, empata, arconte, espreitador noturno. Veículos: tier = nível, pawn = personagem, c/s = q/s, tile = região; nomes de modelo (Bulldog, Tango...) ficam em inglês, menos Bote e Moto de trilha. Títulos imperiais: os oficiais (Império Fragmentado, arconte, dominus, arquistelar, janissário, catafrata).

## Ícones e cores próprios

- `Textures/EcosDaQueda/Icons/{Silencio,Helice,Forja,Coro}.png` — silhuetas brancas 256x256 **com contorno escuro** (legíveis no mapa-múndi), traço grosso, geradas por `EcosDaQueda-Dev/scripts/make-icons.ps1` (System.Drawing). Usadas como ícone da ideologia **e** da facção (`factionIconPath`).
- **Cor da facção** = `colorSpectrum` da FactionDef, calculada em tempo de execução (`Faction.Color` usa o def + `colorFromSpectrum` salvo) → mudar o XML altera mundos existentes. Usamos uma cor única (duas entradas iguais) e **clara**; cores escuras somem no mapa-múndi. Cores: Silêncio (0.72,0.68,0.95), Hélice (0.20,0.78,0.74), Forja (0.95,0.58,0.22), Coro (0.80,0.48,1.00); as `ColorDef` das ideologias usam os mesmos tons.
- `IdeoIconDef` com `<memes>` = meme-assinatura → só nossas ideologias podem sortear/usar.
- Cores: `ColorDef` (ParentName `IdeoColorBase`), **não** `IdeoColorDef`, para não entrar no sorteio de outras ideologias.

## Facções (FactionDef)

- Base `EQ_MainFactionBase` herda `OutlanderFactionBase` (comerciantes, visitantes, loot).
- `requiredCountAtGameStart=1`, `maxConfigurableAtWorldCreation=1`, `fixedIdeo=true`, `structureMemeWeights Inherit="False"`.
- Ideologia fixa = `fixedIdeo` + `ideoName` + `ideoDescription` + `forcedMemes` (estrutura incluída nos forcedMemes, como a Horax Cult faz).
- **`pawnGroupMakers Inherit="False"` é obrigatório**, senão as listas do pai são somadas.
- Mechs (Mech_Militor, Pikeman, Lancer, Scyther) entram direto nas `options` do grupo Combat da Forja.

## Ideologias fixas por arquivo (`Ideos/*.rid`)

- **Fonte:** o usuário monta a ideologia no editor do jogo e salva; o `.rid` vai para `EcosDaQueda-Dev/ideologias/rid/`. `ideologias/ideologias.json` só troca nomes e textos (identidade, descrição, divindades, sala de culto e preceitos por chave `"<def>|<nome atual>"`). `python -X utf8 -I EcosDaQueda-Dev/scripts/ideologias.py` gera `Mods/EcosDaQueda/Ideos/<saida>.rid` e avisa se uma chave não bate mais ou se sobrou nome com cara de automático.
- **Em jogo:** `IdeoTemplateDef.ideoFile` → `IdeoTemplateApplier.TryApplyFile`: `GameDataSaveLoader.TryLoadIdeo(caminho)` + `loaded.CopyTo(ideoDaFacção)` (mesmo caminho do botão "carregar" do editor). Mantém o objeto `Ideo` da facção, então os pawns já gerados continuam apontando para ele. Se o arquivo falhar, cai no modelo antigo por campos.
- **Cuidados:** `TryLoadIdeo` usa o `Scribe`; se um save está carregando (`Scribe.mode != Inactive`), a aplicação é adiada para o primeiro `WorldComponentUpdate`. Não chamar `Precept_Role.RecacheActivity()` na criação do mundo (ainda não há facção do jogador → NullReference). O `.rid` lista os mods no cabeçalho e referencia defs deles: sem o modpack completo, preceitos de mods ausentes dão erro.
- FactionDefs das menores usam `fixedIdeo` + `forcedMemes` coerentes com o arquivo, só para a ideologia provisória (antes da cópia) ser válida.
- Facção sem assentamento: `MinorFactionExtension.noSettlements` + postfix em `FactionGenerator.NewGeneratedFaction(PlanetLayer, FactionGeneratorParms)` (o jogo cria 1 assentamento para toda facção visível, mesmo com `settlementGenerationWeight` 0).

## Facções menores

`Defs/Factions/EQ_MinorFactions.xml`: herdam direto de `OutlanderFactionBase` (não de `EQ_MainFactionBase`), então não entram em ideologia fixa, cargos, `ReputationSettingsDef.factions` nem serviços. O que as liga às potências é só `EQ_FactionRelations.xml`.
- `FactionRelationPresetDef.Involves(def)` + `WorldComponent_Reputation.Record`: alvo que não é potência mas aparece nas relações fixas não ganha registro próprio, só propaga às potências pelo goodwill entre elas.
- Não usar `<hidden>true</hidden>`: `EQUtil.IsProxy` trata facção oculta como proxy do Hegemony (sem relações nem reputação).
- `MinorFactionExtension.playerGoodwill`: boa vontade inicial com o jogador, aplicada uma vez por facção em `WorldComponent_FactionRelations.ApplyPlayerGoodwill` (no tick, porque na criação do mundo a facção do jogador pode não existir). `FactionDef` não tem campo de boa vontade inicial.
- Herdar de `PirateBandBase` (o próprio def `Pirate` do jogo) traz também o que outros mods acrescentam aos piratas por patch (ex.: `VFEE_Deserter_Warlord`), porque patches rodam antes da herança. Por isso o Sindicato do Saque define `pawnGroupMakers Inherit="False"`.
- Ataque com poucos pontos falha (`executou=False` com 150): a facção precisa ter um grupo cujo pawn mais barato caiba nos pontos.
- **Facções de terceiros fora do mundo:** `EQWorldFactions.Apply()` (chamado no construtor estático de `EcosDaQuedaStartup`, depois dos defs) zera `requiredCountAtGameStart`, `startingCountAtWorldCreation` e `maxConfigurableAtWorldCreation` de todo `FactionDef` humano, visível, não-jogador e de fora do mod, menos os de `WorldFactionsDef.keepFactions`; `removeHiddenFactions` tira ocultas específicas. Saves antigos não mudam. Cuidado em testes: `Find.FactionManager.RandomEnemyFaction()` agora pode devolver a colmeia (não humana); usar `allowNonHumanlike: false`.
- **Missões/eventos de facção removida:** `WorldFactionsDef.disabledQuests` (zera `rootSelectionWeight` + prefix em todo `QuestScriptDef.CanRun`) e `disabledIncidents` (zera `baseChance` + prefix em `IncidentWorker.CanFireNow`/`TryExecute`). Necessário porque o Royalty dispara `GiveQuest_Intro_Deserter` mesmo sem Império e o `QuestNode_Raid` estoura com facção nula. As demais missões do Royalty usam qualquer facção com favor real, inclusive as nossas.
- **Assentamento móvel (Feira Andante):** `VEF.Planet.MovingBaseDef` (campos `baseFaction`, `ticksPerMove`, `initialSpawnCount`, `initialSpawnScalesWithPopulation`, `attackConfirmationMessage`); o VEF cria os objetos num postfix de `FactionGenerator.NewGeneratedFaction`. A classe `VFEMedieval.MerchantGuild` (VFE Medieval 2) dá o comportamento: para 5–30 dias (`StayInPlace`), viaja 20–30 tiles (`TryStartPathing`), é `ITrader` e gera mapa de ataque pelo `KCSG.CustomGenOption` do **FactionDef** (por isso a Feira tem essa extensão apontando para `VFEM2_MerchantFactionBase`). O `TraderKind` dela é fixo no código; `Patch_MerchantGuild_TraderKind` (por reflexão, só com o mod ativo) troca pelo `MinorFactionExtension.movingBaseTrader`. `noSettlementsIfMovingBase` zera o peso de assentamentos e remove o inicial só quando existe um MovingBaseDef para a facção.
- **Texto de def de terceiros em PT-BR:** patch de XML em `label`/`description` é sobrescrito pela tradução do jogo; usar `Languages/PortugueseBrazilian/DefInjected/<Tipo>/*.xml` no nosso mod (ex.: `EQ_Insect.xml`). Conferir com `deftext`.
- Ghoul de facção: PawnKind filho de `MutantBase` com `<mutant>Ghoul</mutant>` e `defaultFactionDef` nosso; bestas de carne (`Fingerspike`, `Toughspike`, `Trispike`) entram direto nas opções do `pawnGroupMaker`.
- Xeno sem `.xtp`: lista `genes` direto no `xenotipos.json`.

## PawnKinds

- Bases vanilla usadas: `MercenaryMidTierBase`, `MercenaryEliteBase`, `MercenaryEliteTierBase`, `UnderthrallBase` (Horaxian, Anomaly), `MechanitorBase` (Biotech).
- **`ParentName` usa o atributo `Name`, não o defName** (ex.: `MechanitorBase`, não `Mechanitor`).
- Psíquicos via VPE:
  ```xml
  <li MayRequire="VanillaExpanded.VPsycastsE" Class="VanillaPsycastsExpanded.PawnKindAbilityExtension_Psycasts">
    <implantDef>VPE_PsycastAbilityImplant</implantDef>
    <initialLevel>1</initialLevel>
    <statUpgradePoints>2~6</statUpgradePoints>
    <unlockedPaths><VPE_Warlord>1~4|2~10</VPE_Warlord></unlockedPaths>
  </li>
  ```
  Caminhos válidos: Empath, Skipmaster, Conflagrator, Protector, Warlord, Chronopath, Archotechist, Necropath, Staticlord, Archon, Frostshaper, Wildspeaker, Harmonist, Technomancer, Nightstalker. **Puppeteer e Animat estão comentados no VPE** (módulos separados não instalados).

## Hegemony (Faction Territories 2) — `jaeger972.factionterritories2`

- Namespace C#: `FactionTerritories2` (1.165 classes, decompilar com `scripts/decompile.ps1`).
- **Comportamento da facção** = soma de `FactionTerritories2.FactionDispositionDef` aplicados pelos **traços do líder** e **memes da ideologia** (campo `<meme>` ou `<trait>`). Campos: aggression, peace, diplomacy, trade, expansion, support, vassalise, plunder, raze + efeitos de assentamento (herdados de `SettlementEffectDef`: food, materials, prosperity, defence, research, recovery, growth, upkeep, construction, happiness, wood, stone, metal, components…) + `buildingTags` (tag/weight).
- Tags de construção conhecidas: animals, authoritarian, bionics, civic, civilian, culture, dangerous, defence, diplomacy, education, evil, food, genetics, good, industry, logistics, luxury, mech, medical, military, mining, nature, occult, organic, religious, royal, science, space, trade.
- **API pública** `FactionTerritories2.HegemonyApi`: `TryDeclareWar`, `TryMakePeace`, `TryTransferSettlement`, `AreAtWar`, `GetWars`, `GetOverlord`, `IsPlayerVassal`, `GetTerritoryClaims`, `GetTerritoryTileCount`, `SubscribeToEvents`, `RegisterVeto(HegemonyVetoKind, …)`, `RecordTrade`, preços/estoques de assentamentos… Base para a fase 2.
- **Jogador entrar numa facção:** sistema de fealty já existe ("Swear fealty to {0}?", "Become this faction's vassal"); o jogador vira vassalo aliado e paga tributo. Romper = traição permanente.
- Outros Defs do Hegemony: `CrisisTypeDef`, `SettlementEffectDef`, `BuildingDef`, extensões `SiteEffectExtension`, `RoadTechnologyExtension`, `MechanoidCrisisExtension`.

## packageIds úteis

| Mod | packageId |
|---|---|
| Harmony | brrainz.harmony |
| Vanilla Expanded Framework | OskarPotocki.VanillaFactionsExpanded.Core |
| Vanilla Psycasts Expanded | VanillaExpanded.VPsycastsE |
| VWE Laser / Coilguns / Bioferrite / Makeshift | VanillaExpanded.VWEL / VWEC / VWEBF / VWEMS |
| Vanilla Memes (VIE) | VanillaExpanded.VMemesE |
| Alpha Memes / Alpha Genes | Sarg.AlphaMemes / sarg.alphagenes |
| VRE Saurid / Lycanthrope / Hussar / Sanguophage / Fungoid | vanillaracesexpanded.saurid / .lycanthrope / .hussar / .sanguophage / .fungoid |
| Hegemony | jaeger972.factionterritories2 |
| Jaeger's Utility Framework | jaeger972.utilityframework |
| Combat Extended | CETeam.CombatExtended |

Para descobrir outro: `[xml]$x=Get-Content Mods\<id>\About\About.xml; $x.ModMetaData.packageId` (o grep simples pega o packageId das dependências por engano).

## Tags de armas úteis

`NerveSpiker`, `Flamethrower` (Incinerator), `VWEB_BioferriteWeapon`, `LaserGun`, `Coilgun`, `SpacerGun`, `MakeshiftGun`, `BeamGraserGun`, `GrenadeTox`, `EltexStaff`, `IndustrialGunAdvanced`, `Gun`.

## Xenotipos próprios (`Defs/Xenotypes/EQ_Xenotypes.xml`, gerado)

As potências usam **só xenotipos nossos** (`EQ_Xeno_*`); os do jogo e dos mods ficam fora das facções, do Soro e da ideologia.
- **Fonte:** os genes vêm dos `.xtp` salvos no editor de xenotipo do jogo pelo usuário, copiados para `EcosDaQueda-Dev/xenotipos/xtp/`; defName, textos, herança, `combatPowerFactor` e padrões ficam em `xenotipos/xenotipos.json`.
- **Gerar:** `python -X utf8 -I EcosDaQueda-Dev/scripts/xenotipos.py` na raiz do jogo (`--genes` lista os genes com rótulo e mod de origem). Cada gene de mod ganha `MayRequire` com o packageId de quem o define; genes de aptidão (`Aptitude*_<Skill>`, `VRE_AptitudePhenomenal_*`) são gerados por `GeneTemplateDef` e o script os reconhece pelo modelo. **Não editar o XML à mão.**
- **Padrões x elites:** padrões são hereditários (endogenes). Elites não são (xenogenes) e trazem `doubleXenotypeChances` com os padrões aceitos, somando 1; o `PawnGenerator` aplica o padrão e depois o elite por cima (`SetXenotype` não limpa endogenes), mecanismo vanilla do Sanguófago. Sem C# para NPCs.
- **Facções:** `xenotypeSet` da FactionDef só com padrões (vale para emissários, aprendizes, líderes sem xeno próprio e PawnKinds de outros mods). PawnKinds de elite usam `useFactionXenotypes=false` + `xenotypeSet` próprio somando 1 (o jogo completa a soma com Baseliner).
- **Soro:** sucesso troca só os xenogenes (o padrão fica); falha dá o Lote Descontinuado, hereditário, que limpa os endogenes antes (`EQXenotypeUtil.ClearEndogenesKeepingColors`, mantém cor de pele/cabelo).
- **Mundo todo:** `Patch_PawnGenerator_GenerateGenes` (Prefix, `Xenotypes.cs`) troca **qualquer xeno que não seja `EQ_`** (Baseliner e os de outros mods) por um padrão sorteado de `EQ_WorldXenotypes` (`Defs/Xenotypes/EQ_WorldXenotypes.xml`: Vagante 80%, Esquelético 15%, Antigo 5%). Exceções: lista `keepXenotypes` do mesmo def (Sanguophage, por causa das missões do Biotech; androides do VRE Android), `ForcedXenotype` e `ForcedCustomXenotype` (xeno personalizado do jogador). Os XenotypeDefs de terceiros continuam existindo (editor, xenogermes, preceitos); só não são mais gerados em pawns.
- **Teste:** `tests/xenotipos.txt` (perfil full); comando `xenogen <PawnKind|fac:nome> [n]` conta xeno, padrão (`EQXenotypeUtil.GuessBase`) e gênero.

## Xenotipos de terceiros disponíveis (não usados pelas potências)

Vanilla: Baseliner, Dirtmole, Genie, Hussar, Sanguophage, Neanderthal, Pigskin, Impid, Waster, Yttakin, Highmate, Starjack.
Alpha Genes: AG_Fleetkind, AG_Helixien, AG_Animusen, AG_Lapis, AG_Wretch, AG_Taukai, AG_Hiveling, AG_MindDevourer, AG_Efreet, AG_Drakonori, AG_Nereid, AG_Forsaken.
VRE: VRESaurids_Saurid, VREH_Uhlan, VRE_Poluxkin, VRE_Strigoi, VRE_Ekkimian, VRE_Bruxa, VREA_AndroidAwakened, VREA_AndroidBasic, VRE_Lowmate, VRE_Fungoid, VRE_Wolfman, VRE_Lycan, VRE_Boarskin, VRE_Insector, VRE_Ocularkin, AG_Mycormorph, AG_Malachai.

## Fase 2 (C#) — notas de preparação

- Projeto alvo: referenciar `Assembly-CSharp.dll`, `UnityStubs.dll` e `0Harmony` do mod Harmony (v3.0 no 1.7). Verificar o TargetFramework que o Waylith aceita (Assembly-CSharp é net10.0; mods 1.6 em net472 também carregam).
- Saída sugerida: `Mods/EcosDaQueda/1.7/Assemblies/` (ajustar LoadFolders) ou `Assemblies/` na raiz.
- Hegemony: referenciar `Mods/3825320105/Assemblies/FactionTerritories2.dll` com `Private=false` e usar `HegemonyApi`.
