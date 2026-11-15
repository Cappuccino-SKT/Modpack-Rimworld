# Mods que adicionam mecânicas

Para cada um: o que faz, a mecânica, os ganchos que dá para usar (tipos de Def próprios, extensões) e o que serve para o **Ecos da Queda**. Frameworks têm guia próprio em [`frameworks/`](frameworks/). Pasta = `Mods/<id>`; packageIds em [`../../modpack/mods.md`](../../modpack/mods.md).

Regra geral: tudo que usar defs destes mods vai com `MayRequire="<packageId>"` ou em pasta `IfModActive`.

## Facções e campanhas

### Vanilla Factions Expanded - Empire (`oskarpotocki.vfe.empire`, 2938820380)
Reformula o Império: **honrarias** (`VFEEmpire.HonorDef`) concedidas em cerimônia, **vassalagem** de assentamentos (dízimo por cápsula, `TitheTypeDef`), nobres visitantes, exposições de arte, discurso real, concessão de título, terrorismo (`TerrorismTypeDef`), aba "Realeza" (`RoyaltyTabDef`), Grande Hierarquia.
**Para o Ecos:** nosso C# já intercepta cerimônias de concessão (Leadership.cs) — conferir convivência com os rituais `VFEE_BestowTitle`/`VFEE_RoyalAddress`, que aparecem nas nossas ideologias. `HonorDef` serve de modelo para "medalhas" de potência; `TitheTypeDef` para dízimo da Forja/Coro. Tradução PT-BR nossa feita.

### Vanilla Factions Expanded - Deserters (`oskarpotocki.vfe.deserters`, 3025493377)
Campanha contra o Império: **visibilidade** (`VisibilityLevelDef`: quanto o Império sabe de você; aumenta ataques e preços), **contrabando** (`ContrabandCategoryDef`; itens imperiais têm valor no mercado negro), **serviços dos desertores** pagos com inteligência (`DeserterServiceDef`, aba `DeserterTabDef`), resposta imperial (`ImperialResponseDef`), complexos imperiais gerados com KCSG.
**Para o Ecos:** o sistema de "serviços pagos com moeda de facção" é o mais próximo do nosso `EQ_FactionServiceDef` — boa referência de UI e balanço. Visibilidade é um modelo para a nossa Infâmia. Registrar itens nossos como contrabando: o log avisa "registering item … with no contraband category" quando falta.

### Vanilla Factions Expanded - Pirates (`oskarpotocki.vfe.pirates`, 2723801948)
Piratas com **Warcaskets** (armaduras-tanque, `VFEPirates.WarcasketDef`, montadas em estação), **maldições** (`CurseDef`: o jogador escolhe uma maldição global em troca de bônus), Junkers e outras facções piratas, novos eventos e defesas.
**Para o Ecos:** Warcasket combina com a Forja (PawnKind elite com Warcasket). `CurseDef` é um bom modelo de "pacto" (Coro/Silêncio).

### Vanilla Factions Expanded - Insectoids 2 (`oskarpotocki.vfe.insectoid2`, 3309003431)
Insetoides como facção com **colmeias** que crescem, ondas (`InsectWaveDef`, `WaveActivityDef`), chefes invocáveis, linhagens (`InsectGenelineDef`), tambores sísmicos para atraí-los, mapa de colmeia (`InsectMapGenDef`). Patcheia a facção `Insect` (assentamentos).
**Para o Ecos:** Hegemony avisa no log "Initial insect hive is preserved…" — é ruído conhecido dessa combinação. A Hélice (genética) poderia ter interesse em linhagens insetoides.

### Vanilla Factions Expanded - Medieval 2 (`oskarpotocki.vfe.medieval2`, 3444347874)
Facções medievais (castelos, vikings, guilda mercante), cercos, tecnologia medieval que não fica obsoleta, heráldica (`HeraldicPattern`, `HeraldicSymbol`), muros de pedra modelados (`CobblestoneWallTemplateDef`), economia (vinho, mel).
**Para o Ecos:** heráldica gerada é inspiração para os brasões das potências. O CBF já traz assentamentos para as facções do VFE Medieval 2.

### Vanilla Quests Expanded - Ancients / Drone Factory / The Generator (`vanillaquestsexpanded.*`)
Cadeias de missões longas (via `VEF.Storyteller.QuestChainDef`, ver [vef.md](frameworks/vef.md) §6) com locais KCSG.
- **Ancients:** cofres subterrâneos, câmaras de mutação arquite (`ArchiteInjectionOutcomeDef`, `InjectionBlacklistDef`), genes novos.
- **Drone Factory:** drones despertando, ataque a armazéns, final contra fábrica pirata ("The Tinkerer").
- **Generator:** o gerador ARC que evolui (nuclear, solar, geotérmico).
**Para o Ecos:** são o modelo de "campanha por potência" (cadeia de missões com pré-requisitos, repetição e pesquisa exigida). A Ancients encaixa na Hélice; a Drone Factory na Forja.

### Rimesis (`font.rimesis`, 3767949538)
Transforma alguns saqueadores em **nêmesis recorrentes** que lembram o que aconteceu (quem os feriu, armas, fugas, capturas) e voltam mais fortes, com foco tático (cerco, flanco, mechs, veículos, Warcasket). "Crônica" com a história. Destruir o último assentamento de uma facção dispara **retaliação escalonada**.
**Para o Ecos:** a retaliação por destruir assentamento se sobrepõe às nossas retaliações de potência — testar as duas juntas. Nêmesis de uma potência combina com o tom (o "cobrador" da Forja que sempre volta).

## Sistemas da colônia

### Dubs Bad Hygiene (`dubwise.dubsbadhygiene`, 836308268)
Necessidades de **higiene e bexiga**, água encanada, esgoto, aquecimento central, irrigação, fertilizante, ar-condicionado, sede. Defs próprios: `PollutionDef`, `WashingJobDef`, extensões em `DubsBadHygiene.DefExtensions`. Referência: [`api/BadHygiene.md`](api/BadHygiene.md).
**Para o Ecos:** a "água contaminada" combinaria com o pós-apocalipse; pawns de facção visitante também têm essas necessidades.

### Vanilla Anomaly Expanded - Insanity (`vanillaexpanded.vanomalyeinsanity`, 3342412435)
Medidor de **sanidade**: estudar entidades drena, combatê-las recupera; sanidade baixa leva a loucura ou "desumanização" (que tem vantagens). Efeitos por faixa em `VAEInsanity.SanityEffectsDef`.
**Para o Ecos:** encaixe direto na **Ordem do Silêncio Perpétuo** (acadêmicos do Vazio). Serviços/rituais do Silêncio podem mexer em sanidade.

### Vanilla Recycling Expanded (`vanillaexpanded.recycling`, 3155781848)
Dá uso aos pacotes de resíduos tóxicos: estações de reciclagem (PipeSystem `ProcessDef`), bombas de poluição melhores, lixo → tijolos, conversão de terreno (`TerrainConversionsDef`).
**Para o Ecos:** economia de sucata/reciclagem — tema pós-apocalíptico natural para a Forja.

### Vanilla Cooking / Brewing / Nutrient Paste / Plants Expanded
- **Cooking** (`vanillaexpanded.vcooke`): refeições gourmet, condimentos, especialização de cozinheiro (`VSE.Expertise.ExpertiseDef`), queijo/defumação com PipeSystem. **Bakery**: forno e doces decorativos.
- **Brewing** (+ Coffees and Teas): bebidas com efeitos (hediffs) e pensamentos.
- **Nutrient Paste**: pasta nutritiva por cano (exemplo canônico do PipeSystem).
- **Plants** (+ More Plants, Mushrooms): plantas novas, terra arável (`TillableTerrainDef`), zonas de cultivo aquáticas/arenosas/cogumelos.
**Para o Ecos:** comida e bebida como produtos de facção (estoque de comerciante), e preceitos de bebida (VIE).

### SeedsPlease: Lite Redux (`evyatar108.seedspleaseliteredux`, 3523459853)
Plantas passam a exigir **sementes** (geradas automaticamente para mods). Colher dá sementes; comerciantes vendem.
**Para o Ecos:** plantas novas nossas ganham semente automática; conferir o nome gerado. Avisos "Some Seeds were autogenerated" e de Alpha Biomes no log são ruído conhecido.

### Smart Farming (`owlchemist.smartfarming`, 3220129183) — novo
Ferramentas para zonas de cultivo (prioridade de colheita, colher antes do frio, informação de rendimento). Sem defs; só C#.

### Harvest Organs Post Mortem Continued (`smuffle.harvestorganspostmortem`, 1204502413) — novo
Permite **retirar órgãos de cadáveres** (pesquisas e receitas próprias, filtros especiais por órgão), inclusive órgãos específicos do pawn.
**Para o Ecos:** encaixe com a **Hélice** (genética como produto) e com a moralidade cinza das potências; pensamentos negativos já vêm no mod.

### Expanded Prosthetics and Organ Engineering - Forked (`vat.epoeforked`, 1949064302)
Centenas de próteses e órgãos artificiais por nível tecnológico; opções extras via XML Extensions (ver [xml-extensions.md](frameworks/xml-extensions.md)).
**Para o Ecos:** implantes de facção (Forja/Hélice) devem seguir a escala de qualidade do EPOE.

### Vanilla Traits Expanded (`vanillaexpanded.vanillatraitsexpanded`)
53 traços novos com estados mentais próprios. **Para o Ecos:** traços de líder podem ganhar mapeamento no Hegemony (`FactionDispositionDef` com `trait`).

### Tech Advancing (`ghxx.techadvancing`, 735268789)
Sobe o **nível tecnológico** da colônia ao cumprir condições e permite mudar custos de pesquisa. **Para o Ecos:** afeta preço de pesquisa por nível; o CBF e os veículos usam o nível tecnológico da facção (não o da colônia).

## Genes, raças e xenótipos

### Alpha Genes (`sarg.alphagenes`, 2891845502)
250+ genes, 16 xenótipos, genes de **animal invocado** gerados por modelo (`AnimalGeneTemplateDef`), listas negras (`BlackListedAnimalsDef`, `BlackListedTraitsDef`, `GenePackBlacklistDef`, `ShamblerGeneBlacklistsDef`, `WretchBlacklistDef`), laboratórios abandonados em missões, ferramentas genéticas. Referência: [`api/AlphaGenes.md`](api/AlphaGenes.md).
**Para o Ecos:** já usamos xenótipos do Alpha Genes nas PawnKinds (10 `MayRequire`). Os modelos de gene geram genes automáticos — traduzidos pelo modelo (ver TECNICO › Tradução).

### Vanilla Races Expanded (12 mods: Pigskin, Hussar, Starjack, Genie, Waster, Fungoid, Highmate, Phytokin, Saurid, Lycanthrope, Sanguophage, Insector) e Android
Cada um traz xenótipo(s) com genes e mecânicas próprias: Hussar (aptidão por arma via `WeaponGeneTemplateDef`, `BlackListedWeaponsDef`), Lycanthrope (formas via `MorphGeneTemplateDef`), Sanguophage (habilidades de uso único `SingleUseAbilityDef`, linhagens), Highmate (efeitos de vínculo `HighmateBondEffectDef`), Insector (genes de linhagem `GenelineGeneDef`), Waster (toxicidade), Starjack (vácuo), Android (`AndroidGeneDef` hardware/sub-rotinas, `AndroidGeneTemplateDef`, neutroamina, estação de criação).
**Para o Ecos:** xenótipos já entram nas facções (Saurid, Lycan, Hussar, Fungoid, Sanguophage, Android). Androides combinam com a Forja ("produto com garantia").

### Blessed Conception Psychic Ritual (`fghj.ritualsboon`, 3790308888) — novo
Ritual psíquico do Anomaly (`PsychicRitualDef_HoraxsFavor`, papel `PsychicRitualRoleDef_SacredBirthMother`): custa 2 fragmentos de arcotec sombrio, recarga de 13 dias, gera um bebê "abençoado pelo Vazio" (traço com bônus, habilidade de creepjoiner, recusa da morte). Exige pesquisa de rituais psíquicos avançados.
**Para o Ecos:** exemplo de **ritual psíquico customizado em C#**; tema perfeito para o Silêncio ou o Culto de Horax já presente nos proxies.

## Psíquico e ideologia

- **Vanilla Psycasts Expanded** (+ Hemosage): ver [vpe.md](frameworks/vpe.md).
- **Vanilla Ideology Expanded - Memes and Structures** (`vanillaexpanded.vmemese`) e **Alpha Memes** (`sarg.alphamemes`): dezenas de memes/estruturas/preceitos; listas por def (`FireSourcesForPreceptDefs`, `SupportedDrinksForPreceptDefs`, `NaturalImplantCategoryDefs`, `UntameableInsectDefs`; `AlphaMemes.CoffeesDef`, `DryadsDef`, `RanchAnimalsDef`). Hegemony já mapeia os memes dos dois. **Usamos** vários memes VIE nas nossas ideologias (31 `MayRequire`).
- **Custom Ritual Framework**: ver [custom-ritual-framework.md](frameworks/custom-ritual-framework.md).

## Combate e mapa

- **Combat Extended** (+ Armors, Armory, Guns): ver [combat-extended.md](frameworks/combat-extended.md).
- **Simple Sidearms** (`petetimessix.simplesidearms`): pawns carregam armas secundárias e trocam conforme a situação; `SidearmWeaponTagMapDef` liga tags. **Para o Ecos:** PawnKinds podem ganhar arma secundária (com CE, via `LoadoutPropertiesExtension.sidearms`).
- **Layered Wall Destruction Fork** (`lordfelix.layeredwalldestructionfork`, 3832512386): paredes têm estágios de destruição (seteira, brecha, entulho) e podem ser restauradas; perfis em `LayeredDestruction.WallProfileDef`. **Para o Ecos:** paredes novas nossas precisam de perfil para ter estágios.
- **Trenches** (`oomby.trenches`): trincheiras (terra, madeira, concreto) e abrigos, no menu Segurança.
- **Geological Landforms** (`m00nl1ght.geologicallandforms`) e **Vanilla Landmarks Expanded** (`vanillaexpanded.vexploratione`): formas de relevo e 59 marcos com **mutadores de tile** (`TileMutatorDef`, `VEF.Maps.TileMutatorExtension`). **Para o Ecos:** marcos "pré-Queda" (ruínas, crateras) seriam `LandmarkDef` + mutadores.
- **Alpha Biomes** (`sarg.alphabiomes`): 12 biomas, estruturas por bioma (`BiomeStructuresDef`), spawns especiais (`SpecialSpawnsDef`).

## Mundo e viagem

- **Hegemony**: ver [hegemony.md](frameworks/hegemony.md) — camada de estratégia.
- **RimExodus — Seamless World** (`rimexodus.seamlessworld`): anda direto para o mapa vizinho sem caravana; combate entre mapas; comerciantes em assentamentos amigos sem formar caravana. **Atenção:** antes de desinstalar, rodar "restaurar compatibilidade vanilla" nas opções. Hoje **desativado** no modpack (bug a corrigir depois).
- **Vanilla Vehicles Expanded** (+ Upgrades): ver [vehicle-framework.md](frameworks/vehicle-framework.md).
- **Map Preview**, **World Map Enhanced**: só visuais (ver [conteudo.md](conteudo.md)).

## Trabalho e IA dos colonos

- **Pick Up And Haul**: colonos enchem o inventário antes de levar ao estoque.
- **Common Sense**: comportamentos mais sensatos (limpar antes de cozinhar, não comer cru…).
- **Smart Medicine - Continued**: médicos usam remédio do inventário e de qualquer lugar.
- **Smarter Construction**: evita colonos se trancarem em paredes.
- **Ugh You Got Me**: brigas sociais param quando alguém se fere feio.
- **Replace Stuff - Continued**: troca o material de construções sem demolir; plantas sobre rocha/névoa (`Replace_Stuff.InterchangeableItems`).
- **[FSF] Complex Jobs**: divide tipos de trabalho em mais colunas (configurável por XML Extensions). Carrega no fim da lista. **Para o Ecos:** WorkGivers novos nossos precisam de `workType` existente; checar com as colunas divididas.
- **[FSF] Filth Vanishes With Rain And Time**: sujeira some com chuva/tempo.
- **Allow Tool**: ferramentas de ordem em massa (permitir/proibir, colher tudo, transportar urgente).

## Desativados de propósito (não documentar uso)

- **Hearts of RimWorld - Grand Strategy Overhaul** e **Rimworld Exploration Mode**: desativados; a camada de estratégia é o Hegemony.
