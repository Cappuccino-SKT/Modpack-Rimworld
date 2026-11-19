# Compila os 3 projetos C# do Ecos da Queda (usado por run-test.ps1 -Build e regress.ps1 -Build).
# Um RimWorld aberto (o jogo do usuário ou o teste de outra sessão) trava as DLLs dos mods carregados (o Waylith
# mapeia o arquivo) e o "dotnet build" não consegue sobrescrever. Como o Windows deixa MOVER um arquivo mapeado,
# a DLL travada vai para test-profile\dll-trash e o jogo aberto continua usando a cópia antiga; o próximo jogo
# que abrir pega a nova. As cópias da lixeira que ninguém usa mais são apagadas aqui.
$ErrorActionPreference = 'Stop'
$dev = Split-Path $PSScriptRoot -Parent
$game = Split-Path $dev -Parent
$trash = Join-Path $dev 'test-profile\dll-trash'
New-Item -ItemType Directory -Force $trash | Out-Null
Get-ChildItem $trash -Filter *.dll -ErrorAction SilentlyContinue | ForEach-Object { try { Remove-Item $_.FullName -Force -ErrorAction Stop } catch {} }

function Test-Locked($path) {
    try { $s = [IO.File]::Open($path, 'Open', 'ReadWrite', 'None'); $s.Close(); return $false } catch { return $true }
}
$moved = 0
Get-ChildItem (Join-Path $game 'Mods') -Directory -Filter 'EcosDaQueda*' | ForEach-Object {
    Get-ChildItem $_.FullName -Recurse -Filter *.dll -ErrorAction SilentlyContinue | Where-Object { $_.Directory.Name -eq 'Assemblies' } | ForEach-Object {
        if (Test-Locked $_.FullName) {
            $dst = Join-Path $trash ($_.BaseName + '-' + [guid]::NewGuid().ToString('N') + '.dll')
            Move-Item $_.FullName $dst
            Copy-Item $dst $_.FullName   # se o build não mudar nada, a DLL continua no lugar
            $moved++
        }
    }
}
if ($moved) { "== $moved DLL(s) em uso por um RimWorld aberto: movidas para a lixeira (o jogo aberto segue com a antiga)" }

foreach ($proj in 'Mods\EcosDaQueda\Source\EcosDaQueda', 'Mods\EcosDaQueda\Source\EcosDaQueda.Hegemony', 'Mods\EcosDaQuedaDebug\Source') {
    $out = dotnet build (Join-Path $game $proj) -c Release 2>&1
    if ($LASTEXITCODE -ne 0) { $out | Select-String 'error' | Select-Object -First 15; throw "Falha ao compilar $proj" }
}
'build ok'
