param([string]$ModsDir, [string]$ConfigPath)
$ErrorActionPreference = 'Stop'
$official = @('ludeon.rimworld','ludeon.rimworld.royalty','ludeon.rimworld.ideology','ludeon.rimworld.biotech','ludeon.rimworld.anomaly','ludeon.rimworld.odyssey')
$mods = [ordered]@{}
foreach ($o in $official) { $mods[$o] = @{ Id=$o; Name=$o; After=@(); Before=@() } }
function Lis($node) { if ($node) { @($node.li | ForEach-Object { "$_".Trim().ToLower() } | Where-Object { $_ }) } else { @() } }
foreach ($d in Get-ChildItem $ModsDir -Directory) {
  [xml]$x = Get-Content (Join-Path $d.FullName 'About\About.xml') -Raw -Encoding UTF8
  $m = $x.ModMetaData
  $id = "$($m.packageId)".Trim().ToLower()
  $after = @(Lis $m.loadAfter) + @(Lis $m.forceLoadAfter) + @(Lis $m.loadAfterByVersion.'v1.6') + @($m.modDependencies.li | ForEach-Object { "$($_.packageId)".Trim().ToLower() })
  $before = @(Lis $m.loadBefore) + @(Lis $m.forceLoadBefore) + @(Lis $m.loadBeforeByVersion.'v1.6')
  $mods[$id] = @{ Id=$id; Name="$($m.name)"; After=$after; Before=$before }
}
# official content always after Harmony, before everything else that does not say otherwise
$edges = @{}; foreach ($k in $mods.Keys) { $edges[$k] = New-Object System.Collections.Generic.HashSet[string] }
function AddEdge($a,$b) { if ($mods.Contains($a) -and $mods.Contains($b) -and $a -ne $b) { [void]$edges[$a].Add($b) } }  # a before b
for ($i=0; $i -lt $official.Count-1; $i++) { AddEdge $official[$i] $official[$i+1] }
foreach ($k in @($mods.Keys)) {
  $m = $mods[$k]
  foreach ($a in $m.After) { AddEdge $a $k }
  foreach ($b in $m.Before) { AddEdge $k $b }
  if ($official -notcontains $k -and $m.Before -notcontains 'ludeon.rimworld') { AddEdge 'ludeon.rimworld.odyssey' $k }
}
# Kahn topological sort; tie-break: frameworks/libraries first, then by name
$indeg = @{}; foreach ($k in $mods.Keys) { $indeg[$k] = 0 }
foreach ($k in $mods.Keys) { foreach ($t in $edges[$k]) { $indeg[$t]++ } }
$rank = { param($id) $n = $mods[$id].Name; if ($id -eq 'brrainz.harmony') {0} elseif ($official -contains $id) {1} elseif ($n -match 'Framework|HugsLib|XML Extensions|Utility') {2} elseif ($id -like 'ceteam.*') {4} else {3} }
$order = New-Object System.Collections.Generic.List[string]
$ready = New-Object System.Collections.Generic.List[string]
foreach ($k in $mods.Keys) { if ($indeg[$k] -eq 0) { $ready.Add($k) } }
while ($ready.Count -gt 0) {
  $next = $ready | Sort-Object { & $rank $_ }, { $mods[$_].Name } | Select-Object -First 1
  [void]$ready.Remove($next); $order.Add($next)
  foreach ($t in $edges[$next]) { $indeg[$t]--; if ($indeg[$t] -eq 0) { $ready.Add($t) } }
}
if ($order.Count -ne $mods.Count) { throw "Ciclo na ordem: " + (($mods.Keys | Where-Object { $order -notcontains $_ }) -join ', ') }
[xml]$cfg = Get-Content $ConfigPath -Raw -Encoding UTF8
$am = $cfg.ModsConfigData.activeMods; $am.RemoveAll()
foreach ($id in $order) { $li = $cfg.CreateElement('li'); $li.InnerText = $id; [void]$am.AppendChild($li) }
$cfg.Save($ConfigPath)
$i=0; foreach ($id in $order) { $i++; "{0,3}. {1}" -f $i, $mods[$id].Name }
