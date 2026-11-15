# Referência de API: Custom Ritual Framework

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## DefModExtensions (`<modExtensions>`) (4)

### `TPRitualAttachableOutcomes.Precept_Ritual_Custom` : DefModExtension

- `coolDownDays` (int) = `20`
- `building` (string) = `""`
- `useRoom` (bool) = `true`
- `maxDistance` (int) = `18`

### `TPRitualAttachableOutcomes.RitualAttachableOutcomeEffectDef_TP_Custom` : DefModExtension

- `triggerPositivityIndex` (List<int>) = `new List<int>()`
- `cooldownDays` (int) = `20`
- `hediffToAdd` (List<string>)
- `hediffSeverity` (float)
- `bodyPart` (string)
- `hediffToRemove` (List<string>)
- `thought` (string)
- `inspiration` (string)
- `appliesTo` (List<string>)
- `appliesToRandom` (bool)
- `invertApply` (bool)
- `forEachPawn` (bool)
- `onlyPositiveOutcomes` (bool) = `true`
- `item` (string)
- `baseAmount` (IntRange)
- `amountPerPawn` (IntRange)
- `spawnNearRitual` (bool)
- `instaDrop` (bool)
- `forbid` (bool) = `true`
- `roofPunch` (bool)
- `weather` (string)
- `abilityToAdd` (string)
- `trait` (string)
- `traitDegree` (float)
- `traitForced` (bool)
- `removeTrait` (bool)
- `psylinkLevel` (int)
- `letterLabel` (string)
- `letterText` (string)
- `letterType` (string) = `"positive"`
- `randomFromNode` (bool)
- `weight` (float) = `1f`
- `incident` (string) = `""`
- `suppression` (float)
- `willReduction` (float)
- `resistanceReduction` (float)
- `certainty` (float)
- `incidentParms` (IncidentParmsCustom) = `new IncidentParmsCustom()`
- `node` (List<RitualAttachableOutcomeEffectDef_TP_Custom_Node>)
- `addInnerPawn` (bool)
- … +5 campos (ver `TPRitualAttachableOutcomes\RitualAttachableOutcomeEffectDef_TP_Custom.cs`)

### `TPRitualAttachableOutcomes.RitualAttachableOutcomeEffectDef_TP_Custom_Node` : RitualAttachableOutcomeEffectDef_TP_Custom

### `TPRitualAttachableOutcomes.TargetFilter_ModExtension` : DefModExtension

- `extraCandidates` (List<string>) = `new List<string>()`
- `missingDesc` (string) = `""`

## CompProperties de Thing (`<comps>`) (1)

### `TPRitualAttachableOutcomes.CompProperties_StartRitual_OnlyObligation` : CompProperties_AbilityStartRitual

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (12)

### `TPRitualAttachableOutcomes.JobDriver_HaulToRitualSpot` : JobDriver

### `TPRitualAttachableOutcomes.JobGiver_HaulToRitualSpot` : ThinkNode_JobGiver

- `thingDefName` (string) = `""`
- `stageId` (string) = `""`
- `maxDistance` (int) = `8`

### `TPRitualAttachableOutcomes.JobGiver_IngestThing` : ThinkNode_JobGiver

- `thingDefName` (string) = `""`
- `stageId` (string) = `""`

### `TPRitualAttachableOutcomes.JobGiver_MusicianPlayInstrument` : ThinkNode_JobGiver

- `thingDefNames` (List<string>)
- `maxDistance` (int) = `8`

### `TPRitualAttachableOutcomes.RitualBehaviorWorker_Speech_Custom` : RitualBehaviorWorker

### `TPRitualAttachableOutcomes.RitualObligationTargetWorker_AnyRitualSpotOrAltar_Extra` : RitualObligationTargetWorker_AnyRitualSpotOrAltar

### `TPRitualAttachableOutcomes.RitualObligationTargetWorker_Custom` : RitualObligationTargetFilter

### `TPRitualAttachableOutcomes.RitualObligationTargetWorker_CustomGrave` : RitualObligationTargetWorker_Custom

### `TPRitualAttachableOutcomes.RitualObligationTargetWorker_GraveWithCorpse` : RitualObligationTargetFilter

### `TPRitualAttachableOutcomes.RitualOutcomeEffectWorker_FromQuality_TP_Custom` : RitualOutcomeEffectWorker_FromQuality

### `TPRitualAttachableOutcomes.TPRitualAttachableOutcomeEffectWorker_Custom` : RitualAttachableOutcomeEffectWorker

### `TPRitualAttachableOutcomes.RitualStages.JobDriver_IngestThing` : JobDriver

