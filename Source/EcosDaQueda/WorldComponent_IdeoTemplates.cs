using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Ao criar (ou carregar) um mundo, aplica o IdeoTemplateDef de cada facção do mod à ideologia dela.
    /// Guarda quais facções já receberam o modelo para não sobrescrever mudanças feitas depois em jogo.
    /// </summary>
    public class WorldComponent_IdeoTemplates : WorldComponent
    {
        private List<int> appliedFactionIds = new List<int>();

        public WorldComponent_IdeoTemplates(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref appliedFactionIds, "appliedFactionIds", LookMode.Value);
            if (appliedFactionIds == null)
                appliedFactionIds = new List<int>();
        }

        private bool pending;

        public override void FinalizeInit(bool fromLoad)
        {
            base.FinalizeInit(fromLoad);
            // Ler um .rid usa o Scribe; se um save ainda está sendo carregado, aplica no primeiro Update.
            if (Scribe.mode != LoadSaveMode.Inactive) { pending = true; return; }
            ApplyAll();
        }

        public override void WorldComponentUpdate()
        {
            base.WorldComponentUpdate();
            if (!pending || Scribe.mode != LoadSaveMode.Inactive) return;
            pending = false;
            ApplyAll();
        }

        private void ApplyAll()
        {
            if (!ModsConfig.IdeologyActive || world?.factionManager == null)
                return;

            foreach (IdeoTemplateDef template in DefDatabase<IdeoTemplateDef>.AllDefsListForReading)
            {
                foreach (Faction faction in world.factionManager.AllFactionsListForReading)
                {
                    if (faction.def != template.faction || EQUtil.IsProxy(faction) || appliedFactionIds.Contains(faction.loadID))
                        continue;
                    Ideo ideo = faction.ideos?.PrimaryIdeo;
                    if (ideo == null)
                        continue;
                    try
                    {
                        IdeoTemplateApplier.Apply(template, ideo, faction);
                        appliedFactionIds.Add(faction.loadID);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("[Ecos da Queda] Falha ao aplicar o modelo de ideologia " + template.defName + " em " + faction.Name + ": " + ex);
                    }
                }
            }
        }
    }

    public static class IdeoTemplateApplier
    {
        public static void Apply(IdeoTemplateDef t, Ideo ideo, Faction faction)
        {
            if (!t.ideoFile.NullOrEmpty() && TryApplyFile(t, ideo, faction))
            {
                NameLeader(t, faction);
                return;
            }
            ApplyFields(t, ideo, faction);
        }

        /// <summary>Carrega Ideos/&lt;ideoFile&gt;.rid do mod e copia tudo para a ideologia da facção.</summary>
        private static bool TryApplyFile(IdeoTemplateDef t, Ideo ideo, Faction faction)
        {
            string path = System.IO.Path.Combine(t.modContentPack.RootDir, "Ideos", t.ideoFile + ".rid");
            if (!System.IO.File.Exists(path))
            {
                Log.Error("[Ecos da Queda] Ideologia não encontrada: " + path);
                return false;
            }
            if (!GameDataSaveLoader.TryLoadIdeo(path, out Ideo loaded) || loaded == null)
                return false;
            loaded.CopyTo(ideo);
            ideo.fileName = null;
            ideo.RecachePrecepts();
            // Os cargos são preenchidos pelo próprio jogo (Precept_Role.Tick); RecacheActivity não pode rodar
            // aqui porque durante a criação do mundo ainda não existe facção do jogador.
            return true;
        }

        private static void NameLeader(IdeoTemplateDef t, Faction faction)
        {
            Pawn leader = faction.leader;
            if (leader != null && !t.leaderLastName.NullOrEmpty())
            {
                string first = leader.gender == Gender.Female ? (t.leaderFirstNameFemale ?? t.leaderFirstNameMale) : (t.leaderFirstNameMale ?? t.leaderFirstNameFemale);
                leader.Name = new NameTriple(first, t.leaderNickname ?? first, t.leaderLastName);
            }
        }

        private static void ApplyFields(IdeoTemplateDef t, Ideo ideo, Faction faction)
        {
            // Identidade
            if (!t.ideoName.NullOrEmpty()) ideo.name = t.ideoName;
            if (!t.adjective.NullOrEmpty()) ideo.adjective = t.adjective;
            if (!t.memberName.NullOrEmpty()) ideo.memberName = t.memberName;
            if (!t.leaderTitleMale.NullOrEmpty()) ideo.leaderTitleMale = t.leaderTitleMale;
            if (!t.leaderTitleFemale.NullOrEmpty()) ideo.leaderTitleFemale = t.leaderTitleFemale;
            if (!t.ideoDescription.NullOrEmpty())
            {
                ideo.description = t.ideoDescription;
                ideo.descriptionLocked = true;
            }
            if (t.icon != null || t.color != null)
                ideo.SetIcon(t.icon ?? ideo.iconDef, t.color ?? ideo.colorDef, clearPrimaryFactionColor: true);

            // Divindades
            if (t.deities != null && ideo.foundation is IdeoFoundation_Deity deityFoundation)
            {
                var list = new List<IdeoFoundation_Deity.Deity>();
                foreach (DeityPreset p in t.deities)
                {
                    list.Add(new IdeoFoundation_Deity.Deity
                    {
                        name = p.nameType.name,
                        type = p.nameType.type,
                        gender = p.gender,
                        iconPath = p.iconPath
                    });
                }
                deityFoundation.SetDeities(list);
            }

            // Preceitos, cargos e rituais
            var keep = new HashSet<Precept>();
            foreach (IdeoTemplatePrecept tp in t.precepts)
            {
                if (tp?.def == null)
                    continue;
                Precept existing = ideo.PreceptsListForReading.FirstOrDefault(p =>
                    p.def == tp.def && !keep.Contains(p) &&
                    (tp.xenotype == null || (p as Precept_Xenotype)?.xenotype == tp.xenotype));

                if (existing == null)
                {
                    if (tp.def.issue != null && !tp.def.issue.allowMultiplePrecepts)
                    {
                        foreach (Precept same in ideo.PreceptsListForReading.Where(p => p.def.issue == tp.def.issue && !keep.Contains(p)).ToList())
                            ideo.RemovePrecept(same, replacing: true);
                    }
                    existing = PreceptMaker.MakePrecept(tp.def);
                    if (tp.xenotype != null && existing is Precept_Xenotype px)
                        px.xenotype = tp.xenotype;
                    ideo.AddPrecept(existing, init: true, faction.def, tp.def.ritualPatternBase);
                }

                if (!tp.name.NullOrEmpty())
                {
                    existing.SetName(tp.name);
                    existing.nameLocked = true;
                }
                keep.Add(existing);
            }

            if (t.exactPrecepts)
            {
                foreach (Precept p in ideo.PreceptsListForReading.ToList())
                {
                    if (keep.Contains(p) || p.def == PreceptDefOf.IdeoRole_Leader)
                        continue;
                    // Preceitos ocultos (rituais internos do jogo: julgamento, execução, nascimento...) nunca são removidos.
                    bool removable = p.def.visible && (p.GetType() == typeof(Precept) || p is Precept_Role || p is Precept_Ritual || p is Precept_Xenotype);
                    if (removable)
                        ideo.RemovePrecept(p);
                }
            }

            ideo.RecachePrecepts();

            NameLeader(t, faction);
        }
    }
}
