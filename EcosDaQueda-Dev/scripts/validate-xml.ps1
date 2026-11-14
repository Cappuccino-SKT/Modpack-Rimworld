# Verifica se todos os XML do mod são bem formados.
$mod = Join-Path $PSScriptRoot '..\..\Mods\EcosDaQueda'
$bad = 0
Get-ChildItem $mod -Recurse -Filter *.xml | ForEach-Object {
    try { [xml](Get-Content $_.FullName -Raw -Encoding UTF8) | Out-Null; "OK   $($_.FullName.Substring((Resolve-Path $mod).Path.Length))" }
    catch { $bad++; "ERRO $($_.FullName): $($_.Exception.Message)" }
}
if ($bad) { exit 1 }
