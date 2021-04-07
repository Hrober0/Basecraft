public enum Obj
{
    None=0, Locked=1,

    BuildingUnderConstruction=10, BuildingUnderDemolition=11,

    //StartOfBuildings
    StartOfBuildings = 20,

    Junkyard=21, Woodcuter=22, Quarry=23,
    Planter=24, Smelter=25, Pulverizer=26, Pump=27, Farm=28,
    Warehouse1=40, Warehouse2=41,
    BasicCrafter=60, Crafter=61,
    ChemicalPlant=70,
    DroneStation=80,
    Connector=90, FastConnector=91,

    Ballista=100, GunTurret=101, LaserTurret=102, RocketTurret=103,

    Launchpad=150, 
    BasicRequester=160, SpaceRequester=161,

    TransmissionTower =200, Battery=201, CombustionGenerator=210, SteamGenerator=211,  SolarPanel1=215,
    ElectricSmelter=250, Repairer=251,

    Wall0=300, Wall1=301, Wall2=302, Wall3=303,
    ShieldGenerator=500,

    EndOfBuildings=1000,
    //EndOfBuildings

    ConUnderConstruction = 1001, ConUnderDemolition=1002,
    Connection1=1003, Connection2=1004, Connection3=1005,

    //StartOfterrain
    StartOfTerrain=2000,
    StoneOre=2003, CopperOre=2004, IronOre=2005, CoalOre = 2006, SandOre = 2007,
    TerrainFertile = 2019, Farmland =2020, FarmlandGrape=2021, FarmlandFlax=2022, FarmlandRubber=2023, WaterSource=2040, OilSource=2041,
    Tree=2101, Sapling=2102,
    Mountain=2500,
    EndOfTerrain=3000,
    //EndOfTerrain

    //StartOfEnemy
    StartOfEnemy=4000,
    EnemyCore=4001, EnemySpawner=4002, EnemyTurret=4003, EnemyPlatform=4004, EnemyWall=4005, EnemysTerrain=4006,
    EndOfEnemy=5000,
    //EndOfEnemy
}

public enum Res
{
    None=0,
    Sapling, Wood, Planks,
    StoneOre, StoneBrick,
    CopperOreCtm, CopperOre, CopperDust, CopperPlate, CopperCable, WoodenCircuit,
    IronOre, IronDust, IronPlate, IronGear, IronRod, Steel,
    Coal, GraphiteRod, ElectricEngine, Battery,
    Grape, Flax, RubberPlant, Rubber, Biomass,
    Bag, BagSand, Sand, Glass, Silicon,
    BottleEmpty, BottleWater, BottleEthanol, BottleOil, Hydrogen, Oxygen, Sulfur, SulfuricAcid,
    Plastic, PlasticCircuit, Gunpowder, Dynamite,
    Quarrel, Quarrel2, GunMagazine, GunMagazine2, Rocket,
    InsulatedCable, AdvancedCircuit,
    Drone,
}
public enum TagsEnum { Different, Connection, Platform, ConBorder, ObjectPlan, Dron, EnemyBase, EnemyUnit, Wall, Electricity, TerrainObj }
public enum GameState { MainMenu=1, Sandbox=2, Level=3, Colony=4, SpaceStation=5 }
public enum Difficulty { Peaceful=2, Easy=4, Normal=6, Hard=8 }
public enum PlatfotmGUIType { None, Storage, Procesing, DronStation, Turret, PowerGenerator }
public enum PlatformItemSendingType { None, Storage, Procesing }
public enum DroneTask { None, ReturnToDS, Intlet, Outlet, PutUpItem, PutDownItem, }
public enum ObjectPlanType { Placed, Building, Disasemble }

public enum EnemyDo { None, Moving, Attacking, Wait }
public enum EnemyType { Dif, Tank, Ball, Range, Flying }

public enum BulletsE { None, Quarrel, Quarrel2, GunB, GunB2, Laser, Rocket, EneTurB }

public enum Messages
{
    CantConnectThisObj, CantBuildConnectionThroughObj, ConnectionIsAlreadyBuilt, DoesntSelectConToBuild,
    CantDemolitionObj, CantDisasembleObj, CantMineObj, CantCutObj, CantPlantHere,
    CantBuildItHere, YouAreUnderAttack, AttackIsComing, CantStartHere, NoDronStation, NoTransmisonTower,
}

public enum ColonyNames { Valley=0, Desert=1, Forest=2, Moutains=3, Island=4, River=5, Canyon=6, }

public enum Technologies
{
    Dif=0,
    DroneComunication=1,
    Ballista=2, GunTurret=3, LaserTurret=4, RocketTurret=5,
    Wall0=6, Wall1=7, Wall2=8, Wall3=9, ShieldGenerator=10,
    TransmissionTower=11, CombustionGenerator=12, SteamGenerator=13, SolarPanel1=14,
    BatteryStorage=15, Repairer=16,
    Connection1=17, Connection2=18, Connection3=19,
    Warehouse1=20, Warehouse2=21, Junkyard=22, Connector=23, FastConnector=24, Launchpad=25, BasicRequester=26, SpaceRequester=27,
    Quarry=28,
    Pulverizer=29, CopperOre=30, CopperDust=31, IronDust=32, Biomass=33, StoneCrushing=34, Silicon=35,
    Smelter=36, ElectricSmelter=37, Glass=38, StoneBrick=39, CopperPlate=40, IronPlate=41, Carbon=42, GraphiteRod=43, Steel=44,
    ChemicalPlant=45, BottleEthanol=46, SplitWater=47, Sulfur=48, SulfuricAcid=49, Plastic=50, Gunpowder=51, Rubber=52,
    Planter=53, Woodcuter=54, Farm=55, Grape=56, Flax=57, RubberPlant=58,
    Pump=59,
    BasicCrafter=60, Crafter=61, Planks=62, CopperCable=63, IronGear=64, IronRod=65, Bag=66, Bottle=67, Dynamite=68, Quarrel=69, Quarrel2=70, GunMagazine=71, GunMagazine2=72, Rocket=73, InsulatedCable=74, WoodenCircuit=75, PlasticCircuit=76, AdvancedCircuit=77, ElectricEngine=78, Battery=79, Drone=80,
    OrganicOil=81, DroneStation=82, BagOfSand=83,
}