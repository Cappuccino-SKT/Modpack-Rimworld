# Regenera modpack\ (mods.md, workshop-ids.txt, ModsConfig.xml) a partir de Mods\ e da ordem de carga atual do jogo.
# Uso (na raiz do jogo): powershell -ExecutionPolicy Bypass -File modpack\update.ps1
# -FromModpack: usa a ordem de modpack\ModsConfig.xml (não copia a do jogo por cima).
param([switch]$FromModpack)
$ErrorActionPreference = 'Stop'
$mp = $PSScriptRoot
$root = Split-Path $mp -Parent
$cfgPath = Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Config\ModsConfig.xml'
if ($FromModpack) { $cfgPath = Join-Path $mp 'ModsConfig.xml' } else { Copy-Item $cfgPath (Join-Path $mp 'ModsConfig.xml') -Force }
[xml]$cfg = Get-Content $cfgPath -Raw -Encoding UTF8
$active = @($cfg.ModsConfigData.activeMods.li | ForEach-Object { $_.ToLower() })
$rows = foreach ($d in Get-ChildItem (Join-Path $root 'Mods') -Directory | Where-Object Name -match '^\d+$') {
    [xml]$x = Get-Content (Join-Path $d.FullName 'About\About.xml') -Raw -Encoding UTF8
    $p = "$($x.ModMetaData.packageId)".ToLower(); $i = [array]::IndexOf($active, $p)
    [pscustomobject]@{ id = $d.Name; p = $p; n = "$($x.ModMetaData.name)".Trim() -replace '\|', '/'; o = $(if ($i -ge 0) { $i + 1 } else { 999 }) }
}
$rows = $rows | Sort-Object o, n
$md = @('# Lista de mods do modpack', '', 'Gerada por `modpack/update.ps1` a partir de `Mods/` e da ordem de carga em `ModsConfig.xml`. Ordem = posição na lista de carga (DLCs e mods próprios não aparecem aqui).', '', '| Ordem | Mod | packageId | Workshop |', '|---|---|---|---|')
$md += $rows | ForEach-Object { "| {0} | {1} | ``{2}`` | [{3}](https://steamcommunity.com/sharedfiles/filedetails/?id={3}) |" -f $(if ($_.o -eq 999) { 'desativado' } else { $_.o }), $_.n, $_.p, $_.id }
$md += '', '## Mods próprios (branches deste repositório)', '', '| Mod | packageId | Branch |', '|---|---|---|', '| Ecos da Queda | `ecosdaqueda.core` | `mod/ecos-da-queda` |', '| Ecos da Queda - Ferramentas de Debug | `ecosdaqueda.debug` | `mod/ecos-da-queda-debug` |', '| Ecos da Queda - Tradução PT-BR | `ecosdaqueda.traducao` | `mod/ecos-da-queda-traducao` |'
$utf8 = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllLines((Join-Path $mp 'mods.md'), $md, $utf8)
[IO.File]::WriteAllLines((Join-Path $mp 'workshop-ids.txt'), ($rows | ForEach-Object { "{0}`t# {1}" -f $_.id, $_.n }), $utf8)
"$($rows.Count) mods da Workshop; $(@($rows | Where-Object o -eq 999).Count) desativados."
