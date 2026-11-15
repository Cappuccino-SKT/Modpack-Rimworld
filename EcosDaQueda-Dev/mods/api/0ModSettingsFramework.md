# Referência de API: VEF - ModSettingsFramework

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (1)

### `ModSettingsFramework.ModOptionCategoryDef` : Def

- `modSettingsName` (string)
- `modPackageSettingsID` (string)
- `order` (int)
- `mods` (List<string>)

## PatchOperations (`Patches/*.xml`) (6)

### `ModSettingsFramework.PatchOperationModOption` : PatchOperationModSettings

- `defaultValue` (bool)

### `ModSettingsFramework.PatchOperationModSettings` : PatchOperation

- `order` (int)
- `category` (string)
- `mods` (List<string>)
- `id` (string)
- `label` (string)
- `tooltip` (string)
- `showTooltipAsTinyText` (bool)
- `roundToDecimalPlaces` (int) = `2`
- `modPackageSettingsID` (string)
- `modContentPack` (ModContentPack)
- `scrollHeight` (float) = `100000000f`

### `ModSettingsFramework.PatchOperationRadioButtons` : PatchOperationModSettings

- `options` (List<PatchOperationModOption>)

### `ModSettingsFramework.PatchOperationSliderFloat` : PatchOperationModSettings

- `range` (FloatRange)
- `defaultValue` (float)

### `ModSettingsFramework.PatchOperationSliderInt` : PatchOperationModSettings

- `range` (IntRange)
- `defaultValue` (int)

### `ModSettingsFramework.PatchOperationWorker` : PatchOperationModSettings

## Mod settings / ModBase (2)

### `ModSettingsFramework.ModSettingsFrameworkMod` : Mod

- `modPackOverride` (ModContentPack)
- `fakeMod` (bool)

### `ModSettingsFramework.ModSettingsFrameworkSettings` : ModSettings

