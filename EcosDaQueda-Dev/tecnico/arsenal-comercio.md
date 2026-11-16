# Arsenal, guarda-roupa e comércio por facção

## Tags de armas úteis

`NerveSpiker`, `Flamethrower` (Incinerator), `VWEB_BioferriteWeapon`, `LaserGun`, `Coilgun`, `SpacerGun`, `MakeshiftGun`, `BeamGraserGun`, `GrenadeTox`, `EltexStaff`, `IndustrialGunAdvanced`, `Gun`.

## Arsenal por facção (Arsenal.cs)

- `ArsenalDef`: `tag`, `weapons` (defNames; mod ausente é ignorado), `fromTags` (toda arma com aquela etiqueta), `apparel`, `apparelFromTags`. `EQArsenal.Apply()` (na inicialização) acrescenta a etiqueta a `ThingDef.weaponTags`/`apparel.tags` e chama `PawnWeaponGenerator.Reset()`/`PawnApparelGenerator.Reset()`.
- As unidades usam `<weaponTags Inherit="False">` só com etiquetas `EQ_<Facção>_<Nível>`. `weaponMoney` precisa cobrir o preço (com o CE os lasers custam 2000 a 4500): se o valor sorteado for menor que a arma mais barata, a unidade vem desarmada.
- Etiquetas úteis dos mods: `LaserGun`, `SalvagedLaserGun`, `Coilgun`, `FrontierCoilguns`, `FrontierGunSimple/Advanced`, `MakeshiftGun`, `VWEB_BioferriteWeapon`, `VPE_EltexMelee`, `Warcasket*` (armas e armaduras-caixão), `VFEM2_*`.
- **Combat Extended:** só recebem munição de reserva as unidades com `CombatExtended.LoadoutPropertiesExtension`. O CE põe em `MercenaryEliteTierBase`, `MercenaryGunnerBase` etc., mas **não** em `MercenaryMidTierBase` nem `MercenaryEliteBase`; quem herda dessas precisa da extensão no próprio def (`MayRequire="CETeam.CombatExtended"`).
- Unidade de combate deve ter `<requiredWorkTags><li>Violent</li></requiredWorkTags>`, senão ~20% nascem incapazes de violência e sem arma.
- Armadura-caixão do VFE Pirates numa unidade nossa: basta `apparelRequired` (corpo, ombros, capacete) + `apparelTags`/`weaponTags` `Warcasket*`; o def inteiro com `MayRequire="OskarPotocki.VFE.Pirates"`.

## Comércio por facção (Trade.cs)

- `TraderStockCopyExtension` (modExtension do TraderKindDef): `copyFrom` (os geradores do outro comerciante são clonados e postos na frente, depois dos patches de todos os mods), `dropWeapons` (tira os geradores que são principalmente de armas; nunca os `SingleDef`, os de compra nem os de etiqueta `CE_*`), `dropTradeTags`. `EQTrade.Dropped` guarda o que saiu (aparece no comando `trader`).
- `StockGenerator_EQTags`: `weaponTags`/`apparelTags` + `countRange`; vende armas e roupas do arsenal.
- Itens que não podem ser vendidos por comerciante (ex.: `Ambrosia`) dão erro se postos num `StockGenerator_SingleDef`.
- `Books` como categoria inclui `Tome`: excluir (o jogo usa `StockGenerator_Tomes`).
- Ao gerar estoque em teste, passar `makingFaction` (no jogo sempre há).
- Roupas: o gerador só cobre o corpo com o que as etiquetas permitem; toda unidade leva `EQ_Roupa_Comum` (calça, camisa, parka) além das da facção.

- Não Catalogados (bloco D): 4 arsenais `EQ_Arsenal_NaoCatalogados*` com armas do VFE Medieval 2 (`VFEM2_*`) e neolíticas do jogo. O VFE Medieval 2 não põe etiqueta nas armas de fogo (arcabuz, canhão de mão, mosquete): só entram por defName no arsenal.
- Serviço que cobra itens: `FactionServiceDef.costThings` (lista de `ThingDefCountClass`, aceita `MayRequire` no item); cobra só o que está na área doméstica, como a prata.
