# Gera Mods/EcosDaQueda/1.7/Defs/EQ_Permits.xml (permissões por cargo das 4 potências, bloco B).
# Uso: python scripts/permissoes.py   (a partir de EcosDaQueda-Dev)
# Cada linha: (nome, rótulo, descrição, tipo, dados, cargo mínimo 1-7, custo em favor, recarga em dias,
#              posição na grade (coluna, linha), pré-requisito, MayRequire)
# Tipos: drop {item: qtd} | lab (PawnKind, n, dias) | aid (PawnKind, n) | strike (raio, impactos, raio da explosão) | shuttle
import os

P = {
    'Silencio': [
        ('Racao', 'ração de campo', 'Uma caixa com 20 rações de sobrevivência, rotuladas "nutrição experimental, lote 7". Ninguém pergunta o que aconteceu com os lotes 1 a 6.', 'drop', {'MealSurvivalPack': 20}, 1, 3, 45, (0, 0), None, None),
        ('Coleta', 'equipe de coleta', '4 acólitos pesquisadores ajudam por 4 dias. Fazem qualquer trabalho braçal e anotam tudo, inclusive o que você não quer que anotem.', 'lab', ('EQ_Silencio_Acolito', 4, 4), 2, 4, 60, (0, 1), None, None),
        ('Remedio', 'farmácia do arquivo', '15 remédios industriais, catalogados, carimbados e com prazo de validade em latim.', 'drop', {'MedicineIndustrial': 15}, 2, 4, 45, (0, 2), None, None),
        ('Guardioes', 'guardiões da contenção', '4 guardiões da contenção vêm ajudar em combate. São treinados para manter coisas horríveis do lado de dentro; do lado de fora também funcionam.', 'aid', ('EQ_Silencio_Guardiao', 4), 3, 5, 40, (0, 3), None, None),
        ('Bioferrita', 'remessa de bioferrita', '100 de bioferrita "de origem documentada". A documentação está num cofre que ninguém tem coragem de abrir.', 'drop', {'Bioferrite': 100}, 4, 6, 45, (0, 4), None, 'Ludeon.RimWorld.Anomaly'),
        ('Contencao', 'protocolo de contenção total', '8 guardiões da contenção. Usado quando o problema já tem número de catálogo e fome.', 'aid', ('EQ_Silencio_Guardiao', 8), 6, 8, 60, (1, 3), 'Guardioes', None),
    ],
    'Helice': [
        ('Amostras', 'amostras grátis', '15 remédios industriais em embalagem promocional. "Experimente a versão premium por apenas a sua linhagem."', 'drop', {'MedicineIndustrial': 15}, 1, 3, 45, (0, 0), None, None),
        ('Estagiarios', 'estagiários genéticos', '4 estagiários trabalham para você por 4 dias, sem remuneração, pela experiência e por uma linha no currículo genético.', 'lab', ('EQ_Helice_Estagiario', 4, 4), 2, 4, 60, (0, 1), None, None),
        ('Fiscais', 'auditoria armada', '4 fiscais de linhagem vêm resolver o seu problema. Antes, conferem se você tem licença para tê-lo.', 'aid', ('EQ_Helice_Fiscal', 4), 3, 5, 40, (0, 2), None, None),
        ('Premium', 'linha premium', '5 remédios de ultratecnologia. Assinatura mensal; o cancelamento exige comparecimento pessoal e um rim.', 'drop', {'MedicineUltratech': 5}, 4, 6, 45, (0, 3), None, None),
        ('Prototipos', 'lançamento de protótipos', '4 protótipos premium em teste de campo. Se morrerem, a Hélice agradece os dados.', 'aid', ('EQ_Helice_Prototipo', 4), 5, 8, 60, (1, 2), 'Fiscais', None),
        ('Dividendos', 'dividendos', '600 de prata em dividendos. Pagos com atraso, descontada a taxa de administração da taxa de administração.', 'drop', {'Silver': 600}, 6, 6, 45, (0, 4), None, None),
    ],
    'Forja': [
        ('Aco', 'lote de aço', '250 de aço com certificado de conformidade. O certificado pesa mais do que parece.', 'drop', {'Steel': 250}, 1, 3, 45, (0, 0), None, None),
        ('Aprendizes', 'aprendizes de manutenção', '4 aprendizes trabalham por 4 dias. Leem o manual antes de cada tarefa, em voz alta.', 'lab', ('EQ_Forja_Aprendiz', 4, 4), 2, 4, 60, (0, 1), None, None),
        ('Pecas', 'peças de reposição', '15 componentes originais. Usar peça paralela anula a garantia, e a garantia é a sua alma.', 'drop', {'ComponentIndustrial': 15}, 2, 4, 45, (0, 2), None, None),
        ('Cavaleiros', 'cavaleiros da garantia', '4 cavaleiros da garantia vêm atender o seu chamado. Prazo de atendimento: imediato, se o problema puder levar um tiro.', 'aid', ('EQ_Forja_Cavaleiro', 4), 3, 5, 40, (0, 3), None, None),
        ('Recall', 'recall orbital', 'Um impacto orbital no alvo. A Forja chama isso de "recolhimento de produto defeituoso".', 'strike', (2.9, 1, 7.9), 4, 6, 45, (0, 4), None, None),
        ('Plastaco', 'plastaço licenciado', '100 de plastaço. Cada placa tem número de série; se aparecer no mercado negro, a Forja sabe de onde veio.', 'drop', {'Plasteel': 100}, 5, 6, 45, (0, 5), None, None),
        ('Salva', 'recall em massa', 'Uma salva de 6 impactos orbitais. Para quando o defeito é o lote inteiro.', 'strike', (5.9, 6, 5.9), 6, 8, 60, (1, 4), 'Recall', None),
    ],
    'Coro': [
        ('Banquete', 'banquete de cortesia', '20 refeições finas, servidas com etiqueta. Comer com a mão errada é registrado.', 'drop', {'MealFine': 20}, 1, 3, 45, (0, 0), None, None),
        ('Coristas', 'coristas voluntários', '4 coristas trabalham por 4 dias, cantando. Sem parar. Isso não é opcional.', 'lab', ('EQ_Coro_Corista', 4, 4), 2, 4, 60, (0, 1), None, None),
        ('Tenores', 'tenores de guerra', '4 tenores de guerra vêm ajudar em combate. O inimigo ouve antes de ver; geralmente é a última coisa que ouve.', 'aid', ('EQ_Coro_Tenor', 4), 3, 5, 40, (0, 2), None, None),
        ('Liteira', 'liteira do Coro', 'Uma nave de transporte para seu uso, levando gente e carga a até 70 regiões. Chegar a pé é falta de educação.', 'shuttle', None, 4, 8, 40, (0, 3), None, None),
        ('Sopranos', 'sopranos incandescentes', '4 sopranos incandescentes. O agudo final derrete armaduras e casamentos.', 'aid', ('EQ_Coro_Soprano', 4), 5, 7, 60, (1, 2), 'Tenores', None),
        ('Ouro', 'mecenato', '60 de ouro de um mecenas anônimo. Anônimo até a próxima audição, quando ele vai cobrar.', 'drop', {'Gold': 60}, 6, 6, 45, (0, 4), None, None),
    ],
}

WORKER = {'drop': 'RoyalTitlePermitWorker_DropResources', 'lab': 'RoyalTitlePermitWorker_CallLaborers',
          'aid': 'RoyalTitlePermitWorker_CallAid', 'strike': 'RoyalTitlePermitWorker_OrbitalStrike',
          'shuttle': 'RoyalTitlePermitWorker_CallShuttle'}


def gen():
    out = ['<?xml version="1.0" encoding="utf-8"?>', '<Defs>',
           '  <!-- Permissões por cargo (sistema de permissões da Royalty). Cada cargo dá 1 ponto (permitPointsAwarded em',
           '       EQ_Titles.xml › EQ_TitleBase); o uso gasta favor do líder. GERADO por EcosDaQueda-Dev/scripts/permissoes.py. -->']
    for pw, lst in P.items():
        out.append('')
        out.append('  <!-- ===================================================================== ' + pw.upper() + ' -->')
        for (name, label, desc, kind, data, rank, favor, cd, pos, prereq, mayreq) in lst:
            mr = ' MayRequire="%s"' % mayreq if mayreq else ''
            o = ['  <RoyalTitlePermitDef%s>' % mr,
                 '    <defName>EQ_Permit_%s_%s</defName>' % (pw, name),
                 '    <label>%s</label>' % label,
                 '    <description>%s</description>' % desc,
                 '    <workerClass>%s</workerClass>' % WORKER[kind],
                 '    <minTitle>EQ_Title_%s_%d</minTitle>' % (pw, rank),
                 '    <faction>EQ_Faction_%s</faction>' % pw,
                 '    <permitPointCost>1</permitPointCost>',
                 '    <cooldownDays>%d</cooldownDays>' % cd,
                 '    <uiPosition>(%d,%d)</uiPosition>' % pos]
            if prereq:
                o.append('    <prerequisite>EQ_Permit_%s_%s</prerequisite>' % (pw, prereq))
            if kind in ('drop', 'shuttle'):
                o.append('    <usableOnWorldMap>true</usableOnWorldMap>')
            o += ['    <royalAid>', '      <favorCost>%d</favorCost>' % favor]
            if kind == 'drop':
                o += ['      <targetingRange>39.9</targetingRange>', '      <itemsToDrop>']
                o += ['        <%s>%d</%s>' % (k, v, k) for k, v in data.items()]
                o.append('      </itemsToDrop>')
            elif kind == 'lab':
                o += ['      <pawnKindDef>%s</pawnKindDef>' % data[0], '      <pawnCount>%d</pawnCount>' % data[1],
                      '      <aidDurationDays>%d</aidDurationDays>' % data[2], '      <targetingRange>44.9</targetingRange>',
                      '      <targetingRequireLOS>false</targetingRequireLOS>']
            elif kind == 'aid':
                o += ['      <pawnKindDef>%s</pawnKindDef>' % data[0], '      <pawnCount>%d</pawnCount>' % data[1],
                      '      <targetingRange>39.9</targetingRange>']
            elif kind == 'strike':
                o += ['      <targetingRange>44.9</targetingRange>', '      <radius>%s</radius>' % data[0],
                      '      <explosionRadiusRange>%s</explosionRadiusRange>' % data[2],
                      '      <explosionCount>%d</explosionCount>' % data[1],
                      '      <intervalTicks>60</intervalTicks>', '      <warmupTicks>120</warmupTicks>']
            else:
                o += ['      <targetingRange>44.9</targetingRange>', '      <targetingRequireLOS>false</targetingRequireLOS>']
            o += ['    </royalAid>', '    <layerBlacklist>', '      <li MayRequire="Ludeon.RimWorld.Odyssey">Orbit</li>',
                  '    </layerBlacklist>', '  </RoyalTitlePermitDef>']
            out += o
    out.append('</Defs>')
    root = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'Mods', 'EcosDaQueda', '1.7', 'Defs')
    with open(os.path.join(root, 'EQ_Permits.xml'), 'w', encoding='utf-8') as f:
        f.write('\n'.join(out) + '\n')
    print('permissões:', sum(len(v) for v in P.values()))


if __name__ == '__main__':
    gen()
