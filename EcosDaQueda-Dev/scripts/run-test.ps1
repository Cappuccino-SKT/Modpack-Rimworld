# Roda um teste automático do Ecos da Queda, sem janela e sem tocar nos saves/config do jogador.
#
# Uso:
#   run-test.ps1 -Script ..\tests\smoke.txt               (todos os mods ativos do jogador; "full" é o único perfil)
#   run-test.ps1 -Script ..\tests\x.txt -Save Test          (copia o save "Test" do jogador para o perfil de teste)
#   run-test.ps1 -Script ..\tests\x.txt -Window             (mostra a janela do jogo, para depurar)
#   run-test.ps1 -Build ...                                 (compila os 3 projetos C# antes)
#   run-test.ps1 ... -Full | -Show 'regex'                  (relatório inteiro | resumo + linhas que casam)
#   run-test.ps1 -Batch a.txt,b.txt -Profile full           (lote: vários roteiros numa só abertura do jogo)
#   run-test.ps1 ... -Slot 2                                (perfil isolado próprio, para rodar 2 jogos ao mesmo tempo)
#
# O jogo roda com -savedatafolder numa pasta isolada (EcosDaQueda-Dev\test-profile\<perfil>),
# executa os comandos do script (ver TESTES.md), grava <script>.result.txt em tests\out e fecha.
param(
    [string]$Script,
    [string[]]$Batch = @(),     # lote: vários roteiros numa só abertura do jogo (relatórios iguais aos do modo normal)
    [int]$Slot = 0,             # > 0: perfil test-profile\<perfil>-<slot> (permite jogos em paralelo)
    [ValidateSet('full')][string]$Profile = 'full',   # o perfil "lean" foi aposentado em 2026-11-18 (quebrava com as ideologias)
    [string]$Save,
    [switch]$Window,
    [switch]$Build,
    [switch]$Full,              # imprime o relatório inteiro (o padrão é só o resumo: erros, PROBLEMAS e RESULTADO)
    [string]$Show,              # regex: no resumo, mostra também as linhas do relatório que casarem
    [string[]]$Exclude = @(),   # packageIds a desativar no perfil de teste (ex.: ecosdaqueda.traducao ao exportar textos)
    [int]$TimeoutSec = 1200,    # normal: limite do jogo inteiro; lote: limite de CADA roteiro (mundo.txt ganha 2400)
    [int]$StallSec = 0          # lote: sem nenhuma linha nova no relatório por N s = travado (0 = desligado)
)
$ErrorActionPreference = 'Stop'
$dev = Split-Path $PSScriptRoot -Parent
$game = Split-Path $dev -Parent
$userData = Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios'
if ($Batch.Count -eq 0 -and -not $Script) { throw 'Informe -Script ou -Batch.' }
if ($Script) { $Script = (Resolve-Path $Script).Path }
$Batch = @($Batch | ForEach-Object { $_ -split "," } | Where-Object { $_ } | ForEach-Object { (Resolve-Path $_.Trim()).Path })


if ($Build) { & (Join-Path $PSScriptRoot 'build-mods.ps1') }

# ---- perfil isolado
$profileDir = Join-Path $dev "test-profile\$Profile$(if ($Slot -gt 0) { "-$Slot" })"
New-Item -ItemType Directory -Force (Join-Path $profileDir 'Config'), (Join-Path $profileDir 'Saves') | Out-Null
# Só bloqueia se outro TESTE usa este mesmo perfil. O jogo normal do usuário (perfil dele, ex.: montando roupas no
# Character Creator) pode ficar aberto: os dois não dividem saves nem config, só o Player.log (o Waylith ignora -logFile).
$running = @(Get-CimInstance Win32_Process -Filter "Name='RimWorld.exe'" -ErrorAction SilentlyContinue)
if ($running | Where-Object { $_.CommandLine -and $_.CommandLine.Contains("-savedatafolder=$profileDir`"") }) { throw "Já há um teste rodando no perfil $profileDir. Espere ele acabar ou use -Slot n." }
$userGameOpen = [bool]($running | Where-Object { -not $_.CommandLine -or $_.CommandLine -notmatch 'test-profile' })
if ($userGameOpen) { '== RimWorld do usuário aberto: o teste roda ao lado (resumo do Player.log pode trazer linhas do jogo dele)' }
foreach ($f in 'Prefs.xml', 'KeyPrefs.xml', 'Knowledge.xml') {
    $src = Join-Path $userData "Config\$f"
    if (Test-Path $src) { Copy-Item $src (Join-Path $profileDir 'Config') -Force }
}
[xml]$cfg = Get-Content (Join-Path $userData 'Config\ModsConfig.xml') -Raw -Encoding UTF8
$active = $cfg.ModsConfigData.activeMods
@($active.SelectNodes('li') | Where-Object { $_.InnerText -eq 'ecosdaqueda.debug' -or $Exclude -contains $_.InnerText }) | ForEach-Object { [void]$active.RemoveChild($_) }
$li = $cfg.CreateElement('li'); $li.InnerText = 'ecosdaqueda.debug'; [void]$active.AppendChild($li)
$cfg.Save((Join-Path $profileDir 'Config\ModsConfig.xml'))

if ($Save) {
    $src = Join-Path $userData "Saves\$Save.rws"
    if (-not (Test-Path $src)) { throw "Save não encontrado: $src" }
    Copy-Item $src (Join-Path $profileDir 'Saves') -Force
}

# Cache de XML do Missile Girl (GAGARIN): se foi montado com uma DLL nossa mais velha, um def cujo tipo C# ainda não
# existia fica de fora e o cache não percebe (o XML não mudou). Apaga o cache quando alguma DLL nossa é mais nova.
$cache = Join-Path $profileDir 'MissileGirl\Cache'
$unified = Join-Path $cache 'Unified.xml'
if (Test-Path $unified) {
    $dlls = Get-ChildItem (Join-Path $game 'Mods\EcosDaQueda*') -Recurse -Filter '*.dll' -ErrorAction SilentlyContinue | Where-Object { $_.FullName -like '*\Assemblies\*' }
    if ($dlls | Where-Object { $_.LastWriteTime -gt (Get-Item $unified).LastWriteTime }) { Remove-Item $cache -Recurse -Force; '== cache de XML (Missile Girl) apagado: DLL nova' }
}

# ---- executa
$outDir = Join-Path $dev 'tests\out'
New-Item -ItemType Directory -Force $outDir | Out-Null
$exe = Join-Path $game 'RimWorld.exe'
# Pede o log no perfil de teste; o motor Waylith ignora -logFile e grava no LocalLow (então jogos em paralelo dividem o arquivo).
$gameLog = Join-Path $profileDir 'Player.log'
$sw = [Diagnostics.Stopwatch]::StartNew()
function Get-BatchResult($b) { Join-Path $outDir ((Split-Path $b -Leaf) + ".$Profile.result.txt") }
function Save-GameLog($tag) {
    # Guarda o Player.log de cada abertura (o próximo jogo sobrescreve o arquivo).
    $l = if (Test-Path $gameLog) { $gameLog } else { Join-Path $userData 'Logs\Player.log' }
    if (Test-Path $l) { Copy-Item $l (Join-Path $outDir "lote-$Profile$(if ($Slot) { "-$Slot" })-$tag.log") -Force }
}
if ($Batch.Count -gt 0) {
    # Lote: lista "roteiro|relatório"; o relatório de cada roteiro sai no mesmo lugar do modo normal.
    # Vigia externo: se um roteiro passa do limite (o jogo pode congelar sem rodar quadros, e aí o executor
    # interno não vê o próprio timeout), o jogo é morto, o relatório ganha TIMEOUT e o lote reabre no seguinte.
    foreach ($b in $Batch) { Remove-Item (Get-BatchResult $b) -ErrorAction SilentlyContinue }
    $batchFile = Join-Path $profileDir 'batch.txt'
    $next = 0; $round = 0
    while ($next -lt $Batch.Count) {
        $round++
        $rest = $Batch[$next..($Batch.Count - 1)]
        [IO.File]::WriteAllLines($batchFile, [string[]]@($rest | ForEach-Object { "$_|$(Get-BatchResult $_)" }))
        $gameArgs = @("-savedatafolder=$profileDir", "-eqbatch=$batchFile", '-NoCrashDialog', '-logFile', $gameLog)
        if (-not $Window) { $gameArgs += '-Headless' }
        $p = Start-Process $exe -ArgumentList ($gameArgs | ForEach-Object { '"' + $_ + '"' }) -WorkingDirectory $game -PassThru
        $cur = -1; $curStart = $null; $lastLen = -1; $lastChange = Get-Date; $killed = $false; $doneAt = $null
        while (-not $p.WaitForExit(5000)) {
            # Roteiro atual = último do lote cujo relatório já existe.
            for ($i = $Batch.Count - 1; $i -ge $next; $i--) { if (Test-Path (Get-BatchResult $Batch[$i])) { break } }
            if ($i -lt $next) { $i = $next }
            if ($i -ne $cur) { $cur = $i; $curStart = Get-Date; $lastLen = -1; $doneAt = $null }
            $r = Get-BatchResult $Batch[$cur]
            $len = if (Test-Path $r) { (Get-Item $r).Length } else { 0 }
            if ($len -ne $lastLen) { $lastLen = $len; $lastChange = Get-Date }
            $lim = if ((Split-Path $Batch[$cur] -Leaf) -eq 'mundo.txt') { [Math]::Max($TimeoutSec, 2400) } else { $TimeoutSec }
            $why = $null
            $done = (Test-Path $r) -and (Select-String -Path $r -Pattern '^# RESULTADO' -Quiet)
            # Conta do RESULTADO (não da última linha): o vigia interno do executor grava "# vigia" a cada 30 s.
            if ($done -and -not $doneAt) { $doneAt = Get-Date }
            if ($done -and ((Get-Date) - $doneAt).TotalSeconds -gt 90) { $why = 'FECHOU' }
            elseif (((Get-Date) - $curStart).TotalSeconds -gt $lim + 120) { $why = "passou de $lim s" }
            elseif ($StallSec -gt 0 -and ((Get-Date) - $lastChange).TotalSeconds -gt $StallSec) { $why = "relatório parado há $StallSec s" }
            if ($why) {
                try { $p.Kill() } catch {}
                $killed = $true
                if ($why -eq 'FECHOU') {
                    # Roteiro completo; travou só a volta ao menu (ou o fechamento do jogo). Não é falha do teste.
                    Add-Content $r -Encoding UTF8 -Value '# (a volta ao menu travou; o run-test encerrou o jogo e seguiu)'
                    "-- $(Split-Path $Batch[$cur] -Leaf): volta ao menu travou; jogo encerrado, segue no próximo"
                    break
                }
                Add-Content $r -Encoding UTF8 -Value @("!! TIMEOUT EXTERNO: $why; o jogo foi encerrado pelo run-test (log em lote-$Profile$(if ($Slot) { "-$Slot" })-$round.log)", '# RESULTADO: TRAVOU (timeout externo)')
                "!! $(Split-Path $Batch[$cur] -Leaf): $why; jogo encerrado"
                break
            }
        }
        Save-GameLog $round
        # Continua depois do último roteiro que tem relatório; um relatório sem RESULTADO (o jogo fechou no meio) é marcado.
        $last = -1
        for ($i = $next; $i -lt $Batch.Count; $i++) { if (Test-Path (Get-BatchResult $Batch[$i])) { $last = $i } }
        if ($last -lt 0) { "!! O lote não iniciou nenhum roteiro (mod de debug não carregou?)"; break }
        $r = Get-BatchResult $Batch[$last]
        if (-not $killed -and -not (Select-String -Path $r -Pattern '^# RESULTADO' -Quiet)) {
            Add-Content $r -Encoding UTF8 -Value @("!! O jogo fechou no meio deste roteiro (código $($p.ExitCode); log em lote-$Profile$(if ($Slot) { "-$Slot" })-$round.log)", '# RESULTADO: TRAVOU (o jogo fechou)')
        }
        $next = $last + 1
    }
    "== jogo(s) encerrado(s) em $([int]$sw.Elapsed.TotalSeconds)s ($round abertura(s))"
} else {
    $result = Join-Path $outDir ((Split-Path $Script -Leaf) + ".$Profile.result.txt")
    Remove-Item $result -ErrorAction SilentlyContinue
    $gameArgs = @("-savedatafolder=$profileDir", "-eqtest=$Script", "-eqout=$result", '-NoCrashDialog', '-logFile', $gameLog)
    if (-not $Window) { $gameArgs += '-Headless' }
    $p = Start-Process $exe -ArgumentList ($gameArgs | ForEach-Object { '"' + $_ + '"' }) -WorkingDirectory $game -PassThru
    if (-not $p.WaitForExit($TimeoutSec * 1000)) { $p.Kill(); "!! O jogo não fechou em $TimeoutSec s e foi encerrado." }
    "== jogo encerrado em $([int]$sw.Elapsed.TotalSeconds)s (código $($p.ExitCode))"
}

# ---- relatório
if ($Batch.Count -gt 0) { "== lote de $($Batch.Count) roteiro(s); relatórios em $outDir" }
elseif (Test-Path $result) {
    $lines = @(Get-Content $result -Encoding UTF8)
    if ($Full) { $lines }
    else {
        $pat = '^!!|PROBLEMAS|^# RESULTADO'
        if ($Show) { $pat += '|' + $Show }
        $hit = @($lines | Where-Object { $_ -match $pat })
        $hit | Select-Object -First 40 | ForEach-Object { if ($_.Length -gt 300) { $_.Substring(0, 300) + ' ...' } else { $_ } }
        if ($hit.Count -gt 40) { "... (+$($hit.Count - 40) linhas)" }
        "== relatório completo ($($lines.Count) linhas): $result"
    }
} else { '!! Sem arquivo de resultado: o executor não chegou a iniciar (mod de debug não carregou?).' }
$log = Get-ChildItem $profileDir -Recurse -Filter Player.log -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $log) { $log = Get-Item (Join-Path $userData 'Logs\Player.log') -ErrorAction SilentlyContinue }
if ($log) {
    "== log: $($log.FullName)$(if ($userGameOpen) { ' (dividido com o jogo do usuário)' })"
    $noise = 'Knowledge data was missing key|GAGARIN|\[Vulkan\]|NativeAllocation|hideFlags|Translation data for language|Could not find player faction|trailing whitespace|AsyncGPUReadback failed|Error in ParallelForEach|Bad string pass when reading rule'
    Get-Content $log.FullName -Encoding UTF8 | Where-Object { $_ -match '^\d{4}-\d\d-\d\d .* (ERROR|WARN) ' -and $_ -notmatch $noise } |
        ForEach-Object { $_.Substring(24).Substring(0, [Math]::Min(260, $_.Length - 24)) } |
        Group-Object | Sort-Object Count -Descending | Select-Object -First $(if ($Full) { 25 } else { 8 }) | ForEach-Object { "{0,5} {1}" -f $_.Count, $_.Name.Substring(0, [Math]::Min($(if ($Full) { 260 } else { 140 }), $_.Name.Length)) }
}
