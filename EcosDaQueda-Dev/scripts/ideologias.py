"""Gera as ideologias fixas do Ecos da Queda a partir dos .rid salvos no editor de ideologia do jogo.

Fonte: EcosDaQueda-Dev/ideologias/rid/*.rid (tudo: memes, preceitos, cargos, rituais, estilos, relíquias)
       + ideologias/ideologias.json (só nomes e textos trocados).
Saída: Mods/EcosDaQueda/Ideos/<saida>.rid, carregado pelo mod (IdeoTemplateDef.ideoFile) e copiado para a
       ideologia da facção ao criar o mundo.

Uso (na raiz do jogo):  python -X utf8 -I EcosDaQueda-Dev/scripts/ideologias.py
Avisa quando um nome do json não bate com o .rid (o usuário renomeou no editor) e lista os nomes que
ainda parecem automáticos/em inglês.
"""
import json
import os
import re
import sys
import xml.etree.ElementTree as ET

RAIZ = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
DEV = os.path.join(RAIZ, "EcosDaQueda-Dev", "ideologias")
SAIDA = os.path.join(RAIZ, "Mods", "EcosDaQueda", "Ideos")

# Rituais internos do jogo, iguais em toda ideologia (não precisam de nome próprio)
GENERICOS = {"assento de ritual", "ligação com árvore anima", "nascimento de criança", "execução pública",
             "Lançamento da Gravinave", "troca de função", "discurso do trono", "ritual de conexão com árvore",
             "julgamento", "conceder honra", "conceder título", "discurso real", "funeral sem corpo"}
INGLES = re.compile(r"\b(the|of|royal|sovereign|machine|youthful|brutal|ideo|fleshy|flesh|cult|iron|leader|moral|match|"
                    r"supreme|commissar|funeral \(|bestow|address|agon|sanguis)\b|\(a\)", re.I)


def main():
    meta = json.load(open(os.path.join(DEV, "ideologias.json"), encoding="utf-8"))
    os.makedirs(SAIDA, exist_ok=True)
    problemas = 0
    for cfg in meta["ideologias"]:
        tree = ET.parse(os.path.join(DEV, "rid", cfg["rid"] + ".rid"))
        ideo = tree.getroot().find("ideo")
        # identidade
        for campo in ("name", "adjective", "memberName", "leaderTitleMale", "leaderTitleFemale"):
            if campo in cfg:
                ideo.find(campo).text = cfg[campo]
        if "description" in cfg:
            for campo in ("description", "descriptionTemplate"):
                el = ideo.find(campo)
                if el is None:
                    el = ET.SubElement(ideo, campo)
                el.text = cfg["description"]
            el = ideo.find("descriptionLocked")
            if el is None:
                el = ET.SubElement(ideo, "descriptionLocked")
            el.text = "True"
        if "salaDeCulto" in cfg:
            el = ideo.find("overriddenWorshipRoomLabel")
            if el is None:
                el = ET.SubElement(ideo, "overriddenWorshipRoomLabel")
            el.text = cfg["salaDeCulto"]
        # divindades
        for d in ideo.find("foundation").find("deities") or []:
            troca = cfg.get("divindades", {}).get(d.findtext("name"))
            if troca:
                d.find("name").text, d.find("type").text = troca
        # preceitos com nome
        trocas = dict(meta["comum"])
        trocas.update(cfg.get("preceitos", {}))
        usados = set()
        suspeitos = []
        for p in ideo.find("precepts"):
            nome = p.find("name")
            if nome is None or nome.text is None:
                continue
            chave = p.findtext("def") + "|" + nome.text
            if chave in trocas:
                nome.text = trocas[chave]
                usados.add(chave)
                trava = p.find("nameLocked")
                if trava is None:
                    trava = ET.SubElement(p, "nameLocked")
                trava.text = "True"
            if p.get("Class") and nome.text not in GENERICOS and INGLES.search(nome.text):
                suspeitos.append(p.findtext("def") + "|" + nome.text)
        for chave in cfg.get("preceitos", {}):
            if chave not in usados:
                problemas += 1
                print(f"  !! {cfg['rid']}: '{chave}' não existe mais no .rid (renomeado no editor?)")
        for campo in ("name", "adjective", "memberName", "leaderTitleMale"):
            if INGLES.search(ideo.findtext(campo) or ""):
                suspeitos.append(campo + "=" + ideo.findtext(campo))
        destino = os.path.join(SAIDA, cfg["saida"] + ".rid")
        tree.write(destino, encoding="utf-8", xml_declaration=True)
        print(f"{cfg['rid']} -> Ideos/{cfg['saida']}.rid | {ideo.findtext('name')} | memes: "
              + ", ".join(m.text for m in ideo.find("memes")))
        if suspeitos:
            print("  nomes que ainda parecem automáticos: " + "; ".join(suspeitos))
    sys.exit(1 if problemas else 0)


if __name__ == "__main__":
    main()
