# Notas técnicas

## Notas por assunto

Ler **só o arquivo do assunto da tarefa**. Nota nova vai no arquivo do assunto (criar outro se não couber em nenhum).

| Arquivo | Assunto | Seções |
|---|---|---|
| `tecnico/codigo.md` | Código C# e mod de debug | C#; Mod de debug; Fase 2 |
| `tecnico/traducao.md` | Tradução PT-BR | Tradução PT-BR |
| `tecnico/faccoes.md` | Facções, ideologias, unidades, Hegemony e cerimônia | Ícones e cores próprios; Facções; Ideologias fixas por arquivo; Facções menores; PawnKinds; Hegemony; Cerimônia de outorga e nível psíquico |
| `tecnico/xenotipos.md` | Xenotipos | Xenotipos próprios; Xenotipos de terceiros disponíveis |
| `tecnico/inicio.md` | Início de jogo (cenários, xenos, narrador) | Início de jogo |
| `tecnico/arsenal-comercio.md` | Arsenal, guarda-roupa e comércio por facção | Tags de armas úteis; Arsenal por facção; Comércio por facção |

## Documentação dos mods do modpack

Guias de frameworks, mecânicas e conteúdo, mais referência de API gerada das DLLs: `EcosDaQueda-Dev/mods/README.md`. Ferramentas: `scripts/mods-inventario.py` (inventário) e `scripts/mods-api.py` (referência de API a partir do descompilado).

## Ambiente

- **RimWorld 1.7.5000 rev1275**, cópia **não-Steam**, em `C:\Users\Felipe\Desktop\RimWorld`.
- Todas as DLCs: Royalty, Ideology, Biotech, Anomaly, Odyssey.
- **Motor Waylith** (o 1.7 trocou Unity por motor próprio, .NET 10 + Vulkan). A API do Unity é reimplementada em `UnityStubs.dll`; há camadas de compatibilidade para Harmony e Prepatcher (`RimWorldWaylith.PrepatcherCompat.dll`). DLLs de mods compiladas para 1.6 geralmente funcionam.
- Mods baixados via **SteamCMD anônimo** (`C:\Users\Felipe\steamcmd`). Lista: `scripts/steamcmd-mods.txt` (cópia de `C:\Users\Felipe\steamcmd\mods.txt`). Download: `C:\Users\Felipe\steamcmd\steamcmd.exe +runscript C:\Users\Felipe\steamcmd\mods.txt`, depois copiar `steamapps\workshop\content\294100\*` para `Mods\`.
- Pastas de mods ficam nomeadas pelo **ID da Workshop** (ex.: `Mods\3825320105` = Hegemony). O mod do projeto é `Mods\EcosDaQueda`.
- `Mods_desativados\` guarda versões antigas substituídas (Replace Stuff original, Cherry Picker original).
- Dados do usuário: `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\` (`Config\ModsConfig.xml`, `Logs\Player.log`, `Saves\`).
- .NET SDK 8 e 10 instalados; `ilspycmd` instalado como dotnet tool global (`%USERPROFILE%\.dotnet\tools`).

## Como o jogo escolhe pastas de versão (decompilado de `Verse.ModContentPack.InitLoadFolders`)

1. Se o mod tem `LoadFolders.xml` com versões definidas: usa a entrada da versão atual (`v1.7`); senão, a **maior versão ≤ atual** listada; senão `default`.
2. Sem LoadFolders: usa a pasta `1.7` se existir; senão a **maior pasta de versão ≤ 1.7**; depois `Common/` e a raiz.
3. `supportedVersions` sem 1.7 só gera **aviso amarelo** — o mod carrega normalmente.

Por isso mods sem suporte declarado a 1.7 carregam o conteúdo 1.6 automaticamente.

## Estrutura do mod

```
Mods/EcosDaQueda/
  About/About.xml            packageId ecosdaqueda.core, supportedVersions 1.6/1.7
  LoadFolders.xml            "/" sempre; "Mods/Hegemony" só se jaeger972.factionterritories2 ativo
  Defs/Memes/EQ_Memes.xml    4 memes-assinatura (exclusionTag EQ_SignatureMeme)
  Defs/Factions/EQ_Factions.xml   base abstrata EQ_MainFactionBase + 4 FactionDefs
  Defs/PawnKinds/EQ_PawnKinds.xml 16 PawnKindDefs
  Defs/RulePacks/EQ_Namers.xml    nomes de assentamentos (com pesos p= e numeração de reserva)
  Defs/Ideo/EQ_IdeoSymbols.xml    ícones (IdeoIconDef) e cores (ColorDef) das ideologias
  1.7/Assemblies/EcosDaQueda.dll  C# (só 1.7)
  1.7/Defs/EQ_IdeoTemplates.xml   modelos fixos de ideologia (IdeoTemplateDef)
  1.7/Defs/EQ_FactionRelations.xml  matriz de relações entre as potências
  1.7/Defs/EQ_Reputation.xml      reputação (Fama/Infâmia, títulos) + aba Reputação
  Mods/Hegemony/Defs/EQ_HegemonyDispositions.xml
  Mods/Hegemony/1.7/Assemblies/EcosDaQueda.Hegemony.dll  vetos do Hegemony
  Source/EcosDaQueda/             código C# principal (.csproj, .cs)
  Source/EcosDaQueda.Hegemony/    código C# da integração com o Hegemony
  Textures/EcosDaQueda/Icons/     ícones gerados
```

Convenções: prefixo **`EQ_`** em todos os defNames; textos em PT-BR; dependências opcionais sempre com `MayRequire="<packageId>"`.

## packageIds úteis

| Mod | packageId |
|---|---|
| Harmony | brrainz.harmony |
| Vanilla Expanded Framework | OskarPotocki.VanillaFactionsExpanded.Core |
| Vanilla Psycasts Expanded | VanillaExpanded.VPsycastsE |
| VWE Laser / Coilguns / Bioferrite / Makeshift | VanillaExpanded.VWEL / VWEC / VWEBF / VWEMS |
| Vanilla Memes (VIE) | VanillaExpanded.VMemesE |
| Alpha Memes / Alpha Genes | Sarg.AlphaMemes / sarg.alphagenes |
| VRE Saurid / Lycanthrope / Hussar / Sanguophage / Fungoid | vanillaracesexpanded.saurid / .lycanthrope / .hussar / .sanguophage / .fungoid |
| Hegemony | jaeger972.factionterritories2 |
| Jaeger's Utility Framework | jaeger972.utilityframework |
| Combat Extended | CETeam.CombatExtended |

Para descobrir outro: `[xml]$x=Get-Content Mods\<id>\About\About.xml; $x.ModMetaData.packageId` (o grep simples pega o packageId das dependências por engano).

