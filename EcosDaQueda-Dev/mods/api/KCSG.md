# Referência de API: VEF - KCSG (geração de estruturas)

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (5)

### `KCSG.SettlementLayoutDef` : Def

- `settlementSize` (IntVec2) = `new IntVec2(42, 42)`
- `samplingDistance` (int) = `8`
- `avoidBridgeable` (bool)
- `avoidMountains` (bool)
- `centerBuildings` (CenterBuildings)
- `peripheralBuildings` (PeripheralBuildings) = `new PeripheralBuildings()`
- `roadOptions` (RoadOptions) = `new RoadOptions()`
- `stuffableOptions` (StuffableOptions) = `new StuffableOptions()`
- `propsOptions` (PropsOptions) = `new PropsOptions()`
- `defenseOptions` (DefenseOptions) = `new DefenseOptions()`
- `stockpileOptions` (StockpileOptions) = `new StockpileOptions()`

### `KCSG.StructureLayoutDef` : Def

- `layouts` (List<List<string>>) = `new List<List<string>>()`
- `terrainGrid` (List<string>) = `new List<string>()`
- `foundationGrid` (List<string>) = `new List<string>()`
- `underGrid` (List<string>) = `new List<string>()`
- `tempGrid` (List<string>) = `new List<string>()`
- `terrainColorGrid` (List<string>) = `new List<string>()`
- `roofGrid` (List<string>) = `new List<string>()`
- `isStorage` (bool)
- `spawnConduits` (bool) = `true`
- `forceGenerateRoof` (bool)
- `needRoofClearance` (bool)
- `randomRotation` (bool)
- `randomizeWallStuffAtGen` (bool)
- `tags` (List<string>) = `new List<string>()`
- `modRequirements` (List<string>) = `new List<string>()`
- `spawnAt` (List<IntVec2>) = `new List<IntVec2>()`
- `MaxSize` (int) = `> maxSize`

### `KCSG.SymbolDef` : Def

- `spawnPartOfFaction` (bool) = `true`
- `thing` (string)
- `maxStackSize` (int) = `-1`
- `replacementDef` (ThingDef)
- `randomizeStuff` (bool)
- `stuff` (string)
- `color` (string)
- `styleCategory` (string)
- `rotation` (Rot4) = `Rot4.North`
- `fuelPercent` (float?)
- `powerPercent` (float?)
- `plantGrowth` (float) = `0.5f`
- `chanceToContainPawn` (float) = `1f`
- `containPawnKindAnyOf` (List<PawnKindDef>) = `new List<PawnKindDef>()`
- `containPawnKindForPlayerAnyOf` (List<PawnKindDef>) = `new List<PawnKindDef>()`
- `thingSetMakerDef` (ThingSetMakerDef)
- `thingSetMakerDefForPlayer` (ThingSetMakerDef)
- `crateStackMultiplier` (float) = `1f`
- `pawnKindDef` (string)
- `isSlave` (bool)
- `faction` (FactionDef)
- `numberToSpawn` (int) = `1`
- `spawnDead` (bool)
- `spawnRotten` (bool)
- `spawnFilthAround` (bool)
- `defendSpawnPoint` (bool)

### `KCSG.TileDef` : Def

- `tileLayouts` (List<StructureLayoutDef>)

### `KCSG.TiledStructureDef` : Def

- `tilesNumber` (int) = `4`
- `tilesNumberRange` (IntRange) = `new IntRange(1, 4)`
- `maxDistanceFromCenter` (int) = `1`
- `rectSizeIncrease` (int)
- `increaseTerrainDef` (TerrainDef)
- `placeTileAdjacent` (bool) = `true`
- `centerTileDefs` (List<TileDef>) = `new List<TileDef>()`
- `allowedTileDefs` (List<TileTagOtion>) = `new List<TileTagOtion>()`

## DefModExtensions (`<modExtensions>`) (4)

### `KCSG.BiomeStructGenExtension` : DefModExtension

- `structures` (List<LayoutCommonality>) = `new List<LayoutCommonality>()`
- `spawnCount` (int) = `1`
- `countScaleHiliness` (bool)
- `scalingOptions` (Scaling) = `new Scaling()`
- `canSpawnInMontains` (bool) = `true`
- `clearCellRadiusAround` (int)
- `canSpawnOnWaterTerrain` (bool) = `true`
- `onlyOnPlayerMap` (bool)
- `postGenerateOre` (bool)
- `maxMineableValue` (float) = `float.MaxValue`

### `KCSG.CustomGenOption` : DefModExtension

- `canSpawnSettlements` (bool) = `true`
- `chooseFromlayouts` (List<StructureLayoutDef>) = `new List<StructureLayoutDef>()`
- `chooseFromSettlements` (List<SettlementLayoutDef>) = `new List<SettlementLayoutDef>()`
- `tiledStructures` (List<TiledStructureDef>) = `new List<TiledStructureDef>()`
- `symbolResolver` (string)
- `tryFindFreeArea` (bool)
- `preGenClear` (bool) = `true`
- `fullClear` (bool)
- `preventBridgeable` (bool)
- `clearFogInRect` (bool)
- `symbolResolvers` (List<string>)
- `scatterThings` (List<ThingDef>) = `new List<ThingDef>()`
- `filthTypes` (List<ThingDef>) = `new List<ThingDef>()`
- `scatterChance` (float) = `0.4f`
- `scaleWithQuest` (bool)
- `UsingTiledStructure` (bool) = `> tiledStructures.Count > 0`
- `UsingSingleLayout` (bool) = `> chooseFromlayouts.Count > 0`

### `KCSG.FallingStructure` : DefModExtension

- `structures` (List<LayoutCommonality>) = `new List<LayoutCommonality>()`
- `thingsToSpawnInDropPod` (List<ThingDef>) = `new List<ThingDef>()`
- `canBeUsedBy` (List<FactionDef>) = `new List<FactionDef>()`
- `spawnDormantWhenPossible` (bool) = `true`
- `needToHaveSettlements` (bool) = `true`
- `skyfaller` (Type) = `typeof(KCSG_Skyfaller)`

### `KCSG.SpawnAtWorldGen` : DefModExtension

- `spawnCount` (int) = `1`
- `spawnPartOfFaction` (FactionDef)
- `parts` (List<SitePartDef>) = `new List<SitePartDef>()`
- `allowedBiomes` (List<BiomeDef>) = `new List<BiomeDef>()`
- `disallowedBiomes` (List<BiomeDef>) = `new List<BiomeDef>()`

## CompProperties de Thing (`<comps>`) (1)

### `KCSG.UndergroundRoom.CompProperties_UndergroundPassage` : CompProperties

- `cannotGoKey` (string) = `"KCSG.CantEnter"`
- `goKey` (string) = `"KCSG.Enter"`
- `mapLayouts` (List<StructureLayoutDef>) = `new List<StructureLayoutDef>()`

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (26)

### `KCSG.FallingStructureArrival` : PawnsArrivalModeWorker

### `KCSG.FallingStructureStrategy` : RaidStrategyWorker_ImmediateAttack

### `KCSG.GenStep_BiomeStructures` : GenStep

### `KCSG.GenStep_CustomStructureGen` : GenStep

- `fullClear` (bool)
- `clearFogInRect` (bool)
- `preventBridgeable` (bool)
- `spawnInRandomFreeLocation` (bool)
- `structureLayoutDefs` (List<StructureLayoutDef>) = `new List<StructureLayoutDef>()`
- `settlementLayoutDefs` (List<SettlementLayoutDef>) = `new List<SettlementLayoutDef>()`
- `tiledStructures` (List<TiledStructureDef>) = `new List<TiledStructureDef>()`
- `symbolResolvers` (List<string>) = `new List<string>()`
- `scatterThings` (List<ThingDef>) = `new List<ThingDef>()`
- `filthTypes` (List<ThingDef>) = `new List<ThingDef>()`
- `scatterChance` (float) = `0.4f`
- `scaleWithQuest` (bool)

### `KCSG.GenStep_EnnemiesPresence` : GenStep

- `forcedfaction` (FactionDef)
- `pointMultiplier` (float) = `1f`
- `spawnOnEdge` (bool)
- `defaultPointsRange` (FloatRange) = `new FloatRange(300f, 500f)`

### `KCSG.GenStep_Settlement` : GenStep_Settlement

### `KCSG.GenStep_WorldObject` : GenStep_Settlement

### `KCSG.SymbolResolver_DestroyRefuelableLightSource` : SymbolResolver

### `KCSG.SymbolResolver_EdgeDefenseCustomizable` : SymbolResolver

### `KCSG.SymbolResolver_GenerateRoad` : SymbolResolver

### `KCSG.SymbolResolver_RandomDamage` : SymbolResolver

### `KCSG.SymbolResolver_RandomFilth` : SymbolResolver

### `KCSG.SymbolResolver_RandomItemRemoval` : SymbolResolver

### `KCSG.SymbolResolver_RandomRoofRemoval` : SymbolResolver

### `KCSG.SymbolResolver_RandomTerrainRemoval` : SymbolResolver

### `KCSG.SymbolResolver_RemovePerishable` : SymbolResolver

### `KCSG.SymbolResolver_RoomGenFromStructure` : SymbolResolver

### `KCSG.SymbolResolver_RunResolvers` : SymbolResolver

### `KCSG.SymbolResolver_ScatterPropsAround` : SymbolResolver

### `KCSG.SymbolResolver_ScatterStuffAround` : SymbolResolver

### `KCSG.SymbolResolver_Settlement` : SymbolResolver

### `KCSG.SymbolResolver_SettlementPower` : SymbolResolver

### `KCSG.SymbolResolver_SingleThingRoofed` : SymbolResolver

### `KCSG.SymbolResolver_StorageZone` : SymbolResolver

### `KCSG.SymbolResolver_ThingSetOnlyRoofed` : SymbolResolver

### `KCSG.UndergroundRoom.JobDriver_InteractWithPassage` : JobDriver

