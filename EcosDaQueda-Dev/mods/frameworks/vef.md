# Vanilla Expanded Framework (VEF)

- **packageId:** `OskarPotocki.VanillaFactionsExpanded.Core` · pasta `Mods/2023507013` · Workshop 2023507013
- **DLLs:** `VEF.dll` (núcleo), `KCSG.dll` (estruturas), `MVCF.dll` (vários verbos), `PipeSystem.dll` (redes de canos), `Outposts.dll` (postos avançados), `0ModSettingsFramework.dll`, `0PrepatcherAPI.dll`.
- **Quem usa:** praticamente todos os mods "Vanilla … Expanded" do modpack, mais Alpha Genes/Memes/Biomes. É a dependência mais comum: se o nosso mod usar algo do VEF, não precisa de `MayRequire` só para isso porque o VEF sempre estará presente no modpack; mas, se o def também fizer sentido sem o modpack, use `MayRequire="OskarPotocki.VanillaFactionsExpanded.Core"`.
- **Referência completa gerada:** [`../api/VEF.md`](../api/VEF.md) (574 classes), [`KCSG`](../api/KCSG.md), [`MVCF`](../api/MVCF.md), [`PipeSystem`](../api/PipeSystem.md), [`Outposts`](../api/Outposts.md), [`ModSettingsFramework`](../api/0ModSettingsFramework.md).

> Namespaces mudaram no 1.6: o antigo `VFECore.*` virou `VEF.*` (ex.: `VFECore.Abilities.AbilityDef` → `VEF.Abilities.AbilityDef`). Guias da internet antigos usam o nome velho; aqui vale o nome novo. Na pasta `DefInjected` o tipo vai com nome completo (`VEF.Abilities.AbilityDef`).

## Mapa dos módulos

| Namespace | Para que serve | Classes mais usadas no modpack (contagem de usos em XML) |
|---|---|---|
| `VEF.Genes` | Extensões de gene além do vanilla | `GeneExtension` (962), `ConditionalStatAffecter_Darkness/Outdoors/InLight` |
| `VEF.Abilities` | Sistema próprio de habilidades (base do VPE) | `AbilityDef`, `Ability_Blank` (97), `AbilityExtension_Hediff` (100), `_Projectile`, `_Spawn`, `_FleckOnTarget`, `_EffecterOnTarget`, `_SocialInteraction` |
| `VEF.Weapons` | Armas pesadas, lasers, tesla, lança-chamas, multi-verbo | `HeavyWeapon` (102), `LaserGun`, `LaserBeamDef`, `CompProperties_LaserCapacitor`, `CompProperties_MultiVerbWeapon`, `ExpandableProjectileDef`, `TeslaChainingProps` |
| `VEF.Apparels` | Roupas que mudam stats/traços/capacidades, escudos | `ApparelExtension` (55), `CompProperties_ShieldBubble`, `CompProperties_Shield` |
| `VEF.Things` | Ajustes gerais de ThingDef | `ThingDefExtension` (60), `StuffExtension` |
| `VEF.Buildings` | Construções saqueáveis, herança de receitas, flechas | `RecipeInheritanceExtension`, `LootableBuilding(Details)`, `CompBouncingArrow` |
| `VEF.AnimalBehaviours` | ~200 comps de animal/hediff (produtos, invocar, regenerar, flutuar, mudar clima…) | `CompProperties_InitialAbility`, `_InitialHediff`, `_Draftable`, `_AnimalProduct`, `HediffCompProperties_Regeneration`, `_ChangeWeather`, `_Spawner` |
| `VEF.Factions` | Regras extras de facção | `FactionDefExtension` |
| `VEF.Storyteller` | Cadeias de missões | `QuestChainDef`, `QuestChainExtension` |
| `VEF.Memes` | Memes com efeitos extras | `ExtendedMemeProperties` |
| `VEF.Maps` | Mutadores de tile (1.6) | `TileMutatorExtension`, `TileMutatorWorker_*` |
| `VEF.Graphics` | Gráficos de piso, sobreposições | `FloorGraphicExtension`, `ThingWithFloorGraphic`, `CustomOverlayDef` |
| `VEF.Plants`, `VEF.Cooking`, `VEF.Weathers`, `VEF.Planet`, `VEF.Sounds` | Plantas de colheita dupla, refeições gourmet, climas, etc. | `DualCropExtension`, `GourmetMeal`, `HediffCompProperties_WhileHavingThoughts` |
| `VEF.OptionalFeatures` | Liga partes do framework só quando algum mod pede | `OptionalFeaturesDef` |
| `KCSG` | Estruturas e assentamentos pré-desenhados | `StructureLayoutDef`, `SymbolDef`, `SettlementLayoutDef`, `TiledStructureDef`, `GenStep_CustomStructureGen`, `CustomGenOption` |
| `MVCF` | Vários verbos (ataques) por pawn/arma/hediff | `HediffCompProperties_ExtendedVerbGiver`, `CompProperties_VerbProps`, `CompProperties_VerbGiver` |
| `PipeSystem` | Redes de recurso por cano (pasta, gás, água…) | `PipeNetDef`, `CompProperties_Resource`, `_ResourceStorage`, `_AdvancedResourceProcessor`, `ProcessDef` |
| `Outposts` | Postos avançados no mapa-múndi | `OutpostExtension`, `Outpost` (WorldObject) |

Como achar o campo certo: abra `../api/VEF.md` e procure a classe (Ctrl+F). Para o comportamento, descompile: `scripts/decompile.ps1 -Dll Mods\2023507013\1.7\Assemblies\VEF.dll -Type VEF.Genes.GeneExtension`.

## 1. Extensões de gene — `VEF.Genes.GeneExtension`

Vai em `<modExtensions>` de um `GeneDef`. É o que os VRE e o Alpha Genes mais usam. Campos principais:

- **Hediffs:** `hediffsToBodyParts` (lista de `{hediff, bodyparts}`), `hediffToWholeBody`.
- **Pelagem/visual:** `useSkinColorForFur`, `useSkinAndHairColorsForFur`, `dontColourFur`, `useMaskForFur`, `furHidesBody`, `backgroundPathEndogenes/Xenogenes/Archite` (moldura do ícone).
- **Sexo:** `forceMale`, `forceFemale`, `forGenderOnly`.
- **Corpo:** `customBloodThingDef`, `customBloodIcon`, `customBloodEffect`, `customWoundsFromFleshtype`, `customVomitThingDef`, `customMeatThingDef`, `customLeatherThingDef`.
- **Comida/canibalismo:** `defsTreatedAsHumanMeat`, `foodTypesTreatedAsHumanMeat`, `defsTreatedAsHumanLeather`, `doubleNegativeFoodThought`.
- **Habilidades:** `noSkillLoss`, `skillRecreation`, `globalSkillLossMultiplier`, `skillDegradation`.
- **Outros:** `diseaseProgressionFactor`, `pregnancySpeedFactor`, `hideGene`, `disableGeneExtraction`, `thingSetMaker` (itens ao ganhar o gene).

```xml
<GeneDef>
  <defName>EQ_GarrasDeSucata</defName>
  <label>garras de sucata</label>
  <!-- ... campos vanilla de GeneDef ... -->
  <modExtensions>
    <li Class="VEF.Genes.GeneExtension">
      <hediffsToBodyParts>
        <li>
          <hediff>EQ_GarrasHediff</hediff>
          <bodyparts><li>Hand</li><li>Hand</li></bodyparts>
        </li>
      </hediffsToBodyParts>
      <customBloodThingDef>Filth_MachineBits</customBloodThingDef>
    </li>
  </modExtensions>
</GeneDef>
```

Stats condicionais (genes "no escuro", "ao ar livre"): em `statOffsets`/`conditionalStatAffecters` use `<li Class="VEF.Genes.ConditionalStatAffecter_Darkness">` (também `_Outdoors`, `_InLight`).

## 2. Habilidades — `VEF.Abilities.AbilityDef`

Sistema paralelo ao `AbilityDef` vanilla, mais flexível; o **VPE inteiro** é construído nele (cada poder psíquico é um `VEF.Abilities.AbilityDef` + `AbilityExtension_Psycast`). Para dar uma habilidade VEF a um pawn: comp `VEF.Abilities.CompProperties_AbilitiesApparel` (roupa), `VEF.AnimalBehaviours.CompProperties_InitialAbility` (animal), gene/hediff com `abilities`, ou o `VEF.Memes.ExtendedMemeProperties.abilitiesGiven`.

Campos do def (ver lista completa na API): `abilityClass` (comportamento), `targetMode`/`targetModes`, `targetCount`, `range`/`radius`/`power`/`castTime`/`cooldownTime`/`durationTime` (cada um com `*StatFactors`/`*StatOffsets` para escalar com stat, ex.: `PsychicSensitivity`), `goodwillImpact`, `requireLineOfSight`, `worldTargeting`, `hasAoE`, `requiredHediff`, `requiredTrait`, `castFleck`, `castSound`, `jobDef`.

Classes de `abilityClass` prontas: `VEF.Abilities.Ability_Blank` (não faz nada sozinha: o efeito vem das extensões), `Ability_ShootProjectile`, `Ability_Spawn`, `Ability_Explode`, além de várias específicas.

Efeitos por extensão (`<modExtensions>`, empilháveis): `AbilityExtension_Hediff` (`hediff`, `severity`, `bodyPartToApply`, `applyToCaster`, `targetOnlyEnemies`, `durationMultiplier`), `AbilityExtension_Projectile`, `AbilityExtension_Spawn`, `AbilityExtension_FleckOnTarget`, `AbilityExtension_EffecterOnTarget`, `AbilityExtension_SocialInteraction`, `AbilityExtension_Stun`… (lista em `api/VEF.md` › "Workers").

Exemplo real (VPE Hemosage, `Mods/2990596478/1.6/Defs/AbilityDefs/Hemosage.xml`):

```xml
<VEF.Abilities.AbilityDef ParentName="VPE_PsycastBase">
  <defName>VPEH_Bloodfocus</defName>
  <label>blood focus</label>
  <abilityClass>VPEHemosage.Ability_Bloodfocus</abilityClass>
  <targetMode>Self</targetMode>
  <castTime>180</castTime>
  <modExtensions>
    <li Class="VanillaPsycastsExpanded.AbilityExtension_Psycast">
      <path>VPEH_Hemosage</path><level>1</level><order>1</order>
      <psyfocusCost>0.02</psyfocusCost><entropyGain>0</entropyGain>
    </li>
    <li Class="VEF.Abilities.AbilityExtension_Hediff">
      <hediff>VPEH_Bloodfocus</hediff>
    </li>
  </modExtensions>
</VEF.Abilities.AbilityDef>
```

Uma habilidade simples sem C#: `abilityClass` = `VEF.Abilities.Ability_Blank` + `AbilityExtension_Hediff` + `AbilityExtension_FleckOnTarget`. Para virar poder psíquico do VPE, ver [vpe.md](vpe.md).

## 3. Armas — `VEF.Weapons`

- **Arma pesada** (só certos pawns, lentidão): `<thingClass>VEF.Weapons.HeavyWeapon</thingClass>` em armas grandes.
- **Laser:** `thingClass` `VEF.Weapons.LaserGun`, projétil com `VEF.Weapons.LaserBeamDef`, comp `CompProperties_LaserCapacitor` (carga).
- **Vários modos de tiro:** `CompProperties_MultiVerbWeapon` (alternar verbos).
- **Projétil que cresce/serpenteia** (lança-chamas, raio): `VEF.Weapons.ExpandableProjectileDef` (`FlamethrowProjectile`).
- **Tesla em cadeia:** `TeslaChainingProps`. **Cartuchos explosivos:** `CompProperties_Explosive_Shells`.
- **Desenho na mão:** `VEF.Things.ThingDefExtension.weaponCarryDrawOffsets` / `weaponDraftedDrawOffsets`; `usableWithShields`.

## 4. Roupas — `VEF.Apparels.ApparelExtension`

`<modExtensions>` em apparel: `equippedStatFactors`, `workDisables`, `skillDisables`, `traitsOnEquip`/`traitsOnUnequip`, `pawnCapacityMinLevels`, `preventDowning`, `preventKilling`, `preventBleeding`, `destroyedOnDeath`, `secondaryApparelGraphics`, `hideHead`, `isUnifiedApparel`, `skillGainModifier`, `moveSpeedFactorByTerrainTag`.
Escudo: comp `VEF.Apparels.CompProperties_ShieldBubble` (bolha estilo VFE Pirates) ou `CompProperties_Shield` (escudo físico).

Útil para nós: uniformes de facção que dão traço enquanto vestidos (`traitsOnEquip`) — ex.: "doutrinação" do Silêncio.

## 5. Facções — `VEF.Factions.FactionDefExtension`

Em `<modExtensions>` de `FactionDef`: `hasCities`, `settlementGenerationSymbol` (gerar base KCSG), `startingGoodwillByFactionDefs` (relação inicial com facções específicas), `allowedBiomes`/`disallowedBiomes`, `requiredHillLevels`, `spawnOnCoastalTilesOnly`, `neverConnectToRoads`, `minDistanceToOtherSettlements`, `excludeFromCommConsole`, `excludeFromQuests`, `allowedStrategies` (estratégias de ataque), `siegeParameterSetDef` (cerco customizado), `strangerInBlackReplacement`, `forcedFactionData`.

Útil para as nossas potências: restringir biomas, relações iniciais (hoje feitas no nosso C# via `FactionRelationPresetDef`) e estratégias de raide temáticas. Atenção à interação com o **Custom Base Framework**, que também assume a geração de bases (ver [custom-base-framework.md](custom-base-framework.md)).

## 6. Cadeias de missões — `VEF.Storyteller`

`QuestChainDef` agrupa missões; cada `QuestScriptDef` recebe `<li Class="VEF.Storyteller.QuestChainExtension">` com: `questChainDef`, `conditionSucceedQuests`/`conditionFailQuests` (pré-requisitos), `conditionMinDaysSinceStart`, `ticksSinceSucceed`, `isRepeatable`/`mtbDaysRepeat`, `grantAgainOnFailure/OnSuccess/OnExpiry` + `daysUntilGrantAgain*`, `requiredResearch`. Os VQE (Ancients, Generator, Drone Factory) usam isso. Bom candidato para as campanhas de cada potência do Ecos.

## 7. Memes — `VEF.Memes.ExtendedMemeProperties`

`neededMeme`/`requiredMemes`, `forcedTrait` (todos os membros ganham o traço), `factionOpinionOffset`, `abilitiesGiven`, `removedDesignators` (esconde construções do menu). Já usamos memes do VIE/Alpha Memes nas nossas ideologias; dá para pôr essa extensão nos nossos `EQ_Meme_*`.

## 8. Things, Buildings, Plantas

- `VEF.Things.ThingDefExtension`: `deepColor`/`allowDeepDrill` (minério profundo), `siegeBlueprintPoints`, `destroyCorpse`, `constructionSkillRequirement`, `playerCraftedStyles` (estilo ao fabricar), `shieldDamageIntercepted`.
- `VEF.Things.StuffExtension`: efeitos extras de material.
- `VEF.Buildings.RecipeInheritanceExtension`: uma bancada herda as receitas de outra (evita duplicar receitas).
- `VEF.Buildings.LootableBuilding` + `LootableBuildingDetails`: contêineres que se abrem e soltam itens (bom para "ruínas pré-Queda").
- `VEF.Plants.DualCropExtension`: planta que rende dois produtos.

## 9. Comportamentos de animal e hediff — `VEF.AnimalBehaviours`

~200 classes. As mais úteis: `CompProperties_InitialHediff`, `CompProperties_InitialAbility`, `CompProperties_Draftable` (animal convocável), `CompProperties_AnimalProduct` (produto genérico), `CompProperties_HediffAfterHealthLoss`, `CompProperties_FixedGender`, `CompProperties_GraphicByStyle`, e para hediffs `HediffCompProperties_Regeneration`, `_ChangeWeather`, `_Spawner`, `_GasProducer`, `_Floating`. `AnimalStatExtension` muda stats de raça. Servem também em pawns humanos (os comps de hediff funcionam em qualquer pawn).

## 10. KCSG — estruturas pré-desenhadas

Usado por VFE Deserters/Medieval 2/Insectoids 2, VQE e Alpha Biomes para bases e locais de missão.

- **`KCSG.StructureLayoutDef`**: o desenho. `layouts` = lista de camadas; cada camada é uma lista de linhas `"a,b,c"` onde cada célula é `.` (vazio) ou um **símbolo** (defName de ThingDef/TerrainDef ou de `KCSG.SymbolDef`). Outros campos: `spawnConduits`, `terrainGrid`, `roofGrid`, `tags`.
- **`KCSG.SymbolDef`**: célula com coisa + material + rotação + pawn/item (ex.: `Wall_Steel`, `Bed_Wood_North`).
- **`KCSG.TiledStructureDef`**/`TileDef`: monta uma estrutura grande juntando peças.
- **`KCSG.SettlementLayoutDef`**: assentamento procedural (`settlementSize`, `centerBuildings`, `peripheralBuildings`, `roadOptions`, `stuffableOptions`, `propsOptions`, `defenseOptions`, `stockpileOptions`).
- **Como gerar:** num `SitePartDef`/`MapGeneratorDef`, `<genStep Class="KCSG.GenStep_CustomStructureGen">` com `structureLayoutDefs` ou `tiledStructures`; para a base de uma facção, `CustomGenOption` em `<modExtensions>` da `FactionDef` (`chooseFromlayouts`, `chooseFromSettlements`, `symbolResolvers`, `scatterThings`, `filthTypes`, `preGenClear`).
- **Criar o desenho sem digitar:** construa no mapa, ligue o modo desenvolvedor com "mostrar gizmos de dev" e use o botão **Export** em Arquiteto › Ordens (`KCSG.Designator_ExportToXml`; há também a versão por área, `Designator_ExportToXmlFromArea`). Ele gera o XML do `StructureLayoutDef` (e dos `SymbolDef` que faltarem) numa janela para copiar. No menu de debug há ações do KCSG para **gerar uma estrutura** existente no mapa e testar.
- `KCSG.ScenPart_AddStartingStructure` é `internal`: usado só pelo cenário do VFE (`VFEC_AddStartingStructure`), não serve como ScenPart nosso sem patch.

```xml
<SitePartDef>
  <!-- ... -->
  <genStep Class="KCSG.GenStep_CustomStructureGen">
    <scaleWithQuest>true</scaleWithQuest>
    <tiledStructures><li>VFED_AerodroneStation</li></tiledStructures>
  </genStep>
</SitePartDef>
```

## 11. PipeSystem — redes de recurso

Base do VNPE (pasta nutritiva encanada), VRecycling, gás do VCE etc.

1. **`PipeSystem.PipeNetDef`**: a rede. `resource` (`name`, `unit`, `color`, `onlyShowStored`), `pipeDefs` (os canos), `designator` (categoria e ícone), `overlayOptions`, `noStorageAlert`, `alertProofDefs`.
2. **Canos:** `ThingDef` com `thingClass` `PipeSystem.Building_Pipe` + comp `PipeSystem.CompProperties_Resource` (`<pipeNet>`).
3. **Produtor/consumidor/armazém:** comps `CompProperties_ResourceTrader` (produz ou consome por tick), `CompProperties_ResourceStorage` (`storageCapacity`, barra), `CompProperties_ConvertResourceToThing`/`_ConvertThingToResource`, `CompProperties_AdvancedResourceProcessor` + `ProcessDef` (receita: `ingredients`, `ticks`, `results`), `CompProperties_SpillWhenDamaged`.

```xml
<PipeSystem.PipeNetDef>
  <defName>EQ_RedeDeRadQuimica</defName>
  <resource><name>gosma química</name><unit>L</unit><color>(0.4,0.9,0.3)</color></resource>
  <pipeDefs><li>EQ_CanoGosma</li></pipeDefs>
  <!-- VCHE_PipeNetworks é a aba criada pelo VNPE (vanillaexpanded.vnutriente); sem ele, use uma DesignationCategoryDef nossa -->
  <designator><designationCategoryDef>VCHE_PipeNetworks</designationCategoryDef></designator>
</PipeSystem.PipeNetDef>
```

## 12. MVCF — vários ataques

Dá verbos extras sem trocar a arma: `MVCF.Comps.HediffCompProperties_ExtendedVerbGiver` (implante/gene que ataca), `CompProperties_VerbProps` (opções por verbo: botão próprio, auto-uso), `CompProperties_VerbGiver` (roupa que dá ataque). Usado por genes do Alpha Genes e pelas Warcaskets.

## 13. Outposts — postos avançados

`Outposts.Outpost` é um `WorldObject`; o def recebe `<li Class="Outposts.OutpostExtension">` com produção, custo, requisitos (pawns, skills). Bom para "postos de coleta" de facção no mapa-múndi, mas o Hegemony já cobre território — avaliar sobreposição.

## Armadilhas

- Precisa carregar **depois** do VEF (`loadAfter` ou dependência).
- O VEF tem um patch "OptionalFeatures": alguns sistemas só ligam se um `VEF.OptionalFeatures.OptionalFeaturesDef` pedir. Se um comp não fizer efeito, verifique se o recurso depende de feature opcional.
- `generalRules` de pesquisa dos mods VE sem regras no XML são preenchidas por código depois da injeção de tradução (ver TECNICO › Tradução).
- O Combat Extended tem patches próprios para o VEF (`ModPatches/Vanilla Expanded Framework`): armas VEF novas precisam de patch CE também (ver [combat-extended.md](combat-extended.md)).
