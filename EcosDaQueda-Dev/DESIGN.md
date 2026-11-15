# Design — Ecos da Queda

## Visão

RimWorld pós-apocalíptico **inspirado em Fallout**, mas **sem trazer a história de Fallout**: tudo é contado com a própria lore do RimWorld (arcotecs, mecanoides, o Império, o Vazio, glitterworlds, abrigos antigos).

- **Tom:** satírico (humor ácido estilo Fallout).
- **Moral:** todas as facções são **moralmente cinzas**, cada uma com uma **parte horrível**.
- **Idioma:** textos dos Defs escritos direto em **português (PT-BR)**; o usuário joga em PT-BR.
- O mod deve **usar mecânicas dos mods instalados** (VE, VPE, VRE, Alpha Genes, CE, Hegemony…), não reinventá-las.

## Premissa: "a Queda"

Há gerações, uma guerra entre um Império e um enxame mecanoide queimou o planeta. Sobreviventes se esconderam em abrigos antigos (Vanilla Quests Expanded – Ancients). Hoje o mundo é um deserto poluído (poluição do Biotech, Wasters, Waste Animals) disputado por quatro potências.

O jogador pode:
1. **Entrar** numa das 4 facções por uma cerimônia de submissão e subir de cargo com o líder do assentamento;
2. **Fundar a própria facção** e disputar território (Hegemony);
3. **Ficar independente**, jogando umas contra as outras.

## As 4 facções principais

| | Ordem do Silêncio Perpétuo | Concílio da Hélice | Irmandade da Forja | O Coro Ascendente |
|---|---|---|---|---|
| DLC foco | Anomaly | Biotech (genética) | Biotech (robótica/mecanitores) | Royalty (psíquico) |
| defName | `EQ_Faction_Silencio` | `EQ_Faction_Helice` | `EQ_Faction_Forja` | `EQ_Faction_Coro` |
| Inspiração | Children of Atom + sociedade secreta acadêmica | super mutantes como projeto corporativo | Brotherhood of Steel liderada por mecanitores | Legião / Enclave psíquico aristocrático |
| Ideologia | *A Escuta Profunda* | *O Genoma Ideal* | *O Culto da Garantia* | *A Harmonia Superior* |
| Memes | AM_Structure_Horaxian + Ritualist + **EQ_Meme_VoidScholarship** | VME_Structure_Corporate + VME_Fleshcrafters + **EQ_Meme_GeneticMarket** | Structure_Archist + VME_HardcoreIndustrialism + **EQ_Meme_MachineWarranty** | VME_Structure_CultOfPersonality + VME_Aristocratic + **EQ_Meme_PsychicChoir** |
| Sátira | burocracia acadêmica: "publique ou seja devorado", bolsa paga em prisioneiros | genes como produto de assinatura, marketing, "versão base" | manuais como escritura, firmware como sacramento, "recall" | etiqueta, cerimônias e impostos intermináveis |
| Parte horrível | prisioneiros viram "voluntários de pesquisa" desumanizados e alimento de entidades | humanos não modificados são "descontinuados" | androides livres são desmontados; pessoas "defeituosas" entram em recall | dízimo psíquico: crianças sensíveis levadas para "treinamento vocal" |
| Líder | arquivista-mor | diretor(a) executivo(a) | engenheiro(a)-primaz | maestro supremo / maestrina suprema |
| Unidades | voluntário de pesquisa, acólito pesquisador, guardião da contenção, arquivista-mor | estagiário genético, fiscal de linhagem, protótipo premium, diretor executivo | aprendiz de manutenção, cavaleiro da garantia, técnico autorizado (mecanitor), engenheiro-primaz + mechs | corista, tenor de guerra, soprano incandescente, maestro supremo |
| Equipamento | robes, nerve spiker, armas de bioferrite (VWE Bioferrite), incinerador | armas industriais avançadas, beam graser, granadas tóxicas; xenotipos de elite | blindagem espacial ("power armor"), Laser (VWE) e Coilgun (VWE), mechs | psycasts do VPE (Warlord, Protector, Conflagrator, Staticlord, Archon, Empath), cajado de eltex |
| Hegemony | isolada, pesquisa, arrasa em vez de ocupar | comercial, compra vassalos, abre filiais | militarista, expande e saqueia tecnologia | imperialista, quer vassalos e tributo |

## Ideologias fixas (roleplay)

Decisão do usuário: o mod tem **uma história e um padrão**, não variação a cada partida. Cada facção tem sempre a mesma ideologia: nome, ícone, cor, divindades, regras (preceitos), cargos, rituais e líder fundador. Definido em `Mods/EcosDaQueda/1.7/Defs/EQ_IdeoTemplates.xml` e aplicado pelo C# (`WorldComponent_IdeoTemplates`).

| | Silêncio | Hélice | Forja | Coro |
|---|---|---|---|---|
| Ideologia | A Escuta Profunda | O Genoma Ideal | O Culto da Garantia | A Harmonia Superior |
| Membro / adjetivo | silente / silente | associado / heliciano | irmão de ferro / forjado | cantor / coral |
| Ícone (próprio) / cor | olho fechado / lavanda-do-vazio | dupla hélice / verde-laboratório | engrenagem / laranja-ferrugem | diapasão / púrpura-imperial |
| Memes | void-touched (AM), Ritualist, Darkness, Academia do Vazio | Corporativo, Fleshcrafting, Trader, Mercado Genético | Arquista, Industrialismo, Supremacia Mecanoide, Garantia Sagrada | Culto de Personalidade, Aristocrático, Foco Psíquico, Harmonia Superior |
| Divindades | O Revisor; A Nota de Rodapé | O Acionista Majoritário | O Fabricante Original; A Assistência Técnica | A Primeira Voz; O Eco Imperial |
| Cargos | Arquivista-Mor, Revisor Supremo | Diretor Executivo, Gerente de Compliance, Executivo de Vendas, Geneticista-Chefe | Engenheiro-Primaz, Inspetor de Garantia, Mestre Mecânico | Maestro Supremo, Mestre de Cerimônias |
| Rituais | Arquivamento Final (funeral), Revisão por Pares (conversão), Palestra Magna | Encerramento de Contrato, Onboarding Compulsório, Reunião de Acionistas, Feira de Lançamento | Descomissionamento, Atualização de Firmware, Leitura do Manual | Réquiem, Afinação, Concerto Solene |
| Regras principais | estudo do vazio muito eficiente, rituais psíquicos exaltados, pesquisa muito rápida, **proibido socializar (voto de silêncio)**, luz negra, combate no escuro, biblioteca obrigatória, escrever livros exaltado | xenotipos preferidos Highmate/Genie/Hussar, só modificação biológica, tanques de crescimento essenciais, comércio obrigatório e preços melhores, expectativas altas | mecanoides exaltados, trabalho mecanoide melhorado, torres automáticas exaltadas, bioimplantes aprovados, qualidade de fabricação, automação, veículos, drogas só medicinais | líder = psíquico mais forte, psyfocus dobrado, sensibilidade psíquica, meditação, realeza exaltada, títulos hereditários, permissões mais baratas, expectativas altas |
| Parte horrível (regras) | escravidão aceitável, execução indiferente, uso de órgãos aceitável, intolerância religiosa | escravidão e órgãos aceitáveis ("versão base") | androides são ferramentas, intolerância religiosa | trabalho infantil incentivado, escravidão aceitável, execução respeitada |
| Líder fundador | Ottavio/Ottavia "Rodapé" Kessling | Valério/Valéria "Premium" Sandoval | Bartolomeu/Bartolomeia "Chave-Inglesa" Ferraz | Serafim/Serafina "Fá Imperial" Dellacroce |

## Relações desejadas (ainda NÃO implementadas — fase 2)

| | Silêncio | Hélice | Forja | Coro |
|---|---|---|---|---|
| **Silêncio** | — | desconfiança | guerra | ódio mortal |
| **Hélice** | desconfiança | — | rivalidade | aliança frágil |
| **Forja** | guerra | rivalidade | — | neutra |
| **Coro** | ódio mortal | aliança frágil | neutra | — |

Agradar uma facção sempre custa a confiança de outra.

## Reputação (implementada)

Fama e Infâmia separadas com cada potência (estilo New Vegas). Título = nível de Fama × nível de Infâmia (limiares 30 e 150):

| | Pouca Infâmia | Média | Muita |
|---|---|---|---|
| **Pouca Fama** | Desconhecido (—) | Malvisto (preços −5%; se hostil, caçadores 5%) | Inimigo Jurado (preços −15%; se hostil, caçadores 20%) |
| **Média** | Aceito (preços +5%; presentes 5%) | Imprevisível (se hostil, caçadores 5%) | Temido (preços +10%; tributo 15%; se hostil, caçadores 5%) |
| **Muita** | Idolatrado (preços +15%; presentes 20%) | Herói Controverso (preços +10%; presentes 10%) | Lenda Sombria (preços +15%; tributo 20%) |

Chances por verificação (a cada 3 dias), no máximo 1 evento por facção a cada 8 dias. Presentes e tributos só de facções não hostis; caçadores de recompensa só de hostis.

## Submissão, líder e cargos (implementado)

Fama e Infâmia são do **assentamento**. Cargo e favor são do **líder** (o líder da ideologia do jogador).

**Entrar:** aba Reputação › "Cerimônia" da potência. Exige líder na colônia, o preço disponível e a potência não hostil. Vira uma missão; aceita, um emissário chega de nave com escolta, o jogador clica nele para iniciar o ritual com o líder e os espectadores, e ao fim o emissário recolhe o preço (se faltar, a cerimônia é recusada). O líder recebe o 1º cargo, e o Hegemony registra a vassalagem (tributo do Hegemony incluso). Uma potência por vez. Sair = independência pelo Hegemony (traição) + 50 de Infâmia; o cargo é cassado.

| | Silêncio | Hélice | Forja | Coro |
|---|---|---|---|---|
| Submissão | Banca de Admissão | Assinatura do Contrato de Adesão | Rito de Ativação da Garantia | Audição Solene |
| Preço | 1 prisioneiro ("voluntário inaugural") + 300 prata | 800 prata + sangue do líder (perda de sangue) | 12 componentes + 150 aço | 40 ouro + 400 prata |
| Promoção | Defesa de Tese | Avaliação de Desempenho | Inspeção de Certificação | Recital de Ascensão |
| Emissário | arguidor silente | representante comercial | inspetor de garantia | mestre de cerimônias |
| Favor | citações | ações preferenciais | selos de garantia | aplausos |
| Como ganhar | +1 por pesquisa concluída ("publicação"); contrato de contenção +3; bolsa: prisioneiro +3, prata +1 | +1 a cada 500 de prata negociados com o Concílio; Soro +2; xenogerme implantado +1; mensalidade +2, prisioneiro +3 | +1 a cada 3 itens excelentes fabricados; obra-prima +2; dízimo: componentes +3, prata +1 | +1 por ritual concluído ("concerto"); dízimo: colono +8, ouro +2 |
| Cargos (1→7) | Voluntário de Pesquisa, Assistente de Campo, Pesquisador Associado, Doutor em Vazio, Catedrático do Vazio, Arquivista Sênior, Parecerista Anônimo | Cliente Básico, Assinante Premium, Franqueado, Gerente Regional, Acionista, Diretor de Linhagem, Membro do Conselho | Aprendiz de Manutenção, Técnico Licenciado, Inspetor de Campo, Cavaleiro da Garantia, Paladino do Recall, Engenheiro-Mestre, Guardião do Manual | Ouvinte, Corista, Solista, Spalla, Regente, Mestre de Capela, Maestro Honorário |

Favor para cada cargo: 4 (dado pela cerimônia de submissão), 6, 8, 10, 14, 18, 24. Missões da potência podem pagar em favor, e cerimônias bem assistidas dão favor extra. Todo favor ganho por qualquer colono vai para o líder.

Por cargo: preços com a potência +0/3/5/8/10/13/15%; estipêndio a cada 10 dias (cresce com o cargo); dízimo com quantidades ×1,1,2,2,3,3,4. Serviços exigem o 1º, 3º ou 5º cargo.

**Líder:** sem líder numa potência, o jogo obriga a escolher (janela sem fechar) entre adultos livres que seguem a ideologia e cumprem as exigências do cargo de líder dela. **Sucessor:** botão na aba; se o líder morre ou é perdido, o sucessor assume o cargo de líder da ideologia e herda cargo e favor; sem sucessor, escolha forçada. **Transferência:** trocar o líder da ideologia (ritual vanilla de troca de função) passa cargo e favor ao novo líder; uma cerimônia marcada para o antigo é cancelada e remarcada.

| | Estipêndio (1º → 7º cargo) | Dízimo |
|---|---|---|
| Silêncio | bioferrite 15→80, remédio, fragmentos | **Bolsa de Pesquisa** (15 dias): prisioneiro, 150 prata ou recusar |
| Hélice | remédio, hemogen, remédio glitterworld, neutroamina | **Mensalidade Genética** (15 dias): 200 prata, prisioneiro "versão base" ou cancelar |
| Forja | aço, componentes, plastaço, componentes avançados | **Dízimo Tecnológico** (15 dias): 3 componentes, 250 prata ou recusar |
| Coro | prata, ouro, jade | **Dízimo Psíquico** (30 dias): um colono (de preferência criança sensível), 15 ouro ou recusar. Nunca leva o último colono |

## Mecânicas por facção (implementado)

**Serviços para membros** (aba Reputação › botão "Serviços"; custo em prata, cargo mínimo e recarga; "posto 1/2/3" abaixo = 1º/3º/5º cargo):

| Potência | Serviço 1 | Serviço 2 |
|---|---|---|
| Silêncio | **Contrato de contenção** (posto 1; entrega uma entidade presa em plataforma → 1 fragmento, 40 bioferrite, 300 prata, +20 Fama; recarga 5 d) | **Consulta ao Arquivo** (posto 2; 400 prata → +1500 na pesquisa atual; 12 d) |
| Hélice | **Encomendar o Soro** (posto 2; 900 prata → 1 Soro; 15 d) | **Check-up premium** (posto 3; 1000 prata → soro mecanita curador; 30 d) |
| Forja | **Acionar a garantia** (posto 2; 300 prata → reforço militar com mechs; 12 d) | **Kit de peças originais** (posto 3; 800 prata → 3 componentes avançados + 60 plastaço; 20 d) |
| Coro | **Serenata** (posto 1; 250 prata → +10 de humor para todos por 6 dias; 15 d) | **Coro de guerra** (posto 3; 500 prata → reforço militar com psíquicos; 15 d) |

**O Soro** (`EQ_Soro`): item de uso único; reescreve o xenotipo para um de elite aleatório (Hussar, Genie, Highmate, Drakonori, Efreet, Helixien, Saurid, Lycan, Uhlan). 25% de chance de "versão alternativa" (Pigskin, Waster, Dirtmole, Neanderthal, Wretch, Fungoid).

**Retaliação-assinatura** (potência hostil + título com chance de caçadores; 50% das vezes vem isto em vez da invasão):
- Silêncio — "Experimento de campo": enxame de cambaleantes, bestas de carne, ladrões de visão ou ghouls.
- Hélice — "Amostra grátis": praga, gripe ou precipitação tóxica.
- Forja — "Recall compulsório": aglomerado mecanoide, nave desfolhadora ou curto-circuito.
- Coro — "Nota dissonante": zumbido psíquico ou nave emanadora psíquica.

## Sistemas planejados

### Fase 2 — C#
- **Relações fixas** entre as 4 facções (matriz acima), respeitando a diplomacia do Hegemony.
- **Reputação estilo New Vegas:** cada facção guarda **Fama** e **Infâmia** separadas (pode-se ser admirado e temido ao mesmo tempo); abre diálogos/missões diferentes.
- **Postos dentro da facção** ao jurar lealdade (recruta → oficial → comandante…), com benefícios e obrigações.
- **Mecânicas próprias por facção:**
  - Silêncio: contratos de contenção (capturar/estudar entidades → conhecimento, bioferrite, rituais); **corrupção** que fortalece rituais mas gera eventos de insanidade (VAE Insanity); território "sangra" eventos sombrios.
  - Hélice: comércio de genes; item **"o Soro"** (estilo FEV) que transforma um colono num xenotipo de elite aleatório com risco de mutação ruim; censo genético para membros.
  - Forja: **dízimo tecnológico** (entregar tecnologia em troca de mechs/veículos); conflito com androides livres (VRE Android); licenças de mecanitor e veículos (Vanilla Vehicles Expanded); cotas.
  - Coro: títulos do Royalty como cargos políticos ligados à vassalagem do Hegemony; **dízimo psíquico** (enviar colonos sensíveis); ritual de conversão à distância.

### Ideias extras
- **Campeões nêmesis** via Rimesis para cada facção.
- **Facções menores** como peões: VFE Pirates (Warcaskets = "power armor"), Deserters, Insectoids, Wasters.
- **Bases únicas** via Custom Base Framework.
- Toda arma/armadura nova precisa de **patch para Combat Extended**.
