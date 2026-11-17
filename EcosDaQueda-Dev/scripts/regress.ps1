# Roda a regressão inteira (perfil full) e imprime UMA linha por teste.
#
# Uso:
#   regress.ps1                      (conjunto padrão)
#   regress.ps1 -Build               (compila antes)
#   regress.ps1 -Tests arsenal,comercio
#   regress.ps1 -Long                (inclui mundo.txt, 15 dias de jogo, ~20 min)
# Relatórios completos em tests\out\<teste>.txt.full.result.txt.
param(
    [string[]]$Tests = @('ideo', 'cerimonia-helice', 'cerimonia-forja', 'cerimonia-coro', 'psy-coro', 'androides', 'permissoes', 'bases', 'medieval', 'insetos', 'reino-lodo', 'fungobi', 'inicio', 'inicio-forja', 'arsenal', 'arsenal-forja',
                         'comercio', 'faccoes-menores', 'faccoes-menores2', 'xenotipos', 'fase3'),
    [switch]$Build,
    [switch]$Long
)
$ErrorActionPreference = 'Stop'
$dev = Split-Path $PSScriptRoot -Parent
$run = Join-Path $PSScriptRoot 'run-test.ps1'
if ($Long) { $Tests += 'mundo' }
$falhas = 0
$first = $true
foreach ($t in $Tests) {
    $script = Join-Path $dev "tests\$t.txt"
    $result = Join-Path $dev "tests\out\$t.txt.full.result.txt"
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $timeout = if ($t -eq 'mundo') { 2400 } else { 900 }
    try {
        if ($Build -and $first) { & $run -Script $script -Profile full -TimeoutSec $timeout -Build | Out-Null }
        else { & $run -Script $script -Profile full -TimeoutSec $timeout | Out-Null }
    } catch { "{0,-20} FALHOU AO RODAR: {1}" -f $t, $_.Exception.Message; $falhas++; $first = $false; continue }
    $first = $false
    $lines = if (Test-Path $result) { @(Get-Content $result -Encoding UTF8) } else { @() }
    $res = $lines | Where-Object { $_ -match '^# RESULTADO' } | Select-Object -Last 1
    $prob = @($lines | Where-Object { $_ -match 'PROBLEMAS' }).Count
    if (-not $res) { $res = '# RESULTADO: SEM RELATÓRIO (travou ou não iniciou)' }
    $ok = ($res -match 'SEM ERROS') -and ($prob -eq 0)
    if (-not $ok) { $falhas++ }
    "{0,-20} {1,4}s  {2}{3}" -f $t, [int]$sw.Elapsed.TotalSeconds, ($res -replace '^# RESULTADO: ', ''), $(if ($prob) { " | PROBLEMAS em $prob comando(s)" } else { '' })
    if (-not $ok) { $lines | Where-Object { $_ -match '^!!|PROBLEMAS' } | Select-Object -First 3 | ForEach-Object { '      ' + $_.Substring(0, [Math]::Min(200, $_.Length)) } }
}
"== {0} teste(s), {1} com falha" -f $Tests.Count, $falhas
