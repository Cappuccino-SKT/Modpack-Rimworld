using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Um preceito do modelo. Se "name" for dado, o preceito (cargo, ritual...) recebe esse nome fixo.</summary>
    public class IdeoTemplatePrecept
    {
        public PreceptDef def;
        public string name;
        /// <summary>Só para PreferredXenotype.</summary>
        public XenotypeDef xenotype;
    }

    /// <summary>
    /// Modelo fixo de ideologia para uma facção do mod. Aplicado uma única vez por facção
    /// (ver WorldComponent_IdeoTemplates), garantindo que toda partida tenha a mesma ideologia.
    /// </summary>
    public class IdeoTemplateDef : Def
    {
        public FactionDef faction;

        public string ideoName;
        public string adjective;
        public string memberName;
        public string ideoDescription;
        public string leaderTitleMale;
        public string leaderTitleFemale;

        public IdeoIconDef icon;
        public ColorDef color;

        public List<DeityPreset> deities;

        /// <summary>Se verdadeiro, remove preceitos, cargos e rituais que não estão no modelo.</summary>
        public bool exactPrecepts = true;
        public List<IdeoTemplatePrecept> precepts = new List<IdeoTemplatePrecept>();

        /// <summary>Nome fixo do líder fundador (primeiro nome conforme o gênero do pawn).</summary>
        public string leaderFirstNameMale;
        public string leaderFirstNameFemale;
        public string leaderNickname;
        public string leaderLastName;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string e in base.ConfigErrors())
                yield return e;
            if (faction == null)
                yield return "IdeoTemplateDef sem faction.";
            for (int i = 0; i < precepts.Count; i++)
                if (precepts[i]?.def == null)
                    yield return "Preceito nº " + i + " sem def (defName errado ou mod ausente).";
        }
    }
}
