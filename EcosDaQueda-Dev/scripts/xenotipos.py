"""Gera os XenotypeDefs do Ecos da Queda a partir dos .xtp do editor de xenotipo do jogo.

Fonte: EcosDaQueda-Dev/xenotipos/xtp/*.xtp (genes) + xenotipos/xenotipos.json (defName, textos, herança, padrões).
Saída: Mods/EcosDaQueda/Defs/Xenotypes/EQ_Xenotypes.xml

Cada gene ganha MayRequire="<packageId>" do mod que o define (achado varrendo Mods/ e Data/).
Xeno não hereditário com "bases" ganha doubleXenotypeChances: o jogo sorteia o padrão (endogenes)
e aplica o xeno por cima (xenogenes), como faz com o Sanguófago.

Uso (na raiz do jogo):  python -X utf8 -I EcosDaQueda-Dev/scripts/xenotipos.py [--genes]
  --genes  lista os genes de cada xeno com o rótulo em inglês (para conferir descrições).
"""
import glob
import json
import os
import sys
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape

RAIZ = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
DEV = os.path.join(RAIZ, "EcosDaQueda-Dev", "xenotipos")
SAIDA = os.path.join(RAIZ, "Mods", "EcosDaQueda", "Defs", "Xenotypes", "EQ_Xenotypes.xml")

# packageIds que o nosso mod exige (não precisam de MayRequire)
OBRIGATORIOS = {"ludeon.rimworld", "ludeon.rimworld.royalty", "ludeon.rimworld.ideology",
                "ludeon.rimworld.biotech", "ludeon.rimworld.anomaly", "ludeon.rimworld.odyssey"}


def package_id(pasta_mod):
    about = os.path.join(pasta_mod, "About", "About.xml")
    try:
        return (ET.parse(about).getroot().findtext("packageId") or "").strip().lower()
    except Exception:
        return ""


def mapa_genes():
    """defName do gene -> (packageId, label)."""
    genes = {}
    pastas = [(p, "ludeon.rimworld" + ("" if os.path.basename(p) == "Core" else "." + os.path.basename(p).lower()))
              for p in glob.glob(os.path.join(RAIZ, "Data", "*"))]
    pastas += [(p, package_id(p)) for p in glob.glob(os.path.join(RAIZ, "Mods", "*")) if os.path.isdir(p)]
    for pasta, pid in pastas:
        if not pid or pid.startswith("ecosdaqueda"):
            continue
        for xml in glob.glob(os.path.join(pasta, "**", "*.xml"), recursive=True):
            if os.sep + "Patches" + os.sep in xml or os.sep + "Languages" + os.sep in xml:
                continue
            try:
                root = ET.parse(xml).getroot()
            except Exception:
                continue
            for el in root:
                if not (el.tag.endswith("GeneDef") or el.tag.endswith("GeneTemplateDef")):
                    continue
                dn = el.findtext("defName")
                if not dn:
                    continue
                if el.tag.endswith("GeneTemplateDef"):
                    dn = "TEMPLATE:" + dn
                if dn not in genes:
                    genes[dn] = (pid, (el.findtext("label") or "").strip())
    return genes


def achar_gene(genes, nome):
    """Gene direto ou gerado por GeneTemplateDef (<modelo>_<SkillDef>, ex.: AptitudeStrong_Shooting)."""
    if nome in genes:
        return genes[nome]
    if "_" in nome:
        modelo, resto = nome.rsplit("_", 1)
        if "TEMPLATE:" + modelo in genes:
            pid, label = genes["TEMPLATE:" + modelo]
            return pid, f"{label} ({resto})"
    return None, "?"


def ler_xtp(nome):
    x = ET.parse(os.path.join(DEV, "xtp", nome + ".xtp")).getroot().find("xenotype")
    return [li.text.strip() for li in x.find("genes")], x.findtext("iconDef") or "Basic"


ICONES = {}


def icone(icon_def):
    if not ICONES:
        for xml in glob.glob(os.path.join(RAIZ, "Data", "*", "Defs", "**", "*.xml"), recursive=True):
            try:
                root = ET.parse(xml).getroot()
            except Exception:
                continue
            for el in root.findall("XenotypeIconDef"):
                ICONES[el.findtext("defName")] = el.findtext("texPath")
    return ICONES.get(icon_def, "UI/Icons/Xenotypes/Other")


def main():
    meta = json.load(open(os.path.join(DEV, "xenotipos.json"), encoding="utf-8"))
    genes = mapa_genes()
    listar = "--genes" in sys.argv
    faltando = []
    out = ['<?xml version="1.0" encoding="utf-8"?>',
           "<!-- GERADO por EcosDaQueda-Dev/scripts/xenotipos.py a partir de EcosDaQueda-Dev/xenotipos/. Não editar à mão. -->",
           "<Defs>"]
    for x in meta["xenotipos"]:
        lista, icon_def = (x["genes"], x.get("icon", "Basic")) if "genes" in x else ler_xtp(x["xtp"])
        out.append("")
        out.append("  <XenotypeDef>")
        out.append(f"    <defName>{x['defName']}</defName>")
        out.append(f"    <label>{escape(x['label'])}</label>")
        out.append(f"    <description>{escape(x['description'])}</description>")
        out.append(f"    <descriptionShort>{escape(x['descriptionShort'])}</descriptionShort>")
        out.append(f"    <iconPath>{icone(icon_def)}</iconPath>")
        out.append(f"    <inheritable>{'true' if x['inheritable'] else 'false'}</inheritable>")
        out.append("    <factionlessGenerationWeight>0</factionlessGenerationWeight>")
        if "combatPowerFactor" in x:
            out.append(f"    <combatPowerFactor>{x['combatPowerFactor']}</combatPowerFactor>")
        if x.get("bases"):
            out.append("    <doubleXenotypeChances>")
            for d, c in meta["basesGrupo"][x["bases"]].items():
                out.append(f"      <{d}>{c}</{d}>")
            out.append("    </doubleXenotypeChances>")
        out.append("    <genes>")
        if listar:
            print(f"## {x['label']} ({x['defName']})")
        for g in lista:
            pid, label = achar_gene(genes, g)
            if pid is None:
                faltando.append(f"{x['defName']}: {g}")
            if listar:
                print(f"   {g:40} {label}  [{pid}]")
            attr = f' MayRequire="{pid}"' if pid and pid not in OBRIGATORIOS else ""
            out.append(f"      <li{attr}>{g}</li>")
        out.append("    </genes>")
        out.append("  </XenotypeDef>")
    out.append("")
    out.append("</Defs>")
    os.makedirs(os.path.dirname(SAIDA), exist_ok=True)
    with open(SAIDA, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out) + "\n")
    print(f"{len(meta['xenotipos'])} xenotipos -> {os.path.relpath(SAIDA, RAIZ)}")
    if faltando:
        print("GENES NÃO ENCONTRADOS:\n  " + "\n  ".join(faltando))
        sys.exit(1)


if __name__ == "__main__":
    main()
