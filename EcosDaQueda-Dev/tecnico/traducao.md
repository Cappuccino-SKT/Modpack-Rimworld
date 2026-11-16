# Tradução PT-BR

## Tradução PT-BR — `Mods/EcosDaQuedaTraducao` (`ecosdaqueda.traducao`)

Traduz mods de terceiros sem PT-BR sem tocar neles. Cada mod traduzido fica em `Mods/<packageId>/Languages/PortugueseBrazilian/` e o `LoadFolders.xml` (gerado) carrega a pasta só com `IfModActive`.

Fluxo (ferramenta `scripts/traducao.py`; textos-fonte em `traducao/`, versionados na `main`):
1. `run-test.ps1 -Script tests/trans-export.txt -Profile full -Exclude ecosdaqueda.traducao` → `trans-export/<id>/DefInjected/...` (inglês; usa a mesma travessia do limpador de traduções do jogo, `DefInjectionUtility`). A tradução precisa estar **desativada** na exportação, senão sai em português.
2. `python -I traducao.py dump <ids>` → `traducao/en/<id>.tsv` (+ Keyed em inglês do mod).
3. Traduzir em `traducao/pt/<id>.tsv` ou várias partes em `traducao/pt/<id>/*.tsv` (mesmas chaves). Item de lista sem tradução fica em inglês; chave ausente fica em inglês.
4. `python -I traducao.py build` → gera o mod e confere marcadores (`{0}`, `[PAWN_nameDef]`, `<color>`, `->`; pronomes podem mudar). `todo <id>` lista o que falta (ex.: depois de update do mod).
5. `tests/traducao.txt` (perfil full) confere injeção e erros.

Armadilhas descobertas:
- **Pastas de carga são camadas**: arquivos com o mesmo caminho relativo em pastas diferentes se sobrescrevem. Por isso o build prefixa cada arquivo com o packageId (`sarg_alphagenes__Genes.xml`).
- A pasta do tipo de def é achada por `GenTypes.GetTypeInAnyAssembly`: tipos de mods fora de `Verse`/`RimWorld` precisam do **nome completo** (`AlphaGenes.AnimalGeneTemplateDef`, `VEF.Pawns.PregnancyApproachDef`). O export já gera assim.
- Genes gerados por modelo (`*GeneTemplateDef`) pegam o texto traduzido do modelo; defs `generated` não são exportados.
- **`generalRules` de pesquisa dos mods VE sem regras no XML** (VPE, VRE Android) são preenchidas por código do VEF *depois* da injeção → não dá para traduzir por DefInjected ("Object reference not set"). Ficam fora; o `todo` continua listando essas chaves (é esperado).
- **Keyed com a mesma chave do jogo** (ex.: `NotAllowed`, `Hybrid` no Vehicle Framework) ficam fora: o PT do jogo já cobre e repetir dá chave duplicada.
- `deftext` aceita nome curto ou completo do tipo; com nomes repetidos (`AbilityDef` do jogo e do VEF) usa o tipo que tem o def.
- Não usar `Get-Content`/`Set-Content` do PowerShell 5.1 em arquivos UTF-8 sem BOM: lê como ANSI e estraga os acentos.
- A referência do Workshop (coleção 3018573588, baixada com SteamCMD) é fraca (tradução automática, pt-PT, termos errados): serve só de consulta; o texto é nosso (repo público).
- Glossário (seguir a Biotech oficial): baseliner = humano padrão, xenogerm = xenogerminador, germline = linhagem germinativa, hemogen = hemogênio, deathrest = descanso mortal, luciferium = lucifério, chemfuel = combustível químico, wastepack = pacote de resíduos tóxicos, shambler = trôpego, ghoul = carniçal, psylink = psínculo, "Carriers of this gene" = "Portadores deste gene". Royalty/psíquico: psyfocus = concentração psíquica, neural heat = calor neural, psycast = poder psíquico, psycaster = conjurador, skip/skipgate = salto/portal de salto, eltex = eltex. Caminhos do VPE: cronopata, senhor da estática, conflagrador, moldador do gelo, voz da natureza, harmonista, protetor, senhor da guerra, arcotecnicista, necropata, mestre do salto, tecnomante, empata, arconte, espreitador noturno. Veículos: tier = nível, pawn = personagem, c/s = q/s, tile = região; nomes de modelo (Bulldog, Tango...) ficam em inglês, menos Bote e Moto de trilha. Títulos imperiais: os oficiais (Império Fragmentado, arconte, dominus, arquistelar, janissário, catafrata).
