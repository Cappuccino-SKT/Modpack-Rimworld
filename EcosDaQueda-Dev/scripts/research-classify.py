# Proposta de classificação das pesquisas para a Etapa 3. Lê tests/out/research.tsv (comando `research` do executor).
import csv, collections, sys
rows = list(csv.DictReader(open('tests/out/research.tsv', encoding='utf-8'), delimiter='\t'))
R = {r['defName']: r for r in rows}
import xml.etree.ElementTree as ET
# Fonte única: Mods/EcosDaQueda/1.7/Defs/EQ_Research.xml (mesma regra do EQResearch.Apply no C#).
X = ET.parse('../Mods/EcosDaQueda/1.7/Defs/EQ_Research.xml').getroot()
NAMES = {'EQ_Ramo_Bancada': 'Bancada', 'EQ_Ramo_VelhoMundo': 'Velho Mundo', 'EQ_Ramo_Silencio': 'Silêncio', 'EQ_Ramo_Helice': 'Hélice', 'EQ_Ramo_Forja': 'Forja', 'EQ_Ramo_Coro': 'Coro'}
BR = []
for e in X.findall('EcosDaQueda.ResearchBranchDef'):
    li = lambda tag: [x.text for x in e.findall(tag + '/li')]
    BR.append(dict(name=NAMES[e.findtext('defName')], order=int(e.findtext('order')), free=e.findtext('free') == 'true',
                   projects=set(li('projects')), mods=set(li('mods')), exc=set(li('exceptProjects')), tech=set(li('techLevels'))))
BR.sort(key=lambda b: b['order'])
LISTS = {b['name']: b['projects'] for b in BR}
def cls(r):
    for b in BR:
        if r['defName'] in b['projects']: return b['name']
    for b in BR:
        if r['mod'] in b['mods'] and r['defName'] not in b['exc']: return b['name']
    for b in BR:
        if r['tech'] in b['tech']: return b['name']
    return next(b['name'] for b in BR if b['free'])
by = collections.defaultdict(list)
for r in rows: by[cls(r)].append(r)
out = ['# Pesquisas: proposta de classificação (Etapa 3)', '', '_Gerado por `scripts/research-classify.py` a partir de `tests/out/research.tsv`. Editar `Mods/EcosDaQueda/1.7/Defs/EQ_Research.xml` e gerar de novo._', '']
for k in ['Bancada', 'Velho Mundo', 'Silêncio', 'Hélice', 'Forja', 'Coro']:
    L = by[k]
    out.append(f'## {k} ({len(L)})'); out.append('')
    t = collections.defaultdict(list)
    for r in L: t[r['tech']].append(f"{r['label']} (`{r['defName']}`)")
    for tech in ['Neolithic', 'Medieval', 'Industrial', 'Spacer', 'Ultra', 'Undefined']:
        if t[tech]: out.append(f"- **{tech}** ({len(t[tech])}): " + '; '.join(sorted(t[tech]))); 
    out.append('')
missing = [k for v in LISTS.values() for k in v if k not in R]
if missing: out.append('Não encontrados: ' + ' '.join(missing))
open('tecnico/pesquisa-classificacao.md', 'w', encoding='utf-8').write('\n'.join(out) + '\n')
print({k: len(v) for k, v in by.items()}, 'faltando:', missing)
