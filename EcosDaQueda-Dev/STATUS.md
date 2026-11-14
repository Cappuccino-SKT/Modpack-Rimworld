# Status

_Atualizar este arquivo ao fim de cada etapa._

## Onde estamos (2026-11-13)

Fase 1 (XML) e ajustes (ideologias fixas, ícones/cores) **concluídos e confirmados pelo usuário**.

**Fase 2 em andamento — etapa 1 feita (não testada em jogo):**
- Relações fixas entre as 4 potências (`1.7/Defs/EQ_FactionRelations.xml`): Silêncio×Coro ódio mortal (permanente, paz vetada no Hegemony), Silêncio×Forja guerra, Silêncio×Hélice desconfiança, Hélice×Forja rivalidade, Hélice×Coro aliança frágil, Forja×Coro neutralidade.
- Reputação Fama/Infâmia (`1.7/Defs/EQ_Reputation.xml`) com títulos satíricos 3×3 e aba **Reputação** no menu inferior.
- Módulo `EcosDaQueda.Hegemony.dll` (veto de paz).
- Carregamento do jogo: **0 erros**.

**Confirmado pelo usuário (2026-11-14):** relações aplicadas (verificado no save) e reputação funcionando — matou um membro da Forja: Infâmia com a Forja subiu e Fama com Silêncio e Hélice (inimigas da Forja, ≤ -50) subiu; Coro (neutro com a Forja) não mudou.

**Etapa 2 feita (2026-11-14, carregamento 0 erros, não testada em jogo):** efeitos da reputação — cada um dos 9 títulos altera preços no comércio com a facção e pode disparar presentes, tributos (não hostis) ou caçadores de recompensa (hostis); a aba Reputação mostra os efeitos do título atual. Ver DESIGN.md › Reputação.

**Etapa 3 feita (2026-11-14, carregamento 0 erros, não testada em jogo):** postos na facção — jurar lealdade pelo Hegemony torna o jogador membro; 4 postos por potência pela Fama; estipêndio a cada 10 dias; dízimos por carta de escolha (entregar prisioneiro/itens/prata/colono, ou recusar); painel "Lealdade" no topo da aba Reputação. Ver DESIGN.md › Postos e lealdade. Lição: `ThingDefCountClass` em XML é `<Steel>50</Steel>`, não `<li><thingDef>…`.

**Etapa 4 feita (2026-11-14, não testada em jogo):** mecânicas por facção — 8 serviços para membros (botão "Serviços" na aba Reputação), item "o Soro" (Hélice) e retaliações-assinatura quando hostil. Ver DESIGN.md › Mecânicas por facção.

**Ferramentas de debug (2026-11-14):** mod `Mods/EcosDaQuedaDebug` com executor de testes sem janela (`scripts/run-test.ps1`, ver TESTES.md) e ações no menu de debug. Ativado também no ModsConfig do usuário. Testes rodados e passando (0 erros): `fase2`, `fase2b`, `retaliacao`, `save-test` (perfil full com o save do usuário). Bugs achados e corrigidos pelos testes: Infâmia em dobro ao recusar/ignorar dízimo; invasão de retaliação com pontos insuficientes em colônia nova (`raidMinPoints` 200); incidentes-assinatura nunca disparavam (`CanFireNow` recusa incidentes forçados). **Desenvolvimento do mod principal pausado a pedido do usuário** para outras modificações.

**Próximas etapas da fase 2:** aprofundar mecânicas (corrupção do Silêncio, censo genético da Hélice, questão androide da Forja, conversão à distância do Coro), relações dinâmicas com o Hegemony (declarar guerras pela API).

## Feito

- [x] Mods da lista baixados via SteamCMD (`Mods/<id>`), ordem de carregamento gerada (`scripts/sortmods.ps1`).
- [x] Compatibilidade 1.7 da modlist (ver TESTES.md › histórico).
- [x] Mod `Mods/EcosDaQueda` criado: 4 memes, 4 facções com ideologia fixa, 16 PawnKinds, nomes de assentamentos, disposições do Hegemony.
- [x] Correção: caminho VPE_Puppeteer → VPE_Empath.
- [x] Correção: nomes de assentamento ampliados (não testado em mundo novo ainda).
- [x] Desativados no ModsConfig: **Hearts of RimWorld** (3833698946) e **Rimworld Exploration Mode** (2941608795). Pastas continuam em `Mods/`.

## Pendente / a verificar

- [ ] Mundo novo: conferir na aba Ideologia nome, ícone, cor, divindades, preceitos, cargos e rituais das 4; nome do líder fundador na aba Facções.
- [ ] Ver se `exactPrecepts` removendo preceitos exigidos por memes gera avisos ou comportamento estranho.
- Preceitos de livros (VME_Library_Required, VME_BookWriting_Exalted) só entram se **Vanilla Books Expanded** for instalado.

- [ ] Confirmar em mundo novo que os nomes de assentamento não esgotam mais.
- [ ] Ver em jogo: visual das unidades (robes, xenotipos, mechs na Forja, psycasts no Coro).
- [ ] Confirmar que mechs numa facção humana (Forja) se comportam bem em raides.
- [ ] Ajustes pedidos pelo usuário (próxima mensagem dele).

## Fase 2 (C#) — planejado

Ver DESIGN.md › Sistemas planejados: relações fixas, Fama/Infâmia, postos, mecânicas por facção.

## Histórico

| Data | Mudança |
|---|---|
| 2026-11-14 | Mod de debug + executor de testes headless; 3 bugs corrigidos via testes |
| 2026-11-14 | Fase 2 etapa 4: serviços (FactionServiceDef), o Soro (EQ_Soro), retaliação-assinatura (hostileIncidents) |
| 2026-11-14 | Teste do usuário (save "Test", dia 4,6): jurou lealdade à Hélice pelo Hegemony → mod detectou (memberOf=Hélice, posto 0 Cliente Básico, aliado +75). Estipêndio (10 d) e dízimo (15 d) ainda não venceram — **não verificados em jogo** |
| 2026-11-14 | Fase 2 etapa 3: postos/lealdade (FactionAllegianceDef, WorldComponent_Allegiance, ChoiceLetter_EQTithe, leitura da fealty do Hegemony por reflexão, painel na aba) |
| 2026-11-14 | Teste do usuário (save "Test"): reputação ok (Silêncio/Hélice Fama 38,5 = Aceito; Forja Infâmia 77 = Malvisto, hostil). Corrigido: facções proxy do Hegemony (expedições) tratadas como potências (`EQUtil`), e `exactPrecepts` removendo rituais ocultos. Avisos "Faction_28 not deep-saved" eram do CE/VTE, não nossos |
| 2026-11-14 | Fase 2 etapa 2: efeitos da reputação (ReputationTier com priceGain/giftChance/tributeChance/raidChance, patch de preços no comércio, eventos periódicos, efeitos na aba) |
| 2026-11-13 | Fase 2 etapa 1: relações fixas (C# + XML), reputação Fama/Infâmia com Harmony + aba Reputação, módulo de integração Hegemony (veto de paz). Remote Control ativado na sessão |
| 2026-11-11 | Ícones com contorno escuro e traço grosso + cores claras fixas (colorSpectrum único); **confirmado pelo usuário: visíveis no mapa-múndi** |
| 2026-11-11 | Ideologias fixas (confirmado pelo usuário em mundo novo): C# IdeoTemplateDef/WorldComponent, ícones próprios (make-icons.ps1), cores, memes revisados (Darkness no Silêncio, Trader na Hélice, Supremacia Mecanoide na Forja, Foco Psíquico no Coro), requiredPreceptsOnly |
| 2026-11-11 | Download dos mods, ajuste 1.7, criação do mod (fase 1), correções de VPE e nomes, desativação de Hearts of RimWorld e Exploration Mode, criação desta pasta e do CLAUDE.md |
