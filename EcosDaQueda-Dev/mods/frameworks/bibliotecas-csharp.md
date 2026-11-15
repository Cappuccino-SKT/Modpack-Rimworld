# Bibliotecas só de C#: Jaeger's Utility Framework e RimExodus Framework

Nenhuma das duas adiciona conteúdo; são dependências de outros mods (Hegemony e RimExodus Seamless World). Só interessam se escrevermos C# que conversa com esses mods ou quisermos as mesmas ferramentas de interface.

## Jaeger's Utility Framework (`jaeger972.utilityframework`, pasta `Mods/3825311621`)

- **Guia do autor:** `Mods/3825311621/MODDER_GUIDE.md` (507 linhas, com exemplo compilável). API pública **versão 5**; builds separadas para 1.6 (net472) e 1.7 (net10.0), ambas C# 7.3.
- **Referência gerada:** [`../api/JaegerUtilityFramework.md`](../api/JaegerUtilityFramework.md) (só classes de Mod/Settings — o resto são utilitários estáticos; ver o guia).
- **Dependência do Hegemony**: versões casadas (Hegemony 0.209.11+ exige framework 1.13.0).

Para usar: `modDependencies` + `loadAfter` em `jaeger972.utilityframework`; referenciar `JaegerUtilityFramework.dll` da pasta da versão certa com `Private=false`; chamar `FrameworkApi.Require(5)` na inicialização.

| Ferramenta | Para quê |
|---|---|
| `SettingsMenu` + `SettingsTab` (+ `SettingsGroup`) + `CheckboxSetting`/`SliderSetting`/`Setting` | Menu de opções com busca, abas e reset por categoria; desenhar em `Mod.DoSettingsWindowContents`. **Não salva** os valores: continuam no nosso `ModSettings.ExposeData` |
| `SharedUI` | Controles de janela com o mesmo visual do Hegemony (botões, cartões, `ReadableLabel`, `BulletList`, `CollapsibleHeader`, divisores, `NavigationTab`) |
| `AdaptiveWindow` | Base de janela arrastável/redimensionável que se ajusta à tela |
| `ResponsiveUI` | Colunas que viram pilha em tela pequena (`TryColumns`, `FitWindow`) |
| `SharedConfirmation.Show(id, label, texto, ação)` | Confirmação com "não perguntar de novo" lembrada |
| `ContextMenus.Option/PawnOption/Open` | Menus flutuantes com o visual do framework |
| `WorkQueue` + `FrameWorkBudget` | Trabalho pesado dividido em passos por frame (iterador `IEnumerator<bool>`, `Process(maxSteps, ms)`) |
| Cache por revisão/mundo, `DeferredRefresh`, `Deterministic` | Evitar recalcular e manter resultado estável |
| Assistência contextual (`AssistanceApi`…) | Dicas/realces guiados na interface |

```csharp
var menu = new SettingsMenu();
var geral = new SettingsTab("ecosdaqueda.geral", "Geral");
geral.Add(new CheckboxSetting("ecosdaqueda.retaliacao", "Potências retaliam",
    () => settings.retaliacao, v => settings.retaliacao = v, "Liga as retaliações."));
geral.Add(new SliderSetting("ecosdaqueda.fama", "Multiplicador de Fama",
    () => settings.multFama, v => settings.multFama = v, 0.25f, 4f, 0.05f, true, true, "Escala ganhos de Fama."));
menu.Add(geral);
// Mod.DoSettingsWindowContents(rect) → desenhar o menu (ver FrameworkExample no guia)
```

Avaliação para o Ecos: nossa aba de reputação e opções poderiam usar `SharedUI` para ficar com o visual do Hegemony, mas isso cria dependência obrigatória do framework. Como o Hegemony é parte fixa do modpack, é aceitável **no assembly da integração** (`EcosDaQueda.Hegemony.dll`), nunca no principal.

## RimExodus Framework (`rimexodus.framework`, pasta `Mods/3830299150`)

- DLL `RimExodus.Maps.dll`. Base do **RimExodus — Seamless World** (mapas vizinhos ligados: andar de um mapa para o outro sem caravana).
- Oferece: desenho de vários mapas e mapeamento de coordenadas entre eles (`MapTransform`, `MapRenderService`), **mapas de bolso** sem tile do mundo (`IPocketMapHostProvider`), serialização/reconstrução de **prefabs de região** (`MapPrefabs`, `MapPrefabDocument`), arquivar mapa inteiro (`MapArchive`), **taxa de tick por mapa** (`MapTickRates`), combate entre mapas (`CrossMapCombat`, com ponte para o CE `CombatExtendedBridge`).
- Sem documentação de modder; API só pelo descompilado (`scripts/decompile.ps1 -Dll Mods\3830299150\1.7\Assemblies\RimExodus.Maps.dll`).
- Relevância para o Ecos: baixa. Atenção apenas a efeitos colaterais — mapas vizinhos ficam ativos ao mesmo tempo, então eventos/missões nossas que contam "mapas do jogador" podem ver mais de um mapa. O Seamless World está **desativado no modpack** por um bug a corrigir depois.
