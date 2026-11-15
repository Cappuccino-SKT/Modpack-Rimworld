# Adaptive Storage Framework (ASF)

- **packageId:** `adaptive.storage.framework` · pasta `Mods/3033901359` · DLL principal `AdaptiveStorageFramework.dll` (namespace `AdaptiveStorage`) + DLLs de PatchOperation extras
- **Quem usa:** [sbz] Neat Storage (27 móveis de armazenamento).
- **Referência gerada:** [`../api/AdaptiveStorageFramework.md`](../api/AdaptiveStorageFramework.md).

Armazenamento que **muda o gráfico conforme o conteúdo** (prateleira vazia/cheia, item desenhado em cima, cor do conteúdo), limite de itens por célula/qualidade, rótulo do conteúdo, stats aplicados aos itens guardados (ex.: refrigeração) e aba de conteúdo melhor.

## Como fazer um móvel de armazenamento

1. `ThingDef` de construção com `<thingClass>AdaptiveStorage.ThingClass</thingClass>` (herda `Building_Storage`), `building/fixedStorageSettings` e `defaultStorageSettings` normais.
2. `<modExtensions><li Class="AdaptiveStorage.Extension">` com:
   - `graphics` (lista de `GraphicsDef`), `labelFormat`, `lockStorageSettingsToStuff`;
   - `maxItemsByCell` / `maxItemsPerCellByQuality` (capacidade por célula, por qualidade);
   - `itemStatFactors`/`itemStatOffsets` (+ `ByQuality`) — stats dados aos itens guardados (ex.: `DeteriorationRate`);
   - `temperature` (`coolingOffset`, `heatingOffset`, `coolingMin`, `heatingMax`, `requiresPower`, `requiresSwitchOn`, `requiresFuel`) — "geladeira" sem C#.
3. **`AdaptiveStorage.GraphicsDef`** (um por visual): `targetDef` (o ThingDef), `graphics` (camadas `graphicDatas`, `showBaseGraphic`), `itemGraphics`, filtros de quando usar (`allowedFilter`, `forbiddenFilter`, `stuffFilter`, `requiredThingDefs`, `allowedThingCategories`, `minimumThingCount`/`maximumThingCount`, `allowedRotations`), cor pelo conteúdo (`colorOneSource`, `useDominantContentColor`), `stackBehaviour`, `contentLabelStyle` (`ContentLabelStyleDef`: `Names`, `TotalCount`, `NamesWithCount`…), `randomSelectionWeight`.

Exemplo real: `Mods/3416243474/Defs/ThingDefs_Buildings/Buildings_Art.xml` (`sbz_DisplayTableASF`: mesa com vidro por cima do item).

Ideias para o Ecos: "cofre de garantia" da Forja, vitrine de relíquias do Silêncio, geladeira de soro da Hélice (stats de deterioração + temperatura).

## PatchOperations extras (vêm junto, servem para qualquer mod)

O ASF instala DLLs pequenas com operações de patch que funcionam sem depender do ASF no código:

| Classe | Uso |
|---|---|
| `PostInheritanceOperation.Patch` (`<operation Class="...">` dentro) | Roda a operação **depois da herança** de `ParentName` — resolve o problema clássico de patch que não acha campo herdado |
| `PatchOperationSet.Operation` | "Define" um valor criando o nó se faltar |
| `PatchOperationTryAdd.Operation` | Adiciona só se não existir |
| `CopyOperation.Add` / `Set` / `TryAdd` | Copia valores de um nó para outro por xpath (`value` = origem, `destination` = destino) |
| `GeneratorOperation` / `GeneratorOperationV2` | Gera defs a partir de modelo |
| `DefNameLink`, `1ITransformable`, `SaveGameCompatibility` | Ligações entre defNames e compatibilidade de saves |

Como o ASF faz parte do modpack mas não é dependência nossa, usar essas operações só em patches dentro de pasta `IfModActive="adaptive.storage.framework"`.

```xml
<Operation Class="PostInheritanceOperation.Patch">
  <operation Class="PatchOperationAdd">
    <success>Always</success>
    <xpath>Defs/ThingDef[thingClass="AdaptiveStorage.ThingClass"]/building/fixedStorageSettings/filter[not(disallowNotEverStorable)]</xpath>
    <value><disallowNotEverStorable>true</disallowNotEverStorable></value>
  </operation>
</Operation>
```
