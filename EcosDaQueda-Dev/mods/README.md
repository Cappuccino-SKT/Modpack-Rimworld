# Documentação dos mods do modpack (para desenvolver o Ecos da Queda)

Lista completa de mods e ordem: [`../../modpack/mods.md`](../../modpack/mods.md).

## Estrutura

- `frameworks/` — guias detalhados de frameworks e bibliotecas (como usar no nosso mod).
- `api/` — referência **gerada** de cada DLL de framework (Defs, DefModExtensions, Comps, PatchOperations, Workers, com campos). Regenerar: descompilar com `scripts/decompile.ps1 -Dll <dll> -Out %TEMP%\decomp\<Nome>` e rodar `python -X utf8 -I scripts/mods-api.py %TEMP%\decomp\<Nome> "<título>" mods/api/<Nome>.md`.
- Inventário bruto de todos os mods (descrição, DLLs, tipos de Def, patches): `python -X utf8 -I scripts/mods-inventario.py . saida.json` na raiz do jogo.

## Guias prontos

| Guia | Cobre |
|---|---|
| [frameworks/vef.md](frameworks/vef.md) | Vanilla Expanded Framework: genes, habilidades, armas, roupas, facções, cadeias de missões, memes, KCSG, PipeSystem, MVCF, Outposts |
| [frameworks/vpe.md](frameworks/vpe.md) | Vanilla Psycasts Expanded como framework de caminhos psíquicos |
| [frameworks/vehicle-framework.md](frameworks/vehicle-framework.md) | Vehicle Framework (veículos, torres, veículos em ataques de NPC) |
| [frameworks/xml-extensions.md](frameworks/xml-extensions.md) | XML Extensions (opções em XML, patches condicionais e laços) |
| [frameworks/harmony-hugslib.md](frameworks/harmony-hugslib.md) | Harmony (como o nosso C# patcha o jogo) e HugsLib |
| [frameworks/combat-extended.md](frameworks/combat-extended.md) | Combat Extended (patches de arma, munição, armadura) |
| [frameworks/hegemony.md](frameworks/hegemony.md) | Hegemony: disposições de facção, prédios de assentamento, API C#, eventos e vetos |
| [frameworks/custom-base-framework.md](frameworks/custom-base-framework.md) | Custom Base Framework: assentamentos de NPC (afeta as nossas potências) |
| [frameworks/custom-ritual-framework.md](frameworks/custom-ritual-framework.md) | Custom Ritual Framework: rituais e resultados anexáveis sem C# |
| [frameworks/adaptive-storage.md](frameworks/adaptive-storage.md) | Adaptive Storage Framework + PatchOperations extras (PostInheritance, Copy…) |
| [frameworks/bibliotecas-csharp.md](frameworks/bibliotecas-csharp.md) | Jaeger's Utility Framework e RimExodus Framework |
| [mecanicas.md](mecanicas.md) | Mods que adicionam mecânicas: o que fazem, ganchos e uso no Ecos |
| [conteudo.md](conteudo.md) | Mods de itens, visual e interface (descrição curta) |

## Quando atualizar

- Mod novo no modpack: rodar o inventário, decidir a categoria e acrescentar em `mecanicas.md` ou `conteudo.md`; se for framework, descompilar, gerar `api/` e escrever o guia.
- Update de framework: regenerar o `api/` dele e conferir os exemplos do guia.
