# Início de jogo (cenários, xenos, narrador)

## Início de jogo (GameStart.cs)

- `StartRulesDef` (`Defs/Misc/EQ_StartRules.xml`): liga/desliga as travas; `devModeShowsAll` mostra tudo no modo desenvolvedor.
- Menu de xenotipos: a tela do jogo (`CharacterCardUtility.LifestageAndXenotypeOptions`) e o Prepare Carefully (`PanelXenotype`) montam um `FloatMenu` a partir de `DefDatabase<XenotypeDef>`. O patch no construtor `FloatMenu(List<FloatMenuOption>)` reconhece esse menu (estado Entry e 5+ opções com rótulo de xenotipo) e tira o que o cenário não permite. Na geração, `Patch_PawnGenerator_GenerateGenes` troca o xeno de todo pawn `PawnGenerationContext.PlayerStarter` fora da lista.
- `ScenarioXenotypesExtension` (modExtension do ScenarioDef): `xenotypeChances`. Sem ela: `EQ_WorldXenotypes`.
- Cenários ocultos: postfix em `ScenarioLister.ScenariosInCategory`/`AllScenarios` (os defs continuam existindo).
- Narradores: `StorytellerDef.listVisible = false` nos de fora; `StorytellerCopyExtension.copyFrom` copia comps e curvas de outro narrador depois dos patches (os mods acrescentam comps à Cassandra pelo defName).
- `ScenPart_EQReputation` (`EQ_ScenPart_Reputation`): `factionDef`, `goodwill`, `fame`, `infamy`, `note`; aplica em `PostGameStart`.
