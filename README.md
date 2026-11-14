# Modpack RimWorld — Ecos da Queda

Modpack para **RimWorld 1.7** (com todas as DLCs) com temática própria inspirada em Fallout, usando só a lore do RimWorld. O tom é satírico e todas as facções são moralmente cinzas.

## Organização do repositório

| Branch | Conteúdo |
|---|---|
| `main` | O modpack em si: lista de mods, ordem de carga, script de instalação e documentação de desenvolvimento |
| `mod/ecos-da-queda` | Mod **Ecos da Queda** (`ecosdaqueda.core`): as 4 potências, ideologias fixas, Fama/Infâmia, postos, serviços e integração com o Hegemony |
| `mod/ecos-da-queda-debug` | Mod **Ecos da Queda - Ferramentas de Debug** (`ecosdaqueda.debug`): executor de testes sem janela e ações no menu de debug |

Cada branch `mod/*` contém só a pasta do mod (a raiz da branch é a raiz do mod), pronta para ir em `Mods/<nome>`.

**Os arquivos do jogo e dos mods de terceiros não estão no repositório.** Eles pertencem aos seus autores (Ludeon e os autores da Workshop). O modpack guarda só a lista e a ordem, e os mods são baixados da Workshop pelo script de instalação.

| Caminho | O que é |
|---|---|
| [modpack/mods.md](modpack/mods.md) | Todos os mods, com ordem de carga, packageId e link da Workshop |
| [modpack/workshop-ids.txt](modpack/workshop-ids.txt) | IDs da Workshop usados pelo instalador |
| [modpack/ModsConfig.xml](modpack/ModsConfig.xml) | Lista de mods ativos e ordem de carga do jogo |
| [modpack/install.ps1](modpack/install.ps1) | Instalador |
| [EcosDaQueda-Dev/](EcosDaQueda-Dev/) | Documentação de desenvolvimento (design, status, técnico, testes) e scripts |
| [CLAUDE.md](CLAUDE.md) | Regras do projeto para o assistente |

## Instalação

1. Tenha o RimWorld 1.7 instalado e clone este repositório **dentro da pasta do jogo**:
   ```
   cd <pasta do RimWorld>
   git init
   git remote add origin https://github.com/Cappuccino-SKT/Modpack-Rimworld.git
   git fetch origin
   git checkout -f main
   ```
2. Rode o instalador. Ele baixa os mods da Workshop com o SteamCMD, coloca os mods próprios em `Mods/` e ativa a lista de carga:
   ```
   powershell -ExecutionPolicy Bypass -File modpack\install.ps1
   ```

## Desenvolvimento

Na pasta do jogo, `Mods/EcosDaQueda` e `Mods/EcosDaQuedaDebug` são **worktrees do git**: cada uma já está na sua branch. Um commit feito dentro da pasta do mod vai para a branch do mod, e um commit na raiz vai para a `main`. Ninguém precisa trocar de branch, e o jogo sempre enxerga os dois mods.

Para atualizar a lista de mods depois de adicionar ou remover algum, regenere `modpack/` e faça commit na `main`.
