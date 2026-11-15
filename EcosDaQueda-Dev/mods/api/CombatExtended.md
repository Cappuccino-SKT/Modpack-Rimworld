# Referência de API: Combat Extended

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (18)

### `CombatExtended.AmmoCategoryDef` : Def

- `advanced` (bool) = `false`
- `labelShort` (string)
- `LabelCapShort` (string) = `> GenText.CapitalizeFirst(labelShort)`

### `CombatExtended.AmmoDef` : ThingDef

- `ammoClass` (AmmoCategoryDef)
- `defaultAmmoCount` (int) = `1`
- `cookOffSpeed` (float) = `1f`
- `cookOffFlashScale` (float) = `1f`
- `worldTilesPerTick` (float) = `0.2f`
- `menuHidden` (bool)
- `cookOffProjectile` (ThingDef) = `null`
- `cookOffSound` (SoundDef) = `null`
- `cookOffTailSound` (SoundDef) = `null`
- `detonateProjectile` (ThingDef) = `null`
- `isMortarAmmo` (bool) = `false`
- `spawnAsSiegeAmmo` (bool) = `true`
- `ammoCount` (int) = `1`
- `partialUnloadAmmoDef` (ThingDef) = `null`
- `ammoTags` (List<string>)

### `CombatExtended.AmmoInjectorOptions` : Def

- `benchesByTag` (BenchesByTag)

### `CombatExtended.AmmoSetDef` : Def

- `ammoTypes` (List<AmmoLink>)
- `isMortarAmmoSet` (bool) = `false`
- `similarTo` (AmmoSetDef)
- `ammoConsumedPerShot` (int) = `1`

### `CombatExtended.ApparelModBlacklist` : Def

- `modIDs` (List<string>)
- `defNames` (List<string>)

### `CombatExtended.ApparelPatcherPresetDef` : Def

- `neededLayers` (List<ApparelLayerDef>)
- `vanillaArmorRatingRange` (FloatRange)
- `neededGroups` (List<BodyPartGroupDef>)
- `ArmorCurveSharp` (SimpleCurve) = `null`
- `ArmorCurveBlunt` (SimpleCurve) = `null`
- `ArmorStaticSharp` (float)
- `ArmorStaticBlunt` (float)
- `Bulk` (float)
- `Mass` (float)
- `BulkWorn` (float)
- `partialStats` (List<ApparelPartialStat>)

### `CombatExtended.AttachmentDef` : ThingDef

- `slotTags` (List<string>)
- `attachmentTags` (List<string>)
- `attachmentGraphicData` (GraphicData)
- `outlineGraphicData` (GraphicData)
- `statOffsets` (List<StatModifier>)
- `statMultipliers` (List<StatModifier>)
- `statReplacers` (List<StatModifier>)
- `statsValidated` (bool) = `false`

### `CombatExtended.BipodCategoryDef` : Def

- `bipod_id` (string) = `"Bipod_Undefined"`
- `ad_Range` (int) = `0`
- `setuptime` (int) = `240`
- `recoil_mult_setup` (float) = `1f`
- `recoil_mult_NOT_setup` (float) = `1f`
- `warmup_mult_setup` (float) = `1f`
- `warmup_mult_NOT_setup` (float) = `1f`
- `swayMult` (float) = `1f`
- `swayPenalty` (float) = `1f`
- `autosetMode` (AimMode) = `AimMode.SuppressFire`
- `logColor` (Color) = `Color.blue`
- `useAutoSetMode` (bool) = `true`

### `CombatExtended.BodyShapeDef` : Def

- `width` (float) = `1f`
- `widthLaying` (float) = `1f`
- `height` (float) = `1f`
- `heightLaying` (float) = `1f`

### `CombatExtended.CustomWeaponTraitDef` : WeaponTraitDef

- `magazineCapacityIncrease` (int)
- `magazineCapacityFactor` (float) = `1f`
- `underBarrelProps` (CompProperties_UnderBarrel)
- `verbsOverrideCE` (List<VerbPropertiesCE>)
- `verbsOverridesCE` (Dictionary<ThingDef, List<VerbPropertiesCE>>)
- `explosionOverride` (TraitExplosionDef)
- `burstShotCountMultipliers` (List<float>)
- `meleeDamageOverride` (DamageDef)
- `homingAcceleration` (float)

### `CombatExtended.GunPatcherPresetDef` : Def

- `gunStats` (VerbPropertiesCE)
- `ReloadTime` (float)
- `AmmoCapacity` (int)
- `CooldownTime` (float)
- `MassCurve` (SimpleCurve)
- `Mass` (float)
- `rangeCurve` (SimpleCurve)
- `warmupCurve` (SimpleCurve)
- `cooldownCurve` (SimpleCurve)
- `MiscOtherStats` (List<StatModifier>)
- `Bulk` (float)
- `Spread` (float)
- `Sway` (float)
- `setCaliber` (AmmoSetDef)
- `reloadOneAtATime` (bool) = `false`
- `fireModes` (CompProperties_FireModes)
- `addBipods` (bool)
- `bipodTag` (string)
- `addTags` (List<string>)
- `tags` (List<string>)
- `names` (List<string>)
- `RangeRange` (FloatRange)
- `WarmupRange` (FloatRange)
- `damageRange` (FloatRange)
- `projSpeedRange` (FloatRange)
- `DiscardDesignations` (bool)
- `specialGuns` (List<LabelGun>) = `new List<LabelGun>()`
- `DetermineCaliber` (bool)
- `CaliberRanges` (List<CaliberFloatRange>) = `new List<CaliberFloatRange>()`

### `CombatExtended.LoadoutGenericDef` : Def

- `defaultCountType` (LoadoutCountType) = `LoadoutCountType.dropExcess`
- `defaultCount` (int) = `1`
- `thingRequestGroup` (ThingRequestGroup) = `(ThingRequestGroup)3`
- `isBasic` (bool) = `false`
- `lambda` (Predicate<ThingDef>) = `> _lambda`

### `CombatExtended.ShellingResponseDef` : Def

- `worldObject` (WorldObjectDef)
- `shellingPropability` (float)
- `raidPropability` (float)
- `raidMTBDays` (float)
- `projectile` (ThingDef)
- `points` (float)
- `weight` (float) = `0.1f`
- `defaultShellingPropability` (float) = `0f`
- `defaultRaidPropability` (float) = `0f`
- `defaultRaidMTBDays` (float) = `0f`
- `retaliationShellingCooldownImpact` (FloatRange) = `new FloatRange(1f, 10f)`
- `projectiles` (List<ShellingResponsePart_Projectile>) = `new List<ShellingResponsePart_Projectile>()`
- `worldObjects` (List<ShellingResponsePart_WorldObject>) = `new List<ShellingResponsePart_WorldObject>()`

### `CombatExtended.WeaponPlatformDef` : ThingDef

- `partGraphicData` (GraphicData)
- `outlineGraphicData` (GraphicData)
- `slotTags` (List<string>)
- `HasOutline` (bool) = `> outlineGraphicData != null`
- `HasPartMat` (bool) = `> partGraphicData != null`
- `PartMat` (Material) = `> partGraphicData.Graphic.MatSingle`
- `OutlineMat` (Material) = `> outlineGraphicData.Graphic.MatSingle`
- `attachmentLinks` (List<AttachmentLink>)
- `defaultGraphicParts` (List<WeaponGraphicPart>) = `new List<WeaponGraphicPart>()`

### `CombatExtended.Lasers.Building_LaserGunDef` : ThingDef

- `beamPowerConsumption` (int) = `20`
- `supportsColors` (bool) = `false`

### `CombatExtended.Lasers.LaserBeamDefCE` : AmmoDef

- `capSize` (float) = `1f`
- `capOverlap` (float) = `0.0171875f`
- `lifetime` (int) = `30`
- `flickerFrameTime` (int) = `5`
- `impulse` (float) = `4f`
- `beamWidth` (float) = `1f`
- `shieldDamageMultiplier` (float) = `0.5f`
- `seam` (float) = `-1f`
- `causefireChance` (float) = `-1f`
- `canExplode` (bool) = `false`
- `LightningBeam` (bool) = `false`
- `LightningVariance` (float) = `3f`
- `StaticLightning` (bool) = `true`
- `ArcCount` (int) = `1`
- `decorations` (List<LaserBeamDecoration>)
- `explosionEffect` (EffecterDef)
- `hitLivingEffect` (EffecterDef)
- `beamGraphic` (ThingDef)
- `textures` (List<string>)
- `materials` (List<Material>) = `new List<Material>()`
- `IsWeakToShields` (bool) = `> shieldDamageMultiplier < 1f`

### `CombatExtended.Lasers.LaserGunDef` : ThingDef

- `barrelLength` (float) = `0.9f`
- `supportsColors` (bool) = `false`

### `CombatExtended.Lasers.SpinningLaserGunDef` : LaserGunDef

- `frames` (List<GraphicData>)
- `rotationSpeed` (float) = `1f`

## DefModExtensions (`<modExtensions>`) (33)

### `CombatExtended.ApparelDefExtension` : DefModExtension

- `isRadioPack` (bool) = `false`
- `isSoftArmor` (bool) = `false`
- `isBackpack` (bool) = `false`
- `isWebbing` (bool) = `false`

### `CombatExtended.ApparelHediffExtension` : DefModExtension

- `hediff` (HediffDef)

### `CombatExtended.ApparelLayerExtension` : DefModExtension

- `IsHeadwear` (bool)

### `CombatExtended.ApperalRenderingExtension` : DefModExtension

- `HideHair` (bool) = `true`
- `HideBeard` (bool) = `false`

### `CombatExtended.BodyPartExploderExt` : DefModExtension

- `triggerChance` (float)
- `allowedDamageDefs` (List<DamageDef>)

### `CombatExtended.DamageDefExtensionCE` : DefModExtension

- `noDamageOnDeflect` (bool) = `false`
- `harmOnlyOutsideLayers` (bool) = `false`
- `isAmbientDamage` (bool) = `false`
- `worldDamageMultiplier` (float) = `-1f`
- `canCauseWallFragmentation` (bool) = `true`
- `secondaryDamageShieldOverride` (bool) = `false`
- `shieldDamageMultiplier` (float) = `1f`

### `CombatExtended.FactionDefExtensionCE` : DefModExtension

- `shellingResponse` (ShellingResponseDef)

### `CombatExtended.FireSpreadExtension` : DefModExtension

- `baseSpreadTicks` (int)
- `minSpreadTicks` (int)
- `baseGrowthPerTick` (float)
- `spreadFarBaseChance` (float)
- `fireSizeMultiplier` (float)
- `windSpeedMultiplier` (float)
- `maxHumidity` (float)
- `humidityDecayPerTick` (float)
- `humidityIncreaseMultiplier` (float)
- `flammabilityHumidityMin` (float)
- `flammabilityHumidityHalf` (float)
- `flammabilityHumidityMax` (float)

### `CombatExtended.FlareModExtensionCE` : DefModExtension

- `SmokeMote` (ThingDef)
- `FireMote` (ThingDef)
- `LandglowFleck` (FleckDef)
- `FlareThingDef` (ThingDef)
- `BurnSeconds` (float) = `35f`
- `AltitudeDrawFactor` (float) = `0.7f`
- `FlyoverStartAltitude` (float) = `30f`
- `FlyoverFinalAltitude` (float) = `4f`
- `DirectAltitude` (float) = `0f`
- `LandglowMinAltitude` (float) = `20f`
- `LandglowScale` (float) = `4f`
- `WaterSplashMinAltitude` (float) = `6f`
- `WatersplashVelocity` (float) = `5f`
- `WatersplashSize` (float) = `2.5f`
- `SmokeMinInterval` (int) = `18`
- `SmokeMaxInterval` (int) = `25`
- `SmokeMinSize` (float) = `0.15f`
- `SmokeMaxSize` (float) = `1.25f`

### `CombatExtended.GenericLabelExtension` : DefModExtension

- `genericLabel` (string)
- `genericDescription` (string)
- `genericJobString` (string)

### `CombatExtended.GunDrawExtension` : DefModExtension

- `DrawSize` (Vector2) = `Vector2.one`
- `DrawOffset` (Vector2) = `Vector2.zero`
- `recoilModifier` (float) = `1f`
- `recoilScale` (float) = `-1f`
- `recoilTick` (int) = `-1`
- `muzzleJumpModifier` (float) = `-1f`
- `CasingOffset` (Vector2) = `Vector2.zero`
- `CasingAngleOffset` (float) = `0f`
- `DropCasingWhenReload` (bool)
- `AdvancedCasingVariables` (bool) = `false`
- `CasingAngleOffsetRange` (FloatRange) = `new FloatRange(0f, 0f)`
- `CasingLifeTimeMultiplier` (float) = `-1f`
- `CasingLifeTimeOverrideRange` (FloatRange) = `new FloatRange(-1f, 1f)`
- `CasingSpeedMultiplier` (float) = `-1f`
- `CasingSpeedOverrideRange` (FloatRange) = `new FloatRange(-1f, 1f)`
- `CasingSizeOffset` (float) = `1f`
- `CasingOffsetRandomRange` (Vector2) = `Vector2.zero`
- `CasingRotationRandomRange` (float) = `0f`

### `CombatExtended.JobDefBipodCancelExtension` : DefModExtension

### `CombatExtended.JobDefExtensionCE` : DefModExtension

- `isCrouchJob` (bool) = `false`

### `CombatExtended.LoadoutPropertiesExtension` : DefModExtension

- `primaryMagazineCount` (FloatRange) = `FloatRange.Zero`
- `primaryAttachments` (AttachmentOption)
- `minAmmoCount` (int)
- `shieldMoney` (FloatRange) = `FloatRange.Zero`
- `shieldTags` (List<string>)
- `shieldChance` (float) = `0f`
- `forcedSidearm` (SidearmOption)
- `sidearms` (List<SidearmOption>)
- `forcedAmmoCategory` (AmmoCategoryDef)
- `forceShieldMaterial` (bool)
- `shieldMaterialFilter` (ThingFilter)
- `weightedAmmoCategories` (List<WeightedAmmoCategory>)

### `CombatExtended.MedicineFilthExtension` : DefModExtension

- `filthDefName` (ThingDef)
- `filthSpawnChance` (float) = `1f`
- `filthSpawnQuantity` (IntRange) = `IntRange.One`

### `CombatExtended.ModExtensionMeleeToolPenetration` : DefModExtension

- `canHitInternal` (bool) = `false`

### `CombatExtended.ModExtension_AutoLoaderGraphics` : DefModExtension

- `fullGraphic` (GraphicData)
- `halfFullGraphic` (GraphicData)
- `emptyGraphic` (GraphicData)
- `reloadingSustainer` (SoundDef)
- `reloadCompleteSound` (SoundDef)
- `allowedTurrets` (List<string>) = `new List<string>()`
- `allowedTurretTags` (List<string>) = `new List<string>()`

### `CombatExtended.MultiBarrelExtension` : DefModExtension

- `offsets` (List<Vector2>) = `new List<Vector2>()`

### `CombatExtended.NonSnapTurretExtension` : DefModExtension

- `speed` (float) = `1f`
- `preferedAngleRange` (float) = `-1f`
- `angleWeightMultiplier` (float) = `1f`
- `minAngleWeight` (float) = `0.1f`

### `CombatExtended.OrbitalTurretExtension` : DefModExtension

- `interLayerPrecisionBonusFactor` (float) = `1f`
- `isMarkMandatory` (bool) = `false`

### `CombatExtended.PartialArmorExt` : DefModExtension

- `stats` (List<ApparelPartialStat>)

### `CombatExtended.ProjectileChangeExt` : DefModExtension

- `additionalEquipment` (List<AdditionalWeapon>)

### `CombatExtended.QualityConditionalModExt` : DefModExtension

- `UseQualityMult` (bool) = `true`

### `CombatExtended.RacePropertiesExtensionCE` : DefModExtension

- `bodyShape` (BodyShapeDef)
- `canParry` (bool) = `false`
- `maxParry` (int) = `1`
- `overrideFactors` (bool) = `false`
- `vertScale` (float) = `1f`
- `horiScale` (float) = `1f`

### `CombatExtended.ShieldDefExtension` : DefModExtension

- `shieldCoverage` (List<BodyPartGroupDef>) = `new List<BodyPartGroupDef>()`
- `crouchCoverage` (List<BodyPartGroupDef>) = `new List<BodyPartGroupDef>()`
- `drawAsTall` (bool) = `false`

### `CombatExtended.StuffToughnessMultiplierExtensionCE` : DefModExtension

- `toughnessMultiplier` (float) = `1f`

### `CombatExtended.ThingDefExtensionCE` : DefModExtension

- `MenuHidden` (bool)

### `CombatExtended.VisibleAccessoryDefExtension` : DefModExtension

- `order` (int) = `1`

### `CombatExtended.WorldObjectHealthExtension` : DefModExtension

- `destoyedInstantly` (bool) = `false`
- `healthModifier` (float) = `1f`
- `chanceToNegateDamage` (float) = `-1f`
- `techLevelNoImpact` (bool) = `false`

### `CombatExtended.WorldObjectHostilityExtension` : DefModExtension

- `AbleToRaidResponse` (bool?)
- `AbleToShellingResponse` (bool?)

### `ProjectileImpactFX.BarrelOffsetExtension` : DefModExtension

- `barrellength` (float) = `1f`
- `muzzleFlare` (bool) = `true`
- `muzzleFlareDef` (string) = `string.Empty`
- `muzzleFlareSize` (float) = `1f`
- `muzzleSmoke` (bool) = `true`
- `muzzleSmokeDef` (string) = `string.Empty`
- `muzzleSmokeSize` (float) = `0.35f`

### `ProjectileImpactFX.EffectProjectileExtension` : DefModExtension

- `AutoAssign` (bool) = `true`
- `CreateTerrainEffects` (bool) = `true`
- `explosionFleckDef` (FleckDef)
- `explosionFleckSize` (float) = `1f`
- `explosionEffecter` (EffecterDef)
- `explosionFleckSizeRange` (Nullable)
- `ImpactFleckDef` (FleckDef)
- `ImpactFleckSize` (float) = `1f`
- `ImpactFleckSizeRange` (Nullable)
- `ImpactSoundDef` (SoundDef)
- `ImpactGlowFleckDef` (FleckDef)
- `ImpactGlowFleckSize` (float) = `1f`
- `ImpactGlowFleckSizeRange` (Nullable)
- `StuckProjectileFleckDef` (FleckDef)
- `StuckProjectileFleckSize` (float) = `1f`
- `muzzleFlare` (bool) = `false`
- `muzzleFlareDef` (string) = `string.Empty`
- `muzzleFlareSize` (float) = `1f`
- `muzzleSmoke` (bool) = `false`
- `muzzleSmokeDef` (string) = `string.Empty`
- `muzzleSmokeSize` (float) = `0.35f`

### `ProjectileImpactFX.TrailerProjectileExtension` : DefModExtension

- `trailMoteDef` (string) = `"AirPuff"`
- `trailMoteSize` (float) = `0.5f`
- `trailerMoteInterval` (int) = `30`
- `motesThrown` (int) = `1`

## CompProperties de Thing (`<comps>`) (21)

### `CombatExtended.CompProperties_AbilityExplosionCE` : CompProperties_AbilityExplosion

- `ignoreSelf` (bool)
- `falloffCurveOverride` (SimpleCurve)

### `CombatExtended.CompProperties_AmmoListUser` : CompProperties_AmmoUser

- `additionalAmmoSets` (List<AmmoSetDef>) = `new List<AmmoSetDef>()`
- `ammoSpawnOptions` (List<AmmoSpawnOption>) = `new List<AmmoSpawnOption>()`

### `CombatExtended.CompProperties_AmmoResupplyOnWakeup` : CompProperties

- `dropInPods` (bool)

### `CombatExtended.CompProperties_AmmoUser` : CompProperties

- `magazineSize` (int) = `0`
- `AmmoGenPerMagOverride` (int) = `0`
- `reloadTime` (float) = `1f`
- `reloadOneAtATime` (bool) = `false`
- `throwMote` (bool) = `true`
- `ammoSet` (AmmoSetDef) = `null`
- `loadedAmmoBulkFactor` (float) = `0f`

### `CombatExtended.CompProperties_ArmorDurability` : CompProperties

- `Durability` (float)
- `Regenerates` (bool)
- `RegenInterval` (int)
- `RegenValue` (float)
- `Repairable` (bool)
- `RepairIngredients` (List<ThingDefCountClass>)
- `RepairTime` (int)
- `RepairValue` (float)
- `CanOverHeal` (bool)
- `MaxOverHeal` (float)
- `MinArmorValueSharp` (float) = `-1f`
- `MinArmorValueBlunt` (float) = `-1f`
- `MinArmorValueHeat` (float) = `-1f`
- `MinArmorValueElectric` (float) = `-1f`
- `MinArmorPct` (float) = `0.25f`

### `CombatExtended.CompProperties_BipodComp` : CompProperties

- `additionalrange` (int) = `12`
- `recoilMulton` (float) = `0.5f`
- `recoilMultoff` (float) = `1f`
- `warmupMult` (float) = `0.85f`
- `warmupPenalty` (float) = `2f`
- `swayMult` (float) = `1f`
- `swayPenalty` (float) = `1f`
- `ticksToSetUp` (int) = `60`
- `catDef` (BipodCategoryDef)

### `CombatExtended.CompProperties_CIWSImpactHandler` : CompProperties

- `HP` (float) = `0.0001f`
- `impacted` (SoundDef)
- `impactEffecter` (EffecterDef)

### `CombatExtended.CompProperties_CIWSTarget` : CompProperties

- `alwaysIntercept` (bool)

### `CombatExtended.CompProperties_Charges` : CompProperties

- `chargeSpeeds` (List<int>) = `new List<int>()`

### `CombatExtended.CompProperties_ExplosiveCE` : CompProperties

- `damageAmountBase` (float) = `-1f`
- `fragments` (List<ThingDefCountClass>) = `new List<ThingDefCountClass>()`
- `fragSpeedFactor` (float) = `1f`
- `explosiveRadius` (float) = `1.9f`
- `explosiveDamageType` (DamageDef)
- `explosionSound` (SoundDef) = `null`
- `postExplosionSpawnThingDef` (ThingDef) = `null`
- `postExplosionSpawnChance` (float) = `0f`
- `postExplosionSpawnThingCount` (int) = `1`
- `applyDamageToExplosionCellsNeighbors` (bool) = `false`
- `preExplosionSpawnThingDef` (ThingDef) = `null`
- `preExplosionSpawnChance` (float) = `0f`
- `preExplosionSpawnThingCount` (int) = `1`
- `preExplosionSpawnSingleThingDef` (ThingDef)
- `postExplosionSpawnSingleThingDef` (ThingDef)
- `damageFalloff` (bool) = `true`
- `chanceToStartFire` (float)
- `screenShakeFactor` (float)
- `postExplosionGasType` (Nullable)
- `postExplosionGasRadiusOverride` (float?)
- `postExplosionGasAmount` (int) = `255`

### `CombatExtended.CompProperties_FireArc` : CompProperties

- `spanRange` (FloatRange) = `new FloatRange(0f, 360f)`
- `maxSpanDeviation` (float) = `360f`
- `lineLength` (float) = `3f`

### `CombatExtended.CompProperties_FireModes` : CompProperties

- `aimedBurstShotCount` (int) = `-1`
- `aiUseBurstMode` (bool) = `false`
- `noSingleShot` (bool) = `false`
- `noSnapshot` (bool) = `false`
- `aiAimMode` (AimMode) = `AimMode.AimedShot`

### `CombatExtended.CompProperties_Fragments` : CompProperties

- `fragments` (List<ThingDefCountClass>) = `new List<ThingDefCountClass>()`
- `fragSpeedFactor` (float) = `1f`
- `fragShadowChance` (float) = `0.2f`
- `fragAngleRange` (FloatRange) = `new FloatRange(0.5f, 5f)`
- `fragXZAngleRange` (FloatRange) = `new FloatRange(0f, 360f)`

### `CombatExtended.CompProperties_Inventory` : CompProperties

### `CombatExtended.CompProperties_MechAmmo` : CompProperties

- `gizmoIconSetMagCount` (string)
- `gizmoIconTakeAmmoNow` (string)

### `CombatExtended.CompProperties_MilkableRenameable` : CompProperties_Milkable

- `growthLabel` (string) = `""`

### `CombatExtended.CompProperties_ProjectileFleck` : CompProperties

- `FleckDatas` (List<ProjectileFleckDataCE>)

### `CombatExtended.CompProperties_ShearableRenameable` : CompProperties_Shearable

- `growthLabel` (string) = `""`

### `CombatExtended.CompProperties_Suppressable` : CompProperties

### `CombatExtended.CompProperties_TacticalManager` : CompProperties

### `CombatExtended.CompProperties_UnderBarrel` : CompProperties

- `propsUnderBarrel` (CompProperties_AmmoUser)
- `verbPropsUnderBarrel` (VerbPropertiesCE)
- `propsFireModesUnderBarrel` (CompProperties_FireModes)
- `targetTags` (List<string>)
- `targetHighVal` (bool)
- `oneAmmoHolder` (bool) = `false`
- `requiresReload` (bool) = `false`
- `underBarrelLabel` (string)
- `standardLabel` (string)
- `underBarrelIconTexPath` (string)
- `standardIconTexPath` (string)

## CompProperties de Hediff (6)

### `CombatExtended.HediffCompProperties_Beanbag` : HediffCompProperties

- `BaseSeverityPerDamage` (float) = `0.05f`

### `CombatExtended.HediffCompProperties_FleshOnly` : HediffCompProperties

### `CombatExtended.HediffCompProperties_InfecterCE` : HediffCompProperties

- `infectionChancePerHourUntended` (float) = `0.01f`

### `CombatExtended.HediffCompProperties_Prometheum` : HediffCompProperties

### `CombatExtended.HediffCompProperties_Stabilize` : HediffCompProperties

### `CombatExtended.HediffCompProperties_Venom` : HediffCompProperties

- `VenomPerSeverity` (float)
- `MinTicks` (int)
- `MaxTicks` (int)

## PatchOperations (`Patches/*.xml`) (4)

### `CombatExtended.PatchOperationFindMod` : PatchOperation

- `modName` (string)

### `CombatExtended.PatchOperationMakeGunCECompatible` : PatchOperation

- `defName` (string)
- `texPath` (string)
- `isWeaponPlatform` (bool) = `false`
- `AllowWithRunAndGun` (bool) = `true`
- `statBases` (XmlContainer)
- `Properties` (XmlContainer)
- `AmmoUser` (XmlContainer)
- `FireModes` (XmlContainer)
- `weaponTags` (XmlContainer)
- `weaponClasses` (XmlContainer)
- `costList` (XmlContainer)
- `researchPrerequisite` (XmlContainer)
- `attachmentLinks` (XmlContainer)
- `defaultGraphicParts` (XmlContainer)

### `CombatExtended.PatchOperationSettingsConditional` : PatchOperation

- `match` (PatchOperation)
- `nomatch` (PatchOperation)
- `settingName` (string)

### `CombatExtended.PatchOperation_ConditionalGeneric` : PatchOperation

- `standard` (PatchOperation)
- `generic` (PatchOperation)

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (157)

### `CombatExtended.Alert_ColonistHasShieldAndTwoHandedWeapon` : Alert

### `CombatExtended.Alert_FactionInfluenced` : Alert

### `CombatExtended.AmmoThing` : ThingWithComps

- `AmmoDef` (AmmoDef) = `> ((Thing)this).def as AmmoDef`
- `IsCookingOff` (bool) = `> numToCookOff > 0 && CanCookOffOrDetonate()`

### `CombatExtended.Apparel_Shield` : Apparel

### `CombatExtended.Apparel_VisibleAccessory` : Apparel

### `CombatExtended.BallisticsTrajectoryWorker` : BaseTrajectoryWorker

### `CombatExtended.Bipod_Recoil_StatPart` : StatPart

### `CombatExtended.Bipod_Sway_StatPart` : StatPart

### `CombatExtended.Building_AutoloaderCE` : Building

- `CompAmmoUser` (CompAmmoUser)
- `shouldReplaceAmmo` (bool)
- `ticksToCompleteInitial` (int)
- `ticksToComplete` (int)
- `isReloading` (bool)
- `TargetTurret` (Building_Turret)
- `powerComp` (CompPowerTrader)
- `dormantComp` (CompCanBeDormant)
- `initiatableComp` (CompInitiatable)
- `mannableComp` (CompMannable)
- `graphicsExt` (ModExtension_AutoLoaderGraphics)
- `isActive` (bool) = `> TargetAmmoUser != null`
- `TargetAmmoUser` (CompAmmoUser) = `> TargetTurret.GetAmmo()`
- `shouldBeOn` (bool) = `> ShouldBeOn()`
- `manningRequiredButUnmanned` (bool) = `> mannableComp != null && !mannableComp.MannedNow`
- `powerRequiredButUnpowered` (bool) = `> powerComp != null && !powerComp.PowerOn`

### `CombatExtended.Building_CIWS_CE` : Building_Turret_MultiVerbs

- `IgnoredDefsSettings` (IEnumerable<ThingDef>) = `> ignoredDefs ?? (ignoredDefs = new List<ThingDef>())`

### `CombatExtended.Building_TurretGunCE` : Building_Turret

- `isSlow` (bool) = `false`
- `burstCooldownTicksLeft` (int)
- `burstWarmupTicksLeft` (int)
- `currentTargetInt` (LocalTargetInfo) = `LocalTargetInfo.Invalid`
- `top` (TurretTop)
- `powerComp` (CompPowerTrader)
- `dormantComp` (CompCanBeDormant)
- `initiatableComp` (CompInitiatable)
- `mannableComp` (CompMannable)
- `hackableComp` (CompHackable)
- `targetingWorldMap` (bool) = `false`
- `isReloading` (bool) = `false`
- `globalTargetInfo` (GlobalTargetInfo) = `GlobalTargetInfo.Invalid`
- `NonSnapTurretRot` (float) = `0f`
- `GunCompEq` (CompEquippable) = `> ThingCompUtility.TryGetComp<CompEquippable>(Gun)`
- `NonSnapExtension` (NonSnapTurretExtension) = `> ((Def)((Thing)this).def).GetModExtension<NonSnapTurretExte`
- `NonSnap` (bool) = `> NonSnapExtension != null`
- `CompFireArc` (CompFireArc) = `> ((ThingWithComps)this).GetComp<CompFireArc>()`
- `IsMannable` (bool) = `> mannableComp != null`
- `PlayerControlled` (bool) = `> (((Thing)this).Faction == Faction.OfPlayer || MannedByColo`
- `IsMortar` (bool) = `> ((Thing)this).def.building.IsMortar`
- `IsMortarOrProjectileFliesOverhead` (bool) = `> Projectile.projectile.flyOverhead || IsMortar`
- `MaxWorldRange` (float) = `> ProjectileProps?.shellingProps.range ?? (-1f)`
- `EmptyMagazine` (bool) = `> CompAmmo?.EmptyMagazine ?? false`
- `FullMagazine` (bool) = `> CompAmmo?.FullMagazine ?? false`
- `AutoReloadableMagazine` (bool) = `> AutoReloadableNow && CompAmmo.CurMagCount <= Mathf.CeilToI`
- `AutoReloadableNow` (bool) = `> (mannableComp == null || (!mannableComp.MannedNow && ticks`
- `Reloadable` (bool) = `> CompAmmo?.HasMagazine ?? false`
- `MannableComp` (CompMannable) = `> mannableComp`

### `CombatExtended.Building_Turret_MultiVerbs` : Building_TurretGunCE

### `CombatExtended.BulletCE` : ProjectileCE

### `CombatExtended.CE_DebugTooltipHelper` : GameComponent

### `CombatExtended.CacheClearComponent` : GameComponent

### `CombatExtended.Command_ArtilleryTarget` : Command

- `turret` (Building_TurretGunCE)
- `others` (List<Command_ArtilleryTarget>) = `null`
- `SelectedTurrets` (IEnumerable<Building_TurretGunCE>) = `> others?.Select((Command_ArtilleryTarget o) => o.turret) ??`

### `CombatExtended.Command_Reload` : Command_Action

- `compAmmo` (CompAmmoUser)

### `CombatExtended.Command_ReloadArmor` : Command_Action

- `compReloadable` (CompApparelReloadable)

### `CombatExtended.Command_SetLoadout` : Command

- `equipmentDef` (ThingDef)

### `CombatExtended.DangerTracker` : MapComponent

### `CombatExtended.Flare` : ThingWithComps

- `modExtension` (FlareModExtensionCE)
- `FlareSmokeMote` (ThingDef)
- `FlareGlowMote` (ThingDef)
- `LandglowFleck` (FleckDef)
- `BURN_TICKS` (float) = `2100f`
- `ALTITUDE_DRAW_FACTOR` (float) = `0.7f`
- `DEFAULT_FLYOVER_START_ALT` (float) = `30f`
- `DEFAULT_FLYOVER_FINAL_ALT` (float) = `4f`
- `DEFAULT_DIRECT_ALT` (float) = `0f`
- `LANDGLOW_MIN_ALTITUDE` (float) = `20f`
- `LANDGLOW_SCALE` (float) = `4f`
- `WATERSPLASH_MIN_ALTITUDE` (float) = `6f`
- `WATERSPLASH_VELOCITY` (float) = `5f`
- `WATERSPLASH_SIZE` (float) = `2.5f`
- `Progress` (float) = `> (float)(GenTicks.TicksGame - spawnTick) / BURN_TICKS`

### `CombatExtended.GenStep_Attrition` : GenStep

### `CombatExtended.GizmoAmmoStatus` : Gizmo_Slider

- `compAmmo` (CompAmmoUser)
- `prefix` (string) = `""`

### `CombatExtended.HediffComp_Beanbag` : HediffComp

- `Props` (HediffCompProperties_Beanbag) = `> base.props as HediffCompProperties_Beanbag`

### `CombatExtended.HediffComp_FleshOnly` : HediffComp

### `CombatExtended.HediffComp_InfecterCE` : HediffComp

- `Props` (HediffCompProperties_InfecterCE) = `> (HediffCompProperties_InfecterCE)(object)base.props`

### `CombatExtended.HediffComp_Prometheum` : HediffComp

### `CombatExtended.HediffComp_Stabilize` : HediffComp

- `Props` (HediffCompProperties_Stabilize) = `> base.props as HediffCompProperties_Stabilize`
- `Stabilized` (bool) = `> stabilized`

### `CombatExtended.HediffComp_Venom` : HediffComp

- `Props` (HediffCompProperties_Venom) = `> base.props as HediffCompProperties_Venom`

### `CombatExtended.HomingBulletTrajectoryWorker` : BallisticsTrajectoryWorker

### `CombatExtended.ITab_AttachmentView` : ITab

- `Weapon` (WeaponPlatform) = `> ((ITab)this).SelThing as WeaponPlatform`

### `CombatExtended.ITab_Inventory` : ITab_Pawn_Gear

### `CombatExtended.JobDriver_EquipFromInventory` : JobDriver

- `CompInventory` (CompInventory) = `> ThingCompUtility.TryGetComp<CompInventory>((Thing)(object)`

### `CombatExtended.JobDriver_GiveAmmo` : JobDriver

### `CombatExtended.JobDriver_HunkerDown` : JobDriver

### `CombatExtended.JobDriver_Hunt` : JobDriver

### `CombatExtended.JobDriver_ModifyWeapon` : JobDriver

- `EquipedWeapon` (WeaponPlatform) = `> (WeaponPlatform)(object)base.pawn.equipment.Primary`
- `AttachmentDef` (AttachmentDef) = `> (base.job as JobCE).targetThingDefs[0] as AttachmentDef`

### `CombatExtended.JobDriver_Reload` : JobDriver

### `CombatExtended.JobDriver_ReloadAutoLoader` : JobDriver

### `CombatExtended.JobDriver_ReloadTurret` : JobDriver

### `CombatExtended.JobDriver_RepairNaturalArmor` : JobDriver

### `CombatExtended.JobDriver_SetUpBipod` : JobDriver

- `Bipod` (BipodComp) = `> ThingCompUtility.TryGetComp<BipodComp>((Thing)(object)weap`

### `CombatExtended.JobDriver_Stabilize` : JobDriver

### `CombatExtended.JobDriver_TakeAmmo` : JobDriver

- `amount` (int)

### `CombatExtended.JobDriver_TakeFromOther` : JobDriver

### `CombatExtended.JobDriver_UnloadAmmo` : JobDriver

### `CombatExtended.JobDriver_UnloadYourInventory` : JobDriver

### `CombatExtended.JobDriver_WaitKnockdown` : JobDriver_Wait

### `CombatExtended.JobGiver_CheckReload` : ThinkNode_JobGiver

### `CombatExtended.JobGiver_DefenderReloadTurret` : ThinkNode_JobGiver

### `CombatExtended.JobGiver_HunkerDown` : ThinkNode_JobGiver

### `CombatExtended.JobGiver_ModifyCarriedWeapon` : ThinkNode_JobGiver

### `CombatExtended.JobGiver_ReactToSuppression` : ThinkNode_JobGiver

### `CombatExtended.JobGiver_TakeAndEquip` : ThinkNode_JobGiver

### `CombatExtended.JobGiver_UpdateLoadout` : ThinkNode_JobGiver

### `CombatExtended.LegacyTrajectoryWorker` : BaseTrajectoryWorker

### `CombatExtended.LerpedTrajectoryWorker` : BaseTrajectoryWorker

### `CombatExtended.LerpedTrajectoryWorker_ExactPosDrawing` : LerpedTrajectoryWorker

### `CombatExtended.LetterTracker` : MapComponent

### `CombatExtended.LightingTracker` : MapComponent

- `Intensity` (float) = `> intensity * (1f - Mathf.Min((float)GenTicks.TicksGame - (f`
- `Recent` (bool) = `> createdAt >= 0 && GenTicks.TicksGame - createdAt <= 500`
- `Age` (int) = `> GenTicks.TicksGame - createdAt`
- `NumGridCells` (int) = `> base.map.cellIndices.NumGridCells`
- `IsNight` (bool) = `> base.map.skyManager.CurSkyGlow < 0.5f`

### `CombatExtended.LoadoutManager` : GameComponent

### `CombatExtended.PawnColumnWorker_Loadout` : PawnColumnWorker

### `CombatExtended.PawnColumnWorker_MassBulkBar` : PawnColumnWorker

### `CombatExtended.PawnRenderNodeWorker_Backpack` : PawnRenderNodeWorker_Apparel_Body

### `CombatExtended.PawnRenderNodeWorker_Drafted` : PawnRenderNodeWorker

### `CombatExtended.PawnRenderNodeWorker_Webbing` : PawnRenderNodeWorker_Apparel_Body

### `CombatExtended.PawnRenderSubWorker_Backpack` : PawnRenderSubWorker

### `CombatExtended.PawnRenderSubWorker_Webbing` : PawnRenderSubWorker

### `CombatExtended.ProjectileCE` : ThingWithComps

- `ballisticCoefficient` (float)
- `mass` (float)
- `radius` (float)
- `gravity` (double)
- `GravityPerWidth` (float)
- `GravityPerHeight` (float)
- `fuelTicks` (int)
- `velocity` (Vector3)
- `initialSpeed` (float)
- `lerpPosition` (bool) = `true`
- `ticksToTruePosition` (int)
- `OffMapOrigin` (bool) = `false`
- `origin` (Vector2)
- `OriginIV3` (IntVec3)
- `Destination` (Vector2)
- `damageDefOverride` (DamageDef)
- `traitExplosion` (TraitExplosionDef)
- `homingAcceleration` (float)
- `equipment` (Thing)
- `equipmentDef` (ThingDef)
- `launcher` (Thing)
- `intendedTarget` (LocalTargetInfo)
- `minCollisionDistance` (float)
- `canTargetSelf` (bool)
- `castShadow` (bool) = `true`
- `logMisses` (bool) = `true`
- `globalTargetInfo` (GlobalTargetInfo) = `GlobalTargetInfo.Invalid`
- `globalSourceInfo` (GlobalTargetInfo) = `GlobalTargetInfo.Invalid`
- `landed` (bool)
- `intTicksToImpact` (int)
- `extraDamages` (List<ExtraDamage>) = `new List<ExtraDamage>()`
- `mount` (Thing)
- `AccuracyFactor` (float)
- `startingTicksToImpact` (float)
- `FlightTicks` (int) = `0`
- `intendedTargetHeight` (float) = `0f`
- `LastPos` (Vector3)
- `shotAngle` (float) = `0f`
- `shotRotation` (float) = `0f`
- `shotHeight` (float) = `0f`
- … +8 campos (ver `CombatExtended\ProjectileCE.cs`)

### `CombatExtended.ProjectileCE_BunkerBuster` : ProjectileCE_Explosive

### `CombatExtended.ProjectileCE_Bursting` : ProjectileCE

### `CombatExtended.ProjectileCE_CIWS` : ProjectileCE

### `CombatExtended.ProjectileCE_Explosive` : ProjectileCE

- `ticksToDetonation` (int)

### `CombatExtended.ProjectileCE_Flare` : ProjectileCE_Explosive

### `CombatExtended.ProjectileCE_HeightFuse` : ProjectileCE

### `CombatExtended.ProjectileCE_SpawnPawnkind` : ProjectileCE

### `CombatExtended.ProjectileCE_SpawnsThing` : ProjectileCE

### `CombatExtended.ProjectilePropertiesCE` : ProjectileProperties

- `shellingProps` (TravelingShellProperties)
- `speedGain` (float) = `0f`
- `fuelTicks` (int) = `0`
- `pelletCount` (int) = `1`
- `spreadMult` (float) = `1f`
- `secondaryDamage` (List<SecondaryDamage>) = `new List<SecondaryDamage>()`
- `damageAdjacentTiles` (bool) = `false`
- `dropsCasings` (bool) = `false`
- `casingMoteDefname` (string) = `"Fleck_EmptyCasing"`
- `casingFilthDefname` (string) = `"Filth_RifleAmmoCasings"`
- `gravityFactor` (float) = `1f`
- `isInstant` (bool) = `false`
- `damageFalloff` (bool) = `true`
- `armorPenetrationSharp` (float)
- `armorPenetrationBlunt` (float)
- `castShadow` (bool) = `true`
- `recoilMultiplier` (float) = `1f`
- `recoilOffset` (float) = `0f`
- `warmupMultiplier` (float) = `1f`
- `warmupOffset` (float) = `0f`
- `effectiveRangeMultiplier` (float) = `1f`
- `effectiveRangeOffset` (float) = `0f`
- `muzzleFlashMultiplier` (float) = `1f`
- `muzzleFlashOffset` (float) = `0f`
- `reloadTimeMultiplier` (float) = `1f`
- `suppressionFactor` (float) = `1f`
- `airborneSuppressionFactor` (float) = `1f`
- `dangerFactor` (float) = `1f`
- `ballisticCoefficient` (FloatRange) = `new FloatRange(1f, 1f)`
- `mass` (FloatRange) = `new FloatRange(1f, 1f)`
- `diameter` (FloatRange) = `new FloatRange(1f, 1f)`
- `detonateInSpace` (bool) = `true`
- `detonateMoteDef` (ThingDef)
- `detonateFleckDef` (FleckDef)
- `detonateEffectsScaleOverride` (float) = `-1f`
- `tickToTruePos` (int?)
- `genericLabelOverride` (string) = `null`
- `fuze_delay` (int) = `2`
- `HP_penetration` (bool) = `false`
- `HP_penetration_ratio` (float) = `1f`
- … +25 campos (ver `CombatExtended\ProjectilePropertiesCE.cs`)

### `CombatExtended.Projectile_FireTrail` : ProjectileCE_Explosive

### `CombatExtended.ShadowGrid` : MapComponent

### `CombatExtended.SmartRocketTrajectoryWorker` : BallisticsTrajectoryWorker

### `CombatExtended.StatPart_Attachments` : StatPart

### `CombatExtended.StatPart_Bulk` : StatPart

### `CombatExtended.StatPart_ExtendedMagazine` : StatPart

### `CombatExtended.StatPart_LoadedAmmo` : StatPart

### `CombatExtended.StatPart_NaturalArmorDurability` : StatPart

### `CombatExtended.StatPart_NonHumanParry` : StatPart

- `bodyPartGroupList` (List<BodyPartGroupDef>) = `new List<BodyPartGroupDef>()`
- `notMissingPartList` (List<BodyPartRecord>) = `new List<BodyPartRecord>()`

### `CombatExtended.StatPart_QualityConditional` : StatPart

### `CombatExtended.StatPart_StatMaxima` : StatPart_StatSelect

### `CombatExtended.StatPart_StatMinima` : StatPart_StatSelect

### `CombatExtended.StatPart_StatSelect` : StatPart

- `includeWeapons` (bool)
- `weaponStat` (StatDef) = `null`
- `apparelStat` (StatDef)
- `sumApparelsStat` (bool) = `false`
- `implantStat` (StatDef) = `null`
- `sumImplantsStat` (bool) = `true`

### `CombatExtended.StatWorker_AmmoCaliber` : StatWorker

### `CombatExtended.StatWorker_AmmoConsumedPerShotCount` : StatWorker

### `CombatExtended.StatWorker_ArmorCoverage` : StatWorker

### `CombatExtended.StatWorker_ArmorPartial` : StatWorker

### `CombatExtended.StatWorker_BipodDisplay` : StatWorker

### `CombatExtended.StatWorker_BodyPartBluntArmor` : StatWorker_BodyPartDensity

### `CombatExtended.StatWorker_BodyPartDensity` : StatWorker

### `CombatExtended.StatWorker_BodyPartSharpArmor` : StatWorker_BodyPartDensity

### `CombatExtended.StatWorker_Caliber` : StatWorker

### `CombatExtended.StatWorker_Flammability` : StatWorker

### `CombatExtended.StatWorker_LongRange` : StatWorker

### `CombatExtended.StatWorker_Magazine` : StatWorker

- `compAmmo` (CompAmmoUser)

### `CombatExtended.StatWorker_MeleeArmorPenetration` : StatWorker_MeleeStats

### `CombatExtended.StatWorker_MeleeDamage` : StatWorker_MeleeDamageBase

### `CombatExtended.StatWorker_MeleeDamageAverage` : StatWorker_MeleeDamageBase

### `CombatExtended.StatWorker_MeleeDamageBase` : StatWorker_MeleeStats

### `CombatExtended.StatWorker_MeleeStats` : StatWorker

### `CombatExtended.StatWorker_MoveSpeed` : StatWorker

### `CombatExtended.StatWorker_MuzzleFlash` : StatWorker

### `CombatExtended.StatWorker_OneHandedness` : StatWorker

### `CombatExtended.StatWorker_TicksBetweenBurstShots` : StatWorker

### `CombatExtended.StatWorker_UBGLStats` : StatWorker

### `CombatExtended.StatWorker_WeaponToughness` : StatWorker

### `CombatExtended.StatWorker_WorkSpeedGlobal` : StatWorker

### `CombatExtended.ThinkNode_ConditionalHasLoadout` : ThinkNode_Conditional

### `CombatExtended.ThinkNode_ConditionalHunkering` : ThinkNode_Conditional

### `CombatExtended.ThinkNode_ConditionalSuppressed` : ThinkNode_Conditional

### `CombatExtended.ThoughtWorker_Suppressed` : ThoughtWorker

### `CombatExtended.VerbCIWS` : VerbCIWS

### `CombatExtended.VerbCIWSProjectile` : VerbCIWS

- `Props` (new VerbProperties_CIWSProjectile) = `> ((Verb)this).verbProps as VerbProperties_CIWSProjectile`

### `CombatExtended.VerbCIWSSkyfaller` : VerbCIWS

### `CombatExtended.VerbCIWS_Comp` : VerbCIWS

### `CombatExtended.VerbCIWS_CompSkyfaller` : VerbCIWS_Comp

### `CombatExtended.VerbPropertiesCE` : VerbProperties

- `recoilPattern` (RecoilPattern) = `RecoilPattern.None`
- `ammoConsumedPerShotCount` (int) = `1`
- `recoilAmount` (float) = `0f`
- `indirectFirePenalty` (float) = `0f`
- `circularError` (float) = `0f`
- `ticksToTruePosition` (int) = `5`
- `ejectsCasings` (bool) = `true`
- `ignorePartialLoSBlocker` (bool) = `false`
- `interruptibleBurst` (bool) = `true`
- `minRangeInCover` (int) = `-1`
- `minCoverHeight` (float) = `0.35f`
- `disallowedProjectileDefs` (List<ThingDef>) = `new List<ThingDef>()`
- `useEquipmentStatValues` (bool) = `true`

### `CombatExtended.VerbProperties_CIWS` : VerbPropertiesCE

- `holdFireIcon` (string) = `"UI/Commands/HoldFire"`
- `holdFireLabel` (string) = `"HoldFire"`
- `holdFireDesc` (string)
- `ignored` (List<ThingDef>) = `new List<ThingDef>()`
- `Ignored` (IEnumerable<ThingDef>) = `> ignored`
- `AllTargets` (IEnumerable<ThingDef>) = `> allTargets ?? (allTargets = InitAllTargets())`

### `CombatExtended.VerbProperties_CIWSProjectile` : VerbProperties_CIWS

- `maximumSpeed` (float) = `80f`
- `shouldInterceptUnpredictable` (bool) = `true`

### `CombatExtended.VerbProperties_CIWSSkyfaller` : VerbProperties_CIWS

### `CombatExtended.Verb_AbilityShootCE` : Verb_ShootCE

### `CombatExtended.Verb_LaunchProjectileCE` : Verb

- `compCharges` (CompCharges) = `null`
- `compAmmo` (CompAmmoUser) = `null`
- `compFireModes` (CompFireModes) = `null`
- `compChangeable` (CompChangeableProjectile) = `null`
- `compReloadable` (CompApparelReloadable) = `null`
- `isTurretMannable` (bool) = `false`
- `VerbPropsCE` (VerbPropertiesCE) = `> base.verbProps as VerbPropertiesCE`
- `projectilePropsCE` (ProjectilePropertiesCE) = `> Projectile.projectile as ProjectilePropertiesCE`
- `ShooterPawn` (Pawn) = `> ((Verb)this).CasterPawn ?? CE_Utility.TryGetTurretOperator`
- `Shooter` (Thing) = `> (Thing)(((object)ShooterPawn) ?? ((object)base.caster))`
- `AimingAccuracy` (float) = `> Mathf.Min(StatExtension.GetStatValue(Shooter, CE_StatDefOf`
- `WeaponPlatform` (WeaponPlatform) = `> ((Verb)this).EquipmentSource as WeaponPlatform`
- `MidBurst` (bool) = `> numShotsFired > 0`

### `CombatExtended.Verb_LaunchProjectileChangeAble` : Verb_ShootCE

- `UseManager` (Dictionary<AdditionalWeapon, int>) = `null`
- `ProjectileInt` (ThingDef)
- `fireswitch` (bool)
- `burstSwitcherPair` (Pair<AdditionalWeapon, int>)

### `CombatExtended.Verb_LaunchProjectileStaticCE` : Verb_LaunchProjectileCE

### `CombatExtended.Verb_MarkForArtillery` : Verb_LaunchProjectileCE

### `CombatExtended.Verb_MeleeAttackCE` : Verb_MeleeAttack

- `ToolCE` (ToolCE) = `> ((Verb)this).tool as ToolCE`

### `CombatExtended.Verb_ShootCE` : Verb_LaunchProjectileCE

- `drawPos` (Vector3)

### `CombatExtended.Verb_ShootCEOneUse` : Verb_ShootCE

### `CombatExtended.Verb_ShootCEOneUseStatic` : Verb_ShootCEOneUse

### `CombatExtended.Verb_ShootFlareCE` : Verb_ShootMortarCE

### `CombatExtended.Verb_ShootMortarCE` : Verb_ShootCE

- `mokeTargetInfo` (LocalTargetInfo) = `LocalTargetInfo.Invalid`
- `globalTargetInfo` (GlobalTargetInfo) = `GlobalTargetInfo.Invalid`
- `globalSourceInfo` (GlobalTargetInfo) = `GlobalTargetInfo.Invalid`

### `CombatExtended.Verb_ThrowGrenade` : Verb_ShootCEOneUse

### `CombatExtended.WeaponPlatform` : ThingWithComps

- `attachments` (readonly List<AttachmentLink>) = `new List<AttachmentLink>()`
- `ConfigApplied` (bool) = `> _additionList.Count == 0 && _removalList.Count == 0`
- `RemovalList` (List<AttachmentDef>) = `> _removalList`
- `AdditionList` (List<AttachmentDef>) = `> _additionList`

### `CombatExtended.WeatherTracker` : MapComponent

- `HumidityPercent` (float) = `> Humidity / FireSpread.values.maxHumidity`

### `CombatExtended.WorkGiver_HunterHuntCE` : WorkGiver_HunterHunt

### `CombatExtended.WorkGiver_ModifyWeapon` : WorkGiver_Scanner

### `CombatExtended.WorkGiver_ReloadAutoLoader` : WorkGiver_Scanner

### `CombatExtended.WorkGiver_ReloadTurret` : WorkGiver_Scanner

### `CombatExtended.AI.IJobDriver_Tactical` : JobDriver

- `numAttacksMade` (int) = `0`

### `CombatExtended.AI.JobDriver_AttackOpportunistic` : IJobDriver_Tactical

### `CombatExtended.Lasers.Building_LaserGunCE` : Building_TurretGunCE

- `isCharged` (bool) = `false`
- `previousBurstCooldownTicksLeft` (int) = `0`
- `BurstCooldownTicksLeft` (int) = `> burstCooldownTicksLeft`
- `BurstWarmupTicksLeft` (int) = `> burstWarmupTicksLeft`

### `CombatExtended.Lasers.LaserBeamCE` : BulletCE

- `DamageModifier` (float) = `1f`
- `laserBeamDef` (LaserBeamDefCE) = `> ((Thing)this).def as LaserBeamDefCE`

### `CombatExtended.Lasers.LaserBeamGraphicCE` : Thing

- `projDef` (LaserBeamDefCE)
- `drawingMatrix` (Matrix4x4) = `default`
- `meshes` (List<Mesh>) = `new List<Mesh>()`
- `ticksToDetonation` (int)
- `Opacity` (float) = `> (float)Math.Sin(Math.Pow(1.0 - 1.0 * (double)ticks / (doub`
- `Lightning` (bool) = `> projDef.LightningBeam`
- `Static` (bool) = `> projDef.StaticLightning`

### `CombatExtended.Lasers.LaserGun` : ThingWithComps

- `laserGunDef` (LaserGunDef) = `> (((Thing)this).def as LaserGunDef) ?? LaserGunDef.defaultO`

### `CombatExtended.Lasers.SpinningLaserGun` : SpinningLaserGunBase

### `CombatExtended.Lasers.SpinningLaserGunBase` : LaserGun

- `state` (State) = `State.Idle`
- `def` (SpinningLaserGunDef) = `> ((Thing)this).def as SpinningLaserGunDef`

### `CombatExtended.Lasers.SpinningLaserGunTurret` : SpinningLaserGunBase

### `CombatExtended.Utilities.ThingsTracker` : MapComponent

### `CombatExtended.WorldObjects.WorldObjectTrackerCE` : WorldComponent

- `worldObject` (readonly WorldObject)
- `compsCE` (readonly List<IWorldCompCE>)
- `IsValid` (bool) = `> !GenList.NullOrEmpty<IWorldCompCE>((IList<IWorldCompCE>)co`

## Mod settings / ModBase (2)

### `CombatExtended.Controller` : Mod

### `CombatExtended.Settings` : ModSettings

- `ShowTutorialPopup` (bool) = `true`
- `patchArmorDamage` (bool) = `true`
- `hitRandomVehicleComponents` (bool) = `true`
- `fragmentsFromVehicles` (bool) = `false`
- `ShowCasings` (bool) = `> showCasings`
- `BipodMechanics` (bool) = `> bipodMechanics`
- `GenericAmmo` (bool) = `> genericAmmo`
- `AutoSetUp` (bool) = `> autosetup`
- `ShowTaunts` (bool) = `> showTaunts`
- `AllowMeleeHunting` (bool) = `> allowMeleeHunting`
- `SmokeEffects` (bool) = `> smokeEffects`
- `MergeExplosions` (bool) = `> mergeExplosions`
- `TurretsBreakShields` (bool) = `> turretsBreakShields`
- `ShowBackpacks` (bool) = `> showBackpacks`
- `ShowTacticalVests` (bool) = `> showTacticalVests`
- `EnableArcOfFire` (bool) = `> enableArcOfFire`
- `PartialStat` (bool) = `> partialstats`
- `EnableExtraEffects` (bool) = `> enableExtraEffects`
- `RealWeaponNames` (bool) = `> realWeaponNames`
- `RequireArtilleryMarkerForOffMapArtillery` (bool) = `> requireArtilleryMarkerForOffMapArtillery`
- `ShowExtraTooltips` (bool) = `> showExtraTooltips`
- `DetailedMeleeTooltip` (bool) = `> detailedMeleeTooltip`
- `NonPlayerMeleeTooltip` (bool) = `> nonPlayerMeleeTooltip`
- `ShowExtraStats` (bool) = `> showExtraStats`
- `EnableCIWS` (bool) = `> enableCIWS`
- `OpportunisticReloadMode` (OpportunisticReloadMode) = `> opportunisticReloadMode`
- `OpportunisticReloadSafeDistance` (float) = `> opportunisticReloadSafeDistance`
- `SecondsAfterFightToOpportunisticReload` (int) = `> secondsAfterFightToOpportunisticReload`
- `MedicineSearchRadiusSquared` (float) = `> medicineSearchRadius * medicineSearchRadius`
- `SuppressionCausesRunning` (bool) = `> suppressionCausesRunning`
- `VariedHumanHeight` (bool) = `> variedHumanHeight`
- `LogUnpatchedDefs` (bool) = `> logUnpatchedDefs`
- `EnableAmmoSystem` (bool) = `> enableAmmoSystem`
- `RightClickAmmoSelect` (bool) = `> rightClickAmmoSelect`
- `AutoReloadOnChangeAmmo` (bool) = `> autoReloadOnChangeAmmo`
- `AutoTakeAmmo` (bool) = `> autoTakeAmmo`
- `ShowCaliberOnGuns` (bool) = `> showCaliberOnGuns`
- `ReuseNeolithicProjectiles` (bool) = `> reuseNeolithicProjectiles`
- `ForbiddenNeolithicProjectiles` (bool) = `> forbiddenNeolithicProjectiles`
- `RealisticCookOff` (bool) = `> realisticCookOff`
- … +30 campos (ver `CombatExtended\Settings.cs`)

