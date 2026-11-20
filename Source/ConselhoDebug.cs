using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// N2 (NACAO.md › 3.4): "council [show|recruit [n]|sit &lt;cadeira&gt; [i]|empty &lt;cadeira&gt;|skill &lt;cadeira&gt; &lt;nível&gt;|cooldown|goodwill|down &lt;cadeira&gt;|thoughts]".
    /// </summary>
    public static class EQConselhoDebug
    {
        private static WorldComponent_EQCouncil C => WorldComponent_EQCouncil.Current;

        private static List<Pawn> Eligible() => EQLeader.AllFreeColonists().Distinct().Where(p => EQCouncil.CannotSit(p) == null).ToList();

        public static string Run(string[] a)
        {
            string sub = a.Length > 1 ? a[1] : "show";
            var c = C;
            CouncilDef d = CouncilDef.Active;
            if (c == null || d == null) return "sem conselho";
            string key = a.Length > 2 ? a[2] : null;
            switch (sub)
            {
                case "show": return Show();
                case "recruit":
                {
                    Map map = Find.AnyPlayerHomeMap;
                    int n = a.Length > 2 ? int.Parse(a[2]) : 3;
                    for (int i = 0; i < n; i++)
                    {
                        Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer, forceGenerateNewPawn: true,
                            developmentalStages: DevelopmentalStage.Adult, mustBeCapableOfViolence: true));
                        if (p.ideo != null && EQLeader.PlayerIdeo != null) p.ideo.SetIdeo(EQLeader.PlayerIdeo);
                        GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(map.Center, map, 8), map);
                    }
                    return n + " colonos novos; " + Show();
                }
                case "sit":
                {
                    List<Pawn> el = Eligible().Where(p => !c.seats.ContainsValue(p)).ToList();
                    int i = a.Length > 3 ? int.Parse(a[3]) : 0;
                    if (i < 0 || i >= el.Count) return "candidato inválido (há " + el.Count + ")";
                    string err = c.Assign(key, el[i]);
                    return (err != null ? "RECUSADO: " + err : "sentou " + el[i].LabelShort + " em " + key) + "\n" + Show();
                }
                case "leader":
                {
                    if (EQLeader.IdeoLeader == null && EQLeader.Candidates().Count > 0) EQLeader.Assign(EQLeader.Candidates().Last());
                    Pawn l = EQLeader.IdeoLeader;
                    if (l == null) return "sem líder";
                    return "líder na cadeira " + key + ": " + (c.Assign(key, l) ?? "ACEITO (ERRO)");
                }
                case "empty": return (c.Assign(key, null) ?? "vazia") + "\n" + Show();
                case "skill":
                {
                    Pawn p = c.Holder(key);
                    if (p == null) return "cadeira vazia";
                    int lv = int.Parse(a[3]);
                    foreach (SkillDef s in d.Seat(key).skills) { var r = p.skills.GetSkill(s); if (r != null) r.Level = lv; }
                    return p.LabelShort + " perícia " + lv + "\n" + Show();
                }
                case "cooldown":
                    foreach (string k in c.lastChange.Keys.ToList()) c.lastChange[k] = -999999;
                    return "esperas zeradas";
                case "down":
                {
                    Pawn p = c.Holder(key);
                    if (p == null) return "cadeira vazia";
                    HealthUtility.DamageUntilDowned(p, allowBleedingWounds: false);
                    return p.LabelShort + " caído: " + (EQCouncil.Inactive(p, d.Seat(key)) ?? "valendo") + "\n" + Show();
                }
                case "goodwill":
                {
                    Faction f = EQAllegiance.Powers.FirstOrDefault(x => !x.HostileTo(Faction.OfPlayer)) ?? EQAllegiance.Powers.FirstOrDefault();
                    if (f == null) return "sem potência";
                    int before = f.GoodwillWith(Faction.OfPlayer);
                    f.TryAffectGoodwillWith(Faction.OfPlayer, 10, canSendMessage: false, canSendHostilityLetter: false);
                    return "boa vontade com " + f.Name + ": +10 pedido → " + (f.GoodwillWith(Faction.OfPlayer) - before) + " (fator " + EQCouncil.GoodwillFactor.ToString("0.00") + ")";
                }
                case "thoughts":
                {
                    ThoughtDef m = DefDatabase<ThoughtDef>.GetNamed("EQ_ConselhoMembro"), o = DefDatabase<ThoughtDef>.GetNamed("EQ_ConselhoPreterido");
                    return string.Join(" | ", EQLeader.AllFreeColonists().Distinct().Select(p =>
                    {
                        var list = new List<Thought>();
                        p.needs?.mood?.thoughts?.GetAllMoodThoughts(list);
                        return p.LabelShort + ":" + (list.Any(t => t.def == m) ? " membro" : "") + (list.Any(t => t.def == o) ? " preterido" : "");
                    }));
                }
            }
            return "subcomando desconhecido: " + sub;
        }

        public static string Show()
        {
            Pawn l = EQLeader.IdeoLeader;
            return EQCouncil.Report() + "\n  líder=" + (l?.LabelShort ?? "-") + " fundada=" + EQNation.Founded
                   + " candidatos=" + string.Join(", ", Eligible().Select(p => p.LabelShort + "(soc " + p.skills.GetSkill(SkillDefOf.Social).Level + ", int " + p.skills.GetSkill(SkillDefOf.Intellectual).Level + ")"));
        }
    }
}
