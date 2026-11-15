# Referência de API: Alpha Genes

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (6)

### `AlphaGenes.AnimalGeneTemplateDef` : Def

- `geneClass` (Type) = `typeof(Gene)`
- `customEffectDescriptions` (List<string>)
- `biostatCpx` (int)
- `biostatMet` (int)
- `minAgeActive` (float)
- `displayCategory` (GeneCategoryDef)
- `displayOrderOffset` (int)
- `selectionWeight` (float) = `0f`
- `maxBodySize` (float) = `999f`
- `minBodySize` (float) = `0f`
- `ability` (AbilityDef)
- `labelShortAdj` (string)
- `iconPath` (string)
- `exclusionTagPrefix` (string)

### `AlphaGenes.BlackListedAnimalsDef` : Def

- `blackListedAnimals` (List<string>)

### `AlphaGenes.BlackListedTraitsDef` : Def

- `blackListedTraits` (List<string>)

### `AlphaGenes.GenePackBlacklistDef` : Def

- `blackListedGenes` (List<GeneDef>)

### `AlphaGenes.ShamblerGeneBlacklistsDef` : Def

- `shamblerGenelists` (List<ShamblerGeneLists>)

### `AlphaGenes.WretchBlacklistDef` : Def

- `blackListedGenes` (List<GeneDef>)
- `blackListedDefNameStrings` (List<string>)

## DefModExtensions (`<modExtensions>`) (3)

### `AlphaGenes.AbilityExtension` : DefModExtension

- `isPocketPlaneAbility` (bool) = `false`

### `AlphaGenes.HealthModifierExtension` : DefModExtension

- `healthPointToAdd` (float)

### `AlphaGenes.SummoningGeneDefExtension` : DefModExtension

- `pawn` (PawnKindDef)

## CompProperties de Thing (`<comps>`) (44)

### `AlphaGenes.CompProperties_AbilityMineralCost` : CompProperties_AbilityEffect

- `mineralCost` (float)

### `AlphaGenes.CompProperties_AbilityOcularConversion` : CompProperties_AbilityEffect

- `plantList` (List<string>) = `new List<string>()`

### `AlphaGenes.CompProperties_AbilityParasiticStinger` : CompProperties_AbilityEffect

- `hediffDef` (HediffDef)
- `endogenes` (bool) = `false`

### `AlphaGenes.CompProperties_AbilityStinger` : CompProperties_AbilityEffect

- `endogenes` (bool) = `false`

### `AlphaGenes.CompProperties_AbilitySummon` : CompProperties_AbilityEffect

- `isMechanoid` (bool) = `false`

### `AlphaGenes.CompProperties_AbsorbFleshbeast` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_AdaptiveBiology` : CompProperties_AbilityEffect

- `numberOfTraits` (int) = `1`

### `AlphaGenes.CompProperties_BandwidthLoop` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_DestroyEyes` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_DestroyPocketPlane` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_Detonate` : CompProperties_AbilityEffect

- `radius` (float)
- `damageType` (DamageDef)
- `damageAmount` (int) = `-1`
- `damagePenetration` (float) = `-1f`
- `soundCreated` (SoundDef) = `null`
- `thingCreated` (ThingDef) = `null`
- `thingCreatedChance` (float) = `0f`
- `chanceToStartFire` (float) = `0f`
- `damageUser` (bool) = `true`

### `AlphaGenes.CompProperties_DevourBrain` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_GeneSyphon` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_GiveHediff` : CompProperties_AbilityEffect

- `hediffDef` (HediffDef)
- `applyToCaster` (bool) = `true`
- `applyToRadius` (bool) = `false`

### `AlphaGenes.CompProperties_InsanityBlast` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_MechConversion` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_NightlingAffinity` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_PocketPlane` : CompProperties_AbilityEffect

- `x` (int)
- `z` (int)
- `map` (MapGeneratorDef)

### `AlphaGenes.CompProperties_PsychicAbsorption` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_RandomItemSpawner` : CompProperties

- `items` (List<WeightedItems>)

### `AlphaGenes.CompProperties_RandomRaidSpawner` : CompProperties

### `AlphaGenes.CompProperties_ScorpionCounter` : CompProperties

### `AlphaGenes.CompProperties_SummonScorpion` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_TailGrapple` : CompProperties_AbilityEffect

- `maxBodySize` (float) = `2.5f`

### `AlphaGenes.CompProperties_TargetEffect_ArchoEndogenefier` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_ArchoXenogenefier` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_ChooseAndRemoveGene` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_Endogenefier` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_GenepackDisruptor` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_GenepackTweaker` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_GermlineMutator` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_RemoveGene` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_Xenogenefier` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_XenotypeInjector` : CompProperties

### `AlphaGenes.CompProperties_TargetEffect_XenotypeNullifier` : CompProperties

### `AlphaGenes.CompProperties_TargetableGenepack` : CompProperties_Targetable

### `AlphaGenes.CompProperties_TargetableHuman` : CompProperties_Targetable

### `AlphaGenes.CompProperties_TeleportItemsPocketPlane` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_TempNode` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_TempNodeBuilding` : CompProperties

### `AlphaGenes.CompProperties_TeratogenicHealing` : CompProperties_AbilityEffect

- `targetOther` (bool) = `false`

### `AlphaGenes.CompProperties_TeratogenicHediff` : CompProperties_AbilityEffect

- `hediff` (HediffDef)
- `targetOther` (bool) = `false`

### `AlphaGenes.CompProperties_TeratogenicTransfer` : CompProperties_AbilityEffect

### `AlphaGenes.CompProperties_UnstableRegeneration` : CompProperties_AbilityEffect

## CompProperties de Hediff (15)

### `AlphaGenes.HediffCompProperties_CorpseDecayer` : HediffCompProperties

- `radius` (int) = `5`
- `tickInterval` (int) = `500`
- `decayOnHitPoints` (int) = `1`
- `nutritionGained` (float) = `0.2f`
- `corpseSound` (string) = `""`
- `causeThoughtNearby` (bool) = `false`
- `radiusForThought` (int)
- `thought` (ThoughtDef)

### `AlphaGenes.HediffCompProperties_DeleteAfterTime` : HediffCompProperties

- `disappearsAfterTicks` (int)
- `revertToMechanoid` (bool) = `false`
- `justDeletePawn` (bool) = `false`

### `AlphaGenes.HediffCompProperties_Exploder` : HediffCompProperties

- `radius` (float)
- `damageType` (DamageDef)
- `damageAmount` (int) = `-1`
- `damagePenetration` (float) = `-1f`
- `soundCreated` (SoundDef) = `null`
- `thingCreated` (ThingDef) = `null`
- `thingCreatedChance` (float) = `0f`
- `chanceToStartFire` (float) = `0f`
- `damageUser` (bool) = `true`

### `AlphaGenes.HediffCompProperties_GiveHediffLungRotAlternative` : HediffCompProperties_GiveHediff

- `mtbOverRotGasExposureCurve` (SimpleCurve)
- `minSeverity` (float) = `0.5f`
- `mtbCheckDuration` (int) = `60`

### `AlphaGenes.HediffCompProperties_GiveHediffsInRangeExpanded` : HediffCompProperties

- `range` (float)
- `targetingParameters` (TargetingParameters)
- `hediff` (HediffDef)
- `mote` (ThingDef)
- `hideMoteWhenNotDrafted` (bool)
- `initialSeverity` (float) = `1f`
- `affectSameDef` (bool)

### `AlphaGenes.HediffCompProperties_LimbRegeneration` : HediffCompProperties

- `rateInTicks` (IntRange)
- `healAmount` (float) = `1f`

### `AlphaGenes.HediffCompProperties_MeatBurst` : HediffCompProperties

- `dividePawnKindOptions` (List<PawnKindDef>) = `new List<PawnKindDef>()`
- `dividePawnCount` (int)
- `dividePawnKindAdditionalForced` (List<PawnKindDef>) = `new List<PawnKindDef>()`
- `divideBloodFilthCountRange` (IntRange)

### `AlphaGenes.HediffCompProperties_NoBleedHeartAttack` : HediffCompProperties

### `AlphaGenes.HediffCompProperties_Parasites` : HediffCompProperties

- `severityToTurn` (float) = `0.9f`

### `AlphaGenes.HediffCompProperties_RaiseShambler` : HediffCompProperties

- `severityToTurn` (float) = `0.8f`

### `AlphaGenes.HediffCompProperties_RandomMutation` : HediffCompProperties

- `numberOfGenes` (int) = `1`
- `recurrent` (bool) = `false`
- `period` (int) = `60000`

### `AlphaGenes.HediffCompProperties_RecreationOnDeath` : HediffCompProperties_GiveHediff

### `AlphaGenes.HediffCompProperties_SeverityFromMinerals` : HediffCompProperties

- `severityPerHourEmpty` (float)
- `severityPerHourHemogen` (float)

### `AlphaGenes.HediffCompProperties_StageByMood` : HediffCompProperties

### `AlphaGenes.HediffCompProperties_Teratogenesis` : HediffCompProperties

- `period` (IntRange)

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (60)

### `AlphaGenes.Ability_MineralOverdrive` : Ability

- `active` (bool)

### `AlphaGenes.Ability_ReactiveArmour` : Ability

- `active` (bool)

### `AlphaGenes.BlackHydraBullet` : Bullet

### `AlphaGenes.CommandAbilityToggle` : Command_Toggle

- `ability` (Ability)
- `pawn` (Pawn)

### `AlphaGenes.DamageWorker_BansheeScream` : DamageWorker

### `AlphaGenes.DamageWorker_Freezing` : DamageWorker_Frostbite

### `AlphaGenes.DamageWorker_Noxious` : DamageWorker

### `AlphaGenes.DamageWorker_Sterilizing` : DamageWorker

### `AlphaGenes.HediffComp_CorpseDecayer` : HediffComp

- `tickCounter` (int) = `0`
- `flagOnce` (bool) = `false`
- `Props` (HediffCompProperties_CorpseDecayer) = `> (HediffCompProperties_CorpseDecayer)(object)base.props`

### `AlphaGenes.HediffComp_DeleteAfterTime` : HediffComp

- `timer` (int)
- `Props` (HediffCompProperties_DeleteAfterTime) = `> (HediffCompProperties_DeleteAfterTime)(object)base.props`

### `AlphaGenes.HediffComp_Exploder` : HediffComp

- `Props` (HediffCompProperties_Exploder) = `> (HediffCompProperties_Exploder)(object)base.props`

### `AlphaGenes.HediffComp_GiveHediffLungRotAlternative` : HediffComp

### `AlphaGenes.HediffComp_GiveHediffsInRangeExpanded` : HediffComp

- `Props` (HediffCompProperties_GiveHediffsInRangeExpanded) = `> (HediffCompProperties_GiveHediffsInRangeExpanded)(object)b`

### `AlphaGenes.HediffComp_LimbRegeneration` : HediffComp

- `tickCounter` (int) = `0`
- `rate` (int) = `60000`
- `Props` (HediffCompProperties_LimbRegeneration) = `> (HediffCompProperties_LimbRegeneration)(object)base.props`

### `AlphaGenes.HediffComp_MeatBurst` : HediffComp

- `Props` (HediffCompProperties_MeatBurst) = `> (HediffCompProperties_MeatBurst)(object)base.props`

### `AlphaGenes.HediffComp_NoBleedHeartAttack` : HediffComp

### `AlphaGenes.HediffComp_Parasites` : HediffComp

- `motherGenes` (List<GeneDef>) = `new List<GeneDef>()`
- `mother` (Pawn)
- `motherFaction` (Faction)
- `motherDef` (PawnKindDef)
- `endogenes` (bool) = `false`
- `Props` (HediffCompProperties_Parasites) = `> (HediffCompProperties_Parasites)(object)base.props`

### `AlphaGenes.HediffComp_RaiseShambler` : HediffComp

- `Props` (HediffCompProperties_RaiseShambler) = `> (HediffCompProperties_RaiseShambler)(object)base.props`

### `AlphaGenes.HediffComp_RandomMutation` : HediffComp

- `genes` (List<GeneDef>) = `new List<GeneDef>()`
- `blacklist` (List<GeneDef>) = `new List<GeneDef>()`
- `defnameStrings` (List<string>) = `new List<string>()`
- `Active` (bool) = `false`

### `AlphaGenes.HediffComp_RecreationOnDeath` : HediffComp

### `AlphaGenes.HediffComp_SeverityFromMinerals` : HediffComp

### `AlphaGenes.HediffComp_StageByMood` : HediffComp

- `Props` (HediffCompProperties_StageByMood) = `> (HediffCompProperties_StageByMood)(object)base.props`

### `AlphaGenes.HediffComp_Teratogenesis` : HediffComp

- `tickCounter` (int) = `0`
- `tickInterval` (int)

### `AlphaGenes.Hediff_MineralCraving` : HediffWithComps

### `AlphaGenes.Hediff_MineralFueled` : HediffWithComps

### `AlphaGenes.Hediff_MineralOverdrive` : HediffWithComps

- `ability` (Ability_MineralOverdrive)

### `AlphaGenes.Hediff_ReactiveArmour` : HediffWithComps

- `ability` (Ability_ReactiveArmour)

### `AlphaGenes.JobDriver_ConsumeMetal` : JobDriver

### `AlphaGenes.JobDriver_DeliverMetal` : JobDriver_HaulToCell

- `Deliveree` (Pawn) = `> ((JobDriver)this).job.targetC.Pawn`

### `AlphaGenes.JobDriver_FeedPatient` : JobDriver

- `Deliveree` (Pawn) = `> base.job.targetB.Pawn`

### `AlphaGenes.JobDriver_UseArchoEndogenefier` : JobDriver

### `AlphaGenes.JobDriver_UseArchoGeneRemover` : JobDriver

### `AlphaGenes.JobDriver_UseArchoXenogenefier` : JobDriver

### `AlphaGenes.JobDriver_UseEndogenefier` : JobDriver

### `AlphaGenes.JobDriver_UseGeneRemover` : JobDriver

### `AlphaGenes.JobDriver_UseGenepackDisruptor` : JobDriver

### `AlphaGenes.JobDriver_UseGenepackTweaker` : JobDriver

### `AlphaGenes.JobDriver_UseGermlineMutator` : JobDriver

### `AlphaGenes.JobDriver_UseXenogenefier` : JobDriver

### `AlphaGenes.JobDriver_UseXenotypeInjector` : JobDriver

### `AlphaGenes.JobDriver_UseXenotypeNullifier` : JobDriver

### `AlphaGenes.JobGiver_GetMetalicResource` : ThinkNode_JobGiver

### `AlphaGenes.MentalBreakWorker_Thalassophobia` : MentalBreakWorker

### `AlphaGenes.PawnRenderNodeWorker_OverlayFalseShambler` : PawnRenderNodeWorker_Overlay

### `AlphaGenes.PlaceWorker_ShowSpawnerRadius` : PlaceWorker

### `AlphaGenes.Projectile_Distortion` : Projectile_Liquid

### `AlphaGenes.ScorpionCounter_WorldComponent` : WorldComponent

### `AlphaGenes.ThoughtWorker_BloodsuckerDistrust` : ThoughtWorker

### `AlphaGenes.ThoughtWorker_EldritchVisage` : ThoughtWorker

### `AlphaGenes.ThoughtWorker_Mood_Volatile` : ThoughtWorker

### `AlphaGenes.ThoughtWorker_Stinky` : ThoughtWorker

### `AlphaGenes.ThoughtWorker_Thalassophobia` : ThoughtWorker

### `AlphaGenes.ThoughtWorker_UnsettlingAppearance` : ThoughtWorker

### `AlphaGenes.Web_Projectile` : Projectile

### `AlphaGenes.WorkGiver_AdministerMetal` : WorkGiver_Scanner

### `AlphaGenes.WorkGiver_DoBill_Forsaken` : WorkGiver_DoBill

### `AlphaGenes.WorkGiver_ForceEatMetal` : WorkGiver_Scanner

### `AlphaGenes.WorkGiver_HaulToGeneBank` : WorkGiver_Scanner

### `AlphaGenes.WorkGiver_HaulToGeneBankMixed` : WorkGiver_Scanner

### `AlphaGenes.WorkGiver_Warden_DeliverMetal` : WorkGiver_Warden

## Mod settings / ModBase (3)

### `AlphaGenes.AlphaGenes` : Mod

### `AlphaGenes.AlphaGenes_Mod` : Mod

### `AlphaGenes.AlphaGenes_Settings` : ModSettings

- `AG_QuestRate` (float) = `1f`
- `AG_DisableQuests` (bool) = `false`
- `AG_DisableMutationsMessage` (bool) = `false`
- `AG_DisableThalassophobiaMessage` (bool) = `false`
- `AG_ShowAllAnimalSummonGenes` (bool) = `false`
- `AG_GeneRemovalComa` (bool) = `true`
- `AG_RandomGenesToShamblers` (bool) = `true`

