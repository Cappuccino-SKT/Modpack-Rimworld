# Referência de API: Adaptive Storage Framework

Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).
Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.

## Defs (tipos novos de Def) (2)

### `AdaptiveStorage.ContentLabelStyleDef` : Def

- `workerClass` (Type?)
- `ContentLabelWorker` (ContentLabelWorker) = `> _contentLabelWorker`

### `AdaptiveStorage.GraphicsDef` : Def

- `graphics` (List<StorageGraphic>) = `new List<StorageGraphic>()`
- `itemGraphics` (CellTable<ItemGraphic>?)
- `allowedFilter` (ThingFilter)
- `forbiddenFilter` (ThingFilter) = `new ThingFilter()`
- `buildingFilter` (ThingFilter?)
- `stuffFilter` (ThingFilter?)
- `allowedRequirement` (AllowedRequirement)
- `forbiddenRequirement` (AllowedRequirement)
- `showContainedItems` (bool) = `true`
- `useDominantContentColor` (bool)
- `onlyAllowRequiredThingDefs` (bool)
- `showBaseGraphic` (bool?)
- `colorOneSource` (ContentColorSource)
- `colorTwoSource` (ContentColorSource)
- `stackBehaviour` (StackBehaviour)
- `contentLabelStyle` (ContentLabelStyleDef?)
- `randomSelectionWeight` (int) = `1`
- `minimumThingCount` (int) = `int.MinValue`
- `minimumAllowedThingCount` (int) = `int.MinValue`
- `maximumThingCount` (int) = `int.MaxValue`
- `requiredThingDefs` (List<ThingDef>?)
- `disallowedThingDefs` (List<ThingDef>?)
- `allowedRotations` (List<Rot4>?)
- `allowedThingCategories` (List<ThingCategoryDef>?)
- `targetDef` (ThingDef?)
- `targetDefs` (List<ThingDef>) = `new List<ThingDef>()`
- `workerClass` (Type?) = `typeof(GraphicsDefSelector)`
- `ActiveLabelStyle` (ContentLabelStyleDef) = `> _activeLabelStyle`
- `Worker` (GraphicsDefSelector) = `> _worker`
- `HasEmptyStackGraphic` (bool) = `> _hasEmptyStackGraphic`
- `ForbidsNothing` (bool) = `> _forbidsNothing`

## DefModExtensions (`<modExtensions>`) (2)

### `AdaptiveStorage.Extension` : DefModExtension

- `coolingOffset` (float)
- `heatingOffset` (float)
- `coolingMin` (float) = `float.NegativeInfinity`
- `heatingMax` (float) = `float.PositiveInfinity`
- `requiresPower` (bool) = `true`
- `requiresSwitchOn` (bool) = `true`
- `requiresFuel` (bool) = `true`
- `graphics` (List<GraphicsDef>?)
- `lockStorageSettingsToStuff` (bool)
- `labelFormat` (LabelFormat)
- `itemStatFactors` (List<StatModifier>?)
- `itemStatOffsets` (List<StatModifier>?)
- `itemStatFactorsByQuality` (List<StatModifierQuality>?)
- `itemStatOffsetsByQuality` (List<StatModifierQuality>?)
- `maxItemsPerCellByQuality` (ValuesByQuality?)
- `maxItemsByCell` (CellTable<ValuesByQuality>?)
- `temperature` (TemperatureProperties?)

### `AdaptiveStorage.MinifiedExtension` : DefModExtension

- `packContents` (bool)

## Workers / classes para `workerClass`, `thingClass`, `hediffClass` (14)

### `AdaptiveStorage.Automatic` : ContentLabelWorker

### `AdaptiveStorage.ContentsITab` : ITab_ContentsBase

### `AdaptiveStorage.GroupContentsITab` : ContentsITab

### `AdaptiveStorage.Names` : ContentLabelWorker

### `AdaptiveStorage.NamesOrNameCount` : NamesOrTotalCount

### `AdaptiveStorage.NamesOrTotalCount` : Names

### `AdaptiveStorage.NamesWithCount` : Names

### `AdaptiveStorage.NamesWithCountOrTotalCount` : NamesOrTotalCount

### `AdaptiveStorage.None` : ContentLabelWorker

### `AdaptiveStorage.StatPart` : StatPart

### `AdaptiveStorage.ThingClass` : Building_Storage

- `AllGraphics` (List<GraphicsDef>?) = `> Renderer?.AllGraphics`
- `RandomSeed` (int) = `> ((Thing)this).thingIDNumber`
- `StoredThings` (ThingCollection) = `> _storedThings`
- `FreeStorageSlots` (ReadOnlyCollection<StorageCell>) = `> _storedThings.FreeStorageSlots`
- `FreeMapCells` (IEnumerable<IntVec3>) = `> _storedThings.FreeMapCells`
- `AnyFreeSlots` (bool) = `> StoredThings.CellWiseCount < CurrentSlotLimit`
- `ContentsPacked` (bool) = `> _contentsPacked`
- `TotalThingCount` (int) = `> StoredThings.TotalStackCount`
- `CellCount` (int) = `> _maxItemsByCell.Length`

### `AdaptiveStorage.TotalCount` : ContentLabelWorker

### `AdaptiveStorage.Vanilla` : ContentLabelWorker

### `AdaptiveStorage.Utility.CommandWithFloatMenu` : Command_Action

- `floatMenuOptions` (IEnumerable<FloatMenuOption>?)
- `floatMenuOptionsGetter` (Func<IEnumerable<FloatMenuOption>>)
- `floatMenuOptionsInitializer` (Func<IEnumerable<FloatMenuOption>>) = `() => Array.Empty<FloatMenuOption>()`

## Mod settings / ModBase (2)

### `AdaptiveStorage.AdaptiveStorageFrameworkMod` : Mod

### `AdaptiveStorage.AdaptiveStorageFrameworkSettings` : ModSettings

