# Harmony e HugsLib — bibliotecas de C#

## Harmony (`brrainz.harmony`, pasta `Mods/2009463077`)

Biblioteca que **altera métodos do jogo em tempo de execução** sem mexer no `Assembly-CSharp.dll`: roda código antes (Prefix), depois (Postfix) ou reescreve as instruções (Transpiler) de qualquer método. Quase todo mod com DLL do modpack depende dele. É a única forma de mudar comportamento que não está exposto por Def/XML.

### Como o nosso projeto usa

- Referência no `.csproj`: `$(GameDir)Mods\2009463077\1.7\Assemblies\0Harmony.dll` com `<Private>false</Private>` (não copiar a DLL para o nosso mod: o jogo já carrega a do mod Harmony).
- `About.xml`: `modDependencies` + `loadAfter` em `brrainz.harmony`.
- Inicialização (`Mods/EcosDaQueda/Source/EcosDaQueda/HarmonyPatches.cs`):

```csharp
[StaticConstructorOnStartup]
public static class EcosDaQuedaStartup
{
    static EcosDaQuedaStartup()
    {
        new Harmony("ecosdaqueda.core").PatchAll(typeof(EcosDaQuedaStartup).Assembly);
    }
}
```

`PatchAll` acha toda classe com `[HarmonyPatch]` do assembly. ID do Harmony = nosso packageId (para outros mods saberem quem patchou).


### Tipos de patch

```csharp
[HarmonyPatch(typeof(Faction), nameof(Faction.TryAffectGoodwillWith))]
public static class Patch_Faction_TryAffectGoodwillWith
{
    // Postfix: roda depois; lê argumentos pelo nome e o retorno por __result.
    public static void Postfix(Faction __instance, Faction other, int goodwillChange, bool __result) { ... }
}

[HarmonyPatch(typeof(TradeUtility), nameof(TradeUtility.GetPricePlayerBuy))]
public static class Patch_PricePlayerBuy
{
    // Prefix com ref: muda um argumento antes do método rodar.
    public static void Prefix(ref float priceGain_FactionBase) => priceGain_FactionBase += ReputationTrade.CurrentGain();
}

[HarmonyPatch(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.GainFavor))]
public static class Patch_GainFavor
{
    // Prefix que devolve bool: false = pula o método original (e os prefixes seguintes).
    public static bool Prefix(Pawn_RoyaltyTracker __instance, Faction faction, int amount) { ... return true; }
}
```

Parâmetros especiais: `__instance` (o objeto), `__result` (retorno; `ref` para trocar), `__state` (passa dado do Prefix ao Postfix), `___campoPrivado` (três sublinhados = campo privado da classe), `__args`, `__originalMethod`.

Outras formas de mirar o método:
- Sobrecarga: `[HarmonyPatch(typeof(X), "Metodo", new[] { typeof(int), typeof(Pawn) })]`.
- Getter/setter: `[HarmonyPatch(typeof(X), nameof(X.Prop), MethodType.Getter)]`.
- Construtor: `MethodType.Constructor`.
- Método escolhido em código: `static MethodBase TargetMethod()` dentro da classe de patch.
- Patch condicional: `static bool Prepare()` → `false` pula o patch (ex.: outro mod não está ativo).

### Integrar com um mod sem depender da DLL dele

Para patchar método de **outro mod opcional** (ex.: Hegemony, VEF), duas opções:

1. **Assembly separado** carregado só com o mod ativo (o que fizemos com o Hegemony): `LoadFolders.xml` com `<li IfModActive="jaeger972.factionterritories2">Mods/Hegemony</li>`, e a DLL `EcosDaQueda.Hegemony.dll` referencia `FactionTerritories2.dll` diretamente. Sem o mod, a DLL nem carrega.
2. **Reflexão no mesmo assembly**: `AccessTools.TypeByName("VEF.Genes.GeneExtension")` + `TargetMethod()`/`Prepare()` que devolve `false` se o tipo for `null`. Não referenciar a DLL do outro mod no `.csproj`.

`AccessTools` (Harmony) também serve fora de patches: `AccessTools.Field(typeof(X), "campo")`, `AccessTools.Method(...)`, `Traverse.Create(obj).Field("x").GetValue<T>()`.

### Transpiler (quando Prefix/Postfix não bastam)

Reescreve IL; frágil a updates do jogo. Use só quando precisar mudar algo no meio do método. Padrão com `CodeMatcher`:

```csharp
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    => new CodeMatcher(instr)
        .MatchStartForward(new CodeMatch(OpCodes.Ldc_R4, 0.5f))
        .SetOperandAndAdvance(0.75f)
        .InstructionEnumeration();
```

Para ver o IL/C# alvo: `scripts/decompile.ps1 -Dll Assembly-CSharp.dll -Type RimWorld.TradeUtility`.

### Depurar

- `Harmony.DEBUG = true` antes do `PatchAll` grava `harmony.log.txt` na área de trabalho (só para depurar, nunca publicar ligado).
- Conflito com outro mod: `Harmony.GetPatchInfo(metodo)` lista quem patchou (owners). O **Dubs Performance Analyzer** (no modpack) mostra o custo de cada patch em jogo.
- Erro "Patching exception" no log ao iniciar = assinatura errada (nome de parâmetro, sobrecarga). Os parâmetros do patch precisam ter **o mesmo nome** dos do método original.

## HugsLib (`UnlimitedHugs.HugsLib`, pasta `Mods/818773962`)

Biblioteca de conveniência (mais antiga). Usada no modpack por Allow Tool e Simple Sidearms. **Nosso mod não depende dela** — usar só se trouxer algo que o jogo/Harmony não dão.

O que oferece (ver [`../api/HugsLib.md`](../api/HugsLib.md)):

- **`HugsLib.ModBase`**: classe base de mod com eventos (métodos virtuais) `StaticInitialize`, `Initialize`, `DefsLoaded`, `WorldLoaded`, `MapGenerated`, `MapLoaded`, `MapDiscarded`, `Tick`, `Update`, `OnGUI`, `SettingsChanged`, `ApplicationQuit`; aplica Harmony sozinha (`HarmonyAutoPatch`).
- **Opções** (`Settings.GetHandle<T>("chave", "título", "descrição", padrão)`): opções persistentes sem escrever `ModSettings`.
- **`HugsLib.UpdateFeatureDef`**: janela de "novidades da versão" ao atualizar o mod (Allow Tool usa).
- Utilitários: `HugsLibUtility`, agendadores `HugsLibController.Instance.TickDelayScheduler` (chamar daqui a N ticks) e `DistributedTickScheduler` (espalhar ticks de muitos objetos), dados por save (`UtilityWorldObject`), compartilhar o log (botão no log do jogo que publica num GitHub Gist — **não usar: o log pode ter caminhos com o nome do usuário**).

Hoje o jogo vanilla já tem `Mod`/`ModSettings`/`GameComponent`/`WorldComponent`, que é o que usamos (`WorldComponent_Reputation`, `WorldComponent_IdeoTemplates`). Mantemos assim.
