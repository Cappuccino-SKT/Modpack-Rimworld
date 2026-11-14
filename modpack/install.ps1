# Instala o modpack numa cópia do RimWorld 1.7 (não-Steam).
#
# Uso (na pasta raiz do jogo, com este repositório clonado nela):
#   powershell -ExecutionPolicy Bypass -File modpack\install.ps1 [-SteamCmd C:\steamcmd] [-SkipWorkshop] [-SkipOwnMods] [-SkipConfig]
#
# 1. Baixa os mods da Workshop listados em workshop-ids.txt com o SteamCMD (login anônimo) e copia para Mods\<id>.
# 2. Coloca os mods próprios (branches mod/*) em Mods\<nome> como worktrees do git.
# 3. Copia ModsConfig.xml (lista e ordem de carga) para a pasta de config do jogo (faz backup do atual).
param(
    [string]$SteamCmd = 'C:\steamcmd',
    [switch]$SkipWorkshop,
    [switch]$SkipOwnMods,
    [switch]$SkipConfig
)
$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$game = Split-Path $here -Parent
$mods = Join-Path $game 'Mods'
New-Item -ItemType Directory -Force $mods | Out-Null

if (-not $SkipWorkshop) {
    $exe = Join-Path $SteamCmd 'steamcmd.exe'
    if (-not (Test-Path $exe)) {
        "Baixando SteamCMD em $SteamCmd ..."
        New-Item -ItemType Directory -Force $SteamCmd | Out-Null
        $zip = Join-Path $SteamCmd 'steamcmd.zip'
        Invoke-WebRequest 'https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip' -OutFile $zip
        Expand-Archive $zip $SteamCmd -Force
        Remove-Item $zip
    }
    $ids = Get-Content (Join-Path $here 'workshop-ids.txt') | ForEach-Object { ($_ -split '\s')[0] } | Where-Object { $_ -match '^\d+$' }
    $scriptFile = Join-Path $SteamCmd 'modpack-rimworld.txt'
    $lines = @('@ShutdownOnFailedCommand 0', '@NoPromptForPassword 1', "force_install_dir $SteamCmd", 'login anonymous')
    $lines += $ids | ForEach-Object { "workshop_download_item 294100 $_ validate" }
    $lines += 'quit'
    Set-Content $scriptFile $lines -Encoding ASCII
    "Baixando $($ids.Count) mods da Workshop (pode levar bastante tempo) ..."
    & $exe +runscript $scriptFile
    $content = Join-Path $SteamCmd 'steamapps\workshop\content\294100'
    $missing = @()
    foreach ($id in $ids) {
        $src = Join-Path $content $id
        if (Test-Path $src) { Copy-Item $src (Join-Path $mods $id) -Recurse -Force } else { $missing += $id }
    }
    if ($missing) { "!! Não baixados: $($missing -join ', ') (rode de novo ou baixe manualmente)" } else { 'Mods da Workshop ok.' }
}

if (-not $SkipOwnMods) {
    Push-Location $game
    try {
        git fetch origin --quiet
        foreach ($m in @(@{ dir = 'EcosDaQueda'; branch = 'mod/ecos-da-queda' }, @{ dir = 'EcosDaQuedaDebug'; branch = 'mod/ecos-da-queda-debug' })) {
            $dst = Join-Path $mods $m.dir
            if (Test-Path (Join-Path $dst '.git')) { "$($m.dir): já instalado"; continue }
            if (Test-Path $dst) { throw "Mods\$($m.dir) já existe e não é um worktree do git. Mova a pasta e rode de novo." }
            if (-not (git branch --list $m.branch)) { git branch --quiet $m.branch "origin/$($m.branch)" }
            git worktree add --quiet $dst $m.branch
            "$($m.dir): ok ($($m.branch))"
        }
    } finally { Pop-Location }
}

if (-not $SkipConfig) {
    $cfgDir = Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Config'
    New-Item -ItemType Directory -Force $cfgDir | Out-Null
    $dst = Join-Path $cfgDir 'ModsConfig.xml'
    if (Test-Path $dst) { Copy-Item $dst "$dst.bak" -Force; 'Backup: ModsConfig.xml.bak' }
    Copy-Item (Join-Path $here 'ModsConfig.xml') $dst -Force
    'Lista de mods ativos copiada para o jogo.'
}
