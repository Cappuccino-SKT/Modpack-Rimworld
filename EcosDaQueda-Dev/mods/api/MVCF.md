# Referência de API: VEF - MVCF (vários verbos)

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (1)

### `MVCF.ModDef` : Def

- `ActivateFeatures` (List<string>) = `new List<string>()`
- `IgnoreThisMod` (bool)
- `Features` (FeatureOpts)
- `IgnoredFeatures` (FeatureOpts)

## CompProperties de Thing (`<comps>`) (5)

### `MVCF.Comps.CompProperties_RandomVerbs` : CompProperties

- `meleeChance` (float)

### `MVCF.Comps.CompProperties_VerbGiver` : CompProperties_VerbProps

### `MVCF.Comps.CompProperties_VerbProps` : CompProperties

- `verbProps` (List<AdditionalVerbProps>)

### `MVCF.Comps.CompProperties_VerbsFromInventory` : CompProperties_VerbProps

### `Reloading.CompProperties_Reloadable` : CompProperties

- `AmmoFilter` (ThingFilter)
- `GenerateAmmo` (List<ThingDefCountRangeClass>)
- `GenerateBackupWeapon` (bool)
- `ItemsPerShot` (int)
- `MaxShots` (int)
- `NewVerbClass` (Type)
- `PatchFirstFound` (bool)
- `ReloadSound` (SoundDef)
- `ReloadTimePerShot` (float)
- `StartLoaded` (bool) = `true`
- `VerbLabel` (string)

## CompProperties de Hediff (2)

### `MVCF.Comps.HediffCompProperties_ExtendedVerbGiver` : HediffCompProperties_VerbGiver

- `verbProps` (List<AdditionalVerbProps>)

### `Reloading.HediffCompProperties_Reloadable` : HediffCompProperties

- `AmmoFilter` (ThingFilter)
- `GenerateAmmo` (List<ThingDefCountRangeClass>)
- `GenerateBackupWeapon` (bool)
- `ItemsPerShot` (int)
- `MaxShots` (int)
- `NewVerbClass` (Type)
- `ReloadSound` (SoundDef)
- `ReloadTimePerShot` (float)
- `VerbLabel` (string)

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (32)

### `MVCF.JobGiver_ManhunterRanged` : ThinkNode_JobGiver

### `MVCF.WorldComponent_MVCF` : WorldComponent

- `TickManagers` (readonly List<WeakReference<VerbManager>>) = `new List<WeakReference<VerbManager>>()`

### `MVCF.Commands.Command_ToggleVerbUsage` : Command_Toggle

### `MVCF.Commands.Command_VerbTargetExtended` : Command_VerbTarget

- `groupedVerbs` (List<ManagedVerb>)
- `managedVerb` (ManagedVerb)
- `owner` (Thing)
- `Parts` (IEnumerable<CommandPart>) = `> parts`

### `MVCF.Comps.CompRandomVerbs` : VerbManagerComp

### `MVCF.Comps.HediffComp_ExtendedVerbGiver` : HediffComp_VerbGiver

- `Props` (HediffCompProperties_ExtendedVerbGiver) = `> ((HediffComp)this).props as HediffCompProperties_ExtendedV`

### `MVCF.Reloading.Comps.CommandPart_Reloadable` : CommandPart

- `Reloadable` (VerbComp_Reloadable)

### `MVCF.Reloading.Comps.VerbCompProperties_Reloadable` : VerbCompProperties

- `AmmoFilter` (ThingFilter)
- `GenerateAmmo` (List<ThingDefCountRangeClass>)
- `GenerateAmmoCategories` (List<ThingCategoryCountRangeClass>)
- `GenerateBackupWeapon` (bool)
- `ItemsPerShot` (int)
- `MaxShots` (int)
- `NewVerbClass` (Type)
- `ReloadSound` (SoundDef)
- `ReloadTimePerShot` (float)
- `StartLoaded` (bool) = `true`

### `MVCF.Reloading.Comps.VerbCompProperties_Reloadable_ChangeableAmmo` : VerbCompProperties_Reloadable

- `AmmoProjectileDictionary` (Dictionary<ThingDef, ThingDef>)
- `AmmoProjectiles` (List<AmmoProjectileData>)

### `MVCF.Reloading.Comps.VerbComp_Reloadable` : VerbComp

- `AutoReload` (bool)
- `ShotsRemaining` (int)
- `Props` (VerbCompProperties_Reloadable) = `> props as VerbCompProperties_Reloadable`

### `MVCF.Reloading.Comps.VerbComp_Reloadable_ChangeableAmmo` : VerbComp_Reloadable

- `LoadedAmmo` (readonly ThingOwner<Thing>) = `new ThingOwner<Thing>()`
- `NextAmmoItem` (Thing)
- `ParentHolder` (IThingHolder) = `> null`

### `MVCF.VerbComps.CommandPart_Cooldown` : CommandPart

- `Comp` (VerbComp_Cooldown)

### `MVCF.VerbComps.VerbCompProperties_Cooldown` : VerbCompProperties

- `cooldownTime` (float)

### `MVCF.VerbComps.VerbCompProperties_Draw` : VerbCompProperties

- `defaultPosition` (DrawPosition)
- `drawAsEquipment` (bool)
- `drawScale` (float) = `1f`
- `graphic` (GraphicData)
- `humanAsDefault` (bool)
- `idleRotation` (bool)
- `idleRotationAngle` (FloatRange)
- `idleRotationTicks` (IntRange)
- `onlyWhenDrafted` (bool)
- `scalings` (List<Scaling>)
- `specificPositions` (List<DrawPosition>)

### `MVCF.VerbComps.VerbCompProperties_Switch` : VerbCompProperties

- `startEnabled` (bool)
- `switchIcon` (string)
- `SwitchIcon` (Texture2D)

### `MVCF.VerbComps.VerbCompProperties_TargetEffect` : VerbCompProperties

- `targetEffects` (List<CompProperties>)

### `MVCF.VerbComps.VerbCompProperties_Turret` : VerbCompProperties_Draw

- `invisible` (bool)
- `uniqueTargets` (bool)
- `verbCompTurretIntervalTicks` (int) = `15`

### `MVCF.VerbComps.VerbComp_Cooldown` : VerbComp

- `Props` (VerbCompProperties_Cooldown) = `> props as VerbCompProperties_Cooldown`
- `CooldownPct` (float) = `> GenTicks.TicksToSeconds(cooldownTicksLeft) / Props.cooldow`

### `MVCF.VerbComps.VerbComp_Draw` : VerbComp

- `Props` (VerbCompProperties_Draw) = `> props as VerbCompProperties_Draw`

### `MVCF.VerbComps.VerbComp_ForceUse` : VerbComp

### `MVCF.VerbComps.VerbComp_Switch` : VerbComp

- `Props` (VerbCompProperties_Switch) = `> props as VerbCompProperties_Switch`

### `MVCF.VerbComps.VerbComp_TargetEffect` : VerbComp

- `Props` (VerbCompProperties_TargetEffect) = `> props as VerbCompProperties_TargetEffect`

### `MVCF.VerbComps.VerbComp_Turret` : VerbComp_Draw

- `Props` (new VerbCompProperties_Turret) = `> props as VerbCompProperties_Turret`

### `MVCF.Verbs.Verb_SmokePop` : Verb_SmokePop

### `Reloading.HediffComp_ChangeableAmmo` : HediffComp_Reloadable

### `Reloading.HediffComp_Reloadable` : HediffComp

- `Props` (HediffCompProperties_Reloadable) = `> base.props as HediffCompProperties_Reloadable`

### `Reloading.JobDriver_Reload` : JobDriver

### `Reloading.JobDriver_ReloadFromInventory` : JobDriver

### `Reloading.JobDriver_Unload` : JobDriver

### `Reloading.JobGiver_Reload` : ThinkNode_JobGiver

### `Reloading.JobGiver_ReloadFromInventory` : ThinkNode_JobGiver

### `Reloading.JobGiver_SwitchWeapon` : ThinkNode_JobGiver

## Mod settings / ModBase (2)

### `MVCF.MVCF` : Mod

### `MVCF.MVCFSettings` : ModSettings

