# Referência de API: Vanilla Psycasts Expanded

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (1)

### `VanillaPsycastsExpanded.PsycasterPathDef` : Def

- `abilities` (List<AbilityDef>)
- `abilityLevelsInOrder` (AbilityDef[][])
- `altBackground` (string)
- `altBackgroundImage` (Texture2D)
- `background` (string)
- `backgroundColor` (Color)
- `backgroundImage` (Texture2D)
- `HasAbilities` (bool)
- `height` (int)
- `lockedReason` (string)
- `MaxLevel` (int)
- `order` (int)
- `requiredBackstoriesAny` (List<BackstoryCategoryAndSlot>)
- `requiredFocus` (MeditationFocusDef)
- `requiredGene` (GeneDef)
- `requiredMeme` (MemeDef)
- `requiredMechanitor` (bool) = `false`
- `ignoreLockRestrictionsForNeurotrainers` (bool) = `true`
- `ensureLockRequirement` (bool)
- `tab` (string)
- `tooltip` (string)
- `width` (int)

## DefModExtensions (`<modExtensions>`) (3)

### `VanillaPsycastsExpanded.MeditationFocusExtension` : DefModExtension

- `icon` (string)
- `statParts` (List<StatPart_Focus>)
- `pointsOnly` (bool)
- `canBeUnlocked` (bool) = `true`

### `VanillaPsycastsExpanded.Skipmaster.AbilityExtension_Clamor` : DefModExtension

- `clamorRadius` (int)
- `clamorType` (ClamorDef)

### `VanillaPsycastsExpanded.Technomancer.PsyringExclusionExtension` : DefModExtension

- `excludedAbilities` (List<AbilityDef>)

## CompProperties de Thing (`<comps>`) (6)

### `VanillaPsycastsExpanded.CompProperties_BreakLink` : CompProperties

- `gizmoImage` (string)
- `gizmoLabel` (string)
- `gizmoDesc` (string)

### `VanillaPsycastsExpanded.CompProperties_FleckEmitter` : CompProperties

- `fleck` (FleckDef)
- `scale` (float) = `1f`
- `offset` (Vector3)
- `emissionInterval` (int) = `-1`
- `soundOnEmission` (SoundDef)
- `saveKeysPrefix` (string)

### `VanillaPsycastsExpanded.CompProperties_SetStoneColour` : CompProperties

### `VanillaPsycastsExpanded.CompProperties_UseEffect_Psytrainer` : CompProperties_UseEffectGiveAbility

### `VanillaPsycastsExpanded.Nightstalker.CompProperties_Darkness` : CompProperties

- `darknessRange` (float)

### `VanillaPsycastsExpanded.Wildspeaker.CompProperties_PlaySound` : CompProperties

- `sustainer` (SoundDef)
- `endSound` (SoundDef)

## CompProperties de Hediff (6)

### `VanillaPsycastsExpanded.HediffCompProperties_DisappearsOnDespawn` : HediffCompProperties

### `VanillaPsycastsExpanded.HediffCompProperties_DisappearsOnDowned` : HediffCompProperties

### `VanillaPsycastsExpanded.HediffCompProperties_PlaySound` : HediffCompProperties

- `sustainer` (SoundDef)
- `endSound` (SoundDef)

### `VanillaPsycastsExpanded.HediffCompProperties_SpawnFilth` : HediffCompProperties

- `filthDef` (ThingDef)
- `intervalRate` (int)
- `filthCount` (IntRange)

### `VanillaPsycastsExpanded.HediffCompProperties_SpawnMote` : HediffCompProperties

- `moteDef` (ThingDef)
- `offset` (Vector3)
- `maxScale` (float)

### `VanillaPsycastsExpanded.Graphics.HediffCompProperties_Mote` : HediffCompProperties

- `color` (Color)
- `mote` (ThingDef)

## Comps de habilidade (VEF/Royalty) (25)

### `VanillaPsycastsExpanded.AbilityExtension_AbilityOffsetPrisonerResistance` : AbilityExtension_AbilityMod

- `offset` (float)

### `VanillaPsycastsExpanded.AbilityExtension_CastPsychicSoothe` : AbilityExtension_AbilityMod

- `gender` (Gender)

### `VanillaPsycastsExpanded.AbilityExtension_Flee` : AbilityExtension_AbilityMod

- `onlyHostile` (bool) = `true`

### `VanillaPsycastsExpanded.AbilityExtension_ForceJobOnTarget` : AbilityExtension_ForceJobOnTargetBase

### `VanillaPsycastsExpanded.AbilityExtension_ForceJobOnTargetBase` : AbilityExtension_AbilityMod

- `jobDef` (JobDef)
- `durationMultiplier` (StatDef)
- `fleckOnTarget` (FleckDef)

### `VanillaPsycastsExpanded.AbilityExtension_GameCondition` : AbilityExtension_AbilityMod

- `gameCondition` (GameConditionDef)
- `durationDays` (Nullable)
- `sendLetter` (bool) = `false`

### `VanillaPsycastsExpanded.AbilityExtension_GiveInspiration` : AbilityExtension_AbilityMod

- `onlyPlayer` (bool)

### `VanillaPsycastsExpanded.AbilityExtension_GiveMentalState` : AbilityExtension_AbilityMod

- `applyToSelf` (bool)
- `clearOthers` (bool)
- `durationMultiplier` (StatDef)
- `durationScalesWithCaster` (bool)
- `stateDef` (MentalStateDef)
- `stateDefForMechs` (MentalStateDef)

### `VanillaPsycastsExpanded.AbilityExtension_GiveTrait` : AbilityExtension_AbilityMod

- `trait` (TraitDef)
- `degree` (int)

### `VanillaPsycastsExpanded.AbilityExtension_JoinFaction` : AbilityExtension_AbilityMod

### `VanillaPsycastsExpanded.AbilityExtension_MindWipe` : AbilityExtension_AbilityMod

### `VanillaPsycastsExpanded.AbilityExtension_Neuroquake` : AbilityExtension_AbilityMod

- `goodwillImpactForNeuroquake` (int)
- `goodwillImpactForBerserk` (int)
- `worldRangeTiles` (int)

### `VanillaPsycastsExpanded.AbilityExtension_Psycast` : AbilityExtension_AbilityMod

- `entropyGain` (float) = `0f`
- `entropyGainStatFactors` (List<StatModifier>) = `new List<StatModifier>()`
- `level` (int)
- `order` (int)
- `path` (PsycasterPathDef)
- `prerequisites` (List<AbilityDef>) = `new List<AbilityDef>()`
- `psychic` (bool)
- `psyfocusCost` (float) = `0f`
- `showCastBubble` (bool) = `true`
- `spaceAfter` (bool)

### `VanillaPsycastsExpanded.AbilityExtension_PsycastWordOfSerenity` : AbilityExtension_Psycast

- `exceptions` (List<MentalStateDef>)
- `psyfocusCostForExtreme` (float) = `-1f`
- `psyfocusCostForMajor` (float) = `-1f`
- `psyfocusCostForMinor` (float) = `-1f`

### `VanillaPsycastsExpanded.AbilityExtension_PsychicComa` : AbilityExtension_AbilityMod

- `hours` (float)
- `coma` (HediffDef)
- `multiplier` (StatDef)
- `ticks` (int)
- `autoApply` (bool) = `true`

### `VanillaPsycastsExpanded.AbilityExtension_SpawnSnowAround` : AbilityExtension_AbilityMod

- `radius` (float)
- `depth` (float)

### `VanillaPsycastsExpanded.AbilityExtension_SpawnTemperatureArea` : AbilityExtension_AbilityMod

- `fixedTemperature` (float)
- `fleckToSpawnInArea` (FleckDef)
- `spawnRate` (float)

### `VanillaPsycastsExpanded.AbilityExtension_TransferEntropy` : AbilityExtension_AbilityMod

- `targetReceivesEntropy` (bool) = `true`

### `VanillaPsycastsExpanded.AbilityExtension_WordOfLove` : AbilityExtension_AbilityMod

### `VanillaPsycastsExpanded.Chronopath.AbilityExtension_Age` : AbilityExtension_AbilityMod

- `casterYears` (float?) = `null`
- `targetYears` (float?) = `null`

### `VanillaPsycastsExpanded.Chronopath.AbilityExtension_Foretelling` : AbilityExtension_GiveInspiration

### `VanillaPsycastsExpanded.Chronopath.AbilityExtension_ImproveRelations` : AbilityExtension_AbilityMod

### `VanillaPsycastsExpanded.Chronopath.AbilityExtension_ReduceResistance` : AbilityExtension_AbilityMod

### `VanillaPsycastsExpanded.Empath.AbilityExtension_EnergyDump` : AbilityExtension_AbilityMod

### `VanillaPsycastsExpanded.Skipmaster.AbilityExtension_Wallraise` : AbilityExtension_AbilityMod

- `pattern` (List<IntVec2>)
- `screenShakeIntensity` (float)

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (137)

### `VanillaPsycastsExpanded.Ability_AggressiveHeatDump` : Ability

### `VanillaPsycastsExpanded.Ability_BerserkPulse` : Ability

- `targetCell` (IntVec3)

### `VanillaPsycastsExpanded.Ability_ConsumeBodies` : Ability_TargetCorpse

### `VanillaPsycastsExpanded.Ability_Corpsetalk` : Ability_TargetCorpse

### `VanillaPsycastsExpanded.Ability_DrainPsyessence` : Ability

### `VanillaPsycastsExpanded.Ability_FrostRay` : Ability

### `VanillaPsycastsExpanded.Ability_GoodwillImpact` : Ability

### `VanillaPsycastsExpanded.Ability_GroupLink` : Ability

### `VanillaPsycastsExpanded.Ability_GuardianSkipBarrier` : Ability

- `IsActive` (bool) = `> base.pawn.health.hediffSet.HasHediff(VPE_DefOf.VPE_Guardia`

### `VanillaPsycastsExpanded.Ability_IceBlock` : Ability

### `VanillaPsycastsExpanded.Ability_IceWall` : Ability

### `VanillaPsycastsExpanded.Ability_Killskip` : Ability

### `VanillaPsycastsExpanded.Ability_NeuralHeatDetonation` : Ability

### `VanillaPsycastsExpanded.Ability_ParalysisPulse` : Ability

### `VanillaPsycastsExpanded.Ability_PowerLeap` : Ability

### `VanillaPsycastsExpanded.Ability_PsychicComa` : Ability

### `VanillaPsycastsExpanded.Ability_PsychicShock` : Ability

### `VanillaPsycastsExpanded.Ability_Resurrect` : Ability_TargetCorpse

### `VanillaPsycastsExpanded.Ability_ReunionFarskip` : Ability

### `VanillaPsycastsExpanded.Ability_SnapFreeze` : Ability

- `targetCell` (IntVec3)

### `VanillaPsycastsExpanded.Ability_SpawnSkeleton` : Ability_TargetCorpse

### `VanillaPsycastsExpanded.Ability_StealVitality` : Ability

### `VanillaPsycastsExpanded.Ability_TargetCorpse` : Ability

### `VanillaPsycastsExpanded.Ability_WordOf` : Ability

### `VanillaPsycastsExpanded.Command_Ability_Psycast` : Command_Ability

### `VanillaPsycastsExpanded.DeathActionWorker_RockChunk` : DeathActionWorker

### `VanillaPsycastsExpanded.DeathActionWorker_Skeleton` : DeathActionWorker

### `VanillaPsycastsExpanded.DeathActionWorker_SlagChunk` : DeathActionWorker

### `VanillaPsycastsExpanded.GenStep_EltexMeteor` : GenStep_ScatterLumpsMineable

### `VanillaPsycastsExpanded.HediffComp_DisappearsOnDespawn` : HediffComp

### `VanillaPsycastsExpanded.HediffComp_DisappearsOnDowned` : HediffComp

### `VanillaPsycastsExpanded.HediffComp_PlaySound` : HediffComp

- `Props` (HediffCompProperties_PlaySound) = `> (HediffCompProperties_PlaySound)(object)base.props`

### `VanillaPsycastsExpanded.HediffComp_ShouldBeDestroyed` : HediffComp

### `VanillaPsycastsExpanded.HediffComp_SpawnFilth` : HediffComp

- `Props` (HediffCompProperties_SpawnFilth) = `> base.props as HediffCompProperties_SpawnFilth`

### `VanillaPsycastsExpanded.HediffComp_SpawnMote` : HediffComp

- `spawnedMote` (Mote)
- `Props` (HediffCompProperties_SpawnMote) = `> base.props as HediffCompProperties_SpawnMote`

### `VanillaPsycastsExpanded.Hediff_BlizzardSource` : Hediff_Overlay

### `VanillaPsycastsExpanded.Hediff_BodiesConsumed` : HediffWithComps

- `consumedBodies` (int)

### `VanillaPsycastsExpanded.Hediff_CorpseTalk` : HediffWithComps

- `skillXPDifferences` (Dictionary<SkillDef, int>) = `new Dictionary<SkillDef, int>()`

### `VanillaPsycastsExpanded.Hediff_Deathshield` : Hediff_Overlay

- `curAngle` (float)
- `skinColor` (Nullable)

### `VanillaPsycastsExpanded.Hediff_GroupLink` : Hediff_Overlay

- `linkedPawns` (List<Pawn>) = `new List<Pawn>()`

### `VanillaPsycastsExpanded.Hediff_GuardianSkipBarrier` : Hediff_Overshield

### `VanillaPsycastsExpanded.Hediff_Hallucination` : HediffWithComps

### `VanillaPsycastsExpanded.Hediff_IceBlock` : Hediff_Overlay

### `VanillaPsycastsExpanded.Hediff_IceShield` : Hediff_Overlay

### `VanillaPsycastsExpanded.Hediff_Liferot` : HediffWithComps

### `VanillaPsycastsExpanded.Hediff_NoMerge` : HediffWithComps

### `VanillaPsycastsExpanded.Hediff_Overlay` : Hediff_Ability

- `MatPropertyBlock` (MaterialPropertyBlock) = `new MaterialPropertyBlock()`

### `VanillaPsycastsExpanded.Hediff_Overshield` : Hediff_Overlay

### `VanillaPsycastsExpanded.Hediff_PsycastAbilities` : Hediff_Abilities

- `experience` (float)
- `maxLevelFromTitles` (int)
- `points` (int)
- `previousUnlockedPaths` (List<PsycasterPathDef>) = `new List<PsycasterPathDef>()`
- `psylink` (Hediff_Psylink)
- `psysets` (List<PsySet>) = `new List<PsySet>()`
- `unlockedMeditationFoci` (List<MeditationFocusDef>) = `new List<MeditationFocusDef>()`
- `unlockedPaths` (List<PsycasterPathDef>) = `new List<PsycasterPathDef>()`

### `VanillaPsycastsExpanded.Hediff_PsychicDrone` : Hediff_Overlay

### `VanillaPsycastsExpanded.Hediff_PsychicPain` : HediffWithComps

### `VanillaPsycastsExpanded.Hediff_RegrowLimbs` : HediffWithComps

### `VanillaPsycastsExpanded.Hediff_Thrall` : HediffWithComps

### `VanillaPsycastsExpanded.IncidentWorker_EltexMeteorite` : IncidentWorker

### `VanillaPsycastsExpanded.JobDriver_StandFreeze` : JobDriver

### `VanillaPsycastsExpanded.JobGiver_Clean` : ThinkNode_JobGiver

- `PathEndMode` (PathEndMode) = `> (PathEndMode)2`

### `VanillaPsycastsExpanded.JobGiver_Flee` : ThinkNode_JobGiver

### `VanillaPsycastsExpanded.JobGiver_Flick` : ThinkNode_JobGiver

- `PathEndMode` (PathEndMode) = `> (PathEndMode)2`

### `VanillaPsycastsExpanded.Projectile_FrostRay` : Projectile

### `VanillaPsycastsExpanded.RaidStrategyWorker_ImmediateAttack_Psycasters` : RaidStrategyWorker_ImmediateAttack

### `VanillaPsycastsExpanded.StatPart_Focus` : StatPart

- `focus` (MeditationFocusDef)

### `VanillaPsycastsExpanded.StatPart_Group` : StatPart_Focus

### `VanillaPsycastsExpanded.StatPart_NearbyFoci` : StatPart

### `VanillaPsycastsExpanded.StatPart_NearbyWealth` : StatPart_Focus

### `VanillaPsycastsExpanded.StatPart_PsycastBodiesConsumed` : StatPart

### `VanillaPsycastsExpanded.ThinkNode_Construct` : ThinkNode_Conditional

### `VanillaPsycastsExpanded.Chronopath.Ability_AdvanceSeason` : Ability

### `VanillaPsycastsExpanded.Chronopath.Ability_Finish` : Ability

### `VanillaPsycastsExpanded.Chronopath.Ability_MaturePlants` : Ability

### `VanillaPsycastsExpanded.Chronopath.Ability_Meditate` : Ability

### `VanillaPsycastsExpanded.Chronopath.Ability_TimeQuake` : Ability

### `VanillaPsycastsExpanded.Chronopath.Ability_TimeSphere` : Ability

### `VanillaPsycastsExpanded.Chronopath.ThoughtWorker_TimeQuake` : ThoughtWorker

### `VanillaPsycastsExpanded.Chronopath.TimeSphere` : Thing

- `Duration` (int)
- `Radius` (float)

### `VanillaPsycastsExpanded.Conflagrator.Ability_FireTornado` : Ability

### `VanillaPsycastsExpanded.Conflagrator.FireTornado` : ThingWithComps

- `ticksLeftToDisappear` (int) = `-1`

### `VanillaPsycastsExpanded.Conflagrator.HediffComp_FireShield` : HediffComp_Shield

### `VanillaPsycastsExpanded.Graphics.HediffComp_MoteOverHead` : HediffComp

- `Props` (HediffCompProperties_Mote) = `> base.props as HediffCompProperties_Mote`

### `VanillaPsycastsExpanded.Graphics.Projectile_Pointing` : Projectile_Explosive

### `VanillaPsycastsExpanded.Harmonist.Ability_HealthSwap` : Ability

### `VanillaPsycastsExpanded.Harmonist.Ability_HeatFocus` : Ability

### `VanillaPsycastsExpanded.Harmonist.Ability_LocationSwap` : Ability_Teleport

### `VanillaPsycastsExpanded.Harmonist.Ability_LuckTransfer` : Ability

### `VanillaPsycastsExpanded.Harmonist.Ability_RandomEvent` : Ability

### `VanillaPsycastsExpanded.Harmonist.Ability_Skillroll` : Ability

### `VanillaPsycastsExpanded.Harmonist.Ability_TransmuteItem` : Ability

### `VanillaPsycastsExpanded.Harmonist.Ability_TransmuteStone` : Ability

### `VanillaPsycastsExpanded.Harmonist.HediffComp_MindControl` : HediffComp_Ability

### `VanillaPsycastsExpanded.Nightstalker.Ability_Assassinate` : Ability

### `VanillaPsycastsExpanded.Nightstalker.Ability_TeleportDark` : Ability_Teleport

### `VanillaPsycastsExpanded.Nightstalker.Ability_WorldTeleportNight` : Ability_WorldTeleport

### `VanillaPsycastsExpanded.Nightstalker.Decoy` : ThingWithComps

- `Thing` (Thing) = `> (Thing)(object)this`
- `TargetPriorityFactor` (float) = `> float.MaxValue`

### `VanillaPsycastsExpanded.Nightstalker.HediffComp_DissapearsInLight` : HediffComp

### `VanillaPsycastsExpanded.Nightstalker.HediffComp_DissapearsOnAttack` : HediffComp

### `VanillaPsycastsExpanded.Nightstalker.Hediff_Darkvision` : HediffWithComps

### `VanillaPsycastsExpanded.Nightstalker.Hediff_ShadowFocus` : HediffWithComps

### `VanillaPsycastsExpanded.Skipmaster.Ability_Chunkskip` : Ability

### `VanillaPsycastsExpanded.Skipmaster.Ability_Skipdoor` : Ability

### `VanillaPsycastsExpanded.Skipmaster.Ability_Smokepop` : Ability

### `VanillaPsycastsExpanded.Skipmaster.Ability_Teleport` : Ability

### `VanillaPsycastsExpanded.Skipmaster.Ability_Wallraise` : Ability

- `Props` (AbilityExtension_Wallraise) = `> ((Def)base.def).GetModExtension<AbilityExtension_Wallraise`

### `VanillaPsycastsExpanded.Skipmaster.Ability_Waterskip` : Ability

### `VanillaPsycastsExpanded.Skipmaster.Ability_WorldTeleport` : Ability

### `VanillaPsycastsExpanded.Staticlord.Ability_Flashstorm` : Ability

### `VanillaPsycastsExpanded.Staticlord.Ability_Hurricane` : Ability

### `VanillaPsycastsExpanded.Staticlord.Ability_Recharge` : Ability

### `VanillaPsycastsExpanded.Staticlord.Ability_Thunderbolt` : Ability

### `VanillaPsycastsExpanded.Staticlord.Command_AbilityToggle` : Command_Ability

- `Toggle` (IAbilityToggle) = `> base.ability as IAbilityToggle`

### `VanillaPsycastsExpanded.Staticlord.HediffComp_Hurricane` : HediffComp_SeverityPerDay

### `VanillaPsycastsExpanded.Staticlord.HediffComp_Recharge` : HediffComp_Draw

### `VanillaPsycastsExpanded.Staticlord.Hediff_Vortexed` : Hediff

- `Vortex` (Vortex)

### `VanillaPsycastsExpanded.Staticlord.HurricaneMaker` : Thing

- `Pawn` (Pawn)

### `VanillaPsycastsExpanded.Staticlord.Vortex` : ThingWithComps

### `VanillaPsycastsExpanded.Technomancer.Ability_AffectMechs` : Ability

### `VanillaPsycastsExpanded.Technomancer.Ability_ConjureHeatPearls` : Ability

### `VanillaPsycastsExpanded.Technomancer.Ability_Construct_Rock` : Ability

### `VanillaPsycastsExpanded.Technomancer.Ability_Construct_Steel` : Ability

### `VanillaPsycastsExpanded.Technomancer.Ability_IncreaseQuality` : Ability

### `VanillaPsycastsExpanded.Technomancer.Ability_Mend` : Ability

### `VanillaPsycastsExpanded.Technomancer.Ability_Power` : Ability

### `VanillaPsycastsExpanded.Technomancer.Ability_Psyring` : Ability

### `VanillaPsycastsExpanded.Technomancer.Ability_ReverseEngineer` : Ability

### `VanillaPsycastsExpanded.Technomancer.DamageWorker_NeuralHeatBlast` : DamageWorker

### `VanillaPsycastsExpanded.Technomancer.HediffComp_Haywire` : HediffComp

### `VanillaPsycastsExpanded.Technomancer.HediffComp_InfinitePower` : HediffComp_Draw

### `VanillaPsycastsExpanded.Technomancer.Pawn_Construct` : Pawn

- `IsActive` (bool) = `> ((Thing)this).Spawned || CaravanUtility.GetCaravan((Thing)`
- `MinHeat` (int) = `> 20`

### `VanillaPsycastsExpanded.Technomancer.Psyring` : Apparel

- `Ability` (AbilityDef) = `> ability`
- `Added` (bool) = `> !alreadyHad`
- `Path` (PsycasterPathDef) = `> ability.Psycast().path`

### `VanillaPsycastsExpanded.UI.Command_ActionWithFloat` : Command_Action

- `floatMenuGetter` (Func<IEnumerable<FloatMenuOption>>)

### `VanillaPsycastsExpanded.UI.ITab_Pawn_Psycasts` : ITab

### `VanillaPsycastsExpanded.UI.PsychicStatusGizmo` : Gizmo

### `VanillaPsycastsExpanded.Wildspeaker.Ability_Animal` : Ability

### `VanillaPsycastsExpanded.Wildspeaker.Ability_EssenceTransfer` : Ability

### `VanillaPsycastsExpanded.Wildspeaker.Ability_ShrineShield` : Ability

### `VanillaPsycastsExpanded.Wildspeaker.Ability_SummonPack` : Ability

### `VanillaPsycastsExpanded.Wildspeaker.Hediff_AnimalShield` : Hediff_Overshield

### `VanillaPsycastsExpanded.Wildspeaker.Hediff_Essence` : HediffWithComps

- `EssenceOf` (Pawn)

### `VanillaPsycastsExpanded.Wildspeaker.JobDriver_EssenceTransfer` : JobDriver

## Mod settings / ModBase (2)

### `VanillaPsycastsExpanded.PsycastSettings` : ModSettings

- `additionalAbilityChance` (float) = `0.1f`
- `baseSpawnChance` (float) = `0.1f`
- `changeFocusGain` (bool)
- `maxLevel` (int) = `30`
- `muteSkipdoor` (bool)
- `shrink` (bool) = `true`
- `smallMode` (MultiCheckboxState) = `(MultiCheckboxState)2`
- `XPPerPercent` (float) = `1f`

### `VanillaPsycastsExpanded.PsycastsMod` : Mod

