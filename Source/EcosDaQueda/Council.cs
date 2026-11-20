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
    /// <summary>Um efeito num "canal" do governo (goodwill, army, tribute...). O fator do canal é 1 + soma dos efeitos.</summary>
    public class GovEffect
    {
        public string channel;
        public float amount;
        /// <summary>Só com esta facção (FactionDef): canal "goodwill@EQ_Faction_Coro".</summary>
        public string faction;
        public string Key => faction == null ? channel : channel + "@" + faction;
    }

    /// <summary>Uma cadeira do conselho (N2, NACAO.md › 3.4; cadeiras de meme no N2b, nacao/memes.md).</summary>
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
        public List<GovEffect> effects = new List<GovEffect>();
        /// <summary>Cadeira de função: ocupada sozinha por quem tem esta função da ideologia (defName do PreceptDef).</summary>
        public string role;
        /// <summary>Ocupada pelo líder da ideologia (ex.: pasta das Relações na República).</summary>
        public bool leaderSeat;
        /// <summary>Exigências de quem senta.</summary>
        public int minAge;
        public bool requireTitle, requireMechanitor, requireHemogen, requirePsylink;

        public bool Auto => role != null || leaderSeat;
    }

    /// <summary>N2: parâmetros do conselho. Valores propostos; ajustar no XML.</summary>
    public class CouncilDef : Def
    {
        public List<CouncilSeat> seats = new List<CouncilSeat>();
        /// <summary>Bônus 0 na perícia skillZero, cheio na skillFull.</summary>
        public float skillZero = 4f, skillFull = 15f;
        public float swapCooldownDays = 3f;
        /// <summary>Canal "markers": marco extra só com o bônus da cadeira em pelo menos isto.</summary>
        public float markerThreshold = 0.5f;
        /// <summary>Mesma cadeira dada por mais de um meme: +isto por meme a mais (Primazia + Industrialismo = 150%).</summary>
        public float seatStackBonus = 0.5f;

        public ThoughtDef memberThought, passedOverThought;

        public static CouncilDef Active => DefDatabase<CouncilDef>.AllDefsListForReading.FirstOrDefault();
        public CouncilSeat Seat(string key) => seats.FirstOrDefault(s => s.key == key);
    }

    public class WorldComponent_EQCouncil : WorldComponent
    {
        public Dictionary<string, Pawn> seats = new Dictionary<string, Pawn>();
        public Dictionary<string, int> lastChange = new Dictionary<string, int>();
        /// <summary>Individualista: a cadeira fixa que o jogador fechou.</summary>
        public string droppedSeat;
        private List<string> tmpKeys; private List<Pawn> tmpPawns; private List<string> tmpKeys2; private List<int> tmpInts;

        public WorldComponent_EQCouncil(World world) : base(world) { }

        public static WorldComponent_EQCouncil Current => Find.World?.GetComponent<WorldComponent_EQCouncil>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref seats, "seats", LookMode.Value, LookMode.Reference, ref tmpKeys, ref tmpPawns);
            Scribe_Collections.Look(ref lastChange, "lastChange", LookMode.Value, LookMode.Value, ref tmpKeys2, ref tmpInts);
            Scribe_Values.Look(ref droppedSeat, "droppedSeat");
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
            Cleanup(!EQLeader.SuppressDialogs);
        }

        /// <summary>Morto, fora da facção, virou líder, ou a cadeira saiu do governo (reforma): fica vazia (sem espera).</summary>
        public int Cleanup(bool messages)
        {
            EQCouncil.Dirty();
            var active = EQCouncil.ManualSeatKeys();
            int n = 0;
            foreach (var kv in seats.ToList())
            {
                Pawn p = kv.Value;
                bool gone = !active.Contains(kv.Key);
                if (gone || p == null || p.Dead || p.Destroyed || p.Faction != Faction.OfPlayer || p == EQLeader.IdeoLeader)
                {
                    seats.Remove(kv.Key);
                    n++;
                    if (p != null && messages)
                        Messages.Message(gone
                            ? "A cadeira de " + EQCouncil.LabelOf(kv.Key) + " saiu do conselho com a mudança na ideologia; " + p.LabelShort + " volta para casa."
                            : "A cadeira de " + EQCouncil.LabelOf(kv.Key) + " no conselho ficou vazia (" + p.LabelShort + ").", MessageTypeDefOf.NeutralEvent, false);
                }
            }
            if (n > 0) EQCouncil.Dirty();
            return n;
        }

        public Pawn Holder(string key) => seats.TryGetValue(key, out Pawn p) ? p : null;

        public float CooldownLeftDays(string key)
        {
            CouncilDef d = CouncilDef.Active;
            if (d == null || EQGov.Rule(g => g.noSwapCooldown) || !lastChange.TryGetValue(key, out int t)) return 0f;
            return Mathf.Max(0f, d.swapCooldownDays - (Find.TickManager.TicksGame - t) / (float)GenDate.TicksPerDay);
        }

        /// <summary>null = feito; senão o motivo. p == null esvazia a cadeira.</summary>
        public string Assign(string key, Pawn p)
        {
            CouncilDef d = CouncilDef.Active;
            EQCouncil.Dirty();
            ActiveSeat seat = EQCouncil.SeatOf(key);
            if (d == null || seat == null) return "cadeira desconhecida: " + key;
            if (!EQNation.Founded) return EQCouncil.NotFoundedReason;
            if (seat.def.Auto) return "A cadeira de " + seat.label + " não se nomeia: " + EQCouncil.AutoHolderText(seat.def) + ".";
            if (Holder(key) == p) return null;
            float cd = CooldownLeftDays(key);
            if (cd > 0f && Holder(key) != null) return "Trocar de " + seat.label + " tão cedo pega mal: espere " + cd.ToString("0.#") + " dias.";
            if (p != null)
            {
                string why = EQCouncil.CannotSit(p) ?? EQCouncil.Requirement(p, seat);
                if (why != null) return why;
                string other = seats.FirstOrDefault(kv => kv.Value == p).Key;
                if (other != null) return p.LabelShort + " já ocupa a cadeira de " + EQCouncil.LabelOf(other) + ".";
            }
            if (p == null) seats.Remove(key); else seats[key] = p;
            lastChange[key] = Find.TickManager.TicksGame;
            EQCouncil.Dirty();
            return null;
        }
    }

    /// <summary>Uma cadeira em vigor agora (fixa ou vinda de meme), já com nome trocado e multiplicador.</summary>
    public class ActiveSeat
    {
        public CouncilSeat def;
        public string label;
        public bool fixedSeat;
        /// <summary>Memes que dão esta cadeira (vazio nas fixas).</summary>
        public List<string> memes = new List<string>();
        /// <summary>Multiplicador próprio (pilha de memes, Nacionalista no General...).</summary>
        public float mult = 1f;
        public string Key => def.key;
    }

    public static class EQCouncil
    {
        public const string Ambassador = "embaixador", General = "general", Advisor = "conselheiro";
        public const string NotFoundedReason = "Sem nação, sem conselho: proclame um líder no trono e vença a revolta dos vizinhos.";

        // ------------------------------------------------------------ cache (recalcula a cada tick ou depois de Dirty)

        private static int cacheTick = -1;
        private static bool dirty = true;
        private static List<ActiveSeat> seatCache = new List<ActiveSeat>();
        private static Dictionary<string, float> channelCache = new Dictionary<string, float>();
        private static Dictionary<string, float> effectCache = new Dictionary<string, float>();
        private static int markersCache;

        public static void Dirty() => dirty = true;

        private static void Ensure()
        {
            int now = Find.TickManager?.TicksGame ?? 0;
            if (!dirty && now == cacheTick) return;
            dirty = false;
            cacheTick = now;
            Recompute();
        }

        private static void Recompute()
        {
            seatCache = new List<ActiveSeat>();
            channelCache = new Dictionary<string, float>();
            effectCache = new Dictionary<string, float>();
            markersCache = 0;
            if (Current.Game == null || Find.World == null || Find.FactionManager?.OfPlayer == null) return;
            CouncilDef d = CouncilDef.Active;
            var c = WorldComponent_EQCouncil.Current;
            if (d == null || c == null) return;
            var gov = EQGov.ActiveDefs();

            // cadeiras fixas (Individualista: uma fechada) e as dos memes (mesma chave = pilha)
            bool dropRule = gov.Any(g => g.dropOneFixedSeat);
            foreach (CouncilSeat s in d.seats)
                if (!(dropRule && c.droppedSeat == s.key)) seatCache.Add(new ActiveSeat { def = s, label = s.label, fixedSeat = true });
            foreach (GovMemeDef g in gov)
                foreach (CouncilSeat s in g.seats)
                {
                    ActiveSeat a = seatCache.FirstOrDefault(x => x.Key == s.key);
                    if (a == null) seatCache.Add(a = new ActiveSeat { def = s, label = s.label });
                    else if (!a.fixedSeat) a.mult += d.seatStackBonus;
                    a.memes.Add(g.MemeLabel);
                }
            // cadeiras que um meme proíbe (Pacifista: sem General; Tecnofobia/Neolítico: sem Ministro da Ciência)
            var blocked = new HashSet<string>(gov.SelectMany(g => g.blockSeats));
            seatCache.RemoveAll(a => blocked.Contains(a.Key));
            // líder assume cadeiras (Sacerdócio exaltado: o guia moral é o líder)
            var leaderTakes = new HashSet<string>(gov.SelectMany(g => g.leaderTakes));
            foreach (ActiveSeat a in seatCache.Where(x => leaderTakes.Contains(x.Key)).ToList())
            {
                a.def = new CouncilSeat { key = a.def.key, label = a.def.label, description = a.def.description, skills = a.def.skills, averageSkills = a.def.averageSkills, effects = a.def.effects, leaderSeat = true };
            }
            foreach (GovMemeDef g in gov)
            {
                foreach (SeatRename r in g.renames) { ActiveSeat a = seatCache.FirstOrDefault(x => x.Key == r.key); if (a != null) a.label = r.label; }
                foreach (SeatFactor f in g.seatFactors) { ActiveSeat a = seatCache.FirstOrDefault(x => x.Key == f.key); if (a != null) a.mult *= f.factor; }
            }

            // multiplicador geral: Coletivista ×0,8, Individualista ×1,3 (com a cadeira fechada), Culto à personalidade pelo humor do líder
            float all = 1f;
            foreach (GovMemeDef g in gov)
                if (!g.dropOneFixedSeat || c.droppedSeat != null) all *= g.seatFactor;
            if (gov.Any(g => g.moodScaled)) all *= LeaderMoodFactor();
            if (gov.Any(g => g.councilMoodScaled)) all *= CouncilMoodFactor();
            float leaderShare = gov.Count == 0 ? 0f : gov.Max(g => g.leaderFillsEmpty);
            Pawn leader = EQLeader.IdeoLeader;

            bool founded = EQNation.Founded;
            foreach (ActiveSeat a in seatCache)
            {
                float e = 0f, baseE = 0f;
                if (founded)
                {
                    Pawn p = HolderOf(a);
                    if (p != null && Inactive(p, a.def) == null && Requirement(p, a) == null) baseE = SkillEffect(p, a.def);
                    else if (p == null && a.fixedSeat && leaderShare > 0f && leader != null && Inactive(leader, a.def) == null)
                        baseE = SkillEffect(leader, a.def) * leaderShare; // Deus-imperador: o líder vale por meio conselho
                    e = baseE * a.mult * all;
                }
                effectCache[a.Key] = e;
                foreach (GovEffect fx in a.def.effects)
                {
                    if (fx.channel == "markers") { if (baseE >= d.markerThreshold) markersCache += Mathf.RoundToInt(fx.amount); continue; }
                    Add(fx.Key, fx.amount * e);
                }
            }
            // por cadeira ocupada e valendo (Progressista: pesquisa +5% por cadeira)
            int working = effectCache.Count(kv => kv.Value > 0f);
            if (founded)
                foreach (GovMemeDef g in gov)
                    foreach (GovEffect fx in g.perSeat) Add(fx.Key, fx.amount * working);
            // efeitos fixos dos memes (valem também antes da fundação: ex. revolta da proclamação)
            foreach (GovMemeDef g in gov)
                foreach (GovEffect fx in g.effects)
                {
                    if (fx.channel == "markers") { markersCache += Mathf.RoundToInt(fx.amount); continue; }
                    if (founded || fx.channel == "revolt") Add(fx.Key, fx.amount);
                }
        }

        private static void Add(string ch, float v)
        {
            if (string.IsNullOrEmpty(ch) || v == 0f) return;
            channelCache.TryGetValue(ch, out float cur);
            channelCache[ch] = cur + v;
        }

        public static float LeaderMoodFactor()
        {
            float m = EQLeader.IdeoLeader?.needs?.mood?.CurLevelPercentage ?? 0.5f;
            // humor 30% ou menos = ×0,7; 50% = ×1; 70% ou mais = ×1,2
            return m >= 0.5f ? Mathf.Lerp(1f, 1.2f, (m - 0.5f) / 0.2f) : Mathf.Lerp(0.7f, 1f, (m - 0.3f) / 0.2f);
        }

        /// <summary>Bacanal: humor médio de quem ocupa cadeira. 60%+ = ×1,1; abaixo de 40% = ×0,8.</summary>
        public static float CouncilMoodFactor()
        {
            var moods = seatCache.Select(HolderOf).Where(p => p?.needs?.mood != null).Distinct().Select(p => p.needs.mood.CurLevelPercentage).ToList();
            if (moods.Count == 0) return 1f;
            float m = moods.Average();
            return m >= 0.6f ? 1.1f : m < 0.4f ? 0.8f : 1f;
        }

        public static List<ActiveSeat> Seats() { Ensure(); return seatCache; }
        public static ActiveSeat SeatOf(string key) => Seats().FirstOrDefault(s => s.Key == key);
        public static HashSet<string> ManualSeatKeys() { dirty = true; return new HashSet<string>(Seats().Where(s => !s.def.Auto).Select(s => s.Key)); }
        public static string LabelOf(string key) => SeatOf(key)?.label ?? CouncilDef.Active?.Seat(key)?.label ?? key;

        /// <summary>Soma dos efeitos no canal (0 = nada).</summary>
        public static float Channel(string ch) { Ensure(); return channelCache.TryGetValue(ch, out float v) ? v : 0f; }
        public static float Factor(string ch) => Mathf.Max(0.05f, 1f + Channel(ch));

        // ------------------------------------------------------------ quem senta

        public static string CannotSit(Pawn p)
        {
            if (p == null || p.Dead || p.Faction != Faction.OfPlayer || !p.IsFreeNonSlaveColonist || p.IsQuestLodger()) return "Precisa ser um colono livre.";
            if (!p.RaceProps.Humanlike || !DevelopmentalStageExtensions.Adult(p.DevelopmentalStage)) return "Precisa ser um adulto.";
            if (p == EQLeader.IdeoLeader) return "O líder não senta no próprio conselho; ele preside.";
            return null;
        }

        /// <summary>Exigências da cadeira e das regras do governo (Anciãos, Aristocrático, mecanitor). null = cumpre.</summary>
        public static string Requirement(Pawn p, ActiveSeat a)
        {
            if (p == null || a == null) return null;
            int age = a.def.minAge;
            bool title = a.def.requireTitle;
            if (a.fixedSeat)
                foreach (GovMemeDef g in EQGov.ActiveDefs())
                {
                    age = Math.Max(age, g.fixedSeatMinAge);
                    title |= g.fixedSeatsRequireTitle;
                }
            if (age > 0 && p.ageTracker != null && p.ageTracker.AgeBiologicalYears < age) return "A cadeira de " + a.label + " exige " + age + " anos ou mais.";
            if (title && (p.royalty == null || !p.royalty.AllTitlesForReading.Any())) return "A cadeira de " + a.label + " exige um título de nobreza.";
            if (a.def.requireMechanitor && !MechanitorUtility.IsMechanitor(p)) return "A cadeira de " + a.label + " exige um mecanitor.";
            if (a.def.requireHemogen && p.genes?.GetFirstGeneOfType<Gene_Hemogen>() == null) return "A cadeira de " + a.label + " exige um sanguífago.";
            if (a.def.requirePsylink && !p.HasPsylink) return "A cadeira de " + a.label + " exige um psicolink.";
            return null;
        }

        public static Precept_Role RoleOf(string defName) =>
            defName == null ? null : EQLeader.PlayerIdeo?.PreceptsListForReading.OfType<Precept_Role>().FirstOrDefault(r => r.def.defName == defName);

        public static string RoleLabel(string defName) => RoleOf(defName)?.LabelCap ?? DefDatabase<PreceptDef>.GetNamedSilentFail(defName ?? "")?.LabelCap ?? defName;

        public static string AutoHolderText(CouncilSeat s) =>
            s.leaderSeat ? "é do líder" : "fica com quem tem a função de " + RoleLabel(s.role) + " (aba Ideologia)";

        /// <summary>Quem ocupa: nomeado (cadeira manual), o líder ou o dono da função.</summary>
        public static Pawn HolderOf(ActiveSeat a)
        {
            if (a == null) return null;
            if (a.def.leaderSeat) return EQLeader.IdeoLeader;
            if (a.def.role != null) return RoleOf(a.def.role)?.ChosenPawns().FirstOrDefault();
            return WorldComponent_EQCouncil.Current?.Holder(a.Key);
        }
        public static Pawn HolderOf(string key) => HolderOf(SeatOf(key));

        public static bool IsMember(Pawn p) =>
            p != null && p != EQLeader.IdeoLeader && Seats().Any(a => HolderOf(a) == p);

        public static float SkillOf(Pawn p, CouncilSeat seat)
        {
            if (p?.skills == null || seat.skills.Count == 0) return 0f;
            var lv = seat.skills.Select(s => p.skills.GetSkill(s)).Select(s => s == null || s.TotallyDisabled ? 0f : s.Level).ToList();
            return seat.averageSkills ? lv.Average() : lv.Max();
        }

        public static float SkillEffect(Pawn p, CouncilSeat seat)
        {
            CouncilDef d = CouncilDef.Active;
            if (d == null || p == null) return 0f;
            return Mathf.Clamp01((SkillOf(p, seat) - d.skillZero) / Mathf.Max(1f, d.skillFull - d.skillZero));
        }

        /// <summary>Por que o conselheiro não está valendo agora (null = valendo).</summary>
        public static string Inactive(Pawn p, CouncilSeat seat)
        {
            if (p == null) return seat.role != null && RoleOf(seat.role) == null ? "a ideologia não tem a função de " + RoleLabel(seat.role) : "vazia";
            if (p.Dead || p.Faction != Faction.OfPlayer) return "ausente";
            if (p.Downed) return "inconsciente";
            if (p.IsPrisoner) return "preso";
            if (p.GetCaravan() != null) return seat.allowCaravan ? null : "numa caravana";
            if (p.MapHeld?.IsPlayerHome != true) return "fora da colônia";
            return null;
        }

        /// <summary>Quanto do bônus da cadeira vale agora (0 a 1, mais com pilha de memes e regras).</summary>
        public static float Effect(string key) { Ensure(); return effectCache.TryGetValue(key, out float v) ? v : 0f; }

        // ------------------------------------------------------------ ganchos dos sistemas

        public static float GoodwillFactor => Factor("goodwill");
        /// <summary>Com uma facção de outra ideologia (Nacionalista: menos).</summary>
        public static float GoodwillFactorWith(Faction npc)
        {
            float f = Channel("goodwill");
            Ideo theirs = npc?.ideos?.PrimaryIdeo;
            if (theirs != null && theirs != EQLeader.PlayerIdeo) f += Channel("goodwillOtherIdeo");
            if (npc?.def != null) f += Channel("goodwill@" + npc.def.defName);
            return Mathf.Max(0.05f, 1f + f);
        }
        public static float PatronTensionFactor => Factor("patronTension");
        public static float CeremonyFactor => Factor("ceremony");
        public static int CeremonyCount(int count) => count <= 1 ? count : Mathf.Max(1, Mathf.CeilToInt(count * CeremonyFactor));
        public static float ArmyFactor => Factor("army");
        public static float RevoltFactor => Factor("revolt");
        public static float FrontierSpeedFactor => Factor("frontier");
        public static float TributeFactor => Factor("tribute");
        /// <summary>Tributo de um tipo: tribute + tributeFood / tributeMaterials / tributeSilver.</summary>
        public static float Tribute(string kind) => Mathf.Max(0.05f, 1f + Channel("tribute") + Channel(kind));
        public static int ExtraMarkers { get { Ensure(); return markersCache; } }

        /// <summary>Rendimento de um tile do jogador: floresta/prado (Guardião da Terra), morro e montanha (Ministro das Minas).</summary>
        public static void AdjustPlayerYield(TileYield y, PlanetTile t)
        {
            if (y == null) return;
            Tile tile = Find.WorldGrid[t];
            if (tile == null) return;
            float green = Channel("yieldGreen"), hills = Channel("yieldHills");
            if (green != 0f && tile.PrimaryBiome != null && tile.PrimaryBiome.plantDensity >= 0.5f && !tile.PrimaryBiome.isExtremeBiome)
            {
                float f = Mathf.Max(0f, 1f + green);
                y.food *= f; y.wood *= f; y.prosperity *= f;
            }
            if (hills != 0f && tile.hilliness >= Hilliness.SmallHills)
            {
                float f = Mathf.Max(0f, 1f + hills);
                y.stone *= f; y.metal *= f;
            }
            float road = Channel("yieldRoad"), river = Channel("yieldRiver"), coast = Channel("yieldCoast");
            if (tile is SurfaceTile st)
            {
                if (road != 0f && !st.Roads.NullOrEmpty()) y.prosperity *= Mathf.Max(0f, 1f + road);
                if (river != 0f && !st.Rivers.NullOrEmpty())
                {
                    float f = Mathf.Max(0f, 1f + river);
                    y.food *= f; y.wood *= f; y.stone *= f; y.metal *= f; y.components *= f; y.prosperity *= f;
                }
            }
            if (coast != 0f && Find.World.CoastDirectionAt(t).IsValid) y.food *= Mathf.Max(0f, 1f + coast);
            float fw = Channel("yieldForestWood"), fp = Channel("yieldForestProsperity");
            if ((fw != 0f || fp != 0f) && IsForest(tile.PrimaryBiome))
            {
                y.wood *= Mathf.Max(0f, 1f + fw);
                y.prosperity *= Mathf.Max(0f, 1f + fp);
            }
        }

        public static bool IsForest(BiomeDef b) =>
            b != null && (b.defName.Contains("Forest") || b.defName.Contains("Jungle") || b.defName.Contains("Taiga") || b.defName.Contains("Rainforest"));

        private static readonly string[] ReportChannels = { "workSpeed", "research", "tradePrice", "quality", "yieldGreen", "yieldHills", "tributeFood", "tributeMaterials", "tributeSilver", "tributeComponents", "goodwillOtherIdeo",
            "markerCost", "markerReach", "markerSetup", "startRadius", "invasionVeto", "yieldRoad", "yieldRiver", "yieldCoast", "yieldForestWood", "yieldForestProsperity" };

        public static string Report()
        {
            CouncilDef d = CouncilDef.Active;
            var c = WorldComponent_EQCouncil.Current;
            if (d == null || c == null) return "sem conselho";
            Dirty();
            var parts = Seats().Select(a =>
            {
                Pawn p = HolderOf(a);
                string who = p == null ? (Inactive(null, a.def) ?? "vazia")
                    : p.LabelShort + " (perícia " + SkillOf(p, a.def).ToString("0") + ", " + (Effect(a.Key) * 100f).ToString("0") + "%"
                      + (Inactive(p, a.def) is string w ? ", " + w : "") + (Requirement(p, a) is string r ? ", não cumpre" : "") + ")";
                if (p == null && Effect(a.Key) > 0f) who += " [líder " + (Effect(a.Key) * 100f).ToString("0") + "%]";
                return a.Key + (a.label != a.def.label ? "(" + a.label + ")" : "") + (a.def.Auto ? "*" : "") + "=" + who
                       + (a.mult != 1f ? " ×" + a.mult.ToString("0.##") : "")
                       + (c.CooldownLeftDays(a.Key) > 0f && c.Holder(a.Key) != null ? " espera=" + c.CooldownLeftDays(a.Key).ToString("0.0") + "d" : "");
            });
            Ensure();
            var shown = ReportChannels.Concat(channelCache.Keys.Where(k => k.Contains("@")).OrderBy(k => k));
            string extra = string.Join(" ", shown.Where(ch => Channel(ch) != 0f).Select(ch => ch + (Channel(ch) >= 0f ? "+" : "") + Channel(ch).ToString("0.###")));
            var gov = EQGov.ActiveDefs();
            return "conselho: " + string.Join(" | ", parts)
                   + "\n  fatores: boa_vontade×" + GoodwillFactor.ToString("0.00") + " tensão_patrona×" + PatronTensionFactor.ToString("0.00")
                   + " cerimônia×" + CeremonyFactor.ToString("0.00") + " exército×" + ArmyFactor.ToString("0.00") + " revolta×" + RevoltFactor.ToString("0.00")
                   + " fronteira×" + FrontierSpeedFactor.ToString("0.00") + " tributo×" + TributeFactor.ToString("0.00") + " marcos+" + ExtraMarkers
                   + (extra.Length > 0 ? "\n  canais: " + extra : "")
                   + (gov.Count > 0 ? "\n  governo: " + string.Join(", ", gov.Select(g => g.meme)) + (EQGov.Succession() is string s ? " | sucessão: " + s : "") + (c.droppedSeat != null && gov.Any(g => g.dropOneFixedSeat) ? " | fechada=" + c.droppedSeat : "") : "");
        }

        public static Command CouncilCommand()
        {
            var cmd = new Command_Action
            {
                defaultLabel = "Conselho",
                defaultDesc = "Quem aconselha o trono: Embaixador, General, Conselheiro e as cadeiras que a ideologia acrescenta. Cada cadeira dá um bônus que cresce com a perícia de quem senta nela.",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/ThroneSpeech", false) ?? BaseContent.BadTex,
                action = () => Find.WindowStack.Add(new Dialog_EQCouncil())
            };
            if (!EQNation.Founded) cmd.Disable(NotFoundedReason);
            return cmd;
        }
    }

    public class Dialog_EQCouncil : Window
    {
        private Vector2 scroll;
        private float viewHeight = 600f;

        public override Vector2 InitialSize => new Vector2(680f, 640f);

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
            EQCouncil.Dirty();
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "Conselho de " + Faction.OfPlayer.Name);
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 40f, inRect.width, 40f), "O líder (" + (EQLeader.IdeoLeader?.LabelShort ?? "ninguém") + ") preside e não ocupa cadeira. O bônus vale com o conselheiro vivo, consciente e na colônia.");
            Rect outer = new Rect(0f, 84f, inRect.width, inRect.height - 84f - CloseButSize.y - 8f);
            Rect view = new Rect(0f, 0f, outer.width - 16f, viewHeight);
            Widgets.BeginScrollView(outer, ref scroll, view);
            float y = 0f;
            foreach (ActiveSeat seat in EQCouncil.Seats().ToList())
            {
                DrawSeat(seat, c, view.width, ref y);
            }

            var gov = EQGov.ActiveDefs();
            if (gov.Count > 0)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(0f, y, view.width, 30f), "Governo pela ideologia");
                Text.Font = GameFont.Small;
                y += 32f;
                foreach (GovMemeDef g in gov.Where(g => !g.summary.NullOrEmpty()))
                {
                    string line = "<b>" + g.MemeLabel.CapitalizeFirst() + ":</b> " + g.summary;
                    float h = Text.CalcHeight(line, view.width);
                    Widgets.Label(new Rect(0f, y, view.width, h), line);
                    y += h + 4f;
                }
                if (gov.Any(g => g.dropOneFixedSeat))
                {
                    Rect b = new Rect(0f, y, 260f, 30f);
                    if (c.droppedSeat == null)
                    {
                        if (Widgets.ButtonText(b, "Fechar uma cadeira fixa"))
                            Find.WindowStack.Add(new FloatMenu(d.seats.Select(s => new FloatMenuOption(s.label.CapitalizeFirst(), () => { c.droppedSeat = s.key; c.Cleanup(true); })).ToList()));
                    }
                    else if (Widgets.ButtonText(b, "Reabrir a cadeira de " + (d.Seat(c.droppedSeat)?.label ?? c.droppedSeat)))
                    {
                        c.droppedSeat = null; EQCouncil.Dirty();
                    }
                    y += 34f;
                }
                if (EQGov.Succession() is string s)
                {
                    Widgets.Label(new Rect(0f, y, view.width, 24f), "Sucessão: " + s);
                    y += 26f;
                }
            }
            viewHeight = y + 10f;
            Widgets.EndScrollView();
        }

        private static void DrawSeat(ActiveSeat seat, WorldComponent_EQCouncil c, float width, ref float y)
        {
            Pawn p = EQCouncil.HolderOf(seat);
            Rect row = new Rect(0f, y, width, 106f);
            Widgets.DrawMenuSection(row);
            Rect inner = row.ContractedBy(6f);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inner.x, inner.y, inner.width - 130f, 30f), seat.label.CapitalizeFirst());
            Text.Font = GameFont.Tiny;
            if (seat.memes.Count > 0)
                Widgets.Label(new Rect(inner.x + 250f, inner.y + 6f, inner.width - 380f, 20f), "(" + string.Join(", ", seat.memes) + (seat.mult != 1f ? ", ×" + seat.mult.ToString("0.##") : "") + ")");
            Text.Font = GameFont.Small;
            string who = p == null ? "(" + (EQCouncil.Inactive(null, seat.def) ?? "vazia") + ")"
                : p.LabelShort + " — perícia " + EQCouncil.SkillOf(p, seat.def).ToString("0") + ", bônus " + (EQCouncil.Effect(seat.Key) * 100f).ToString("0") + "%"
                  + (EQCouncil.Inactive(p, seat.def) is string w ? " (" + w + ")" : "")
                  + (EQCouncil.Requirement(p, seat) is string r ? " (" + r + ")" : "");
            if (p == null && EQCouncil.Effect(seat.Key) > 0f) who += " — o líder cobre " + (EQCouncil.Effect(seat.Key) * 100f).ToString("0") + "%";
            Widgets.Label(new Rect(inner.x, inner.y + 30f, inner.width - 130f, 24f), who);
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(inner.x, inner.y + 52f, inner.width - 130f, 44f), seat.def.description);
            Text.Font = GameFont.Small;
            Rect btn = new Rect(inner.xMax - 120f, inner.y + 30f, 120f, 32f);
            if (seat.def.Auto)
            {
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(btn.x, btn.y, 120f, 60f), seat.def.leaderSeat ? "Cadeira do líder." : "Função da ideologia: " + EQCouncil.RoleLabel(seat.def.role) + ".");
                Text.Font = GameFont.Small;
            }
            else
            {
                if (Widgets.ButtonText(btn, p == null ? "Nomear" : "Trocar"))
                {
                    var opts = new List<FloatMenuOption>();
                    if (p != null) opts.Add(new FloatMenuOption("Deixar vazia", () => Report(c.Assign(seat.Key, null))));
                    foreach (Pawn cand in EQLeader.AllFreeColonists().Distinct().Where(x => EQCouncil.CannotSit(x) == null && x != p)
                                 .OrderByDescending(x => EQCouncil.SkillOf(x, seat.def)))
                    {
                        Pawn cc = cand;
                        string seatOf = c.seats.FirstOrDefault(kv => kv.Value == cc).Key;
                        string req = EQCouncil.Requirement(cc, seat);
                        string label = cc.LabelShort + " (" + EQCouncil.SkillOf(cc, seat.def).ToString("0") + ")" + (seatOf != null ? " — já é " + EQCouncil.LabelOf(seatOf) : req != null ? " — " + req : "");
                        opts.Add(seatOf != null || req != null ? new FloatMenuOption(label, null) : new FloatMenuOption(label, () => Report(c.Assign(seat.Key, cc))));
                    }
                    if (opts.Count == 0) opts.Add(new FloatMenuOption("Ninguém disponível", null));
                    Find.WindowStack.Add(new FloatMenu(opts));
                }
                float cd = c.CooldownLeftDays(seat.Key);
                if (cd > 0f && p != null)
                {
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(new Rect(btn.x, btn.yMax + 2f, 140f, 20f), "troca em " + cd.ToString("0.#") + " dias");
                    Text.Font = GameFont.Small;
                }
            }
            y += 112f;
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
            return c != null && p.Faction == Faction.OfPlayer && EQNation.Founded && EQCouncil.IsMember(p);
        }
    }

    /// <summary>− "Preterido": tem perícia maior que o conselheiro de uma cadeira nomeada e ficou de fora (Igualitário: nunca).</summary>
    public class ThoughtWorker_EQCouncilPassedOver : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            var c = WorldComponent_EQCouncil.Current;
            if (c == null || c.seats.Count == 0 || p.Faction != Faction.OfPlayer || !p.IsFreeNonSlaveColonist || p == EQLeader.IdeoLeader || EQCouncil.IsMember(p))
                return false;
            if (EQGov.Rule(g => g.noPassedOver)) return false;
            foreach (var kv in c.seats)
            {
                ActiveSeat seat = EQCouncil.SeatOf(kv.Key);
                if (seat != null && kv.Value != null && EQCouncil.SkillOf(p, seat.def) >= EQCouncil.SkillOf(kv.Value, seat.def) + 3f) return true;
            }
            return false;
        }
    }

    /// <summary>Embaixador (e Nacionalista): boa vontade ganha com qualquer facção (só ganhos, só com o jogador).</summary>
    [HarmonyPatch(typeof(Faction), nameof(Faction.TryAffectGoodwillWith))]
    public static class Patch_Faction_TryAffectGoodwillWith_Council
    {
        [HarmonyPriority(Priority.High)]
        public static void Prefix(Faction __instance, Faction other, ref int goodwillChange)
        {
            if (goodwillChange <= 0 || __instance == null || other == null || __instance.IsPlayer == other.IsPlayer) return;
            float f = EQCouncil.GoodwillFactorWith(__instance.IsPlayer ? other : __instance);
            if (Mathf.Abs(f - 1f) > 0.001f) goodwillChange = Mathf.Max(1, Mathf.RoundToInt(goodwillChange * f));
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
