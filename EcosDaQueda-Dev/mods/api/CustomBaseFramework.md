# Referência de API: Custom Base Framework

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (20)

### `CustomBaseFramework.BuildableRuleDef` : Def

- `fileContains` (List<string>) = `new List<string>()`
- `category` (string)
- `categoryLabel` (string)
- `order` (int)
- `rank` (int)

### `CustomBaseFramework.CropListDef` : Def

- `plants` (List<CropEintrag>) = `new List<CropEintrag>()`

### `CustomBaseFramework.DungeonProfileDef` : Def

- `style` (StyleDef)
- `mapSize` (int) = `175`
- `usable` (bool) = `true`

### `CustomBaseFramework.EaveFamilyDef` : Def

- `straight` (string)
- `outerCorner` (string)
- `innerCorner` (string)
- `stuffCategories` (List<StuffCategoryDef>) = `new List<StuffCategoryDef>()`
- `fallback` (bool)

### `CustomBaseFramework.FactionFlagDef` : Def

- `faction` (string)
- `objects` (List<string>) = `new List<string>()`

### `CustomBaseFramework.FloorListDef` : Def

- `floors` (List<FloorListEntry>) = `new List<FloorListEntry>()`
- `Usable` (bool) = `> summe > 0f`

### `CustomBaseFramework.HelpDef` : Def

- `images` (List<string>) = `new List<string>()`
- `imageWidth` (float)

### `CustomBaseFramework.LandmarkPlanDef` : Def

- `commonality` (float) = `1f`
- `nameMaker` (RulePackDef)
- `icon` (string)
- `plan` (SettlementPlanDef)
- `ruin` (RuinDef)
- `styleFaction` (FactionDef)
- `techLevel` (TechLevel) = `(TechLevel)3`
- `loot` (bool) = `true`
- `biomes` (List<BiomeDef>)
- `biomesExcluded` (List<BiomeDef>)
- `minHilliness` (Hilliness)
- `maxHilliness` (Hilliness)
- `temperature` (FloatRange) = `new FloatRange(-999f, 999f)`
- `coast` (Landmarkenlage)
- `river` (Landmarkenlage)
- `mutator` (TileMutatorDef)
- `landmark` (LandmarkDef)

### `CustomBaseFramework.LootBuildingCategoryDef` : Def

- `members` (List<LootMember>) = `new List<LootMember>()`
- `kind` (LootDarstellung)
- `pawnList` (string)
- `pawnChance` (float)
- `pawnHostile` (bool)
- `pawnCount` (IntRange) = `new IntRange(1, 1)`
- `pawnFaction` (string)
- `resolved` (List<ThingDef>) = `new List<ThingDef>()`
- `aufgeloesteEinheiten` (PawnListDef)

### `CustomBaseFramework.LootTableDef` : Def

- `wdBudget` (bool)
- `rolls` (IntRange) = `new IntRange(1, 3)`
- `rollsOverDepth` (SimpleCurve)
- `rollChance` (float) = `1f`
- `uniqueEntries` (bool)
- `maxEntriesPerResolve` (int) = `16`
- `emptyFallback` (string)
- `always` (List<LootEntry>) = `new List<LootEntry>()`
- `entries` (List<LootEntry>) = `new List<LootEntry>()`
- `resolvedFallback` (LootTableDef)
- `usable` (bool)

### `CustomBaseFramework.MapStepsDef` : Def

- `suppress` (List<string>) = `new List<string>()`
- `fromTechLevel` (List<Techschritt>) = `new List<Techschritt>()`

### `CustomBaseFramework.MaterialListDef` : Def

- `wert` (ResolvedRole)
- `forPieces` (HashSet<string>)
- `district` (Stadtteilart?)
- `stuff` (ThingDef)
- `mapStone` (bool)
- `mapWood` (bool)
- `parent` (MaterialListDef)
- `roles` (List<MaterialRole>) = `new List<MaterialRole>()`
- `usable` (bool) = `true`
- `failures` (List<string>) = `new List<string>()`

### `CustomBaseFramework.PawnListDef` : Def

- `members` (List<PawnListEntry>) = `new List<PawnListEntry>()`
- `role` (Pawnrolle)
- `radius` (float) = `4f`
- `techLevel` (TechLevel)
- `Nutzbar` (bool) = `> summe > 0f`

### `CustomBaseFramework.PieceCategoryDef` : Def

- `blockMax` (int) = `-1`
- `merge` (bool?)
- `parent` (PieceCategoryDef)
- `zones` (List<string>)
- `needsRoadAccess` (bool?)
- `weight` (float?)
- `factions` (List<string>)
- `excludeFactions` (List<string>)
- `withIdeo` (List<string>)
- `withoutIdeo` (List<string>)
- `wallContact` (bool?)
- `annexes` (List<PieceCategoryDef>)
- `annexChance` (float?)
- `annexMax` (int?)
- `annexMultiple` (bool?)
- `annexFill` (bool?)
- `annexOverhang` (int?)
- `annexCornerPath` (bool?)
- `wallAccess` (bool?)
- `wallPart` (WallPartKind?)
- `openAccess` (bool?)
- `farmhouse` (bool)
- `pieceType` (PieceType?)
- `dungeonPart` (DungeonPartKind?)

### `CustomBaseFramework.PieceDef` : Def

- `schemaVersion` (int)
- `categories` (List<PieceCategoryDef>) = `new List<PieceCategoryDef>()`
- `type` (PieceType)
- `tag` (PieceCategoryDef)
- `style` (StyleDef)
- `template` (bool)
- `outwardIsLow` (bool?)
- `drawnOutward` (string)
- `allowRotation` (bool) = `true`
- `allowMirror` (bool) = `true`
- `clearFootprint` (bool) = `true`
- `eaves` (bool)
- `eavesMaterial` (string)
- `offIfMod` (string)
- `onlyWithMod` (string)
- `placement` (PlacementRules)
- `legend` (List<LegendEntry>) = `new List<LegendEntry>()`
- `mask` (List<string>)
- `terrain` (List<string>)
- `things` (List<string>)
- `things2` (List<string>)
- `things3` (List<string>)
- `plants` (List<string>)
- `roof` (List<string>)
- `anchors` (List<string>)
- `anchorsOver` (List<string>)
- `content` (List<string>)
- `contentZones` (List<ContentZoneSpec>) = `new List<ContentZoneSpec>()`
- `maskGrid` (PieceGrid)
- `terrainGrid` (PieceGrid)
- `thingGrid` (PieceGrid)
- `thing2Grid` (PieceGrid)
- `thing3Grid` (PieceGrid)
- `plantGrid` (PieceGrid)
- `roofGrid` (PieceGrid)
- `anchorGrid` (PieceGrid)
- `anchorOverGrid` (PieceGrid)
- `contentGrid` (PieceGrid)
- `Factions` (List<string>) = `> Regel.Liste(placement?.factions, Kette(), (PieceCategoryDe`
- `ExcludeFactions` (List<string>) = `> Regel.Liste(placement?.excludeFactions, Kette(), (PieceCat`
- … +11 campos (ver `CustomBaseFramework\PieceDef.cs`)

### `CustomBaseFramework.RuinDef` : Def

- `wallDemolition` (float)
- `clustering` (float) = `0.6f`
- `breaches` (IntRange) = `IntRange.Zero`
- `gateDestroyed` (float)
- `attackSide` (Rot4) = `Rot4.Invalid`
- `buildingDamage` (FloatRange) = `FloatRange.Zero`
- `turretsDestroyed` (float)
- `burnMarks` (float)
- `fires` (IntRange) = `IntRange.Zero`
- `rubble` (float)
- `lootFactor` (float) = `1f`
- `lootWithOccupiers` (float)
- `residentCorpses` (IntRange) = `IntRange.Zero`
- `attackerCorpses` (IntRange) = `IntRange.Zero`
- `rotDays` (FloatRange) = `FloatRange.Zero`
- `prisoners` (IntRange) = `IntRange.Zero`
- `fieldsRavaged` (float)
- `overgrowth` (float)
- `overgrowthTrees` (float) = `0.3f`
- `lightsOut` (bool)
- `townWallDecay` (float)
- `roofCollapse` (float)
- `livestockGone` (float)
- `occupation` (Besatzungsart) = `Besatzungsart.Occupier`
- `occupationChance` (float) = `1f`
- `creatures` (PawnListDef)

### `CustomBaseFramework.SettlementPlanDef` : Def

- `kind` (Ortsart)
- `styles` (List<StyleDef>) = `new List<StyleDef>()`
- `style` (StyleDef)
- `emplacements` (List<WallEntry>) = `new List<WallEntry>()`
- `gatehouses` (List<WallEntry>) = `new List<WallEntry>()`
- `walls` (List<WallEntry>) = `new List<WallEntry>()`
- `wards` (List<Stadtteil>) = `new List<Stadtteil>()`
- `levels` (List<DungeonLevel>) = `new List<DungeonLevel>()`
- `spread` (List<SpreadEntry>) = `new List<SpreadEntry>()`
- `resolvedEmplacements` (List<PieceDef>) = `new List<PieceDef>()`
- `resolvedGatehouses` (List<PieceDef>) = `new List<PieceDef>()`
- `coreSize` (IntRange) = `new IntRange(66, 74)`
- `mapSize` (int) = `175`
- `openTerrain` (bool)
- `towerCorner` (int) = `7`
- `roadMain` (int) = `2`
- `roadLane` (int) = `1`
- `minBuildingGap` (int)
- `maxBuildingGap` (int)
- `targetBuilt` (float)
- `treeShare` (float)
- `districts` (List<DistrictSpec>) = `new List<DistrictSpec>()`
- `budget` (List<BudgetEntry>) = `new List<BudgetEntry>()`
- `maxCivilians` (int) = `30`
- `maxSoldiers` (int) = `40`
- `vehicles` (IntRange) = `new IntRange(0, 3)`
- `defenderPoints` (FloatRange) = `new FloatRange(180f, 420f)`
- `wealthFactor` (float)
- `anchorPointShare` (float) = `0.5f`
- `defendersOnWall` (float) = `0.25f`
- `defenderGroupKind` (string)
- `ruin` (RuinDef)
- `factionTags` (List<string>) = `new List<string>()`
- `excludeFactions` (List<string>) = `new List<string>()`
- `techLevels` (List<TechLevel>) = `new List<TechLevel>()`
- `withIdeo` (List<string>) = `new List<string>()`
- `withoutIdeo` (List<string>) = `new List<string>()`
- `biomes` (List<string>) = `new List<string>()`
- `notBiomes` (List<string>) = `new List<string>()`
- `hilliness` (List<Hilliness>) = `new List<Hilliness>()`
- … +22 campos (ver `CustomBaseFramework\SettlementPlanDef.cs`)

### `CustomBaseFramework.StyleDef` : Def

- `fallback` (StyleDef)

### `CustomBaseFramework.UnitSwapDef` : Def

- `faction` (string)
- `priority` (int)
- `units` (List<Tauscheintrag>) = `new List<Tauscheintrag>()`
- `things` (List<Tauscheintrag>) = `new List<Tauscheintrag>()`
- `neutral` (List<string>) = `new List<string>()`

### `CustomBaseFramework.WallProfileDef` : Def

- `style` (StyleDef)
- `usable` (bool) = `true`
- `depth` (int) = `3`
- `outwardIsLow` (bool) = `true`
- `segments` (List<string>) = `new List<string>()`
- `tower` (string)
- `gatehouse` (string)
- `gatehouseOutward` (string) = `"South"`
- `resolvedTower` (PieceDef)
- `resolvedGatehouse` (PieceDef)
- `resolved` (PieceDef[])
- `vertical` (bool[])
- `outward` (bool[])

## DefModExtensions (`<modExtensions>`) (10)

### `CustomBaseFramework.CBFForeignFactionExtension` : DefModExtension

### `CustomBaseFramework.CBFOccupierExtension` : DefModExtension

- `occupiers` (List<FactionDef>) = `new List<FactionDef>()`

### `CustomBaseFramework.CBFRewardExtension` : DefModExtension

- `maxTechLevel` (TechLevel) = `(TechLevel)3`
- `forbiddenThings` (List<ThingDef>) = `new List<ThingDef>()`

### `CustomBaseFramework.CBFSiteExtension` : DefModExtension

- `ruin` (RuinDef)
- `plan` (SettlementPlanDef)
- `planTags` (List<string>) = `new List<string>()`

### `CustomBaseFramework.CBFStructureExtension` : DefModExtension

- `structure` (PieceStructure) = `PieceStructure.Furniture`

### `CustomBaseFramework.CBFTakeoverGeneratorExtension` : DefModExtension

### `CustomBaseFramework.FesteLage` : DefModExtension

### `CustomBaseFramework.LootContainerExtension` : DefModExtension

- `emptyGraphic` (GraphicData)
- `emptyBuilding` (string)
- `destroyWhenEmpty` (bool)
- `searchSound` (SoundDef)
- `searchEffect` (FleckDef)
- `effectSize` (float) = `1.5f`

### `CustomBaseFramework.SupportGraphic` : DefModExtension

- `top` (GraphicData)

### `CustomBaseFramework.UndeadExtension` : DefModExtension

- `bodyTexPath` (string) = `"Pawn/AncientDeadBody/AncientDead"`
- `headTexPath` (string) = `"Pawn/AncientDeadHead/Male_Average"`

## CompProperties de Hediff (1)

### `CustomBaseFramework.HediffCompProperties_Undead` : HediffCompProperties

- `bones` (ThingDef)
- `boneCount` (IntRange) = `new IntRange(2, 5)`

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (29)

### `CustomBaseFramework.Alarmlage` : MapComponent

### `CustomBaseFramework.Bauschau` : MapComponent

### `CustomBaseFramework.Brennerwache` : MapComponent

### `CustomBaseFramework.Building_LootContainer` : Building_Casket

- `tabelle` (string)
- `einheiten` (string)
- `einheitenChance` (float)
- `einheitenFeindlich` (bool)
- `einheitenFraktion` (string)
- `einheitenAnzahl` (IntRange) = `new IntRange(1, 1)`
- `tiefe` (int)
- `wohlstand` (float)
- `vorlage` (string)
- `vorlageLeer` (string)
- `stoff` (string)
- `abstiegPlaene` (List<string>)
- `abstiegChance` (float)

### `CustomBaseFramework.Building_MineSupport` : Building

### `CustomBaseFramework.CBF_FremdflussWorker` : CBF_RiverWorker

### `CustomBaseFramework.CBF_RiverDeltaWorker` : CBF_RiverVariante

### `CustomBaseFramework.CBF_RiverIslandWorker` : CBF_RiverVariante

### `CustomBaseFramework.CBF_RiverVariante` : CBF_RiverWorker

### `CustomBaseFramework.CBF_RiverWorker` : TileMutatorWorker_River

### `CustomBaseFramework.Garnisonsruhe` : MapComponent

### `CustomBaseFramework.GenStep_CBFSettlement` : GenStep

- `forcePlan` (SettlementPlanDef)
- `spawnPawns` (bool) = `true`
- `fallbackToVanilla` (bool) = `true`

### `CustomBaseFramework.GenStep_CBFStartRuin` : GenStep

### `CustomBaseFramework.GenStep_CBFTerrain` : GenStep

### `CustomBaseFramework.GenStep_CBFVerliesFels` : GenStep

### `CustomBaseFramework.HediffComp_Undead` : HediffComp

- `Props` (HediffCompProperties_Undead) = `> (HediffCompProperties_Undead)(object)base.props`

### `CustomBaseFramework.JobDriver_CBFClaimAnimal` : JobDriver

### `CustomBaseFramework.JobGiver_CBFFahrzeugAngriff` : ThinkNode_JobGiver

- `leine` (float) = `60f`
- `halt` (float) = `11f`

### `CustomBaseFramework.JobGiver_CBFFahrzeugStreife` : ThinkNode_JobGiver

- `radius` (float) = `18f`
- `pause` (IntRange) = `new IntRange(2500, 6000)`

### `CustomBaseFramework.Nachzeichnen` : MapComponent

### `CustomBaseFramework.QuestNode_CBFAttack` : QuestNode

- `site` (SlateRef<Site>)
- `attacker` (SlateRef<Faction>)
- `points` (SlateRef<float>)

### `CustomBaseFramework.QuestNode_CBFChoice` : QuestNode

- `inSignalDefeated` (SlateRef<string>)
- `inSignalGone` (SlateRef<string>)
- `outSignalFaction` (SlateRef<string>)
- `outSignalLoot` (SlateRef<string>)
- `site` (SlateRef<Site>)
- `faction` (SlateRef<Faction>)
- `lord` (SlateRef<Pawn>)
- `goodwillFaction` (SlateRef<int>)
- `goodwillLoot` (SlateRef<int>)
- `keyLabel` (SlateRef<string>)
- `keyText` (SlateRef<string>)
- `keyFaction` (SlateRef<string>)
- `keyLoot` (SlateRef<string>)

### `CustomBaseFramework.QuestNode_CBFClient` : QuestNode

- `storeAs` (SlateRef<string>)
- `storeFactionAs` (SlateRef<string>)
- `sitePart` (SlateRef<SitePartDef>)

### `CustomBaseFramework.QuestNode_CBFOccupier` : QuestNode

- `storeAs` (SlateRef<string>)
- `storeSitePartDefsAs` (SlateRef<string>)
- `sitePart` (SlateRef<SitePartDef>)
- `enemyOf` (SlateRef<Faction>)

### `CustomBaseFramework.QuestNode_CBFSite` : QuestNode

- `site` (SlateRef<Site>)
- `builder` (SlateRef<Faction>)
- `points` (SlateRef<float>)
- `ruin` (SlateRef<RuinDef>)
- `plan` (SlateRef<SettlementPlanDef>)

### `CustomBaseFramework.Reserve` : MapComponent

### `CustomBaseFramework.TileMutatorWorker_CBFLandmarke` : TileMutatorWorker

### `CustomBaseFramework.Wardkarte` : MapComponent

- `Leer` (bool) = `> felder == null`

### `CustomBaseFramework.WorkGiver_CBFClaimAnimal` : WorkGiver_Scanner

## Mod settings / ModBase (2)

### `CustomBaseFramework.CBFMod` : Mod

### `CustomBaseFramework.CBFSettings` : ModSettings

- `targetModPath` (string) = `""`
- `allesBaubar` (bool)
- `editorImArchitekt` (bool)
- `weltherrschaftUebernehmen` (bool) = `true`
- `riafHoldings` (bool) = `true`
- `worksitesBauen` (bool) = `true`
- `fahrzeugeBauen` (bool) = `true`
- `wdEigenerAnteil` (float) = `0.5f`
- `garnisonGrundanteil` (float) = `0.35f`
- `garnisonObergrenze` (int) = `60`
- `nachschubSchub` (int) = `4`
- `nachschubSekunden` (float) = `1f`
- `nachschubAbstand` (float) = `12f`
- `garnisonAnzeigen` (bool)
- `garnisonEingeklappt` (bool)
- `garnisonGroesse` (float) = `1f`
- `garrisonSize` (float) = `1f`
- `garnisonReserve` (bool) = `true`
- `garnisonLiegendeZaehlen` (bool)
- `fraktionenAus` (List<string>) = `new List<string>()`
- `editorModfilter` (List<string>) = `new List<string>()`
- `fraktionenGeprueft` (List<string>) = `new List<string>()`

