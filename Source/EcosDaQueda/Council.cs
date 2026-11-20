using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Uma cadeira do conselho (N2, NACAO.md › 3.4).</summary>
    public class CouncilSeat
    {
        public string key;
        public string label;
        [MustTranslate] public string description;
        public List<SkillDef> skills = new List<SkillDef>();
        /// <summary>true: média das perícias; false: a maior.</summary>
        public bool averageSkills;
        /// <summary>Vale também com o conselheiro numa caravana (Embaixador).</summary>
        public bool allowCaravan;
    }

    /// <summary>N2: parâmetros do conselho. Valores propostos; ajustar no XML.</summary>
    public class CouncilDef : Def
    {
        public List<CouncilSeat> seats = new List<CouncilSeat>();
        /// <summary>Bônus 0 na perícia skillZero, cheio na skillFull.</summary>
        public float skillZero = 4f, skillFull = 15f;
        public float swapCooldownDays = 3f;

        // Embaixador
        public float goodwillGain = 0.20f;
        public float patronTensionSlowdown = 0.30f;
        public float ceremonyDiscount = 0.15f;
        // General
        public float armyStrength = 0.15f;
        public float revoltReduction = 0.20f;
        // Conselheiro
        public float frontierSpeed = 0.25f;
        public float tributeBonus = 0.25f;
        public int extraMarkers = 1;

        public ThoughtDef memberThought, passedOverThought;

        public static CouncilDef Active => DefDatabase<CouncilDef>.AllDefsListForReading.FirstOrDefault();
        public CouncilSeat Seat(string key) => seats.FirstOrDefault(s => s.key == key);
    }

    public class WorldComponent_EQCouncil : WorldComponent
    {
        public Dictionary<string, Pawn> seats = new Dictionary<string, Pawn>();
        public Dictionary<string, int> lastChange = new Dictionary<string, int>();
        private List<string> tmpKeys; private List<Pawn> tmpPawns; private List<string> tmpKeys2; private List<int> tmpInts;

        public WorldComponent_EQCouncil(World world) : base(world) { }

        public static WorldComponent_EQCouncil Current => Find.World?.GetComponent<WorldComponent_EQCouncil>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref seats, "seats", LookMode.Value, LookMode.Reference, ref tmpKeys, ref tmpPawns);
            Scribe_Collections.Look(ref lastChange, "lastChange", LookMode.Value, LookMode.Value, ref tmpKeys2, ref tmpInts);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (seats == null) seats = new Dictionary<string, Pawn>();
                if (lastChange == null) lastChange = new Dictionary<string, int>();
                foreach (string k in seats.Where(kv => kv.Value == null).Select(kv => kv.Key).ToList()) seats.Remove(k);
            }
        }

        public override void WorldComponentTick()
        {
            if (Find.TickManager.TicksGame % 2500 != 0 || seats.Count == 0) return;
            // Morto, fora da facção ou virou líder: a cadeira fica vazia (sem espera).
            foreach (var kv in seats.ToList())
            {
                Pawn p = kv.Value;
                if (p == null || p.Dead || p.Destroyed || p.Faction != Faction.OfPlayer || p == EQLeader.IdeoLeader)
                {
                    seats.Remove(kv.Key);
                    if (p != null && !EQLeader.SuppressDialogs)
                        Messages.Message("A cadeira de " + (CouncilDef.Active?.Seat(kv.Key)?.label ?? kv.Key) + " no conselho ficou vazia (" + p.LabelShort + ").", MessageTypeDefOf.NeutralEvent, false);
                }
            }
        }

        public Pawn Holder(string key) => seats.TryGetValue(key, out Pawn p) ? p : null;

        public float CooldownLeftDays(string key)
        {
            CouncilDef d = CouncilDef.Active;
            if (d == null || !lastChange.TryGetValue(key, out int t)) return 0f;
            return Mathf.Max(0f, d.swapCooldownDays - (Find.TickManager.TicksGame - t) / (float)GenDate.TicksPerDay);
        }

        /// <summary>null = feito; senão o motivo. p == null esvazia a cadeira.</summary>
        public string Assign(string key, Pawn p)
        {
            CouncilDef d = CouncilDef.Active;
            if (d?.Seat(key) == null) return "cadeira desconhecida: " + key;
            if (!EQNation.Founded) return EQCouncil.NotFoundedReason;
            if (Holder(key) == p) return null;
            float cd = CooldownLeftDays(key);
            if (cd > 0f && Holder(key) != null) return "Trocar de " + d.Seat(key).label + " tão cedo pega mal: espere " + cd.ToString("0.#") + " dias.";
            if (p != null)
            {
                string why = EQCouncil.CannotSit(p);
                if (why != null) return why;
                string other = seats.FirstOrDefault(kv => kv.Value == p).Key;
                if (other != null) return p.LabelShort + " já ocupa a cadeira de " + d.Seat(other).label + ".";
            }
            if (p == null) seats.Remove(key); else seats[key] = p;
            lastChange[key] = Find.TickManager.TicksGame;
            return null;
        }
    }

    public static class EQCouncil
    {
        public const string Ambassador = "embaixador", General = "general", Advisor = "conselheiro";
        public const string NotFoundedReason = "Sem nação, sem conselho: proclame um líder no trono e vença a revolta dos vizinhos.";

        public static string CannotSit(Pawn p)
        {
            if (p == null || p.Dead || p.Faction != Faction.OfPlayer || !p.IsFreeNonSlaveColonist || p.IsQuestLodger()) return "Precisa ser um colono livre.";
            if (!p.RaceProps.Humanlike || !DevelopmentalStageExtensions.Adult(p.DevelopmentalStage)) return "Precisa ser um adulto.";
            if (p == EQLeader.IdeoLeader) return "O líder não senta no próprio conselho; ele preside.";
            return null;
        }

        public static float SkillOf(Pawn p, CouncilSeat seat)
        {
            if (p?.skills == null || seat.skills.Count == 0) return 0f;
            var lv = seat.skills.Select(s => p.skills.GetSkill(s)).Select(s => s == null || s.TotallyDisabled ? 0f : s.Level).ToList();
            return seat.averageSkills ? lv.Average() : lv.Max();
        }

        /// <summary>Por que o conselheiro não está valendo agora (null = valendo).</summary>
        public static string Inactive(Pawn p, CouncilSeat seat)
        {
            if (p == null) return "vazia";
            if (p.Dead || p.Faction != Faction.OfPlayer) return "ausente";
            if (p.Downed) return "inconsciente";
            if (p.IsPrisoner) return "preso";
            if (p.GetCaravan() != null) return seat.allowCaravan ? null : "numa caravana";
            if (p.MapHeld?.IsPlayerHome != true) return "fora da colônia";
            return null;
        }

        /// <summary>0 a 1: quanto do bônus da cadeira vale agora.</summary>
        public static float Effect(string key)
        {
            CouncilDef d = CouncilDef.Active;
            var c = WorldComponent_EQCouncil.Current;
            if (d == null || c == null || !EQNation.Founded) return 0f;
            CouncilSeat seat = d.Seat(key);
            Pawn p = c.Holder(key);
            if (seat == null || p == null || Inactive(p, seat) != null) return 0f;
            return Mathf.Clamp01((SkillOf(p, seat) - d.skillZero) / Mathf.Max(1f, d.skillFull - d.skillZero));
        }

        // ------------------------------------------------------------ ganchos dos sistemas

        public static float GoodwillFactor => 1f + (CouncilDef.Active?.goodwillGain ?? 0f) * Effect(Ambassador);
        public static float PatronTensionFactor => 1f - (CouncilDef.Active?.patronTensionSlowdown ?? 0f) * Effect(Ambassador);
        public static float CeremonyFactor => 1f - (CouncilDef.Active?.ceremonyDiscount ?? 0f) * Effect(Ambassador);
        public static int CeremonyCount(int count) => count <= 1 ? count : Mathf.Max(1, Mathf.CeilToInt(count * CeremonyFactor));
        public static float ArmyFactor => 1f + (CouncilDef.Active?.armyStrength ?? 0f) * Effect(General);
        public static float RevoltFactor => 1f - (CouncilDef.Active?.revoltReduction ?? 0f) * Effect(General);
        public static float FrontierSpeedFactor => 1f + (CouncilDef.Active?.frontierSpeed ?? 0f) * Effect(Advisor);
        public static float TributeFactor => 1f + (CouncilDef.Active?.tributeBonus ?? 0f) * Effect(Advisor);
        /// <summary>Marco extra só com o bônus em pelo menos metade.</summary>
        public static int ExtraMarkers => Effect(Advisor) >= 0.5f ? (CouncilDef.Active?.extraMarkers ?? 0) : 0;

        public static string Report()
        {
            CouncilDef d = CouncilDef.Active;
            var c = WorldComponent_EQCouncil.Current;
            if (d == null || c == null) return "sem conselho";
            var parts = d.seats.Select(s =>
            {
                Pawn p = c.Holder(s.key);
                return s.label + "=" + (p == null ? "vazia" : p.LabelShort + " (perícia " + SkillOf(p, s).ToString("0") + ", " + (Effect(s.key) * 100f).ToString("0") + "%" + (Inactive(p, s) is string w ? ", " + w : "") + ")")
                       + (c.CooldownLeftDays(s.key) > 0f ? " espera=" + c.CooldownLeftDays(s.key).ToString("0.0") + "d" : "");
            });
            return "conselho: " + string.Join(" | ", parts)
                   + "\n  fatores: boa_vontade×" + GoodwillFactor.ToString("0.00") + " tensão_patrona×" + PatronTensionFactor.ToString("0.00")
                   + " cerimônia×" + CeremonyFactor.ToString("0.00") + " exército×" + ArmyFactor.ToString("0.00") + " revolta×" + RevoltFactor.ToString("0.00")
                   + " fronteira×" + FrontierSpeedFactor.ToString("0.00") + " tributo×" + TributeFactor.ToString("0.00") + " marcos+" + ExtraMarkers;
        }

        public static Command CouncilCommand()
        {
            var cmd = new Command_Action
            {
                defaultLabel = "Conselho",
                defaultDesc = "Quem aconselha o trono: Embaixador, General e Conselheiro. Cada cadeira dá um bônus que cresce com a perícia de quem senta nela.",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/ThroneSpeech", false) ?? BaseContent.BadTex,
                action = () => Find.WindowStack.Add(new Dialog_EQCouncil())
            };
            if (!EQNation.Founded) cmd.Disable(NotFoundedReason);
            return cmd;
        }
    }

    public class Dialog_EQCouncil : Window
    {
        public override Vector2 InitialSize => new Vector2(640f, 520f);

        public Dialog_EQCouncil()
        {
            doCloseButton = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            CouncilDef d = CouncilDef.Active;
            var c = WorldComponent_EQCouncil.Current;
            if (d == null || c == null) { Close(); return; }
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "Conselho de " + Faction.OfPlayer.Name);
            Text.Font = GameFont.Small;
            float y = 42f;
            Widgets.Label(new Rect(0f, y, inRect.width, 40f), "O líder (" + (EQLeader.IdeoLeader?.LabelShort ?? "ninguém") + ") preside e não ocupa cadeira. O bônus vale com o conselheiro vivo, consciente e na colônia.");
            y += 46f;
            foreach (CouncilSeat seat in d.seats)
            {
                Pawn p = c.Holder(seat.key);
                Rect row = new Rect(0f, y, inRect.width, 96f);
                Widgets.DrawMenuSection(row);
                Rect inner = row.ContractedBy(6f);
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(inner.x, inner.y, 260f, 30f), seat.label.CapitalizeFirst());
                Text.Font = GameFont.Small;
                string who = p == null ? "(vazia)" : p.LabelShort + " — perícia " + EQCouncil.SkillOf(p, seat).ToString("0") + ", bônus " + (EQCouncil.Effect(seat.key) * 100f).ToString("0") + "%"
                                                      + (EQCouncil.Inactive(p, seat) is string w ? " (" + w + ")" : "");
                Widgets.Label(new Rect(inner.x, inner.y + 30f, inner.width - 130f, 24f), who);
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(inner.x, inner.y + 52f, inner.width - 130f, 34f), seat.description);
                Text.Font = GameFont.Small;
                Rect btn = new Rect(inner.xMax - 120f, inner.y + 30f, 120f, 32f);
                float cd = c.CooldownLeftDays(seat.key);
                if (Widgets.ButtonText(btn, p == null ? "Nomear" : "Trocar"))
                {
                    var opts = new List<FloatMenuOption>();
                    if (p != null) opts.Add(new FloatMenuOption("Deixar vazia", () => Report(c.Assign(seat.key, null))));
                    foreach (Pawn cand in EQLeader.AllFreeColonists().Distinct().Where(x => EQCouncil.CannotSit(x) == null && x != p)
                                 .OrderByDescending(x => EQCouncil.SkillOf(x, seat)))
                    {
                        Pawn cc = cand;
                        string seatOf = c.seats.FirstOrDefault(kv => kv.Value == cc).Key;
                        string label = cc.LabelShort + " (" + EQCouncil.SkillOf(cc, seat).ToString("0") + ")" + (seatOf != null ? " — já é " + d.Seat(seatOf).label : "");
                        opts.Add(seatOf != null ? new FloatMenuOption(label, null) : new FloatMenuOption(label, () => Report(c.Assign(seat.key, cc))));
                    }
                    if (opts.Count == 0) opts.Add(new FloatMenuOption("Ninguém disponível", null));
                    Find.WindowStack.Add(new FloatMenu(opts));
                }
                if (cd > 0f && p != null)
                {
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(new Rect(btn.x, btn.yMax + 2f, 140f, 20f), "troca em " + cd.ToString("0.#") + " dias");
                    Text.Font = GameFont.Small;
                }
                y += 102f;
            }
        }

        private static void Report(string err)
        {
            if (err != null) Messages.Message(err, MessageTypeDefOf.RejectInput, false);
        }
    }

    /// <summary>+ "Membro do conselho".</summary>
    public class ThoughtWorker_EQCouncilMember : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            var c = WorldComponent_EQCouncil.Current;
            return c != null && p.Faction == Faction.OfPlayer && c.seats.ContainsValue(p) && EQNation.Founded;
        }
    }

    /// <summary>− "Preterido": tem perícia maior que o conselheiro de uma cadeira e ficou de fora.</summary>
    public class ThoughtWorker_EQCouncilPassedOver : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            var c = WorldComponent_EQCouncil.Current;
            CouncilDef d = CouncilDef.Active;
            if (c == null || d == null || c.seats.Count == 0 || p.Faction != Faction.OfPlayer || !p.IsFreeNonSlaveColonist || c.seats.ContainsValue(p) || p == EQLeader.IdeoLeader)
                return false;
            foreach (var kv in c.seats)
            {
                CouncilSeat seat = d.Seat(kv.Key);
                if (seat != null && kv.Value != null && EQCouncil.SkillOf(p, seat) >= EQCouncil.SkillOf(kv.Value, seat) + 3f) return true;
            }
            return false;
        }
    }

    /// <summary>Embaixador: boa vontade ganha com qualquer facção (só ganhos, só com o jogador).</summary>
    [HarmonyPatch(typeof(Faction), nameof(Faction.TryAffectGoodwillWith))]
    public static class Patch_Faction_TryAffectGoodwillWith_Council
    {
        [HarmonyPriority(Priority.High)]
        public static void Prefix(Faction __instance, Faction other, ref int goodwillChange)
        {
            if (goodwillChange <= 0 || __instance == null || other == null || __instance.IsPlayer == other.IsPlayer) return;
            float f = EQCouncil.GoodwillFactor;
            if (f > 1.001f) goodwillChange = Mathf.RoundToInt(goodwillChange * f);
        }
    }

    [HarmonyPatch(typeof(Building_Throne), nameof(Building_Throne.GetGizmos))]
    public static class Patch_Building_Throne_GetGizmos_Council
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Building_Throne __instance)
        {
            foreach (Gizmo g in gizmos) yield return g;
            if (EQNation.Applies && EQNation.Founded && CouncilDef.Active != null && __instance.Faction == Faction.OfPlayer)
                yield return EQCouncil.CouncilCommand();
        }
    }
}
