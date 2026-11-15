# Referência de API: Vanilla Expanded Framework (VEF.dll)

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (35)

### `VEF.DefMigrationDef` : Def

- `type` (Type)
- `migrations` (List<DefMigration>) = `new List<DefMigration>()`
- `original` (string)
- `replacement` (string)
- `removeIfMissingReplacement` (bool)
- `providedClassName` (string)
- `migrations` (List<TypeMigration>) = `new List<TypeMigration>()`
- `newType` (Type)
- `baseType` (Type)
- `priority` (float)
- `migratedDefs` (List<DefMigrationsByType>)
- `migratedTypes` (List<TypeMigrationsByClassName>)

### `VEF.Abilities.AbilityDef` : Def

- `abilityClass` (Type)
- `needsTicking` (bool)
- `needsTickingInterval` (bool)
- `showUndrafted` (bool)
- `isPositive` (bool?)
- `keepCarryingThing` (bool?)
- `requiredHediff` (HediffWithLevelCombination)
- `requiredTrait` (TraitDef)
- `targetCount` (int) = `1`
- `targetModes` (List<AbilityTargetingMode>) = `new List<AbilityTargetingMode>()`
- `targetingParametersForAoE` (TargetingParametersForAoE)
- `targetingParametersList` (List<TargetingParameters>) = `new List<TargetingParameters>()`
- `range` (float)
- `maxRange` (float) = `float.MaxValue`
- `minRange` (float) = `-1f`
- `rangeStatFactors` (List<StatModifier>) = `new List<StatModifier>()`
- `rangeStatOffsets` (List<StatModifier>) = `new List<StatModifier>()`
- `radius` (float)
- `maxRadius` (float) = `float.MaxValue`
- `minRadius` (float) = `-1f`
- `radiusStatFactors` (List<StatModifier>) = `new List<StatModifier>()`
- `radiusStatOffsets` (List<StatModifier>) = `new List<StatModifier>()`
- `power` (float)
- `powerStatFactors` (List<StatModifier>) = `new List<StatModifier>()`
- `powerStatOffsets` (List<StatModifier>) = `new List<StatModifier>()`
- `castTime` (int)
- `castTimeStatFactors` (List<StatModifier>) = `new List<StatModifier>()`
- `castTimeStatOffsets` (List<StatModifier>) = `new List<StatModifier>()`
- `cooldownTime` (int)
- `cooldownTimeStatFactors` (List<StatModifier>) = `new List<StatModifier>()`
- `cooldownTimeStatOffsets` (List<StatModifier>) = `new List<StatModifier>()`
- `durationTime` (int)
- `durationTimeStatFactors` (List<StatModifier>) = `new List<StatModifier>()`
- `durationTimeStatOffsets` (List<StatModifier>) = `new List<StatModifier>()`
- `goodwillImpact` (int)
- `applyGoodwillImpactToLodgers` (bool) = `true`
- `worldTargeting` (bool)
- `hasAoE` (bool)
- `requireLineOfSight` (bool) = `true`
- `jobDef` (JobDef)
- … +31 campos (ver `VEF.Abilities\AbilityDef.cs`)

### `VEF.AnimalBehaviours.AnimalsDisabledFromQuestsDef` : Def

- `disabledFromQuestsPawns` (List<PawnKindDef>)

### `VEF.AnimalBehaviours.AnimalsUnaffectedBySelfTameDef` : Def

- `unaffectedBySelfTamePawns` (List<PawnKindDef>)

### `VEF.AnimalBehaviours.GenericToggleableAnimalDef` : Def

- `toggleablePawns` (List<string>)

### `VEF.AnimalBehaviours.RiverNeedingAnimalDef` : Def

- `riverNeedingAnimals` (List<ThingDef>)

### `VEF.AnimalGenes.AnimalGeneDef` : Def

- `iconPath` (string)
- `stability` (int)
- `statOffsets` (List<StatModifier>) = `new List<StatModifier>()`
- `statFactors` (List<StatModifier>) = `new List<StatModifier>()`
- `marketValueFactor` (float) = `1f`
- `isIllnessGene` (bool)
- `animalIllnessMTB` (float) = `60f`
- `animalTantrumMTB` (float)
- `hediffToAdd` (HediffDef)
- `abilityToAdd` (AbilityDef)
- `trainabilityDef` (TrainabilityDef)
- `familyTag` (AnimalGeneFamilyTagDef)
- `litterSizeCurveOverride` (SimpleCurve)
- `stillbirthChance` (float)
- `extraButcherProducts` (List<ThingDefCountClass>)
- `scaleButcherProductsByMeatAmount` (bool) = `true`
- `isSpecialized` (bool)
- `singleRankGene` (bool)
- `makeAnimalSterile` (bool)
- `dontGenerateInGeneTweakTools` (bool)
- `extraDescriptions` (string) = `""`
- `DescriptionFull` (string) = `> cachedDescription ?? (cachedDescription = GetDescriptionFu`
- `GeneLevel` (int) = `> 3 - stability`

### `VEF.AnimalGenes.AnimalGeneFamilyTagDef` : Def

- `order` (float)

### `VEF.AnimalGenes.FeratypeDef` : Def

- `iconPath` (string)
- `race` (PawnKindDef)
- `feratypeFamily` (FeratypeFamilyDef)
- `canBeAlpha` (bool)
- `animalGenes` (List<AnimalGeneDef>) = `new List<AnimalGeneDef>()`

### `VEF.AnimalGenes.FeratypeFamilyDef` : Def

### `VEF.Buildings.ConfigurableSpawnerDef` : Def

- `items` (List<string>)
- `allowedTerrains` (List<TerrainDef>)
- `listName` (string)
- `building` (string)
- `GizmoIcon` (string) = `""`
- `GizmoLabel` (string) = `""`
- `GizmoDescription` (string) = `""`
- `timeInTicks` (int) = `1000`
- `timeInterval` (Nullable)

### `VEF.Buildings.HiddenDesignatorsDef` : Def

- `hiddenDesignators` (List<BuildableDef>)

### `VEF.Factions.ContrabandDef` : Def

- `things` (List<ThingDef>)
- `chemicals` (List<ChemicalDef>)
- `thingCategories` (List<ThingCategoryDef>)
- `ignoreStuff` (bool)
- `ignoreRecipes` (bool)
- `giftedHistoryEvent` (HistoryEventDef)
- `soldHistoryEvent` (HistoryEventDef)
- `factions` (List<FactionDef>)
- `illegalFactions` (List<FactionDef>)
- `giftWarningKey` (string) = `"VEF.Factions.Contraband_Booze_GiftWarning"`
- `sellWarningKey` (string) = `"VEF.Factions.Contraband_Booze_SellWarning"`
- `giftIllegalFactionWarningKey` (string) = `"VEF.Factions.Contraband_Booze_GiftIllegalWarning"`
- `sellIllegalFactionWarningKey` (string) = `"VEF.Factions.Contraband_Booze_SellIllegalWarning"`
- `letterType` (LetterDef) = `LetterDefOf.ThreatSmall`
- `letterLabelKey` (string) = `"VEF.Factions.Contraband_Booze_LetterLabel"`
- `letterDescGiftKey` (string) = `"VEF.Factions.Contraband_Booze_LetterDescGift"`
- `letterDescSoldKey` (string) = `"VEF.Factions.Contraband_Booze_LetterDescSold"`
- `letterDescGifIllegalFactionKey` (string) = `"VEF.Factions.Contraband_Booze_LetterDescIllegalGift"`
- `letterDescSoldIllegalFactionKey` (string) = `"VEF.Factions.Contraband_Booze_LetterDescIllegalSold"`
- `relationInfoKey` (string) = `"VEF.Factions.Contraband_Booze_RelationInfo"`
- `chanceToGetCaught` (float) = `0.5f`
- `impactMultiplier` (float) = `-1f`
- `daysToImpact` (FloatRange) = `new FloatRange(7f, 14f)`
- `ImpactInTicks` (int) = `> Find.TickManager.TicksGame + 60 + (int)(60000f * daysToImp`

### `VEF.Graphics.CustomOverlayDef` : Def

- `workerClass` (Type) = `typeof(CustomOverlayWorker)`
- `pulsing` (bool) = `true`
- `useCustomOffset` (bool)
- `customOffset` (Vector3) = `Vector3.zero`
- `overlayPath` (string)
- `shaderType` (ShaderTypeDef)

### `VEF.Graphics.GraphicOffsets` : Def

- `thingDef` (ThingDef)
- `ingredientsAndOffsetList` (Dictionary<ThingDef, int>)

### `VEF.Graphics.SubEffecterDef_SlideTowardsTarget` : SubEffecterDef

- `ticksToEnd` (int) = `120`
- `ticksToEndOverrideByWeaponWarmup` (bool) = `true`
- `endPoint` (Vector3)
- `endPointZOverrideByWeapon` (bool)
- `startPointFactor` (FloatRange)
- `endPointFactor` (FloatRange)
- `scaleXByStart` (FloatRange)
- `scaleYByStart` (FloatRange)
- `scaleXByEnd` (FloatRange)
- `scaleYByEnd` (FloatRange)
- `minimumProgress` (FloatRange)

### `VEF.Maps.ActiveTerrainDef` : TerrainDef

- `terrainComps` (List<TerrainCompProperties>) = `new List<TerrainCompProperties>()`
- `terrainInstanceClass` (Type) = `typeof(TerrainInstance)`
- `tickerType` (TickerType)

### `VEF.Maps.ObjectSpawnsDef` : Def

- `thingDef` (ThingDef)
- `thingDefs` (List<ThingOption>)
- `category` (ThingCategoryDef)
- `pawnKindDef` (PawnKindDef)
- `factionDef` (FactionDef)
- `allowOnWater` (bool)
- `allowOnChunks` (bool)
- `numberToSpawn` (IntRange)
- `allowedTerrains` (List<string>)
- `disallowedTerrainTags` (List<string>)
- `allowedBiomes` (List<BiomeDef>)
- `allowedRoads` (List<RoadDef>)
- `findCellsOutsideColony` (bool)
- `spawnOnlyInPlayerMaps` (bool)
- `randomRotation` (bool)
- `allowSpawningOnPocketMaps` (bool)
- `allowedPlanetLayers` (List<PlanetLayerDef>)

### `VEF.Memes.StartingItemsByIdeologyDef` : Def

- `associatedMeme` (MemeDef)
- `thingSetMaker` (ThingSetMakerDef)

### `VEF.OptionalFeatures.ModDef` : Def

- `Activate` (List<string>) = `new List<string>()`

### `VEF.OptionalFeatures.OptionalFeaturesDef` : Def

- `feature` (string)
- `activationClass` (Type)
- `activationMethod` (MethodInfo)
- `harmonyCategory` (string)

### `VEF.Pawns.PregnancyApproachDef` : Def

- `cancelLabel` (string)
- `iconPath` (string)
- `icon` (CachedTexture)
- `pregnancyChanceForPartners` (float?)
- `pregnancyChanceFactorBase` (float?)
- `requiredGene` (GeneDef)
- `requireDifferentGender` (bool) = `true`
- `lovinDurationMultiplier` (float) = `1f`
- `requireFertility` (bool)

### `VEF.Planet.HireableFactionDef` : Def

- `color` (Color)
- `commTag` (string)
- `editBuffer` (string)
- `pawnKinds` (List<PawnKindDef>)
- `referencedFaction` (FactionDef)
- `texPath` (string)
- `Texture` (Texture2D) = `> texture ?? (texture = ContentFinder<Texture2D>.Get(texPath`

### `VEF.Planet.MovingBaseDef` : WorldObjectDef

- `baseFaction` (FactionDef)
- `ticksPerMove` (int) = `3300`
- `initialSpawnCount` (IntRange)
- `initialSpawnScalesWithPopulation` (bool)
- `attackConfirmationMessage` (string)

### `VEF.Planet.SiegeParameterSetDef` : Def

- `coverDef` (ThingDef)
- `artilleryBuildingTags` (List<string>)
- `artilleryCountRange` (IntRange)
- `mealDef` (ThingDef)
- `artilleryDefs` (List<ThingDef>) = `new List<ThingDef>()`
- `lowestArtilleryBlueprintPoints` (float) = `2.1474836E+09f`
- `maxArtilleryConstructionSkill` (int)

### `VEF.Storyteller.QuestChainDef` : Def

- `iconPath` (string)
- `icon` (Texture2D)
- `workerClass` (Type)
- `uniqueCharacters` (List<PawnKindDef>)
- `Worker` (QuestChainWorker) = `> cachedWorker`
- `State` (QuestChainState) = `> GameComponent_QuestChains.Instance?.GetStateFor(this)`

### `VEF.Storyteller.QuestGiverDef` : Def

- `currency` (QuestCurrency)
- `fixedQuestGiverFaction` (FactionDef)
- `onlySpecifiedQuests` (List<QuestScriptDef>)
- `resetEveryTick` (int) = `-1`
- `hideGeneratedQuestsInVanilla` (bool)
- `generateOnce` (bool)
- `onlyOneReward` (bool)
- `maximumAvailableQuestCount` (int) = `-1`
- `windowClass` (Type) = `typeof(Window_Contracts)`
- `windowTitleKey` (string)

### `VEF.Storyteller.StructureSetDef` : Def

- `structureLayouts` (List<StructurePatternOffset>)

### `VEF.Things.CustomFireDef` : ThingDef

- `maxFireSize` (float) = `1.75f`
- `fireBaseGrowth` (float) = `0.082499996f`
- `damageBase` (float) = `3.75f`
- `damagePerFireSize` (float) = `1.0799999f`
- `damageRange` (FloatRange) = `new FloatRange(3.75f, 15f)`
- `heatBase` (float)
- `heatPerFireSize` (float) = `160f`
- `snowClearRadiusBase` (float)
- `snowClearRadiusPerFireSize` (float) = `3f`
- `snowClearDepthBase` (float)
- `snowClearDepthPerFireSize` (float) = `0.1f`
- `rainExtinguishDamage` (float) = `10f`
- `rainExtinguishDamageChance` (float) = `1f`
- `vacuumExtinguishDamage` (float) = `20f`
- `vacuumExtinguishDamageChance` (float) = `1f`
- `vacuumAffectsFireSizeGrowth` (bool) = `true`
- `minSizeToIgnitePawns` (float) = `0.4f`
- `selfExtinguishDuration` (int) = `150`
- `affectedByExtinguishDamage` (bool) = `true`
- `causesPanic` (bool) = `true`
- `canExtinguishInWater` (bool) = `true`
- `chanceToSelfExtinguish` (float) = `0.1f`
- `minSizeForFireSpread` (float) = `1f`
- `spreadIntervalTicksBase` (float) = `150f`
- `spreadIntervalTicksReductionPerFireSize` (float) = `40f`
- `minTicksBetweenFireSpread` (float) = `75f`
- `chanceForNonAdjacentSpread` (float) = `0.2f`
- `beatFireExtinguishDamageAmount` (float) = `16f`
- `ticksToBurnFloor` (int) = `7500`
- `damagesApparel` (bool) = `true`
- `replacesFire` (bool)
- `useVanillaFireAlert` (bool) = `true`
- `useDefaultWorkGiver` (bool) = `true`
- `smokeIntervalRange` (IntRange) = `new IntRange(130, 200)`
- `smokeIntervalRandomAddition` (int) = `10`
- `throwSmoke` (bool) = `true`
- `throwFireGlow` (bool) = `true`
- `fireSizeForMicroSparks` (float) = `0.7f`
- `microSparksChancePerFireSize` (float) = `0.01f`
- `translationKeyFireOn` (string) = `"FireOn"`
- … +17 campos (ver `VEF.Things\CustomFireDef.cs`)

### `VEF.Weapons.Building_LaserGunDef` : ThingDef

- `beamPowerConsumption` (int) = `20`
- `supportsColors` (bool)

### `VEF.Weapons.ExpandableProjectileDef` : ThingDef

- `lifeTimeDuration` (int) = `100`
- `widthScaleFactor` (float) = `1f`
- `heightScaleFactor` (float) = `1f`
- `startingPositionOffset` (Vector3) = `Vector3.zero`
- `totalSizeScale` (float) = `1f`
- `graphicData` (ExpandableGraphicData)
- `tickFrameRate` (int) = `1`
- `finalTickFrameRate` (int)
- `tickDamageRate` (int) = `60`
- `minDistanceToAffect` (float)
- `disableVanillaDamageMethod` (bool)
- `dealsDamageOnce` (bool)
- `reachMaxRangeAlways` (bool)
- `arcSize` (float) = `2.5f`
- `debugMode` (bool)
- `wideAtStart` (bool)
- `stopMotionOnFadeoutStarted` (bool)
- `impactFleck` (FleckDef)
- `triggerWaterSplashes` (bool)
- `impactSound` (SoundDef)
- `impactFleckRotation` (FloatRange) = `FloatRange.Zero`
- `impactFleckRotationRate` (FloatRange) = `FloatRange.Zero`
- `impactFleckAngle` (FloatRange) = `FloatRange.Zero`
- `impactFleckSpeed` (FloatRange) = `FloatRange.Zero`
- `impactFleckUsesProjectileAngle` (bool)
- `filthOnUninterrupted` (ThingDef)
- `filthOnUninterruptedChance` (float) = `1f`
- `filthOnUninterruptedCount` (IntRange)
- `stopWhenHit` (bool) = `true`
- `stopAtBuildingWithCover` (float) = `1f`
- `stopWhenNaturalRockHit` (bool)
- `stopWhenZeroDamageAfterHit` (bool)
- `stopWhenHitAt` (List<string>) = `new List<string>()`
- `gauss` (GaussProperties)
- `IsGaussProjectile` (bool) = `> GenTypes.SameOrSubclassOf<GaussProjectile>(base.thingClass`

### `VEF.Weapons.LaserBeamDef` : ThingDef

- `capSize` (float) = `1f`
- `capOverlap` (float) = `0.0171875f`
- `lifetime` (int) = `30`
- `flickerFrameTime` (int) = `5`
- `impulse` (float) = `4f`
- `beamWidth` (float) = `1f`
- `shieldDamageMultiplier` (float) = `0.5f`
- `seam` (float) = `-1f`
- `causefireChance` (float) = `-1f`
- `canExplode` (bool)
- `decorations` (List<LaserBeamDecoration>)
- `explosionEffect` (EffecterDef)
- `hitLivingEffect` (EffecterDef)
- `beamGraphic` (ThingDef)
- `textures` (List<string>)
- `materials` (List<Material>) = `new List<Material>()`
- `IsWeakToShields` (bool) = `> shieldDamageMultiplier < 1f`

### `VEF.Weapons.LaserGunDef` : ThingDef

- `barrelLength` (float) = `0.9f`
- `supportsColors` (bool)

### `VEF.Weapons.SpinningLaserGunDef` : LaserGunDef

- `frames` (List<GraphicData>)
- `rotationSpeed` (float) = `1f`

### `VEF.Weapons.ToolsUsableByNonViolentPawnsDef` : Def

- `toolsUsableByNonViolentPawns` (List<ThingDef>)

## DefModExtensions (`<modExtensions>`) (91)

### `VEF.Abilities.AbilityExtension_AbilityMod` : DefModExtension

- `abilityDef` (AbilityDef)

### `VEF.Abilities.AbilityExtension_AdditionalRadius` : AbilityExtension_AbilityMod

- `radius` (float)
- `radiusStatFactors` (List<StatModifier>) = `new List<StatModifier>()`

### `VEF.Abilities.AbilityExtension_Building` : DefModExtension

- `building` (ThingDef)

### `VEF.Abilities.AbilityExtension_ClassMigration` : DefModExtension

- `oldClass` (string)

### `VEF.Abilities.AbilityExtension_ConfirmationMessage` : AbilityExtension_AbilityMod

- `message` (string)

### `VEF.Abilities.AbilityExtension_EffecterOnTarget` : AbilityExtension_AbilityMod

- `onCaster` (bool)
- `effecterDef` (EffecterDef)
- `maintainForTicks` (int) = `-1`
- `scale` (float) = `1f`
- `maintainForDuration` (bool)

### `VEF.Abilities.AbilityExtension_Explosion` : DefModExtension

- `applyDamageToExplosionCellsNeighbors` (bool)
- `chanceToStartFire` (float)
- `damageFalloff` (bool)
- `explosionArmorPenetration` (float) = `-1f`
- `explosionDamageAmount` (int) = `-1`
- `explosionDamageDef` (DamageDef)
- `explosionDirection` (float?)
- `explosionRadius` (float)
- `explosionSound` (SoundDef)
- `onCaster` (bool)
- `postExplosionSpawnChance` (float)
- `postExplosionSpawnThingCount` (int) = `1`
- `postExplosionGasType` (Nullable)
- `postExplosionGasRadiusOverride` (float?)
- `postExplosionGasAmount` (int) = `255`
- `postExplosionSpawnThingDef` (ThingDef)
- `preExplosionSpawnChance` (float)
- `preExplosionSpawnThingCount` (int) = `1`
- `preExplosionSpawnThingDef` (ThingDef)
- `casterImmune` (bool)

### `VEF.Abilities.AbilityExtension_ExtraHediffs` : AbilityExtension_AbilityMod

- `onTarget` (List<HediffDef>)
- `onCaster` (List<HediffDef>)
- `durationMultiplier` (StatDef)
- `durationTimeOverride` (int?)

### `VEF.Abilities.AbilityExtension_FleckOnTarget` : AbilityExtension_AbilityMod

- `allTargets` (bool)
- `fleckDef` (FleckDef)
- `fleckDefs` (List<FleckDef>)
- `preCastTicks` (int) = `-1`
- `scale` (float) = `1f`
- `sound` (SoundDef)
- `tryCenter` (bool)

### `VEF.Abilities.AbilityExtension_GiveMentalState` : AbilityExtension_AbilityMod

- `stateDef` (MentalStateDef)
- `stateDefForMechs` (MentalStateDef)
- `durationMultiplier` (StatDef)
- `durationScalesWithCaster` (bool)
- `applyToSelf` (bool)
- `clearOthers` (bool)
- `causedByPsycast` (bool)

### `VEF.Abilities.AbilityExtension_Hediff` : AbilityExtension_AbilityMod

- `hediff` (HediffDef)
- `bodyPartToApply` (BodyPartDef)
- `severity` (float) = `-1f`
- `applyAuto` (bool) = `true`
- `durationMultiplier` (StatDef)
- `durationMultiplierFromCaster` (bool)
- `targetOnlyEnemies` (bool)
- `applyToCaster` (bool)

### `VEF.Abilities.AbilityExtension_Projectile` : DefModExtension

- `projectile` (ThingDef)
- `soundOnImpact` (SoundDef)
- `forcedMissRadius` (float)
- `hitFlags` (ProjectileHitFlags) = `(ProjectileHitFlags)1`
- `accuracyStatFactors` (List<StatModifier>) = `new List<StatModifier>()`
- `accuracyStatOffsets` (List<StatModifier>) = `new List<StatModifier>()`

### `VEF.Abilities.AbilityExtension_RandomPowerMultiplier` : DefModExtension

- `range` (FloatRange) = `FloatRange.One`

### `VEF.Abilities.AbilityExtension_RandomRadius` : DefModExtension

- `minRadius` (float)
- `maxRadius` (float) = `1f`

### `VEF.Abilities.AbilityExtension_ScreenShaker` : AbilityExtension_AbilityMod

- `intensity` (float)

### `VEF.Abilities.AbilityExtension_Shield` : DefModExtension

- `shieldTexPath` (string)

### `VEF.Abilities.AbilityExtension_ShootProjectile_Snow` : DefModExtension

- `radius` (float) = `3f`
- `depth` (float) = `1f`

### `VEF.Abilities.AbilityExtension_Skyfaller` : DefModExtension

- `skyfaller` (ThingDef)
- `rocks` (int)

### `VEF.Abilities.AbilityExtension_SocialInteraction` : AbilityExtension_AbilityMod

- `interactionDef` (InteractionDef)
- `canApplyToMentallyBroken` (bool)
- `canApplyToUnconscious` (bool)

### `VEF.Abilities.AbilityExtension_Spawn` : DefModExtension

- `allowOnBuildings` (bool)
- `thing` (ThingDef)

### `VEF.Abilities.AbilityExtension_Stun` : AbilityExtension_AbilityMod

- `stunTicks` (Nullable)
- `durationMultiplier` (StatDef)
- `durationMultiplierFromCaster` (bool)

### `VEF.Abilities.AbilityExtension_ThoughtOnTarget` : AbilityExtension_AbilityMod

- `thoughtDef` (ThoughtDef)

### `VEF.Abilities.AbilityExtension_WarmupEffecter` : AbilityExtension_AbilityMod

- `onCaster` (bool)
- `effecterDef` (EffecterDef)
- `scale` (float) = `1f`

### `VEF.Abilities.PawnKindAbilityExtension` : DefModExtension

- `giveAbilities` (List<AbilityDef>) = `new List<AbilityDef>()`
- `implantDef` (HediffDef)
- `initialLevel` (int) = `1`
- `giveRandomAbilities` (bool)

### `VEF.AnimalBehaviours.AnimalCrossbreedExtension` : DefModExtension

- `crossBreedKindDef` (FatherOrMother)
- `otherPawnKindsByWeight` (List<PawnKindDefWeight>)
- `otherPawnKind` (PawnKindDef)
- `weightForMother` (float) = `0.5f`
- `specificCombinationsOverride` (SpecificCombinations)

### `VEF.AnimalBehaviours.AnimalStatExtension` : DefModExtension

- `statToAdd` (List<string>)
- `statValues` (List<string>)
- `statDescriptions` (List<string>)
- `showImageInInfoCard` (bool)
- `ImageToShowInInfoCard` (string) = `"UI/EmptyImage"`

### `VEF.AnimalBehaviours.ExtendedRaceProperties` : DefModExtension

- `bodyClock` (BodyClock)

### `VEF.Apparels.ApparelExtension` : DefModExtension

- `priority` (float)
- `skillGainModifier` (float) = `1f`
- `workDisables` (WorkTags)
- `skillDisables` (List<SkillDef>)
- `equippedStatFactors` (List<StatModifier>)
- `traitsOnEquip` (List<TraitRequirement>)
- `traitsOnUnequip` (List<TraitRequirement>)
- `pawnCapacityMinLevels` (List<PawnCapacityMinLevel>)
- `preventDowning` (bool)
- `preventKilling` (bool)
- `preventKillingUntilHealthHPPercentage` (float) = `1f`
- `preventKillingUntilBrainMissing` (bool)
- `preventBleeding` (bool)
- `destroyedOnDeath` (bool)
- `secondaryApparelGraphics` (List<ThingDef>)
- `isUnifiedApparel` (bool)
- `hideHead` (bool)
- `showBodyInBedAlways` (bool)
- `moveSpeedFactorByTerrainTag` (Dictionary<string, List<MoveSpeedFactor>>)
- `Priority` (float) = `> priority`

### `VEF.Buildings.AffectedByFacilitiesExtension` : DefModExtension

- `copyLinksFrom` (List<ThingDef>)
- `disableFacilityExtensionLinking` (bool)

### `VEF.Buildings.ColorModExtension` : DefModExtension

- `colorOptions` (List<ColorOption>) = `new List<ColorOption>()`

### `VEF.Buildings.DoorTeleporterExtension` : DefModExtension

- `doorTeleporterTexPath` (string)
- `doorTeleporterMaskPath` (string)
- `maskThreshold` (float)
- `doorTeleporterBackgroundPath` (string)
- `mainMatPath` (string)
- `distortionMatPath` (string)
- `distortionMaskPath` (string)
- `destroyIconPath` (string)
- `renameIconPath` (string)
- `destroyLabelKey` (string)
- `destroyDescKey` (string)
- `renameLabelKey` (string)
- `renameDescKey` (string)
- `sustainer` (SoundDef)

### `VEF.Buildings.FacilityExtension` : DefModExtension

- `linkOnInteractionSpots` (bool)
- `equivalentToFacility` (ThingDef)
- `copyLinksFrom` (List<ThingDef>)
- `disableAffectedByFacilitiesExtensionLinking` (bool)

### `VEF.Buildings.GhostGraphicExtension` : DefModExtension

- `texPath` (string)
- `graphicClass` (Type)
- `drawSize` (Nullable)
- `drawRotated` (bool?)
- `allowFlip` (bool?)
- `ghostMode` (CustomGhostMode)
- `extraGraphicGhostMode` (CustomGhostMode)
- `customGraphicData` (GraphicDataOverride)
- `extraCustomGraphicData` (GraphicDataOverride)

### `VEF.Buildings.GravshipLaunchExtension` : DefModExtension

- `isOxygenPusher` (bool)
- `isHeater` (bool)

### `VEF.Buildings.LootBoxExtension` : DefModExtension

- `thingSetMakerDef` (ThingSetMakerDef)
- `totalMarketValueRange` (FloatRange) = `new FloatRange(850f, 1000f)`
- `minSingleItemMarketValuePct` (float?)
- `allowNonStackableDuplicates` (bool) = `true`
- `countRange` (IntRange) = `new IntRange(1, 1)`

### `VEF.Buildings.LootableBuildingDetails` : DefModExtension

- `randomFromContents` (bool)
- `totalRandomLoops` (IntRange) = `new IntRange(1, 1)`
- `contents` (List<ThingAndCount>)
- `buildingLeft` (ThingDef)
- `deconstructSound` (SoundDef)
- `gizmoTexture` (string)
- `gizmoText` (string)
- `gizmoDesc` (string)
- `cancelLootingGizmoTexture` (string)
- `cancelLootinggizmoText` (string)
- `cancelLootinggizmoDesc` (string)
- `requiredMod` (string) = `""`
- `overlayTexture` (string)
- `secondsToOpen` (int) = `20`
- `useHackingSpeed` (bool)
- `useThingSetMakerDef` (bool)
- `setMakerDetails` (ThingSetMakerDetails)

### `VEF.Buildings.ProjectileInterceptorExtension` : DefModExtension

- `healthColorPoints` (List<HealthColorPoint>)

### `VEF.Buildings.RecipeInheritanceExtension` : DefModExtension

- `allowedProductFilter` (ThingFilter)
- `allowedRecipes` (List<RecipeDef>)
- `disallowedProductFilter` (ThingFilter)
- `disallowedRecipes` (List<RecipeDef>)
- `inheritRecipesFrom` (List<ThingDef>)

### `VEF.Buildings.RefuelableExtension` : DefModExtension

- `ejectingFuelRespectsFuelMultiplier` (bool)
- `customFuelGauge` (CustomFillableBarGaugeData)

### `VEF.Buildings.ResearchBuildingExtension` : DefModExtension

- `equivalentBenches` (List<ThingDef>)
- `equivalentFacilities` (List<ThingDef>)
- `Priority` (float) = `> 0f`

### `VEF.Buildings.ShowBlueprintExtension` : DefModExtension

- `showBlueprintInGhostMode` (bool) = `true`

### `VEF.Buildings.StudiableBuildingDetails` : DefModExtension

- `buildingLeft` (ThingDef)
- `deconstructSound` (SoundDef)
- `gizmoTexture` (string)
- `gizmoText` (string)
- `gizmoDesc` (string)
- `craftingInspiration` (bool)
- `skillForStudying` (SkillDef)
- `overlayTexture` (string)
- `showProgressBar` (bool)
- `showResearchEffecter` (bool) = `true`

### `VEF.Buildings.SwappableBuildingDetails` : DefModExtension

- `buildingLeft` (ThingDef)
- `deconstructSound` (SoundDef)
- `swappingTimer` (int) = `-1`

### `VEF.Buildings.ThoughtGiverByProximityDefExtension` : DefModExtension

- `ThingToGiveThought` (ThingDef)
- `DistanceToGiveThought` (float) = `15f`

### `VEF.Cooking.Recipe_Extension` : RecipeExtension

### `VEF.Cooking.Thought_Hediff_Extension` : DefModExtension

- `hediffToAffect` (HediffDef)
- `partToAffect` (BodyPartDef)
- `percentage` (float) = `1f`
- `secondHediffToAffect` (HediffDef)
- `secondPartToAffect` (BodyPartDef)
- `secondPercentage` (float) = `1f`
- `increaseJoy` (bool)
- `extraJoy` (float)

### `VEF.Factions.FactionDefExtension` : DefModExtension

- `hasCities` (bool) = `true`
- `settlementGenerationSymbol` (string)
- `packAnimalTexNameSuffix` (string)
- `strangerInBlackReplacement` (PawnKindDef)
- `siegeParameterSetDef` (SiegeParameterSetDef)
- `startingGoodwillByFactionDefs` (List<StartingGoodwillByFaction>) = `new List<StartingGoodwillByFaction>()`
- `allowedBiomes` (List<BiomeDef>) = `new List<BiomeDef>()`
- `disallowedBiomes` (List<BiomeDef>) = `new List<BiomeDef>()`
- `requiredHillLevels` (List<Hilliness>)
- `spawnOnCoastalTilesOnly` (bool)
- `neverConnectToRoads` (bool)
- `minDistanceToOtherSettlements` (float)
- `excludeFromCommConsole` (bool)
- `excludeFromQuests` (bool)
- `allowedStrategies` (List<RaidStrategyDef>) = `new List<RaidStrategyDef>()`
- `forcedFactionData` (ForcedFactionData) = `new ForcedFactionData()`

### `VEF.Genes.AbilityGeneExtension` : DefModExtension

- `dontModifyAbilityOnGeneRemoval` (bool)

### `VEF.Genes.GeneExtension` : DefModExtension

- `minOffset` (float)
- `maxOffset` (float)
- `range` (FloatRange) = `new FloatRange(0f, 0f)`
- `renderCacheOff` (bool)
- `backgroundPathEndogenes` (string)
- `backgroundPathXenogenes` (string)
- `backgroundPathArchite` (string)
- `hediffsToBodyParts` (List<HediffToBodyparts>)
- `hediffToWholeBody` (HediffDef)
- `useSkinColorForFur` (bool)
- `useSkinAndHairColorsForFur` (bool)
- `dontColourFur` (bool)
- `useMaskForFur` (bool)
- `furHidesBody` (bool)
- `thingSetMaker` (ThingSetMakerDef)
- `forceMale` (bool)
- `forceFemale` (bool)
- `forGenderOnly` (Gender)
- `customBloodThingDef` (ThingDef)
- `customBloodIcon` (string) = `""`
- `customBloodEffect` (EffecterDef)
- `customWoundsFromFleshtype` (FleshTypeDef)
- `customBloodSmearThingDef` (ThingDef)
- `customVomitThingDef` (ThingDef)
- `customVomitEffect` (EffecterDef)
- `customMeatThingDef` (ThingDef)
- `customLeatherThingDef` (ThingDef)
- `defsTreatedAsHumanMeat` (List<ThingDef>)
- `foodTypesTreatedAsHumanMeat` (List<FoodTypeFlags>)
- `defsTreatedAsHumanLeather` (List<ThingDef>)
- `diseaseProgressionFactor` (float) = `1f`
- `hideGene` (bool)
- `disableGeneExtraction` (bool)
- `noSkillLoss` (SkillDef)
- `skillRecreation` (SkillDef)
- `globalSkillLossMultiplier` (float) = `1f`
- `skillDegradation` (bool)
- `pregnancySpeedFactor` (float) = `1f`
- `foodBingeMentalBreakSelectionChanceFactor` (float) = `1f`
- `doubleNegativeFoodThought` (bool)
- … +11 campos (ver `VEF.Genes\GeneExtension.cs`)

### `VEF.Graphics.DynamicGraphicProps` : DefModExtension

- `dataList` (List<ExtendedGraphicData>)

### `VEF.Graphics.FloorGraphicExtension` : DefModExtension

- `graphicData` (GraphicData)

### `VEF.Graphics.HeadExtension` : DefModExtension

- `forcedHeadShader` (ShaderTypeDef)

### `VEF.Graphics.TaggedDefProperties` : DefModExtension

- `generateAdvancedColors` (List<TaggedAdvancedColor>) = `new List<TaggedAdvancedColor>()`
- `generateRandomStrings` (List<List<TaggedText>>) = `new List<List<TaggedText>>()`
- `taggedColors` (List<TaggedColor>) = `new List<TaggedColor>()`
- `taggedStrings` (List<TaggedText>) = `new List<TaggedText>()`

### `VEF.Maps.DefExtensionActive` : DefModExtension

### `VEF.Maps.DefExtension_ShaderSpeedMult` : DefExtensionActive

### `VEF.Maps.MapConditionExtension` : DefModExtension

- `tideStrengthMultiplier` (float) = `1f`
- `watermillStrengthMultiplier` (float) = `1f`

### `VEF.Maps.RoofExtension` : DefModExtension

- `drawSize` (Vector2)
- `offset` (Vector3)
- `layer` (AltitudeLayer)
- `material` (Material)
- `drawSize` (Vector2) = `Vector2.one`
- `offset` (Vector3) = `Vector3.zero`
- `layer` (AltitudeLayer) = `(AltitudeLayer)30`
- `customRoofGraphicPath` (string)
- `customRoofGraphicShader` (ShaderTypeDef)
- `customRoofGraphicColor` (Color) = `Color.white`
- `renderQueue` (int)
- `drawRoofShadow` (bool) = `true`
- `dealDamageOnCollapsed` (bool) = `true`
- `roofOverlayColor` (Nullable)
- `customRoofGraphic` (CustomRoofGraphic)

### `VEF.Maps.TileMutatorExtension` : DefModExtension

- `forcedPawnKindDefs` (List<PawnKindDefAndChance>)
- `thingToSpawn` (ThingDef)
- `thingToSpawnAmount` (IntRange)
- `terrainValidation` (List<TerrainDef>)
- `allowWater` (bool) = `true`
- `prefabsToSpawn` (List<PrefabDef>)
- `prefabsToSpawnAmount` (IntRange) = `new IntRange(1, 1)`
- `minSeparationBetweenPrefabs` (int) = `10`
- `KCSGStructuresToSpawn` (List<StructureLayoutDef>)
- `KCSGStructuresToSpawnAmount` (IntRange) = `new IntRange(1, 1)`
- `minSeparationBetweenKCSGStructures` (int) = `10`
- `terrainToSwap` (TerrainDef)
- `terrainToSwapTo` (TerrainDef)
- `mapSizeOverrideX` (int) = `-1`
- `mapSizeOverrideZ` (int) = `-1`
- `mapSizeMultiplier` (float) = `1f`
- `deepOresMultiplier` (float) = `1f`
- `diseaseMTBMultiplier` (float) = `1f`
- `movementDifficultyOffset` (float)
- `riverbankSizeMultiplier` (int) = `1`
- `plantDefsWithCommonality` (List<PlantsWithCommonality>)
- `extraDeepHelixienGasDeposits` (int)
- `tideStrengthMultiplier` (float) = `1f`

### `VEF.Maps.WorldObjectExtension` : DefModExtension

- `disableObjectSpawnDefs` (bool)

### `VEF.Memes.ExtendedMemeProperties` : DefModExtension

- `neededMeme` (string)
- `requiredMemes` (List<string>)
- `forcedTrait` (TraitDef)
- `factionOpinionOffset` (int)
- `abilitiesGiven` (List<AbilityDef>)
- `removedDesignators` (List<ThingDef>)

### `VEF.Pawns.PawnKindDefExtension` : DefModExtension

- `factionColourApparelPartList` (List<BodyPartGroupDef>)
- `factionColourApparelLayerList` (List<ApparelLayerDef>)
- `shieldTags` (List<string>)
- `shieldMoney` (FloatRange)

### `VEF.Pawns.ThoughtExtensions` : DefModExtension

- `removeThoughtsWhenAdded` (List<ThoughtDef>)

### `VEF.Pawns.TraitExtension` : DefModExtension

- `painFactor` (float)
- `apparelExclusiveTrait` (bool)

### `VEF.Planet.EquipmentOffsetConditions` : DefModExtension

- `techLevels` (List<TechLevel>)

### `VEF.Plants.BloomingPlantExtension` : DefModExtension

- `AgeBeautyModifier` (int)
- `MaxAgeBeautyModifier` (int)
- `BloomBeautyModifier` (float)
- `LeaflessBeauty` (int)
- `WeededBeauty` (int) = `-4`
- `BloomSeasonStart` (Season)
- `BloomDayStart` (int) = `1`
- `BloomSeasonStop` (Season)
- `BloomDayEnd` (int) = `1`
- `CanBloomAgain` (bool) = `true`
- `BloomTemperatureMin` (int) = `-250`
- `BloomTemperatureMax` (int) = `999`
- `DeadlyColdTemperature` (int) = `-250`
- `DamageWhenBelowDeadlyTemp` (int) = `30`
- `BloomLightMax` (float) = `1f`
- `bloomGraphicPath` (string)
- `alternateBloomGraphicPath` (string) = `""`
- `ImmuneToWeeds` (bool)
- `DisableJoyGiver` (bool)
- `CantBeExtracted` (bool)
- `itemProducedWhenBlooming` (ThingDef)
- `longTicksPerItemProduced` (int) = `1`
- `itemProducedAmount` (int) = `1`
- `filthProducedWhenBlooming` (ThingDef)
- `longTicksPerFilthProduced` (int) = `1`
- `filthProducedAmount` (IntRange) = `IntRange.One`
- `filthProducedRadius` (float) = `1f`
- `hediffWhenBlooming` (HediffDef)
- `hediffRadius` (float) = `1f`
- `hediffSeverity` (float) = `1f`
- `hediffOnlyAffectsColonists` (bool) = `true`

### `VEF.Plants.DualCropExtension` : DefModExtension

- `secondaryOutput` (ThingDef)
- `outPutAmount` (int)
- `randomOutput` (bool)
- `randomSecondaryOutput` (List<ThingDef>)

### `VEF.Plants.SowerExtension` : DefModExtension

- `extraSowTags` (List<string>)

### `VEF.Plants.WinterBloomingExtension` : DefModExtension

- `graphicSowing` (GraphicData)
- `graphicWinter` (GraphicData)

### `VEF.Storyteller.IncidentDefExtension` : DefModExtension

- `forcedFaction` (FactionDef)
- `forcedPointsRange` (IntRange) = `IntRange.Zero`
- `forcedStrategy` (RaidStrategyDef)

### `VEF.Storyteller.PrefabExtension` : DefModExtension

- `roofs` (List<PrefabRoofData>)
- `pawns` (List<PrefabPawnSpawnData>)

### `VEF.Storyteller.QuestChainExtension` : DefModExtension

- `questChainDef` (QuestChainDef)
- `conditionSucceedQuests` (List<QuestScriptDef>)
- `conditionFailQuests` (List<QuestScriptDef>)
- `ticksSinceSucceed` (IntRange)
- `ticksSinceFail` (IntRange)
- `conditionEither` (QuestScriptDef)
- `conditionMinDaysSinceStart` (FloatRange)
- `isRepeatable` (bool)
- `mtbDaysRepeat` (float)
- `grantAgainOnFailure` (bool)
- `daysUntilGrantAgainOnFailure` (FloatRange)
- `grantAgainOnSuccess` (bool)
- `daysUntilGrantAgainOnSuccess` (FloatRange)
- `grantAgainOnExpiry` (bool)
- `daysUntilGrantAgainOnExpiry` (FloatRange)
- `delayTicksAfterTriggering` (Nullable)
- `conditionSucceedQuestsCount` (List<ConditionSucceedQuestsCount>)
- `requiredResearch` (ResearchProjectDef)

### `VEF.Storyteller.StorytellerDefExtension` : DefModExtension

- `raidRestlessness` (RaidRestlessness)
- `storytellerThreat` (StorytellerThreat)
- `incidentSpawnOptions` (IncidentSpawnOptions)

### `VEF.Storyteller.UniqueCharacterExtension` : DefModExtension

- `tag` (string)

### `VEF.Things.CustomFireDamageExtension` : DefModExtension

- `fireDef` (CustomFireDef)
- `initialAttachedFireSize` (FloatRange) = `new FloatRange(0.15f, 0.25f)`
- `initialExplosionFireSize` (FloatRange) = `new FloatRange(0.2f, 0.6f)`
- `spawnFilthOnThingDestroyed` (bool) = `true`
- `filthOnThingDestroyed` (ThingDef)
- `ignoreFireDamageDefForExplosions` (bool)

### `VEF.Things.RecipeExtension` : DefModExtension

- `individualIngredients` (bool)
- `chunksAsStuff` (bool)

### `VEF.Things.StuffExtension` : DefModExtension

- `structureGenerationCommonalityFactor` (float?)
- `weaponGenerationCommonalityFactor` (float?)
- `apparelGenerationCommonalityFactor` (float?)
- `structureGenerationCommonalityOffset` (float?)
- `weaponGenerationCommonalityOffset` (float?)
- `apparelGenerationCommonalityOffset` (float?)

### `VEF.Things.ThingDefExtension` : DefModExtension

- `usableWithShields` (bool?) = `false`
- `weaponCarryDrawOffsets` (WeaponDrawOffsets)
- `weaponDraftedDrawOffsets` (WeaponDrawOffsets)
- `useFactionColourForPawnKinds` (List<PawnKindDef>)
- `siegeBlueprintPoints` (float) = `60f`
- `deepColor` (Color) = `Color.white`
- `transparencyMultiplier` (float) = `0.5f`
- `allowDeepDrill` (bool) = `true`
- `deepResourcesOnGUI` (bool)
- `deepResourcesOnGUIRequireScanner` (bool) = `true`
- `shieldDamageIntercepted` (int) = `-1`
- `destroyCorpse` (bool)
- `constructionSkillRequirement` (ConstructionSkillRequirement)
- `playerCraftedStyles` (List<ThingStyleChance>)
- `playerCraftedStylesOverrideOtherStyles` (bool)
- `playerCraftedStyleChance` (float) = `1f`

### `VEF.Weapons.AutoRefuelMannedTurrets` : DefModExtension

- `reloadsMoreThanSingleItem` (bool)

### `VEF.Weapons.DamageExtension` : DefModExtension

- `pushBackDistance` (FloatRange)
- `soundOnDamage` (SoundDef)
- `fleckOnDamage` (FleckDef)
- `fleckRadius` (float)
- `fleckOnInstigator` (bool)

### `VEF.Weapons.ExpandedShowTurretRadiusExtension` : DefModExtension

- `allowedVerbClass` (Type)
- `allowAnyVerb` (bool)
- `drawMaxRange` (bool) = `true`
- `drawMinRange` (bool) = `true`

### `VEF.Weapons.HeavyWeapon` : DefModExtension

- `supportedTraits` (List<string>)
- `supportedArmors` (List<string>)
- `supportedGenes` (List<string>)
- `disableOptionLabelKey` (string)
- `weaponHitPointsDeductionOnShot` (int)
- `isHeavy` (bool)

### `VEF.Weapons.MakeshiftProperties` : DefModExtension

- `shots` (IntRange) = `new IntRange(0, 1)`

### `VEF.Weapons.MoteProperties` : DefModExtension

- `angleRange` (Nullable)
- `fleckDef` (FleckDef)
- `moteDef` (ThingDef)
- `numTimesThrown` (int) = `1`
- `rotationRange` (Nullable)
- `size` (float) = `-1f`
- `velocity` (float) = `-1f`

### `VEF.Weapons.ProjectileExtension` : DefModExtension

- `beamLifetimeTicks` (int) = `30`
- `beamSkyFadeInTicks` (int)
- `beakSkyHoldTikcs` (int) = `25`
- `beakSkyFadeOutTicks` (int) = `5`
- `flashIntensity` (float) = `-1f`
- `hitFleck` (FleckDef)
- `excludeFromStaticCollection` (bool)
- `attachedEffecter` (EffecterDef)
- `attachedFleck` (FleckDef)
- `fleckScale` (float) = `1f`
- `fleckRefreshInterval` (int) = `10`
- `filthOnMiss` (ThingDef)
- `filthOnMissChance` (float)
- `filthOnMissCount` (IntRange)

### `VEF.Weapons.TeslaChainingProps` : DefModExtension

- `addFire` (bool)
- `bounceRange` (float)
- `maxBounceCount` (int)
- `damageDef` (DamageDef)
- `explosionDamageDef` (DamageDef)
- `impactRadius` (float)
- `targetFriendly` (bool)
- `maxLifetime` (int)
- `impactSound` (SoundDef)

### `VEF.Weapons.WeaponTraitDefExtension` : DefModExtension

- `projectileOverride` (ThingDef)
- `projectileOverrides` (Dictionary<ThingDef, ThingDef>)
- `lowPreferenceProjectileOverride` (bool)
- `soundOverride` (SoundDef)
- `meleeSoundOverride` (SoundDef)
- `meleeDamageOverride` (DamageDef)
- `graphicOverrides` (Dictionary<ThingDef, GraphicData>)
- `graphicOverridePriority` (float) = `100f`
- `abilityToAdd` (AbilityDef)
- `killHediff` (HediffDef)
- `killHediffSeverity` (float) = `1f`
- `sizeMultiplier` (float) = `1f`
- `randomprojectiles` (bool)
- `refreshMaxHitPointsStat` (bool)
- `conditionalStatAffecters` (List<ConditionalStatAffecter>)
- `abilityWithCharges` (AbilityWithChargesDetails)
- `verbsOverride` (List<VerbProperties>)
- `verbsOverrides` (Dictionary<ThingDef, List<VerbProperties>>)
- `coolDownRange` (FloatRange) = `FloatRange.Zero`
- `burstShotCountRange` (List<float>)
- `limitedUses` (int)
- `factionRelationImpacts` (List<FactionRelationImpacts>)
- `drawDuplicate` (bool)

### `VEF.Weathers.WeatherEffectsExtension` : DefModExtension

- `hediffsToApply` (List<HediffAndStat>)
- `ticksInterval` (IntRange)
- `causesRotting` (bool)
- `killsPlants` (bool)
- `chanceToKillPlants` (float)
- `rotProgressPerDamage` (float)
- `activeOnWeatherPerceived` (WeatherDef)
- `worksOnNonFleshPawns` (bool)
- `damageToApply` (DamageDef)
- `damageRange` (FloatRange)
- `worksIndoors` (bool)
- `percentOfPawnsToDealDamage` (float)

### `VEF.Weathers.WeatherExtension` : DefModExtension

- `canRandomlyGenerate` (bool) = `true`

### `VEF.Weathers.WeatherLetterExtensions` : DefModExtension

- `letterDef` (LetterDef)
- `letterTitle` (string)
- `letterText` (string)

### `VEF.Weathers.WeatherOverlayExtension` : DefModExtension

- `overlayPath` (string)
- `copyPropertiesFrom` (string)
- `worldOverlayPanSpeed1` (float)
- `worldPanDir1` (Vector2)
- `worldOverlayPanSpeed2` (float)
- `worldPanDir2` (Vector2)

### `VEF.Weathers.WeatherOverlayExtensionTwo` : DefModExtension

- `overlayPath` (string)
- `copyPropertiesFrom` (string)
- `worldOverlayPanSpeed1` (float)
- `worldPanDir1` (Vector2)
- `worldOverlayPanSpeed2` (float)
- `worldPanDir2` (Vector2)

## CompProperties de Thing (`<comps>`) (121)

### `AnimalBehaviours.CompProperties_FactionAfterHealthLoss` : CompProperties

- `healthPercent` (int) = `50`
- `tickInterval` (int) = `1000`
- `factionToReturnTo` (string) = `""`
- `nonHostileFaction` (bool)
- `attackColony` (bool)

### `AnimalBehaviours.CompProperties_GraphicByHediff` : CompProperties

- `hediffDef` (HediffDef)
- `newImagePath` (string)
- `maskPath` (string)
- `changeDesiccatedGraphic` (bool)
- `dessicatedTxt` (string)
- `changeGraphicsInterval` (int) = `2000`
- `useCutoutComplex` (bool)

### `VEF.Abilities.CompProperties_AbilitiesApparel` : CompProperties

- `abilities` (List<AbilityDef>)

### `VEF.Abilities.CompProperties_GiveHediff` : CompProperties_AbilityEffect

- `hediffDef` (HediffDef)
- `applyToCaster` (bool) = `true`
- `applyToRadius` (bool)

### `VEF.Abilities.CompProperties_UseEffectGiveAbility` : CompProperties_UseEffect

- `ability` (AbilityDef)
- `level` (int) = `1`
- `requiredHediff` (HediffDef)

### `VEF.AnimalBehaviours.CompProperties_AcidImmunity` : CompProperties

### `VEF.AnimalBehaviours.CompProperties_AnimalProduct` : CompProperties

- `hideDisplayOnWildAnimals` (bool)
- `gatheringIntervalDays` (int) = `1`
- `resourceAmount` (int) = `1`
- `resourceDef` (ThingDef)
- `customResourceString` (string) = `""`
- `isRandom` (bool)
- `randomItems` (List<string>)
- `seasonalItems` (List<string>)
- `hasAditional` (bool)
- `additionalItemsProb` (int) = `1`
- `additionalItemsNumber` (int) = `1`
- `additionalItems` (List<string>)
- `goInOrder` (bool)

### `VEF.AnimalBehaviours.CompProperties_AnimalProductOnCaravan` : CompProperties

- `gatheringIntervalTicks` (int) = `30000`
- `resourceAmount` (int) = `1`
- `resourceDef` (ThingDef)
- `femaleOnly` (bool)

### `VEF.AnimalBehaviours.CompProperties_ApplyHediffWhenBound` : CompProperties

- `checkingInterval` (int) = `1000`
- `hediffToApply` (HediffDef)
- `applyHediffToBonded` (bool)
- `hediffToApplyToBonded` (HediffDef)
- `doJobIfBondedDies` (bool)
- `jobToDoIfBondedDies` (JobDef)
- `dieIfBondedDies` (bool)

### `VEF.AnimalBehaviours.CompProperties_AsexualReproduction` : CompProperties

- `reproductionIntervalDays` (int) = `1`
- `customString` (string) = `""`
- `produceEggs` (bool)
- `eggDef` (string) = `""`
- `isGreenGoo` (bool)
- `GreenGooLimit` (int)
- `GreenGooTarget` (string) = `""`
- `asexualHatchedMessage` (string) = `"VEF_AsexualHatched"`
- `asexualCloningMessage` (string) = `"VEF_AsexualCloning"`
- `asexualEggMessage` (string) = `"VEF_AsexualHatchedEgg"`
- `convertsIntoAnotherDef` (bool)
- `newDef` (string) = `""`

### `VEF.AnimalBehaviours.CompProperties_AttachEffecter` : CompProperties

- `effecterDef` (EffecterDef)

### `VEF.AnimalBehaviours.CompProperties_AutoNutrition` : CompProperties

- `tickInterval` (int) = `250`
- `consumingFoodReportString` (string) = `"Eating food"`

### `VEF.AnimalBehaviours.CompProperties_Blink` : CompProperties

- `blinkInterval` (int) = `500`
- `distance` (IntRange) = `new IntRange(5, 10)`
- `warpEffect` (bool)
- `effectOnlyWhenManhunter` (bool)
- `blinkWhenManhunter` (bool)

### `VEF.AnimalBehaviours.CompProperties_BuildPeriodically` : CompProperties

- `defOfBuilding` (string) = `""`
- `ticksToBuild` (int) = `1000`
- `maxBuildingsPerMap` (int) = `10`
- `acceptedTerrains` (List<string>)
- `onlyOneExistingPerPawn` (bool)
- `checkForExistingEdifices` (bool)
- `ifBedAssignOwnership` (bool)
- `onlyTamed` (bool)

### `VEF.AnimalBehaviours.CompProperties_CauseIncident` : CompProperties

- `checkingInterval` (int) = `450000`
- `requiresTamed` (bool)
- `incidentToCause` (string)

### `VEF.AnimalBehaviours.CompProperties_ChangeDefIfNotUnique` : CompProperties

- `defToChangeTo` (string) = `""`

### `VEF.AnimalBehaviours.CompProperties_ChangeWeather` : CompProperties

- `tickInterval` (int) = `250`
- `weatherDef` (string) = `"Fog"`
- `isRandomWeathers` (bool)
- `randomWeathers` (List<WeatherDef>)

### `VEF.AnimalBehaviours.CompProperties_ControlledBlinking` : CompProperties_AbilityEffect

- `warpEffect` (bool) = `true`

### `VEF.AnimalBehaviours.CompProperties_CorpseDecayer` : CompProperties

- `radius` (int) = `5`
- `tickInterval` (int) = `500`
- `decayOnHitPoints` (int) = `1`
- `nutritionGained` (float) = `0.2f`
- `corpseSound` (string) = `""`

### `VEF.AnimalBehaviours.CompProperties_DestroyThisItem` : CompProperties

- `buttonLabel` (string) = `""`
- `buttonDesc` (string) = `""`
- `buttonIcon` (string) = `""`
- `buttonCancelLabel` (string) = `""`
- `buttonCancelDesc` (string) = `""`
- `buttonCancelIcon` (string) = `""`

### `VEF.AnimalBehaviours.CompProperties_DieAfterPeriod` : CompProperties

- `timeToDieInTicks` (int) = `1000`
- `justVanish` (bool)
- `effect` (bool)
- `effectFilth` (string) = `"Filth_Blood"`

### `VEF.AnimalBehaviours.CompProperties_DieAndChangeIntoOtherDef` : CompProperties

- `needsDiggableTerrain` (bool)
- `mustBeTamed` (bool) = `true`
- `defToChangeTo` (ThingDef)
- `gizmoImage` (string)
- `gizmoLabel` (string)
- `gizmoDesc` (string)

### `VEF.AnimalBehaviours.CompProperties_DigPeriodically` : CompProperties

- `customThingToDig` (List<string>)
- `customAmountToDig` (List<int>)
- `ticksToDig` (int) = `60000`
- `onlyWhenTamed` (bool)
- `spawnForbidden` (bool)
- `digBiomeRocks` (bool)
- `digBiomeBricks` (bool)
- `customAmountToDigIfRocksOrBricks` (int) = `1`
- `resultIsCorpse` (bool)
- `onlyDigIfPolluted` (bool)

### `VEF.AnimalBehaviours.CompProperties_DigWhenHungry` : CompProperties

- `customThingToDig` (string) = `""`
- `customAmountToDig` (int) = `1`
- `customThingsToDig` (List<string>)
- `customAmountsToDig` (List<int>)
- `timeToDig` (int) = `40000`
- `acceptedTerrains` (List<string>)
- `spawnForbidden` (bool)
- `digAnywayEveryXTicks` (bool) = `true`
- `timeToDigForced` (int) = `120000`
- `isFrostmite` (bool)
- `digOnlyOnGrowingSeason` (bool)
- `minTemperature` (int)
- `maxTemperature` (int) = `58`

### `VEF.AnimalBehaviours.CompProperties_DiseaseEventImmunity` : CompProperties

### `VEF.AnimalBehaviours.CompProperties_DiseaseImmunity` : CompProperties

- `hediffsToRemove` (List<string>)
- `tickInterval` (int) = `250`

### `VEF.AnimalBehaviours.CompProperties_DiseasesAfterPeriod` : CompProperties

- `timeToApplyInTicks` (int) = `1000`
- `hediffsToApply` (List<HediffDef>)
- `percentageOfMaxToReapply` (float) = `0.8f`

### `VEF.AnimalBehaviours.CompProperties_DoesntFlee` : CompProperties

### `VEF.AnimalBehaviours.CompProperties_Draftable` : CompProperties

- `makeNonFleeingToo` (bool)
- `canHandleWeapons` (bool)
- `conditionalOnTrainability` (bool)
- `checkingInterval` (int) = `500`

### `VEF.AnimalBehaviours.CompProperties_DropOnDeath` : CompProperties

- `resourceAmount` (int) = `1`
- `resourceDef` (string)
- `dropChance` (float) = `1f`
- `isRandom` (bool)
- `randomItems` (List<string>)

### `VEF.AnimalBehaviours.CompProperties_EatWeirdFood` : CompProperties

- `customThingToEat` (List<string>)
- `nutrition` (int) = `2`
- `fullyDestroyThing` (bool)
- `percentageOfDestruction` (float) = `0.1f`
- `ignoreUseHitPoints` (bool)
- `digThingIfMapEmpty` (bool)
- `thingToDigIfMapEmpty` (string) = `""`
- `customAmountToDig` (int) = `1`
- `hediffWhenEaten` (string) = `""`
- `advanceLifeStage` (bool)
- `advanceAfterXFeedings` (int) = `1`
- `defToAdvanceTo` (string) = `""`
- `fissionAfterXFeedings` (bool)
- `defToFissionTo` (string) = `""`
- `numberOfOffspring` (int) = `2`
- `fissionOnlyIfTamed` (bool) = `true`
- `drainBattery` (bool)
- `percentageDrain` (float) = `0.1f`
- `areFoodSourcesPlants` (bool)
- `needsWater` (bool) = `true`

### `VEF.AnimalBehaviours.CompProperties_Electrified` : CompProperties

- `electroRate` (int)
- `electroRadius` (int)
- `electroChargeAmount` (int) = `1`
- `batteriesToAffect` (List<string>)

### `VEF.AnimalBehaviours.CompProperties_EnrageOtherPawns` : CompProperties

- `pawnkinddefsToAffect` (List<PawnKindDef>)
- `checkingInterval` (int) = `200`

### `VEF.AnimalBehaviours.CompProperties_ExplodingEggLayer` : CompProperties

- `eggLayIntervalDays` (float) = `1f`
- `eggCountRange` (IntRange) = `IntRange.One`
- `eggUnfertilizedDef` (ThingDef)
- `eggFertilizedDef` (ThingDef)
- `eggFertilizationCountMax` (int) = `1`
- `eggLayFemaleOnly` (bool) = `true`
- `eggProgressUnfertilizedMax` (float) = `1f`

### `VEF.AnimalBehaviours.CompProperties_ExplodingHatcher` : CompProperties

- `hatcherDaystoHatch` (float) = `1f`
- `hatcherPawn` (PawnKindDef)
- `range` (float) = `3f`
- `damage` (int) = `10`
- `damageDef` (string) = `"Flame"`
- `soundDef` (string) = `"AA_GooPop"`

### `VEF.AnimalBehaviours.CompProperties_ExtremeXenophobia` : CompProperties

- `berserkRate` (int) = `10000`
- `AcceptedDefnames` (List<string>)

### `VEF.AnimalBehaviours.CompProperties_FilthProducer` : CompProperties

- `filthType` (string) = `""`
- `rate` (float)
- `radius` (int)
- `ticksToCreateFilth` (int) = `600`

### `VEF.AnimalBehaviours.CompProperties_FixedGender` : CompProperties

- `gender` (Gender) = `(Gender)2`

### `VEF.AnimalBehaviours.CompProperties_Floating` : CompProperties

- `inSpace` (bool)
- `isFloater` (bool)
- `canCrossWater` (bool)

### `VEF.AnimalBehaviours.CompProperties_GasProducer` : CompProperties

- `gasType` (string) = `""`
- `rate` (float)
- `radius` (int)
- `generateIfDowned` (bool)

### `VEF.AnimalBehaviours.CompProperties_GiveThoughtsOnCaravan` : CompProperties

- `intervalTicks` (int) = `30000`
- `thought` (ThoughtDef)
- `causeNegativeAtRandom` (bool)
- `randomNegativeChance` (float) = `0.1f`
- `negativeThought` (ThoughtDef)

### `VEF.AnimalBehaviours.CompProperties_GraphicByStyle` : CompProperties

- `styleGraphics` (List<StyleGraphics>)
- `changeGraphicsInterval` (int) = `2000`

### `VEF.AnimalBehaviours.CompProperties_GraphicByTerrain` : CompProperties

- `changeGraphicsInterval` (int) = `240`
- `terrains` (List<string>)
- `suffix` (List<string>)
- `hediffToApply` (List<string>)
- `waterOverride` (bool)
- `waterSuffix` (string) = `"_Winter"`
- `waterHediffToApply` (string) = `""`
- `waterSeasonalItemsIndex` (int)
- `lowTemperatureOverride` (bool)
- `temperatureThreshold` (int) = `-10`
- `lowTemperatureSuffix` (string) = `"_Winter"`
- `lowTemperatureHediffToApply` (string) = `""`
- `lowTemperatureSeasonalItemsIndex` (int)
- `snowOverride` (bool)
- `snowySuffix` (string) = `"_Winter"`
- `snowyHediffToApply` (string) = `""`
- `snowySeasonalItemsIndex` (int)
- `provideSeasonalItems` (bool)
- `seasonalItemsIndexes` (List<int>)

### `VEF.AnimalBehaviours.CompProperties_HediffAfterHealthLoss` : CompProperties

- `healthPercent` (int) = `50`
- `tickInterval` (int) = `1000`
- `hediff` (HediffDef)
- `severity` (float) = `1f`
- `bodyPart` (BodyPartDef)

### `VEF.AnimalBehaviours.CompProperties_HediffByTemperature` : CompProperties

- `doTemperatureBelow` (bool)
- `doTemperatureAbove` (bool)
- `temperatureBelow` (float)
- `temperatureAbove` (float)
- `tickInterval` (int) = `1000`
- `hediffBelow` (HediffDef)
- `hediffAbove` (HediffDef)
- `severity` (float) = `1f`
- `bodyPart` (BodyPartDef)

### `VEF.AnimalBehaviours.CompProperties_HediffEffecter` : CompProperties

- `radius` (int) = `1`
- `severity` (float) = `1f`
- `tickInterval` (int) = `1000`
- `hediff` (string) = `"Plague"`
- `notOnlyAffectColonists` (bool)

### `VEF.AnimalBehaviours.CompProperties_HediffWhenFleeing` : CompProperties

- `tickInterval` (int) = `60`
- `hediffToCause` (HediffDef)
- `graphicAndSoundEffect` (bool)
- `hediffOnRadius` (bool)
- `radius` (float) = `3f`

### `VEF.AnimalBehaviours.CompProperties_HighlyFlammable` : CompProperties

- `hediffToInflict` (string) = `""`
- `tickInterval` (int) = `50`

### `VEF.AnimalBehaviours.CompProperties_Infecter` : CompProperties

- `infectionChance` (int) = `10`
- `worsenExistingInfection` (bool)
- `severityToAdd` (float) = `0.15f`

### `VEF.AnimalBehaviours.CompProperties_InitialAbility` : CompProperties

- `initialAbility` (AbilityDef)

### `VEF.AnimalBehaviours.CompProperties_InitialHediff` : CompProperties

- `hediffname` (string) = `""`
- `hediffseverity` (float)
- `applyToAGivenBodypart` (bool)
- `part` (BodyPartDef)
- `addRandomHediffs` (bool)
- `numberOfHediffs` (int) = `1`

### `VEF.AnimalBehaviours.CompProperties_InitialMentalState` : CompProperties

- `mentalstate` (MentalStateDef)

### `VEF.AnimalBehaviours.CompProperties_LastStand` : CompProperties

- `finalCoolDownMultiplier` (float) = `2f`

### `VEF.AnimalBehaviours.CompProperties_LightSustenance` : CompProperties

### `VEF.AnimalBehaviours.CompProperties_MakeOtherPawnsFlee` : CompProperties

- `pawnkinddefsToAffect` (List<PawnKindDef>)
- `checkingInterval` (int) = `200`

### `VEF.AnimalBehaviours.CompProperties_Metamorphosis` : CompProperties

- `timeInYears` (float)
- `pawnToTurnInto` (string)
- `reportString` (string) = `"VEF_TimeToMetamorphosis"`

### `VEF.AnimalBehaviours.CompProperties_MindEffecter` : CompProperties

- `radius` (int) = `1`
- `tickInterval` (int) = `1000`
- `mentalState` (string) = `"Berserk"`
- `notOnlyAffectColonists` (bool)

### `VEF.AnimalBehaviours.CompProperties_MoteReleaser` : CompProperties

- `moteDef` (ThingDef)

### `VEF.AnimalBehaviours.CompProperties_NearbyEffecter` : CompProperties

- `thingsToAffect` (List<string>)
- `thingsToConvertTo` (List<string>)
- `ticksConversionRate` (int) = `1000`
- `radius` (int) = `2`
- `feedCauser` (bool)
- `nutritionGained` (float)
- `isForbidden` (bool)

### `VEF.AnimalBehaviours.CompProperties_NoFilth` : CompProperties

### `VEF.AnimalBehaviours.CompProperties_NoTamingDecay` : CompProperties

### `VEF.AnimalBehaviours.CompProperties_PassiveRegenerator` : CompProperties

- `radius` (int) = `1`
- `tickInterval` (int) = `1000`
- `healAmount` (float) = `0.1f`
- `healAll` (bool) = `true`
- `showEffect` (bool)
- `needsToBeTamed` (bool)

### `VEF.AnimalBehaviours.CompProperties_PawnOverlay` : CompProperties_FireOverlay

- `graphicElements` (List<GraphicData>)

### `VEF.AnimalBehaviours.CompProperties_Refueling` : CompProperties

- `fuelingRate` (int)
- `fuelingRadius` (int)
- `buildingsToAffect` (List<string>)
- `mustBeTamed` (bool)

### `VEF.AnimalBehaviours.CompProperties_Regeneration` : CompProperties

- `rateInTicks` (int) = `1000`
- `healAmount` (float) = `0.1f`
- `healAll` (bool) = `true`
- `needsSun` (bool)
- `needsWater` (bool)
- `onlyBleeding` (bool)
- `onlyTendButNotHeal` (bool)
- `healOneTendOne` (bool)
- `tendMin` (float) = `0.7f`
- `tendMax` (float) = `1f`
- `bodypart` (BodyPartDef)

### `VEF.AnimalBehaviours.CompProperties_SpawnPawnsOnMissingBodyPart` : CompProperties

- `pawnKindOptions` (List<PawnKindDef>) = `new List<PawnKindDef>()`
- `filthCreated` (ThingDef)
- `filthCountRange` (IntRange)
- `sound` (SoundDef)

### `VEF.AnimalBehaviours.CompProperties_StateAfterHealthLoss` : CompProperties

- `healthPercent` (int) = `50`
- `tickInterval` (int) = `1000`
- `mentalState` (string) = `"PanicFlee"`

### `VEF.AnimalBehaviours.CompProperties_SummonOnSpawn` : CompProperties

- `pawnDef` (string) = `"Pig"`
- `groupMinMax` (List<int>)
- `summonsAreManhunters` (bool) = `true`

### `VEF.AnimalBehaviours.CompProperties_SwallowWhole` : CompProperties

- `stomachCapacity` (int) = `5`
- `maximumBodysize` (float) = `30f`
- `nutritionGained` (int) = `5`
- `soundPlayedWhenEating` (string)
- `sendLetterWhenEating` (bool)
- `letterLabel` (string) = `""`
- `letterText` (string) = `""`
- `digestionPeriod` (int) = `240`
- `createFilthWhenKilled` (bool)
- `filthToMake` (ThingDef)
- `playSoundWhenKilled` (bool)
- `soundToPlay` (string) = `"Hive_Spawn"`

### `VEF.AnimalBehaviours.CompProperties_TerrainChanger` : CompProperties

- `checkingRate` (int) = `100`
- `FirstStageTerrain` (string) = `""`
- `SecondStageTerrain` (string) = `""`
- `doThirdStage` (bool)
- `ThirdStageTerrain` (string) = `""`
- `inRadius` (bool)
- `radius` (int) = `2`

### `VEF.AnimalBehaviours.CompProperties_ThoughtEffecter` : CompProperties

- `radius` (int) = `1`
- `tickInterval` (int) = `1000`
- `thoughtDef` (string) = `"AteWithoutTable"`
- `showEffect` (bool)
- `needsToBeTamed` (bool)
- `conditionalOnWellBeing` (bool)
- `thoughtDefWhenSuffering` (string) = `"AteWithoutTable"`

### `VEF.AnimalBehaviours.CompProperties_TriggerAbilityOnDamage` : CompProperties

- `ability` (AbilityDef)
- `minDamageToTrigger` (float)

### `VEF.AnimalBehaviours.CompProperties_Untameable` : CompProperties

- `factionToReturnTo` (string) = `""`
- `goWild` (bool)
- `goesManhunter` (bool) = `true`
- `sendMessage` (bool)
- `message` (string) = `"VEF_NotTameable"`

### `VEF.AnimalGenes.CompProperties_AnimalGenes` : CompProperties

### `VEF.AnimalGenes.CompProperties_StackByGenes` : CompProperties

### `VEF.Apparels.CompProperties_ApparelHediffs` : CompProperties

- `hediffDefnames` (List<string>)

### `VEF.Apparels.CompProperties_Camouflage` : CompProperties

### `VEF.Apparels.CompProperties_Shield` : CompProperties

- `shieldTags` (List<string>)
- `useDeflectMetalEffect` (bool)
- `coveredBodyPartGroups` (List<BodyPartGroupDef>)
- `offHandGraphicData` (GraphicData)
- `offHandHoldOffset` (HoldOffsetSet)

### `VEF.Apparels.CompProperties_ShieldBubble` : CompProperties

- `EnergyShieldEnergyMax` (float)
- `EnergyShieldRechargeRate` (float)
- `chargeFullyWhenMade` (bool)
- `initialChargePct` (float)
- `blockRangedAttack` (bool) = `true`
- `blockMeleeAttack` (bool)
- `dontAllowRangedAttack` (bool)
- `dontAllowMeleeAttack` (bool)
- `shieldTexPath` (string)
- `showWhenDrafted` (bool)
- `showAlways` (bool)
- `showOnHostiles` (bool) = `true`
- `showOnNeutralInCombat` (bool)
- `minShieldSize` (float) = `1.5f`
- `maxShieldSize` (float) = `2f`
- `shieldColor` (Color) = `Color.white`
- `EnergyLossPerDamage` (float) = `1f`
- `disableRotation` (bool)
- `absorbDamageSound` (SoundDef)
- `brokenSound` (SoundDef)
- `resetSound` (SoundDef)
- `tooltipKey` (string)

### `VEF.Apparels.CompProperties_ShieldField` : CompProperties

- `activeAlways` (bool)
- `initialEnergyPercentage` (float)
- `rechargeTicksWhenDepleted` (int)
- `shortCircuitChancePerEnergyLost` (float)
- `inactivePowerConsumption` (float)
- `shieldColour` (Color) = `Color.white`
- `rechargeRateStat` (StatDef)
- `shieldEnergyMaxStat` (StatDef)
- `shieldRadiusStat` (StatDef)
- `workingTimeTicks` (int) = `-1`
- `cooldownTicks` (int) = `-1`
- `manualActivation` (bool)
- `activationLabelKey` (string)
- `activationDescKey` (string)
- `activationIconTexPath` (string)
- `disarmedByEmpForTicks` (int) = `-1`
- `activeSound` (SoundDef)
- `toggleable` (bool)
- `toggleIconPath` (string) = `"UI/ToggleIcon"`
- `toggleLabelKey` (string)
- `toggleDescKey` (string)
- `reactivateEffect` (EffecterDef)
- `healthColorPoints` (List<HealthColorPoint>)

### `VEF.Apparels.CompProperties_SwitchApparel` : CompProperties

- `SwitchTo` (ThingDef)
- `Label` (string)
- `graphicPath` (string) = `"UI/Gizmo/Switch"`

### `VEF.Buildings.CompProperties_BouncingArrow` : CompProperties

- `startBouncingArrowUponSpawning` (bool)

### `VEF.Buildings.CompProperties_BuildingCalls` : CompProperties

- `interval` (IntRange)
- `soundDefs` (List<SoundDef>)

### `VEF.Buildings.CompProperties_ConfigurableSpawner` : CompProperties

- `spawnCount` (int) = `1`
- `spawnForbidden` (bool)
- `requiresPower` (bool)
- `requiresFuel` (bool)
- `writeTimeLeftToSpawn` (bool)
- `showMessageIfOwned` (bool)
- `saveKeysPrefix` (string)
- `inheritFaction` (bool)

### `VEF.Buildings.CompProperties_CustomCauseHediff_AoE` : CompProperties

- `requiredCapacities` (List<PawnCapacityDef>)
- `hediff` (HediffDef)
- `mustBeAwake` (bool)
- `sameRoomOnly` (bool) = `true`
- `range` (float)
- `allowHumanlike` (bool) = `true`
- `allowInsects` (bool)
- `allowDryads` (bool)
- `allowAnimals` (bool)
- `allowMechanoids` (bool)
- `allowEntities` (bool)
- `worksInside` (bool) = `true`
- `worksOutside` (bool) = `true`
- `startingSeverity` (float) = `1f`
- `checkInterval` (int) = `100`
- `hediffDuration` (int) = `120`

### `VEF.Buildings.CompProperties_CustomizableGraphic` : CompProperties

- `name` (string)
- `sortingPriority` (int)
- `clockwiseRotationIndex` (int) = `-1`
- `iconPath` (string) = `"UI/VEF_ChooseGraphic"`
- `gizmoLabel` (string)
- `gizmoDescription` (string)
- `defaultIndex` (int) = `-1`
- `defaultStyleIndex` (Dictionary<ThingStyleDef, int>)
- `defaultGraphicData` (List<CustomizableGraphicOptionData>)
- `styledGraphicData` (Dictionary<ThingStyleDef, List<CustomizableGraphicOptionData>>)

### `VEF.Buildings.CompProperties_ExtendedBiosculpterPod` : CompProperties_BiosculpterPod

- `copyCyclesFrom` (ThingDef)
- `drawPawn` (bool) = `true`
- `pawnFacingDirectionOverride` (Nullable)
- `pawnOffsetNorth` (Vector3) = `Vector3.zero`
- `pawnOffsetSouth` (Vector3) = `Vector3.zero`
- `pawnOffsetEast` (Vector3) = `Vector3.zero`
- `pawnOffsetWest` (Vector3) = `Vector3.zero`
- `drawBackground` (bool) = `true`
- `backgroundOffsetNorth` (Vector3) = `Vector3.zero`
- `backgroundOffsetSouth` (Vector3) = `Vector3.zero`
- `backgroundOffsetEast` (Vector3) = `Vector3.zero`
- `backgroundOffsetWest` (Vector3) = `Vector3.zero`
- `backgroundSize` (Vector3) = `Vector3.zero`
- `backgroundMaterialPath` (string)
- `backgroundMaterial` (Material)

### `VEF.Buildings.CompProperties_FireOverlayRotatable` : CompProperties_FireOverlay

- `northOffset` (Vector3)
- `southOffset` (Vector3)
- `westOffset` (Vector3)
- `eastOffset` (Vector3)
- `texPath` (string) = `"Things/Special/Fire"`
- `color` (Color) = `Color.white`
- `size` (Vector2) = `Vector2.one`

### `VEF.Buildings.CompProperties_GlowerExtended` : CompProperties

- `colorOptions` (List<ColorOption>)
- `spawnGlowerInFacedCell` (bool)

### `VEF.Buildings.CompProperties_HediffGiver` : CompProperties

- `hediffDef` (HediffDef)
- `severityIncrease` (float)
- `radius` (float)
- `stats` (List<StatDef>)
- `tickRate` (int) = `500`

### `VEF.Buildings.CompProperties_JammedAirlock` : CompProperties_Interactable

- `doorToConvertTo` (ThingDef)
- `stringExtra` (string)

### `VEF.Buildings.CompProperties_MountableOnWall` : CompProperties

### `VEF.Buildings.CompProperties_RandomBuildingGraphic` : CompProperties

- `randomGraphics` (List<string>)
- `optionalNames` (List<string>)
- `startAsRandom` (bool) = `true`
- `disableRandomButton` (bool)
- `disableGraphicChoosingButton` (bool)
- `disableAllButtons` (bool)
- `useSouthOrientation` (bool)
- `onlyApplyToThisDef` (ThingDef)

### `VEF.Buildings.CompProperties_Refuelable_DualFuel` : CompProperties_Refuelable

- `secondaryFuelCapacity` (float) = `10f`
- `initialSecondaryFuelPercent` (float)
- `autoRefuelSecondaryPercent` (float) = `0.3f`
- `secondaryFuelFilter` (ThingFilter)
- `initialAllowAutoRefuelSecondary` (bool) = `true`
- `showAllowAutoRefuelSecondaryToggle` (bool)
- `targetSecondaryFuelLevelConfigurable` (bool)
- `initialConfigurableSecondaryTargetFuelLevel` (float)
- `minimumSecondaryFueledThreshold` (float) = `1f`
- `factorSecondaryByDifficulty` (bool)
- `secondaryFuelLabel` (string)
- `secondaryFuelGizmoLabel` (string)
- `outOfSecondaryFuelMessage` (string)
- `secondaryFuelIconPath` (string)

### `VEF.Buildings.CompProperties_RockSpawner` : CompProperties

- `spawnCount` (int) = `1`
- `spawnIntervalRange` (IntRange) = `new IntRange(100, 100)`
- `spawnForbidden` (bool)
- `requiresPower` (bool)
- `requiresFuel` (bool)
- `writeTimeLeftToSpawn` (bool)
- `showMessageIfOwned` (bool)
- `saveKeysPrefix` (string)
- `inheritFaction` (bool)

### `VEF.Buildings.CompProperties_ScheduleExtended` : CompProperties_Schedule

- `minLight` (float)
- `maxLight` (float) = `1f`
- `sunlightMessage` (string)
- `disableUnderRoof` (bool)
- `disableWithoutRoof` (bool)
- `disabledDueToRoofMessage` (string)

### `VEF.Buildings.CompProperties_SelectBuildingBehind` : CompProperties

- `buildingToSelect` (string)
- `commandButtonImage` (string) = `""`
- `commandButtonText` (string) = `""`
- `commandButtonDesc` (string) = `""`

### `VEF.Buildings.CompProperties_SpawnOtherBuildingWhenPowered` : CompProperties

- `defOfBuildingToSpawn` (string) = `"HorseshoesPin"`
- `tickRaresToCheck` (int) = `1`

### `VEF.Buildings.CompProperties_StatsWhenPowered` : CompProperties

- `poweredStatFactors` (List<StatModifier>)
- `poweredStatOffsets` (List<StatModifier>)
- `unpoweredStatFactors` (List<StatModifier>)
- `unpoweredStatOffsets` (List<StatModifier>)
- `clearRoomCacheOnPowerChange` (bool)
- `clearStatCacheOnPowerChange` (List<StatDef>)
- `onlyWorksIndoors` (bool)
- `onlyWorksOutdoors` (bool)

### `VEF.Buildings.CompProperties_ThrowMote` : CompProperties

- `mote` (ThingDef)
- `emissionInterval` (int) = `-1`
- `moteScale` (int) = `1`
- `solidTime` (int) = `-1`
- `fadeOutTime` (int) = `-1`
- `speedRange` (FloatRange) = `new FloatRange(0.6f, 0.75f)`
- `angleRange` (FloatRange) = `new FloatRange(0f, 360f)`
- `rotationRange` (FloatRange) = `new FloatRange(-60f, 60f)`

### `VEF.Cooking.CompProperties_IngestedThoughtFromQuality` : CompProperties

- `ingestedThought` (ThoughtDef)

### `VEF.Cooking.CompProperties_MaturingAlcohol` : CompProperties

- `daysToRotStart` (float) = `2f`
- `rotDestroys` (bool)
- `rotDamagePerDay` (float) = `40f`
- `daysToDessicated` (float) = `999f`
- `dessicatedDamagePerDay` (float)
- `disableIfHatcher` (bool)
- `maturingString` (string)
- `maturingProperly` (string)
- `maturingSlowly` (string)
- `maturingStopped` (string)
- `thingToTransformTo` (string)
- `TicksToRotStart` (int) = `> Mathf.RoundToInt(daysToRotStart * 60000f)`
- `TicksToDessicated` (int) = `> Mathf.RoundToInt(daysToDessicated * 60000f)`

### `VEF.Cooking.CompProperties_StackByQuality` : CompProperties

### `VEF.Cooking.CompProperties_TempTransforms` : CompProperties

- `minSafeTemperature` (float)
- `maxSafeTemperature` (float) = `100f`
- `progressPerDegreePerTick` (float) = `1E-05f`
- `thingToTransformInto` (string) = `""`
- `preserveHp` (bool) = `true`
- `keepForbidden` (bool) = `true`
- `keepQuality` (bool) = `true`
- `keepRottableProgress` (bool) = `true`

### `VEF.Genes.CompProperties_HumanHatcher` : CompProperties

- `hatcherDaystoHatch` (float) = `1f`

### `VEF.Graphics.CompProperties_GraphicCustomization` : CompProperties

- `graphics` (List<GraphicPart>)
- `customizable` (bool)
- `customizationTitle` (string)

### `VEF.Pawns.CompProperties_DependsOnBuilding` : CompProperties

### `VEF.Pawns.CompProperties_PawnDependsOn` : CompProperties

- `pawnToSpawn` (PawnKindDef)
- `killPawnAfterDestroying` (bool) = `true`

### `VEF.Plants.CompProperties_GlowerBlooming` : CompProperties_Glower

### `VEF.Sounds.CompProperties_AmbientSound` : CompProperties

- `ambientSound` (SoundDef)

### `VEF.Storyteller.CompProperties_QuestGiver` : CompProperties

- `floatOptionLabel` (string)
- `jobToGive` (JobDef)
- `questManagerID` (int)
- `questGiver` (QuestGiverDef)

### `VEF.Things.CompProperties_LabelByIngredients` : CompProperties

- `fullReplace` (bool)
- `overrides` (Dictionary<ThingDef, string>) = `new Dictionary<ThingDef, string>()`
- `exclusions` (List<ThingDef>) = `new List<ThingDef>()`

### `VEF.Things.CompProperties_RandomOutcomeComp` : CompProperties

- `canProvideTags` (List<string>) = `new List<string>()`

### `VEF.Weapons.CompProperties_ApplyWeaponTraits` : CompProperties

### `VEF.Weapons.CompProperties_Explosive_Shells` : CompProperties_Explosive

- `shell` (ThingDef)
- `shellCount` (IntRange)
- `shellDist` (IntRange)

### `VEF.Weapons.CompProperties_GuidedProjectile` : CompProperties

- `hitChance` (float)
- `selectDifferentTargets` (bool)

### `VEF.Weapons.CompProperties_HomingProjectile` : CompProperties

- `homingDistanceFractionPassed` (float)
- `homingCorrectionTickRate` (float)
- `initialDispersionFromTarget` (float)
- `launchAngleDeviationMax` (float)
- `maxTurnDegreesPerTick` (float)
- `maxTravelDistance` (float)
- `maxTravelTicks` (int)
- `targetLockTicks` (int)
- `hitSound` (SoundDef)

### `VEF.Weapons.CompProperties_LaserCapacitor` : CompProperties

- `Overheats` (bool)
- `OverheatDestroys` (bool) = `true`
- `OverheatChance` (float) = `0.05f`
- `OverheatBlastDamageDef` (string) = `"Burn"`
- `OverheatBlastExtraDamage` (int) = `5`
- `OverheatBlastRadius` (float) = `1.5f`
- `WarmUpReductionPerShot` (float) = `0.1f`
- `OverheatMoteThrown` (ThingDef)
- `OverheatMoteSize` (float) = `0.5f`
- `UiIconPath` (string) = `string.Empty`

### `VEF.Weapons.CompProperties_MultiVerbWeapon` : CompProperties

- `verbLabel` (string)
- `statExplanationLabelOverride` (string)
- `statOffsets` (List<StatModifier>)
- `statFactors` (List<StatModifier>)
- `gizmoLabelOverride` (string)
- `gizmoDescriptionOverride` (string)
- `gizmoIconPathOverride` (string)
- `gizmoIconOverride` (Texture2D)
- `defaultVerbLabel` (string)
- `statExplanationLabel` (string)
- `switchMode` (SwitchMode) = `SwitchMode.SingleSwitchGizmo`
- `verbs` (List<VerbData>) = `new List<VerbData>()`
- `gizmoLabel` (string)
- `gizmoDescription` (string)
- `gizmoIconPath` (string)
- `gizmoIcon` (Texture2D)

### `VEF.Weapons.CompProperties_WeaponHediffs` : CompProperties

- `hediffs` (List<HediffDef>)

### `VEF.Weapons.Projectile_VisualEffectCompProperties` : CompProperties

- `gaussDistortion` (bool)
- `lightningGlow` (bool)

## CompProperties de Hediff (57)

### `VEF.AnimalBehaviours.HediffCompProperties_Ability` : HediffCompProperties

- `ability` (AbilityDef)
- `checkingInterval` (int) = `6000`

### `VEF.AnimalBehaviours.HediffCompProperties_Animation` : HediffCompProperties

- `animation` (AnimationDef)
- `shamblerParticles` (bool)

### `VEF.AnimalBehaviours.HediffCompProperties_AsexualReproduction` : HediffCompProperties

- `reproductionIntervalDays` (int) = `1`
- `customString` (string) = `""`
- `produceEggs` (bool)
- `eggDef` (string) = `""`
- `isGreenGoo` (bool)
- `GreenGooLimit` (int)
- `GreenGooTarget` (string) = `""`
- `asexualHatchedMessage` (string) = `"VEF_AsexualHatched"`
- `asexualCloningMessage` (string) = `"VEF_AsexualCloning"`
- `asexualEggMessage` (string) = `"VEF_AsexualHatchedEgg"`
- `convertsIntoAnotherDef` (bool)
- `newDef` (string) = `""`
- `endogeneTransfer` (bool)

### `VEF.AnimalBehaviours.HediffCompProperties_AutoPermanentInjury` : HediffCompProperties

### `VEF.AnimalBehaviours.HediffCompProperties_CauseIncident` : HediffCompProperties

- `checkingInterval` (int) = `450000`
- `requiresTamed` (bool)
- `incidentToCause` (string)

### `VEF.AnimalBehaviours.HediffCompProperties_ChangeWeather` : HediffCompProperties

- `tickInterval` (int) = `250`
- `weatherDef` (string) = `"Fog"`
- `isRandomWeathers` (bool)
- `randomWeathers` (List<WeatherDef>)

### `VEF.AnimalBehaviours.HediffCompProperties_CorpseDecayer` : HediffCompProperties

- `radius` (int) = `5`
- `tickInterval` (int) = `500`
- `decayOnHitPoints` (int) = `1`
- `nutritionGained` (float) = `0.2f`
- `corpseSound` (string) = `""`
- `causeThoughtNearby` (bool)
- `radiusForThought` (int)
- `thought` (ThoughtDef)

### `VEF.AnimalBehaviours.HediffCompProperties_DieAfterPeriod` : HediffCompProperties

- `timeToDieInTicks` (int) = `1000`
- `justVanish` (bool)
- `effect` (bool)
- `effectFilth` (string) = `"Filth_Blood"`
- `DescriptionLabel` (string) = `"VEF_TimeToDie"`

### `VEF.AnimalBehaviours.HediffCompProperties_Draftable` : HediffCompProperties

- `checkingInterval` (int) = `500`
- `makeNonFleeingToo` (bool)
- `canHandleWeapons` (bool)

### `VEF.AnimalBehaviours.HediffCompProperties_Electrified` : HediffCompProperties

- `electroRate` (int)
- `electroRadius` (int)
- `electroChargeAmount` (int) = `1`
- `batteriesToAffect` (List<string>)

### `VEF.AnimalBehaviours.HediffCompProperties_ExplodeOnDamage` : HediffCompProperties

- `minDamageToExplode` (int)
- `damageType` (DamageDef)
- `damageAmount` (int) = `-1`
- `radius` (float)
- `sound` (SoundDef)
- `spawnThingDef` (ThingDef)
- `spawnThingChance` (float)

### `VEF.AnimalBehaviours.HediffCompProperties_ExplodeOnDowned` : HediffCompProperties

### `VEF.AnimalBehaviours.HediffCompProperties_ExplodeOnFire` : HediffCompProperties

- `minFireToExplode` (int)
- `damageType` (DamageDef)
- `damageAmount` (int) = `-1`
- `radius` (float)
- `ticksToRecheck` (int)
- `checkInterval` (int) = `300`

### `VEF.AnimalBehaviours.HediffCompProperties_Exploder` : HediffCompProperties

- `explosionForce` (float) = `5.9f`
- `damageDef` (DamageDef)

### `VEF.AnimalBehaviours.HediffCompProperties_FilthProducer` : HediffCompProperties

- `filthType` (string) = `""`
- `rate` (float)
- `radius` (int)
- `ticksToCreateFilth` (int) = `600`

### `VEF.AnimalBehaviours.HediffCompProperties_Floating` : HediffCompProperties

- `inSpace` (bool)
- `checkingInterval` (int) = `500`

### `VEF.AnimalBehaviours.HediffCompProperties_GasProducer` : HediffCompProperties

- `amount` (int)
- `timer` (int)
- `gasType` (GasType)

### `VEF.AnimalBehaviours.HediffCompProperties_GiveHediffsInRangeExpanded` : HediffCompProperties

- `range` (float)
- `targetingParameters` (TargetingParameters)
- `hediff` (HediffDef)
- `mote` (ThingDef)
- `hideMoteWhenNotDrafted` (bool)
- `initialSeverity` (float) = `1f`
- `affectSameDef` (bool)
- `needLOS` (bool) = `true`
- `onlyItsFaction` (bool) = `true`

### `VEF.AnimalBehaviours.HediffCompProperties_HeatPusher` : HediffCompProperties

- `tickCounter` (int) = `60`
- `heatPushMaxTemperature` (float) = `99999f`
- `heatPushMinTemperature` (float) = `-99999f`
- `heatPerSecond` (float)

### `VEF.AnimalBehaviours.HediffCompProperties_HighlyFlammable` : HediffCompProperties

- `damageToInflict` (DamageDef)
- `damageAmount` (float) = `15f`
- `tickInterval` (int) = `50`
- `sunlightBurns` (bool)

### `VEF.AnimalBehaviours.HediffCompProperties_LastStand` : HediffCompProperties

- `finalCoolDownMultiplier` (float) = `2f`

### `VEF.AnimalBehaviours.HediffCompProperties_LightSustenance` : HediffCompProperties

### `VEF.AnimalBehaviours.HediffCompProperties_MentalBreakOnDamage` : HediffCompProperties

- `damageTypeReceived` (DamageDef)
- `mentalBreak` (MentalBreakDef)
- `reason` (string)

### `VEF.AnimalBehaviours.HediffCompProperties_NearbyEffecter` : HediffCompProperties

- `thingsToAffect` (List<string>)
- `thingsToConvertTo` (List<string>)
- `ticksConversionRate` (int) = `1000`
- `radius` (int) = `2`
- `feedCauser` (bool)
- `nutritionGained` (float)
- `isForbidden` (bool)

### `VEF.AnimalBehaviours.HediffCompProperties_PassiveRegenerator` : HediffCompProperties

- `radius` (int) = `1`
- `tickInterval` (int) = `1000`
- `healAmount` (float) = `0.1f`
- `healAll` (bool) = `true`
- `showEffect` (bool)
- `needsToBeTamed` (bool)

### `VEF.AnimalBehaviours.HediffCompProperties_PassiveResearch` : HediffCompProperties

- `researchPoints` (int) = `1`
- `tickInterval` (int) = `6000`

### `VEF.AnimalBehaviours.HediffCompProperties_PlaySound` : HediffCompProperties

- `sustainer` (SoundDef)
- `endSound` (SoundDef)

### `VEF.AnimalBehaviours.HediffCompProperties_Polluter` : HediffCompProperties

- `amount` (int)
- `timer` (int)

### `VEF.AnimalBehaviours.HediffCompProperties_PsyfocusRegeneration` : HediffCompProperties

- `rateInTicks` (int) = `1000`
- `regenAmount` (float) = `0.1f`

### `VEF.AnimalBehaviours.HediffCompProperties_ReducePrisonerCertainty` : HediffCompProperties

- `certaintyPerTick` (float)
- `checkingInterval` (int) = `250`

### `VEF.AnimalBehaviours.HediffCompProperties_ReducePrisonerResistance` : HediffCompProperties

- `resistancePerTick` (float)
- `checkingInterval` (int) = `250`

### `VEF.AnimalBehaviours.HediffCompProperties_Regeneration` : HediffCompProperties

- `rateInTicks` (int) = `1000`
- `healAmount` (float) = `0.1f`
- `healAll` (bool) = `true`
- `needsSun` (bool)
- `needsWater` (bool)
- `onlyBleeding` (bool)
- `onlyTendButNotHeal` (bool)
- `tendMin` (float) = `0.7f`
- `tendMax` (float) = `1f`
- `bodypart` (BodyPartDef)

### `VEF.AnimalBehaviours.HediffCompProperties_RegrowLimbs` : HediffCompProperties

- `regeneratingHediff` (HediffDef)

### `VEF.AnimalBehaviours.HediffCompProperties_RemoveFromMechs` : HediffCompProperties

### `VEF.AnimalBehaviours.HediffCompProperties_Resurrect` : HediffCompProperties

- `livesLeft` (int) = `1`

### `VEF.AnimalBehaviours.HediffCompProperties_SpawnMote` : HediffCompProperties

- `moteDef` (ThingDef)
- `offset` (Vector3)
- `maxScale` (float)

### `VEF.AnimalBehaviours.HediffCompProperties_SpawnPawnOnMaxSeverity` : HediffCompProperties

- `pawnKindOptions` (List<PawnKindDef>) = `new List<PawnKindDef>()`
- `filthCreated` (ThingDef)
- `filthCountRange` (IntRange)
- `sound` (SoundDef)
- `damage` (DamageDef)
- `damageAmount` (FloatRange)

### `VEF.AnimalBehaviours.HediffCompProperties_Spawner` : HediffCompProperties

- `thingToSpawn` (ThingDef)
- `spawnCount` (int) = `1`
- `initialSpawnWait` (int) = `10000`
- `spawnIntervalRange` (IntRange) = `new IntRange(100, 100)`
- `spawnMaxAdjacent` (int) = `-1`
- `spawnForbidden` (bool)
- `requiresPower` (bool)
- `writeTimeLeftToSpawn` (bool)
- `showMessageIfOwned` (bool)
- `saveKeysPrefix` (string)
- `inheritFaction` (bool)

### `VEF.AnimalBehaviours.HediffCompProperties_StageByHealth` : HediffCompProperties

- `lowHealthStageIndex` (float) = `0.1f`
- `highHealthStageIndex` (float) = `1f`
- `healthThreshold` (float) = `0.5f`

### `VEF.AnimalBehaviours.HediffCompProperties_StageByPsylink` : HediffCompProperties

### `VEF.AnimalBehaviours.HediffCompProperties_StageBySunlight` : HediffCompProperties

- `sunlightStageIndex` (float) = `0.1f`
- `sunlessStageIndex` (float) = `1f`

### `VEF.AnimalBehaviours.HediffCompProperties_StageByTemperature` : HediffCompProperties

- `minTemp` (int)
- `maxTemp` (int)

### `VEF.AnimalBehaviours.HediffCompProperties_StageByVacuum` : HediffCompProperties

- `notVacuumStageIndex` (float) = `0.1f`
- `vacuumStageIndex` (float) = `1f`
- `vacuumResistanceInArmorDisablesHediff` (bool)
- `vacuumResistanceValueToDisable` (float) = `0.8f`
- `reverseVacuumResistanceEffects` (bool)

### `VEF.AnimalBehaviours.HediffCompProperties_ThoughtEffecter` : HediffCompProperties

- `radius` (int) = `1`
- `tickInterval` (int) = `1000`
- `thoughtDef` (string) = `"AteWithoutTable"`
- `showEffect` (bool)
- `needsToBeTamed` (bool)
- `conditionalOnWellBeing` (bool)
- `thoughtDefWhenSuffering` (string) = `"AteWithoutTable"`

### `VEF.AnimalBehaviours.HediffCompProperties_TurnWhenDead` : HediffCompProperties

- `thingToTurnTo` (string) = `""`
- `severityToTurn` (float) = `0.85f`
- `numberOfSpawn` (List<int>)
- `isHostile` (bool) = `true`
- `keepGender` (bool)
- `factionToTurnTo` (string) = `""`

### `VEF.AnimalBehaviours.HediffCompProperties_Waterstriding` : HediffCompProperties

- `checkingInterval` (int) = `500`

### `VEF.AnimalGenes.HediffCompProperties_GameteGenes` : HediffCompProperties

### `VEF.Cooking.HediffCompProperties_WhileHavingThoughts` : HediffCompProperties

- `thoughtDefs` (List<ThoughtDef>) = `new List<ThoughtDef>()`
- `removeThoughtDefs` (List<ThoughtDef>) = `new List<ThoughtDef>()`
- `hediffReduction` (string) = `""`
- `reductionAmount` (float)
- `resurrectionEffect` (bool)

### `VEF.Genes.HediffCompProperties_CustomBlood` : HediffCompProperties

- `customBloodThingDef` (ThingDef)
- `customBloodIcon` (string) = `""`
- `customBloodEffect` (EffecterDef)
- `customWoundsFromFleshtype` (FleshTypeDef)

### `VEF.Genes.HediffCompProperties_HumanEggLayer` : HediffCompProperties

- `eggUnfertilizedDef` (ThingDef)
- `eggFertilizedDef` (ThingDef)
- `eggLayIntervalDays` (float) = `1f`
- `eggLayFemaleOnly` (bool) = `true`
- `eggProgressUnfertilizedMax` (float) = `1f`
- `maleDominant` (bool)
- `femaleDominant` (bool)

### `VEF.Hediffs.HediffCompProperties_DamageAura` : HediffCompProperties_Draw

- `damageDef` (DamageDef)
- `damageAmount` (float) = `-1f`
- `armorPenetration` (float) = `-1f`
- `ticksBetween` (int)
- `radius` (float)
- `sustainer` (SoundDef)
- `soundEnded` (SoundDef)
- `hostileOnly` (bool) = `true`
- `targetingParameters` (TargetingParameters) = `new TargetingParameters
	{
		canTargetPawns = true,
		canTar`

### `VEF.Hediffs.HediffCompProperties_Draw` : HediffCompProperties

- `graphic` (GraphicData)

### `VEF.Hediffs.HediffCompProperties_Mote` : HediffCompProperties

- `mote` (ThingDef)
- `scale` (float)

### `VEF.Hediffs.HediffCompProperties_MoveSpeedFactorByTerrainTag` : HediffCompProperties

- `moveSpeedFactorByTerrainTag` (Dictionary<string, List<MoveSpeedFactor>>)

### `VEF.Hediffs.HediffCompProperties_Shield` : HediffCompProperties_Draw

- `breakOn` (List<DamageDef>)
- `absorb` (List<DamageDef>)
- `absorbAttackType` (AttackType) = `AttackType.Ranged`
- `cannotUseAttackType` (AttackType) = `AttackType.Ranged`
- `maxEnergy` (float) = `-1f`
- `energyPerTick` (float) = `-1f`
- `rechargeDelay` (int) = `-1`
- `energyLossPerDamage` (float) = `0.033f`
- `fullOnAdd` (bool) = `true`
- `energyPctOnReset` (float) = `0.2f`
- `sustainer` (SoundDef)
- `soundBroken` (SoundDef)
- `soundRecharge` (SoundDef)
- `soundEnded` (SoundDef)
- `absorbedFleck` (FleckDef)
- `brokenFleck` (FleckDef)
- `doDust` (bool) = `true`
- `damageOnAttack` (AttackType)
- `damageType` (DamageDef)
- `damageAmount` (int) = `-1`
- `armorPenetration` (float) = `-1f`
- `doRandomRotation` (bool) = `true`

### `VEF.Hediffs.HediffCompProperties_Spreadable` : HediffCompProperties

- `radiusToSpread` (float)
- `severityToInfect` (float)
- `baseDiseaseContractChance` (float)
- `spreadingTickInterval` (IntRange)
- `socialInteractionTransmissionChance` (float)
- `requiresLineOfSightToSpread` (bool)
- `speciesCanCatch` (List<RaceCategory>)
- `apparelsPreventingSpreading` (List<ThingDef>)
- `statsPreventingSpreading` (List<StatModifier>)
- `fleckDefOnPawn` (FleckDef)
- `fleckSpawnInterval` (IntRange)
- `fleckColor` (Color)
- `fleckScale` (float) = `1f`
- `spreadingMessageWarningKey` (string)

### `VEF.Hediffs.HediffCompProperties_Targeting` : HediffCompProperties

- `neverMiss` (bool)
- `neverHit` (bool)
- `alwaysHit` (bool)
- `alwaysMiss` (bool)
- `targetingMote` (ThingDef)
- `initialTargetingMoteScale` (float)
- `sizeScalesWithProgress` (bool) = `true`
- `targetingLineTexPath` (string)
- `targetingLineColor` (Color) = `Color.red`
- `targetingLineWidth` (float) = `0.2f`
- `maintainLockAfterShot` (bool)
- `skipTargetLock` (bool)
- `aimingGraphicData` (GraphicData)
- `hostileColorFactor` (float) = `1f`

## PatchOperations (`Patches/*.xml`) (1)

### `VEF.PatchOperationToggableSequence` : PatchOperation

- `enabled` (bool)
- `label` (string)

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (255)

### `VEF.AI.Command_ToggleWithRClick` : Command_Toggle

- `rightClickAction` (Action)

### `VEF.AI.DraftedActionHolder` : GameComponent

### `VEF.Abilities.AbilityProjectile` : Projectile

- `ability` (Ability)

### `VEF.Abilities.Ability_Barrier` : Ability

### `VEF.Abilities.Ability_Blank` : Ability

### `VEF.Abilities.Ability_Explode` : Ability

### `VEF.Abilities.Ability_OrbitalStrike` : Ability

### `VEF.Abilities.Ability_ShootProjectile` : Ability

### `VEF.Abilities.Ability_Skyfallers` : Ability

### `VEF.Abilities.Ability_Spawn` : Ability

### `VEF.Abilities.Ability_SpawnBuilding` : Ability

### `VEF.Abilities.Command_Ability` : Command_Action

- `pawn` (Pawn)
- `ability` (Ability)

### `VEF.Abilities.HediffComp_AbilityTargetReact` : HediffComp

### `VEF.Abilities.Hediff_Abilities` : Hediff_Level

- `giveRandomAbilities` (bool) = `true`

### `VEF.Abilities.Hediff_Ability` : HediffWithComps

- `ability` (Ability)

### `VEF.Abilities.JobDriver_CastAbilityOnce` : JobDriver

### `VEF.Abilities.JobDriver_GotoTargetAndCastAbilityOnce` : JobDriver_CastAbilityOnce

### `VEF.Abilities.JobDriver_StandAndFaceTarget` : JobDriver

### `VEF.Abilities.Verb_CastAbility` : Verb

- `ability` (Ability)

### `VEF.AestheticScaling.CachedPawnDataSlowUpdate` : GameComponent

- `pawnsToRefresh` (Queue<Pawn>) = `new Queue<Pawn>()`

### `VEF.AnimalBehaviours.DamageWorker_Berserk` : DamageWorker_AddInjury

### `VEF.AnimalBehaviours.DamageWorker_ExtraDamageAnimals` : DamageWorker_Cut

### `VEF.AnimalBehaviours.DamageWorker_ExtraDamageInsectoids` : DamageWorker_Cut

### `VEF.AnimalBehaviours.DamageWorker_ExtraDamageMechanoids` : DamageWorker_Cut

### `VEF.AnimalBehaviours.DamageWorker_ExtraDamagePirates` : DamageWorker_Bite

### `VEF.AnimalBehaviours.DamageWorker_ExtraInfecter` : DamageWorker_Cut

### `VEF.AnimalBehaviours.DamageWorker_SwallowWhole` : DamageWorker_Cut

### `VEF.AnimalBehaviours.DeathActionWorker_ConfigurableExplosion` : DeathActionWorker

- `Props` (DeathActionProperties_ConfigurableExplosion) = `> (DeathActionProperties_ConfigurableExplosion)(object)base.`

### `VEF.AnimalBehaviours.DeathActionWorker_DivideAndCreateFilth` : DeathActionWorker

- `Props` (DeathActionProperties_DivideAndCreateFilth) = `> (DeathActionProperties_DivideAndCreateFilth)(object)base.p`

### `VEF.AnimalBehaviours.DeathActionWorker_DropOnDeath` : DeathActionWorker

### `VEF.AnimalBehaviours.DeathActionWorker_SpawnItemsAndFilth` : DeathActionWorker

- `Props` (DeathActionProperties_SpawnItemsAndFilth) = `> (DeathActionProperties_SpawnItemsAndFilth)(object)base.pro`

### `VEF.AnimalBehaviours.DeathActionWorker_VanishInsect` : DeathActionWorker

- `Props` (DeathActionProperties_VanishInsect) = `> (DeathActionProperties_VanishInsect)(object)base.props`

### `VEF.AnimalBehaviours.Electric_Projectile` : Projectile

### `VEF.AnimalBehaviours.HediffComp_Ability` : HediffComp

- `tickCounter` (int)
- `Props` (HediffCompProperties_Ability) = `> (HediffCompProperties_Ability)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_Animation` : HediffComp

- `Props` (HediffCompProperties_Animation) = `> (HediffCompProperties_Animation)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_AsexualReproduction` : HediffComp

- `ticksInday` (int) = `60000`
- `asexualFissionCounter` (int)
- `Props` (HediffCompProperties_AsexualReproduction) = `> (HediffCompProperties_AsexualReproduction)(object)base.pro`

### `VEF.AnimalBehaviours.HediffComp_AutoPermanentInjury` : HediffComp

### `VEF.AnimalBehaviours.HediffComp_CauseIncident` : HediffComp

- `waitingForNight` (bool)
- `checkingForNightInterval` (int) = `100`
- `Props` (HediffCompProperties_CauseIncident) = `> (HediffCompProperties_CauseIncident)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_ChangeWeather` : HediffComp

- `Props` (HediffCompProperties_ChangeWeather) = `> (HediffCompProperties_ChangeWeather)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_CorpseDecayer` : HediffComp

- `flagOnce` (bool)
- `Props` (HediffCompProperties_CorpseDecayer) = `> (HediffCompProperties_CorpseDecayer)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_DieAfterPeriod` : HediffComp

- `tickCounter` (int)
- `Props` (HediffCompProperties_DieAfterPeriod) = `> (HediffCompProperties_DieAfterPeriod)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_Draftable` : HediffComp

- `tickCounter` (int)
- `Props` (HediffCompProperties_Draftable) = `> (HediffCompProperties_Draftable)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_Electrified` : HediffComp

- `Props` (HediffCompProperties_Electrified) = `> (HediffCompProperties_Electrified)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_ExplodeOnDamage` : HediffComp

- `Props` (HediffCompProperties_ExplodeOnDamage) = `> (HediffCompProperties_ExplodeOnDamage)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_ExplodeOnDowned` : HediffComp

- `checkEveryTicks` (int) = `60`
- `Props` (HediffCompProperties_ExplodeOnDowned) = `> (HediffCompProperties_ExplodeOnDowned)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_ExplodeOnFire` : HediffComp

- `onCoolDown` (bool)
- `coolDownCounter` (int)
- `Props` (HediffCompProperties_ExplodeOnFire) = `> (HediffCompProperties_ExplodeOnFire)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_Exploder` : HediffComp

- `Props` (HediffCompProperties_Exploder) = `> (HediffCompProperties_Exploder)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_FilthProducer` : HediffComp

- `Props` (HediffCompProperties_FilthProducer) = `> (HediffCompProperties_FilthProducer)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_Floating` : HediffComp

- `Props` (HediffCompProperties_Floating) = `> (HediffCompProperties_Floating)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_GasProducer` : HediffComp

- `Props` (HediffCompProperties_GasProducer) = `> (HediffCompProperties_GasProducer)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_GiveHediffsInRangeExpanded` : HediffComp

- `Props` (HediffCompProperties_GiveHediffsInRangeExpanded) = `> (HediffCompProperties_GiveHediffsInRangeExpanded)(object)b`

### `VEF.AnimalBehaviours.HediffComp_HeatPusher` : HediffComp

- `Props` (HediffCompProperties_HeatPusher) = `> (HediffCompProperties_HeatPusher)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_HighlyFlammable` : HediffComp

- `Props` (HediffCompProperties_HighlyFlammable) = `> (HediffCompProperties_HighlyFlammable)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_LastStand` : HediffComp

- `tickCounter` (int)
- `Props` (HediffCompProperties_LastStand) = `> (HediffCompProperties_LastStand)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_LightSustenance` : HediffComp

- `growOptimalGlow` (float) = `0.4f`
- `Props` (HediffCompProperties_LightSustenance) = `> (HediffCompProperties_LightSustenance)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_MentalBreakOnDamage` : HediffComp

- `Props` (HediffCompProperties_MentalBreakOnDamage) = `> (HediffCompProperties_MentalBreakOnDamage)(object)base.pro`

### `VEF.AnimalBehaviours.HediffComp_NearbyEffecter` : HediffComp

- `Props` (HediffCompProperties_NearbyEffecter) = `> (HediffCompProperties_NearbyEffecter)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_PassiveRegenerator` : HediffComp

- `pawnList` (List<Pawn>) = `new List<Pawn>()`
- `thisPawn` (Pawn)
- `Props` (HediffCompProperties_PassiveRegenerator) = `> (HediffCompProperties_PassiveRegenerator)(object)base.prop`

### `VEF.AnimalBehaviours.HediffComp_PassiveResearch` : HediffComp

- `Props` (HediffCompProperties_PassiveResearch) = `> (HediffCompProperties_PassiveResearch)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_PlaySound` : HediffComp

- `Props` (HediffCompProperties_PlaySound) = `> (HediffCompProperties_PlaySound)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_Polluter` : HediffComp

- `Props` (HediffCompProperties_Polluter) = `> (HediffCompProperties_Polluter)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_PsyfocusRegeneration` : HediffComp

- `Props` (HediffCompProperties_PsyfocusRegeneration) = `> (HediffCompProperties_PsyfocusRegeneration)(object)base.pr`

### `VEF.AnimalBehaviours.HediffComp_ReducePrisonerCertainty` : HediffComp

- `Props` (HediffCompProperties_ReducePrisonerCertainty) = `> (HediffCompProperties_ReducePrisonerCertainty)(object)base`

### `VEF.AnimalBehaviours.HediffComp_ReducePrisonerResistance` : HediffComp

- `Props` (HediffCompProperties_ReducePrisonerResistance) = `> (HediffCompProperties_ReducePrisonerResistance)(object)bas`

### `VEF.AnimalBehaviours.HediffComp_Regeneration` : HediffComp

- `Props` (HediffCompProperties_Regeneration) = `> (HediffCompProperties_Regeneration)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_RegrowLimbs` : HediffComp

- `Props` (HediffCompProperties_RegrowLimbs) = `> (HediffCompProperties_RegrowLimbs)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_RemoveFromMechs` : HediffComp

- `Props` (HediffCompProperties_RemoveFromMechs) = `> (HediffCompProperties_RemoveFromMechs)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_Resurrect` : HediffComp

- `resurrectionsLeft` (int) = `1`
- `Props` (HediffCompProperties_Resurrect) = `> (HediffCompProperties_Resurrect)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_SpawnMote` : HediffComp

- `spawnedMote` (Mote)
- `Props` (HediffCompProperties_SpawnMote) = `> base.props as HediffCompProperties_SpawnMote`

### `VEF.AnimalBehaviours.HediffComp_SpawnPawnOnMaxSeverity` : HediffComp

- `Props` (HediffCompProperties_SpawnPawnOnMaxSeverity) = `> base.props as HediffCompProperties_SpawnPawnOnMaxSeverity`

### `VEF.AnimalBehaviours.HediffComp_Spawner` : HediffComp

- `PropsSpawner` (HediffCompProperties_Spawner) = `> (HediffCompProperties_Spawner)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_StageByHealth` : HediffComp

- `Props` (HediffCompProperties_StageByHealth) = `> (HediffCompProperties_StageByHealth)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_StageByPsylink` : HediffComp

- `Props` (HediffCompProperties_StageByPsylink) = `> (HediffCompProperties_StageByPsylink)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_StageBySunlight` : HediffComp

- `Props` (HediffCompProperties_StageBySunlight) = `> (HediffCompProperties_StageBySunlight)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_StageByTemperature` : HediffComp

- `Props` (HediffCompProperties_StageByTemperature) = `> (HediffCompProperties_StageByTemperature)(object)base.prop`

### `VEF.AnimalBehaviours.HediffComp_StageByVacuum` : HediffComp

- `Props` (HediffCompProperties_StageByVacuum) = `> (HediffCompProperties_StageByVacuum)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_ThoughtEffecter` : HediffComp

- `pawnList` (List<Pawn>) = `new List<Pawn>()`
- `thisPawn` (Pawn)
- `Props` (HediffCompProperties_ThoughtEffecter) = `> (HediffCompProperties_ThoughtEffecter)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_TurnWhenDead` : HediffComp

- `Props` (HediffCompProperties_TurnWhenDead) = `> (HediffCompProperties_TurnWhenDead)(object)base.props`

### `VEF.AnimalBehaviours.HediffComp_Waterstriding` : HediffComp

- `Props` (HediffCompProperties_Waterstriding) = `> (HediffCompProperties_Waterstriding)(object)base.props`

### `VEF.AnimalBehaviours.Hediff_AcidBuildup` : HediffWithComps

- `comp` (CompAcidImmunity)

### `VEF.AnimalBehaviours.JobDriver_AnimalResource` : JobDriver_GatherAnimalBodyResources

### `VEF.AnimalBehaviours.JobDriver_AutoNutrition` : JobDriver

### `VEF.AnimalBehaviours.JobDriver_DestroyItem` : JobDriver

### `VEF.AnimalBehaviours.JobDriver_IngestWeird` : JobDriver

### `VEF.AnimalBehaviours.JobDriver_LayExplodingEgg` : JobDriver

### `VEF.AnimalBehaviours.JobGiver_GetWeirdFood` : ThinkNode_JobGiver

- `forceScanWholeMap` (bool)

### `VEF.AnimalBehaviours.JobGiver_Harvest` : ThinkNode

- `emergency` (bool)

### `VEF.AnimalBehaviours.JobGiver_LayExplodingEgg` : ThinkNode_JobGiver

### `VEF.AnimalBehaviours.JobGiver_XenophobicRage` : ThinkNode_JobGiver

### `VEF.AnimalBehaviours.MentalStateWorker_XenophobicRage` : MentalStateWorker

### `VEF.AnimalBehaviours.Pawn_SwallowWhole` : Pawn

- `innerContainer` (ThingOwner)
- `tickCounter` (int)

### `VEF.AnimalBehaviours.StatWorker_AnimalProductIntervalFactor` : StatWorker

### `VEF.AnimalBehaviours.StatWorker_AnimalProductYieldFactor` : StatWorker

### `VEF.AnimalBehaviours.WorkGiver_AnimalResource` : WorkGiver_GatherAnimalBodyResources

### `VEF.AnimalBehaviours.WorkGiver_DestroyItem` : WorkGiver_Scanner

### `VEF.AnimalGenes.HediffComp_GameteGenes` : HediffComp

- `genes` (List<AnimalGeneDef>) = `new List<AnimalGeneDef>()`

### `VEF.AnimalGenes.ITab_AnimalGenes` : ITab

### `VEF.AnimalGenes.WorldComponent_AnimalGenes` : WorldComponent

### `VEF.Apparels.Apparel_Shield` : Apparel

### `VEF.Apparels.Command_ActionWithCooldown` : Command_Action

### `VEF.Apparels.Gizmo_EnergyCompShieldStatus` : Gizmo

- `shield` (CompShieldBubble)

### `VEF.Apparels.Gizmo_EnergyShieldGeneratorStatus` : Gizmo

- `shieldGenerator` (CompShieldField)

### `VEF.Apparels.JobDriver_EquipShield` : JobDriver_Equip

### `VEF.Apparels.StatWorker_MultiplyBy100` : StatWorker

### `VEF.Buildings.Building_AutoDoorLockable` : Building_Door

- `DoorStateMaterial` (Material) = `> curDoorAccess switch
	{
		DoorAccess.Everyone => MaterialP`

### `VEF.Buildings.Command_SetItemsToSpawn` : Command

- `building` (CompConfigurableSpawner)

### `VEF.Buildings.Command_SetStoneType` : Command

- `building` (CompRockSpawner)

### `VEF.Buildings.DoorTeleporter` : ThingWithComps

- `backgroundMat` (Material)
- `background1` (RenderTexture)
- `background2` (RenderTexture)
- `rotation` (float)
- `distortAmount` (float) = `1.5f`
- `backgroundOffset` (Vector2)
- `sustainer` (Sustainer)
- `teleportEffecters` (Dictionary<Thing, Effecter>) = `new Dictionary<Thing, Effecter>()`
- `BaseLabel` (string) = `> ((Def)((Thing)this).def).label`
- `InspectLabel` (string) = `> RenamableLabel`

### `VEF.Buildings.DummyGlower` : Building

- `parentComp` (CompGlowerExtended)

### `VEF.Buildings.Gizmo_SetSecondaryFuelLevel` : Gizmo_Slider

### `VEF.Buildings.JobDriver_Loot` : JobDriver

- `totalTimeCached` (int) = `-1`
- `totalTimer` (int)

### `VEF.Buildings.JobDriver_RefuelSecondary` : JobDriver

### `VEF.Buildings.JobDriver_StudyBuilding` : JobDriver

- `totalTimer` (int)

### `VEF.Buildings.JobDriver_UseDoorTeleporter` : JobDriver

- `targetCell` (IntVec3)
- `Origin` (DoorTeleporter) = `> base.job.targetA.Thing as DoorTeleporter`
- `Dest` (DoorTeleporter) = `> base.job.globalTarget.Thing as DoorTeleporter`

### `VEF.Buildings.LootBox` : ThingWithComps

- `cachedLootBoxExtension` (LootBoxExtension)
- `cachedCompQuality` (CompQuality)
- `OpenTicks` (int) = `> 300`
- `CanOpen` (bool) = `> true`

### `VEF.Buildings.LootableBuilding` : Building

- `OpenTicks` (int) = `> 300`

### `VEF.Buildings.LootableBuilding_Custom` : Building

### `VEF.Buildings.PlaceWorker_AttachedToWallMultiCell` : PlaceWorker

### `VEF.Buildings.PlaceWorker_CompCustomCauseHediff_AoE` : PlaceWorker

### `VEF.Buildings.PlaceWorker_DeepDrillLimitation` : PlaceWorker

### `VEF.Buildings.PlaceWorker_GodModeVisible` : PlaceWorker

### `VEF.Buildings.PlaceWorker_OnSand` : PlaceWorker

### `VEF.Buildings.PlaceWorker_OnWall` : PlaceWorker

### `VEF.Buildings.PlaceWorker_OnWater` : PlaceWorker

### `VEF.Buildings.StudiableBuilding` : Building

### `VEF.Buildings.SwappableBuilding` : Building

- `tickCounter` (int)

### `VEF.Buildings.ThoughtWorker_ThoughtFromNearbyThingDef` : ThoughtWorker

### `VEF.Buildings.WorkGiver_Loot` : WorkGiver_Scanner

### `VEF.Buildings.WorkGiver_RefuelSecondary` : WorkGiver_Scanner

### `VEF.Buildings.WorkGiver_StudyBuilding` : WorkGiver_Scanner

### `VEF.Buildings.WorldComponent_DoorTeleporterManager` : WorldComponent

### `VEF.Cooking.GourmetMeal` : ThingWithComps

### `VEF.Cooking.HediffComp_WhileHavingThoughts` : HediffComp

- `flagAmIThinking` (bool)
- `checkingInterval` (int) = `600`
- `Props` (HediffCompProperties_WhileHavingThoughts) = `> (HediffCompProperties_WhileHavingThoughts)(object)base.pro`

### `VEF.Factions.FactionGoodwillModifier` : WorldComponent

### `VEF.Factions.NewFactionSpawningState` : WorldComponent

### `VEF.Factions.GameComponents.WorldComponent_FactionGoodwillImpactManager` : WorldComponent

### `VEF.Genes.GameComponent_GeneGoodies` : GameComponent

- `sentOncePerGame` (bool)

### `VEF.Genes.HediffComp_CustomBlood` : HediffComp

- `Props` (HediffCompProperties_CustomBlood) = `> (HediffCompProperties_CustomBlood)(object)base.props`

### `VEF.Genes.HediffComp_HumanEggLayer` : HediffComp

- `pregnancyRemovalCounter` (int) = `-1`
- `motherGenes` (List<GeneDef>) = `new List<GeneDef>()`
- `fatherGenes` (List<GeneDef>) = `new List<GeneDef>()`
- `FullyFertilized` (bool) = `> fertilizationCount >= 1`
- `Props` (HediffCompProperties_HumanEggLayer) = `> (HediffCompProperties_HumanEggLayer)(object)base.props`

### `VEF.Genes.MapComponent_AstrogeneOverrides` : MapComponent

### `VEF.Genes.MapComponent_GeneGoodies` : MapComponent

### `VEF.Graphics.DynamicGraphicBuilding` : Building

### `VEF.Graphics.DynamicGraphicThing` : ThingWithComps

### `VEF.Graphics.JobDriver_CustomizeItem` : JobDriver

### `VEF.Graphics.PawnRenderNodeWorker_OmniBodyApparel` : PawnRenderNodeWorker_Body

### `VEF.Graphics.TaggedColorPropManager` : TaggedPropManagerBase

### `VEF.Graphics.TaggedPropManagerBase` : GameComponent

### `VEF.Graphics.TaggedTextPropManager` : TaggedPropManagerBase

### `VEF.Graphics.ThingWithFloorGraphic` : ThingWithComps

- `graphicIntOverride` (Graphic)

### `VEF.Hediffs.HediffComp_DamageAura` : HediffComp_Draw

- `Props` (HediffCompProperties_DamageAura) = `> ((HediffComp)this).props as HediffCompProperties_DamageAur`

### `VEF.Hediffs.HediffComp_Draw` : HediffComp

### `VEF.Hediffs.HediffComp_Mote` : HediffComp

- `mote` (Mote)
- `Props` (HediffCompProperties_Mote) = `> base.props as HediffCompProperties_Mote`

### `VEF.Hediffs.HediffComp_MoveSpeedFactorByTerrainTag` : HediffComp

- `Props` (HediffCompProperties_MoveSpeedFactorByTerrainTag) = `> (HediffCompProperties_MoveSpeedFactorByTerrainTag)(object)`

### `VEF.Hediffs.HediffComp_Phasing` : HediffComp

### `VEF.Hediffs.HediffComp_Shield` : HediffComp_Draw

- `energy` (float)
- `useEnergy` (bool)
- `Props` (HediffCompProperties_Shield) = `> ((HediffComp)this).props as HediffCompProperties_Shield`

### `VEF.Hediffs.HediffComp_Spreadable` : HediffComp

- `nextSpreadingTick` (int)
- `nextFleckSpawnTick` (int)
- `Props` (HediffCompProperties_Spreadable) = `> base.props as HediffCompProperties_Spreadable`

### `VEF.Hediffs.HediffComp_Targeting` : HediffComp

- `mote` (Mote)
- `lockedTarget` (Thing)
- `Props` (HediffCompProperties_Targeting) = `> base.props as HediffCompProperties_Targeting`
- `TargetingLine` (Material) = `> Props.TargetingLine(Hostile)`
- `Hostile` (bool) = `> GenHostility.HostileTo((Thing)(object)((HediffComp)this).P`

### `VEF.Maps.SymbolResolverShips` : SymbolResolver

### `VEF.Maps.TileMutatorWorker_ExtraAnimal` : TileMutatorWorker

### `VEF.Maps.TileMutatorWorker_GenericKCSGSpawner` : TileMutatorWorker

### `VEF.Maps.TileMutatorWorker_GenericPrefabSpawner` : TileMutatorWorker

### `VEF.Maps.TileMutatorWorker_GenericSpawner` : TileMutatorWorker

### `VEF.Maps.TileMutatorWorker_PlantsWithCommonality` : TileMutatorWorker

- `cachedExtension` (TileMutatorExtension)

### `VEF.Maps.TileMutatorWorker_TerrainSwapper` : TileMutatorWorker

### `VEF.Memes.GameComponent_IdeologicalGoodies` : GameComponent

- `sentOncePerGame` (bool)

### `VEF.Memes.MapComponent_IdeologicalGoodies` : MapComponent

### `VEF.Memes.PlaceWorker_DisabledByMeme` : PlaceWorker

### `VEF.Pawns.StatWorker_MassCarryCapacity` : StatWorker

### `VEF.Pawns.ThoughtWorker_InVacuum` : ThoughtWorker

### `VEF.Pawns.ThoughtWorker_NotInVacuum` : ThoughtWorker

### `VEF.Pawns.ThoughtWorker_NotInVacuum_Inverted` : ThoughtWorker

### `VEF.Pawns.WorkGiver_ConstructionSkill_DeliverResourcesToBlueprints` : WorkGiver_ConstructDeliverResourcesToBlueprints

### `VEF.Pawns.WorkGiver_ConstructionSkill_DeliverResourcesToFrames` : WorkGiver_ConstructDeliverResourcesToFrames

### `VEF.Pawns.WorkGiver_ConstructionSkill_FinishFrames` : WorkGiver_ConstructFinishFrames

### `VEF.Planet.HiringContractTracker` : WorldComponent

- `deadCount` (Dictionary<Hireable, List<ExposablePair>>) = `new Dictionary<Hireable, List<ExposablePair>>()`
- `endTicks` (int)
- `factionDef` (HireableFactionDef)
- `hireable` (Hireable)
- `pawns` (List<Pawn>) = `new List<Pawn>()`
- `price` (float)

### `VEF.Planet.JobDriver_LeaveMap` : JobDriver

### `VEF.Plants.IncidentWorker_Weeds` : IncidentWorker

### `VEF.Plants.JobDriver_ExtractFlower` : JobDriver

### `VEF.Plants.JobDriver_RemoveWeeds` : JobDriver

### `VEF.Plants.StatPart_BeautyByAge` : StatPart

### `VEF.Plants.StatPart_BeautyByBlooming` : StatPart

### `VEF.Plants.StatPart_BeautyByLeafless` : StatPart

### `VEF.Plants.StatPart_BeautyByWeeds` : StatPart

### `VEF.Plants.WorkGiver_ExtractFlower` : WorkGiver_Scanner

### `VEF.Plants.WorkGiver_RemoveWeeds` : WorkGiver_Scanner

### `VEF.Sounds.ForcedMusicManager` : GameComponent

### `VEF.Sounds.QuestNode_ForceMusic` : QuestNode

- `inSignalEnable` (SlateRef<string>)
- `inSignalDisable` (SlateRef<string>)
- `possibleSongs` (SlateRef<List<SongDef>>)
- `priority` (SlateRef<int>)

### `VEF.Storyteller.GameComponent_QuestChains` : GameComponent

- `quests` (List<QuestInfo>) = `new List<QuestInfo>()`
- `futureQuests` (List<FutureQuestInfo>) = `new List<FutureQuestInfo>()`
- `questChainStates` (Dictionary<QuestChainDef, QuestChainState>) = `new Dictionary<QuestChainDef, QuestChainState>()`
- `QuestsInChains` (List<QuestScriptDef>) = `> questsInChains ?? (questsInChains = DefDatabase<QuestScrip`

### `VEF.Storyteller.GenStep_Site` : GenStep

- `structureSetDef` (StructureSetDef)

### `VEF.Storyteller.IncidentWorker_RaidEnemySpecial` : IncidentWorker_RaidEnemy

### `VEF.Storyteller.IncidentWorker_Reinforcements` : IncidentWorker_RaidEnemy

### `VEF.Storyteller.JobDriver_UseQuestGiver` : JobDriver

### `VEF.Storyteller.QuestNode_GetFaction` : QuestNode

- `storeAs` (SlateRef<string>)
- `factionDef` (SlateRef<FactionDef>)
- `allowEnemy` (SlateRef<bool>)
- `allowNeutral` (SlateRef<bool>)
- `allowAlly` (SlateRef<bool>)
- `allowAskerFaction` (SlateRef<bool>)
- `allowPermanentEnemy` (SlateRef<bool?>)
- `mustBePermanentEnemy` (SlateRef<bool>)
- `playerCantBeAttackingCurrently` (SlateRef<bool>)
- `peaceTalksCantExist` (SlateRef<bool>)
- `leaderMustBeSafe` (SlateRef<bool>)
- `mustHaveGoodwillRewardsEnabled` (SlateRef<bool>)
- `ofPawn` (SlateRef<Pawn>)
- `mustBeHostileToFactionOf` (SlateRef<Thing>)
- `exclude` (SlateRef<IEnumerable<Faction>>)
- `allowedHiddenFactions` (SlateRef<IEnumerable<Faction>>)

### `VEF.Storyteller.QuestNode_Site` : QuestNode

- `sitePartDef` (SlateRef<SitePartDef>)
- `distanceRange` (SlateRef<IntRange>)
- `keepSiteWhenQuestActive` (SlateRef<bool>)

### `VEF.Storyteller.StorytellerWatcher` : GameComponent

- `lastRaidExpansionTicks` (int)
- `currentStoryteller` (StorytellerDef)
- `raidGroups` (List<RaidGroup>)
- `reinforcementGroups` (List<RaidGroup>)
- `raidQueues` (List<RaidQueue>)
- `questGiverManagers` (Dictionary<int, QuestGiverManager>)

### `VEF.Storyteller.ThoughtWorker_RaidRestlessness` : ThoughtWorker

### `VEF.Things.CustomFireSpark` : Projectile

- `fireDef` (ThingDef)

### `VEF.Things.DamageWorker_CustomFire` : DamageWorker_AddInjury

### `VEF.Things.DamageWorker_ExtinguishCustomFire` : DamageWorker

### `VEF.Things.JobDriver_BeatCustomFire` : JobDriver

### `VEF.Things.JobDriver_ExtinguishCustomFireSelf` : JobDriver

### `VEF.Things.JobDriver_ExtinguishCustomFiresNearby` : JobDriver

### `VEF.Things.JobGiver_ExtinguishCustomFireSelf` : ThinkNode_JobGiver

### `VEF.Things.ThinkNode_ConditionalCustomFireBurning` : ThinkNode_Conditional

- `mustSatisfyAllConditions` (bool)
- `requirePanicCausingFire` (bool)
- `requireCanExtinguishInWater` (bool)

### `VEF.Things.WorkGiver_FightCustomFires` : WorkGiver_Scanner

### `VEF.Weapons.BeamDraw` : ThingWithComps

### `VEF.Weapons.BeamProjectile` : Projectile_Explosive

### `VEF.Weapons.Building_LaserGun` : Building_TurretGun

- `isCharged` (bool)
- `previousBurstCooldownTicksLeft` (int)
- `def` (Building_LaserGunDef) = `> ((Thing)this).def as Building_LaserGunDef`
- `BurstCooldownTicksLeft` (int) = `> base.burstCooldownTicksLeft`
- `BurstWarmupTicksLeft` (int) = `> base.burstWarmupTicksLeft`

### `VEF.Weapons.DamageWorker_BombNoCamShake` : DamageWorker_AddInjury

### `VEF.Weapons.DamageWorker_ExtinguishNoCamShake` : DamageWorker

### `VEF.Weapons.DamageWorker_FlameNoCamShake` : DamageWorker_AddInjury

### `VEF.Weapons.DamageWorker_OPToxic` : DamageWorker

### `VEF.Weapons.DamageWorker_PushBackAttackMelee` : DamageWorker_Blunt

### `VEF.Weapons.ExpandableBullet` : ExpandableProjectile

### `VEF.Weapons.ExpandableProjectile` : Bullet

- `startingPosition` (Vector3)
- `doFinalAnimations` (bool)
- `pawnMoved` (bool)
- `curPosition` (Nullable)
- `hitThings` (List<Thing>)
- `ProgressPct` (float) = `> (float)curDuration / (float)def.lifeTimeDuration`
- `def` (ExpandableProjectileDef) = `> ((Thing)this).def as ExpandableProjectileDef`

### `VEF.Weapons.FlamethrowProjectile` : ExpandableProjectile

- `affectedCells` (HashSet<IntVec3>) = `new HashSet<IntVec3>()`

### `VEF.Weapons.GaussProjectile` : ExpandableProjectile

- `damageFalloff` (float)

### `VEF.Weapons.GaussProjectileDefaultDamageWorker` : GaussProjectileDamageWorker

### `VEF.Weapons.GaussProjectileLinearDamageWorker` : GaussProjectileDamageWorker

### `VEF.Weapons.JobDriver_ChangeLaserColor` : JobDriver

### `VEF.Weapons.JobDriver_ReloadWeaponTrait` : JobDriver

### `VEF.Weapons.JobGiver_ReloadWeaponTrait` : ThinkNode_JobGiver

### `VEF.Weapons.LaserBeam` : Bullet

### `VEF.Weapons.LaserBeamGraphic` : Thing

- `laserBeam` (LaserBeam)
- `drawingMatrix` (Matrix4x4)
- `LaserBeamDef` (LaserBeamDef) = `> ((Thing)(laserBeam?)).def as LaserBeamDef`
- `Opacity` (float) = `> (float)Math.Sin(Math.Pow(1.0 - 1.0 * (double)ticks / (doub`

### `VEF.Weapons.LaserGun` : ThingWithComps

- `def` (LaserGunDef) = `> (((Thing)this).def as LaserGunDef) ?? LaserGunDef.defaultO`

### `VEF.Weapons.PlaceWorker_ExpandedShowTurretRadius` : PlaceWorker

### `VEF.Weapons.ProjectileProperties_Shrapnel` : ProjectileProperties

- `angleVariance` (float)
- `shrapnelCount` (int)
- `shrapnelProjectile` (ThingDef)
- `shrapnelRange` (float)

### `VEF.Weapons.Projectile_Attachments` : Bullet

- `cachedProjectileExtension` (ProjectileExtension)

### `VEF.Weapons.Projectile_Attachments_Explosive` : Projectile_Explosive

- `cachedProjectileExtension` (ProjectileExtension)

### `VEF.Weapons.Projectile_DoomsdaySmokeReleaser` : Projectile_DoomsdayRocket

### `VEF.Weapons.Projectile_FireTrail` : Projectile_Explosive

### `VEF.Weapons.Projectile_FlameThrower` : Projectile_Explosive

### `VEF.Weapons.Projectile_GasGrenade` : Projectile_Explosive

### `VEF.Weapons.Projectile_Healing` : Bullet

### `VEF.Weapons.Projectile_IncendiaryLauncher` : Projectile_Explosive

### `VEF.Weapons.Projectile_RocketSmokeReleaser` : Projectile_Explosive

### `VEF.Weapons.Projectile_Shrapnel` : Projectile_Explosive

- `Props` (ProjectileProperties_Shrapnel) = `> ((Thing)this).def.projectile as ProjectileProperties_Shrap`

### `VEF.Weapons.Projectile_ShrapnelPiece` : Bullet

### `VEF.Weapons.Projectile_SmokeGrenade` : Projectile_Explosive

### `VEF.Weapons.Projectile_SmokeReleaserMortar` : Projectile_Explosive

### `VEF.Weapons.Projectile_TopAttackMissile` : Projectile_Explosive

### `VEF.Weapons.SpinningLaserGun` : SpinningLaserGunBase

### `VEF.Weapons.SpinningLaserGunBase` : LaserGun

- `state` (State)
- `def` (new SpinningLaserGunDef) = `> base.def as SpinningLaserGunDef`

### `VEF.Weapons.SpinningLaserGunTurret` : SpinningLaserGunBase

### `VEF.Weapons.StatPart_Ammo` : StatPart

### `VEF.Weapons.TeslaProjectile` : Bullet

- `curLifetime` (int)
- `Props` (TeslaChainingProps) = `> ((Def)((Thing)this).def).GetModExtension<TeslaChainingProp`

### `VEF.Weapons.VerbProps_ShootCone` : VerbProperties

- `coneAngle` (int)

### `VEF.Weapons.Verb_MakeshiftShoot` : Verb_ShootWithSmoke

### `VEF.Weapons.Verb_Shoot` : Verb_LaunchProjectile

### `VEF.Weapons.Verb_ShootCone` : Verb_Shoot

- `VerbProps` (VerbProps_ShootCone) = `> ((Verb)this).verbProps as VerbProps_ShootCone`

### `VEF.Weapons.Verb_ShootOneUse_FlyOverhead` : Verb_ShootOneUse

### `VEF.Weapons.Verb_ShootWithFlintlockSmoke` : Verb_Shoot

### `VEF.Weapons.Verb_ShootWithSmoke` : Verb_Shoot

### `VEF.Weapons.Verb_Shoot_FlyOverhead` : Verb_Shoot

### `VEF.Weapons.WeaponTraitWorker_Extended` : WeaponTraitWorker

## Mod settings / ModBase (14)

### `VEF.VEF_Mod` : Mod

### `VEF.VFEGlobal` : Mod

### `VEF.VFEGlobalSettings` : ModSettings

- `toggablePatch` (Dictionary<string, bool>) = `new Dictionary<string, bool>()`
- `enableVerboseLogging` (bool)
- `disableCaching` (bool)
- `randomStartsAsRandom` (bool)
- `hideRandomizeButtons` (bool)
- `enablePipeSystemNoStorageAlert` (bool) = `true`
- `weatherDamagesOptions` (Dictionary<string, bool>) = `new Dictionary<string, bool>()`
- `disableModSourceReport` (bool)

### `VEF.AnimalBehaviours.AnimalBehaviours_Mod` : Mod

### `VEF.AnimalBehaviours.AnimalBehaviours_Settings` : ModSettings

### `VEF.AnimalBehaviours.VanillaAnimalsExpanded_Mod` : Mod

### `VEF.AnimalBehaviours.VanillaAnimalsExpanded_Settings` : ModSettings

- `pawnSpawnStates` (Dictionary<string, bool>) = `new Dictionary<string, bool>()`

### `VEF.Apparels.VanillaShieldsExpandedMod` : Mod

### `VEF.Apparels.VanillaShieldsExpandedSettings` : ModSettings

- `showMeleeWeapons` (bool) = `true`
- `showRangedWeapons` (bool) = `true`

### `VEF.Buildings.Furniture_Mod` : Mod

### `VEF.Buildings.Furniture_Settings` : ModSettings

### `VEF.OptionalFeatures.OptionalFeatures` : Mod

### `VEF.Weapons.HeavyWeaponsMod` : Mod

### `VEF.Weapons.HeavyWeaponsSettings` : ModSettings

- `weaponStates` (Dictionary<string, bool>) = `new Dictionary<string, bool>()`
- `weaponHPStates` (Dictionary<string, int>) = `new Dictionary<string, int>()`

