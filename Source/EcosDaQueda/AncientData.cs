using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Bloco 3d: dados antigos (manual técnico, disco do Velho Mundo). Cada item vem ligado a um projeto fora da bancada
    /// (Velho Mundo ou patente de potência), sorteado quando o item nasce. Lido por um colono, libera o projeto e dá parte
    /// da pesquisa; sem os pré-requisitos para em 99% (como a engenharia reversa). Patente de potência sem licença dá Infâmia.
    /// </summary>
    public class CompProperties_UseEffectAncientData : CompProperties_UseEffect
    {
        /// <summary>Fração do custo do projeto que a leitura rende.</summary>
        public float fraction = 0.4f;
        public TechLevel minTech = TechLevel.Undefined;
        public TechLevel maxTech = TechLevel.Archotech;
        /// <summary>Peso do Velho Mundo no sorteio (patentes de potência pesam 1).</summary>
        public float oldWorldWeight = 3f;
        /// <summary>Peso extra de projeto ainda sem acesso.</summary>
        public float lockedWeight = 2f;
        public float infamy = 2f;
        public string text;
        public string capText;
        public string piracyText;
        public string knownText;
        public string corruptLabel;

        public CompProperties_UseEffectAncientData() => compClass = typeof(CompUseEffect_AncientData);
    }

    public class CompUseEffect_AncientData : CompUseEffect
    {
        public ResearchProjectDef project;
        private bool rolled;

        public CompProperties_UseEffectAncientData Props => (CompProperties_UseEffectAncientData)props;

        public override void PostPostMake()
        {
            base.PostPostMake();
            Roll();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!rolled)
                Roll();
        }

        /// <summary>Sorteia o projeto do item (só uma vez). Fora do jogo, fica para o primeiro spawn.</summary>
        public void Roll(bool force = false)
        {
            if ((rolled && !force) || Find.ResearchManager == null)
                return;
            rolled = true;
            project = Pick(Props);
        }

        public static IEnumerable<ResearchProjectDef> Pool(CompProperties_UseEffectAncientData p)
        {
            foreach (KeyValuePair<ResearchProjectDef, ResearchBranchDef> kv in EQResearch.BranchOf)
            {
                ResearchProjectDef r = kv.Key;
                if (kv.Value == null || kv.Value.free || r.knowledgeCategory != null || r.baseCost <= 0f || r.IsFinished)
                    continue;
                if (r.techLevel < p.minTech || r.techLevel > p.maxTech)
                    continue;
                yield return r;
            }
        }

        public static ResearchProjectDef Pick(CompProperties_UseEffectAncientData p)
        {
            List<ResearchProjectDef> pool = Pool(p).ToList();
            if (pool.Count == 0)
                return null;
            return pool.RandomElementByWeight(r => (EQResearch.Branch(r)?.faction == null ? p.oldWorldWeight : 1f)
                * (EQResearch.HasAccess(r) ? 1f : p.lockedWeight));
        }

        public override string TransformLabel(string label)
        {
            if (project != null)
                return label + " (" + project.label + ")";
            return rolled && !Props.corruptLabel.NullOrEmpty() ? label + " (" + Props.corruptLabel + ")" : label;
        }

        public override string CompInspectStringExtra()
        {
            if (project == null)
                return null;
            ResearchBranchDef b = EQResearch.Branch(project);
            string s = "Projeto: " + project.LabelCap + " (" + (b?.label ?? "?") + ", " + project.ProgressPercent.ToStringPercent() + ")";
            if (project.IsFinished)
                s += " — já conhecido";
            return s;
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (project == null)
                return "Dados corrompidos: não ensinam nada";
            if (project.IsFinished)
                return Props.knownText.NullOrEmpty() ? "Já conhecido" : Props.knownText;
            if (p.skills?.GetSkill(SkillDefOf.Intellectual)?.TotallyDisabled ?? true)
                return "Incapaz de trabalho intelectual";
            return base.CanBeUsedBy(p);
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            EQAncientData.Apply(project, Props, parent.LabelNoCount, usedBy);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref project, "eqProject");
            Scribe_Values.Look(ref rolled, "eqRolled");
        }
    }

    /// <summary>Onde os dados aparecem na geração de mapa: perto de marcadores (prédios antigos) ou em marcos nossos.</summary>
    public class AncientDataSpawnDef : Def
    {
        /// <summary>Prefixos de defName que contam como marcador (ex.: "VQEA_").</summary>
        public List<string> markerPrefixes = new List<string>();
        public List<ThingDef> markers = new List<ThingDef>();
        public int minMarkers = 1;
        /// <summary>Marcos (LandmarkDef, por defName) em que a regra vale sem marcador.</summary>
        public List<string> landmarks = new List<string>();
        public float chance = 0.5f;
        public IntRange count = new IntRange(1, 1);
        /// <summary>Itens possíveis (peso por item).</summary>
        public List<ThingDefCountClass> items = new List<ThingDefCountClass>();
        public bool allowPlayerHome;
    }

    [StaticConstructorOnStartup]
    public static class EQAncientData
    {
        /// <summary>Último relatório de geração (para o executor de testes).</summary>
        public static string LastSpawnReport = "-";

        /// <summary>Executor de testes: ignora o sorteio de chance das regras.</summary>
        public static bool ForceSpawn;

        public static void Apply(ResearchProjectDef r, CompProperties_UseEffectAncientData s, string itemLabel, Pawn usedBy)
        {
            if (r == null || r.IsFinished)
                return;
            LookTargets look = usedBy != null && usedBy.Spawned ? new LookTargets(usedBy) : LookTargets.Invalid;
            if (EQResearch.Branch(r) != null && !EQResearch.HasAccess(r))
                WorldComponent_EQResearch.Current?.Grant(r);
            float amount = s.fraction * r.Cost;
            bool capped = EQResearchGain.AddCapped(r, ref amount, EQReverse.Settings.capWithoutPrereqs);
            if (!s.text.NullOrEmpty())
                Messages.Message(s.text.Formatted((usedBy?.LabelShort ?? "?").Named("PAWN"), itemLabel.Named("ITEM"), r.LabelCap.Named("PROJECT"),
                    (amount / r.Cost).ToStringPercent().Named("GAIN"), r.ProgressPercent.ToStringPercent().Named("TOTAL")), look, MessageTypeDefOf.PositiveEvent, false);
            if (capped && !s.capText.NullOrEmpty())
                Messages.Message(s.capText.Formatted(r.LabelCap.Named("PROJECT")), look, MessageTypeDefOf.NeutralEvent, false);
            EQResearchGain.Piracy(r, s.infamy, s.piracyText, itemLabel, look);
        }

        /// <summary>Espalha dados antigos num mapa recém-gerado conforme as regras AncientDataSpawnDef.</summary>
        public static void OnMapGenerated(Map map)
        {
            if (map == null || Current.ProgramState == ProgramState.Entry)
                return;
            List<string> report = new List<string>();
            LandmarkDef lm = null;
            try { lm = map.Tile.Valid ? Find.World?.landmarks?[map.Tile]?.def : null; } catch { }
            List<Thing> all = map.listerThings.AllThings;
            foreach (AncientDataSpawnDef rule in DefDatabase<AncientDataSpawnDef>.AllDefsListForReading)
            {
                if (map.IsPlayerHome && !rule.allowPlayerHome)
                    continue;
                List<Thing> found = all.Where(t => t.def.category == ThingCategory.Building
                    && (rule.markers.Contains(t.def) || rule.markerPrefixes.Any(p => t.def.defName.StartsWith(p)))).ToList();
                bool byLandmark = lm != null && rule.landmarks.Contains(lm.defName);
                if (found.Count < rule.minMarkers && !byLandmark)
                    continue;
                if (!ForceSpawn && !Rand.Chance(rule.chance))
                {
                    report.Add(rule.defName + ":sorteio_falhou");
                    continue;
                }
                int n = rule.count.RandomInRange;
                int placed = 0;
                for (int i = 0; i < n; i++)
                {
                    ThingDefCountClass pick = rule.items.Where(x => x.thingDef != null).RandomElementByWeightWithFallback(x => x.count);
                    if (pick == null)
                        break;
                    IntVec3 center = found.Count > 0 ? found.RandomElement().Position : map.Center;
                    if (!CellFinder.TryFindRandomCellNear(center, map, found.Count > 0 ? 4 : 30,
                            c => c.Standable(map) && c.GetFirstItem(map) == null,
                            out IntVec3 cell))
                        continue;
                    Thing t = ThingMaker.MakeThing(pick.thingDef);
                    if (GenPlace.TryPlaceThing(t, cell, map, ThingPlaceMode.Near))
                        placed++;
                }
                report.Add(rule.defName + ":" + placed + (byLandmark ? "(marco " + lm.defName + ")" : "(marcadores " + found.Count + ")"));
            }
            if (report.Count > 0)
                LastSpawnReport = string.Join(" ", report);
        }
    }

    /// <summary>Progresso de pesquisa com teto sem pré-requisitos e pirataria de patente (usado pela engenharia reversa e pelos dados).</summary>
    public static class EQResearchGain
    {
        /// <summary>Soma <paramref name="amount"/> ao projeto; sem os pré-requisitos, para em cap×custo. Devolve true se parou no teto.</summary>
        public static bool AddCapped(ResearchProjectDef r, ref float amount, float cap)
        {
            ResearchManager rm = Find.ResearchManager;
            bool capped = false;
            if (!r.PrerequisitesCompleted)
            {
                // FinishProject concluiria os pré-requisitos de graça: para antes e libera o resto na bancada.
                float before = rm.GetProgress(r);
                float max = r.Cost * cap;
                if (before + amount >= max)
                {
                    amount = Mathf.Max(0f, max - before);
                    capped = true;
                }
            }
            rm.AddProgress(r, amount);
            return capped;
        }

        /// <summary>Patente de potência sem licença: Infâmia na dona.</summary>
        public static void Piracy(ResearchProjectDef r, float infamy, string text, string itemLabel, LookTargets look)
        {
            ResearchBranchDef b = EQLicenses.OwnedBranch(r);
            if (b == null || infamy <= 0f || EQLicenses.IsLicensed(r))
                return;
            Faction owner = EQUtil.MainFaction(b.faction);
            if (owner == null || owner.defeated || owner == Faction.OfPlayer)
                return;
            WorldComponent_Reputation.Current?.Record(owner, 0f, infamy);
            if (!text.NullOrEmpty())
                Messages.Message(text.Formatted(owner.Named("FACTION"), itemLabel.Named("ITEM"), infamy.ToString("0.#").Named("INFAMY")),
                    look, MessageTypeDefOf.NegativeEvent, false);
        }
    }

    [HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
    public static class Patch_MapGenerator_GenerateMap_AncientData
    {
        public static void Postfix(Map __result)
        {
            try
            {
                EQAncientData.OnMapGenerated(__result);
            }
            catch (System.Exception e)
            {
                Log.Warning("[Ecos da Queda] dados antigos na geração do mapa: " + e);
            }
        }
    }
}
