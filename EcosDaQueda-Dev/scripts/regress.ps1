# Roda a regressão inteira (perfil full) e imprime UMA linha por teste.
#
# Uso:
#   regress.ps1                      (conjunto padrão, em lote: o jogo abre uma vez só)
#   regress.ps1 -Build               (compila antes)
#   regress.ps1 -Tests arsenal,comercio
#   regress.ps1 -Long                (inclui mundo.txt, 15 dias de jogo, ~20 min)
#   regress.ps1 -Parallel 2          (2 jogos ao mesmo tempo, cada um com seu perfil; ver TESTES.md antes)
#   regress.ps1 -Single              (modo antigo: abre o jogo uma vez por teste)
# No lote, um teste que trava é morto pelo vigia do run-test (TRAVOU no relatório) e o lote segue no seguinte;
# teste sem relatório nenhum roda de novo sozinho no fim.
# Relatórios completos em tests\out\<teste>.txt.full.result.txt.
param(
    [string[]]$Tests = @('ideo', 'cerimonia-helice', 'cerimonia-forja', 'cerimonia-coro', 'psy-coro', 'androides', 'permissoes', 'bases', 'medieval', 'insetos', 'missoes', 'sanidade', 'reino-lodo', 'colmeia', 'sucata', 'veiculos', 'marcos', 'genes', 'pesquisa', 'licencas', 'engenharia-reversa', 'dados-antigos', 'zonas-cercos', 'pedintes', 'livros', 'fungobi', 'inicio', 'inicio-forja', 'arsenal', 'arsenal-forja', 'kits', 'arvore-pesquisa',
                         'comercio', 'faccoes-menores', 'faccoes-menores2', 'xenotipos', 'fase3', 'fechamento-tech', 'eventos-potencias', 'eventos-mundo', 'mercados', 'rituais', 'obrigacoes', 'encargos', 'promocao', 'tropas', 'comando', 'frente', 'frente-hegemony', 'investigacao', 'narrador', 'tensao', 'fora-de-linha', 'nao-catalogados', 'menores-eventos'),
    [switch]$Build,
    [switch]$Long,
    [switch]$Single,
    [int]$Parallel = 1
)
$ErrorActionPreference = 'Stop'
$dev = Split-Path $PSScriptRoot -Parent
$run = Join-Path $PSScriptRoot 'run-test.ps1'
if ($Long) { $Tests += 'mundo' }
$total = [Diagnostics.Stopwatch]::StartNew()
if ($Build) { & (Join-Path $PSScriptRoot 'build-mods.ps1') }

function Get-ResultPath($t) { Join-Path $dev "tests\out\$t.txt.full.result.txt" }
function Get-Timeout($t) { if ($t -eq 'mundo') { 2400 } else { 900 } }

# Imprime a linha de resumo de um teste. Em $script:lastOk: $true se passou, $false se falhou, $null se não há relatório.
# (O resultado não pode ser o retorno da função: a linha impressa também iria para a variável.)
function Show-Result([string]$t, [int]$secs) {
    $result = Get-ResultPath $t
    $lines = if (Test-Path $result) { @(Get-Content $result -Encoding UTF8) } else { @() }
    $res = $lines | Where-Object { $_ -match '^# RESULTADO' } | Select-Object -Last 1
    $script:lastOk = $null
    if (-not $res) { return }
    if ($secs -lt 0) {
        $d = $lines | Where-Object { $_ -match '^# fim: .*duração (\d+)s' } | Select-Object -Last 1
        $secs = if ($d -and $d -match 'duração (\d+)s') { [int]$Matches[1] } else { 0 }
    }
    # Só "PROBLEMA" em maiúsculas é problema ("problemas=0" do xenogen é contagem, não alarme).
    $prob = @($lines | Where-Object { $_ -cmatch 'PROBLEMA' }).Count
    $ok = ($res -match 'SEM ERROS') -and ($prob -eq 0)
    "{0,-20} {1,4}s  {2}{3}" -f $t, $secs, ($res -replace '^# RESULTADO: ', ''), $(if ($prob) { " | PROBLEMA em $prob linha(s)" } else { '' })
    if (-not $ok) { $lines | Where-Object { $_ -match '^!!' -or $_ -cmatch 'PROBLEMA' } | Select-Object -First 3 | ForEach-Object { '      ' + $_.Substring(0, [Math]::Min(200, $_.Length)) } }
    $script:lastOk = $ok
}

$falhas = 0
$pendentes = @()
if (-not $Single) {
    foreach ($t in $Tests) { Remove-Item (Get-ResultPath $t) -ErrorAction SilentlyContinue }
    $scripts = @($Tests | ForEach-Object { Join-Path $dev "tests\$_.txt" })
    if ($Parallel -le 1) {
        try { & $run -Batch $scripts -Profile full -TimeoutSec 900 | Where-Object { $_ -match '^!!' } } catch { "lote FALHOU AO RODAR: $($_.Exception.Message)" }
    } else {
        $groups = @(for ($i = 0; $i -lt $Parallel; $i++) { , @() })
        for ($i = 0; $i -lt $scripts.Count; $i++) { $groups[$i % $Parallel] += $scripts[$i] }
        $jobs = for ($i = 0; $i -lt $Parallel; $i++) {
            if ($groups[$i].Count -eq 0) { continue }
            Start-Job -ArgumentList $run, $groups[$i], ($i + 1) -ScriptBlock {
                param($run, $list, $slot)
                & $run -Batch $list -Profile full -Slot $slot -TimeoutSec 900 | Where-Object { $_ -match '^!!' }
            }
            Start-Sleep -Seconds 20   # não carregar os dois jogos no mesmo instante
        }
        $jobs | Wait-Job | Receive-Job -ErrorAction SilentlyContinue
        $jobs | Remove-Job
    }
    foreach ($t in $Tests) {
        Show-Result $t -1
        $ok = $script:lastOk
        if ($null -eq $ok) { $pendentes += $t } elseif (-not $ok) { $falhas++ }
    }
    if ($pendentes.Count -gt 0) { "-- sem relatório no lote, de novo um por um: $($pendentes -join ', ')" }
} else { $pendentes = $Tests }

foreach ($t in $pendentes) {
    $script = Join-Path $dev "tests\$t.txt"
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try { & $run -Script $script -Profile full -TimeoutSec (Get-Timeout $t) | Out-Null }
    catch { "{0,-20} FALHOU AO RODAR: {1}" -f $t, $_.Exception.Message; $falhas++; continue }
    Show-Result $t ([int]$sw.Elapsed.TotalSeconds)
    $ok = $script:lastOk
    if ($null -eq $ok) { "{0,-20} {1,4}s  SEM RELATÓRIO (travou ou não iniciou)" -f $t, [int]$sw.Elapsed.TotalSeconds; $falhas++ }
    elseif (-not $ok) { $falhas++ }
}
"== {0} teste(s), {1} com falha, {2:0.0} min" -f $Tests.Count, $falhas, $total.Elapsed.TotalMinutes
