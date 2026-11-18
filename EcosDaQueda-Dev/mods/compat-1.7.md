# Compatibilidade dos mods com o RimWorld 1.7

Levantamento de 2026-11-18 sobre os 120 mods ativos do modpack (About.xml + pastas de versão), cruzado com a regressão completa (32 testes, perfil full).

## Resumo

- **91 mods declaram 1.7** em `supportedVersions` ou têm pasta `1.7/`.
- **29 declaram só até 1.6, mas carregam e funcionam**: o RimWorld 1.7 carrega a pasta 1.6 (ou a raiz) e nenhum dos 32 testes acusou erro deles além do ruído conhecido.
- **Nenhum mod de versão mais antiga** (≤1.5) está ativo.
- Avisos 'mod feito para outra versão' no menu são esperados para os mods abaixo; não é preciso desativar nada.

## Mods que ainda não declaram 1.7

| Mod | packageId | Versões declaradas | Observação |
|---|---|---|---|
| Adaptive Storage Framework | `adaptive.storage.framework` | 1.4,1.5,1.6 |  |
| HugsLib | `unlimitedhugs.hugslib` | 1.0,1.1,1.2,1.3,1.4,1.5,1.6 | Biblioteca; carrega sem erro. |
| Vehicle Framework | `smashphil.vehicleframework` | 1.4,1.5,1.6 | Usado pela Etapa 2 (veículos); testes veiculos/fechamento-tech passam. |
| Adaptive Simple Storage | `adaptive.simplestorage` | 1.4,1.5,1.6 |  |
| LWM's Adaptive Deep Storage | `asf.deepstorage` | 1.4,1.5,1.6 |  |
| Adaptive Primitive Storage | `adaptive.primitivestorage` | 1.5,1.6 |  |
| Allow Tool | `unlimitedhugs.allowtool` | 1.0,1.1,1.2,1.3,1.4,1.5,1.6 |  |
| Cherry Picker | `owlchemist.cherrypicker` | 1.3,1.4,1.5,1.6 |  |
| Common Sense | `avilmask.commonsense` | 1.0,1.1,1.2,1.3,1.4,1.5,1.6 |  |
| EdB Prepare Carefully | `edb.preparecarefully` | 1.1,1.2,1.3,1.4,1.5,1.6 |  |
| Pick Up And Haul | `mehni.pickupandhaul` | 1.0,1.1,1.2,1.3,1.4,1.5,1.6 |  |
| Simple sidearms | `petetimessix.simplesidearms` | 1.0,1.1,1.2,1.3,1.4,1.5,1.6 |  |
| Snap Out! | `weilbyte.snapout` | 1.1,1.0,1.2,1.3,1.4,1.5,1.6 |  |
| Realistic Rooms Rewritten | `lucifer.realisticrooms` | 1.3,1.4,1.5,1.6 |  |
| Vanilla Vehicles Expanded | `oskarpotocki.vanillavehiclesexpanded` | 1.4,1.5,1.6 | Idem; EQ_TituloSeminovo depende dele. |
| World Map Enhanced (Continued) | `zal.worldmapenhanced` | 1.4,1.5,1.6 |  |
| Combat Extended | `ceteam.combatextended` | 1.6 | Erros de AsyncGPUReadback/CropVertical só no modo sem janela (ruído filtrado no executor). |
| Missile Girl - Performance Mod | `vr.missilegirl` | 1.6 | Mod de desempenho; ver Etapa 10c. |
| Combat Extended Armors | `ceteam.combatextendedarmors` | 1.3,1.4,1.5,1.6 |  |
| Combat Extended Armory | `ceteam.combatextendedarmory` | 1.4,1.5,1.6 |  |
| Combat Extended Guns | `ceteam.combatextendedguns` | 1.3,1.4,1.5,1.6 |  |
| Door Mat | `dracoix.doormat.r12a` | 1.0,1.1,1.2,1.3,1.4,1.5,1.6 |  |
| SeedsPlease: Lite Redux | `evyatar108.seedspleaseliteredux` | 1.4,1.5,1.6 |  |
| Harvest Organs Post Mortem Continued | `smuffle.harvestorganspostmortem` | 1.0,1.1,1.2,1.3,1.4,1.5,1.6 |  |
| Smart Farming | `owlchemist.smartfarming` | 1.3,1.4,1.5,1.6 |  |
| Show Me Your Tools | `meathax.showmeyourtools` | 1.6 |  |
| Character Creator | `astryl.moderncc` | 1.6 | Só interface; nada testável sem janela. |
| Hospitality | `orion.hospitality` | 1.0,1.1,1.2,1.3,1.4,1.5,1.6 |  |
| Clean Textures | `ih.clean.textures` | 1.3,1.4,1.5,1.6 |  |

## Problemas de execução achados e corrigidos no nosso mod

- **Vanilla Exploration Expanded — `TileMutatorWorker_Predators`**: guarda a lista de predadores do primeiro bioma da sessão e devolve null quando não há predador comum → `Error in GenStep` (NRE) ao gerar mapa. Corrigido por Harmony em `Source/EcosDaQueda/ThirdPartyFixes.cs` (lista por bioma/costa, vazia em vez de null). Era o 'NRE intermitente de GenStep' do arsenal-forja.

## Intermitentes ainda abertos (não bloqueiam)

- Alpha Memes: ideologia removida durante o teste; MeatBurst; BestowingCeremony; VIE 'invalid pawn role'; 'pathing ran out of nodes'. Ver TESTES.md (ruído conhecido).

## Como refazer

Tabela bruta gerada por script lendo `Mods/*/About/About.xml` e as pastas de versão de cada mod ativo em `ModsConfig.xml`. Erros de execução: `regress.ps1` (os logs de cada abertura do lote ficam em `tests/out/lote-*.log`).
