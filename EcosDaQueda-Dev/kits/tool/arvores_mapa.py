"""Classifica todo o conteúdo liberável do catálogo nas árvores propostas. Saída: contagens + kits/arvores-mapa.tsv."""
import json, os, re, sys, collections
KITS = r'C:\Users\Felipe\Desktop\RimWorld\EcosDaQueda-Dev\kits'
c = json.load(open(os.path.join(KITS, 'kitcatalog.json'), encoding='utf-8'))
R = c['research']
faz = json.load(open(os.path.join(KITS, 'pesquisa', 'Fazenda.json'), encoding='utf-8'))
in_faz = {u for n in faz['nodes'] for u in n.get('unlocks', [])}
U = [u for u in R['unlocks'] if not u['def'].startswith('Seed_')]

def m(rx, s):
    return re.search(rx, s or '', re.I) is not None

def tree(u):
    d, cat, mod, kind, lab = u['def'], u.get('cat') or '', u['mod'] or '', u['kind'], u['label']
    if d in in_faz or kind == 'plant':
        return 'Fazenda'
    if kind == 'recipe':
        if cat == 'Cirurgia':
            return 'Medicina'
        return 'Produção'
    # por mod (conteúdo bem delimitado)
    if m(r'Insectoids|Insector', mod) or cat == 'Insetoides': return 'Insetoides'
    if m(r'^Apparel_Psyfocus|Eltex|Psytrainer', d): return 'Psíquica'
    if kind == 'item' and m(r'^Apparel_|_Apparel_|^CE_Armor_(Gambeson|Jumpsuit|Poncho)', d) and cat in ('Diversos', ''): return 'Vestuário'
    if cat == 'Anti-materiel': return 'Armas e munição'
    if d in ('VitalsMonitor',): return 'Medicina'
    if d in ('CommsConsole', 'OrbitalTradeBeacon', 'ShipLandingBeacon', 'TransportPod', 'PodLauncher', 'VFED_BurnoutIntelScraper'): return 'Comunicação e transporte'
    if d in ('CryptosleepCasket',): return 'Espaço'
    if d in ('GroundPenetratingScanner', 'LongRangeMineralScanner', 'MoisturePump', 'MultiAnalyzer', 'ToolCabinet'): return 'Produção'
    if d in ('FirefoamPopper',): return 'Defesa'
    if m(r'Android', mod) or cat == 'Partes de corpo androide': return 'Androides'
    if m(r'Vehicles', mod) or cat in ('Veículos', 'Peças de carro'): return 'Veículos'
    if m(r'Drone Factory', mod) or m(r'Drone', d): return 'Mecanoides e drones'
    if m(r'Anomaly', mod) or cat in ('Anomaly', 'Carniçal', 'Artefatos'): return 'Vazio'
    if m(r'Psycasts', mod): return 'Psíquica'
    if m(r'Odyssey', mod) or cat in ('Odyssey', 'Nave'): return 'Espaço'
    if m(r'Alpha Memes|Memes and Structures', mod) or cat == 'Ideologia': return 'Ideologia'
    if m(r'Hygiene', mod) or cat in ('Higiene',): return 'Água e higiene'
    if m(r'Sanguophage|Alpha Genes|Genie|Fungoid|Waster', mod): return 'Genética'
    if m(r'Recycling', mod) or cat == 'Pacotes de reciclagem': return 'Logística'
    if m(r'Cooking|Brewing|Bakery|Nutrient Paste', mod) or cat in ('Refeições', 'Condimentos', 'Mostos e xaropes', 'Sopas cruas',
                                                                  'Massas de bolo', 'Alimentos', 'Vegetariano', 'Carne', 'Produtos animais', 'Matéria vegetal'):
        return 'Cozinha e bebidas'
    if m(r'Prosthetics|Medical Module|Harvest Organs', mod) or cat in ('Biônico', 'Protético', 'Ultra-tecnológico', 'Medicamento',
                                                                      'Body restoration', 'Organic optimizer'):
        return 'Medicina'
    if cat == 'Biotech':
        if m(r'Mech|Recharger|Gestator|Subcore|Band|Toxifier|Wastepack|Atomizer', d): return 'Mecanoides e drones'
        return 'Genética'
    if cat == 'Meca-tecnologia' or m(r'^Mech_|Mechlink|Mech', d) and kind == 'item': return 'Mecanoides e drones'
    if cat == 'Drogas': return 'Drogas e química'
    # armas, munição, defesa
    if kind == 'item' and (m(r'Armas|Granadas|Shells?|ammo|Ammo|Arrows|bolts|Rocket|Grenade|Cannon|Bolter|Charged|Railgun|canister|Power Cell|power pack|Missile|Flare|Nail|Musket|Sling|Bore|gauge|Blunderbuss|Catapult|Fuel Cell|Thermal Bolt|Fliegerfaust|Nebelwerfer|ATGM|charge pack|fuel|\d', cat)):
        return 'Armas e munição'
    if cat in ('Armadura', 'Shields', 'Escudos', 'Loadbearing gear'): return 'Armaduras'
    if kind == 'item' and cat in ('Chapéus', 'Chapéu', 'Vestuário', 'Vestuário nobre', 'Utilitário', 'Têxteis', 'Couros'): return 'Vestuário'
    if cat == 'Segurança' or m(r'Trenches|Security', mod): return 'Defesa'
    if cat == 'Energia' or m(r'Generator|Power', mod): return 'Energia'
    if cat == 'Temperatura': return 'Clima'
    if cat in ('Storage', 'Redes de canos') or m(r'Storage', mod): return 'Logística'
    if cat == 'Lazer' or (kind == 'item' and cat in ('Arte', 'Recreação')): return 'Mobília e lazer'
    if cat == 'Mobília' or m(r'Door Mat', mod): return 'Mobília e lazer'
    if kind == 'terrain' or cat in ('Estrutura', 'Pisos'): return 'Construção'
    if cat == 'Produção' or m(r'Production', mod) or cat in ('Manufaturado', 'Blocos de pedra', 'Recursos brutos', 'Inacabado'): return 'Produção'
    if cat == 'Diversos': return 'Mobília e lazer'
    return '?'

rows, cnt, mods, res = [], collections.Counter(), collections.defaultdict(collections.Counter), collections.defaultdict(collections.Counter)
for u in U:
    t = tree(u)
    cnt[t] += 1
    mods[t][u['mod']] += 1
    for r in u.get('research') or ['(sem pesquisa)']:
        res[t][r] += 1
    rows.append((t, u['kind'], u['def'], u['label'], u['mod'], u.get('cat') or '', ','.join(u.get('research') or [])))
for t, n in cnt.most_common():
    print('%-22s %5d  mods: %s' % (t, n, ', '.join('%s %d' % x for x in mods[t].most_common(5))))
if '--detail' in sys.argv:
    for t in sys.argv[2:]:
        print('==', t)
        for r in sorted(x for x in rows if x[0] == t):
            print('  ', r[1], r[2], '|', r[3], '|', r[4][:25], '|', r[5], '|', r[6])
if '--write' in sys.argv:
    with open(os.path.join(KITS, 'arvores-mapa.tsv'), 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('arvore\ttipo\tdef\trotulo\tmod\tcategoria\tpesquisa_atual\n')
        for r in sorted(rows):
            fh.write('\t'.join(r) + '\n')
    
