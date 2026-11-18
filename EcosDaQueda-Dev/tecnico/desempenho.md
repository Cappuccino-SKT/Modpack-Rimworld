# Desempenho (Etapa 10a)

## Ferramenta

- `perf start`: instala (uma vez) prefixo/sufixo com relógio em `TickManager.DoSingleTick` (total), `World.WorldTick`, pawns/objetos/facções do mundo, `Map.MapPreTick`/`MapPostTick`, `TickList.Tick` (coisas do mapa), narrador e **todo** override de `WorldComponentTick`, `MapComponentTick` e `GameComponentTick` de qualquer mod (172 métodos no perfil full). Zera os contadores.
- `perf report [arquivo]`: ms por tique de cada um, % do tique inteiro (itens aninhados: os componentes do mundo estão dentro de `World.WorldTick`), contagem de pawns e objetos do mundo; tabela completa em `test-profile/<perfil>/perf.tsv`.
- Roteiro: `tests/perf.txt` (1 dia + 10 dias; ~5 min no lote). Fora da regressão padrão. Código: `Mods/EcosDaQuedaDebug/Source/Perf.cs`.
- Mede só a simulação (sem desenho, perfil sem janela): o número é tiques/s **máximo** da CPU, não o FPS do jogador.

## Linha de base (2026-11-18, perfil full, jogo novo, Crashlanded, 1 mapa)

| Trecho | dia 0–1 | dias 1–11 |
|---|---|---|
| Tique inteiro | 0,472 ms (≈2100 tiques/s) | 0,411 ms (≈2430 tiques/s) |
| Mundo (`World.WorldTick`) | 30% | 29% |
| — objetos do mundo (bases, caravanas, expedições) | 6,6% | 7,4% |
| — facções | 2,7% | 3,0% |
| — pawns do mundo (20–27) | 5,8% | 2,3% |
| — Faction Territories 2 (soma dos 8 componentes) | ~3,5% | ~3,5% |
| Mapa (pré + pós tique) | 46% | 50% |
| Coisas do mapa (`TickList`) | 20% | 16% |
| Maior componente de mapa: Dubs Bad Hygiene | 1,1% | 1,2% |

Leitura: no começo do jogo nada pesa; velocidade 3 (180 tiques/s) usa <10% do orçamento de simulação. O custo vai aparecer com o tempo (pawns do mundo, guerras do Hegemony/Faction Territories, colônia grande). **Próximas medições:** save longo do usuário (`-Save <nome>`), e jogo de 60+ dias com guerras. Só depois decidir o 10c (mod de desempenho) e o 10d.
