# Facções, ideologias, unidades, Hegemony e cerimônia

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

## Cerimônia de outorga e nível psíquico (VPE)

- O Vanilla Psycasts Expanded troca o trecho de nível psíquico de `RitualOutcomeEffectWorker_Bestowing.Apply` por `RitualOutcomeEffectWorker_Bestowing_Apply_Patch.ApplyTitlePsylink(pawn, oldTitle, newTitle)`: sem psylink, dá 1 nível e depois `newTitle.maxPsylinkLevel - oldTitle.maxPsylinkLevel`; ou seja, **1 nível mesmo com `maxPsylinkLevel` 0**.
- Nosso patch (`Patch_VPE_ApplyTitlePsylink`, por reflexão, só se o VPE existir) pula esse método para cargos de potência sem `FactionAllegianceDef.grantsPsylink`. Com `maxPsylinkLevel` 0,1,…,6 nos cargos do Coro o total fica 1 a 7.
- **Caminhos licenciados** (`Source/EcosDaQueda/Psycasts.cs`, tudo por reflexão): `AllegianceRank.psycastPath` (defName de um `PsycasterPathDef`) + `psycastLockedReason` opcional. No início, cada caminho licenciado recebe `ensureLockRequirement=true`, `ignoreLockRestrictionsForNeurotrainers=false` (psitreinador não contorna) e o `lockedReason`. Postfix em `PsycasterPathDef.CanPawnUnlock`: colono do **jogador** sem aquele cargo (ou acima, em qualquer título da potência) → falso; NPCs não são afetados (a geração do VPE usa o mesmo método). Postfix em `RitualOutcomeEffectWorker_Bestowing.Apply` destrava de graça (sem gastar ponto) os caminhos de todos os cargos que o líder tem; postfix em `Pawn_RoyaltyTracker.SetTitle` chama `PsycastUtility.RecheckPaths` (o VPE move para `previousUnlockedPaths` o que perdeu a licença e devolve quando ela volta; também roda em todo `DirtyCache` de hediffs).
- Coro: cargo 1 Protetor, 2 Empata, 3 Senhor da Estática, 4 Senhor da Guerra, 5 Conflagrador, 6 Arconte, 7 Hemosage (`VPEH_Hemosage`, que ainda exige o gene `Hemogenic`). Os demais caminhos do VPE ficam livres.
- `FactionAllegianceDef.ceremonyHediff` (+ `ceremonyHediffText`): implante posto no cérebro do líder ao fim da cerimônia (Forja: `MechlinkImplant`).

## Questão androide (VRE Android)

- `Source/EcosDaQueda/Androids.cs` + `1.7/Defs/EQ_Androids.xml` (`AndroidPolicyDef`): potência que licencia (`licensor`, Forja), índice do cargo que licencia (`licenseRank` 1 = técnico licenciado; vale qualquer colono livre com esse cargo ou acima), boa vontade da pirataria, do androide desperto e dos libertadores (`liberator`, Fora de Linha). Só mexe em **boa vontade**: o patch de `TryAffectGoodwillWith` já converte em Fama/Infâmia (1:1) e a repercussão manda Infâmia extra à Forja quando os Fora de Linha (inimigos dela) ganham.
- Ganchos (por reflexão; sem o VRE Android nada acontece): postfix em `Building_AndroidCreationStation.FinishAndroidProject` (pirataria; o prefixo olha o `curAndroidProject` para saber se sai desperto — desperto = nenhum `AndroidGeneDef.removeWhenAwakened`); postfix em `Gene_SyntheticBody.Awaken` (desperta sendo do jogador); prefixo+postfix em `Pawn.SetFaction` (androide já desperto passa a ser do jogador). `VREAndroids.Utils.IsAndroid/IsAwakened` por reflexão.
