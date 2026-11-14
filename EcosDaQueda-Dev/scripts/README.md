# Scripts

| Script | Uso |
|---|---|
| `check-log.ps1` | Resume ERROR/WARN do `Player.log` sem o ruído conhecido. `-Prev` lê o log anterior; `-Trace "texto"` mostra o stack trace da 1ª ocorrência. |
| `validate-xml.ps1` | Confere se todos os XML do mod são bem formados. |
| `decompile.ps1` | Descompila DLL do jogo/mod com ilspycmd (projeto inteiro ou um tipo com `-Type`). |
| `sortmods.ps1` | Gera ordem de carregamento respeitando loadAfter/loadBefore/dependências e grava no ModsConfig.xml. **Ativa todos os mods da pasta** — depois é preciso desativar de novo os que estão desligados (Hearts of RimWorld, Exploration Mode). Uso: `sortmods.ps1 -ModsDir ..\..\Mods -ConfigPath "$env:USERPROFILE\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Config\ModsConfig.xml"` |
| `steamcmd-mods.txt` | Cópia do script do SteamCMD com todos os mods (o original fica em `C:\Users\Felipe\steamcmd\mods.txt`). |

Todos são PowerShell: `powershell -ExecutionPolicy Bypass -File <script>`.
