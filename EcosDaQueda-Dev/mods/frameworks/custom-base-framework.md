# Custom Base Framework (CBF)

- **packageId:** `EvilEyeGuy.CustomBaseFramework` · pasta `Mods/3813689040` · DLLs `CustomBaseFramework.dll`, `CustomBaseFrameworkEditor.dll` (código com nomes em alemão: `Fraktionsuebernahme`, `Stadtteil`…)
- **Guia oficial offline (muito completo):** `Mods/3813689040/Wiki/Open Guide.html` (páginas em `Wiki/pages/`, ex.: `Modding-Overview.html`, `Modding-Settlements.html`, `Factions.html`, `Field-Reference.html`, `SettlementPlanDef.html`, `PieceDef.html`).
- **Referência gerada:** [`../api/CustomBaseFramework.md`](../api/CustomBaseFramework.md) (62 classes).

Gera **assentamentos de NPC** a partir de prédios desenhados à mão ("peças"), arrumados no terreno real do mapa com muralhas, ruas, campos, currais, defensores e saque. Também gera ruínas, esconderijos e masmorras de vários níveis. Traz conteúdo para o jogo base, VFE Medieval 2 e Medieval Overhaul, e um **editor no jogo**.

## ⚠ Afeta as nossas potências

Decompilado de `Fraktionsuebernahme` + guia `Factions.html`: **toda facção que recebe pelo menos um plano tem as bases geradas pelo CBF** (sem patch). Um plano chega a uma facção:

1. **Pelo nome:** o plano cita a facção (ou a categoria) em `factionTags`; ou
2. **Pelo nível tecnológico:** o plano tem `forAllFactions = true` e a `techLevel` da facção está em `techLevels`.

Uma facção com plano próprio pelo nome **nunca** recebe os planos genéricos por nível. `excludeFactions` tira uma facção de um plano. Facção sem plano nenhum fica com a base vanilla. O jogador pode desligar o CBF por facção nas opções do mod.

As nossas quatro potências são industriais → hoje recebem os planos modernos genéricos do CBF. Para ter bases temáticas (biblioteca-bunker do Silêncio, laboratório da Hélice, forja-fábrica, catedral do Coro): escrever `SettlementPlanDef` próprios com `factionTags` = nosso `FactionDef`, num arquivo com `MayRequire="EvilEyeGuy.CustomBaseFramework"` (ou pasta `IfModActive`). Isso convive com o KCSG/`FactionDefExtension` do VEF: se uma `FactionDef` usar os dois, o CBF ganha (ele troca o `MapGeneratorDef` da base).

## Tipos de Def

| Def | Papel |
|---|---|
| `SettlementPlanDef` | O "programa" do assentamento: alas (`wards`), muralhas, portões, distritos, orçamento de prédios, defensores, facções/biomas/terrenos aceitos |
| `PieceDef` | Um prédio desenhado (grades `terrain`, `things`, `roof`, `legend` de símbolos, `placement`) |
| `PieceCategoryDef` | Grupo de peças intercambiáveis (ex.: todas as tavernas), com `weight`, `factions`, `withIdeo` |
| `PawnListDef` | Lista ponderada de PawnKinds (defensores, civis) |
| `LootTableDef`, `LootBuildingCategoryDef` | Saque por prédio |
| `MaterialListDef` | Materiais por papel (parede, piso…) — sem ela o plano fica desligado |
| `StyleDef`, `WallProfileDef`, `FloorListDef`, `CropListDef`, `EaveFamilyDef`, `FactionFlagDef` | Visual |
| `RuinDef`, `DungeonProfileDef`, `LandmarkPlanDef`, `MapStepsDef`, `UnitSwapDef`, `BuildableRuleDef`, `HelpDef` | Ruínas, masmorras, marcos, passos de mapa, troca de unidades |
| Extensões: `CBFSiteExtension` (`plan`, `planTags`, `ruin` num `SitePartDef`), `CBFOccupierExtension`, `CBFRewardExtension`, `CBFForeignFactionExtension`, `LootContainerExtension`, `UndeadExtension` | Ganchos em defs de outros sistemas |
| `QuestNode_CBFAttack`, `_CBFSite`, `_CBFOccupier`, `_CBFClient`, `_CBFChoice` | Nós de missão para usar CBF em missões |

## Campos principais do `SettlementPlanDef`

- **Quem usa:** `factionTags`, `excludeFactions`, `techLevels`, `forAllFactions`, `withIdeo`/`withoutIdeo` (memes), `biomes`/`notBiomes`, `hilliness`, `landforms`, `siteTags`, `requiredMods`, `share` (peso).
- **Forma:** `kind` (`MedievalTown` padrão, `ModernTown`, camp…), `mapSize` (≥150, padrão 175), `coreSize`, `wards` (alas: `Town`, `Suburb`, `Countryside`, cada uma com `districts` e `entries` `{tag, count, filler}`), `walls`, `gatehouses`, `emplacements`, `roadMain`/`roadLane`, `harbor`, `coreShapes`.
- **Gente:** `defenderPoints` (padrão 180–420), `maxSoldiers`, `maxCivilians`, `defendersOnWall`, `defenderGroupKind`, `vehicles` (0–3: usa o Vehicle Framework!).
- **Visual/saque:** `materialList` (obrigatório), `styles`, `ideoStyle`, `wealthFactor`, `worldTexture` (ícone em `Textures/World/WorldObjects/CBF/`).

Exemplo mínimo (do guia `Modding-Settlements.html`, adaptado):

```xml
<CustomBaseFramework.SettlementPlanDef MayRequire="EvilEyeGuy.CustomBaseFramework">
  <defName>EQ_Plan_ForjaFabrica</defName>
  <label>forja-fábrica</label>
  <kind>ModernTown</kind>
  <materialList>CBF_Modern_MaterialList</materialList>
  <styles><li>CBF_ModernVanilla</li></styles>
  <factionTags><li>EQ_Faction_Forja</li></factionTags>
  <wards>
    <li>
      <kind>Town</kind>
      <districts>
        <li><entries><li><tag>CBF_Vanilla_HouseSmall</tag><count>6~10</count><filler>true</filler></li></entries></li>
      </districts>
    </li>
  </wards>
</CustomBaseFramework.SettlementPlanDef>
```

`factionTags` aceita o **defName da FactionDef** ou a sua **`categoryTag`** (decompilado de `GenStep_CBFSettlement.IstEigenerPlan`). Usar `categoryTag` permite um plano para várias facções nossas de uma vez.

## Fluxo recomendado: o editor

1. Colônia de teste → Opções do mod CBF → ligar **Editor in the architect menu**.
2. Construir o prédio no mapa, abrir o editor (aba no Arquiteto), selecionar a área e salvar como `PieceDef` numa categoria. O editor cria/atualiza um **mod próprio** (About.xml com dependência do CBF, pastas `1.6/Defs/CustomBaseFramework/...`).
3. Montar o `SettlementPlanDef` no editor (alas, distritos, defensores) e ver o resultado na pré-visualização de mapa.
4. Copiar os XML gerados para o nosso mod (pasta condicional) e trocar o prefixo para `EQ_`.

O editor **não** escreve `LoadFolders.xml` nem copia ícones de mapa-múndi.

## Armadilhas

- Saves existentes: bases já visitadas ficam como estão; só bases novas/não visitadas usam o plano.
- Herdar de pais do CBF (`ParentName="CBF_Ruin_Basis"`) exige carregar **depois** do CBF.
- Sem `materialList` ou sem nenhuma entrada de prédio, o plano é desligado com aviso no log.
- Peças com `onlyWithMod`/`offIfMod` e `MayRequire` em itens de lista para conteúdo de outros mods.
