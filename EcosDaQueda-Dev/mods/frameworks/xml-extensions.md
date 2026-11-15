# XML Extensions

- **packageId:** `imranfish.xmlextensions` · pasta `Mods/2574315206` · DLL `XmlExtensions.dll`
- **Quem usa:** EPOE Forked (opções de fabricação), [FSF] Complex Jobs (liga/desliga cada tipo de trabalho), e vários mods para opções sem C#.
- **Referência gerada:** [`../api/XmlExtensions.md`](../api/XmlExtensions.md) (80 operações/classes).

Duas funções:

1. **PatchOperations novas**: condicionais, laços, variáveis, matemática, operações "seguras" que não falham, operações direto no `DefDatabase` (depois da carga).
2. **Opções do mod em XML** (`SettingsMenuDef`): caixas, sliders, botões de rádio… sem uma linha de C#. Os valores entram nos patches como `{chave}`.

Bônus para depurar: com o XML Extensions ativo, **erros de XML/patch mostram stack trace** e qual operação falhou, o que acelera muito achar patch quebrado.

## Como usar sem criar dependência obrigatória

O nosso mod não deve exigir o XML Extensions. Padrão usado pelo EPOE (`Mods/1949064302/LoadFolders.xml`): pôr tudo que usa XML Extensions numa pasta que só carrega se ele estiver ativo.

```xml
<loadFolders>
  <v1.6>
    <li>/</li>
    <li>1.6</li>
    <li IfModActive="imranfish.xmlextensions">1.6/Mods/XmlExtensions</li>
  </v1.6>
</loadFolders>
```

Dentro de `1.6/Mods/XmlExtensions/` ficam `Defs/` (o `SettingsMenuDef`) e `Patches/` (os patches que leem as opções). Sem o mod, nada daquilo carrega e o comportamento fica o padrão.

## Opções em XML — `XmlExtensions.SettingsMenuDef`

| Campo | Uso |
|---|---|
| `modId` | Identificador das opções (use o nosso packageId). Precisa ser igual em todos os patches que leem as opções |
| `label` | Nome no menu de opções |
| `settings` | Lista de controles (abaixo) |
| `defaultSpacing`, `submenu`, `tKey` | Espaço, submenu, chave de tradução |
| `preOpenActions`, `postCloseActions`, `keyedActions` | Ações ao abrir/fechar |

Controles (`<li Class="XmlExtensions.Setting.X">`): `Checkbox`, `Slider` (`min`, `max`, `decimals`), `Range` (dois valores, `key2`), `IntEntry`, `Numeric`, `Textbox`, `RadioButtons`, `DropdownButton`, `ColorEntry`/`ColorWheel`/`ColorPalette`, `Text`, `Gap`, `GapLine`, `SetColor` (cor do texto seguinte), `Section`, `Group`, `TabView`, `CollapsibleSettings`, `ToggleableSettings`, `SplitColumns`, `ScrollView`, `IfStatement` (mostra controles conforme outro valor), `Button`, `ResetSettings`, `DrawImage`, `Searchbox`.
Os controles com valor herdam `KeyedSettingContainer`: `key`, `defaultValue`, `label`, `tKey` (tradução), `tooltip`.

```xml
<XmlExtensions.SettingsMenuDef>
  <defName>EQ_Opcoes</defName>
  <modId>ecosdaqueda.core</modId>
  <label>Ecos da Queda</label>
  <settings>
    <li Class="XmlExtensions.Setting.Text"><text>Ajustes de reputação</text><font>Medium</font></li>
    <li Class="XmlExtensions.Setting.GapLine"/>
    <li Class="XmlExtensions.Setting.Checkbox">
      <key>EQ_RetaliacaoAtiva</key><label>Potências retaliam</label><defaultValue>true</defaultValue>
    </li>
    <li Class="XmlExtensions.Setting.Slider">
      <key>EQ_MultFama</key><label>Multiplicador de Fama</label>
      <min>0.25</min><max>4</max><decimals>2</decimals><defaultValue>1</defaultValue>
    </li>
  </settings>
</XmlExtensions.SettingsMenuDef>
```

O botão aparece em Opções › Opções de mods e também no botão "Mais opções de mods" que o XML Extensions adiciona (`MainButtonDef`). Mudanças valem **depois de reiniciar** (os patches rodam na carga).

## Ler a opção num patch

- **`XmlExtensions.OptionalPatch`** (liga/desliga): `modId`, `key`, `defaultValue`, `caseTrue`, `caseFalse` (ou `keys`/`defaultValues`/`logic` para várias chaves).
- **`XmlExtensions.UseSetting`** (usa o valor): `modId`, `key`, `defaultValue`, e dentro de `apply` o valor vira `{chave}`. `UseSettings` para várias.
- **`XmlExtensions.IfStatement`** + `condition` (`XmlExtensions.Boolean.Comparision` com `value1`, `value2`, `relation` = `eq`, `neq`, `sl` (<), `leq` (<=), `sg` (>), `geq` (>=) — decompilado de `Helpers.cs`; `nonNumeric`; `logic`).

```xml
<Operation Class="XmlExtensions.OptionalPatch">
  <modId>ecosdaqueda.core</modId>
  <key>EQ_RetaliacaoAtiva</key>
  <defaultValue>true</defaultValue>
  <caseFalse>
    <Operation Class="PatchOperationReplace">
      <xpath>Defs/EcosDaQueda.ReputationSettingsDef/retaliationEnabled</xpath>
      <value><retaliationEnabled>false</retaliationEnabled></value>
    </Operation>
  </caseFalse>
</Operation>

<Operation Class="XmlExtensions.UseSetting">
  <modId>ecosdaqueda.core</modId>
  <key>EQ_MultFama</key>
  <defaultValue>1</defaultValue>
  <apply>
    <Operation Class="PatchOperationReplace">
      <xpath>Defs/EcosDaQueda.ReputationSettingsDef/fameMultiplier</xpath>
      <value><fameMultiplier>{EQ_MultFama}</fameMultiplier></value>
    </Operation>
  </apply>
</Operation>
```

(Os nomes de campo do `ReputationSettingsDef` acima são ilustrativos; conferir os reais no nosso C#.)

**Em C#:** `XmlExtensions.XmlMod.getSetting(modId, key)` devolve a string do valor. Usar só com o mod ativo (`ModsConfig.IsActive("imranfish.xmlextensions")`) e por reflexão, para não criar dependência de DLL.

## PatchOperations úteis

| Operação | Para quê |
|---|---|
| `XmlExtensions.PatchOperationSafeAdd` / `SafeReplace` / `SafeAddOrReplace` / `SafeRemove` / `SafeCopy` | Igual às vanilla, mas **não falha** se o nó já existe/não existe (`compare` = `Name`/`InnerText`/`Both`, `safetyDepth`, `checkAttributes`). Ótimo para patch de compatibilidade |
| `XmlExtensions.PatchOperationAddOrReplace` | Adiciona ou substitui |
| `XmlExtensions.FindMod` | Como o `PatchOperationFindMod` vanilla, com `logic` (`or`/`and`) e `packageId` |
| `XmlExtensions.Conditional` / `ConditionalInherited` | Se o `xpath` existe (considerando herança `ParentName`) → `caseTrue`/`caseFalse` |
| `XmlExtensions.ForEach` / `ForEachDescendant` / `ForLoop` / `WhileLoop` | Laços sobre nós (ex.: "para cada arma com tag X, adicione a tag Y") |
| `XmlExtensions.CreateVariable` / `AggregateValues` / `CumulativeMath` / `PatchOperationMath` | Variáveis e contas (ex.: multiplicar todos os `MarketValue` por 1.2) |
| `XmlExtensions.PatchOperationEditString` | Editar texto (regex) |
| `XmlExtensions.PatchOperationAddModExtension` | Adiciona `modExtensions` criando o nó se faltar |
| `XmlExtensions.PatchOperationSortList`, `PatchOperationRemoveDuplicateNodes` | Organizar listas |
| `XmlExtensions.TryCatch`, `Test`, `Nop`, `Stopwatch*` | Depuração e testes de patch |
| `XmlExtensions.DefDatabaseOperation*` (`Add`, `Remove`, `Replace`, `Insert`, `ForEach`, `Conditional`, `Log`) | Mexem nos defs **já carregados** (depois de herança e de todos os patches), por caminho de campo — resolve casos em que o XML não basta |
| `XmlExtensions.PatchByCase`, `ApplyPatch` + `XmlExtensions.PatchDef` | Patches reutilizáveis com parâmetros |

Exemplo de laço (adicionar tag de arma a todas as armas de uma família):

```xml
<Operation Class="XmlExtensions.ForEach">
  <xpath>Defs/ThingDef[weaponTags/li="LaserGun"]</xpath>
  <storeIn>arma</storeIn>
  <apply>
    <Operation Class="XmlExtensions.PatchOperationSafeAdd">
      <xpath>{arma}/weaponTags</xpath>
      <value><li>EQ_ArmaDaForja</li></value>
    </Operation>
  </apply>
</Operation>
```

`storeIn` (padrão `DEF`) é o nome da variável e `brackets` (padrão `{}`) o delimitador — confirmado em `ForEach.cs`. A documentação completa do autor está no Steam/GitHub do mod.

## Armadilhas

- `modId` do `SettingsMenuDef` e dos patches **precisa ser idêntico**.
- Valores chegam como texto: em `Boolean.Comparision` com números, não marque `nonNumeric`.
- Os patches rodam uma vez, na carga: mudar a opção sem reiniciar não muda nada.
- Ordem de carga: o XML Extensions já fica cedo na lista (posição 13); nossos patches que usam ele rodam depois, sem problema.
