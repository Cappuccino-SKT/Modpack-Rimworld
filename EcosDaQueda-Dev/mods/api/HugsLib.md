# Referência de API: HugsLib

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (1)

### `HugsLib.UpdateFeatureDef` : Def
Describes a single update news item. Recommended to be placed in the /News folder in the root directory of the mod. Can be loaded from the /Defs folder, but any <see cref="T:HugsLib.UpdateFeatureDef" /> placed in the /News  folder will unload all <see cref="T:HugsLib.UpdateFeatureDef" />s loaded from /Defs.

- `modIdentifier` (string)
- `modNameReadable` (string)
- `titleOverride` (string)
- `assemblyVersion` (string)
- `content` (string)
- `trimWhitespace` (bool) = `true`
- `linkUrl` (string)
- `targetAudience` (UpdateFeatureTargetAudience) = `UpdateFeatureTargetAudience.ReturningPlayers`

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (1)

### `HugsLib.Core.HugsTickProxy` : Thing
Forwards ticks to the controller. Will not be saved and is never spawned.

## Mod settings / ModBase (2)

### `HugsLib.Core.HugsLibMod` : Mod
Entry point for the library. Instantiated by the game at the start of DoPlayLoad().

### `HugsLib.Settings.SettingsProxyMod` : Mod

