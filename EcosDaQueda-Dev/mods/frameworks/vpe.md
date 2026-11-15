# Vanilla Psycasts Expanded (VPE) — framework de caminhos psíquicos

- **packageId:** `VanillaExpanded.VPsycastsE` · pasta `Mods/2842502659` · DLL `VanillaPsycastsExpanded.dll`
- **Depende de:** VEF (as habilidades são `VEF.Abilities.AbilityDef`, ver [vef.md](vef.md) §2) e Royalty.
- **Referência gerada:** [`../api/VanillaPsycastsExpanded.md`](../api/VanillaPsycastsExpanded.md).
- **Já usamos:** `PawnKindAbilityExtension_Psycasts` nas PawnKinds do Coro e de outras potências (TECNICO › PawnKinds).

O VPE substitui o psínculo vanilla por uma árvore: o pawn ganha **níveis de conjurador** meditando, gasta **pontos** em **caminhos** (`PsycasterPathDef`) e em atributos. Cada caminho é uma grade de poderes por nível. O VPE Hemosage é o exemplo de **mod que adiciona um caminho novo** sem mexer no VPE — é o modelo a seguir para um caminho próprio do Ecos (ex.: "Coralista" do Coro Ascendente).

## Criar um caminho novo

### 1. `VanillaPsycastsExpanded.PsycasterPathDef`

| Campo | Uso |
|---|---|
| `defName`, `label`, `description`, `tooltip` | Textos (traduzíveis por DefInjected `VanillaPsycastsExpanded.PsycasterPathDef`) |
| `background`, `altBackground` | Textura de fundo do painel (caminho em `Textures/`) |
| `backgroundColor` | Cor alternativa |
| `tab` | Aba onde aparece (os do VPE usam `Psycasts`) |
| `order` | Ordem na lista |
| `width`, `height` | Tamanho da grade (normalmente automático) |
| `requiredGene`, `requiredMeme`, `requiredFocus`, `requiredMechanitor`, `requiredBackstoriesAny` | Trava o caminho (ex.: só quem tem o gene `Hemogenic`) |
| `lockedReason` | Texto mostrado quando travado |
| `ensureLockRequirement` | Garante a trava mesmo em pawns gerados |
| `ignoreLockRestrictionsForNeurotrainers` | Neurotreinadores ignoram a trava (padrão `true`) |

As habilidades **não** são listadas no path: cada habilidade aponta para o caminho (abaixo); o VPE monta `abilities` e `abilityLevelsInOrder` sozinho.

```xml
<VanillaPsycastsExpanded.PsycasterPathDef>
  <defName>EQ_Coralista</defName>
  <label>coralista</label>
  <description>Poderes do Coro Ascendente: harmonia forçada, à força de decibéis.</description>
  <background>UI/EcosDaQueda/PathBG_Coro</background>
  <tab>Psycasts</tab>
  <requiredMeme>EQ_Meme_PsychicChoir</requiredMeme>
  <lockedReason>Só membros do Coro</lockedReason>
  <ensureLockRequirement>true</ensureLockRequirement>
</VanillaPsycastsExpanded.PsycasterPathDef>
```

### 2. Os poderes: `VEF.Abilities.AbilityDef` + `AbilityExtension_Psycast`

Herdar de `VPE_PsycastBase` (abstrato do VPE, já traz verbo, cast, sons padrão). A extensão do VPE define onde o poder fica e quanto custa:

| Campo de `VanillaPsycastsExpanded.AbilityExtension_Psycast` | Uso |
|---|---|
| `path` | O `PsycasterPathDef` |
| `level` | Linha da grade (1 = primeiro nível do caminho) |
| `order` | Coluna dentro do nível |
| `prerequisites` | Outros poderes exigidos antes |
| `psyfocusCost` | Custo de concentração psíquica (0–1) |
| `entropyGain` (+ `entropyGainStatFactors`) | Calor neural gerado |
| `psychic` | Afetado por sensibilidade psíquica |
| `showCastBubble`, `spaceAfter` | Visual |

O efeito vem do `abilityClass` (C# próprio ou `VEF.Abilities.Ability_Blank`) e das outras extensões VEF (`AbilityExtension_Hediff`, `_Projectile`…). Exemplo completo em [vef.md](vef.md) §2 (`VPEH_Bloodfocus`).

### 3. Dar o caminho a NPCs — `PawnKindAbilityExtension_Psycasts`

Em `<modExtensions>` da `PawnKindDef` (sempre com `MayRequire="VanillaExpanded.VPsycastsE"`):

```xml
<li MayRequire="VanillaExpanded.VPsycastsE" Class="VanillaPsycastsExpanded.PawnKindAbilityExtension_Psycasts">
  <implantDef>VPE_PsycastAbilityImplant</implantDef>
  <initialLevel>1</initialLevel>
  <statUpgradePoints>2~6</statUpgradePoints>
  <unlockedPaths>
    <!-- <caminho>faixa de níveis de poder | quantidade de poderes</caminho> -->
    <VPE_Warlord>1~4|2~10</VPE_Warlord>
  </unlockedPaths>
</li>
```

O valor `A~B|C~D` (decompilado de `PathUnlockData.LoadDataFromXmlCustom`): antes do `|` é `unlockedAbilityLevelRange` (de quais níveis do caminho sortear poderes), depois é `unlockedAbilityCount` (quantos poderes liberar).

## Outros ganchos úteis

- **Focos de meditação** novos: `MeditationFocusDef` (VPE adiciona grupo, arcotecnologia, ciência, riqueza). Um foco "ruído de rádio"/"sucata" combinaria com o tom do mod.
- **Eltex:** recurso (`VPE_Eltex`) e equipamentos; o Império pune quem vende eltex (história `VPE_SoldEltex`) — útil se uma potência quiser ser o mercado negro de eltex.
- **Ataques de conjuradores:** `RaidStrategyDef` `VPE_ImmediateAttack_Psycasters`.
- **Narrador** `VPE_Basilicus` (gera conjuradores com frequência).
- **Habilidades mantidas** geram calor mínimo permanente (stat `VPE_PsychicEntropyMinimum`).

## Armadilhas

- Poderes do VPE não aparecem como `RimWorld.AbilityDef`: `DefDatabase<RimWorld.AbilityDef>` não os encontra; use `VEF.Abilities.AbilityDef`.
- Na tradução (`DefInjected`), as pastas são `VEF.Abilities.AbilityDef` e `VanillaPsycastsExpanded.PsycasterPathDef`.
- `generalRules` das pesquisas do VPE não são traduzíveis (preenchidas por código).
- CE tem patch próprio do VPE (`ModPatches/Vanilla Psycasts Expanded`): poderes que disparam projéteis usam projéteis CE.
