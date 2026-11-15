# Referência de API: Vehicle Framework (Vehicles.dll)

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (21)

### `Vehicles.AerialVehicleArrivalModeDef` : Def

- `workerClass` (Type) = `typeof(AerialVehicleArrivalModeWorker)`
- `selectionWeightCurve` (SimpleCurve)
- `pointsFactorCurve` (SimpleCurve)
- `minTechLevel` (TechLevel)
- `forQuickMilitaryAid` (bool)
- `walkIn` (bool)
- `textEnemy` (string)
- `textFriendly` (string)
- `textWillArrive` (string)

### `Vehicles.AirdropDef` : ThingDef

- `from` (Vector2)
- `to` (Vector2)
- `layer` (int)
- `parachuteGraphicData` (GraphicData)
- `ropes` (List<AnchorPoint>)

### `Vehicles.AntiAircraftDef` : WorldObjectDef

- `distance` (float) = `4f`
- `altitude` (IntRange) = `new IntRange(1000, 10000)`
- `arc` (int) = `30`
- `buildings` (IntRange) = `new IntRange(1, 4)`
- `damage` (float)
- `accuracy` (float)
- `properties` (AirDefenseProperties) = `new AirDefenseProperties()`
- `ticksBetweenShots` (int) = `20`
- `antiAircraftWorker` (Type)
- `minTechLevel` (TechLevel) = `(TechLevel)4`
- `explosionGraphic` (GraphicData)
- `framesForExplosion` (int) = `-1`
- `drawSizeMultiplier` (float) = `1f`

### `Vehicles.CustomHitFlags` : Def

- `minFillPercent` (float) = `-1f`
- `hitThroughPawns` (bool)

### `Vehicles.IndicatorDef` : Def

- `iconPath` (string)

### `Vehicles.PatternDef` : Def

- `path` (string)
- `properties` (PatternProperties)
- `exclusiveFor` (List<VehicleDef>)
- `MaterialCount` (int) = `> 4`
- `Name` (string) = `> base.modContentPack.Name + "_" + base.defName`

### `Vehicles.RGBShaderTypeDef` : ShaderTypeDef

### `Vehicles.RetextureDef` : Def

- `graphicData` (GraphicDataRGB)

### `Vehicles.SkinDef` : PatternDef

### `Vehicles.StatUpgradeCategoryDef` : Def

- `toStringStyle` (ToStringStyle) = `(ToStringStyle)2`
- `toStringNumberSense` (ToStringNumberSense) = `(ToStringNumberSense)1`
- `formatString` (string)
- `upgradeEffectType` (UpgradeEffectType)

### `Vehicles.TransferableVehicleSorterDef` : TransferableSorterDef

### `Vehicles.UpgradeTreeDef` : Def

- `nodes` (List<UpgradeNode>)

### `Vehicles.VehicleBuildDef` : ThingDef

- `thingToSpawn` (VehicleDef)
- `soundBuilt` (SoundDef)
- `shakeAmountPerAreaCurve` (SimpleCurve)

### `Vehicles.VehicleDef` : ThingDef

- `enabled` (VehicleEnabled.For) = `VehicleEnabled.For.Everyone`
- `nameable` (bool) = `true`
- `combatPower` (float) = `100f`
- `vehicleStats` (List<VehicleStatModifier>) = `new List<VehicleStatModifier>()`
- `canCaravan` (bool) = `true`
- `vehicleCategory` (VehicleCategory)
- `type` (VehicleType) = `VehicleType.Land`
- `navigationCategory` (NavigationCategory) = `NavigationCategory.Opportunistic`
- `buildDef` (VehicleBuildDef)
- `graphicData` (GraphicDataRGB)
- `properties` (VehicleProperties)
- `npcProperties` (VehicleNPCProperties)
- `drawProperties` (VehicleDrawProperties)
- `fishingProperties` (FishingProperties)
- `statEvents` (List<StatCache.EventLister>)
- `soundOneShotsOnEvent` (List<VehicleSoundEventEntry<VehicleEventDef>>) = `new List<VehicleSoundEventEntry<VehicleEventDef>>()`
- `soundSustainersOnEvent` (List<VehicleSustainerEventEntry<VehicleEventDef>>)
- `events` (SimpleDictionary<VehicleEventDef, List<DynamicDelegate<VehiclePawn>>>)
- `designatorTypes` (List<Type>) = `new List<Type>()`
- `draftLabel` (string)
- `soundBuilt` (SoundDef)
- `kindDef` (PawnKindDef)
- `components` (List<VehicleComponentProperties>)
- `BodyType` (VehicleFleshTypeDef) = `> kindDef.RaceProps.FleshType as VehicleFleshTypeDef`
- `MaterialCount` (int) = `> 4`
- `PatternDef` (PatternDef) = `> PatternDefOf.Default`
- `Name` (string) = `> ((Def)this).defName`

### `Vehicles.VehicleEventDef` : Def

### `Vehicles.VehicleFleshTypeDef` : FleshTypeDef

- `nonPenetrationEffect` (EffecterDef)
- `diminishedEffect` (EffecterDef)
- `deflectionEffect` (EffecterDef)
- `deflectionEffectBullet` (EffecterDef)
- `electrifiedEffect` (EffecterDef)
- `soundImpactDeflect` (SoundDef)
- `soundImpactDiminished` (SoundDef)
- `soundImpactNonPenetrated` (SoundDef)
- `soundImpactPenetrated` (SoundDef)

### `Vehicles.VehicleRaidParamsDef` : Def

- `factions` (List<FactionDef>)
- `inventory` (List<PawnInventoryOption>)
- `arrivalModes` (List<PawnsArrivalModeDef>)

### `Vehicles.VehicleStatDef` : Def

- `defaultBaseValue` (float)
- `minValue` (float) = `float.MinValue`
- `maxValue` (float) = `float.MaxValue`
- `hideAtValue` (float) = `float.NaN`
- `alwaysHide` (bool)
- `showIfUndefined` (bool)
- `neverDisabled` (bool)
- `showZeroBaseValue` (bool)
- `applyFactorsIfNegative` (bool) = `true`
- `statFactors` (List<VehicleStatDef>)
- `parts` (List<VehicleStatPart>)
- `modSettingsInfo` (SettingsValueInfo)
- `category` (StatCategoryDef)
- `showIfModsLoaded` (List<string>)
- `showOnVehicleTypes` (List<VehicleType>)
- `formatString` (string)
- `toStringStyle` (ToStringStyle)
- `toStringStyleUnfinalized` (Nullable)
- `toStringNumberSense` (ToStringNumberSense) = `(ToStringNumberSense)1`
- `operationType` (EfficiencyOperationType)
- `upgradeEffectType` (UpgradeEffectType)
- `postProcessCurve` (SimpleCurve)
- `postProcessStatFactors` (List<VehicleStatDef>)
- `workerClass` (Type) = `typeof(VehicleStatWorker)`
- `displayPriorityInCategory` (int) = `1`
- `formatStringUnfinalized` (string)

### `Vehicles.VehicleStrategyDef` : Def

- `strategyWorker` (Type)

### `Vehicles.VehicleTurretDef` : Def

- `turretType` (TurretType)
- `motes` (List<AnimationProperties>)
- `ammunition` (ThingFilter)
- `magazineCapacity` (int) = `1`
- `chargePerAmmoCount` (float) = `1f`
- `genericAmmo` (bool)
- `cooldown` (TurretCooldownProperties)
- `recoil` (RecoilProperties)
- `vehicleRecoil` (RecoilProperties)
- `graphicData` (GraphicDataRGB)
- `graphics` (List<VehicleTurretRenderData>)
- `gizmoDescription` (string)
- `gizmoIconScale` (float) = `1f`
- `gizmoIconTexPath` (string)
- `matchParentColor` (bool) = `true`
- `fireModes` (List<FireMode>) = `new List<FireMode>()`
- `autoSnapTargeting` (bool)
- `rotationSpeed` (float) = `1f`
- `rotationDelta` (float)
- `maxRange` (float)
- `minRange` (float)
- `reloadTimer` (float) = `5f`
- `reloadTimerMultiplierPerCrewCount` (LinearCurve)
- `warmUpTimer` (float) = `3f`
- `autoRefuelProportion` (float) = `2f`
- `empDisables` (bool)
- `shotSound` (SoundDef)
- `reloadSound` (SoundDef)
- `targetScanFlags` (TargetScanFlags)
- `projectile` (ThingDef)
- `attachProjectileFlag` (CustomHitFlags)
- `projectileOffset` (float)
- `projectileSpeed` (float) = `-1f`
- `projectileShifting` (List<float>) = `new List<float>()`
- `restrictionType` (Type)

### `Vehicles.VehicleTurretEventDef` : Def

## DefModExtensions (`<modExtensions>`) (10)

### `Vehicles.CETurretDataDefModExtension` : DefModExtension

- `speed` (float) = `-1f`
- `sway` (float) = `-1f`
- `spread` (float) = `-1f`
- `recoil` (float) = `-1f`
- `shotHeight` (float) = `1f`
- `_ammoSet` (Def)
- `ammoSet` (string)

### `Vehicles.CustomCostDefModExtension` : DefModExtension

- `vehicles` (List<VehicleDef>)
- `cost` (float)

### `Vehicles.PawnFleeSettingsDefModExtension` : DefModExtension

- `shouldFlee` (bool) = `true`
- `fleeDistance` (float)

### `Vehicles.ProjectilePropertiesDefModExtension` : DefModExtension

- `speed` (float) = `-1f`
- `hitFlagDef` (CustomHitFlags)

### `Vehicles.ReduceExplosionOnWater` : DefModExtension

### `Vehicles.RoofDefPositionStateDefModExtension` : DefModExtension

- `state` (LandingTargeter.PositionState)

### `Vehicles.SpaceObjectDefModExtension` : DefModExtension

### `Vehicles.SpawnerPropertiesDefModExtension` : DefModExtension

- `chanceToSpawnThing` (float) = `1f`
- `thingToSpawn` (ThingDef)

### `Vehicles.VehicleDamageMultiplierDefModExtension` : DefModExtension

- `multiplier` (float)

### `Vehicles.VehicleRaiderDefModExtension` : DefModExtension

- `pointMultiplier` (float) = `1f`
- `techLevelRestricted` (bool) = `true`
- `arrivalModes` (HashSet<PawnsArrivalModeDef>)

## CompProperties de Thing (`<comps>`) (7)

### `Vehicles.CompProperties_DrawLayer` : CompProperties

- `graphicDatas` (List<GraphicData>)

### `Vehicles.CompProperties_DrawLayerTurret` : CompProperties

- `graphicDatas` (List<GraphicData>)

### `Vehicles.CompProperties_FueledTravel` : VehicleCompProperties

- `fuelType` (ThingDef)
- `leakDef` (ThingDef)
- `electricPowered` (bool)
- `dischargeRate` (float) = `2f`
- `chargeRate` (float) = `1f`
- `fuelConsumptionRate` (float)
- `fuelCapacity` (int)
- `fuelConsumptionWorldMultiplier` (float) = `1f`
- `autoRefuelPercent` (float) = `1f`
- `targetFuelLevelConfigurable` (bool) = `true`
- `ambientHeat` (float)
- `gizmoLabel` (string)
- `fuelConsumptionCondition` (FuelConsumptionCondition) = `FuelConsumptionCondition.All`
- `motesGenerated` (List<OffsetMote>)
- `moteDisplayed` (ThingDef)
- `ticksToSpawnMote` (int)
- `fuelIconPath` (string)
- `ElectricPowered` (bool) = `> electricPowered`

### `Vehicles.CompProperties_UpgradeTree` : CompProperties

- `def` (UpgradeTreeDef)

### `Vehicles.CompProperties_VehicleLauncher` : VehicleCompProperties

- `fuelConsumptionWorldMultiplier` (float) = `10f`
- `rateOfClimb` (float) = `10f`
- `maxFallRate` (float) = `20f`
- `maxAltitude` (int) = `10000`
- `landingAltitude` (int) = `1000`
- `reconDistance` (int) = `1`
- `fixedLaunchDistanceMax` (int)
- `controlInFlight` (bool) = `true`
- `canRoofPunch` (bool)
- `animationPunchAt` (float) = `0.95f`
- `spaceFlight` (bool)
- `signalJammer` (bool)
- `faceDirectionOfTravel` (bool) = `true`
- `circleToLand` (bool) = `true`
- `deployTicks` (int)
- `shadow` (string) = `"Things/Skyfaller/SkyfallerShadowCircle"`
- `bombing` (BomberProperties)
- `strafing` (StrafingProperties)
- `launchProtocol` (LaunchProtocol)
- `skyfallerLeaving` (ThingDef)
- `skyfallerIncoming` (ThingDef)
- `skyfallerCrashing` (ThingDef)
- `skyfallerStrafing` (ThingDef)
- `skyfallerBombing` (ThingDef)

### `Vehicles.CompProperties_VehicleTurrets` : VehicleCompProperties

- `deployTime` (float)
- `turrets` (List<VehicleTurret>) = `new List<VehicleTurret>()`
- `deployingSustainer` (SoundDef)
- `deploySound` (SoundDef)
- `undeploySound` (SoundDef)

### `Vehicles.VehicleCompProperties` : CompProperties

## PatchOperations (`Patches/*.xml`) (1)

### `Vehicles.PatchOperationFeature` : PatchOperation

- `feature` (string)
- `patch` (PatchOperation)

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (104)

### `Vehicles.AerialVehicleArrivalModeWorker_CenterDrop` : AerialVehicleArrivalModeWorker

### `Vehicles.AerialVehicleArrivalModeWorker_EdgeDrop` : AerialVehicleArrivalModeWorker

### `Vehicles.AerialVehicleArrivalModeWorker_TargetedDrop` : AerialVehicleArrivalModeWorker

### `Vehicles.Airdrop` : Building

- `innerContainer` (ThingOwner<Thing>) = `new ThingOwner<Thing>()`

### `Vehicles.AirdropManager` : MapComponent

### `Vehicles.Alert_IdleInVehicle` : Alert

### `Vehicles.Building_Artillery` : Building_TurretGun

- `WarmingUp` (bool) = `> base.burstWarmupTicksLeft > 0`
- `CanToggleHoldFire` (bool) = `> PlayerControlled`
- `IsMortar` (bool) = `> ((Thing)this).def.building.IsMortar`

### `Vehicles.Building_RecoiledTurret` : Building_Artillery

### `Vehicles.Command_ActionHighlighter` : Command_Action

- `mouseOver` (Action)

### `Vehicles.Command_ActionPawnDrawer` : Command_Action

- `pawn` (Pawn)

### `Vehicles.Command_ActionVehicleDrawn` : Command_Action

- `buildDef` (VehicleBuildDef)

### `Vehicles.Command_TransferToVehicle_Cancel` : Command_Action

### `Vehicles.Command_TransferToVehicle_Order` : Command_Target

### `Vehicles.Command_VerbWorldTarget` : Command

- `verb` (Verb)
- `drawRadius` (bool) = `true`

### `Vehicles.ITab_Airdrop_Container` : ITab

### `Vehicles.ITab_Vehicle_Cargo` : ITab_Airdrop_Container

### `Vehicles.ITab_Vehicle_Health` : ITab

- `Vehicle` (VehiclePawn) = `> ((ITab)this).SelPawn as VehiclePawn`

### `Vehicles.ITab_Vehicle_Passengers` : ITab

- `Vehicle` (VehiclePawn) = `> ((ITab)this).SelPawn as VehiclePawn`

### `Vehicles.ITab_Vehicle_Upgrades` : ITab

### `Vehicles.JobDriverGetItemForVehicleBase` : JobDriverLoadVehicleBase

- `Count` (int) = `> neededThingDefs.Count`

### `Vehicles.JobDriverLoadVehicleBase` : JobDriver

### `Vehicles.JobDriver_Board` : JobDriver

### `Vehicles.JobDriver_CarryPawnToVehicle` : JobDriver

### `Vehicles.JobDriver_Deploy` : JobDriver

### `Vehicles.JobDriver_DisassembleVehicle` : JobDriver

### `Vehicles.JobDriver_FollowVehicle` : JobDriver

### `Vehicles.JobDriver_GiveItemToVehicle` : JobDriverGetItemForVehicleBase

### `Vehicles.JobDriver_GiveToUpgradeContainer` : JobDriverGetItemForVehicleBase

### `Vehicles.JobDriver_IdleVehicle` : JobDriver

### `Vehicles.JobDriver_IdleVehicleDeSpawned` : JobDriver

### `Vehicles.JobDriver_LoadVehicle` : JobDriverLoadVehicleBase

- `Count` (int) = `> neededThings.Count`

### `Vehicles.JobDriver_PaintVehicle` : VehicleJobDriver

### `Vehicles.JobDriver_PrepareVehicleCaravan_GatheringItems` : JobDriverLoadVehicleBase

### `Vehicles.JobDriver_RefuelVehicle` : JobDriver

### `Vehicles.JobDriver_RefuelVehicleAtomic` : JobDriver

### `Vehicles.JobDriver_RemoveFuelFromVehicle` : JobDriver

### `Vehicles.JobDriver_RepairVehicle` : JobDriver_WorkVehicle

### `Vehicles.JobDriver_RopeAnimalToVehicle` : JobDriver_RopeToDestination

### `Vehicles.JobDriver_SabotageVehicle` : JobDriver_WorkVehicle

### `Vehicles.JobDriver_UpgradeVehicle` : JobDriver_WorkVehicle

### `Vehicles.JobDriver_WorkVehicle` : VehicleJobDriver

### `Vehicles.JobGiver_AwaitOrders` : ThinkNode_JobGiver

### `Vehicles.JobGiver_AwaitOrdersDeSpawned` : ThinkNode_JobGiver

### `Vehicles.JobGiver_BoardVehicle` : ThinkNode_JobGiver

### `Vehicles.JobGiver_BreachWalls` : JobGiver_RangedSupport

### `Vehicles.JobGiver_CarryPawnToVehicle` : ThinkNode_JobGiver

### `Vehicles.JobGiver_CombatFormation` : ThinkNode_JobGiver

### `Vehicles.JobGiver_ExitMap` : ThinkNode_JobGiver

### `Vehicles.JobGiver_ExitMapBest` : JobGiver_ExitMap

### `Vehicles.JobGiver_ExitMapRandom` : JobGiver_ExitMap

### `Vehicles.JobGiver_GotoNearestHostile` : ThinkNode_JobGiver

### `Vehicles.JobGiver_PrepareVehicleCaravan_GatheringItems` : ThinkNode_JobGiver

### `Vehicles.JobGiver_ProtectVehicle` : ThinkNode_JobGiver

### `Vehicles.JobGiver_RangedSupport` : JobGiver_CombatFormation

### `Vehicles.JobGiver_SabotageVehicle` : ThinkNode_JobGiver

### `Vehicles.JobGiver_SendSlavesToVehicle` : ThinkNode_JobGiver

### `Vehicles.Laser` : Bullet

### `Vehicles.ProjectileSkyfaller` : Thing

- `ticksToImpact` (int)
- `caster` (Thing)
- `projectileDef` (ThingDef)
- `origin` (Vector3)
- `destination` (Vector3)
- `reverseDraw` (bool)
- `speedTilesPerTick` (float) = `1f`

### `Vehicles.Projectile_Deflected` : Projectile

- `DeflectedArcHeight` (float) = `> 0f`

### `Vehicles.ThinkNode_ConditionalHasTurret` : ThinkNode_Conditional

### `Vehicles.ThinkNode_ConditionalVehicle` : ThinkNode_Conditional

### `Vehicles.ThinkNode_ConditionalVehicleState` : ThinkNode_Conditional

### `Vehicles.ThinkNode_ExecuteAction` : ThinkNode

### `Vehicles.VehicleBuilding` : Building

- `vehicle` (VehiclePawn)

### `Vehicles.VehicleIdManager` : GameComponent

### `Vehicles.VehicleJobDriver` : JobDriver

### `Vehicles.VehiclePathingSystem` : MapComponent

- `Suspended` (bool) = `> !VehicleRegionAndRoomUpdater.Enabled`
- `VehicleReachability` (VehicleReachability) = `> ReachabilityData.reachability`
- `VehicleRegionGrid` (VehicleRegionGrid) = `> ReachabilityData.regionGrid`
- `VehicleRegionMaker` (VehicleRegionMaker) = `> ReachabilityData.regionMaker`
- `VehicleRegionAndRoomUpdater` (VehicleRegionAndRoomUpdater) = `> ReachabilityData.regionAndRoomUpdater`
- `VehicleRegionDirtyer` (VehicleRegionDirtyer) = `> ReachabilityData.regionDirtyer`
- `regionGrid` (readonly VehicleRegionGrid)
- `regionMaker` (readonly VehicleRegionMaker)
- `regionAndRoomUpdater` (readonly VehicleRegionAndRoomUpdater)
- `regionDirtyer` (readonly VehicleRegionDirtyer)
- `reachability` (readonly VehicleReachability)
- `dedicatedThread` (DedicatedThread)
- `deferredGridGeneration` (DeferredGridGeneration)

### `Vehicles.VehiclePawn` : Pawn

- `vehicleAI` (VehicleAI)
- `vehiclePather` (VehiclePathFollower)
- `ignition` (VehicleIgnitionController)
- `sharedJob` (SharedJob)
- `cargoToLoad` (List<TransferableOneWay>)
- `handlers` (List<VehicleRoleHandler>) = `new List<VehicleRoleHandler>()`
- `beached` (bool)
- `statHandler` (VehicleStatHandler)
- `movementStatus` (VehicleMovementStatus) = `VehicleMovementStatus.Online`
- `patternData` (PatternData)
- `animator` (AnimationManager)
- `patternToPaint` (PatternData)
- `sustainers` (VehicleSustainers)
- `Drafted` (bool) = `> ignition.Drafted`
- `VehicleDef` (VehicleDef) = `> ((Thing)this).def as VehicleDef`
- `IsFishing` (bool) = `> fishing`
- `Handlers` (List<VehicleRoleHandler>) = `> handlers`
- `PawnCountToOperateLeft` (int) = `> PawnCountToOperate - PawnsByHandlingType[HandlingType.Move`
- `CanMoveWithOperators` (bool) = `> HasEnoughOperators`
- `Passengers` (List<Pawn>) = `> PawnsByHandlingType[HandlingType.None]`
- `CanPaintNow` (bool) = `> patternToPaint != null`
- `Nameable` (bool) = `> SettingsCache.TryGetValue(VehicleDef, typeof(VehicleDef), `
- `Thing` (ThingWithComps) = `> (ThingWithComps)(object)this`
- `Retexture` (RetextureDef) = `> retextureDef`
- `Transform` (Transform) = `> transform`
- `Drawer` (VehicleDrawTracker) = `> DrawTracker`
- `DrawTracker` (VehicleDrawTracker) = `> drawTracker`
- `MaterialCount` (int) = `> 8`
- `PatternDef` (PatternDef) = `> Pattern`
- `AttachedExplosives` (int) = `> explosives.Count`

### `Vehicles.VehicleSkyfaller` : Thing

- `angle` (float)
- `vehicle` (VehiclePawn)
- `Thing` (ThingWithComps) = `> (ThingWithComps)(object)vehicle`

### `Vehicles.VehicleSkyfaller_Arriving` : VehicleSkyfaller

- `delayLandingTicks` (int)
- `rotatePostLanding` (Rot4) = `Rot4.Invalid`

### `Vehicles.VehicleSkyfaller_Crashing` : VehicleSkyfaller_Arriving

- `rotCrashing` (Rot4)
- `shrapnelDirection` (float)
- `ticksToImpact` (int) = `220`

### `Vehicles.VehicleSkyfaller_FlyOver` : VehicleSkyfaller

- `start` (IntVec3)
- `end` (IntVec3)
- `aerialVehicle` (AerialVehicleInFlight)

### `Vehicles.VehicleSkyfaller_Leaving` : VehicleSkyfaller

- `arrivalAction` (IArrivalAction)
- `flightPath` (List<FlightNode>)
- `orderRecon` (bool)
- `createWorldObject` (bool) = `true`

### `Vehicles.VehicleSkyfaller_Strafe` : VehicleSkyfaller_FlyOver

### `Vehicles.VehicleStatWorker_BodyIntegrity` : VehicleStatWorker

### `Vehicles.VehicleStatWorker_MoveSpeed` : VehicleStatWorker

### `Vehicles.VehicleWorkGiver` : WorkGiver_Scanner

### `Vehicles.VerbProperties_Animated` : VerbProperties

- `shootOffset` (Vector2) = `Vector2.zero`
- `motes` (List<AnimationProperties>)

### `Vehicles.VerbProperties_Recoil` : VerbProperties_Animated

- `recoil` (RecoilProperties) = `new RecoilProperties()`

### `Vehicles.Verb_ShootRealistic` : Verb_Shoot

- `VerbProps` (VerbProperties_Recoil) = `> ((Verb)this).verbProps as VerbProperties_Recoil`

### `Vehicles.Verb_ShootRecoiled` : Verb_ShootRealistic

### `Vehicles.Verb_ShootWorldRecoiled` : Verb_ShootRecoiled

- `IsTargeting` (bool) = `> target.IsValid`

### `Vehicles.WorkGiver_BringUpgradeMaterial` : WorkGiver_CarryToVehicle

### `Vehicles.WorkGiver_CarryToVehicle` : WorkGiver_Scanner

### `Vehicles.WorkGiver_DisassembleVehicle` : WorkGiver_RemoveBuilding

### `Vehicles.WorkGiver_HelpGatheringItemsForVehicleCaravan` : WorkGiver

### `Vehicles.WorkGiver_PackVehicle` : WorkGiver_CarryToVehicle

### `Vehicles.WorkGiver_PaintVehicle` : VehicleWorkGiver

### `Vehicles.WorkGiver_RefuelVehicle` : WorkGiver_Scanner

### `Vehicles.WorkGiver_RefuelVehicleTurret` : WorkGiver_CarryToVehicle

### `Vehicles.WorkGiver_RemoveFuelFromVehicle` : WorkGiver_Scanner

### `Vehicles.WorkGiver_RepairVehicle` : VehicleWorkGiver

### `Vehicles.WorkGiver_WorkOnUpgrade` : VehicleWorkGiver

### `Vehicles.Raiders.PawnsArrivalModeWorker_Airdrop` : PawnsArrivalModeWorker

### `Vehicles.Rendering.Command_CooldownAction` : Command_Turret

### `Vehicles.Rendering.Command_TargeterCooldownAction` : Command_CooldownAction

### `Vehicles.Rendering.Command_Turret` : Command

- `targetingParams` (TargetingParameters)
- `canReload` (bool)
- `vehicle` (VehiclePawn)
- `turret` (VehicleTurret)

### `Vehicles.Rendering.Gizmo_RefuelableFuelTravel` : Gizmo_Slider

### `Vehicles.World.FlakWorker` : AntiAircraftWorker

### `Vehicles.World.IncidentWorker_ShuttleDowned` : IncidentWorker

### `Vehicles.World.VehicleRoutePlanner` : WorldComponent

### `Vehicles.World.VehicleWorldObjectsHolder` : WorldComponent

- `AerialVehicles` (List<AerialVehicleInFlight>) = `> aerialVehicles`
- `VehicleCaravans` (List<VehicleCaravan>) = `> vehicleCaravans`
- `StashedVehicles` (List<StashedVehicle>) = `> stashedVehicles`

### `Vehicles.World.WorldVehiclePathGrid` : WorldComponent

- `Owner` (VehicleDef) = `> owner`
- `pathGrids` (readonly PathGrid[])
- `reachability` (readonly WorldVehicleReachability)

### `Vehicles.World.WorldVehiclePathfinder` : WorldComponent

- `tile` (readonly int) = `tile`
- `cost` (readonly int) = `cost`
- `knownCost` (int)
- `heuristicCost` (int)
- `parentTile` (int)
- `costNodeCost` (int)
- `status` (ushort)
- `TileCacheSize` (int) = `> biomeDefs.Count * riverDefs.Count * roadDefs.Count * hills`

## Mod settings / ModBase (2)

### `Vehicles.VehicleMod` : Mod

### `Vehicles.VehiclesModSettings` : ModSettings

- `showAllCargoItems` (bool)
- `promptVehicleCollision` (bool)
- `main` (SectionMain) = `new SectionMain()`
- `vehicles` (SectionVehicles) = `new SectionVehicles()`
- `upgrades` (SectionUpgrades) = `new SectionUpgrades()`
- `debug` (SectionDebug) = `new SectionDebug()`
- `colorStorage` (ColorStorage) = `new ColorStorage()`

