# Referência de API: VEF - Outposts (postos avançados)

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## DefModExtensions (`<modExtensions>`) (2)

### `Outposts.OutpostExtension` : DefModExtension

- `AllowedBiomes` (List<BiomeDef>)
- `CostToMake` (List<ThingDefCountClass>)
- `DisallowedBiomes` (List<BiomeDef>)
- `DisplaySkills` (List<SkillDef>)
- `Event` (HistoryEventDef)
- `MinPawns` (int)
- `ProvidedFood` (ThingDef)
- `Range` (int) = `-1`
- `RequiredSkills` (List<AmountBySkill>)
- `RequiresGrowing` (bool)
- `ResultOptions` (List<ResultOption>)
- `TicksPerProduction` (int) = `900000`
- `TicksToPack` (int) = `420000`
- `TicksToSetUp` (int) = `-1`
- `RelevantSkills` (List<SkillDef>) = `> new HashSet<SkillDef>(RequiredSkills.SelectOrEmpty((Amount`

### `Outposts.OutpostExtension_Choose` : OutpostExtension

- `ChooseDesc` (string)
- `ChooseLabel` (string)

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (1)

### `Outposts.JobGiver_DropAll` : ThinkNode_JobGiver

## Mod settings / ModBase (2)

### `Outposts.OutpostsMod` : Mod

### `Outposts.OutpostsSettings` : ModSettings

