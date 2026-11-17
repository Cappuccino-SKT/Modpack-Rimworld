# Mapa-mundi

## Marcos do mundo (bloco N)

- Arquivo: `Mods/EcosDaQueda/Defs/World/EQ_Landmarks.xml`. 10 `LandmarkDef` `EQ_Marco_*` + 10 `RulePackDef` `EQ_NamerMarco_*`.
- `LandmarkDef` (vanilla, só com Odyssey): `mutatorChances` (precisa de ao menos um com chance ≥ 1 ou `Required`), `nameMaker` (raiz `r_name`, gerado em `WorldLandmarks.AddLandmark`), `commonality`, `iconTexturePath`, `category`. A `description` do Def aparece como dica no nome do marco na aba Terreno (`WITab_Terrain`).
- Geração (`WorldGenStep_Landmarks`): ~0,22% dos tiles × densidade escolhida; sorteio por `commonality` entre os defs com tile válido. Com o modpack há 137 defs (vanilla + VEE + nossos); num mundo de teste saíram 266 marcos, 26 nossos.
- `MayRequire` com vários mods separados por vírgula funciona em Def de topo (`ModLister.AllModsActiveNoSuffix`). Marcos com mutador do VEE exigem `vanillaexpanded.vexploratione`; os poços (VEE_FleshPits) também Anomaly.
- Não há regra `[Digit]` no jogo: os namers usam a regra própria `dig` (1–9).
- Estruturas antigas (AncientWarehouse, AncientInfestedSettlement, AncientLaunchSite, AncientGarrison, AncientChemfuelRefinery) usam o mesmo mutador dos marcos vanilla; as missões de estrutura antiga leem `Landmark.name`, então mostram o nosso nome.
- Os 59 marcos do VEE seguem em inglês (sem tradução PT-BR).
