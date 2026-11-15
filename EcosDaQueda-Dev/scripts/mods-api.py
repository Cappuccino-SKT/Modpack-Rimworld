# Gera uma referência de API (Markdown) de uma DLL de mod já descompilada com decompile.ps1.
# Uso: python -I mods-api.py <pasta descompilada> <titulo> <saida.md>
# Lista, por categoria, as classes que um modder usa no XML: Defs, DefModExtensions,
# CompProperties (+ a Comp), HediffComps, AbilityComps, PatchOperations, Workers etc.,
# com os campos públicos (nome, tipo, valor padrão) e o resumo /// da classe, se houver.
import io, os, re, sys
from collections import defaultdict

src, title, out = sys.argv[1], sys.argv[2], sys.argv[3]

# base -> categoria (as bases que indicam "uso por XML")
CATS = [
    ('Defs (tipos novos de Def)', r'^(Def|BuildableDef|ThingDef|HediffDef|AbilityDef|GeneDef|ResearchProjectDef|PreceptDef|RitualPatternDef|QuestScriptDef|WorldObjectDef|PawnKindDef|\w+Def)$'),
    ('DefModExtensions (`<modExtensions>`)', r'^DefModExtension$'),
    ('CompProperties de Thing (`<comps>`)', r'^CompProperties(_\w+)?$'),
    ('CompProperties de Hediff', r'^HediffCompProperties(_\w+)?$'),
    ('Comps de habilidade (VEF/Royalty)', r'^(AbilityCompProperties\w*|CompProperties_AbilityEffect\w*|AbilityExtension_\w+|AbilityExtension)$'),
    ('PatchOperations (`Patches/*.xml`)', r'^PatchOperation\w*$'),
    ('Workers / classes para `workerClass`, `thingClass`, `hediffClass`', r'^(\w+Worker\w*|Building\w*|ThingWithComps|Thing|Hediff\w*|Projectile\w*|Bullet|Verb\w*|Pawn|Apparel|JobDriver\w*|WorkGiver\w*|ThinkNode\w*|IncidentWorker\w*|QuestNode\w*|RitualOutcomeEffectWorker\w*|RitualObligationTargetFilter\w*|RitualBehaviorWorker\w*|GenStep\w*|SymbolResolver\w*|PlaceWorker|StatPart|Ability|MapComponent|WorldComponent|GameComponent|Gizmo\w*|Command\w*|ITab\w*|Alert\w*)$'),
    ('Mod settings / ModBase', r'^(Mod|ModSettings|ModBase)$'),
]
# internal também conta: o XML instancia por nome (Class="Ns.Tipo") sem precisar de public
cls_re = re.compile(r'^\s*(?:public|internal)\s+(?:abstract\s+|sealed\s+|static\s+|partial\s+)*class\s+(\w+)(?:<[^>]+>)?\s*:\s*([\w.<>]+)', re.M)
field_re = re.compile(r'^\s*public\s+(?!static|const|override|virtual|abstract|class|enum|struct|interface|event|delegate)([\w.<>,\[\]? ]+?)\s+(\w+)\s*(?:=\s*([^;]+))?;', re.M)
summary_re = re.compile(r'((?:\s*///[^\n]*\n)+)\s*public[^\n]*class\s+(\w+)')

classes = {}  # name -> (base, ns, fields, summary, file)
for dp, _, fs in os.walk(src):
    for f in fs:
        if not f.endswith('.cs'):
            continue
        t = io.open(os.path.join(dp, f), encoding='utf-8', errors='replace').read()
        ns = (re.search(r'^namespace\s+([\w.]+)', t, re.M) or [None, ''])[1]
        sums = {m.group(2): re.sub(r'\s*///\s*(<[^>]+>)?', ' ', m.group(1)).strip() for m in summary_re.finditer(t)}
        for m in cls_re.finditer(t):
            name, base = m.group(1), m.group(2).split('.')[-1].split('<')[0]
            # corpo da classe: do "{" até a próxima declaração de classe no mesmo arquivo (aproximação)
            start = t.find('{', m.end())
            nxt = cls_re.search(t, m.end())
            body = t[start: nxt.start() if nxt else len(t)]
            fields = [(fm.group(2), fm.group(1).strip(), (fm.group(3) or '').strip()) for fm in field_re.finditer(body)
                      if '(' not in fm.group(1)]
            classes[name] = (base, ns, fields, sums.get(name, ''), os.path.relpath(os.path.join(dp, f), src))

# resolve herança dentro da própria DLL (ex.: X : MeuDefBase : Def)
def root_base(name, seen=()):
    b = classes[name][0]
    if b in classes and b not in seen:
        r = root_base(b, seen + (name,))
        return r
    return b

groups = defaultdict(list)
for name, (base, ns, fields, summ, fn) in classes.items():
    rb = root_base(name)
    for cat, pat in CATS:
        if re.match(pat, base) or re.match(pat, rb):
            groups[cat].append(name)
            break

lines = ['# Referência de API: ' + title, '',
         'Gerado por `scripts/mods-api.py` a partir da DLL descompilada (só leitura; não editar à mão).',
         'Campos = membros públicos que o XML pode preencher. Para os detalhes, ver o guia em `mods/frameworks/`.', '']
total = 0
for cat, _ in CATS:
    names = sorted(groups.get(cat, []), key=lambda n: (classes[n][1], n))
    if not names:
        continue
    lines += ['## ' + cat + ' (%d)' % len(names), '']
    for n in names:
        base, ns, fields, summ, fn = classes[n]
        total += 1
        full = (ns + '.' if ns else '') + n
        lines.append('### `%s` : %s' % (full, base))
        if summ:
            lines.append(summ[:400])
        if fields:
            lines.append('')
            lines += ['- `%s` (%s)%s' % (fn_, ty, ' = `%s`' % dv[:60] if dv else '') for fn_, ty, dv in fields[:40]]
            if len(fields) > 40:
                lines.append('- … +%d campos (ver `%s`)' % (len(fields) - 40, fn))
        lines.append('')
io.open(out, 'w', encoding='utf-8', newline='\n').write('\n'.join(lines) + '\n')
print(out, total, 'classes')
