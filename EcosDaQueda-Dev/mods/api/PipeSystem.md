# Referência de API: VEF - PipeSystem (redes de canos)

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (2)

### `PipeSystem.PipeNetDef` : Def

- `pipeNetClass` (Type) = `typeof(PipeNet)`
- `resource` (Resource)
- `pipeDefs` (List<ThingDef>)
- `designator` (DeconstructOption)
- `overlayOptions` (OverlayOption)
- `linkToRefuelables` (List<LinkOption>)
- `transferAmount` (float) = `100f`
- `convertAmount` (float) = `-1f`
- `noStorageAlert` (bool)
- `foggedNetAlert` (bool)
- `alertProofDefs` (List<ThingDef>) = `new List<ThingDef>()`
- `noLinkedGraphicDefs` (List<ThingDef>) = `new List<ThingDef>()`
- `enableOverflow` (bool) = `true`
- `productionToOverflowCapacityRatio` (float) = `2f`
- `overflowAmount` (float) = `1f`
- `resourceAmountFormat` (string) = `"F0"`
- `resourceAmountFormatWorkerType` (Type) = `typeof(ResourceAmountFormatWorker)`

### `PipeSystem.ProcessDef` : Def

- `pipeNet` (PipeNetDef)
- `thing` (ThingDef)
- `thingCategory` (ThingCategoryDef)
- `disallowedThingDefs` (List<ThingDef>) = `new List<ThingDef>()`
- `countNeeded` (float)
- `nutritionGetter` (bool)
- `ingredientIconOverride` (string) = `""`
- `onlySmeltable` (bool)
- `onlyFreshCorpses` (bool)
- `pipeNet` (PipeNetDef)
- `thing` (ThingDef)
- `workerClass` (Type) = `typeof(ResultWorker)`
- `_worker` (ResultWorker)
- `count` (int)
- `outputCellOffset` (IntVec3) = `IntVec3.Invalid`
- `outputStringOverride` (string) = `""`
- `result` (Result)
- `ingredients` (List<Ingredient>) = `new List<Ingredient>()`
- `destroyIngredientsOnStart` (bool)
- `destroyIngredientsDirectly` (bool)
- `ticks` (int) = `600`
- `ticksQuality` (List<int>)
- `wastePackToProduce` (int)
- `researchPrerequisites` (List<ResearchProjectDef>)
- `hideProcessIfNotNaturalRock` (bool)
- `rockToDetect` (ThingDef)
- `worksInSpace` (bool) = `true`
- `hideProcessIfNotNaturalFish` (bool)
- `stopProcessUnderGillRot` (bool)
- `results` (List<Result>) = `new List<Result>()`
- `resultToDetect` (ThingDef)
- `finishedColor` (Color) = `new Color(0.9f, 0.85f, 0.2f)`
- `lowProgressColor` (Color) = `new Color(0.4f, 0.27f, 0.22f)`
- `isFactoryProcess` (bool)
- `autoGrabFromHoppers` (bool)
- `autoInputSlots` (List<IntVec3>)
- `onlyGrabAndOutputToFactoryHoppers` (bool)
- `autoExtract` (bool) = `true`
- `manualExtractAllowNet` (bool) = `true`
- `extractTicks` (int) = `800`
- … +40 campos (ver `PipeSystem\ProcessDef.cs`)

## DefModExtensions (`<modExtensions>`) (2)

### `PipeSystem.AutoRefuelableLinkingExtension` : DefModExtension

- `disabledAutoLinkingNetDefs` (List<PipeNetDef>)

### `PipeSystem.FactoryHopperExtension` : DefModExtension

- `isfactoryHopper` (bool) = `true`

## CompProperties de Thing (`<comps>`) (14)

### `PipeSystem.CompProperties_AdvancedResourceProcessor` : CompProperties

- `showBufferInfo` (bool) = `true`
- `notWorkingKey` (string)
- `processes` (List<ProcessDef>)
- `soundAmbient` (SoundDef)
- `showProgressBar` (bool) = `true`
- `progressBarOffset` (Vector3) = `Vector3.zero`
- `progressBarOffsetByRotation` (List<Vector3>)
- `progressBarSizeX` (float) = `0.55f`
- `progressBarSizeY` (float) = `0.1f`
- `progressBarMargin` (float) = `0.1f`
- `rotateWithBuilding` (bool)
- `showWastepackBar` (bool) = `true`
- `wastepackBarOffset` (Vector3) = `Vector3.zero`
- `wastepackBarSizeX` (float) = `0.55f`
- `wastepackBarSizeY` (float) = `0.1f`
- `stopWhenWastepackFull` (bool)
- `showResultItem` (bool)
- `resultItemOffset` (Vector3) = `Vector3.zero`
- `resultItemSize` (Vector3) = `new Vector3(0.5f, 1f, 0.5f)`
- `ticksToFill` (int) = `200`
- `heatPushWhileWorking` (bool)
- `inProgressTexture` (string) = `""`
- `finishedTexture` (string) = `""`
- `useProgressGlowShader` (bool)
- `glowerTexturePath` (string)
- `startColor` (ColorInt) = `new ColorInt(255, 255, 255)`
- `finishedColor` (ColorInt) = `new ColorInt(255, 255, 255)`
- `canOverclock` (bool)
- `overclockGizmo` (string)
- `overclockLabel` (string)
- `overclockWindowLabel` (string)
- `overclockDesc` (string)
- `minOverclock` (float) = `0.1f`
- `maxOverclock` (float) = `2f`
- `hideSettings` (bool) = `true`
- `alwaysShowProgressBar` (bool)

### `PipeSystem.CompProperties_ConvertResourceToThing` : CompProperties_Resource

- `ratio` (int) = `1`
- `maxOutputStackSize` (int) = `-1`
- `thing` (ThingDef)

### `PipeSystem.CompProperties_ConvertThingToResource` : CompProperties_Resource

- `ratio` (int) = `1`
- `thing` (ThingDef)
- `category` (ThingCategoryDef)

### `PipeSystem.CompProperties_DeepExtractor` : CompProperties_Resource

- `deepThing` (ThingDef)
- `ticksPerPortion` (int) = `60`
- `noStorageLeftKey` (string) = `"PipeSystem_CantExtract"`
- `onlyExtractToNet` (bool)
- `onlyExtractToGround` (bool)
- `useDeepCountPerPortion` (bool) = `true`
- `showDeepCountLeft` (bool) = `true`

### `PipeSystem.CompProperties_ExplosiveContent` : CompProperties_Explosive

- `explosiveMaxRadius` (float)
- `explosiveMinRadius` (float)
- `radiusRequiredForExplosion` (float)

### `PipeSystem.CompProperties_GlowerOnProcess` : CompProperties_Glower

### `PipeSystem.CompProperties_PipeValve` : CompProperties_Resource

- `alwaysLinkToPipes` (bool)

### `PipeSystem.CompProperties_RefillWithPipes` : CompProperties_Resource

- `ratio` (int) = `1`
- `thing` (ThingDef)

### `PipeSystem.CompProperties_Resource` : CompProperties

- `pipeNet` (PipeNetDef)
- `soundAmbient` (SoundDef)
- `displayBasicInspectStringData` (bool) = `true`
- `Resource` (Resource) = `> pipeNet.resource`

### `PipeSystem.CompProperties_ResourceProcessor` : CompProperties_Resource

- `eachTicks` (int) = `600`
- `countNeeded` (float)
- `thing` (ThingDef)
- `thingCount` (float)
- `net` (PipeNetDef)
- `netCount` (float)
- `showBufferInfo` (bool) = `true`
- `notWorkingKey` (string)
- `results` (List<Result>)

### `PipeSystem.CompProperties_ResourceStorage` : CompProperties_Resource

- `storageCapacityStat` (StatDef)
- `storageCapacity` (float)
- `drawStorageBar` (bool) = `true`
- `addStorageInfo` (bool) = `true`
- `addTransferGizmo` (bool) = `true`
- `showOffMatWhenTransfering` (bool) = `true`
- `barData` (CustomFillableBarGaugeData)
- `margin` (float) = `0.15f`
- `barSize` (Vector2) = `new Vector2(1.3f, 0.4f)`
- `centerOffset` (Vector3) = `new Vector3(0f, 0f)`
- `barHorizontal` (bool)
- `rotateBarWithBuilding` (bool) = `true`
- `extractOptions` (ExtractOptions)
- `refillOptions` (RefillOptions)
- `destroyOption` (DestroyOption)
- `destroyOptions` (List<DestroyOption>)
- `contentRequirePower` (bool)
- `preventRotInNegativeTemp` (bool) = `true`
- `daysToRotStart` (float) = `0.75f`

### `PipeSystem.CompProperties_ResourceTrader` : CompProperties_Resource

- `consumptionPerTick` (float)
- `idleConsumptionPerTick` (float) = `-1f`
- `resourceOffOverlay` (bool) = `true`
- `visualOnlyConsumption` (bool)
- `handleCompRefuelableTicking` (bool)
- `disableRefuelableConsumptionInLowPowerMode` (bool)
- `lowPowerWhenRefuelableEmpty` (bool)
- `producerLowPowerWhenStorageFull` (bool)
- `soundAmbientReceivingResource` (SoundDef)

### `PipeSystem.CompProperties_SpawnerOrNet` : CompProperties_Resource

- `inheritFaction` (bool)
- `requiresPower` (bool)
- `saveKeysPrefix` (string)
- `showMessageIfOwned` (bool)
- `spawnCount` (int) = `1`
- `spawnForbidden` (bool)
- `spawnIntervalRange` (IntRange) = `new IntRange(100, 100)`
- `spawnMaxAdjacent` (int) = `-1`
- `thingToSpawn` (ThingDef)
- `writeTimeLeftToSpawn` (bool)

### `PipeSystem.CompProperties_SpillWhenDamaged` : CompProperties

- `spillEachTicks` (int) = `250`
- `startAtHitPointsPercent` (float)
- `amountToDraw` (float)
- `chooseFilthFrom` (List<ThingDef>) = `new List<ThingDef>()`
- `filthMaxSpawnRadius` (int) = `5`
- `filthAmountPerSpawn` (int) = `1`
- `chooseFleckFrom` (List<FleckDef>) = `new List<FleckDef>()`

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (22)

### `PipeSystem.AdvancedProcessorsManager` : MapComponent

- `AwaitingPickup` (List<Thing>) = `> awaitingPickup`
- `AwaitingIngredients` (List<Thing>) = `> awaitingIngredients`
- `ProcessIDsManager` (ProcessIDsManager) = `> processIDsManager`

### `PipeSystem.Alert_NoStorage` : Alert

### `PipeSystem.Building_Pipe` : Building

- `compResource` (CompResource)

### `PipeSystem.Building_PipeValve` : Building

### `PipeSystem.Building_ResourceHeater` : Building_TempControl

- `compResourceTrader` (CompResourceTrader)

### `PipeSystem.Building_ResourceWorkTable` : Building_WorkTable

### `PipeSystem.Building_ResourceWorkTable_HeatPush` : Building_ResourceWorkTable

### `PipeSystem.GenericPipeNetOverlayWorker` : CustomOverlayWorker

- `OverlayForThing` (Dictionary<Thing, Material>) = `new Dictionary<Thing, Material>()`

### `PipeSystem.ITab_Processor` : ITab

### `PipeSystem.JobDriver_BringToProcessor` : JobDriver

### `PipeSystem.JobDriver_DrainFromMarkedStorage` : JobDriver

### `PipeSystem.JobDriver_FillStorage` : JobDriver

### `PipeSystem.JobDriver_PickUpProcessor` : JobDriver

### `PipeSystem.PipeNetManager` : MapComponent

- `wantRefill` (HashSet<Thing>) = `new HashSet<Thing>()`
- `pipeNets` (List<PipeNet>) = `new List<PipeNet>()`
- `valveGrid` (BoolGrid)
- `noStorage` (List<PipeNet>) = `new List<PipeNet>()`

### `PipeSystem.PipeNetOverlayDrawer` : MapComponent

### `PipeSystem.PlaceWorker_DeepExtractor` : PlaceWorker_ShowDeepResources

### `PipeSystem.PlaceWorker_Pipe` : PlaceWorker

### `PipeSystem.ResourceAmountFormatWorker_DecimalIfSmall` : ResourceAmountFormatWorker

### `PipeSystem.WorkGiver_BringToProcessor` : WorkGiver_Scanner

### `PipeSystem.WorkGiver_DrainFromMarkedStorage` : WorkGiver_Scanner

### `PipeSystem.WorkGiver_PickUpProcessor` : WorkGiver_Scanner

### `PipeSystem.WorkGiver_RefillStorage` : WorkGiver_Scanner

