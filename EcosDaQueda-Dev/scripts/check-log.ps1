# Resume erros e avisos do Player.log, filtrando ruído conhecido.
# Uso: powershell -File check-log.ps1 [-Prev] [-Trace "trecho da mensagem"]
param([switch]$Prev, [string]$Trace)

$dir = Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Logs'
$log = Join-Path $dir ($(if ($Prev) { 'Player-prev.log' } else { 'Player.log' }))
$lines = Get-Content $log -Encoding UTF8

$noise = 'Knowledge data was missing key|GAGARIN|\[Vulkan\]|NativeAllocation|hideFlags|Translation data for language|Could not find player faction'

if ($Trace) {
    # Mostra a primeira ocorrência com 25 linhas de stack trace
    $i = ($lines | Select-String -SimpleMatch $Trace | Select-Object -First 1).LineNumber
    if ($i) { $lines[($i-1)..([Math]::Min($i+24, $lines.Count-1))] } else { "Nao encontrado: $Trace" }
    return
}

$entries = $lines | Where-Object { $_ -match '^\d{4}-\d\d-\d\d ' }
"== Totais por nivel"
$entries | ForEach-Object { ($_ -split '\s+')[2] } | Group-Object | ForEach-Object { "{0,6} {1}" -f $_.Count, $_.Name }
"`n== ERROR/WARN relevantes (agrupados)"
$entries | Where-Object { $_ -match ' (ERROR|WARN) ' -and $_ -notmatch $noise } |
    ForEach-Object { $_.Substring(24).Substring(0, [Math]::Min(300, $_.Length - 24)) } |
    Group-Object | Sort-Object Count -Descending | Select-Object -First 40 |
    ForEach-Object { "{0,5} {1}" -f $_.Count, $_.Name }
"`n== Linhas que citam o mod (EQ_ / EcosDaQueda)"
$lines | Select-String 'EQ_|EcosDaQueda|ecosdaqueda' | Select-Object -First 20 | ForEach-Object { $_.Line.Substring(0, [Math]::Min(250, $_.Line.Length)) }
