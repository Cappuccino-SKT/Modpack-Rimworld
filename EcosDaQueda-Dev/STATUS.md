# Status

_Atualizar este arquivo ao fim de cada etapa._

## Onde estamos (2026-11-14)

Fases 1 e 2 concluídas. **Fase 3 feita e testada pelo executor (0 erros), falta a conferência visual do usuário:** submissão por cerimônia, líder do assentamento e cargos por favor. Ver DESIGN.md › Submissão, líder e cargos.

- **Cerimônia de submissão** com emissário (aba Reputação › botão "Cerimônia" de cada potência): gera uma missão; o emissário chega de nave com escolta e preside o ritual com o líder; o **preço** temático é cobrado no fim. Concluída, o mod faz o juramento de vassalagem do Hegemony sozinho. O juramento direto pelo Hegemony está bloqueado para as 4 potências.
- **Líder do assentamento = líder da ideologia do jogador.** Só ele tem cargo e favor. Sem líder (estando numa potência ou com cerimônia pedida), abre a janela de escolha forçada. Sucessor designável (botão "Sucessor"); se o líder morre, o sucessor assume, senão o jogador escolhe. Trocar o líder da ideologia (ritual vanilla de troca de função) transfere cargo e favor.
- **7 cargos por potência** sobre o sistema de títulos da Royalty (`RoyalTitleDef`), com favor de nome próprio: citações (Silêncio), ações preferenciais (Hélice), selos de garantia (Forja), aplausos (Coro). Promoção = juntar o favor do próximo cargo → a potência manda o emissário para a cerimônia de promoção.
- Favor vem de fontes temáticas (pesquisa, comércio, Soro, xenogermes, fabricação de qualidade, rituais), do dízimo, de serviços, das missões da potência e da qualidade das cerimônias.
- Estipêndio, dízimo e serviços continuam, agora pelo cargo do líder (antes era pela Fama). Fama/Infâmia continuam do assentamento.
- Saves antigos (vassalo pelo Hegemony sem cerimônia): o líder recebe o 1º cargo.

**A conferir pelo usuário (visual):** aba Reputação (painel de lealdade com líder/favor/sucessor, botão "Cerimônia" em cada potência, janelas de escolha de líder e sucessor), a cena da cerimônia no mapa (clicar no emissário › iniciar o ritual), a aba de títulos do líder, e as ações do menu de debug.

**Modpack (2026-11-14):** +10 mods da lista do usuário (RimExodus Seamless World + Framework, Trenches, Tech Advancing, YART, Door Mat, Clean Research Sort, SeedsPlease Lite Redux no lugar do Lite original que só ia até 1.4, VVE Upgrades, VQE Generator, VFE Medieval 2). 109 mods ativos, mundo novo com janela: 0 erros. Repositório git criado (ver CLAUDE.md › Versionamento).

**Tradução PT-BR (2026-11-15):** novo mod `Mods/EcosDaQuedaTraducao` (`ecosdaqueda.traducao`, branch `mod/ecos-da-queda-traducao`), ativo no ModsConfig. Traduzidos os mods de genes: **Alpha Genes** (2.349 textos) e 12 **Vanilla Races Expanded** (Pigskin, Hussar, Starjack, Genie, Waster, Fungoid, Highmate, Phytokin, Saurid, Lycanthrope, Sanguophage, Insector). Teste `tests/traducao.txt` (perfil full): tudo injetado, 0 erros de tradução nesses mods; genes gerados por modelo (aptidões, invocações, transformações) também saem em PT-BR. Depois, usando a coleção de traduções do Workshop (3018573588) só como consulta ("base para revisar"; texto nosso), foram traduzidos também: **VRE Android** (710/869), **VFE Empire** (869/881), **VFE Deserters** (481/515), **VPE Hemosage** (71/75), **Vanilla Vehicles Expanded** (725/725), **Vehicle Framework** (697/699) e **Vanilla Psycasts Expanded** (1.142/1.292). O que falta nesses é de propósito: sinais internos de missão, chaves Keyed que o jogo já traduz e `generalRules` de pesquisa que o VEF preenche por código (não injetáveis). Teste `tests/traducao.txt` (perfil full): 0 problemas de tradução em todos, SEM ERROS no log. Fluxo e regras em TECNICO.md › Tradução. **Falta traduzir:** VQE Ancients (690) e os outros ~50 mods sem PT-BR (lista: rodar `tests/trans-export.txt`). Nomes próprios (penteados, barbas, nomes de facção sáuria) ficaram em inglês de propósito. Conferência visual pendente (editor de xenotipo, abas de genes).

**Próximas ideias:** permissões/poderes por cargo (gastar favor, como as permissões do Império), nível psíquico pelos cargos do Coro, exigências por cargo (roupa, quarto), nomes temáticos na janela do ritual (hoje aparece "cerimônia de outorga"), corrupção do Silêncio, censo genético da Hélice, questão androide da Forja, conversão à distância do Coro, guerras pela API do Hegemony.

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
| 2026-11-14 | Fase 3: cerimônia de submissão com emissário, líder/sucessor, 7 cargos por potência com favor (títulos da Royalty), fontes temáticas de favor; +10 mods; git. Corrigido: Highmate (incapaz de violência) saía em grupos de combate da Hélice |
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
