# Roda um teste automático do Ecos da Queda, sem janela e sem tocar nos saves/config do jogador.
#
# Uso:
#   run-test.ps1 -Script ..\tests\smoke.txt                 (perfil "lean": só os mods essenciais, carrega rápido)
#   run-test.ps1 -Script ..\tests\smoke.txt -Profile full   (todos os mods ativos do jogador)
#   run-test.ps1 -Script ..\tests\x.txt -Save Test          (copia o save "Test" do jogador para o perfil de teste)
#   run-test.ps1 -Script ..\tests\x.txt -Window             (mostra a janela do jogo, para depurar)
#   run-test.ps1 -Build ...                                 (compila os 3 projetos C# antes)
#
# O jogo roda com -savedatafolder numa pasta isolada (EcosDaQueda-Dev\test-profile\<perfil>),
# executa os comandos do script (ver TESTES.md), grava <script>.result.txt em tests\out e fecha.
param(
    [Parameter(Mandatory)][string]$Script,
    [ValidateSet('lean', 'full')][string]$Profile = 'lean',
    [string]$Save,
    [switch]$Window,
    [switch]$Build,
    [int]$TimeoutSec = 1200
)
$ErrorActionPreference = 'Stop'
$dev = Split-Path $PSScriptRoot -Parent
$game = Split-Path $dev -Parent
$userData = Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios'
$Script = (Resolve-Path $Script).Path

if (Get-Process RimWorld -ErrorAction SilentlyContinue) { throw 'O RimWorld já está aberto. Feche antes de rodar um teste.' }

if ($Build) {
    foreach ($proj in 'Mods\EcosDaQueda\Source\EcosDaQueda', 'Mods\EcosDaQueda\Source\EcosDaQueda.Hegemony', 'Mods\EcosDaQuedaDebug\Source') {
        $out = dotnet build (Join-Path $game $proj) -c Release 2>&1
        if ($LASTEXITCODE -ne 0) { $out | Select-String 'error' | Select-Object -First 15; throw "Falha ao compilar $proj" }
    }
    'build ok'
}

# ---- perfil isolado
$profileDir = Join-Path $dev "test-profile\$Profile"
New-Item -ItemType Directory -Force (Join-Path $profileDir 'Config'), (Join-Path $profileDir 'Saves') | Out-Null
foreach ($f in 'Prefs.xml', 'KeyPrefs.xml', 'Knowledge.xml') {
    $src = Join-Path $userData "Config\$f"
    if (Test-Path $src) { Copy-Item $src (Join-Path $profileDir 'Config') -Force }
}
[xml]$cfg = Get-Content (Join-Path $userData 'Config\ModsConfig.xml') -Raw -Encoding UTF8
$active = $cfg.ModsConfigData.activeMods
if ($Profile -eq 'lean') {
    $lean = 'brrainz.harmony', 'ludeon.rimworld', 'ludeon.rimworld.royalty', 'ludeon.rimworld.ideology', 'ludeon.rimworld.biotech',
            'ludeon.rimworld.anomaly', 'ludeon.rimworld.odyssey', 'oskarpotocki.vanillafactionsexpanded.core', 'jaeger972.utilityframework',
            'vanillaexpanded.vmemese', 'sarg.alphamemes', 'vanillaexpanded.vpsycastse', 'jaeger972.factionterritories2', 'ecosdaqueda.core'
    $active.RemoveAll()
    foreach ($id in $lean) { $li = $cfg.CreateElement('li'); $li.InnerText = $id; [void]$active.AppendChild($li) }
}
@($active.SelectNodes('li') | Where-Object { $_.InnerText -eq 'ecosdaqueda.debug' }) | ForEach-Object { [void]$active.RemoveChild($_) }
$li = $cfg.CreateElement('li'); $li.InnerText = 'ecosdaqueda.debug'; [void]$active.AppendChild($li)
$cfg.Save((Join-Path $profileDir 'Config\ModsConfig.xml'))

if ($Save) {
    $src = Join-Path $userData "Saves\$Save.rws"
    if (-not (Test-Path $src)) { throw "Save não encontrado: $src" }
    Copy-Item $src (Join-Path $profileDir 'Saves') -Force
}

# ---- executa
$outDir = Join-Path $dev 'tests\out'
New-Item -ItemType Directory -Force $outDir | Out-Null
$result = Join-Path $outDir ((Split-Path $Script -Leaf) + ".$Profile.result.txt")
Remove-Item $result -ErrorAction SilentlyContinue
$gameArgs = @("-savedatafolder=$profileDir", "-eqtest=$Script", "-eqout=$result", '-NoCrashDialog')
if (-not $Window) { $gameArgs += '-Headless' }
$sw = [Diagnostics.Stopwatch]::StartNew()
$p = Start-Process (Join-Path $game 'RimWorld.exe') -ArgumentList ($gameArgs | ForEach-Object { '"' + $_ + '"' }) -WorkingDirectory $game -PassThru
if (-not $p.WaitForExit($TimeoutSec * 1000)) { $p.Kill(); "!! O jogo não fechou em $TimeoutSec s e foi encerrado." }
"== jogo encerrado em $([int]$sw.Elapsed.TotalSeconds)s (código $($p.ExitCode))"

# ---- relatório
if (Test-Path $result) { Get-Content $result -Encoding UTF8 } else { '!! Sem arquivo de resultado: o executor não chegou a iniciar (mod de debug não carregou?).' }
$log = Get-ChildItem $profileDir -Recurse -Filter Player.log -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $log) { $log = Get-Item (Join-Path $userData 'Logs\Player.log') -ErrorAction SilentlyContinue }
if ($log) {
    "== log: $($log.FullName)"
    $noise = 'Knowledge data was missing key|GAGARIN|\[Vulkan\]|NativeAllocation|hideFlags|Translation data for language|Could not find player faction|trailing whitespace|AsyncGPUReadback failed|Error in ParallelForEach|Bad string pass when reading rule'
    Get-Content $log.FullName -Encoding UTF8 | Where-Object { $_ -match '^\d{4}-\d\d-\d\d .* (ERROR|WARN) ' -and $_ -notmatch $noise } |
        ForEach-Object { $_.Substring(24).Substring(0, [Math]::Min(260, $_.Length - 24)) } |
        Group-Object | Sort-Object Count -Descending | Select-Object -First 25 | ForEach-Object { "{0,5} {1}" -f $_.Count, $_.Name }
}
