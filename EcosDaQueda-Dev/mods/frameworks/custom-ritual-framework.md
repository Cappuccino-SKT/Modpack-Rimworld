# Custom Ritual Framework (CRF)

- **packageId:** `thesepeople.ritualattachableoutcomes` · pasta `Mods/2561617361` · DLL `TPRitualAttachableOutcomes.dll` (namespace `TPRitualAttachableOutcomes`)
- **Depende de:** Ideology. **Referência gerada:** [`../api/TPRitualAttachableOutcomes.md`](../api/TPRitualAttachableOutcomes.md).
- **Tutorial comentado do autor:** `Mods/2561617361/1.7/Defs/Custom_Ritual_Example.xml` (ritual inteiro "Serviço ideológico", linha a linha) e `Ritual_AttachableOutcomes_TP.xml` (resultados anexáveis). Ler antes de criar um ritual.

Dois usos:

1. **Resultados anexáveis** novos (os bônus que o jogador escolhe ao criar um ritual: "colheita farta", "maná do céu"…) só com XML.
2. **Rituais completos** (preceito + padrão + comportamento + resultado) com extensões que o vanilla não tem (recarga própria, alvo customizado, falha por item ausente, músico tocando…).

Relevante para o Ecos: cerimônias de cada potência (já temos missão de cerimônia em C#; um ritual ideológico daria versão "da colônia"), e resultados como "a potência X manda uma caravana" ou "doutrinação" (reduz resistência/vontade de prisioneiros).

## Anatomia de um ritual (vanilla + CRF)

1. **`PreceptDef`** (`issue` Ritual, `preceptClass` `Precept_Ritual`, `ritualPatternBase`) — + extensão `TPRitualAttachableOutcomes.Precept_Ritual_Custom`: `coolDownDays` (padrão 20), `building` (prédio exigido), `useRoom`, `maxDistance`.
   - O autor recomenda **não herdar** de `FestivalBase` quando usar extensões: copiar os campos.
2. **`RitualPatternDef`**: `ritualBehavior`, `ritualOutcomeEffect`, `ritualTargetFilter`, `ritualObligationTargetFilter`, `ritualObligationTriggers` (`RitualObligationTrigger_DateProperties`…), `canStartAnytime`, `nameMaker`, `tags`, `patternGroupTag`.
3. **`RitualBehaviorDef`**: `durationTicks`, `roles` (`RitualRoleTag` com `id`, `tag` Moralist/Leader, `precept`, `required`), `stages` (cada estágio: `defaultDuty`, `endTriggers` como `StageEndTrigger_DurationPercentage`, `roleBehaviors` com `dutyDef`/`speakerInteraction`/`customPositions`, `postAction` como `RitualStageAction_Message`). CRF acrescenta `StageEndTrigger_ThingNotPresent` e `StageFailTrigger_ThingNotPresentInArea`, `RitualBehaviorWorker_Speech_Custom`, e `JobGiver_MusicianPlayInstrument`/`JobGiver_HaulToRitualSpot`/`JobGiver_IngestThing` para duties.
4. **`RitualOutcomeEffectDef`**: `workerClass` (`RitualOutcomeEffectWorker_FromQuality` vanilla ou `TPRitualAttachableOutcomes.RitualOutcomeEffectWorker_FromQuality_TP_Custom`, que aceita as extensões do CRF), `comps` de qualidade (`RitualOutcomeComp_BuildingOfDefUsed`, `_RitualSeatPresent`, `_ParticipantCount`, `_RitualTargetDefs`), `outcomeChances` (4 faixas com `chance`, `memory`, `description`, `positivityIndex` −2…+2), `allowAttachableOutcome`.
5. **`ThoughtDef`**s das memórias de cada faixa.
6. Alvos extras: `RitualTargetFilterDef`/`RitualObligationTargetFilterDef` com `workerClass` `RitualTargetFilter_IdeoBuilding_Extra` / `RitualObligationTargetWorker_AnyRitualSpotOrAltar_Extra` + `TargetFilter_ModExtension` (`extraCandidates` = defNames de coisas que também servem de alvo, `missingDesc`). Também `RitualObligationTargetWorker_GraveWithCorpse`.

## Resultado anexável sem C#

`RitualAttachableOutcomeEffectDef` com `workerClass` `TPRitualAttachableOutcomes.TPRitualAttachableOutcomeEffectWorker_Custom` e a extensão `RitualAttachableOutcomeEffectDef_TP_Custom`:

| Grupo | Campos |
|---|---|
| Quando | `onlyPositiveOutcomes` (padrão true), `triggerPositivityIndex` (lista de faixas), `cooldownDays` |
| Em quem | `appliesTo` (papéis/ids), `appliesToRandom`, `invertApply`, `forEachPawn` |
| Pawn | `hediffToAdd` (+ `hediffSeverity`, `bodyPart`), `hediffToRemove`, `thought`, `inspiration`, `abilityToAdd`, `trait` (+ `traitDegree`, `traitForced`, `removeTrait`), `psylinkLevel`, `resurrect`, `relationshipsToAdd/Remove` |
| Prisioneiros/escravos | `suppression`, `willReduction`, `resistanceReduction`, `certainty` |
| Itens | `item`, `baseAmount`, `amountPerPawn`, `spawnNearRitual`, `instaDrop`, `forbid`, `roofPunch` |
| Mundo | `weather`, `incident` + `incidentParms` (`IncidentParmsCustom`: `myFaction`, `myRaidStrategy`, `myRaidArrivalMode`, `myPawnGroups`, `mySpawnCenter`, `scalePoints`…) |
| Carta | `letterLabel`, `letterText`, `letterType` |
| Sorteio | `node` (lista de `RitualAttachableOutcomeEffectDef_TP_Custom_Node`, cada um com os mesmos campos + `weight`) com `randomFromNode` |

```xml
<RitualAttachableOutcomeEffectDef>
  <defName>EQ_Outcome_CaravanaDaForja</defName>
  <label>bênção da garantia</label>
  <effectDesc>a Irmandade da Forja manda peças de reposição "por cortesia" (sujeito a taxa)</effectDesc>
  <workerClass>TPRitualAttachableOutcomes.TPRitualAttachableOutcomeEffectWorker_Custom</workerClass>
  <modExtensions>
    <li Class="TPRitualAttachableOutcomes.RitualAttachableOutcomeEffectDef_TP_Custom">
      <item>ComponentIndustrial</item>
      <baseAmount>3~6</baseAmount>
      <spawnNearRitual>true</spawnNearRitual>
      <letterLabel>Entrega da Forja</letterLabel>
      <letterText>Chegou um pacote. A nota fiscal chega depois.</letterText>
    </li>
  </modExtensions>
</RitualAttachableOutcomeEffectDef>
```

Tudo com `MayRequire="thesepeople.ritualattachableoutcomes"` (e Ideology).

## Armadilhas

- Campos de texto que apontam defs (`hediffToAdd`, `item`, `thought`) são **strings**, não referências: erro de digitação só aparece em jogo. Testar com o executor.
- Itens de DLC (`abilityToAdd` de Royalty) quebram saves sem a DLC: o autor deixa comentados.
- O outro mod novo **Blessed Conception Psychic Ritual** usa o sistema de rituais psíquicos do Anomaly (`PsychicRitualDef`), não o CRF.
