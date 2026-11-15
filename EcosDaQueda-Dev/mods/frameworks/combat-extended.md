# Combat Extended (CE)

- **packageId:** `CETeam.CombatExtended` · pasta `Mods/2890901044` · DLL principal `CombatExtended.dll` (+ compat: VEFCompat, VehiclesCompat…)
- **Add-ons no modpack:** CE Armors, CE Armory, CE Guns.
- **Referência gerada:** [`../api/CombatExtended.md`](../api/CombatExtended.md) (241 classes).

CE troca o combate vanilla: balística real (munição, carregador, recarga), armadura em mm/MPa, volume (`Bulk`) no inventário, modos de tiro e de mira. **Toda arma/armadura/pawn nova precisa de patch CE**, senão fica desbalanceada ou sem munição.

## Cobertura no modpack

O CE já traz patches (`ModPatches/<mod>`, carregados por `IfModActive`) para 38 mods nossos: Alpha Biomes/Genes/Memes, EPOE Forked, VAE Waste Animals, VBE, VEF, VFE Deserters/Empire/Insectoids 2/Medieval 2/Pirates, VIE Memes, VPE (+ Hemosage), VPlants Mushrooms, VQE Ancients/Drone Factory/Generator, todos os VRE, VVE (+ Upgrades), VWE Bioferrite/Coilguns/Frontier/Laser/Makeshift e o Vehicle Framework. **Não há patch CE para o Ecos da Queda** — qualquer arma/apparel nosso precisa do próprio.

As nossas PawnKinds herdam de bases vanilla (`MercenaryEliteBase`, `MercenaryMidTierBase`…) que o CE já patcheia com `CombatExtended.LoadoutPropertiesExtension` (munição no inventário). Como patches rodam antes da herança, nossos filhos ganham munição sem nada extra. PawnKind sem esse pai precisa da extensão.

## Arma de fogo — `CombatExtended.PatchOperationMakeGunCECompatible`

Converte um `ThingDef` de arma num só passo. Campos: `defName`, `statBases` (`Mass`, `Bulk`, `RangedWeapon_Cooldown`, `SightsEfficiency`, `ShotSpread`, `SwayFactor`), `Properties` (o verbo `VerbPropertiesCE`), `AmmoUser` (`magazineSize`, `reloadTime`, `ammoSet`), `FireModes` (`aiUseBurstMode`, `aiAimMode`, `aimedBurstShotCount`, `noSingleShot`, `noSnapshot`), `weaponTags` (ex.: `CE_AI_Rifle`, `CE_OneHandedWeapon`), `costList`, `researchPrerequisite`, `texPath`, `isWeaponPlatform`.

```xml
<Operation Class="CombatExtended.PatchOperationMakeGunCECompatible">
  <defName>EQ_Gun_Exemplo</defName>
  <statBases><Mass>3.2</Mass><Bulk>8</Bulk><SightsEfficiency>1</SightsEfficiency>
             <ShotSpread>0.1</ShotSpread><SwayFactor>1.2</SwayFactor><RangedWeapon_Cooldown>0.4</RangedWeapon_Cooldown></statBases>
  <Properties>
    <verbClass>CombatExtended.Verb_ShootCE</verbClass>
    <hasStandardCommand>true</hasStandardCommand>
    <defaultProjectile>Bullet_556x45mmNATO_FMJ</defaultProjectile>
    <warmupTime>1.1</warmupTime><range>55</range><burstShotCount>5</burstShotCount>
    <soundCast>Shot_AssaultRifle</soundCast>
  </Properties>
  <AmmoUser><magazineSize>30</magazineSize><reloadTime>4</reloadTime><ammoSet>AmmoSet_556x45mmNATO</ammoSet></AmmoUser>
  <FireModes><aiAimMode>AimedShot</aiAimMode></FireModes>
  <weaponTags><li>CE_AI_AR</li></weaponTags>
</Operation>
```

Envolver tudo em `PatchOperationFindMod` (`<mods><li>Combat Extended</li></mods>`) ou pôr numa pasta `IfModActive="CETeam.CombatExtended"` no nosso `LoadFolders.xml`. Os patches reais de referência: `Mods/2890901044/ModPatches/Vanilla Weapons Expanded - Makeshift/Patches/.../RangedIndustrial.xml`.

## Corpo a corpo e coronhada

Trocar `tools` por `<li Class="CombatExtended.ToolCE">` com `armorPenetrationSharp`/`armorPenetrationBlunt` e `linkedBodyPartsGroup` (`Stock`, `Barrel`, `Muzzle`, `Grip`…).

## Munição

- `CombatExtended.AmmoSetDef`: `ammoTypes` (lista `AmmoLink`: munição → projétil), `similarTo`, `ammoConsumedPerShot`.
- `CombatExtended.AmmoDef` (ThingDef): `ammoClass` (`AmmoCategoryDef`: FMJ, AP, HP…), `cookOffProjectile`, `ammoTags`.
- Projétil: `projectile Class="CombatExtended.ProjectilePropertiesCE"` com `armorPenetrationSharp`, `armorPenetrationBlunt`, `pelletCount`, `secondaryDamage`, `speedGain`, `damageFalloff`.
- Prefira reaproveitar os calibres existentes (936 `AmmoSetDef`); só crie munição para algo realmente novo.

## Armadura/roupa

Stats CE: `Bulk`, `WornBulk`, `Mass`, `ArmorRating_Sharp` (mm RHA), `ArmorRating_Blunt` (MPa), `StuffEffectMultiplierArmor`. Proteção parcial por parte do corpo: `CombatExtended.PartialArmorExt` (`stats`).

## Outros

- `CombatExtended.PatchOperationSettingsConditional`: patch conforme opção do CE.
- Ordem de carga: CE fica perto do fim (posição 88) e os add-ons logo depois; nossos patches CE devem carregar depois do CE.
- Teste: o executor de testes com `-Profile full` carrega o CE; erros de patch aparecem como erro no log.
