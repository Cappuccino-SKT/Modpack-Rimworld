# Vehicle Framework (Smash Phil)

- **packageId:** `SmashPhil.VehicleFramework` · pasta `Mods/3014915404` · DLLs `Vehicles.dll`, `SmashTools.dll`, `UpdateLogTool.dll`
- **Quem usa:** Vanilla Vehicles Expanded (`OskarPotocki.VanillaVehiclesExpanded`, 26 veículos) e VVE Upgrades.
- **Referência gerada:** [`../api/Vehicles.md`](../api/Vehicles.md) (145 classes), [`../api/SmashTools.md`](../api/SmashTools.md).
- Tradução PT-BR nossa já cobre o framework e o VVE (mod `ecosdaqueda.traducao`).

Um veículo é um **pawn** (`Vehicles.VehiclePawn`) com corpo próprio, componentes com vida e blindagem, papéis (motorista, artilheiro, passageiro), combustível, torres e, se for aéreo, decolagem pelo mapa-múndi. Para construir, o jogador ergue uma **planta** (`VehicleBuildDef`) que vira o veículo ao terminar.

## Peças de um veículo

| Def | Papel |
|---|---|
| `Vehicles.VehicleDef` (herda `ThingDef`) | O veículo em si. Pai abstrato usual: `VehiclePawn_Mechanical` (do framework) |
| `Vehicles.VehicleBuildDef` (herda `ThingDef`) | O "blueprint" construível; `thingToSpawn` aponta o `VehicleDef`. Pai: `VehicleBaseBuildable` |
| `Vehicles.VehicleTurretDef` | Torre (canhão, lança-chamas, foguetes) |
| `Vehicles.UpgradeTreeDef` | Árvore de melhorias (VVE Upgrades) |
| `Vehicles.PatternDef` / `SkinDef` | Padrões de pintura / visuais |
| `Vehicles.VehicleStatDef` | Atributos de veículo (`MoveSpeed`, `CargoCapacity`, `Mass`, `RepairRate`, `BodyIntegrity`…) |
| `Vehicles.VehicleRaidParamsDef` | Restrições para NPCs usarem o veículo em ataques |
| `Vehicles.VehicleEventDef` | Eventos (ligar, desligar, decolar) para sons/efeitos |

## VehicleDef — campos principais

- **Identidade:** `vehicleCategory` (flags: `Transport`, `Trader`, `Combat`, `Work`), `type`/`vehicleType` (`Land`, `Sea`, `Air`, `Universal`), `techLevel`, `buildDef`, `nameable`, `combatPower`, `enabled` (`Everyone`, `Player`, `Raiders`, `None`).
- **Gráfico:** `graphicData` com `graphicClass` `Vehicles.Graphic_Vehicle` e `shaderType` `CutoutComplexPattern` (aceita 3 cores: `color`, `colorTwo`, `colorThree` → pintura pelo jogador). `drawProperties` (deslocamentos por direção).
- **Stats:** `statBases` (armadura, inflamabilidade, valor) + `vehicleStats` (`Mass`, `MoveSpeed`, `CargoCapacity`, `RepairRate`, `AccelerationRate` do VVE).
- **`properties`:** `roles` (papéis: `key`, `label`, `slots`, `slotsToOperate`, `exposed`, `handlingTypes` = `Movement`/`Turret`, `turretIds`, `pawnRenderer` para desenhar o pawn), `worldSpeedMultiplier`, `winterSpeedMultiplier`, `offRoadMultiplier`, `customHillinessCosts`, `customWeatherCosts`, `customRoadCosts`, `visibilityWeight`, `manhunterTargetsVehicle`, `diagonalRotation`.
- **`components`:** partes com `key`, `label`, `health`, `depth` (`External`/`Internal`), `armor`, `categories` (stat que a peça afeta, ex. `MoveSpeed`), `tags`, `reactors` (ex.: `Vehicles.Reactor_Explosive` = tanque que explode), `hitbox`.
- **`comps`:** `Vehicles.CompProperties_FueledTravel` (`fuelType`, `fuelCapacity`, `fuelConsumptionRate`, `leakDef`, ou `electricPowered`), `Vehicles.CompProperties_VehicleTurrets` (`turrets`: id, `turretDef`, posição, papel), `Vehicles.CompProperties_VehicleLauncher` (aéreos: `launchProtocol`, `rateOfClimb`, `maxAltitude`, `controlInFlight`, `bombing`, `strafing`, `spaceFlight`).
- **NPC:** `npcProperties` (`runDownTargets`, `stopToShoot`, `targetAcquireRadius`, `raidParams`, `strategy`).
- **Sons:** `soundOneShotsOnEvent` e `soundSustainersOnEvent` por `VehicleEventDef` (`IgnitionOn`, `IgnitionOff`…).

Exemplo real resumido (VVE Dirtbike, `Mods/3014906877/1.6/Defs/VehicleDefs/Tier1/Dirtbike/`):

```xml
<Vehicles.VehicleDef ParentName="VehiclePawn_Mechanical">
  <defName>VVE_Dirtbike</defName>
  <label>Dirtbike</label>
  <graphicData>
    <texPath>Things/Vehicles/Tier1/Tier1_Motorbike/Tier1_Motorbike</texPath>
    <graphicClass>Vehicles.Graphic_Vehicle</graphicClass>
    <shaderType>CutoutComplexPattern</shaderType>
    <drawSize>(2,2)</drawSize>
  </graphicData>
  <buildDef>VVE_Dirtbike_Blueprint</buildDef>
  <size>(1,1)</size>
  <vehicleCategory>Transport</vehicleCategory>
  <techLevel>Industrial</techLevel>
  <vehicleType>Land</vehicleType>
  <vehicleStats><Mass>30</Mass><MoveSpeed>12</MoveSpeed><CargoCapacity>10</CargoCapacity></vehicleStats>
  <properties>
    <roles>
      <li><key>rider</key><label>rider</label><slots>1</slots><slotsToOperate>1</slotsToOperate>
          <exposed>true</exposed><handlingTypes><li>Movement</li></handlingTypes></li>
    </roles>
  </properties>
  <components>
    <li><key>Engine</key><label>Engine</label><health>25</health><depth>External</depth>
        <categories><li>MoveSpeed</li></categories></li>
  </components>
  <comps>
    <li Class="Vehicles.CompProperties_FueledTravel">
      <fuelType>Chemfuel</fuelType><fuelCapacity>80</fuelCapacity><fuelConsumptionRate>14.5</fuelConsumptionRate>
    </li>
  </comps>
</Vehicles.VehicleDef>

<Vehicles.VehicleBuildDef ParentName="VehicleBaseBuildable">
  <defName>VVE_Dirtbike_Blueprint</defName>
  <designationCategory>VF_Vehicles</designationCategory>
  <graphicData><texPath>Things/Vehicles/Tier1/Tier1_Motorbike/Tier1_Motorbike</texPath><graphicClass>Graphic_Multi</graphicClass></graphicData>
  <researchPrerequisites><li>VVE_BasicVehicles</li></researchPrerequisites>
  <costList><VVE_EngineBlock>1</VVE_EngineBlock><Steel>40</Steel></costList>
  <thingToSpawn>VVE_Dirtbike</thingToSpawn>
</Vehicles.VehicleBuildDef>
```

Texturas: `Graphic_Vehicle` procura `_north`, `_east`, `_south`, `_west` (e diagonais `_northEast` etc. se `diagonalRotation`), mais as máscaras `_m` para a pintura em 3 cores. Torres usam `Vehicles.Graphic_Turret`.

## Torres — `Vehicles.VehicleTurretDef`

`turretType` (`Rotatable`/`Static`), `projectile`, `magazineCapacity`, `chargePerAmmoCount`, `genericAmmo` (consome o combustível/aço como munição), `ammunition` (filtro), `reloadTimer`, `warmUpTimer`, `rotationSpeed`, `minRange`/`maxRange`, `fireModes` (`shotsPerBurst`, `ticksBetweenBursts`, `spreadRadius`, `label`, `texPath`), `graphicData`, `shotSound`, `attachProjectileFlag`, `restrictionType` (classe que restringe, ex.: não atirar em movimento). O veículo liga a torre no `CompProperties_VehicleTurrets` e um papel com `handlingTypes` `Turret`.

## Veículos em ataques de NPCs (importante para o Ecos)

Decompilado de `Vehicles.RaidInjectionHelper.ValidRaiderVehicle`: um veículo entra num ataque de facção se

1. `type` = `Land`; 2. `vehicleCategory` inclui `Combat`; 3. `combatPower` ≤ pontos do ataque; 4. `techLevel` da facção ≥ `techLevel` do veículo; 5. `enabled` inclui `Raiders`; 6. tem `npcProperties` (mesmo vazio); 7. se tiver `raidParams`, a facção e o modo de chegada estão na lista (`factions`, `arrivalModes`).

Ou seja: os veículos de combate do VVE com `npcProperties` vazio **podem aparecer com qualquer facção industrial**, inclusive as nossas potências. Para dar um veículo só a uma potência: criar um `VehicleRaidParamsDef` com `<factions><li>EQ_Faction_Forja</li></factions>` e apontar em `npcProperties.raidParams` (por patch no VVE, `MayRequire`).

## Integração com outras coisas

- **Ideologia:** VVE adiciona preceitos (dirigir/voar/navegar aprovado/obrigatório/horrível), os `HistoryEventDef` `VF_BoardLandVehicle` etc. já existem para reagir em preceitos nossos.
- **Caravanas:** caravanas de veículos têm regras próprias (combustível, terreno, bioma intransitável). Missões que mandam o jogador longe ficam mais fáceis com veículos aéreos.
- **Combat Extended:** CE tem `ModPatches/VehicleFramework` e `Vanilla Vehicles Expanded` (projéteis e armaduras CE).
- **Opções do mod:** cada veículo pode ser desligado ou ajustado pelo jogador (Opções › Vehicle Framework). Testar com valores padrão.

## Depuração

- Modo dev: opções de depuração do framework (Opções › Vehicle Framework › DevMode): desenhar hitbox, caminhos, "ligar qualquer veículo", decolagem instantânea.
- Os veículos são pawns: `Find.CurrentMap.mapPawns` os inclui; filtre por `is VehiclePawn`.
