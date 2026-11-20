# Nação do jogador: fundação, revolta e conselho (desenho, 2026-11-19)

_Ideia do usuário de 2026-11-19, organizada. Junta-se ao mundo fixo e às fronteiras vivas (`MUNDO.md`, F4) e à submissão (`DESIGN.md` › Submissão). Propostas minhas marcadas com **(sugestão)**; decisões em aberto no fim (✱)._

## 1. A ideia (palavras do usuário, organizadas)

- **No começo, o jogador é só um assentamento comum.** Não tem as mecânicas do Hegemony: sem território, sem fronteira, sem ser uma "nação".
- **Fundar a nação:** construir uma **sala do trono** com um **trono**. O trono ganha a opção de **escolher o líder da facção**. Escolhido o líder, os **povos dos mapas vizinhos não aceitam** e vem uma **revolta**: uma raid de tamanho definido para impedir que o jogador mande na região. **Vencer a revolta funda a facção do jogador**, e o assentamento entra no Hegemony (território, fronteira, tudo do F4).
- **Conselho:** depois de fundada a nação, o jogador monta um conselho com colonos: **Embaixador, General e Conselheiro**, que dão bônus em atributos e situações do Hegemony.
- **Cadeiras pela ideologia** (2ª rodada): além das 3 fixas, as outras cadeiras do conselho dependem da ideologia que o jogador montar.
- **Ideologia trancada até a fundação** (2ª rodada): o jogo sempre começa no **modo clássico** de ideologia. Ao fundar a nação, o jogador escolhe os **primeiros memes** e a ideologia vira **fluida** (reformas com pontos de desenvolvimento). A partir daí, **cada meme muda o governo da nação**. Exemplos do usuário: **Democracia** → 3 líderes, cada um com uma função na facção; **Supremacia Humana** → uma cadeira no conselho ligada à função de **especialista de produção** da ideologia, com bônus de produção. O que cada meme acrescenta fica para planejar (3.7).
- **Lealdade só para nações:** jurar lealdade a uma potência (a cerimônia de submissão) **exige ter fundado a nação antes**.

## 2. O que já existe para reaproveitar

| Peça | Onde | Serve para |
|---|---|---|
| Sala do trono e trono | Royalty: `RoomRoleDef ThroneRoom` (`RoomRoleWorker_ThroneRoom`), tronos vanilla e de mods | Requisito da fundação; o botão fica no trono |
| Líder | Nosso: o líder é o **líder da ideologia do jogador** (cargo e favor são dele; escolha forçada, sucessor, transferência: `Allegiance.cs`) | O líder escolhido no trono vira o líder da ideologia; o resto do sistema já funciona |
| Território do jogador | F4: postfix em `TerritorySolver.Sources` (colônia e marcos) | Antes da fundação, a colônia simplesmente não vira fonte de território |
| Submissão | `Allegiance.cs` (cerimônia) + `BlockPostfix` no `FactionFealty.Block` do Hegemony | A trava "só nação fundada" entra nos dois |
| Vetos do Hegemony | `HegemonyApi.RegisterVeto`: `AIWar`, `AIPeaceOrTreaty`, `InvasionTarget`, `SettlementSite` | `InvasionTarget`: antes da fundação, nenhuma invasão de conquista do Hegemony mira o jogador (sugestão) |
| Raid com líder | Vanilla: `IncidentParms`, `RaidStrategyDef`, grupo com líder; nossas facções ocultas (Fungobi) | A revolta |
| Frentes, exércitos, tensão | `Fronts.cs`, `Forces.cs`, `WarDiplomacy.cs` | Onde o General age |
| Rendimento e crescimento | `PlayerFrontier.cs`, `TerritoryYield.cs` | Onde o Conselheiro age |
| Boa vontade, Fama/Infâmia, favor | vanilla + `Reputation`, `EQFavor` | Onde o Embaixador age |

## 3. Proposta

### 3.1 Antes da fundação: "assentamento comum"

- A colônia **não tem território** no Hegemony (o postfix do F4 tira a fonte dela). Sem marco de fronteira, sem tributo, sem carta de cidade nova.
- **Sem submissão:** a cerimônia na aba Reputação fica desabilitada com o motivo ("A potência não aceita a vassalagem de um sítio; volte quando tiver um trono e um país em volta dele"). O juramento direto pelo Hegemony já era bloqueado; passa a explicar o mesmo.
- **(sugestão)** Veto `InvasionTarget`: o Hegemony não manda invasões de conquista contra quem ainda não é nação. As raids comuns continuam (narrador).
- **Ideologia no modo clássico:** sem memes, preceitos nem funções; a escolha de ideologia do começo de jogo é pulada (ou travada no clássico) nos cenários do Ecos.
- Missões, comércio, Fama/Infâmia, eventos: iguais. (O cargo e o favor do sistema de submissão dependem do líder da ideologia; como a submissão só existe depois da fundação, o modo clássico antes dela não atrapalha.)

### 3.2 A proclamação

- **Requisitos:** uma sala do trono válida (papel `ThroneRoom` do jogo) com um trono; **(sugestão)** pelo menos 3 adultos livres na colônia. Fora disso, o botão aparece desabilitado com o motivo.
- **Botão no trono: "Proclamar líder".** Escolhe um adulto livre da colônia que possa ser líder da ideologia (mesmas exigências da escolha de líder de hoje). Ele vira o líder da ideologia na hora (o cargo, o favor e o sucessor seguem as regras atuais).
- **Aviso** (carta): "A notícia chegou aos vizinhos antes do arauto." Os povos da região se reúnem: a revolta chega em **1 a 2 dias**.
- **(sugestão)** Na carta, três respostas:
  - **Esperar a revolta** (padrão).
  - **Comprar parte dos vizinhos:** prata (proporcional ao tamanho da revolta) tira 30–40% dos revoltosos. Satírico: "a vontade popular tem tabela de preços".
  - **Desistir:** o líder continua líder da ideologia, mas a proclamação é cancelada (pode tentar de novo depois).

### 3.3 A revolta

- **Quem vem:** uma facção oculta nova, **"Povos da Região"** (sugestão de nome satírico: "Associação de Moradores da Região"), sempre hostil, com gente de tipos variados (camponeses, andarilhos, guerreiros tribais, alguns veteranos), liderada por um "síndico". **(sugestão)** Se houver facções menores com território perto, uma parte dos revoltosos vem com as roupas e armas delas (a revolta tem a cara da região).
- **Tamanho:** pontos de raid do momento × 1,3 (✱), com mínimo e máximo; o jogador sabe o tamanho na carta ("cerca de N pessoas com tochas, foices e um abaixo-assinado").
- **Vitória:** todos os revoltosos mortos, caídos ou fugidos → **a nação é fundada**:
  - janela para dar **nome** à nação (renomear a facção do jogador; opcional);
  - **a fé da nação:** o editor de ideologia abre para escolher os primeiros memes (regras da ideologia fluida: começa com poucos memes e ganha mais nas reformas); o jogo sai do modo clássico e a ideologia vira fluida; o líder proclamado recebe a função de líder;
  - a colônia entra no Hegemony: território com raio 2 e todo o F4 (marcos, tributo, colônias novas);
  - a cerimônia de submissão e o conselho ficam liberados;
  - carta às potências (ver 3.6).
- **Derrota (✱):** o líder proclamado morre ou é capturado, ou os revoltosos ficam 6 horas na sala do trono → a proclamação falha; nova tentativa só depois de 15 dias, e a próxima revolta vem 20% maior ("agora eles têm experiência").

### 3.4 Depois: o conselho

Botão no trono (e resumo na aba Reputação): **"Conselho"**. Três cadeiras fixas (abaixo) e **cadeiras e regras de governo vindas dos memes** (3.7), cada uma com um adulto livre da colônia; o líder não pode ocupar cadeira; o mesmo colono só em uma. O bônus cresce com a perícia do conselheiro (0 na perícia 4, cheio na 15+, por exemplo) e só vale com ele vivo, na colônia (ou numa caravana, para o Embaixador) e consciente.

| Cadeira | Perícia | Bônus (proposta) |
|---|---|---|
| **Embaixador** | Social | +20% de boa vontade ganha com todas as facções; tensão entre potências (`WarDiplomacy`) contra a patrona do jogador sobe mais devagar; −15% no preço das cerimônias e dízimos; NPCs aceitam paz com o jogador mais fácil |
| **General** | Tiro ou Corpo a corpo (a maior) | +15% de força do lado do jogador nas frentes de trincheira e nos exércitos que ele comanda (cargos altos); aviso 1 dia antes das invasões do Hegemony; revoltas futuras 20% menores; no F5, a fronteira do jogador cai mais devagar |
| **Conselheiro** | Intelectual | Fronteira cresce 25% mais rápido; +25% no tributo do território; +1 marco de fronteira por colônia; colônia sugerida com mais opções |

- **(sugestão)** Pensamentos: o conselheiro ganha "+4 Membro do conselho"; quem tinha perícia maior e ficou de fora, "−3 Preterido".
- **(sugestão)** Cadeira vazia não dá penalidade; trocar alguém de cadeira tem espera de 3 dias (evita trocar o General só para uma batalha).
- **(sugestão, 4ª cadeira) Intendente** (Social + Intelectual, média): preços com comerciantes, estipêndio maior, manutenção de exércitos mais barata. Só se o usuário quiser.

### 3.5 Lealdade

A cerimônia de submissão exige a nação fundada (3.1). Quem já é vassalo num jogo salvo de antes continua vassalo (a nação conta como fundada nesses jogos).

### 3.6 Sugestões extras (para o usuário escolher)

1. **Legitimidade (0–100):** sobe com o tempo no trono, vitórias e Fama; cai com a morte do líder, derrotas, Infâmia alta e com a sala do trono sem trono. Abaixo de 30, revoltas menores voltam de tempos em tempos; abaixo de 10, o território para de crescer. Dá peso ao trono depois da fundação.
2. **Sucessão coroada:** se o líder morre, o sucessor assume (como hoje), mas precisa ser **reproclamado no trono** em até 10 dias; senão vem uma revolta menor ("a região não reconhece o herdeiro").
3. **Reação das potências à fundação:** uma carta de cada, no tom delas. Coro: "nota de protocolo" (boa vontade −5 se você não tem trono "de qualidade"). Hélice: proposta de "franquia" com desconto na cerimônia. Forja: pede o registro da nação (taxa em aço; pagar dá boa vontade). Silêncio: "você foi catalogado". Sindicato: oferece mercenários para a próxima revolta, pagos em prata.
4. **Mercenários na revolta:** antes da revolta, comprar defensores do Sindicato do Saque (eles chegam com a revolta e lutam do seu lado; às vezes fogem com a prata).
5. **Decretos da nação (mais tarde):** 1 a 2 leis com prós e contras (ex.: alistamento obrigatório, pedágio nas estradas, fronteiras abertas). Fica para outro bloco, se agradar.

### 3.7 Governo pela ideologia (planejamento, a montar com o usuário)

Cada meme da ideologia do jogador pode acrescentar algo ao governo da nação: **cadeira no conselho** (ligada ou não a uma função da ideologia, como especialista de produção), **mais líderes** com funções próprias, **regra** (ex.: eleição, sucessão, revolta) ou **bônus/penalidade** no Hegemony. Como a ideologia é fluida, uma reforma que troca memes muda o governo (cadeira que some devolve o conselheiro; regra nova vale na hora).

- **Base:** o modpack tem **295 memes** (vanilla, VE Memes, Alpha Memes e outros). Estrutura (animista, arquista, teísta…) e memes comuns podem ter efeito. O Hegemony já usa os memes para a disposição das facções NPC (`FT2_Disposition_meme_*`): dá para ler o mesmo para o jogador.
- **Tabela de propostas (rascunho, 2026-11-19): `nacao/memes.md`.** Os 138 memes do modpack (a contagem de 295 repetia pastas de versão), cada um com o tipo de efeito (governo, cadeira de função, cadeira, regra, bônus ou nenhum) e uma proposta; lista bruta em `nacao/memes-bruto.tsv` (extraída dos XML, sem abrir o jogo). Decisões ✱ no fim dela.
- **Achado:** a **República** do VE Memes (`VME_Republic`) já cria um triunvirato (líder + `VME_IdeoRole_LeaderSecond` e `VME_IdeoRole_LeaderThird`): os "3 líderes" do usuário já existem como funções da ideologia; o nosso sistema só dá a cada triúnviro uma pasta (proposta: Relações, Guerra, Terras). A **Primazia humana** (`HumanPrimacy`) liga a cadeira ao especialista de produção.
- **Ideias para preencher depois (sugestão):** Lealista (`Loyalist`) → sucessão sem revolta; Coletivista (`Collectivist`) → conselho maior, líder mais fraco; Supremacias de gênero → cadeiras só para um gênero; Supremacia mecanoide/insetoide (VE Memes) → cadeira para um mecanitor/criador de insetos com bônus de exército; estruturas teístas → cadeira de sacerdote (legitimidade); Arquista → cadeira técnica (pesquisa).

### Resultado do N1 (2026-11-19)

Feito com as propostas (✱1: qualquer mundo com o Hegemony; ✱2: 3 adultos livres; ✱3, ✱4 e ✱5 como na 3.3 e 3.2). Valores em `Defs/World/EQ_Nacao.xml` (`NationDef`).
- **Trava:** sem nação, nenhuma fonte de território do jogador no Hegemony (postfix em `TerritorySolver.Sources`), F4 parado, cerimônia e juramento direto recusados com o texto da 3.1, veto `InvasionTarget` (o Hegemony não manda invasão de conquista; raids comuns seguem). Vassalo de save antigo conta como fundado.
- **Proclamação:** botão no trono (`Building_Throne.GetGizmos`), escolha por menu entre os candidatos a líder da ideologia (`EQLeader`). Carta `EQ_ProclamationLetter`: esperar, comprar (0,6 de prata por ponto de raid, tira 30–40%), desistir.
- **Revolta:** facção oculta `EQ_Faction_PovosDaRegiao` ("Associação de Moradores da Região", líder "síndico"), raid a pé pela borda. Pontos do mapa × 1,3 × 1,2 por derrota anterior, entre 200 e 8000.
- **Vitória:** carta "Nação fundada", janela de nome da facção, território do F4. **Derrota:** carta, sobra da revolta vai embora, 15 dias de espera.
- **Fica para o N1b:** ideologia clássica → fluida e o editor de memes na vitória (o líder proclamado já é o líder da ideologia). Não fiz: o número de pessoas na carta é aproximado (pontos/85); a revolta ainda não usa roupas e armas das facções vizinhas (sugestão da 3.3).

### Resultado do N1b (2026-11-19)

- **Começo:** nos cenários do Ecos (com o Hegemony e a Ideology) a tela de escolha de ideologia é pulada e o jogador começa com uma ideologia **clássica só dele**; o modo clássico global do jogo continua desligado (as facções NPC têm as delas; o risco dos ~80 pontos não se aplica). `classicStartAllScenarios` liga em todo cenário (✱9 continua aberto; proposta: só os do Ecos).
- **Fundação:** a ideologia vira fluida e abre a escolha dos memes do jogo (estrutura + 1, impacto ≤ 2, regras da fluida). Fechar a janela sem terminar faz ela reabrir. Depois: nome, título do líder e ícone novos, o líder proclamado volta à função, e a 1ª reforma já fica liberada (`freeReformOnFounding`) para acertar os preceitos sorteados. Carta "A fé da nação".
- Jogos sem a ideologia clássica (outro cenário, save antigo) fundam a nação como no N1, sem mexer na fé.

## 4. Blocos

| Bloco | O que entrega | Depende de |
|---|---|---|
| **N1** | Fundação: trava antes da fundação (território, marcos, tributo, cerimônia, veto de invasão), trono + "Proclamar líder", facção "Povos da Região", revolta, vitória/derrota, nome da nação. Roteiro de teste (proclamar, vencer pela força no executor, conferir território e cerimônia) | F4 |
| **N1b** | Ideologia: começar no modo clássico (cenários do Ecos), sair do clássico na fundação com a escolha dos primeiros memes e virar fluida; o líder proclamado ganha a função de líder. **Risco técnico:** o modo clássico é lido em ~80 pontos do jogo (`IdeoManager.classicMode`, `Ideo.classicMode`); trocar no meio da partida precisa de teste próprio (pode virar parte do N1 se for simples) | N1 |
| **N2** | Conselho: 3 cadeiras fixas, janela, bônus nos sistemas (F4, frentes, exércitos, boa vontade, tensão, cerimônias), pensamentos | N1 |
| **N2b** | Governo pela ideologia, 1º corte: grupos 1 (governo) e 2 (cadeiras de função) do `nacao/memes.md` (República com 3 pastas, Primazia humana com o Ministro da Indústria etc.) | N1b, N2 |
| **N2c** | Governo pela ideologia, 2º corte: grupos 3 (regras e bônus no Hegemony) e 4 (estruturas) | N2b |
| **N3** | Sugestões aprovadas da 3.6 (legitimidade, sucessão coroada, reações, mercenários) | N1 (N2 para parte) |

Ordem sugerida: **N1 e N1b antes do F5** (mudam o começo de todo jogo); N2 logo depois; N2b quando a tabela da 3.7 estiver pronta; F5 e N3 em seguida.

## 5. Decisões do usuário (✱)

1. **Mundo aleatório também?** Proposta: sim, a fundação vale em qualquer mundo com o Hegemony (no aleatório, sem o F2–F4, a colônia só ganha o território do Hegemony depois de fundada).
2. **Requisitos da proclamação:** só a sala do trono com trono, ou também um mínimo de colonos (proposta: 3 adultos livres)?
3. **Derrota na revolta:** como na 3.3 (líder morto/capturado ou trono ocupado → 15 dias de espera, revolta 20% maior)?
4. **Tamanho da revolta:** pontos de raid × 1,3?
5. **Comprar parte dos vizinhos** (suborno) e **desistir** na carta: entram?
6. **Conselho:** os bônus da tabela servem? Entra a 4ª cadeira (Intendente)?
7. **Sugestões extras (3.6):** quais entram (1 a 5)?
8. ~~**Governo pela ideologia (3.7)**~~ decidido em 2026-11-19 (`nacao/memes.md` › 6): sem limite de cadeiras (o limite são os 4 memes), República e Primazia humana como propostas, N2b começa pelos grupos 1 e 2, memes do Ecos exclusivos das potências (já feito por `factionWhitelist`).
9. **Ideologia clássica no começo:** vale para todo cenário ou só os do Ecos? Quantos memes na primeira escolha (proposta: as regras da ideologia fluida do jogo)?
