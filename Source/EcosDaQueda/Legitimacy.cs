using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace EcosDaQueda
{
    /// <summary>
    /// N3 (NACAO.md › 3.6): legitimidade da nação, sucessão coroada, reações das potências à fundação e mercenários do
    /// Sindicato nas revoltas. Valores propostos em Defs/World/EQ_Legitimidade.xml.
    /// </summary>
    public class LegitimacyDef : Def
    {
        public float start = 50f, max = 100f;
        // por dia
        public float throneDaily = 0.3f, noThroneDaily = -0.5f, noLeaderDaily = -1f, fameDaily = 0.3f, infamyDaily = -0.4f;
        // eventos
        public float raidRepelled = 3f, raidMinPawns = 3f, armyWin = 5f, armyLoss = -6f, borderWin = 6f, borderLoss = -8f;
        public float leaderDied = -20f, leaderChanged = -5f, crowned = 5f;
        public float revoltWon = 12f, revoltLost = -15f, guiltyRevoltLost = 8f;
        public float councilExecuted = -10f, bloodCourtExecuted = 6f;
        // limiares
        public float unrestBelow = 30f, frozenBelow = 10f, highAbove = 70f;
        public float unrestMtbDays = 8f, unrestCooldownDays = 10f, unrestPointsFactor = 0.6f;
        public List<GovEffect> highEffects = new List<GovEffect>();
        public List<GovEffect> lowEffects = new List<GovEffect>();
        // sucessão
        public float crownDays = 10f, successionPointsFactor = 1f;
        public float revoltDelayMinDays = 1f, revoltDelayMaxDays = 2f;
        // reações à fundação
        public int coroGoodwill = 5;
        public QualityCategory coroMinQuality = QualityCategory.Excellent;
        public float heliceCeremony = -0.2f, heliceDays = 60f;
        public int heliceGoodwill = 5;
        public int forjaSteel = 80, forjaGoodwill = 10, forjaRefuse = -5;
        public float forjaLegitimacy = 5f, silencioLegitimacy = 3f, sindicatoDiscount = 0.25f;
        // mercenários
        public FactionDef mercFaction;
        public float mercPointsFactor = 0.5f, mercSilverPerPoint = 1f, mercFleeChance = 0.15f;

        public static LegitimacyDef Active => DefDatabase<LegitimacyDef>.AllDefsListForReading.FirstOrDefault();
    }

    /// <summary>Revolta menor depois da fundação: agitação (legitimidade baixa) ou sucessão (herdeiro não coroado).</summary>
    public class EQMinorRevolt : IExposable
    {
        public string kind = "unrest";
        public int arriveTick = -1;
        public float points;
        public bool bribed, mercsHired, forced, started;
        public int throneTicks;
        public Map map;
        public Building_Throne throne;
        public List<Pawn> revolters = new List<Pawn>();
        public List<Pawn> mercs = new List<Pawn>();

        public bool Succession => kind == "succession";
        public string Label => Succession ? "a região não reconhece o herdeiro" : "agitação na região";

        public void ExposeData()
        {
            Scribe_Values.Look(ref kind, "kind", "unrest");
            Scribe_Values.Look(ref arriveTick, "arriveTick", -1);
            Scribe_Values.Look(ref points, "points");
            Scribe_Values.Look(ref bribed, "bribed");
            Scribe_Values.Look(ref mercsHired, "mercsHired");
            Scribe_Values.Look(ref forced, "forced");
            Scribe_Values.Look(ref started, "started");
            Scribe_Values.Look(ref throneTicks, "throneTicks");
            Scribe_References.Look(ref map, "map");
            Scribe_References.Look(ref throne, "throne");
            Scribe_Collections.Look(ref revolters, "revolters", LookMode.Reference);
            Scribe_Collections.Look(ref mercs, "mercs", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (revolters == null) revolters = new List<Pawn>();
                if (mercs == null) mercs = new List<Pawn>();
                revolters.RemoveAll(p => p == null);
                mercs.RemoveAll(p => p == null);
            }
        }
    }

    public static class EQLegitimacy
    {
        /// <summary>Último relatório (executor de testes).</summary>
        public static string LastReport;

        public static WorldComponent_EQLegitimacy Comp => WorldComponent_EQLegitimacy.Current;

        /// <summary>Vale com a nação fundada de verdade (não com a suposição do executor de testes).</summary>
        public static bool On => EQNation.Applies && LegitimacyDef.Active != null && (WorldComponent_EQNation.Current?.IsFounded ?? false);

        public static float Value => On && Comp != null && Comp.initialized ? Comp.value : -1f;

        /// <summary>Abaixo do limiar, o território para de crescer (F4).</summary>
        public static float FrontierFactor => On && Comp != null && Comp.initialized && Comp.value < LegitimacyDef.Active.frozenBelow ? 0f : 1f;

        public static string Tier(float v)
        {
            LegitimacyDef d = LegitimacyDef.Active;
            if (d == null || v < 0f) return "-";
            if (v < d.frozenBelow) return "contestada (o território não cresce)";
            if (v < d.unrestBelow) return "frágil (agitação na região)";
            if (v >= d.highAbove) return "sólida";
            return "estável";
        }

        /// <summary>Evento que muda a legitimidade (só com a nação fundada).</summary>
        public static void Event(float amount, string why)
        {
            if (!On || amount == 0f) return;
            Comp?.Add(amount, why);
        }

        /// <summary>Canais extras para o conselho: faixa da legitimidade e franquia da Hélice.</summary>
        public static IEnumerable<KeyValuePair<string, float>> ExtraChannels()
        {
            var c = Comp;
            LegitimacyDef d = LegitimacyDef.Active;
            if (!On || c == null || d == null || !c.initialized) yield break;
            if (c.value >= d.highAbove) foreach (GovEffect fx in d.highEffects) yield return new KeyValuePair<string, float>(fx.Key, fx.amount);
            else if (c.value < d.unrestBelow) foreach (GovEffect fx in d.lowEffects) yield return new KeyValuePair<string, float>(fx.Key, fx.amount);
            if (c.franchiseUntil > Find.TickManager.TicksGame) yield return new KeyValuePair<string, float>("ceremony", d.heliceCeremony);
        }

        public static Building_Throne FindThrone(Map preferred = null)
        {
            IEnumerable<Map> maps = Find.Maps.Where(m => m.IsPlayerHome);
            if (preferred != null) maps = new[] { preferred }.Concat(maps.Where(m => m != preferred));
            foreach (Map m in maps)
                foreach (Building_Throne t in m.listerBuildings.AllBuildingsColonistOfClass<Building_Throne>())
                    if (EQNation.IsThroneRoom(t)) return t;
            return null;
        }

        public static Faction PowerOf(string defName)
        {
            FactionDef def = DefDatabase<FactionDef>.GetNamedSilentFail(defName);
            Faction f = def == null ? null : Find.FactionManager.FirstFactionOfDef(def);
            return f != null && !f.defeated ? f : null;
        }

        // ------------------------------------------------------------ reações das potências à fundação

        public static string FoundingReactions(Building_Throne throne)
        {
            LegitimacyDef d = LegitimacyDef.Active;
            var c = Comp;
            if (d == null || c == null) return "sem legitimidade";
            var parts = new List<string>();
            Faction pf = Faction.OfPlayer;

            Faction coro = PowerOf("EQ_Faction_Coro");
            if (coro != null)
            {
                QualityCategory q = QualityCategory.Normal;
                bool hasQ = throne != null && throne.TryGetQuality(out q);
                bool good = hasQ && q >= d.coroMinQuality;
                int gw = good ? d.coroGoodwill : -d.coroGoodwill;
                coro.TryAffectGoodwillWith(pf, gw, canSendMessage: false, canSendHostilityLetter: false);
                parts.Add("Coro " + gw.ToString("+0;-0") + " (trono " + (hasQ ? q.GetLabel() : "sem qualidade") + ")");
                Find.LetterStack.ReceiveLetter("Nota de protocolo: " + coro.Name,
                    good
                        ? "O Coro registra, em papel timbrado e perfumado, a fundação de " + pf.Name + ". O trono foi considerado \"adequado ao canto\". Boa vontade " + gw.ToString("+0") + "."
                        : "O Coro registra a fundação de " + pf.Name + " com uma ressalva de rodapé: o trono é " + (hasQ ? q.GetLabel() : "de qualidade indeterminada")
                          + ", e \"um país se mede pela cadeira em que se senta\". Boa vontade " + gw.ToString("+0;-0") + ". Um trono " + d.coroMinQuality.GetLabel() + " ou melhor teria resolvido.",
                    LetterDefOf.NeutralEvent, null, coro);
            }

            Faction helice = PowerOf("EQ_Faction_Helice");
            if (helice != null && !helice.HostileTo(pf))
            {
                SendReaction("helice", "Proposta de franquia: " + helice.Name,
                    "O Concílio da Hélice parabeniza " + pf.Name + " pela fundação e oferece um contrato de franquia: o seu país passa a usar o selo da Hélice nos "
                    + "documentos oficiais e, em troca, as cerimônias saem " + (-d.heliceCeremony * 100f).ToString("0") + "% mais baratas por " + d.heliceDays.ToString("0") + " dias. "
                    + "Cláusula 47-B: a Hélice pode citar o seu país em material publicitário. Cláusula 47-C: o seu país não pode ler a cláusula 47-D.", helice);
                parts.Add("Hélice: franquia oferecida");
            }

            Faction forja = PowerOf("EQ_Faction_Forja");
            if (forja != null && !forja.HostileTo(pf))
            {
                SendReaction("forja", "Registro de nação: " + forja.Name,
                    "A Irmandade da Forja informa que " + pf.Name + " ainda não consta no Livro de Registro das Nações Produtivas. A taxa de registro é de "
                    + d.forjaSteel + " de aço, \"o metal da seriedade\". Nações registradas são tratadas como clientes; as outras, como jazidas.", forja);
                parts.Add("Forja: registro pedido");
            }

            Faction silencio = PowerOf("EQ_Faction_Silencio");
            if (silencio != null)
            {
                c.Add(d.silencioLegitimacy, "o Silêncio catalogou a nação");
                Find.LetterStack.ReceiveLetter("Você foi catalogado",
                    "Nenhum emissário veio. Nenhuma carta chegou. Mesmo assim, na manhã seguinte, havia uma ficha pregada na porta da sala do trono: o nome de "
                    + pf.Name + ", a data da fundação, o número de colonos (correto) e o tipo sanguíneo do líder (também correto).\n\nAté a Ordem do Silêncio Perpétuo "
                    + "reconhece que o seu país existe. Legitimidade +" + d.silencioLegitimacy.ToString("0") + ".",
                    LetterDefOf.NeutralEvent, null, silencio);
                parts.Add("Silêncio: catalogado");
            }

            Faction sind = EQMercs.Faction;
            if (sind != null && !sind.HostileTo(pf))
            {
                c.mercDiscount = d.sindicatoDiscount;
                Find.LetterStack.ReceiveLetter("Cartão de visitas: " + sind.Name,
                    "O Sindicato do Saque deixou um cartão de visitas no portão: \"Revolta? Agitação? Herdeiro contestado? Mão de obra organizada para todas as ocasiões.\"\n\n"
                    + "Na próxima revolta, a carta de aviso oferece contratar mercenários do Sindicato, " + (d.sindicatoDiscount * 100f).ToString("0") + "% mais baratos na primeira vez "
                    + "(desconto de filiação). Letras miúdas: o Sindicato não se responsabiliza por filiados que entram em greve com o pagamento no bolso.",
                    LetterDefOf.NeutralEvent, null, sind);
                parts.Add("Sindicato: desconto de " + (d.sindicatoDiscount * 100f).ToString("0") + "%");
            }
            return LastReport = "reações: " + (parts.Count == 0 ? "nenhuma" : string.Join("; ", parts));
        }

        private static void SendReaction(string kind, string label, string text, Faction f)
        {
            var letter = (ChoiceLetter_EQReaction)LetterMaker.MakeLetter(label, text, DefDatabase<LetterDef>.GetNamed("EQ_ReactionLetter"), LookTargets.Invalid, f);
            letter.kind = kind;
            Find.LetterStack.ReceiveLetter(letter);
        }

        /// <summary>Resposta a uma carta de reação. null = feito.</summary>
        public static string Answer(string kind, bool accept)
        {
            LegitimacyDef d = LegitimacyDef.Active;
            var c = Comp;
            if (d == null || c == null) return "sem legitimidade";
            Faction pf = Faction.OfPlayer;
            if (kind == "helice")
            {
                Faction h = PowerOf("EQ_Faction_Helice");
                if (h == null) return "a Hélice não existe";
                if (!accept) return LastReport = "franquia recusada";
                c.franchiseUntil = Find.TickManager.TicksGame + (int)(d.heliceDays * GenDate.TicksPerDay);
                h.TryAffectGoodwillWith(pf, d.heliceGoodwill, canSendMessage: false, canSendHostilityLetter: false);
                EQCouncil.Dirty();
                Messages.Message("Franquia assinada: cerimônias " + (-d.heliceCeremony * 100f).ToString("0") + "% mais baratas por " + d.heliceDays.ToString("0") + " dias. O selo da Hélice já está no papel timbrado.", MessageTypeDefOf.PositiveEvent, false);
                return LastReport = "franquia assinada até " + c.franchiseUntil;
            }
            if (kind == "forja")
            {
                Faction f = PowerOf("EQ_Faction_Forja");
                if (f == null) return "a Forja não existe";
                if (!accept)
                {
                    f.TryAffectGoodwillWith(pf, d.forjaRefuse, canSendMessage: false, canSendHostilityLetter: false);
                    return LastReport = "registro recusado (boa vontade " + d.forjaRefuse.ToString("+0;-0") + ")";
                }
                Map m = Find.AnyPlayerHomeMap;
                if (m == null || EQPlayerFrontier.CountOn(m, ThingDefOf.Steel) < d.forjaSteel) return "Faltam " + d.forjaSteel + " de aço na colônia.";
                EQPlayerFrontier.TakeFrom(m, ThingDefOf.Steel, d.forjaSteel);
                f.TryAffectGoodwillWith(pf, d.forjaGoodwill, canSendMessage: false, canSendHostilityLetter: false);
                c.Add(d.forjaLegitimacy, "registro na Forja");
                Messages.Message(pf.Name + " consta no Livro de Registro das Nações Produtivas. A Forja agora trata o seu país como cliente.", MessageTypeDefOf.PositiveEvent, false);
                return LastReport = "registro pago (" + d.forjaSteel + " de aço)";
            }
            return "reação desconhecida: " + kind;
        }

        // ------------------------------------------------------------ comandos do trono

        public static string CanCrown(Building_Throne th)
        {
            var c = Comp;
            if (!On || c == null) return "Indisponível";
            if (!c.crownPending) return "O líder já foi coroado.";
            Pawn p = EQLeader.IdeoLeader;
            if (p == null) return "Não há líder para coroar: escolha um na aba Ideologia (ou designe um sucessor no conselho).";
            if (!EQNation.IsThroneRoom(th)) return "O trono precisa estar numa sala do trono de verdade (fechada e com teto).";
            if (!p.Spawned || p.Map != th.Map) return p.LabelShort + " precisa estar no mapa do trono.";
            if (p.Downed || p.InMentalState) return p.LabelShort + " não está em condições de subir ao trono.";
            return null;
        }

        public static Command CrownCommand(Building_Throne th)
        {
            var c = Comp;
            Pawn p = EQLeader.IdeoLeader;
            int left = c == null ? 0 : c.crownDeadline - Find.TickManager.TicksGame;
            var cmd = new Command_Action
            {
                defaultLabel = "Coroar " + (p?.LabelShort ?? "o herdeiro"),
                defaultDesc = "O novo líder sobe ao trono e é reproclamado diante da região. Sem isso, em "
                              + (left / (float)GenDate.TicksPerDay).ToString("0.#") + " dias os vizinhos concluem que o país está sem dono e vêm conferir.",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/ThroneSpeech", false) ?? BaseContent.BadTex,
                action = () =>
                {
                    string err = c?.Crown(th);
                    if (err != null) Messages.Message(err, MessageTypeDefOf.RejectInput, false);
                }
            };
            string why = CanCrown(th);
            if (why != null) cmd.Disable(why);
            return cmd;
        }
    }

    /// <summary>Mercenários do Sindicato do Saque: contratados na carta da revolta, chegam com ela e lutam do lado do jogador.</summary>
    public static class EQMercs
    {
        /// <summary>Teste: força a fuga (true) ou a chegada (false).</summary>
        public static bool? TestFlee;

        public static Faction Faction
        {
            get
            {
                FactionDef def = LegitimacyDef.Active?.mercFaction;
                if (def == null || Find.FactionManager == null) return null;
                Faction f = Find.FactionManager.FirstFactionOfDef(def);
                if (f == null)
                {
                    // Save antigo ou mundo sem a seção: cria na hora.
                    try
                    {
                        f = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(def));
                        Find.FactionManager.Add(f);
                    }
                    catch (Exception ex) { Log.Error("[Ecos da Queda] Não consegui criar a Seção de Contratos: " + ex); return null; }
                }
                return f != null && !f.defeated ? f : null;
            }
        }

        public static int Cost(float revoltPoints)
        {
            LegitimacyDef d = LegitimacyDef.Active;
            if (d == null) return 0;
            float disc = WorldComponent_EQLegitimacy.Current?.mercDiscount ?? 0f;
            return Mathf.Max(10, Mathf.CeilToInt(revoltPoints * d.mercPointsFactor * d.mercSilverPerPoint * (1f - disc) / 10f) * 10);
        }

        public static string CannotHire(float revoltPoints, bool hired, Map m)
        {
            Faction f = Faction;
            if (LegitimacyDef.Active == null || f == null) return "O Sindicato não existe neste mundo.";
            if (f.HostileTo(Faction.OfPlayer)) return "O Sindicato não aceita contrato de quem atira nos filiados.";
            if (hired) return "Já contratados.";
            if (m == null || EQPlayerFrontier.SilverOn(m) < Cost(revoltPoints)) return "Faltam " + Cost(revoltPoints) + " de prata na colônia.";
            return null;
        }

        /// <summary>Paga o contrato. null = feito.</summary>
        public static string Hire(Map m, float revoltPoints, ref bool hired)
        {
            string why = CannotHire(revoltPoints, hired, m);
            if (why != null) return why;
            int cost = Cost(revoltPoints);
            WorldComponent_EQNation.TakeSilver(m, cost);
            hired = true;
            var c = WorldComponent_EQLegitimacy.Current;
            if (c != null) c.mercDiscount = 0f;
            EQLegitimacy.LastReport = "mercenários contratados por " + cost + " de prata";
            Messages.Message("Contrato assinado com o Sindicato do Saque (" + cost + " de prata). Os filiados chegam junto com a revolta.", MessageTypeDefOf.NeutralEvent, false);
            return null;
        }

        public static string LastArrival;

        /// <summary>Na chegada da revolta. Às vezes o Sindicato some com a prata.</summary>
        public static List<Pawn> Arrive(Map m, float revoltPoints)
        {
            var list = new List<Pawn>();
            LegitimacyDef d = LegitimacyDef.Active;
            Faction f = Faction;
            if (d == null || f == null || m == null) { LastArrival = "sem Sindicato"; return list; }
            bool flee = TestFlee ?? Rand.Chance(d.mercFleeChance);
            if (flee)
            {
                LastArrival = "fugiram com a prata";
                Find.LetterStack.ReceiveLetter("Os mercenários não vieram",
                    "O Sindicato do Saque manda dizer que houve uma assembleia, que a categoria decidiu por uma paralisação e que a prata fica \"retida para o fundo de greve\". "
                    + "Os revoltosos, esses vieram.", LetterDefOf.NegativeEvent, null, f);
                return list;
            }
            try
            {
                var gp = new PawnGroupMakerParms { groupKind = PawnGroupKindDefOf.Combat, faction = f, points = Mathf.Max(100f, revoltPoints * d.mercPointsFactor), tile = m.Tile };
                list = PawnGroupMakerUtility.GeneratePawns(gp).ToList();
            }
            catch (Exception ex) { Log.Warning("[Ecos da Queda] Mercenários do Sindicato: " + ex.Message); }
            if (list.Count == 0 || !RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entry, m, CellFinder.EdgeRoadChance_Friendly))
            {
                LastArrival = "não consegui gerar";
                return new List<Pawn>();
            }
            // Os vizinhos revoltosos são inimigos dos filiados (greve é greve, mas contrato é contrato).
            Faction rf = EQNation.RevoltFaction;
            try { if (rf != null && !f.HostileTo(rf)) f.SetRelationDirect(rf, FactionRelationKind.Hostile, canSendHostilityLetter: false); }
            catch (Exception ex) { Log.Warning("[Ecos da Queda] Relação Sindicato × revolta: " + ex.Message); }
            foreach (Pawn p in list) GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(entry, m, 6), m);
            IntVec3 fallback = WorldComponent_EQLegitimacy.Current?.revolt?.throne?.Position ?? WorldComponent_EQNation.Current?.throne?.Position ?? m.Center;
            LordMaker.MakeNewLord(f, new LordJob_AssistColony(f, fallback), m, list);
            LastArrival = "chegaram " + list.Count + " (" + string.Join(",", list.GroupBy(p => p.kindDef.defName).Select(g => g.Key + "x" + g.Count())) + ")"
                          + " hostis_à_revolta=" + (rf != null && f.HostileTo(rf));
            Find.LetterStack.ReceiveLetter("Os mercenários chegaram",
                list.Count + " filiados do Sindicato do Saque chegaram com crachá, marmita e a ordem de serviço assinada. Lutam do seu lado até o fim da revolta; depois, vão embora contando a prata.",
                LetterDefOf.PositiveEvent, new LookTargets(list), f);
            return list;
        }

        /// <summary>Fim da revolta: quem ficou de pé vai embora.</summary>
        public static int Dismiss(List<Pawn> mercs, Map m)
        {
            Faction f = Faction;
            var left = mercs?.Where(p => p != null && !p.Dead && p.Spawned && p.Map == m && !p.Downed).ToList() ?? new List<Pawn>();
            if (left.Count > 0 && f != null && !f.HostileTo(Faction.OfPlayer))
            {
                foreach (Pawn p in left) p.GetLord()?.RemovePawn(p);
                LordMaker.MakeNewLord(f, new LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.Walk, canDig: false), m, left);
                if (!EQLeader.SuppressDialogs)
                    Messages.Message("Os mercenários do Sindicato bateram o ponto e foram embora.", MessageTypeDefOf.NeutralEvent, false);
            }
            mercs?.Clear();
            return left.Count;
        }
    }

    /// <summary>Estado da legitimidade e da sucessão da nação do jogador.</summary>
    public class WorldComponent_EQLegitimacy : WorldComponent
    {
        public bool initialized;
        public float value;
        public Pawn crowned;
        public Pawn lastHeir;
        public Pawn heirAtLoss;
        public bool crownPending;
        public int crownDeadline = -1;
        public float successionFactor = 1f;
        public EQMinorRevolt revolt;
        public int unrestCooldownUntil = -1;
        public int franchiseUntil = -1;
        public float mercDiscount;
        public int lastRaidTick = -999999;
        public List<string> log = new List<string>();

        public WorldComponent_EQLegitimacy(World world) : base(world) { }

        public static WorldComponent_EQLegitimacy Current => Find.World?.GetComponent<WorldComponent_EQLegitimacy>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref initialized, "initialized");
            Scribe_Values.Look(ref value, "value");
            Scribe_References.Look(ref crowned, "crowned");
            Scribe_References.Look(ref lastHeir, "lastHeir");
            Scribe_References.Look(ref heirAtLoss, "heirAtLoss");
            Scribe_Values.Look(ref crownPending, "crownPending");
            Scribe_Values.Look(ref crownDeadline, "crownDeadline", -1);
            Scribe_Values.Look(ref successionFactor, "successionFactor", 1f);
            Scribe_Deep.Look(ref revolt, "revolt");
            Scribe_Values.Look(ref unrestCooldownUntil, "unrestCooldownUntil", -1);
            Scribe_Values.Look(ref franchiseUntil, "franchiseUntil", -1);
            Scribe_Values.Look(ref mercDiscount, "mercDiscount");
            Scribe_Values.Look(ref lastRaidTick, "lastRaidTick", -999999);
            Scribe_Collections.Look(ref log, "log", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && log == null) log = new List<string>();
        }

        public void Add(float amount, string why, bool silent = false)
        {
            LegitimacyDef d = LegitimacyDef.Active;
            if (d == null || amount == 0f) return;
            if (!initialized) Init(null);
            float before = value;
            value = Mathf.Clamp(value + amount, 0f, d.max);
            EQCouncil.Dirty();
            if (silent) return;
            log.Add(Find.TickManager.TicksGame / GenDate.TicksPerDay + "d " + amount.ToString("+0.#;-0.#") + " " + why);
            while (log.Count > 12) log.RemoveAt(0);
            EQLegitimacy.LastReport = "legitimidade " + before.ToString("0.#") + " → " + value.ToString("0.#") + " (" + why + ")";
            if (Mathf.Abs(amount) >= 3f && !EQLeader.SuppressDialogs)
                Messages.Message("Legitimidade " + amount.ToString("+0;-0") + " (" + why + "): " + value.ToString("0") + "/" + d.max.ToString("0") + ".",
                    amount > 0f ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NegativeEvent, false);
        }

        public void Init(Pawn leader)
        {
            LegitimacyDef d = LegitimacyDef.Active;
            initialized = true;
            value = d?.start ?? 50f;
            crowned = leader ?? EQLeader.IdeoLeader;
            crownPending = crowned == null;
            crownDeadline = crownPending ? Find.TickManager.TicksGame + (int)((d?.crownDays ?? 10f) * GenDate.TicksPerDay) : -1;
            lastHeir = WorldComponent_Allegiance.Current?.successor;
            EQCouncil.Dirty();
        }

        /// <summary>Na vitória da revolta da proclamação.</summary>
        public void OnFounding(WorldComponent_EQNation n)
        {
            initialized = false;
            log.Clear();
            Init(n?.leader);
            revolt = null;
            EQLegitimacy.FoundingReactions(n?.throne ?? EQLegitimacy.FindThrone(n?.map));
        }

        public override void WorldComponentTick()
        {
            if (Find.TickManager.TicksGame % 250 != 0 || !EQLegitimacy.On) return;
            LegitimacyDef d = LegitimacyDef.Active;
            if (!initialized) Init(null);
            int now = Find.TickManager.TicksGame;
            SuccessionCheck();
            if (revolt != null) RevoltTick();
            if (now % GenDate.TicksPerHour == 0)
            {
                Add(DailyDrift(out _) / 24f, "o dia a dia", silent: true);
                if (value < d.unrestBelow && revolt == null && now >= unrestCooldownUntil && Rand.MTBEventOccurs(d.unrestMtbDays, GenDate.TicksPerDay, GenDate.TicksPerHour))
                    StartRevolt("unrest", d.unrestPointsFactor, false);
            }
        }

        /// <summary>Variação diária e o detalhe dela.</summary>
        public float DailyDrift(out string detail)
        {
            LegitimacyDef d = LegitimacyDef.Active;
            var parts = new List<string>();
            float v = 0f;
            Pawn leader = EQLeader.IdeoLeader;
            Building_Throne th = EQLegitimacy.FindThrone();
            if (leader == null) { v += d.noLeaderDaily; parts.Add("sem líder " + d.noLeaderDaily.ToString("+0.##;-0.##")); }
            else if (th != null && !crownPending) { v += d.throneDaily; parts.Add("líder coroado no trono " + d.throneDaily.ToString("+0.##")); }
            if (th == null) { v += d.noThroneDaily; parts.Add("sala do trono sem trono " + d.noThroneDaily.ToString("+0.##;-0.##")); }
            var rep = WorldComponent_Reputation.Current;
            var s = ReputationSettingsDef.Settings;
            if (rep != null && s != null && s.maxValue > 0f)
            {
                var recs = Find.FactionManager.AllFactionsVisible.Where(WorldComponent_Reputation.Tracks).Select(rep.Get).ToList();
                if (recs.Count > 0)
                {
                    float fame = recs.Max(r => r.fame) / s.maxValue, inf = recs.Max(r => r.infamy) / s.maxValue;
                    if (d.fameDaily * fame >= 0.005f) { v += d.fameDaily * fame; parts.Add("Fama " + (d.fameDaily * fame).ToString("+0.##")); }
                    if (-d.infamyDaily * inf >= 0.005f) { v += d.infamyDaily * inf; parts.Add("Infâmia " + (d.infamyDaily * inf).ToString("+0.##;-0.##")); }
                }
            }
            detail = parts.Count == 0 ? "nada" : string.Join(", ", parts);
            return v;
        }

        // ------------------------------------------------------------ sucessão coroada

        public void SuccessionCheck()
        {
            LegitimacyDef d = LegitimacyDef.Active;
            int now = Find.TickManager.TicksGame;
            Pawn cur = EQLeader.IdeoLeader;
            var alg = WorldComponent_Allegiance.Current;
            if (crowned != null && cur == crowned)
            {
                lastHeir = alg?.successor;
                return;
            }
            var gov = EQGov.ActiveDefs();
            if (crowned != null)
            {
                bool lost = crowned.Dead || crowned.Destroyed || crowned.Faction != Faction.OfPlayer || crowned.IsPrisoner;
                string who = crowned.LabelShort;
                Add(lost ? d.leaderDied : d.leaderChanged, lost ? "o líder " + who + " " + (crowned.Dead ? "morreu" : "foi perdido") : "troca de líder");
                heirAtLoss = lastHeir;
                crowned = null;
                crownPending = true;
                crownDeadline = now + (int)(d.crownDays * GenDate.TicksPerDay);
                successionFactor = d.successionPointsFactor;
                if (lost && gov.Any(g => g.successionCouncilFalls))
                {
                    var council = WorldComponent_EQCouncil.Current;
                    int n = council?.seats.Count ?? 0;
                    council?.seats.Clear();
                    EQCouncil.Dirty();
                    if (n > 0) Find.LetterStack.ReceiveLetter("O conselho caiu",
                        "Sem o deus, os conselheiros descobriram que eram só pessoas sentadas em cadeiras. " + n + " cadeiras vazias; nomeie de novo no trono.", LetterDefOf.NegativeEvent);
                }
                bool now2 = lost && gov.Any(g => g.successionRevoltNow);
                Find.LetterStack.ReceiveLetter("Sucessão: o trono espera",
                        (lost ? who + " não ocupa mais o trono." : who + " deixou o cargo de líder.")
                        + " O novo líder precisa ser coroado no trono (botão \"Coroar\") em até " + d.crownDays.ToString("0") + " dias, senão a região conclui que o país está sem dono."
                        + (EQGov.Succession() is string rule ? "\n\nRegra da ideologia: " + rule : "")
                        + (now2 ? "\n\nO deus morreu: a revolta não espera prazo." : ""),
                        LetterDefOf.NegativeEvent);
                if (now2)
                {
                    float f = gov.Where(g => g.successionRevoltNow).Max(g => g.successionRevoltFactor);
                    StartRevolt("succession", d.successionPointsFactor * f, true);
                }
            }
            // Sem potência, o sucessor designado também assume (com potência, a Lealdade já faz isso).
            if (cur == null && alg?.successor != null && alg.ActivePower == null && EQLeader.Assign(alg.successor))
            {
                alg.successor = null;
                cur = EQLeader.IdeoLeader;
            }
            if (cur != null && crownPending)
            {
                if (gov.Any(g => g.successionNoCrown)) { Crowned(cur, "a república segue sem coroa"); return; }
                if (cur == heirAtLoss && gov.Any(g => g.successionHeirCrowned)) { Crowned(cur, "o herdeiro designado assume sem cerimônia"); return; }
            }
            if (crownPending && crownDeadline >= 0 && now >= crownDeadline && revolt == null)
            {
                float f = d.successionPointsFactor;
                if (cur == null || cur != heirAtLoss) f *= gov.Count == 0 ? 1f : Mathf.Max(1f, gov.Max(g => g.successionNoHeirFactor));
                crownDeadline = -1;
                StartRevolt("succession", f, false);
            }
        }

        private void Crowned(Pawn p, string how)
        {
            crowned = p;
            lastHeir = WorldComponent_Allegiance.Current?.successor;
            crownPending = false;
            crownDeadline = -1;
            heirAtLoss = null;
            if (revolt != null && revolt.Succession && !revolt.started && !revolt.forced)
            {
                revolt = null;
                how += "; a revolta da sucessão foi desmarcada";
            }
            EQLegitimacy.LastReport = "coroado " + p.LabelShort + " (" + how + ")";
            Find.LetterStack.ReceiveLetter("Coroação: " + p.LabelShort,
                p.LabelShort + " é o líder de " + Faction.OfPlayer.Name + " (" + how + ").", LetterDefOf.PositiveEvent, p.Spawned ? new LookTargets(p) : LookTargets.Invalid);
        }

        /// <summary>Botão "Coroar" no trono. null = feito.</summary>
        public string Crown(Building_Throne th)
        {
            string why = EQLegitimacy.CanCrown(th);
            if (why != null) return why;
            Pawn p = EQLeader.IdeoLeader;
            Add(LegitimacyDef.Active.crowned, "coroação de " + p.LabelShort);
            Crowned(p, "reproclamado no trono");
            return null;
        }

        // ------------------------------------------------------------ revoltas menores

        public string StartRevolt(string kind, float factor, bool forced)
        {
            LegitimacyDef d = LegitimacyDef.Active;
            if (revolt != null) return "já há uma revolta";
            Building_Throne th = EQLegitimacy.FindThrone(WorldComponent_EQNation.Current?.map);
            Map m = th?.Map ?? Find.AnyPlayerHomeMap;
            if (m == null || EQNation.RevoltFaction == null) return "sem mapa ou sem facção";
            revolt = new EQMinorRevolt
            {
                kind = kind,
                forced = forced,
                map = m,
                throne = th,
                points = EQNation.RevoltPoints(m, 0) * factor,
                arriveTick = Find.TickManager.TicksGame + (int)(Rand.Range(d.revoltDelayMinDays, d.revoltDelayMaxDays) * GenDate.TicksPerDay),
            };
            string text = (revolt.Succession
                    ? "A região não reconhece " + (EQLeader.IdeoLeader?.LabelShort ?? "um trono vazio") + " como líder de " + Faction.OfPlayer.Name + ". O síndico redigiu um abaixo-assinado novo, com espaço para mais assinaturas."
                    : "A legitimidade de " + Faction.OfPlayer.Name + " anda baixa (" + value.ToString("0") + "/100), e os vizinhos perceberam. O síndico convocou uma assembleia extraordinária com pauta única: \"quem manda aqui?\".")
                + "\n\nChegam em 1 a 2 dias, cerca de " + Mathf.Max(3, Mathf.RoundToInt(revolt.points / 85f)) + " pessoas. Vencer dá legitimidade; se tomarem a sala do trono por "
                + NationDef.Active.throneHoldHours.ToString("0") + " horas, a nação perde legitimidade"
                + (EQGov.Rule(g => g.guiltyExpiation) ? " (a ideologia chama isso de expiação: perder também conta a favor)." : ".");
            var letter = (ChoiceLetter_EQUnrest)LetterMaker.MakeLetter("Revolta: " + revolt.Label, text, DefDatabase<LetterDef>.GetNamed("EQ_UnrestLetter"), th != null ? new LookTargets(th) : LookTargets.Invalid);
            letter.revoltId = revolt.arriveTick;
            Find.LetterStack.ReceiveLetter(letter);
            return EQLegitimacy.LastReport = "revolta menor (" + kind + ") marcada: " + revolt.points.ToString("0") + " pontos em "
                                            + ((revolt.arriveTick - Find.TickManager.TicksGame) / (float)GenDate.TicksPerDay).ToString("0.0") + " dias";
        }

        public string Bribe()
        {
            if (revolt == null || revolt.started) return "Não há revolta para comprar.";
            if (revolt.bribed) return "Os que se vendiam já foram comprados.";
            int cost = EQNation.BribeCost(revolt.points);
            if (EQPlayerFrontier.SilverOn(revolt.map) < cost) return "Faltam " + cost + " de prata na colônia.";
            WorldComponent_EQNation.TakeSilver(revolt.map, cost);
            NationDef nd = NationDef.Active;
            float cut = Rand.Range(nd.bribeMinCut, nd.bribeMaxCut);
            revolt.points *= 1f - cut;
            revolt.bribed = true;
            return EQLegitimacy.LastReport = "suborno de " + cost + ": -" + (cut * 100f).ToString("0") + "% (" + revolt.points.ToString("0") + " pontos)";
        }

        public string HireMercs()
        {
            if (revolt == null || revolt.started) return "Não há revolta marcada.";
            return EQMercs.Hire(revolt.map, revolt.points, ref revolt.mercsHired);
        }

        public void ArriveNow()
        {
            if (revolt == null || revolt.started) return;
            var r = revolt;
            Map m = r.map != null && Find.Maps.Contains(r.map) ? r.map : Find.AnyPlayerHomeMap;
            r.map = m;
            r.revolters = WorldComponent_EQNation.SpawnRevolt(m, EQNation.RevoltFaction, r.points, "Revolta: " + r.Label,
                (r.Succession ? "Os vizinhos vieram desfazer a sucessão." : "Os vizinhos vieram perguntar quem manda aqui.") + " Vença para recuperar a legitimidade.");
            if (r.mercsHired) r.mercs = EQMercs.Arrive(m, r.points);
            r.started = true;
            r.throneTicks = 0;
            EQLegitimacy.LastReport = "revolta menor chegou: " + r.revolters.Count + " pessoas (" + r.points.ToString("0") + " pontos)"
                                     + (r.mercsHired ? "; mercenários: " + EQMercs.LastArrival : "");
            if (r.revolters.Count == 0) EndRevolt(true, "ninguém apareceu");
        }

        private void RevoltTick()
        {
            var r = revolt;
            if (!r.started) { if (Find.TickManager.TicksGame >= r.arriveTick) ArriveNow(); return; }
            CheckRevolt();
        }

        public void CheckRevolt()
        {
            var r = revolt;
            if (r == null || !r.started) return;
            Room room = r.throne != null && r.throne.Spawned ? r.throne.GetRoom() : null;
            if (room != null && r.revolters.Any(p => !WorldComponent_EQNation.OutOfFight(p, r.map) && p.GetRoom() == room))
            {
                r.throneTicks += 250;
                if (r.throneTicks >= NationDef.Active.throneHoldHours * GenDate.TicksPerHour) { EndRevolt(false, "os revoltosos tomaram a sala do trono"); return; }
            }
            if (r.revolters.All(p => WorldComponent_EQNation.OutOfFight(p, r.map))) EndRevolt(true, "vizinhos derrotados");
        }

        public void EndRevolt(bool won, string why)
        {
            LegitimacyDef d = LegitimacyDef.Active;
            var r = revolt;
            if (r == null) return;
            revolt = null;
            unrestCooldownUntil = Find.TickManager.TicksGame + (int)(d.unrestCooldownDays * GenDate.TicksPerDay);
            int merc = EQMercs.Dismiss(r.mercs, r.map);
            bool guilty = EQGov.Rule(g => g.guiltyExpiation);
            if (won) Add(d.revoltWon, "revolta vencida (" + r.Label + ")");
            else Add(guilty ? d.guiltyRevoltLost : d.revoltLost, guilty ? "revolta perdida: expiação" : "revolta perdida (" + r.Label + ")");
            if (!won)
            {
                Faction f = EQNation.RevoltFaction;
                var left = r.revolters.Where(p => !WorldComponent_EQNation.OutOfFight(p, r.map)).ToList();
                if (left.Count > 0 && f != null)
                {
                    foreach (Pawn p in left) p.GetLord()?.RemovePawn(p);
                    LordMaker.MakeNewLord(f, new LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.Walk, canDig: false), r.map, left);
                }
            }
            string succ = "";
            if (r.Succession)
            {
                Pawn cur = EQLeader.IdeoLeader;
                crownPending = cur == null;
                crowned = cur;
                crownDeadline = crownPending ? Find.TickManager.TicksGame + (int)(d.crownDays * GenDate.TicksPerDay) : -1;
                heirAtLoss = null;
                succ = cur == null ? " O trono continua vazio." : won ? " " + cur.LabelShort + " está coroado: ninguém sobrou para discordar." : " " + cur.LabelShort + " governa assim mesmo, uma região que não esquece.";
            }
            string rep = (won ? "revolta menor vencida" : "revolta menor perdida") + " (" + why + "); legitimidade " + value.ToString("0") + (merc > 0 ? "; mercenários indo embora " + merc : "");
            Find.LetterStack.ReceiveLetter(won ? "Revolta vencida" : "Revolta perdida",
                (won ? "Os vizinhos voltaram para casa (os que puderam). " : "Os vizinhos ocuparam a sala do trono, leram o abaixo-assinado em voz alta e foram embora satisfeitos. ")
                + "Legitimidade agora: " + value.ToString("0") + "/" + d.max.ToString("0") + " (" + EQLegitimacy.Tier(value) + ")." + succ,
                won ? LetterDefOf.PositiveEvent : LetterDefOf.NegativeEvent);
            EQLegitimacy.LastReport = rep;
        }

        public string Report()
        {
            LegitimacyDef d = LegitimacyDef.Active;
            int now = Find.TickManager.TicksGame;
            if (!EQLegitimacy.On) return "legitimidade: inativa (nação não fundada de verdade)";
            float drift = DailyDrift(out string detail);
            return "legitimidade=" + value.ToString("0.0") + " (" + EQLegitimacy.Tier(value) + ") dia " + drift.ToString("+0.##;-0.##") + " [" + detail + "]"
                   + " coroado=" + (crowned?.LabelShort ?? "-") + " líder=" + (EQLeader.IdeoLeader?.LabelShort ?? "-")
                   + (crownPending ? " coroação_pendente prazo=" + (crownDeadline < 0 ? "-" : ((crownDeadline - now) / (float)GenDate.TicksPerDay).ToString("0.0") + "d") + " herdeiro=" + (heirAtLoss?.LabelShort ?? "-") : "")
                   + " sucessor=" + (WorldComponent_Allegiance.Current?.successor?.LabelShort ?? "-")
                   + (revolt != null ? " revolta=" + revolt.kind + (revolt.started ? " em curso " + revolt.revolters.Count(p => !WorldComponent_EQNation.OutOfFight(p, revolt.map)) + "/" + revolt.revolters.Count : " em " + ((revolt.arriveTick - now) / (float)GenDate.TicksPerDay).ToString("0.0") + "d") + " pontos=" + revolt.points.ToString("0") + (revolt.mercsHired ? " mercs" : "") + (revolt.bribed ? " suborno" : "") : "")
                   + (franchiseUntil > now ? " franquia=" + ((franchiseUntil - now) / (float)GenDate.TicksPerDay).ToString("0") + "d" : "")
                   + (mercDiscount > 0f ? " desconto_merc=" + (mercDiscount * 100f).ToString("0") + "%" : "")
                   + " fronteira×" + EQLegitimacy.FrontierFactor.ToString("0")
                   + " canais=" + string.Join(",", EQLegitimacy.ExtraChannels().Select(kv => kv.Key + kv.Value.ToString("+0.##;-0.##")))
                   + "\n  últimos: " + string.Join(" | ", log.Skip(Math.Max(0, log.Count - 5)));
        }
    }

    /// <summary>Carta de reação de uma potência à fundação (Hélice: franquia; Forja: registro).</summary>
    public class ChoiceLetter_EQReaction : ChoiceLetter
    {
        public string kind;
        public bool answered;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly || answered)
                {
                    yield return Option_Close;
                    yield break;
                }
                LegitimacyDef d = LegitimacyDef.Active;
                string yes = kind == "helice" ? "Assinar a franquia" : "Pagar o registro (" + d?.forjaSteel + " de aço)";
                string no = kind == "helice" ? "Recusar (sem efeito)" : "Recusar (boa vontade " + d?.forjaRefuse.ToString("+0;-0") + ")";
                var accept = new DiaOption(yes) { resolveTree = true };
                if (kind == "forja" && (Find.AnyPlayerHomeMap == null || EQPlayerFrontier.CountOn(Find.AnyPlayerHomeMap, ThingDefOf.Steel) < (d?.forjaSteel ?? 0)))
                    accept.Disable("falta aço");
                accept.action = () =>
                {
                    string err = EQLegitimacy.Answer(kind, true);
                    if (err != null && !err.StartsWith("franquia") && !err.StartsWith("registro")) Messages.Message(err, MessageTypeDefOf.RejectInput, false);
                    else { answered = true; Find.LetterStack.RemoveLetter(this); }
                };
                yield return accept;
                var refuse = new DiaOption(no) { resolveTree = true };
                refuse.action = () => { EQLegitimacy.Answer(kind, false); answered = true; Find.LetterStack.RemoveLetter(this); };
                yield return refuse;
                yield return Option_Postpone;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref kind, "kind");
            Scribe_Values.Look(ref answered, "answered");
        }
    }

    /// <summary>Aviso de revolta menor: esperar, comprar parte dos vizinhos, contratar mercenários.</summary>
    public class ChoiceLetter_EQUnrest : ChoiceLetter
    {
        /// <summary>Qual revolta esta carta anuncia (arriveTick dela).</summary>
        public int revoltId = -1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref revoltId, "revoltId", -1);
        }

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                var c = WorldComponent_EQLegitimacy.Current;
                var r = c?.revolt;
                if (ArchivedOnly || r == null || r.started || r.arriveTick != revoltId)
                {
                    yield return Option_Close;
                    yield break;
                }
                var wait = new DiaOption("Esperar a revolta") { resolveTree = true };
                wait.action = () => Find.LetterStack.RemoveLetter(this);
                yield return wait;
                int cost = EQNation.BribeCost(r.points);
                var bribe = new DiaOption("Comprar parte dos vizinhos (" + cost + " de prata: a revolta vem 30–40% menor)") { resolveTree = true };
                if (r.bribed) bribe.Disable("já comprados");
                else if (EQPlayerFrontier.SilverOn(r.map) < cost) bribe.Disable("falta prata");
                bribe.action = () => { c.Bribe(); };
                yield return bribe;
                yield return MercOption(r.points, r.mercsHired, r.map, () => c.HireMercs());
                yield return Option_Postpone;
            }
        }

        public static DiaOption MercOption(float points, bool hired, Map map, Func<string> hire)
        {
            var o = new DiaOption("Contratar mercenários do Sindicato (" + EQMercs.Cost(points) + " de prata: chegam com a revolta; às vezes não chegam)") { resolveTree = true };
            string why = EQMercs.CannotHire(points, hired, map);
            if (why != null) o.Disable(why.TrimEnd('.').ToLowerInvariant());
            o.action = () =>
            {
                string err = hire();
                if (err != null) Messages.Message(err, MessageTypeDefOf.RejectInput, false);
            };
            return o;
        }
    }

    // ------------------------------------------------------------ ganchos

    [HarmonyPatch(typeof(Building_Throne), nameof(Building_Throne.GetGizmos))]
    public static class Patch_Building_Throne_GetGizmos_Crown
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Building_Throne __instance)
        {
            foreach (Gizmo g in gizmos) yield return g;
            if (EQLegitimacy.On && __instance.Faction == Faction.OfPlayer && WorldComponent_EQLegitimacy.Current?.crownPending == true)
                yield return EQLegitimacy.CrownCommand(__instance);
        }
    }

    /// <summary>Raid hostil rechaçado num mapa da colônia (metade ou mais caiu): legitimidade.</summary>
    [HarmonyPatch(typeof(LordManager), nameof(LordManager.RemoveLord))]
    public static class Patch_LordManager_RemoveLord_Legitimacy
    {
        public static void Prefix(Lord oldLord)
        {
            try
            {
                if (!EQLegitimacy.On || oldLord?.faction == null || oldLord.LordJob == null) return;
                LegitimacyDef d = LegitimacyDef.Active;
                Map m = oldLord.Map;
                if (m == null || !m.IsPlayerHome || !oldLord.faction.HostileTo(Faction.OfPlayer) || oldLord.faction == EQNation.RevoltFaction) return;
                string job = oldLord.LordJob.GetType().Name;
                if (!job.Contains("Assault") && !job.Contains("Siege") && !job.Contains("StageThenAttack")) return;
                if (oldLord.numPawnsEverGained < d.raidMinPawns || oldLord.numPawnsLostViolently * 2 < oldLord.numPawnsEverGained) return;
                var c = WorldComponent_EQLegitimacy.Current;
                if (c == null || Find.TickManager.TicksGame - c.lastRaidTick < GenDate.TicksPerDay) return;
                c.lastRaidTick = Find.TickManager.TicksGame;
                c.Add(d.raidRepelled, "ataque de " + oldLord.faction.Name + " rechaçado");
            }
            catch (Exception ex) { Log.WarningOnce("[Ecos da Queda] Legitimidade/raid: " + ex.Message, 0x51E9A1); }
        }
    }

    /// <summary>Executar um conselheiro: tira legitimidade, ou dá (Corte de sangue).</summary>
    [HarmonyPatch(typeof(ExecutionUtility), nameof(ExecutionUtility.DoExecutionByCut))]
    public static class Patch_ExecutionUtility_DoExecutionByCut_Legitimacy
    {
        public static void Prefix(Pawn victim, out bool __state)
        {
            __state = false;
            try { __state = EQLegitimacy.On && victim != null && EQCouncil.IsMember(victim); } catch { }
        }

        public static void Postfix(Pawn victim, bool __state)
        {
            if (!__state) return;
            LegitimacyDef d = LegitimacyDef.Active;
            bool blood = EQGov.Rule(g => g.bloodCourtLegitimacy);
            EQLegitimacy.Event(blood ? d.bloodCourtExecuted : d.councilExecuted, blood ? "corte de sangue: conselheiro " + victim.LabelShort + " executado" : "conselheiro " + victim.LabelShort + " executado");
        }
    }
}
