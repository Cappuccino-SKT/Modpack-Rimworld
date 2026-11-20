"""Árvores de pesquisa do editor (página /pesquisa do kits.py).

Cada árvore é kits/pesquisa/<id>.json:
  {id, label, desc, active, ownTab, branch, replace,
   nodes: [{id, label, desc, cost, tech, x, y, prereqs: [id de nó], unlocks: [defName], branch, bench,
            facilities: [defName], techprints, techprintTags: [tag], absorbs: [ResearchProjectDef original], notes}
           | {id, ref: <ResearchProjectDef original>, x, y}]}
Nó com "ref" é uma pesquisa que já existe no jogo, posta na árvore só para servir de pré-requisito.
"absorbs" lista pesquisas originais que o nó substitui: elas somem e tudo que dependia delas passa a depender do nó.
build_research() gera Mods/EcosDaQueda/Defs/Pesquisa/EQ_Pesquisa.xml com as árvores ativas: uma ResearchTabDef por
árvore (se ownTab) e um ResearchProjectDef EQ_R_<árvore>_<nó> por nó, com a extensão EcosDaQueda.ResearchEditorExt
(desbloqueios, ramo, manter aba). O código do mod (ResearchEditor.cs) aplica os desbloqueios na carga.
"""
import json
import os
import re

KITS = os.path.dirname(os.path.abspath(__file__))
GAME = os.path.dirname(os.path.dirname(KITS))
TREES = os.path.join(KITS, 'pesquisa')
OUT = os.path.join(GAME, 'Mods', 'EcosDaQueda', 'Defs', 'Pesquisa', 'EQ_Pesquisa.xml')
ID_RE = re.compile(r'^[A-Za-z0-9_]+$')
TECHS = ['Neolithic', 'Medieval', 'Industrial', 'Spacer', 'Ultra', 'Archotech']


def load_trees():
    out = []
    if os.path.isdir(TREES):
        for f in sorted(os.listdir(TREES)):
            if f.endswith('.json'):
                with open(os.path.join(TREES, f), encoding='utf-8') as fh:
                    out.append(json.load(fh))
    return out


def save_tree(t, old_id=None):
    if not ID_RE.match(t.get('id', '')):
        raise ValueError('id inválido: use letras sem acento, números e _')
    for n in t.get('nodes', []):
        if not ID_RE.match(n.get('id', '')):
            raise ValueError('nome interno inválido no nó "%s"' % n.get('label', n.get('id')))
    os.makedirs(TREES, exist_ok=True)
    with open(os.path.join(TREES, t['id'] + '.json'), 'w', encoding='utf-8', newline='\n') as fh:
        json.dump(t, fh, ensure_ascii=False, indent=1)
        fh.write('\n')
    if old_id and old_id != t['id']:
        delete_tree(old_id)


def delete_tree(tid):
    f = os.path.join(TREES, os.path.basename(tid) + '.json')
    if os.path.exists(f):
        os.remove(f)


def proj_def(tree_id, node_id):
    return 'EQ_R_%s_%s' % (tree_id, node_id)


def _x(s):
    return str(s).replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;')


def _num(v, d):
    try:
        return float(v)
    except (TypeError, ValueError):
        return d


def _fmt(v):
    return ('%.2f' % v).rstrip('0').rstrip('.')


def build_research(catalog=None):
    """Gera o XML; devolve (cabeçalho, avisos)."""
    research = (catalog or {}).get('research') or {}
    known_proj = {p['def'] for p in research.get('projects', [])}
    known_unl = {u['def'] for u in research.get('unlocks', [])}
    branches = {b['def'] for b in research.get('branches', [])}
    report = []
    lines = ['<?xml version="1.0" encoding="utf-8"?>',
             '<!-- GERADO por EcosDaQueda-Dev/kits/kits.py (página Pesquisa do editor). Não editar à mão: mude em kits/pesquisa/*.json. -->',
             '<Defs>']
    n_proj = n_trees = 0
    claimed = {}
    claimed_abs = {}
    for t in load_trees():
        if not t.get('active'):
            continue
        n_trees += 1
        tid = t['id']
        nodes = t.get('nodes', [])
        by_id = {n['id']: n for n in nodes}
        real = [n for n in nodes if not n.get('ref')]
        own_tab = t.get('ownTab', True)
        tab = 'EQ_RTab_' + tid
        if own_tab:
            lines += ['', '  <ResearchTabDef>', '    <defName>%s</defName>' % tab, '    <label>%s</label>' % _x(t.get('label') or tid)]
            if t.get('desc'):
                lines.append('    <generalTitle>%s</generalTitle>' % _x(t.get('label') or tid))
                lines.append('    <generalDescription>%s</generalDescription>' % _x(t['desc']))
            lines.append('  </ResearchTabDef>')
        for n in nodes:
            if n.get('ref') and known_proj and n['ref'] not in known_proj:
                report.append('%s: pesquisa original não existe no catálogo: %s' % (tid, n['ref']))

        # ciclos
        def cyc(nid, seen, stack):
            if nid in stack:
                return True
            if nid in seen:
                return False
            seen.add(nid)
            stack.add(nid)
            r = any(cyc(p, seen, stack) for p in by_id.get(nid, {}).get('prereqs', []) if p in by_id)
            stack.discard(nid)
            return r
        if any(cyc(n['id'], set(), set()) for n in real):
            report.append('%s: há um ciclo de pré-requisitos (A exige B que exige A); o jogo não aceita' % tid)

        for n in real:
            d = proj_def(tid, n['id'])
            n_proj += 1
            name = '%s "%s"' % (tid, n.get('label') or n['id'])
            unl = [u for u in n.get('unlocks', []) if u]
            if not unl:
                report.append('%s: não desbloqueia nada' % name)
            for u in unl:
                if known_unl and u not in known_unl:
                    report.append('%s: desbloqueio não existe no catálogo: %s' % (name, u))
                if u in claimed:
                    report.append('%s: %s também é desbloqueado por %s (fica com quem carregar por último)' % (name, u, claimed[u]))
                claimed[u] = name
            tech = n.get('tech') or 'Industrial'
            if tech not in TECHS:
                tech = 'Industrial'
            pre = []
            for p in n.get('prereqs', []):
                q = by_id.get(p)
                if not q:
                    report.append('%s: pré-requisito some da árvore: %s' % (name, p))
                    continue
                pre.append(q['ref'] if q.get('ref') else proj_def(tid, q['id']))
            if n.get('notes'):
                lines.append('')
                lines.append('  <!-- %s -->' % _x(n['notes']).replace('--', '—'))
            else:
                lines.append('')
            lines += ['  <ResearchProjectDef>',
                      '    <defName>%s</defName>' % d,
                      '    <label>%s</label>' % _x(n.get('label') or n['id']),
                      '    <description>%s</description>' % _x(n.get('desc') or n.get('label') or n['id']),
                      '    <baseCost>%d</baseCost>' % int(_num(n.get('cost'), 500)),
                      '    <techLevel>%s</techLevel>' % tech]
            if own_tab:
                lines.append('    <tab>%s</tab>' % tab)
            lines += ['    <researchViewX>%s</researchViewX>' % _fmt(_num(n.get('x'), 0)),
                      '    <researchViewY>%s</researchViewY>' % _fmt(_num(n.get('y'), 0))]
            if pre:
                lines.append('    <prerequisites>' + ''.join('<li>%s</li>' % p for p in pre) + '</prerequisites>')
            if n.get('bench'):
                lines.append('    <requiredResearchBuilding>%s</requiredResearchBuilding>' % n['bench'])
            if n.get('facilities'):
                lines.append('    <requiredResearchFacilities>' + ''.join('<li>%s</li>' % f for f in n['facilities']) + '</requiredResearchFacilities>')
            tp = int(_num(n.get('techprints'), 0))
            if tp > 0:
                lines.append('    <techprintCount>%d</techprintCount>' % tp)
                tags = n.get('techprintTags') or []
                if tags:
                    lines.append('    <heldByFactionCategoryTags>' + ''.join('<li>%s</li>' % x for x in tags) + '</heldByFactionCategoryTags>')
                else:
                    report.append('%s: tem techprints mas nenhuma facção que os guarde (só aparecem em comerciante)' % name)
            branch = n.get('branch') or t.get('branch') or ''
            if branch and branches and branch not in branches:
                report.append('%s: ramo não existe: %s' % (name, branch))
                branch = ''
            lines += ['    <modExtensions>', '      <li Class="EcosDaQueda.ResearchEditorExt">']
            if unl:
                lines.append('        <unlocks>' + ''.join('<li>%s</li>' % u for u in unl) + '</unlocks>')
            ab = [a for a in n.get('absorbs', []) if a]
            for a in ab:
                if known_proj and a not in known_proj and not a.startswith('EQ_R_'):
                    report.append('%s: pesquisa absorvida não existe no catálogo: %s' % (name, a))
                if a in claimed_abs:
                    report.append('%s: %s também é absorvida por %s' % (name, a, claimed_abs[a]))
                claimed_abs[a] = name
            if ab:
                lines.append('        <absorbs>' + ''.join('<li>%s</li>' % a for a in ab) + '</absorbs>')
            if not t.get('replace', True):
                lines.append('        <replace>false</replace>')
            if branch:
                lines.append('        <branch>%s</branch>' % branch)
            if not own_tab:
                lines.append('        <keepTab>false</keepTab>')
            lines += ['      </li>', '    </modExtensions>', '  </ResearchProjectDef>']
    lines += ['', '</Defs>', '']
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('\n'.join(lines))
    return '%d árvores ativas, %d pesquisas -> %s' % (n_trees, n_proj, os.path.relpath(OUT, GAME)), report
