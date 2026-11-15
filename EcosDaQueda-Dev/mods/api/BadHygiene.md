# Referência de API: Dubs Bad Hygiene

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (5)

### `DubsBadHygiene.DefExtensions` : Def

- `drinkDefsToLookfor` (string[]) = `> new string[17]
	{
		"Cuptea", "Coffee", "Fruitdrink", "smo`

### `DubsBadHygiene.DubsModOptions` : Def

- `AddonPack` (string) = `string.Empty`
- `Exper` (bool)
- `AutoList` (bool) = `true`
- `defaultSetting` (int)
- `options` (List<DubsModOption>)
- `tip` (string)
- `Val` (float)
- `Setting` (DubsModOption) = `> options[selection]`

### `DubsBadHygiene.Needy` : NeedDef

- `Exemptions` (Exemptions)

### `DubsBadHygiene.PollutionDef` : Def

- `TerrainTypes` (List<PollutionType>)
- `ThingTypes` (List<PollutionType>)

### `DubsBadHygiene.WashingJobDef` : JobDef

- `CustomBodyRender` (bool) = `true`

## DefModExtensions (`<modExtensions>`) (3)

### `DubsBadHygiene.BathHeaterExt` : DefModExtension

- `HeatSources` (List<ThingDef>) = `new List<ThingDef>()`

### `DubsBadHygiene.BuildWaterExt` : DefModExtension

### `DubsBadHygiene.WaterExt` : DefModExtension

- `water` (float) = `1f`
- `UseForWashing` (bool)
- `SeekForThirst` (bool)

## CompProperties de Thing (`<comps>`) (22)

### `DubsBadHygiene.CompProperties_AirconBaseUnit` : CompProperties

- `BaseCapacity` (float) = `100f`
- `PowerModes` (int) = `8`
- `MinLoad` (float) = `0.2f`
- `StandbyPower` (float) = `20f`

### `DubsBadHygiene.CompProperties_AssignableToPawn_Fixture_Unused` : CompProperties

- `CanSetGender` (bool) = `true`
- `CanHaveOwners` (bool) = `true`

### `DubsBadHygiene.CompProperties_Blockage` : CompProperties

### `DubsBadHygiene.CompProperties_CompAirconUnit` : CompProperties

- `Capacity` (float) = `100f`
- `CoolingRate` (int) = `250`
- `energyPerSecond` (float) = `-21f`

### `DubsBadHygiene.CompProperties_CompBoiler` : CompProperties

- `effects` (GraphicData)
- `BaseCapacity` (float) = `250f`
- `GizmoLabel` (string) = `"Power Mode"`
- `LowPowerMode` (float) = `-20f`
- `PowerModes` (int) = `8`
- `ThermostatControl` (bool)
- `ShouldUsePowerModes` (bool) = `true`

### `DubsBadHygiene.CompProperties_Composter` : CompProperties

- `Material` (ThingDef)
- `Product` (ThingDef)
- `capacity` (int) = `250`
- `productPerMaterial` (int) = `1`

### `DubsBadHygiene.CompProperties_GreyWaterSaving` : CompProperties

- `baseSaving` (float) = `1f`

### `DubsBadHygiene.CompProperties_IceWell` : CompProperties_WaterInlet

### `DubsBadHygiene.CompProperties_KickOver` : CompProperties

### `DubsBadHygiene.CompProperties_OxygenGenerationSystem` : CompProperties

- `effects` (GraphicData)
- `Capacity` (float) = `2000f`

### `DubsBadHygiene.CompProperties_Pipe` : CompProperties

- `mode` (PipeType)
- `stuffed` (bool)
- `vertPipe` (bool)

### `DubsBadHygiene.CompProperties_Radiator` : CompProperties_HeatStore

- `Energy` (float) = `21f`
- `HeatingRate` (int) = `250`

### `DubsBadHygiene.CompProperties_SaunaHeater` : CompProperties_HeatPusher

- `effects` (GraphicData)

### `DubsBadHygiene.CompProperties_SepticTank` : CompProperties_SewageHandler

- `TreatedPerDay` (float) = `1500f`

### `DubsBadHygiene.CompProperties_SewageHandler` : CompProperties

- `capacity` (float) = `10000f`
- `priority` (int)

### `DubsBadHygiene.CompProperties_SewageVacuumDump` : CompProperties

### `DubsBadHygiene.CompProperties_Sprinkler` : CompProperties

- `usagePerCell` (float) = `0.16f`
- `fireDamage` (float) = `20f`

### `DubsBadHygiene.CompProperties_WaterFiltration` : CompProperties

### `DubsBadHygiene.CompProperties_WaterInlet` : CompProperties

- `effects` (GraphicData)
- `Capacity` (float) = `2000f`
- `Radius` (float) = `9f`
- `Deep` (bool)
- `Piped` (bool) = `true`

### `DubsBadHygiene.CompProperties_WaterPumpingStation` : CompProperties

- `Capacity` (float) = `2000f`

### `DubsBadHygiene.CompProperties_WaterStorage` : CompProperties

- `WaterStorageCap` (float) = `10000f`
- `AutoGenRate` (float)
- `Tickrate` (int) = `60`
- `Auto` (bool)
- `AutoOnRain` (bool)

### `DubsBadHygiene.CompProperties_WaterTrader` : CompProperties

- `WaterPerTick` (float)

## PatchOperations (`Patches/*.xml`) (1)

### `DubsBadHygiene.PatchOperationAddDesignator` : PatchOperationAdd

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (126)

### `DubsBadHygiene.Alert_BlockedSewer` : Alert

### `DubsBadHygiene.Alert_ContamLevels` : Alert

### `DubsBadHygiene.Alert_ContaminatedTower` : Alert

### `DubsBadHygiene.Alert_MissingPump` : Alert

### `DubsBadHygiene.Alert_MissingTower` : Alert

### `DubsBadHygiene.Alert_MissingWell` : Alert

### `DubsBadHygiene.Alert_ToxicFallout` : Alert

### `DubsBadHygiene.Alert_WaterTemp` : Alert

### `DubsBadHygiene.Building_AdvToilet` : Building_toilet

### `DubsBadHygiene.Building_AssignableFixture` : Building

- `blockageComp` (CompBlockage)
- `brokenPipe` (bool)
- `gender` (Gender)
- `AllowColonists` (bool) = `true`
- `AllowSlaves` (bool) = `true`
- `AllowGuests` (bool) = `true`
- `powerComp` (CompPowerTrader)
- `pipe` (CompPipe)
- `AnimalsOnly` (bool)

### `DubsBadHygiene.Building_BurnPit` : Building_WorkTable

- `fuelComp` (CompRefuelable)

### `DubsBadHygiene.Building_Butt` : Building_AssignableFixture

### `DubsBadHygiene.Building_CeilingFan` : Building

- `FanRotation` (float)
- `powerComp` (CompPowerTrader)
- `refuelableComp` (CompRefuelable)
- `FanSpeed` (float)
- `breakdownableComp` (CompBreakdownable)

### `DubsBadHygiene.Building_Composter` : Building

- `pertick` (float) = `3.3333333E-06f`
- `MaxCapacity` (int) = `> ((ThingWithComps)this).GetComp<CompComposter>()?.Props.cap`

### `DubsBadHygiene.Building_FillableThing` : Building

- `geyserLoc` (IntVec3) = `IntVec3.Invalid`
- `occupants` (HashSet<Pawn>) = `new HashSet<Pawn>()`
- `cleanedPerTick` (float) = `0.0001f`
- `fuel` (float)
- `pipe` (CompPipe)
- `poolmat` (Material)
- `RefreshedTerrain` (bool)
- `water` (Material)
- `caustic1` (Material)
- `caustic_col_1` (Color)
- `caustic_col_2` (Color)
- `causticOffset1` (Vector2)
- `ticker` (int)
- `flowRate` (int) = `50`
- `FuelPercentOfMax` (float) = `> fuel / capacity`
- `IsFull` (bool) = `> FuelPercentOfMax > 0.8f`
- `waterUsedPerTick` (float) = `> 1f`

### `DubsBadHygiene.Building_Fridge` : Building

- `container` (ThingOwner)
- `contentsKnown` (bool) = `true`
- `SpaceLeft` (int) = `> 40 - container.Count`
- `HasSpace` (bool) = `> container.Count < 40`
- `HasAnyContents` (bool) = `> GetContainer().Count > 0`
- `CanOpen` (bool) = `> HasAnyContents`
- `StorageTabVisible` (bool) = `> true`

### `DubsBadHygiene.Building_HotTub` : Building_AssignableFixture

- `occupants` (HashSet<Pawn>) = `new HashSet<Pawn>()`
- `cleanedPerTick` (float) = `0.0001f`
- `contaminated` (ContaminationLevel)
- `emptycache` (Graphic)
- `fullcache` (Graphic)
- `PipeComp` (CompPipe)
- `seatCount` (int) = `2`
- `SteamyNow` (bool)
- `usage` (float) = `1000f`
- `WaterInTub` (float)
- `IsFull` (bool) = `> WaterInTub >= usage`
- `waterUsedPerTick` (float) = `> 1f`

### `DubsBadHygiene.Building_Latrine` : bibblefuckwit

- `sewage` (float)
- `sewageLimit` (float) = `500f`

### `DubsBadHygiene.Building_Litterbox` : Building_AssignableFixture

### `DubsBadHygiene.Building_Pipe` : Building

- `pipe` (CompPipe)

### `DubsBadHygiene.Building_Pool` : Building_FillableThing

### `DubsBadHygiene.Building_PoolTile` : Building_FillableThing

- `body` (PoolBody)
- `printedLevel` (int) = `-1`
- `WaterLevel` (int) = `> Mathf.Clamp(Mathf.CeilToInt(FuelPercentOfMax * 10f), 0, 10`

### `DubsBadHygiene.Building_PowerShower` : Building_shower

### `DubsBadHygiene.Building_SpacerToilet` : bibblefuckwit

### `DubsBadHygiene.Building_StallDoor` : Building

- `visualTicksOpen` (int)
- `ticksUntilClose` (int)
- `TicksToOpenNow` (int) = `> Mathf.RoundToInt(45f / StatExtension.GetStatValue((Thing)(`

### `DubsBadHygiene.Building_TowelRail` : Building

- `towely` (Vector3)

### `DubsBadHygiene.Building_Valve` : Building

### `DubsBadHygiene.Building_WashingMachine` : Building

- `innerContainer` (ThingOwner)
- `PipeComp` (CompPipe)
- `powerComp` (CompPowerTrader)
- `SpinCycleTicksLeft` (int)
- `storageCap` (int) = `5`
- `StorageTabVisible` (bool) = `> true`

### `DubsBadHygiene.Building_WaterFeature` : Building_FillableThing

### `DubsBadHygiene.Building_WaterTrough` : Building_washbucket

### `DubsBadHygiene.Building_basin` : Building_AssignableFixture

- `heatUsedPerTick` (float) = `0.00025f`
- `sewageUsedPerTick` (float) = `0.01f`
- `waterUsedPerTick` (float) = `0.04f`

### `DubsBadHygiene.Building_bath` : Building_AssignableFixture

- `cleanedPerTick` (float) = `0.001f`
- `contamination` (ContaminationLevel)
- `heatUsedPerTick` (float) = `0.00025f`
- `WaterInTub` (float)
- `waterLevel` (float)
- `occupant` (Pawn)
- `IsFull` (bool) = `> WaterInTub >= waterUsed`
- `waterUsedPerTick` (float) = `> 1f`
- `sewageUsedPerTick` (float) = `> 0.1f`

### `DubsBadHygiene.Building_shower` : Building_AssignableFixture

### `DubsBadHygiene.Building_toilet` : bibblefuckwit

- `seatUp` (bool) = `Rand.Bool`
- `toiletLidUpCache` (Graphic)

### `DubsBadHygiene.Building_washbucket` : Building_AssignableFixture

- `allowAutoRefuel` (bool) = `true`
- `ballcock` (bool)
- `Contamination` (ContaminationLevel) = `ContaminationLevel.Untreated`
- `WaterUsesRemaining` (int)
- `waterGraphicCache` (Graphic)

### `DubsBadHygiene.Command_SetTargetDrainLevel` : Command

- `refuelable` (CompSepticTank)

### `DubsBadHygiene.Gizmo_BoilerStatus` : Gizmo

- `boiler` (CompBoiler)

### `DubsBadHygiene.HygienePipeMapComp` : MapComponent

- `PipeNets` (PlumbingNet[]) = `new List<PlumbingNet>().ToArray()`
- `cachedPipes` (List<CompPipe>) = `new List<CompPipe>()`
- `masterID` (int) = `1`
- `PipeGrid` (int[,])
- `DirtyPipe` (bool[])
- `FullRebuild` (bool[])

### `DubsBadHygiene.IncidentWorker_SewageSpill` : IncidentWorker

### `DubsBadHygiene.IncidentWorker_TowerContamination` : IncidentWorker

### `DubsBadHygiene.JobDriver_AdministerFluids` : JobDriver

- `UsedCarriedWater` (bool)
- `usingBuilding` (bool)

### `DubsBadHygiene.JobDriver_DrainWater` : JobDriver

### `DubsBadHygiene.JobDriver_DrinkFromBasin` : JobDriver

- `waterQuality` (ContaminationLevel)

### `DubsBadHygiene.JobDriver_DrinkFromGround` : JobDriver

### `DubsBadHygiene.JobDriver_FixBlockage` : JobDriver

### `DubsBadHygiene.JobDriver_GoSwimming` : JobDriver

### `DubsBadHygiene.JobDriver_LoadComposter` : JobDriver

### `DubsBadHygiene.JobDriver_LoadFridge` : JobDriver

### `DubsBadHygiene.JobDriver_PackWaterBottle` : JobDriver

- `waterQuality` (ContaminationLevel)

### `DubsBadHygiene.JobDriver_PlaceFertilizer` : JobDriver

### `DubsBadHygiene.JobDriver_Poop` : JobDriver

### `DubsBadHygiene.JobDriver_RefillTub` : JobDriver

- `ContaminationLevel` (ContaminationLevel)

### `DubsBadHygiene.JobDriver_RefillWater` : JobDriver

### `DubsBadHygiene.JobDriver_RelieveInVacSuit` : JobDriver

### `DubsBadHygiene.JobDriver_RemoveSewage` : JobDriver_AffectFloor

### `DubsBadHygiene.JobDriver_StockpileWaterBottles` : JobDriver

- `waterQuality` (ContaminationLevel)
- `DoX` (int)

### `DubsBadHygiene.JobDriver_SwimPoolTiles` : JobDriver

### `DubsBadHygiene.JobDriver_TipOverSewage` : JobDriver

### `DubsBadHygiene.JobDriver_TriggerSprinkler` : JobDriver

### `DubsBadHygiene.JobDriver_UnloadComposter` : JobDriver

### `DubsBadHygiene.JobDriver_UseHotTub` : JobDriver

- `UseTub` (Toil)

### `DubsBadHygiene.JobDriver_UseSauna` : JobDriver

### `DubsBadHygiene.JobDriver_UseToilet` : JobDriver

### `DubsBadHygiene.JobDriver_WashingMachineLoad` : JobDriver

### `DubsBadHygiene.JobDriver_WashingMachineUnload` : JobDriver

### `DubsBadHygiene.JobDriver_WatchWashingMachine` : JobDriver_WatchBuilding

### `DubsBadHygiene.JobDriver_cleanBedpan` : JobDriver

### `DubsBadHygiene.JobDriver_emptyLatrine` : JobDriver

### `DubsBadHygiene.JobDriver_emptySepticTank` : JobDriver

- `comp` (CompSepticTank) = `> tank.GetComp<CompSepticTank>()`

### `DubsBadHygiene.JobDriver_poopOutside` : JobDriver

### `DubsBadHygiene.JobDriver_takeBath` : JobDriver

- `waterQuality` (ContaminationLevel)

### `DubsBadHygiene.JobDriver_takeShower` : JobDriver

- `ColdShower` (bool)
- `waterQuality` (ContaminationLevel)

### `DubsBadHygiene.JobDriver_useWashBucket` : JobDriver

- `waterQuality` (ContaminationLevel)

### `DubsBadHygiene.JobDriver_washAtCell` : JobDriver

### `DubsBadHygiene.JobDriver_washHands` : JobDriver

- `waterQuality` (ContaminationLevel)

### `DubsBadHygiene.JobDriver_washPatient` : JobDriver

### `DubsBadHygiene.JobGiver_DrinkWater` : ThinkNode_JobGiver

### `DubsBadHygiene.JobGiver_DrinkWaterUrgent` : JobGiver_DrinkWater

### `DubsBadHygiene.JobGiver_HaveWash` : ThinkNode_JobGiver

### `DubsBadHygiene.JobGiver_PackDrink` : ThinkNode_JobGiver

### `DubsBadHygiene.JobGiver_UseToilet` : ThinkNode_JobGiver

### `DubsBadHygiene.JobGiver_UseToiletUrgent` : JobGiver_UseToilet

### `DubsBadHygiene.MapComponent_Hygiene` : MapComponent

- `blockageManager` (BlockageManager) = `new BlockageManager()`
- `DeepWaterGridSeed` (int)
- `InitFinalized` (bool)
- `MarkHeatingForDraw` (bool)
- `MarkTowersForDraw` (bool)
- `UpdateCapacities` (bool)
- `WaterGridSeed` (int)
- `SludgeDrumVersion` (int) = `1`
- `FertilizerGrid` (GridLayer)
- `FertilizeZones` (List<Zone>) = `new List<Zone>()`
- `IrrigationGrid` (GridLayer)
- `SewageGrid` (GridLayer)
- `WaterUserGrid` (GridLayer)
- `WaterGrid` (GridLayer)
- `DeepUserGrid` (GridLayer)
- `DeepWaterGrid` (GridLayer)
- `IceUserGrid` (GridLayer)
- `IceGrid` (GridLayer)
- `skylights` (Traverse<bool[]>)
- `num` (int) = `-1`
- `ticksincelastcycle` (int)

### `DubsBadHygiene.PlaceWorker_Aircon` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_DeepWaterGrid` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_Freezer` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_IceGrid` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_IceUserGrid` : PlaceWorker

- `waterCells` (List<IntVec3>) = `new List<IntVec3>()`

### `DubsBadHygiene.PlaceWorker_NotUnderEdiface` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_Pipe` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_PipeOverlap` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_RecyclerDrawLines` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_SewageArea` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_SewageGrid` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_Sprinkler` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_SurfaceWater` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_UserGrid` : PlaceWorker

- `waterCells` (List<IntVec3>) = `new List<IntVec3>()`

### `DubsBadHygiene.PlaceWorker_WaterGrid` : PlaceWorker

### `DubsBadHygiene.PlaceWorker_WindPump` : PlaceWorker

### `DubsBadHygiene.RoomRoleWorker_PrivateBathroom` : RoomRoleWorker

### `DubsBadHygiene.RoomRoleWorker_PublicBathroom` : RoomRoleWorker

### `DubsBadHygiene.RoomRoleWorker_Sauna` : RoomRoleWorker

### `DubsBadHygiene.ThinkNode_ConditionalThirst` : ThinkNode_Conditional

### `DubsBadHygiene.ThinkNode_ConditionalToilet` : ThinkNode_Conditional

### `DubsBadHygiene.ThoughtWorker_Bowel` : ThoughtWorker

### `DubsBadHygiene.ThoughtWorker_Hygiene` : ThoughtWorker

### `DubsBadHygiene.WaterMassStat` : StatPart

### `DubsBadHygiene.WaterValueStat` : StatPart

### `DubsBadHygiene.WorkGiver_AdministerFluids` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_CleanBedpan` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_CleanFilth` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_DrainWater` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_FillComposter` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_FixBlockage` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_FridgeLoad` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_PlaceFertilizer` : WorkGiver_Scanner

- `hy` (MapComponent_Hygiene)
- `AnyFertilizer` (bool) = `true`
- `FoundCell` (bool)

### `DubsBadHygiene.WorkGiver_RefillWater` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_RemoveSewage` : WorkGiver_ConstructAffectFloor

### `DubsBadHygiene.WorkGiver_TipSewage` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_UnloadComposter` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_WashingLoad` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_WashingUnload` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_emptySepticTank` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_washChild` : WorkGiver_Scanner

### `DubsBadHygiene.WorkGiver_washPatient` : WorkGiver_Scanner

### `DubsBadHygiene.bibblefuckwit` : Building_AssignableFixture

- `floater` (bool)

## Mod settings / ModBase (2)

### `DubsBadHygiene.DubsBadHygieneMod` : Mod

### `DubsBadHygiene.Settings` : ModSettings

- `genes` (List<string>)
- `genescrollPosition` (Vector2)
- `traits` (List<string>)
- `traitscrollPosition` (Vector2)
- `bodies` (List<string>)
- `bodyscrollPosition` (Vector2)
- `hediffs` (List<string>)
- `hediffscrollPosition` (Vector2)
- `listing_Standard` (Listing_Standard)
- `races` (List<string>)
- `racescrollPosition` (Vector2)
- `settings` (Settings)
- `raceMods` (Dictionary<string, string>) = `new Dictionary<string, string>()`
- `hediffMods` (Dictionary<string, string>) = `new Dictionary<string, string>()`
- `bodyMods` (Dictionary<string, string>) = `new Dictionary<string, string>()`
- `geneMods` (Dictionary<string, string>) = `new Dictionary<string, string>()`
- `traitMods` (Dictionary<string, string>) = `new Dictionary<string, string>()`
- `PassiveWaterCoolers` (bool)
- `AllowNonHuman` (bool) = `true`
- `AnimalsGetBladder` (bool)
- `BladderBody` (List<string>) = `new List<string>()`
- `BladderHediff` (List<string>) = `new List<string>()`
- `ThirstBody` (List<string>) = `new List<string>()`
- `ThirstHediff` (List<string>) = `new List<string>()`
- `ThirstRace` (List<string>) = `new List<string>()`
- `BladderGene` (List<string>) = `new List<string>()`
- `HygieneGene` (List<string>) = `new List<string>()`
- `ThirstGene` (List<string>) = `new List<string>()`
- `BladderTrait` (List<string>) = `new List<string>()`
- `HygieneTrait` (List<string>) = `new List<string>()`
- `ThirstTrait` (List<string>) = `new List<string>()`
- `BladderNeed` (bool) = `true`
- `BladderRace` (List<string>) = `new List<string>()`
- `console` (Dialog_NeedExemptionSettings)
- `CoolingEfficiency` (bool) = `true`
- `FertilizerVisible` (bool) = `true`
- `GuestsGetBladder` (bool) = `true`
- `GuestsGetHygiene` (bool) = `true`
- `ChildrenGetBladder` (bool) = `true`
- `ChildrenGetHygiene` (bool) = `true`
- … +26 campos (ver `DubsBadHygiene\Settings.cs`)

