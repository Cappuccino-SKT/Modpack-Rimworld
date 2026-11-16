# Xenotipos

## Xenotipos próprios (`Defs/Xenotypes/EQ_Xenotypes.xml`, gerado)

As potências usam **só xenotipos nossos** (`EQ_Xeno_*`); os do jogo e dos mods ficam fora das facções, do Soro e da ideologia.
- **Fonte:** os genes vêm dos `.xtp` salvos no editor de xenotipo do jogo pelo usuário, copiados para `EcosDaQueda-Dev/xenotipos/xtp/`; defName, textos, herança, `combatPowerFactor` e padrões ficam em `xenotipos/xenotipos.json`.
- **Gerar:** `python -X utf8 -I EcosDaQueda-Dev/scripts/xenotipos.py` na raiz do jogo (`--genes` lista os genes com rótulo e mod de origem). Cada gene de mod ganha `MayRequire` com o packageId de quem o define; genes de aptidão (`Aptitude*_<Skill>`, `VRE_AptitudePhenomenal_*`) são gerados por `GeneTemplateDef` e o script os reconhece pelo modelo. **Não editar o XML à mão.**
- **Padrões x elites:** padrões são hereditários (endogenes). Elites não são (xenogenes) e trazem `doubleXenotypeChances` com os padrões aceitos, somando 1; o `PawnGenerator` aplica o padrão e depois o elite por cima (`SetXenotype` não limpa endogenes), mecanismo vanilla do Sanguófago. Sem C# para NPCs.
- **Facções:** `xenotypeSet` da FactionDef só com padrões (vale para emissários, aprendizes, líderes sem xeno próprio e PawnKinds de outros mods). PawnKinds de elite usam `useFactionXenotypes=false` + `xenotypeSet` próprio somando 1 (o jogo completa a soma com Baseliner).
- **Soro:** sucesso troca só os xenogenes (o padrão fica); falha dá o Lote Descontinuado, hereditário, que limpa os endogenes antes (`EQXenotypeUtil.ClearEndogenesKeepingColors`, mantém cor de pele/cabelo).
- **Mundo todo:** `Patch_PawnGenerator_GenerateGenes` (Prefix, `Xenotypes.cs`) troca **qualquer xeno que não seja `EQ_`** (Baseliner e os de outros mods) por um padrão sorteado de `EQ_WorldXenotypes` (`Defs/Xenotypes/EQ_WorldXenotypes.xml`: Vagante 80%, Esquelético 15%, Antigo 5%). Exceções: lista `keepXenotypes` do mesmo def (Sanguophage, por causa das missões do Biotech; androides do VRE Android), `ForcedXenotype` e `ForcedCustomXenotype` (xeno personalizado do jogador). Os XenotypeDefs de terceiros continuam existindo (editor, xenogermes, preceitos); só não são mais gerados em pawns.
- **Teste:** `tests/xenotipos.txt` (perfil full); comando `xenogen <PawnKind|fac:nome> [n]` conta xeno, padrão (`EQXenotypeUtil.GuessBase`) e gênero.

## Xenotipos de terceiros disponíveis (não usados pelas potências)

Vanilla: Baseliner, Dirtmole, Genie, Hussar, Sanguophage, Neanderthal, Pigskin, Impid, Waster, Yttakin, Highmate, Starjack.
Alpha Genes: AG_Fleetkind, AG_Helixien, AG_Animusen, AG_Lapis, AG_Wretch, AG_Taukai, AG_Hiveling, AG_MindDevourer, AG_Efreet, AG_Drakonori, AG_Nereid, AG_Forsaken.
VRE: VRESaurids_Saurid, VREH_Uhlan, VRE_Poluxkin, VRE_Strigoi, VRE_Ekkimian, VRE_Bruxa, VREA_AndroidAwakened, VREA_AndroidBasic, VRE_Lowmate, VRE_Fungoid, VRE_Wolfman, VRE_Lycan, VRE_Boarskin, VRE_Insector, VRE_Ocularkin, AG_Mycormorph, AG_Malachai.
