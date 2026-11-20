# Governo pela ideologia: o que cada meme acrescenta (rascunho, 2026-11-19)

_Parte do `NACAO.md` › 3.7 (bloco N2b). Lista completa: **138 memes** no modpack (101 comuns, 37 de estrutura), extraídos dos XML em `memes-bruto.tsv`. Tudo aqui é **proposta para o usuário cortar, trocar e acrescentar**. Os rótulos em português são os do jogo/mod quando existem; os outros são tradução minha._

## Como ler

Cada meme dá até um destes efeitos no **governo da nação** (o sistema do conselho do N2):

| Tipo | O que é |
|---|---|
| **Governo** | Muda quem manda: número de líderes, sucessão, revolta, poder do líder |
| **Cadeira de função** | Uma cadeira a mais no conselho, ocupada automaticamente pelo colono que tem aquela **função da ideologia** (especialista). Só existe se a ideologia tiver a função |
| **Cadeira** | Uma cadeira a mais, escolhida pelo jogador (com a perícia pedida) |
| **Regra** | Muda uma regra da nação (território, revolta, tributo, conselho, Hegemony) |
| **Bônus** | Número a mais ou a menos em algum sistema (F4, frentes, exércitos, boa vontade, tributo) |
| — | Sem efeito de governo (o meme já faz o bastante no jogo) |

Funções da ideologia que existem no jogo para as **cadeiras de função**: especialista de produção, de plantio, de mineração, de animais, de pesquisa, médico, de tiro, de corpo a corpo e guia moral; o VE Memes acrescenta outras (triúnviros, herdeiro etc.).

**Sem limite de cadeiras** (decisão do usuário): o limite é o do próprio jogo (no máximo 4 memes comuns por ideologia, mais a estrutura), e o conselho pode ficar grande.

## 1. Forma de governo (os memes que mais pesam)

| Meme | defName | Mod | Tipo | Proposta |
|---|---|---|---|---|
| **República** | `VME_Republic` | VE Memes | Governo | **O exemplo "democracia" do usuário.** O VE Memes já cria um triunvirato (líder + 2º e 3º triúnviro, como funções da ideologia). Cada triúnviro toma uma pasta: **Relações** (faz o papel do Embaixador, +10%), **Guerra** (do General, +10%) e **Terras** (do Conselheiro, +10%). Sucessão sem revolta (os outros dois seguram o país). Mas toda decisão grande (declarar guerra, jurar lealdade, proclamar) precisa de 2 dos 3 com humor acima de 40% |
| **Real** | `VME_Royal` | VE Memes | Governo | Sucessão por herança: com herdeiro designado (filho/parente), a morte do líder não traz revolta e não exige nova proclamação. Sem herdeiro, revolta maior |
| **Aristocrático** | `VME_Aristocratic` | VE Memes | Governo + cadeira | Cadeira **Nobre** (Social; ocupada por quem tem título real do Royalty): cerimônias das potências −20% de preço; colonos sem título não podem ocupar cadeiras fixas |
| **Deus-imperador** | `VME_GodEmperor` | VE Memes | Governo | O líder vale por um conselho: sem conselho, o líder dá metade de cada bônus fixo. Revolta da proclamação 30% menor ("ninguém se revolta contra um deus, só contra o preço do pão"). Se o líder morrer: revolta grande e todo o conselho cai |
| **Sacerdócio exaltado** | `VME_ExaltedPriesthood` | VE Memes | Governo | Líder e guia moral viram um só: a cadeira do guia moral some, e o líder ganha os bônus dela. Rituais do líder dão legitimidade (N3) |
| **Anciãos** | `VME_Elders` | VE Memes | Governo | Cadeira **Conselho dos anciãos** (colono com 60+ anos): +bônus do Conselheiro; cadeiras fixas exigem 30+ anos |
| **Igualitário** | `VME_Egalitarian` | VE Memes | Governo | Cadeiras fixas não exigem perícia mínima (o bônus ainda cresce com ela); sem pensamento de "preterido" |
| **Nacionalista** | `VME_Nationalist` | VE Memes | Bônus | Revoltas 25% menores; boa vontade com facções de outra ideologia −10%; General +10% |
| **Coletivista** | `Collectivist` | Ideology | Governo | +1 cadeira livre (até 7 no total); cada bônus de cadeira vale 80% ("todo mundo opina um pouco") |
| **Individualista** | `Individualist` | Ideology | Governo | −1 cadeira fixa à escolha; as que ficam valem 130% |
| **Lealista** | `Loyalist` | Ideology | Regra | Sucessão sem revolta e sem nova proclamação se o sucessor já estiver escolhido |
| **Autoritarismo** (estrutura) | `VME_Structure_Authoritarianism` | VE Memes | Governo | Líder pode **destituir** um conselheiro na hora (sem espera); revoltas 20% menores, mas aparecem de novo se o humor médio da colônia passar 20 dias abaixo de 35% |
| **Culto à personalidade** (estrutura) | `VME_Structure_CultOfPersonality` | VE Memes | Governo | Bônus do conselho multiplicados pelo humor do líder (líder feliz = +20%, triste = −30%) |
| **Origem corporativa** (estrutura) | `VME_Structure_Corporate` | VE Memes | Governo + cadeira | A nação é uma empresa: cadeira **Diretor financeiro** (Social + Intelectual): tributo em prata dobra; o Embaixador vira "Relações com investidores" (sátira com a Hélice: desconto na cerimônia dela) |
| **Ideológico** (estrutura) | `Structure_Ideological` | Ideology | Cadeira de função | Cadeira do **guia moral**: conversões e humor (+legitimidade no N3) |

## 2. Cadeiras ligadas a funções (o modelo "Supremacia Humana")

| Meme | defName | Mod | Função ligada | Proposta |
|---|---|---|---|---|
| **Primazia humana** ("Supremacia Humana") | `HumanPrimacy` | Ideology | especialista de **produção** | **Exemplo do usuário.** Cadeira **Ministro da Indústria**: +10% de velocidade de trabalho nas bancadas da colônia; tributo de materiais +25%; aço e componentes no tributo |
| **Transumanista** | `Transhumanist` | Ideology | especialista de **pesquisa** (ou médico) | Cadeira **Ministro da Ciência**: pesquisa +10%; compra de projetos com favor −15% |
| **Industrialismo** | `VME_HardcoreIndustrialism` | VE Memes | especialista de **produção** | Se não tiver a Primazia: a mesma cadeira; se tiver as duas, a cadeira vale 150% |
| **Cultura artesã** | `VME_CraftCulture` | VE Memes | especialista de **produção** | Cadeira **Mestre de ofícios**: qualidade dos itens +1 nível de chance; Forja aprecia (+favor ao fabricar) |
| **Fazendeiro / criador** | `Rancher` | Ideology | especialista de **animais** | Cadeira **Ministro da Pecuária**: comida do tributo +30%; animais de carga para caravanas e exércitos |
| **Primazia da natureza** | `NaturePrimacy` | Ideology | especialista de **plantio** | Cadeira **Guardião da Terra**: tiles de floresta/prado rendem +50%; o território não cresce em zonas da Queda |
| **Conexão com as árvores** | `TreeConnection` | Ideology | especialista de **plantio** | Como acima, com bônus de dríades |
| **Escavador** | `Tunneler` | Ideology | especialista de **mineração** | Cadeira **Ministro das Minas**: tiles de morro e montanha rendem pedra e metal em dobro |
| **Supremacia mecanoide** | `VME_MechanoidSupremacy` | VE Memes | mecanitor (colono com mecanitor) | Cadeira **Comandante de mechs**: exércitos e frentes do jogador ganham mechs; Forja aprecia, Silêncio desconfia |
| **Supremacia insetoide** | `VME_InsectoidSupremacy` | VE Memes | (cadeira livre, Animais) | Cadeira **Rainha-mãe honorária**: a Colmeia Erguida fica amistosa; revoltas recebem "visita" de insetos do seu lado |
| **Supremacia de Gauranlen** | `AM_GauranlenSupremacy` | Alpha Memes | especialista de **plantio** | Como Primazia da natureza |
| **Atirador de elite** | `AM_Sharpshooter` | Alpha Memes | especialista de **tiro** | Cadeira **Mestre de armas**: General +15% se for atirador |
| **Gladiador** | `AM_Gladiator` | Alpha Memes | especialista de **corpo a corpo** | Cadeira **Campeão**: o campeão pode resolver uma revolta em **duelo** com o síndico (vence = revolta acaba) |
| **Bushido** | `VME_Bushido` | VE Memes | especialista de **corpo a corpo** | Como Gladiador; honra: rendição dá Fama |
| **Ritualista** | `Ritualist` | Anomaly | (guia moral) | Cadeira **Mestre de cerimônias**: cerimônias das potências dão favor extra |
| **Proselitista** | `Proselytizer` | Ideology | guia moral | Cadeira **Missionário**: conversão de vizinhos — facções menores perto viram amistosas mais rápido; revoltas 15% menores |
| **Comerciante** | `VME_Trader` | VE Memes | (cadeira livre, Social) | Cadeira **Ministro do Comércio**: preços com comerciantes +5%; tributo em prata +50% |
| **Saúde pública** | `VME_Healthcare` | VE Memes | **médico** | Cadeira **Ministro da Saúde**: exércitos com menos baixas; doenças não matam o conselho (sátira: atende primeiro o conselho) |
| **Saqueador** | `Raider` | Ideology | especialista de **tiro** ou corpo a corpo | Cadeira **Capitão de saques**: frentes de trincheira rendem saque; boa vontade das vítimas cai mais |
| **Pirata** | `VFEP_PirateMeme` | VFE Pirates | (cadeira livre, Tiro) | Como Saqueador, e o Sindicato do Saque trata a nação como "colega" |
| **Corsário** (estrutura) | `AM_Structure_Corsair` | Alpha Memes | (cadeira livre) | Como Pirata |

## 3. Regras e bônus no Hegemony (território, estradas, guerra)

| Meme | defName | Mod | Proposta |
|---|---|---|---|
| **Construtores de cidades** | `VME_CityBuilders` | VE Memes | Fronteira da colônia cresce 30% mais rápido; −1 marco de fronteira (constroem, não andam) |
| **Nômade** | `VME_Nomad` | VE Memes | Sem território fixo: a nação "mora" na caravana; marcos baratos (metade da prata), mas somem depois de 30 dias |
| **Isolacionista** | `VFEA_Isolationist` | VE Memes | Fronteira não cresce além do teto natural, mas as invasões do Hegemony contra a nação são mais raras; Embaixador −50% |
| **Viajantes** | `VVE_Travelers` | VV Expanded | Tiles com estrada rendem prosperidade dobrada; marcos podem ficar 3 tiles longe da borda (em vez de 1,5) |
| **Fúria da estrada** | `VVE_Roadragers` | VV Expanded | Frentes de trincheira com veículos; Sindicato do Saque respeita |
| **Náuticos** | `VVE_Nauticals` | VV Expanded | Território atravessa água (navegação do Hegemony); costas rendem comida |
| **Buscadores do céu** | `VVE_Skyseekers` | VV Expanded | Marcos podem ser plantados longe da borda (até 6 tiles, por aeronave) |
| **Nascidos na nave** | `Shipborn` | Odyssey | Marcos em qualquer tile alcançável pela nave gravitacional |
| **Desmatamento** | `AM_Deforestation` | Alpha Memes | Tiles de floresta rendem madeira ×2 e prosperidade −50% depois de 30 dias (a floresta acaba) |
| **Primazia da água** | `AM_WaterPrimacy` | Alpha Memes | Tiles com rio rendem ×2; Sindicato do Saque (Primazia da Água) vira rival direto |
| **Ferro-velho** | `VME_Scrapper` | VE Memes | Tributo inclui sucata/componentes; ruínas do Velho Mundo no território rendem |
| **Preferência por pré-fabricados** | `AM_PrefabPreference` | Alpha Memes | Marcos de fronteira custam aço em vez de prata e ficam prontos em 1 dia |
| **Pacifista** | `VME_Pacifist` | VE Memes | Revolta da proclamação vira **negociação** (Social do líder contra o síndico; perder ainda traz a raid, menor); sem General |
| **Voto de não violência** | `AM_NonViolence` | Alpha Memes | Como Pacifista |
| **Baluarte** | `AM_Bulwark` | Alpha Memes | A fronteira do jogador perde tiles 50% mais devagar no F5; General defende melhor |
| **Conversão violenta** | `VME_ViolentConversion` | VE Memes | Vencer a revolta converte 1–3 revoltosos caídos à ideologia (sátira: "voluntários") |
| **Supremacista** | `Supremacist` | Ideology | Revolta maior (+20%), mas a vitória dá Fama e tiles iniciais +1 ("a região aprendeu") |
| **Canibal** | `Cannibal` | Ideology | Revoltosos mortos viram tributo de comida; boa vontade de todos −5 na fundação |
| **Tecnofobia** | `VQE_Technophobia` | VQE Generator | Sem cadeiras ligadas a pesquisa; Forja e Hélice com boa vontade −10 |
| **Progressista** | `VME_Progressive` | VE Memes | Pesquisa +5% por cadeira ocupada; Coro se ofende |
| **Emancipação** | `VME_Emancipation` | VE Memes | Escravos não podem ser conselheiros, mas libertados ganham cadeira; revoltas menores |
| **Corte de sangue** | `VME_BloodCourt` | VE Memes | Execução de um conselheiro (punição) dá legitimidade em vez de tirar |
| **Alimentação de sangue** | `Bloodfeeding` | Biotech | Cadeira para sanguífago com bônus de General à noite |
| **Inumano** | `Inhuman` | Anomaly | Revoltosos fogem antes (−30% de moral); boa vontade de todos −10 |
| **Escuridão** | `Darkness` | Ideology | Frentes e revolta à noite favorecem a nação |
| **Culpado** | `Guilty` | Ideology | Derrota numa revolta dá legitimidade (expiação) em vez de tirar |
| **Concentração psíquica** | `VME_PsychicFocus` | VE Memes | Coro aprecia; cadeira psíquica (colono com psicotalentos) dá bônus de Embaixador com o Coro |
| **Memes do Ecos** (as 4 assinaturas) | `EQ_Meme_GeneticMarket`, `EQ_Meme_MachineWarranty`, `EQ_Meme_PsychicChoir`, `EQ_Meme_VoidScholarship` | Ecos da Queda | **Sem efeito, e o jogador não pode escolhê-los** (decisão do usuário): exclusivos das superpotências, por `factionWhitelist` em `Defs/Memes/EQ_Memes.xml` (feito em 2026-11-19; XML validado, sem teste em jogo ainda) |

## 4. Estruturas (teístas e de origem): uma regra pequena cada

A estrutura é obrigatória em toda ideologia, então o efeito deve ser **leve**, e o bloco pode deixar as de origem iguais por grupo.

| Grupo | Memes | Proposta |
|---|---|---|
| **Teístas** | `Structure_TheistEmbodied`, `Structure_TheistAbstract`, `VME_Structure_Pantheism`, `VME_Structure_Omnism`, `AM_Structure_Kemetism`, `VME_Structure_Shintaoism`, `VFEM2_Structure_Norse`, `AM_Structure_Mesoamerican` | Cadeira **Sumo sacerdote** (guia moral) disponível; rituais dão legitimidade |
| **Origem religiosa real** | `Structure_OriginChristian`, `Structure_OriginIslamic`, `Structure_OriginBuddhist`, `Structure_OriginHindu`, `AM_Structure_Jewish`, `AM_Structure_Sikhism`, `AM_Structure_Jainism` | Igual aos teístas (sem diferença entre elas, para não ter efeito por religião real) |
| **Seculares** | `Structure_Ideological`, `AM_Structure_Atheist`, `VME_Structure_Agnosticism`, `VME_Structure_SecularSpirituality` | Sem sacerdote; +1 cadeira livre |
| **Animista** | `Structure_Animist` | Cadeira de plantio disponível |
| **Arquista** | `Structure_Archist` | Cadeira **Ministro da Ciência** disponível sem o Transumanista; Silêncio aprecia |
| **Ocultas / sobrenaturais** | `VME_Structure_ChthonianCult`, `AM_Structure_Horaxian`, `VME_Structure_Eschatologism`, `VME_Structure_Esotericism`, `AM_Structure_FleshCult`, `VME_Structure_Serketist` | Cadeira **Oráculo** (psicossensível): aviso de invasões e revoltas 1 dia antes; Silêncio vigia a nação (Exposição?) |
| **Hedonismo** | `VME_Structure_Bacchanalianism` | Conselho com humor alto dá +10% em tudo, com humor baixo −20% |
| **Outras** | `AM_Structure_Alienism`, `AM_Structure_Cubicism`, `AM_Structure_Neolithic`, `AM_Structure_Scavenger`, `AM_Structure_SteampunkRevival` | Neolítico: revolta menor (vizinhos com lanças) e sem cadeira de pesquisa; Catador: tributo com sucata; Steampunk: +produção; Alienismo e Cubismo: — |

## 5. Sem efeito de governo (proposta: deixar como estão)

Memes de costume, comida, corpo ou estética. Já mudam o jogo pela ideologia e não precisam de regra de governo; dá para dar sabor de texto no conselho depois.

`AnimalPersonhood`, `Blindsight`, `FemaleSupremacy`*, `MaleSupremacy`*, `FleshPurity`, `HighLife`, `Nudism`, `PainIsVirtue`, `AM_Artist`, `AM_BiologicalCorruptors`, `AM_BiologicalDefilers`, `AM_BiologicalReconstructors`, `AM_CoffeePrimacy`, `AM_TeaPrimacy`, `AM_Cowboys`, `AM_DeepDevotion`, `AM_Epicurean`, `AM_Fertility`, `AM_Iconoclast`, `AM_Madness`, `AM_Monastic`, `AM_NakedTruth`, `AM_PainIsDivine`, `AM_PsychicVampirism`, `AM_Sadist`, `AM_VampireHunting`, `AM_Youth`, `VME_Angler`, `VME_Anonymity`, `VME_Astrology`, `VME_Cultured`, `VME_FireWorship`, `VME_Fleshcrafters`, `VME_Gestalt`, `VME_HolyDiseases`, `VME_Light`, `VME_PartyLife`, `VME_SweetTeeth`, `VME_Vegan`.

\* **Supremacias de gênero:** proposta mínima: cadeiras do conselho só para o gênero supremo (é o que o meme já faz com os líderes).

## 6. Decisões do usuário (2026-11-19)

> **N2b e N2c feitos (2026-11-20):** os 4 grupos em `Defs/World/EQ_Governo.xml` (93 memes); resultado e simplificações em `NACAO.md` › Resultado do N2b e do N2c. Falta só o grupo 5 (supremacias de gênero, se o usuário quiser).


1. **Sem limite de cadeiras** vindas de memes; o limite natural é o dos 4 memes da ideologia. Conselho grande não é problema.
2. **República:** como proposto (pastas Relações, Guerra e Terras; decisões grandes com 2 dos 3).
3. **Primazia humana:** como proposto (Ministro da Indústria: +10% de velocidade nas bancadas e +25% no tributo de materiais).
4. **Primeiro corte do N2b:** grupos 1 (governo) e 2 (cadeiras de função); grupos 3 (regras) e 4 (estruturas) num bloco seguinte.
5. **Memes do Ecos:** não entram no governo e são **exclusivos das superpotências** (o jogador não os escolhe).

As propostas das seções 1–5 que não foram discutidas uma a uma valem como ponto de partida; o usuário corta ou troca na hora de implementar.
