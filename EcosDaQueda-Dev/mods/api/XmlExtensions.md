# Referência de API: XML Extensions

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (3)

### `XmlExtensions.PatchDef` : Def
A Def that contains a patch

- `parameters` (List<string>)
- `apply` (XmlContainer)
- `brackets` (string) = `"{}"`

### `XmlExtensions.SettingsMenuDef` : Def
A Def that defines a settings menu

- `tKey` (string)
- `defaultSpacing` (int)
- `settings` (List<SettingContainer>)
- `keyedActions` (List<KeyedAction>)
- `preOpenActions` (List<ActionContainer>)
- `onFrameActions` (List<ActionContainer>)
- `postCloseActions` (List<ActionContainer>)
- `modId` (string)
- `submenu` (bool)
- `tagMap` (Dictionary<string, HashSet<SettingContainer>>)
- `ticksOpen` (int)

### `XmlExtensions.SlateDef` : Def
A Def that does not do anything, exists only as a way to store information

## PatchOperations (`Patches/*.xml`) (74)

### `XmlExtensions.AggregateValues` : PatchOperationExtended

- `valueOperations` (XmlContainer)
- `apply` (XmlContainer)

### `XmlExtensions.ApplyAction` : PatchOperationValue

- `action` (ActionContainer)

### `XmlExtensions.ApplyInDocument` : PatchOperation

- `docName` (string) = `"Defs"`
- `apply` (PatchContainer)

### `XmlExtensions.ApplyPatch` : PatchOperationExtended

- `patchName` (string)
- `defName` (string)
- `arguments` (List<string>)

### `XmlExtensions.ApplyValueOperation` : PatchOperationValue

- `patchName` (string)
- `arguments` (List<string>)

### `XmlExtensions.CreateDocument` : PatchOperationExtendedPathed

- `docName` (string)
- `emptyDocument` (bool)

### `XmlExtensions.CreateSettings` : PatchOperation

- `modId` (string)
- `label` (string)
- `defaultSpacing` (int) = `2`
- `settings` (XmlContainer)
- `tKey` (string)

### `XmlExtensions.CreateVariable` : PatchOperationValue

- `value` (string) = `""`
- `value2` (string) = `""`
- `defaultValue` (string)
- `defaultValue2` (string)
- `fromXml` (bool)
- `fromXml2` (bool)
- `operation` (string) = `""`

### `XmlExtensions.CumulativeMath` : PatchOperationValue

### `XmlExtensions.DefDatabaseConditional` : DefDatabaseOperation

- `defType` (string)
- `defName` (string)
- `objPath` (string)

### `XmlExtensions.DefDatabaseCreateVariable` : DefDatabaseOperation

- `objPath` (string)

### `XmlExtensions.DefDatabaseForEach` : DefDatabaseOperation

- `objPath` (string)
- `prefixLength` (int) = `2`

### `XmlExtensions.DefDatabaseLog` : DefDatabaseOperation

### `XmlExtensions.DefDatabaseOperation` : PatchOperationValue

### `XmlExtensions.DefDatabaseOperationAdd` : DefDatabaseOperation

- `defType` (string)
- `defName` (string)
- `objPath` (string)
- `objPath2` (string)
- `value` (XmlContainer)

### `XmlExtensions.DefDatabaseOperationAddDef` : DefDatabaseOperation

- `value` (XmlContainer)

### `XmlExtensions.DefDatabaseOperationInsert` : DefDatabaseOperation

- `objPath` (string)
- `objPath2` (string)
- `value` (XmlContainer)

### `XmlExtensions.DefDatabaseOperationRemove` : DefDatabaseOperation

- `defType` (string)
- `defName` (string)
- `objPath` (string)

### `XmlExtensions.DefDatabaseOperationReplace` : DefDatabaseOperation

- `defType` (string)
- `defName` (string)
- `objPath` (string)
- `objPath2` (string)
- `value` (XmlContainer)

### `XmlExtensions.DefDatabaseOperationSortList` : DefDatabaseOperation

- `objPath` (string)
- `objPathLocal` (string) = `"*"`
- `reverse` (bool)
- `nonNumeric` (bool)

### `XmlExtensions.EvaluateBoolean` : PatchOperationValue

- `condition` (BooleanBase)

### `XmlExtensions.FindNodeInherited` : PatchOperationValue

- `defaultValue` (string)
- `xpathDef` (string)
- `xpathLocal` (string)

### `XmlExtensions.ForEach` : PatchOperationExtended

- `xpath` (string)

### `XmlExtensions.ForEachDescendant` : PatchOperationExtended

- `concreteOnly` (bool)
- `xpathParent` (string)

### `XmlExtensions.ForEachLoop` : PatchOperationExtended

- `xpath` (string)

### `XmlExtensions.ForLoop` : PatchOperationExtended

### `XmlExtensions.GetAttribute` : PatchOperationValuePathed

- `attribute` (string)

### `XmlExtensions.GetName` : PatchOperationValuePathed

### `XmlExtensions.HybridPatch` : PatchOperationSafe

- `xpathLocal` (string)

### `XmlExtensions.MergeDocument` : PatchOperation

- `docName` (string)

### `XmlExtensions.Nop` : PatchOperationExtended

### `XmlExtensions.OptionalPatch` : PatchOperationExtended

- `key` (string)
- `modId` (string)
- `defaultValue` (string)
- `keys` (List<string>)
- `defaultValues` (List<string>)
- `logic` (Logic)
- `caseTrue` (XmlContainer)
- `caseFalse` (XmlContainer)

### `XmlExtensions.OptionalPatchExternal` : PatchOperationExtended

- `ModClass` (string)

### `XmlExtensions.PatchByCase` : PatchOperationExtended

- `value` (string)
- `cases` (List<Case>)
- `defaultCase` (string)

### `XmlExtensions.PatchOperationAdd` : PatchOperationExtendedPathed

### `XmlExtensions.PatchOperationAddModExtension` : PatchOperationExtendedPathed

- `value` (XmlContainer)

### `XmlExtensions.PatchOperationAddOrReplace` : PatchOperationSafe

- `value` (XmlContainer)

### `XmlExtensions.PatchOperationAttributeAdd` : PatchOperationExtendedAttribute

- `value` (string)

### `XmlExtensions.PatchOperationAttributeRemove` : PatchOperationExtendedAttribute

### `XmlExtensions.PatchOperationAttributeSet` : PatchOperationExtendedAttribute

- `value` (string)

### `XmlExtensions.PatchOperationCopy` : PatchOperationExtendedPathed

- `paste` (string)

### `XmlExtensions.PatchOperationEditString` : PatchOperationExtendedPathed

- `substring` (string)
- `replaceWith` (string)

### `XmlExtensions.PatchOperationExtended` : PatchOperation

- `xmlDoc` (string)
- `requiresDelay` (bool)

### `XmlExtensions.PatchOperationExtendedAttribute` : PatchOperationExtendedPathed

- `attribute` (string)

### `XmlExtensions.PatchOperationExtendedPathed` : PatchOperationExtended

- `xpath` (string)
- `selectSingleNode` (bool)

### `XmlExtensions.PatchOperationInsert` : PatchOperationExtendedPathed

### `XmlExtensions.PatchOperationMath` : PatchOperationExtendedPathed

- `value` (string)
- `operation` (string)
- `fromXml` (bool)

### `XmlExtensions.PatchOperationRemove` : PatchOperationExtendedPathed

### `XmlExtensions.PatchOperationRemoveDuplicateNodes` : PatchOperationExtendedPathed

- `operation` (Operation)
- `compare` (Compare)
- `checkAttributes` (bool)

### `XmlExtensions.PatchOperationReplace` : PatchOperationExtendedPathed

### `XmlExtensions.PatchOperationSafe` : PatchOperationExtendedPathed

- `safetyDepth` (int) = `-1`
- `checkAttributes` (bool)
- `compare` (Compare)

### `XmlExtensions.PatchOperationSafeAdd` : PatchOperationSafe

- `value` (XmlContainer)

### `XmlExtensions.PatchOperationSafeAddOrReplace` : PatchOperationSafe

### `XmlExtensions.PatchOperationSafeCopy` : PatchOperationSafe

- `paste` (string)

### `XmlExtensions.PatchOperationSafeRemove` : PatchOperationExtended

- `xpath` (string)

### `XmlExtensions.PatchOperationSafeReplace` : PatchOperationSafe

### `XmlExtensions.PatchOperationSetName` : PatchOperationExtendedPathed

### `XmlExtensions.PatchOperationSortList` : PatchOperationExtendedPathed

- `xpathLocal` (string) = `"li"`
- `reverse` (bool)
- `nonNumeric` (bool)

### `XmlExtensions.PatchOperationValue` : PatchOperationExtended

- `apply` (XmlContainer)
- `storeIn` (string)
- `brackets` (string) = `"{}"`

### `XmlExtensions.PatchOperationValuePathed` : PatchOperationValue

- `xpath` (string)

### `XmlExtensions.Sequence` : PatchOperationExtended

- `apply` (XmlContainer)

### `XmlExtensions.SetScope` : PatchOperationExtendedPathed

- `apply` (XmlContainer)

### `XmlExtensions.StopwatchPause` : PatchOperationExtended

### `XmlExtensions.StopwatchResume` : PatchOperationExtended

### `XmlExtensions.StopwatchStart` : PatchOperationExtended

### `XmlExtensions.StopwatchStop` : PatchOperationExtended

### `XmlExtensions.Test` : PatchOperationExtendedPathed

### `XmlExtensions.Translate` : PatchOperationValue

- `tKey` (string)

### `XmlExtensions.TryCatch` : PatchOperationExtended

- `tryApply` (XmlContainer)
- `catchApply` (XmlContainer)

### `XmlExtensions.UseSetting` : PatchOperationValue

- `modId` (string)
- `key` (string)
- `defaultValue` (string)

### `XmlExtensions.UseSettingExternal` : PatchOperationValue

- `ModClass` (string)
- `field` (string)

### `XmlExtensions.UseSettings` : PatchOperationValue

- `modId` (string)
- `keys` (List<string>)
- `defaultValues` (List<string>)

### `XmlExtensions.UseSettingsExternal` : PatchOperationValue

- `ModClass` (string)
- `fields` (List<string>)

### `XmlExtensions.WhileLoop` : PatchOperationExtended

- `condition` (BooleanBase)
- `apply` (XmlContainer)

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (1)

### `XmlExtensions.MainButtonWorker_OpenMoreModSettings` : MainButtonWorker

- `OpenTab` (MainButtonDef) = `> Find.WindowStack.WindowOfType<MainTabWindow>()?.def`

## Mod settings / ModBase (2)

### `XmlExtensions.XmlMod` : Mod
Class containing internal data and obsolete methods (for backwards compatability).

### `XmlExtensions.XmlModBaseSettings` : ModSettings

- `dataDict` (Dictionary<string, string>)
- `mainButton` (bool)
- `showSettingsButton` (bool)
- `PinnedMods` (HashSet<string>)

