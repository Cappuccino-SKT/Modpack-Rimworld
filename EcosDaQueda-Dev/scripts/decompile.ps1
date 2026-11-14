# Descompila uma DLL (do jogo ou de um mod) para leitura, usando ilspycmd.
# Uso: powershell -File decompile.ps1 -Dll <caminho.dll> [-Out <pasta>] [-Type <Namespace.Tipo>]
# Exemplos:
#   decompile.ps1 -Dll ..\..\Assembly-CSharp.dll -Out $env:TEMP\decomp\game
#   decompile.ps1 -Dll ..\..\Mods\3825320105\Assemblies\FactionTerritories2.dll -Out $env:TEMP\decomp\hegemony
#   decompile.ps1 -Dll ..\..\Mods\3825320105\Assemblies\FactionTerritories2.dll -Type FactionTerritories2.HegemonyApi
# Instalar a ferramenta (uma vez): dotnet tool install -g ilspycmd
param([Parameter(Mandatory)][string]$Dll, [string]$Out, [string]$Type)
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"
if ($Type) { ilspycmd -t $Type $Dll; return }
if (-not $Out) { $Out = Join-Path $env:TEMP ("decomp\" + [IO.Path]::GetFileNameWithoutExtension($Dll)) }
New-Item -ItemType Directory -Force $Out | Out-Null
ilspycmd -p -o $Out $Dll
"Saida: $Out ($((Get-ChildItem $Out -Recurse -Filter *.cs).Count) arquivos .cs)"
