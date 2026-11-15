# Hegemony — Faction Territories and Vassalage 2 (FT2)

- **packageId:** `jaeger972.factionterritories2` · pasta `Mods/3825320105` · DLL `FactionTerritories2.dll` (~1.165 classes, namespace `FactionTerritories2`)
- **Dependência:** [Jaeger's Utility Framework](bibliotecas-csharp.md) (`jaeger972.utilityframework`).
- **Guia oficial do autor (1.222 linhas, muito detalhado):** `Mods/3825320105/MODDER_GUIDE.md`. Esta página resume o que importa ao Ecos; para detalhes, ir direto às seções citadas.
- **Notas antigas nossas:** TECNICO.md › Hegemony. **Nossa integração:** `Mods/EcosDaQueda/Mods/Hegemony/` (defs de disposição + `EcosDaQueda.Hegemony.dll` com vetos), carregada por `IfModActive`.

É a **camada de estratégia** do modpack (decisão do projeto: Hearts of RimWorld e Exploration Mode desativados). Facções têm território no mapa-múndi, assentamentos com economia (comida, materiais, prosperidade, população), prédios abstratos, prefeitos, guerras, vassalagem, invasões reais entre facções, comércio com estoque e preço local.

## Escolher a abordagem (do guia, § "Choose an integration approach")

| Objetivo | Use | Evite |
|---|---|---|
| Fazer um meme/traço/estrutura influenciar a IA | `FactionDispositionDef` nova para chave sem mapeamento; patch se já existe | Dois mapeamentos para a mesma chave (não somam) |
| Bônus de bioma/marco | Def de efeito + extensão de origem | Recalcular território ou editar nível salvo |
| Produção de assentamento | `BuildingDef` (campos de produção e tags) | Achar que XML cria enum novo de especialista |
| Comércio próprio | `HegemonyApi` (preço, recebimento, `RecordTrade`) | Reflexão em campos internos |
| Janela/opções | `SharedUI` do Jaeger's Utility Framework | Copiar controles |
| Ação diplomática nova | Pedir extensão ao autor ou patch Harmony bem estreito | Supor que há registro/evento genérico |

## 1. Personalidade da facção — `FactionTerritories2.FactionDispositionDef`

Mapeia **um** traço de líder (`trait`) ou **um** meme (`meme`) — defName exato, sensível a maiúsculas — para preferências. Memes e traços sem mapeamento **não influenciam nada**. Hegemony já traz mapeamentos para Alpha Memes (41) e VIE Memes (50) em `Defs/Compatibility/`, nomeados `FT2_Disposition_meme_<defName>`: para mudar, **patch** neles.

| Campos | Significado |
|---|---|
| `aggression`, `peace`, `diplomacy`, `trade`, `expansion`, `support` | Tendências; **somam como desvio de 1** (`trade=0.3` → ×1.3); final limitado a 0.25–3 |
| `vassalise`, `plunder`, `raze` | Peso da escolha após vitória; 0.05–100 |
| `buildingTags` (`tag`, `weight`) | Preferência de construção por tag; final 0.1–8 |
| `scaleDegree` | Só traços: multiplica pelo grau (deixar false para traços de grau 0) |
| Efeitos de assentamento (herdados de `SettlementEffectDef`) | `food`, `materials`, `prosperity`, `defence`, `research`, `recovery`, `growth`, `upkeep`, `construction`, `happiness`, `wood`, `stone`, `metal`, `components`… (unidades próprias) |

Tags conhecidas: animals, authoritarian, bionics, civic, civilian, culture, dangerous, defence, diplomacy, education, evil, food, genetics, good, industry, logistics, luxury, mech, medical, military, mining, nature, occult, organic, religious, royal, science, space, trade (tags novas são aceitas, mas só têm efeito se algum perfil der peso).

Nós já mapeamos os 4 memes-assinatura (`EQ_HegemonyDispositions.xml`). Ao criar meme novo, criar o mapeamento junto.

## 2. Prefeitos — § "Give mayor traits and roles settlement effects"

`MayorTraitEffectDef` (traço do prefeito → efeitos no assentamento) e `MayorRoleDef` (cargos). Útil para dar às nossas potências prefeitos com traços temáticos que mudam a economia local.

## 3. Prédios de assentamento — `FactionTerritories2.BuildingDef`

Abstratos (não são `ThingDef`). Campos: `minimumTech`, `materialCost`, `prosperityCost`, `costGrowth`, `dailyFood`/`dailyMaterials`/`dailyProsperity` (por nível), `materialOutput` (`Wood`, `Stone`, `Metal`, `MedievalComponents`, `IndustrialComponents`, `UltraComponents`, `ArchotechComponents`), `housing`, `defence`, `happiness`, `tags`, `tierLabels` (6 nomes: Neolítico → Arcotec), `initialChance` (peso na geração do mundo), `grand` (+ `automaticUpgrades`), `territoryDecayReduction`, `siteRequirement`, `resources`/`siteGoods` (bens físicos de dízimo/estoque), `special` (só enums compilados existentes).

Exemplo temático: "Arquivo Silencioso" (tags `occult`, `science`) para o Silêncio; "Clínica de Garantia" (tags `genetics`, `medical`) para a Hélice. As tags casam com as `buildingTags` das disposições → a IA da potência prefere construir.

## 4. Biomas, marcos, estradas, crise mecanoide

- `BiomeEffectDef`, `SiteEffectDef` + extensão `SiteEffectExtension` (§ "Map biomes and landmarks").
- `RoadTechnologyExtension` (§ "Declare road technology").
- `MechanoidCrisisExtension` (§ "Opt a faction into the mechanoid crisis").

## 5. API pública em C# — `FactionTerritories2.HegemonyApi`

Chamar na thread do jogo com mundo carregado; checar `IsAvailable`. `ApiVersion` em tempo de execução (hoje 5; APIs 1–4 continuam).

| Membro | Uso |
|---|---|
| `TryGetSettlementState(Settlement, out SettlementState)` | Estado do assentamento (só leitura, nunca cria economia) |
| `Price`/`TryGetPrice(state, ThingDef, …)` | Preço local em prata |
| `GetReserve`, `GetTechnology`, `GetOwner` | Reservas, tecnologia efetiva, dono político |
| `CanCredit`, `TryQuoteContribution`, `Credit`/`TryCredit` | Entregar bens físicos ao assentamento |
| `RecordTrade(Faction, Faction, double)` | Registrar venda customizada |
| `IsDiplomaticVisitor(Pawn)` | Emissários e coletores de imposto |
| `TryDeclareWar(provider, id, attacker, defender, objective, …, out reason)` | Guerra pelo livro-razão do Hegemony |
| `TryMakePeace(provider, id, a, b, out reason)` | Paz negociada (respeita vetos e travas) |
| `TryTransferSettlement(provider, id, settlement, owner, overlord, reasonKind, out reason)` | Mudar dono de assentamento (repetir o **mesmo id** se precisar concluir) |
| `AreAtWar`, `GetWars`, `GetOverlord`, `IsPlayerVassal`, `GetTerritoryClaims`, `GetTerritoryTileCount` | Leitura |
| `SubscribeToEvents(Action<HegemonyEvent>)`, `ReadEvents(cursor)` | Eventos (posse, guerra, invasão, facção criada/destruída, território). Diário salvo guarda os 1024 últimos; guardar o cursor no nosso save |
| `RegisterVeto(HegemonyVetoKind, Func<HegemonyVetoContext,string>)` | Bloquear decisões da IA (guerra, paz, alvo de invasão…): devolver texto = recusa; null = permite. **Já usamos** para manter a matriz de relações das potências |

Regras do autor: callbacks curtos e só de leitura; registros não são salvos (registrar a cada carga e descartar); no máximo 128 assinaturas e 64 vetos por tipo; `provider`+`id` estáveis para operações idempotentes. Detalhes: guia § "Lifecycle, writes and actual supplied forces (API 5)".

## Armadilhas

- Tabelas de mapeamento ficam em cache: **reiniciar o jogo** depois de mudar.
- Para mudar campo de def do Hegemony: patch em `Patches/` (`PatchOperationReplace`/`Add`), nunca redefinir o def.
- Adicionar extensão sem apagar as de outros mods: § "Add an extension without discarding other mods' extensions".
- Guia § "Harmony and internal boundaries": não patchear internos sem necessidade; o autor muda versão com frequência (0.209.x).
- Requer versões casadas de Hegemony e do Utility Framework (ex.: Hegemony 0.209.11+ exige framework 1.13.0). Ao atualizar um, atualizar o outro.
