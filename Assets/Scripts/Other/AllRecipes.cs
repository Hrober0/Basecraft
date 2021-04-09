using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjToBuild
{
    public Obj objectType;
    public ObjectPlanType planType;
    public int xTB;
    public int yTB;
    public List<ItemRAQ> neededItems;
    public int startPointRoadsX;
    public int startPointRoadsY;
    public Transform trans;

    public ObjToBuild(Obj objectType, ObjectPlanType planType, int xTB, int yTB, List<ItemRAQ> neededItems, Transform trans)
    {
        this.objectType = objectType;
        this.planType = planType;
        this.xTB = xTB;
        this.yTB = yTB;
        this.neededItems = neededItems;
        this.trans = trans;
    }
}

[System.Serializable]
public class ItemRAQ
{
    public Res res;
    public int qua;

    public ItemRAQ(Res res, int qua)
    {
        this.res = res;
        this.qua = qua;
    }
}

[System.Serializable]
public class ObjToGrow
{
    public float time;
    public Obj obj;
    public int x;
    public int y;

    public ObjToGrow(float time, Obj obj, int x, int y)
    {
        this.time = time;
        this.obj = obj;
        this.x = x;
        this.y = y;
    }
}

public class CraftRecipe
{
    public List<ItemRAQ> ItemIn;
    public List<ItemRAQ> ItemOut;
    public float exeTime;
    public Technologies unlockingTech;
    public bool active = false;

    public CraftRecipe(List<ItemRAQ> itemIn, List<ItemRAQ> itemOut, float exeTime, Technologies unlockingTech)
    {
        ItemIn = itemIn;
        ItemOut = itemOut;
        this.exeTime = exeTime;
        this.unlockingTech = unlockingTech;
    }
}

public class BuildingRecipe
{
    public Obj building;
    public List<ItemRAQ> neededItems;
    public List<Obj> whereItCanBeBuild;
    public List<Res> products;
    public Technologies unlockingTech;
    public string stats = "";
    public bool active = false;
    public int page;

    public BuildingRecipe(Obj building, List<ItemRAQ> neededItems, List<Obj> whereItCanBeBuild, List<Res> products, Technologies unlockingTech, string stats)
    {
        this.building = building;
        this.neededItems = neededItems;
        this.whereItCanBeBuild = whereItCanBeBuild;
        this.products = products;
        this.unlockingTech = unlockingTech;
        this.stats = stats;
    }
}

public class ConnectionRecipe
{
    public Obj connection;
    public List<ItemRAQ> neededItems;
    public float iTLMultiplayer = 1;
    public float itemPerSecond = 1;
    public float maxLenght = 100f;
    public Technologies unlockingTech;
    public bool active = false;

    public ConnectionRecipe(Obj connection, List<ItemRAQ> neededItems, float iTLMultiplayer, float itemPerSecond, float maxLenght, Technologies unlockingTech)
    {
        this.connection = connection;
        this.neededItems = neededItems;
        this.iTLMultiplayer = iTLMultiplayer;
        this.itemPerSecond = itemPerSecond;
        this.maxLenght = maxLenght;
        this.unlockingTech = unlockingTech;
    }
}

public class Fuel
{
    public Res fuelRes;
    public int energyValue;

    public Fuel(Res fuelRes, int energyValue)
    {
        this.fuelRes = fuelRes;
        this.energyValue = energyValue;
    }
}

public class AllRecipes : MonoBehaviour
{
    private List<CraftRecipe> basicCrafterRecipes;
    private List<CraftRecipe> crafterRecipes;
    private List<CraftRecipe> smelterRecipes;
    private List<CraftRecipe> pulverizerRecipes;
    private List<CraftRecipe> farmRecipes;
    private List<CraftRecipe> chemicalPlantRecipes;

    public readonly List<Obj> objectWithCraftRecipes = new List<Obj> { Obj.BasicCrafter, Obj.Crafter, Obj.Smelter, Obj.ElectricSmelter, Obj.Pulverizer, Obj.Farm, Obj.ChemicalPlant };
    public List<Obj> ObjectThatCanBeBuilt { get => objectThatCanBeBuilt; }
    private List<Obj> objectThatCanBeBuilt;

    //public bool[] GetUnlockRes { get => UnlockRes; }
    private bool[] UnlockRes;

    private List<BuildingRecipe> buildingProductionRecipes;
    private List<BuildingRecipe> buildingMilitaryRecipes;
    private List<BuildingRecipe> buildingEnergyRecipes;
    private List<BuildingRecipe> buildingOtherRecipes;

    public List<ConnectionRecipe> connectionRecipes;

    public List<Fuel> fuelList;

    public static AllRecipes instance;

    void Awake()
    {
        if (instance != null) { Debug.Log("More then one " + this + " on scen, return."); return; }
        instance = this;

        //Basic crafter Recipes
        basicCrafterRecipes = new List<CraftRecipe>
        {
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Wood, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Planks, 2) },
                4f,
                Technologies.Planks
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.IronPlate, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.IronGear, 2) },
                4f,
                Technologies.IronGear
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.IronPlate, 2), new ItemRAQ(Res.GraphiteRod, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.IronRod, 1) },
                6f,
                Technologies.IronRod
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Wood, 2), new ItemRAQ(Res.StoneOre, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Quarrel, 1) },
                6f,
                Technologies.Quarrel
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperPlate, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.CopperCable, 3) },
                10f,
                Technologies.CopperCable
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperCable, 4), new ItemRAQ(Res.Wood, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.WoodenCircuit, 1) },
                14f,
                Technologies.WoodenCircuit
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Flax, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Bag, 2) },
                8f,
                Technologies.Bag
            ),
             new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Glass, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.BottleEmpty, 1) },
                12f,
                Technologies.Bottle
            ),
             new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperPlate, 2), new ItemRAQ(Res.IronPlate, 1)},
                new List<ItemRAQ> { new ItemRAQ(Res.GunMagazine, 1) },
                10f,
                Technologies.GunMagazine
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperCable, 4), new ItemRAQ(Res.IronPlate, 2), new ItemRAQ(Res.IronGear, 2), new ItemRAQ(Res.GraphiteRod, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.ElectricEngine, 1) },
                10f,
                Technologies.ElectricEngine
            ),
        };

        //Crafter Recipes
        crafterRecipes = new List<CraftRecipe>
        {
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Wood, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Planks, 2) },
                2f,
                Technologies.Planks
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.IronPlate, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.IronGear, 2) },
                2f,
                Technologies.IronGear
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.IronPlate, 2), new ItemRAQ(Res.GraphiteRod, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.IronRod, 1) },
                3f,
                Technologies.IronRod
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Wood, 2), new ItemRAQ(Res.StoneOre, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Quarrel, 1) },
                3f,
                Technologies.Quarrel
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Planks, 1), new ItemRAQ(Res.IronPlate, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Quarrel2, 1) },
                6f,
                Technologies.Quarrel2
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperPlate, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.CopperCable, 3) },
                5f,
                Technologies.CopperCable
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperCable, 4), new ItemRAQ(Res.Wood, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.WoodenCircuit, 1) },
                7f,
                Technologies.WoodenCircuit
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Flax, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Bag, 2) },
                5f,
                Technologies.Bag
            ),
             new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Glass, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.BottleEmpty, 1) },
                6f,
                Technologies.Bottle
            ),
             new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperPlate, 2), new ItemRAQ(Res.IronPlate, 1)},
                new List<ItemRAQ> { new ItemRAQ(Res.GunMagazine, 1) },
                6f,
                Technologies.GunMagazine
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Gunpowder, 1), new ItemRAQ(Res.IronPlate, 2)},
                new List<ItemRAQ> { new ItemRAQ(Res.GunMagazine2, 1) },
                10f,
                Technologies.GunMagazine2
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Gunpowder, 3), new ItemRAQ(Res.Plastic, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.Dynamite, 1) },
                7f,
                Technologies.Dynamite
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperCable, 4), new ItemRAQ(Res.IronPlate, 2), new ItemRAQ(Res.IronGear, 2), new ItemRAQ(Res.GraphiteRod, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.ElectricEngine, 1) },
                10f,
                Technologies.ElectricEngine
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Plastic, 2), new ItemRAQ(Res.CopperCable, 3), new ItemRAQ(Res.WoodenCircuit, 5) },
                new List<ItemRAQ> { new ItemRAQ(Res.PlasticCircuit, 1) },
                10f,
                Technologies.PlasticCircuit
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Steel, 4), new ItemRAQ(Res.ElectricEngine, 4), new ItemRAQ(Res.Battery, 1),new ItemRAQ(Res.PlasticCircuit, 3) },
                new List<ItemRAQ> { new ItemRAQ(Res.Drone, 1) },
                10f,
                Technologies.Drone
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Steel, 3), new ItemRAQ(Res.Battery, 1), new ItemRAQ(Res.Gunpowder, 3), new ItemRAQ(Res.ElectricEngine, 2),new ItemRAQ(Res.PlasticCircuit, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.Rocket, 1) },
                16f,
                Technologies.Rocket
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperCable, 3), new ItemRAQ(Res.Rubber, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.InsulatedCable, 2) },
                10f,
                Technologies.InsulatedCable
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.InsulatedCable, 3), new ItemRAQ(Res.SulfuricAcid, 1), new ItemRAQ(Res.PlasticCircuit, 2), new ItemRAQ(Res.IronPlate, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.AdvancedCircuit, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                12f,
                Technologies.AdvancedCircuit
            ),
        };

        //Smelter recipes
        smelterRecipes = new List<CraftRecipe>
        {
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Wood, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Coal, 1) },
                8f,
                Technologies.Carbon
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Coal, 3) },
                new List<ItemRAQ> { new ItemRAQ(Res.GraphiteRod, 1) },
                5f,
                Technologies.GraphiteRod
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.StoneOre, 3) },
                new List<ItemRAQ> { new ItemRAQ(Res.StoneBrick, 1) },
                8f,
                Technologies.StoneBrick
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperOreCtm, 4) },
                new List<ItemRAQ> { new ItemRAQ(Res.CopperPlate, 2) },
                12f,
                Technologies.CopperPlate
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperOre, 3) },
                new List<ItemRAQ> { new ItemRAQ(Res.CopperPlate, 2) },
                5f,
                Technologies.CopperPlate
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperDust, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.CopperPlate, 2) },
                4f,
                Technologies.CopperPlate
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.IronOre, 4) },
                new List<ItemRAQ> { new ItemRAQ(Res.IronPlate, 3) },
                5f,
                Technologies.IronPlate
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.IronDust, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.IronPlate, 2) },
                4f,
                Technologies.IronPlate
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Sand, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Glass, 1) },
                4f,
                Technologies.Glass
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.BagSand, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Glass, 4) },
                6f,
                Technologies.Glass
            ),
             new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.IronPlate, 5), new ItemRAQ(Res.GraphiteRod, 2), },
                new List<ItemRAQ> { new ItemRAQ(Res.Steel, 1) },
                4f,
                Technologies.Steel
            ),
        };

        //Pulverizer recipes
        pulverizerRecipes = new List<CraftRecipe>
        {
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperOreCtm, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.CopperOre, 1) },
                3f,
                Technologies.CopperOre
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.CopperOre, 4) },
                new List<ItemRAQ> { new ItemRAQ(Res.CopperDust, 5) },
                3f,
                Technologies.CopperDust
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.IronOre, 3) },
                new List<ItemRAQ> { new ItemRAQ(Res.IronDust, 4) },
                3f,
                Technologies.IronDust
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.StoneOre, 3) },
                new List<ItemRAQ> { new ItemRAQ(Res.Sand, 1) },
                8f,
                Technologies.StoneCrushing
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Sand, 2), new ItemRAQ(Res.Bag, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.BagSand, 1) },
                8f,
                Technologies.StoneCrushing
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Sapling, 1), new ItemRAQ(Res.BottleWater, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Biomass, 1) },
                7f,
                Technologies.Biomass
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Grape, 1), new ItemRAQ(Res.BottleWater, 1), new ItemRAQ(Res.BottleEmpty, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.Biomass, 3) },
                7f,
                Technologies.Biomass
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Flax, 1), new ItemRAQ(Res.BottleWater, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Biomass, 2) },
                7f,
                Technologies.Biomass
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.RubberPlant, 1), new ItemRAQ(Res.BottleWater, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.Biomass, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                7f,
                Technologies.Biomass
            ),
        };

        //Farm recipe
        farmRecipes = new List<CraftRecipe>
        {
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.BottleWater, 1)},
                new List<ItemRAQ> { new ItemRAQ(Res.Grape, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                10f,
                Technologies.Grape
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.BottleWater, 1)},
                new List<ItemRAQ> { new ItemRAQ(Res.Flax, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                10f,
                Technologies.Flax
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.BottleWater, 1)},
                new List<ItemRAQ> { new ItemRAQ(Res.RubberPlant, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                10f,
                Technologies.RubberPlant
            ),
        };

        //Chemical plant recipe
        chemicalPlantRecipes = new List<CraftRecipe>
        {
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.BottleEmpty, 1), new ItemRAQ(Res.Grape, 1)},
                new List<ItemRAQ> { new ItemRAQ(Res.BottleEthanol, 1) },
                8f,
                Technologies.BottleEthanol
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.BottleEthanol, 1), new ItemRAQ(Res.Wood, 3), new ItemRAQ(Res.Sulfur, 2)},
                new List<ItemRAQ> { new ItemRAQ(Res.Gunpowder, 3), new ItemRAQ(Res.BottleEmpty, 1) },
                3f,
                Technologies.Gunpowder
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.RubberPlant, 1), new ItemRAQ(Res.BottleEthanol, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Rubber, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                3f,
                Technologies.Rubber
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.BottleWater, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Hydrogen, 2), new ItemRAQ(Res.Oxygen, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                9f,
                Technologies.SplitWater
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Biomass, 1), new ItemRAQ(Res.Oxygen, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.BottleOil, 1), new ItemRAQ(Res.BottleWater, 1)},
                15f,
                Technologies.OrganicOil
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.BottleOil, 1), new ItemRAQ(Res.Coal, 2), new ItemRAQ(Res.Hydrogen, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Plastic, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                7f,
                Technologies.Plastic
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.BottleOil, 1), new ItemRAQ(Res.BottleWater, 1), new ItemRAQ(Res.IronPlate, 3) },
                new List<ItemRAQ> { new ItemRAQ(Res.Sulfur, 1), new ItemRAQ(Res.BottleEmpty, 2) },
                7f,
                Technologies.Sulfur
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.Sulfur, 1), new ItemRAQ(Res.Hydrogen, 1), new ItemRAQ(Res.BottleWater, 2) },
                new List<ItemRAQ> { new ItemRAQ(Res.SulfuricAcid, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                12f,
                Technologies.SulfuricAcid
            ),
            new CraftRecipe(
                new List<ItemRAQ> { new ItemRAQ(Res.SulfuricAcid, 1), new ItemRAQ(Res.CopperPlate, 2), new ItemRAQ(Res.IronPlate, 1) },
                new List<ItemRAQ> { new ItemRAQ(Res.Battery, 1), new ItemRAQ(Res.BottleEmpty, 1) },
                7f,
                Technologies.Battery
            ),
        };

        //Res
        UnlockRes = new bool[Enum.GetNames(typeof(Res)).Length];

        //Building Recipes
        objectThatCanBeBuilt = new List<Obj>();
        buildingProductionRecipes = new List<BuildingRecipe>
        {
            new BuildingRecipe(
                Obj.Smelter,
                new List<ItemRAQ>{ new ItemRAQ(Res.StoneOre, 10)},
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.Coal, Res.GraphiteRod, Res.StoneBrick, Res.CopperPlate, Res.IronPlate, Res.Glass, Res.Steel },
                Technologies.Smelter,
                "Requirement: fuel\n-Health: " + GetMaxHelthOfObj(Obj.Smelter)
            ),
            new BuildingRecipe(
                Obj.Woodcuter,
                new List<ItemRAQ>{ new ItemRAQ(Res.Wood, 3), new ItemRAQ(Res.StoneBrick, 4) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile },
                new List<Res>{ Res.Wood },
                Technologies.Woodcuter,
                "-Range: 4 fields\n-Items from one tree:\n saplings 1-2, wood 2-4\n-Cutting time: 15s\n-Health: " + GetMaxHelthOfObj(Obj.Woodcuter)
            ),
            new BuildingRecipe(
                Obj.Planter,
                new List<ItemRAQ>{ new ItemRAQ(Res.Wood, 5), new ItemRAQ(Res.CopperPlate, 3), new ItemRAQ(Res.StoneOre, 4) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile },
                new List<Res>{},
                Technologies.Planter,
                "-Range: 3 fields\n-Requirement: saplings\n-Planting time: 12s\n-Health: " + GetMaxHelthOfObj(Obj.Planter)
            ),
            new BuildingRecipe(
                Obj.BasicCrafter,
                new List<ItemRAQ>{ new ItemRAQ(Res.StoneBrick, 5), new ItemRAQ(Res.CopperPlate, 2), new ItemRAQ(Res.Wood, 4)},
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.Planks, Res.CopperCable, Res.WoodenCircuit, Res.IronGear, Res.ElectricEngine, Res.Bag, Res.BottleEmpty, Res.Quarrel, Res.GunMagazine },
                Technologies.BasicCrafter,
                "-Health: " + GetMaxHelthOfObj(Obj.BasicCrafter)
            ),
            new BuildingRecipe(
                Obj.Quarry,
                new List<ItemRAQ>{ new ItemRAQ(Res.IronGear, 2), new ItemRAQ(Res.IronPlate, 4), new ItemRAQ(Res.StoneBrick, 3) },
                new List<Obj>{ Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.StoneOre, Res.IronOre, Res.CopperOreCtm },
                Technologies.Quarry,
                "-Mining time: 6s\n-Health: " + GetMaxHelthOfObj(Obj.Quarry)
            ),

            new BuildingRecipe(
                Obj.Pulverizer,
                new List<ItemRAQ>{ new ItemRAQ(Res.StoneBrick, 3), new ItemRAQ(Res.CopperPlate, 4), new ItemRAQ(Res.IronGear, 2)},
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.BagSand, Res.CopperOre, Res.Biomass },
                Technologies.Pulverizer,
                "-Health: " + GetMaxHelthOfObj(Obj.Pulverizer)
            ),
            new BuildingRecipe(
                Obj.Farm,
                new List<ItemRAQ>{new ItemRAQ(Res.StoneBrick, 3), new ItemRAQ(Res.Planks, 8), new ItemRAQ(Res.WoodenCircuit, 3)},
                new List<Obj>{ Obj.None, Obj.TerrainFertile },
                new List<Res>{ Res.Grape, Res.Flax, Res.RubberPlant },
                Technologies.Farm,
                "Requirement: bottle of water\n-Health: " + GetMaxHelthOfObj(Obj.Farm)
            ),
            new BuildingRecipe(
                Obj.Pump,
                new List<ItemRAQ>{new ItemRAQ(Res.StoneBrick, 5), new ItemRAQ(Res.IronGear, 2), new ItemRAQ(Res.CopperPlate, 5), new ItemRAQ(Res.Planks, 3)},
                new List<Obj>{ Obj.WaterSource, Obj.OilSource },
                new List<Res>{ Res.BottleWater, Res.BottleOil },
                Technologies.Pump,
                "-Health: " + GetMaxHelthOfObj(Obj.Pump)
            ),
            new BuildingRecipe(
                Obj.Crafter,
                new List<ItemRAQ>{ new ItemRAQ(Res.IronPlate, 6), new ItemRAQ(Res.IronGear, 4), new ItemRAQ(Res.WoodenCircuit, 3), new ItemRAQ(Res.ElectricEngine, 2)},
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.Planks, Res.CopperCable, Res.WoodenCircuit, Res.IronGear, Res.ElectricEngine, Res.Bag, Res.BottleEmpty, Res.PlasticCircuit, Res.Quarrel, Res.Quarrel2, Res.GunMagazine, Res.GunMagazine2, Res.Rocket, Res.Drone },
                Technologies.Crafter,
                "-Power required: 2kW/s\n-Health: " + GetMaxHelthOfObj(Obj.Crafter)
            ),
             new BuildingRecipe(
                Obj.ElectricSmelter,
                new List<ItemRAQ>{ new ItemRAQ(Res.Steel, 1), new ItemRAQ(Res.IronPlate, 4), new ItemRAQ(Res.CopperCable, 5), new ItemRAQ(Res.StoneBrick, 3) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.Coal, Res.GraphiteRod, Res.StoneBrick, Res.CopperPlate, Res.IronPlate, Res.Glass, Res.Steel },
                Technologies.ElectricSmelter,
                "-Power required: 10kW/s\n-Health: " + GetMaxHelthOfObj(Obj.ElectricSmelter)
            ),
            new BuildingRecipe(
                Obj.ChemicalPlant,
                new List<ItemRAQ>{ new ItemRAQ(Res.StoneBrick, 4), new ItemRAQ(Res.IronPlate, 6), new ItemRAQ(Res.Steel, 6), new ItemRAQ(Res.WoodenCircuit, 4)},
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.BottleEthanol, Res.Gunpowder, Res.BottleOil, Res.Rubber, Res.Hydrogen, Res.Oxygen, Res.Sulfur, Res.SulfuricAcid, Res.Plastic, Res.Battery },
                Technologies.ChemicalPlant,
                "-Power required: 5kW/s\n-Health: " + GetMaxHelthOfObj(Obj.ChemicalPlant)
            ),
        };
        for (int i = 0; i < buildingProductionRecipes.Count; i++)   { buildingProductionRecipes[i].page = 1; objectThatCanBeBuilt.Add(buildingProductionRecipes[i].building); }
        buildingMilitaryRecipes = new List<BuildingRecipe>
        {
                new BuildingRecipe(
                Obj.Ballista,
                new List<ItemRAQ>{ new ItemRAQ(Res.Planks, 4), new ItemRAQ(Res.StoneBrick, 4), new ItemRAQ(Res.Wood, 2),},
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.Quarrel, Res.Quarrel2 },
                Technologies.Ballista,
                "-Bullet capacity: 3\n-Fire rate: 1 shot/s\n-Damage: 20/40 per shot\n-Range: 7 fields\n-Health: " + GetMaxHelthOfObj(Obj.Ballista)
            ),
            new BuildingRecipe(
                Obj.GunTurret,
                new List<ItemRAQ>{ new ItemRAQ(Res.StoneBrick, 4), new ItemRAQ(Res.IronPlate, 4), new ItemRAQ(Res.ElectricEngine, 2), new ItemRAQ(Res.IronGear, 3)},
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.GunMagazine, Res.GunMagazine2 },
                Technologies.GunTurret,
                "-Bullet capacity: 10\n-Fire rate: 2 shot/s\n-Damage: 40/80 per shot\n-Range: 8 fields\n-Health: " + GetMaxHelthOfObj(Obj.GunTurret)
            ),
            new BuildingRecipe(
                Obj.LaserTurret,
                new List<ItemRAQ>{ new ItemRAQ(Res.CopperCable, 5), new ItemRAQ(Res.Battery, 3), new ItemRAQ(Res.PlasticCircuit, 4), new ItemRAQ(Res.Steel, 6) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.LaserTurret,
                "-Fire rate: 4 shot/s\n-Damage: 15 per shot\n-Range: 7 fields\n-Power required: 30kW/s\n-Health: " + GetMaxHelthOfObj(Obj.LaserTurret)
            ),
            new BuildingRecipe(
                Obj.RocketTurret,
                new List<ItemRAQ>{ new ItemRAQ(Res.IronPlate, 6), new ItemRAQ(Res.ElectricEngine, 2), new ItemRAQ(Res.WoodenCircuit, 5) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{ Res.Rocket },
                Technologies.RocketTurret,
                "Bullet capacity: 1\n-Fire rate: 0.1 shoot/s\n-Damage: 200 per shot\n-Range: 16 fields\n-Health: " + GetMaxHelthOfObj(Obj.RocketTurret)
            ),
            new BuildingRecipe(
                Obj.Wall0,
                new List<ItemRAQ>{ new ItemRAQ(Res.Wood, 2) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Wall0,
                "-Health: " + GetMaxHelthOfObj(Obj.Wall0)
            ),
            new BuildingRecipe(
                Obj.Wall1,
                new List<ItemRAQ>{ new ItemRAQ(Res.BagSand, 1), new ItemRAQ(Res.StoneOre, 1), new ItemRAQ(Res.StoneBrick, 1) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Wall1,
                "Health: " + GetMaxHelthOfObj(Obj.Wall1)
            ),
            new BuildingRecipe(
                Obj.Wall2,
                new List<ItemRAQ>{ new ItemRAQ(Res.IronPlate, 2), new ItemRAQ(Res.Steel, 1) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Wall2,
                "-Health: " + GetMaxHelthOfObj(Obj.Wall2)
            ),
            new BuildingRecipe(
                Obj.Wall3,
                new List<ItemRAQ>{ new ItemRAQ(Res.Plastic, 1), new ItemRAQ(Res.CopperCable, 1), new ItemRAQ(Res.Glass, 1) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Wall3,
                "-Health: " + GetMaxHelthOfObj(Obj.Wall3) + "\n-Power required: 0.5kW/s"
            ),
        };
        for (int i = 0; i < buildingMilitaryRecipes.Count; i++)     { buildingMilitaryRecipes[i].page  = 2;  objectThatCanBeBuilt.Add(buildingMilitaryRecipes[i].building); }
        buildingEnergyRecipes = new List<BuildingRecipe>
        {
            new BuildingRecipe(
                Obj.TransmissionTower,
                new List<ItemRAQ>{ new ItemRAQ(Res.IronPlate, 4), new ItemRAQ(Res.CopperCable, 3), new ItemRAQ(Res.WoodenCircuit, 3) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.TransmissionTower,
                "-Range: 7 fields\n-Health: " + GetMaxHelthOfObj(Obj.TransmissionTower)
            ),
            new BuildingRecipe(
                Obj.CombustionGenerator,
                new List<ItemRAQ>{ new ItemRAQ(Res.CopperPlate, 6), new ItemRAQ(Res.WoodenCircuit, 2), new ItemRAQ(Res.CopperCable, 4), new ItemRAQ(Res.IronPlate, 4)},
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.CombustionGenerator,
                "-Electricity production: 10kW/s\n-Requirement: fuel\n-Health: " + GetMaxHelthOfObj(Obj.CombustionGenerator)
            ),
            new BuildingRecipe(
                Obj.SteamGenerator,
                new List<ItemRAQ>{ new ItemRAQ(Res.CopperPlate, 3), new ItemRAQ(Res.ElectricEngine, 2), new ItemRAQ(Res.WoodenCircuit, 2), new ItemRAQ(Res.Steel, 4)},
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.SteamGenerator,
                "-Electricity production: 30kW/s\n-Requirement: bottle of water + fuel\n-Health: " + GetMaxHelthOfObj(Obj.SteamGenerator)
            ),
            new BuildingRecipe(
                Obj.SolarPanel1,
                new List<ItemRAQ>{ new ItemRAQ(Res.Glass, 9), new ItemRAQ(Res.IronPlate, 4), new ItemRAQ(Res.CopperCable, 4), new ItemRAQ(Res.PlasticCircuit, 3) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.SolarPanel1,
                "-Electricity production: 5kW/s\n-Health: " + GetMaxHelthOfObj(Obj.SolarPanel1)
            ),
            new BuildingRecipe(
                Obj.Battery,
                new List<ItemRAQ>{ new ItemRAQ(Res.Battery, 3), new ItemRAQ(Res.CopperPlate, 7), new ItemRAQ(Res.CopperCable, 3), new ItemRAQ(Res.WoodenCircuit, 3) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Battery,
                "-Capacity: 500kW\n-Health: " + GetMaxHelthOfObj(Obj.Battery)
            ),
            /*new BuildingRecipe(
                Obj.Repairer,
                new List<ItemInRecipe>{ new ItemInRecipe(Res.Battery, 1), new ItemInRecipe(Res.CopperPlate, 7), new ItemInRecipe(Res.CopperCable, 3), new ItemInRecipe(Res.PlasticCircuit, 3) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Repairer,
                "-Range: 80\n-Power required: 20kW/s\n-Health: " + GetMaxHelthOfObj(Obj.Repairer)
            ),*/
        };
        for (int i = 0; i < buildingEnergyRecipes.Count; i++)       { buildingEnergyRecipes[i].page = 3;     objectThatCanBeBuilt.Add(buildingEnergyRecipes[i].building); }
        buildingOtherRecipes = new List<BuildingRecipe>
        {
            new BuildingRecipe(
                Obj.Warehouse1,
                new List<ItemRAQ>{ new ItemRAQ(Res.Wood, 3), new ItemRAQ(Res.StoneOre, 4) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Warehouse1,
                "-Maximum number of items: 30\n-Health: " + GetMaxHelthOfObj(Obj.Warehouse1)
            ),
            new BuildingRecipe(
                Obj.Warehouse2,
                new List<ItemRAQ>{ new ItemRAQ(Res.StoneBrick, 4), new ItemRAQ(Res.IronPlate, 5), new ItemRAQ(Res.Planks, 4) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Warehouse2,
                "-Maximum number of items: 200\n-Health: " + GetMaxHelthOfObj(Obj.Warehouse2)
            ),
            new BuildingRecipe(
                Obj.Connector,
                new List<ItemRAQ>{ new ItemRAQ(Res.IronGear, 1), new ItemRAQ(Res.WoodenCircuit, 2), new ItemRAQ(Res.StoneBrick, 4) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Connector,
                "-Health: " + GetMaxHelthOfObj(Obj.Connector)
            ),
            /*new BuildingRecipe(
                Obj.FastConnector,
                new List<ItemInRecipe>{ new ItemInRecipe(Res.IronGear, 3), new ItemInRecipe(Res.AdvancedCircuit, 2), new ItemInRecipe(Res.Steel, 5) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.FastConnector,
                "-Health: " + GetMaxHelthOfObj(Obj.FastConnector)
            ),*/
            new BuildingRecipe(
                Obj.DroneStation,
                new List<ItemRAQ>{ new ItemRAQ(Res.Plastic, 6), new ItemRAQ(Res.StoneBrick, 6), new ItemRAQ(Res.ElectricEngine, 2), new ItemRAQ(Res.PlasticCircuit, 3) },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.DroneStation,
                "-Range: 20 fields\n-Holds 24 drones\n-Health: " + GetMaxHelthOfObj(Obj.DroneStation)
            ),
            new BuildingRecipe(
                Obj.Junkyard,
                new List<ItemRAQ>{ new ItemRAQ(Res.StoneBrick, 4), new ItemRAQ(Res.StoneOre, 6), new ItemRAQ(Res.IronGear, 4), },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Junkyard,
                "-Frequency: 0.2 item per s\n-Maximum number of items: 20\n-Health: " + GetMaxHelthOfObj(Obj.Junkyard)
            ),
            new BuildingRecipe(
                Obj.Launchpad,
                new List<ItemRAQ>{ new ItemRAQ(Res.StoneBrick, 12), new ItemRAQ(Res.Planks, 14), new ItemRAQ(Res.CopperPlate, 8), },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.Launchpad,
                "-Maximum number of items: 50\n-Health: " + GetMaxHelthOfObj(Obj.Launchpad)
            ),
            /*new BuildingRecipe(
                Obj.BasicRequester,
                new List<ItemInRecipe>{ new ItemInRecipe(Res.WoodenCircuit, 3), new ItemInRecipe(Res.CopperPlate, 5), new ItemInRecipe(Res.IronGear, 4), },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.BasicRequester,
                "-Maximum number of items: 50\n-Health: " + GetMaxHelthOfObj(Obj.BasicRequester)
            ),*/
            /*new BuildingRecipe(
                Obj.SpaceRequester,
                new List<ItemInRecipe>{ new ItemInRecipe(Res.PlasticCircuit, 4), new ItemInRecipe(Res.Steel, 6), new ItemInRecipe(Res.IronGear, 4), },
                new List<Obj>{ Obj.None, Obj.TerrainFertile, Obj.StoneOre, Obj.CopperOre, Obj.IronOre, },
                new List<Res>{},
                Technologies.SpaceRequester,
                "-Maximum number of items: 200\n-Health: " + GetMaxHelthOfObj(Obj.SpaceRequester)
            ),*/
        };
        for (int i = 0; i < buildingOtherRecipes.Count; i++)        { buildingOtherRecipes[i].page  = 4;     objectThatCanBeBuilt.Add(buildingOtherRecipes[i].building); }

        //Connection Recipe
        connectionRecipes = new List<ConnectionRecipe>
        {
            new ConnectionRecipe(
                Obj.Connection1,
                new List<ItemRAQ>{ new ItemRAQ(Res.Planks, 2) },
                0.3f,
                0.4f,
                50,
                Technologies.Connection1
            ),
            new ConnectionRecipe(
                Obj.Connection2,
                new List<ItemRAQ>{ new ItemRAQ(Res.Glass, 2), new ItemRAQ(Res.IronGear, 1) },
                0.3f,
                1f,
                200,
                Technologies.Connection2
            ),
             new ConnectionRecipe(
                Obj.Connection3,
                new List<ItemRAQ>{ new ItemRAQ(Res.Rubber, 1), new ItemRAQ(Res.ElectricEngine, 1), new ItemRAQ(Res.IronGear, 1) },
                0.25f,
                5f,
                50,
                Technologies.Connection3
            ),
        };

        //Fuel list
        fuelList = new List<Fuel> { new Fuel(Res.Wood, 200), new Fuel(Res.Coal, 500) };
    }

    public BuildingRecipe GetBuildRecipe(Obj _obj)
    {
        foreach (BuildingRecipe recipe in buildingProductionRecipes) { if (recipe.building == _obj) { return RewriteRecipe(recipe); } }
        foreach (BuildingRecipe recipe in buildingMilitaryRecipes)   { if (recipe.building == _obj) { return RewriteRecipe(recipe); } }
        foreach (BuildingRecipe recipe in buildingEnergyRecipes)     { if (recipe.building == _obj) { return RewriteRecipe(recipe); } }
        foreach (BuildingRecipe recipe in buildingOtherRecipes)      { if (recipe.building == _obj) { return RewriteRecipe(recipe); } }

        //Debug.Log("Error! " + _obj + " doesn't have BuildingRecipe");
        return null;

        BuildingRecipe RewriteRecipe(BuildingRecipe recipe)
        {
            List<ItemRAQ> rIIRL = new List<ItemRAQ>();
            for (int ii = 0; ii < recipe.neededItems.Count; ii++)
            {
                ItemRAQ item = new ItemRAQ(recipe.neededItems[ii].res, recipe.neededItems[ii].qua);
                rIIRL.Add(item);
            }

            List<Obj> rTL = recipe.whereItCanBeBuild;

            BuildingRecipe newRecipe = new BuildingRecipe(recipe.building, rIIRL, rTL, recipe.products, recipe.unlockingTech, recipe.stats);
            newRecipe.page = recipe.page;
            newRecipe.active = recipe.active;
            return newRecipe;
        }
    }
    public void ActActiveOfRecipe()
    {
        Debug.Log("Updating active recipes");

        //buildings
        foreach (BuildingRecipe recipe in buildingProductionRecipes) { if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(recipe.unlockingTech)) { recipe.active = true; } }
        foreach (BuildingRecipe recipe in buildingMilitaryRecipes)   { if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(recipe.unlockingTech)) { recipe.active = true; } }
        foreach (BuildingRecipe recipe in buildingEnergyRecipes)     { if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(recipe.unlockingTech)) { recipe.active = true; } }
        foreach (BuildingRecipe recipe in buildingOtherRecipes)      { if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(recipe.unlockingTech)) { recipe.active = true; } }

        //connections
        foreach (ConnectionRecipe recipe in connectionRecipes)       { if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(recipe.unlockingTech)) { recipe.active = true; } }

        //recipe
        foreach (Obj obj in objectWithCraftRecipes)
        {
            foreach (CraftRecipe recipe in GetCraftRecipes(obj)) { if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(recipe.unlockingTech)) { recipe.active = true; } }
        }
    }
    public void ActUnlockRes()
    {
        for (int i = 0; i < UnlockRes.Length; i++)
        {
            UnlockRes[i] = IsUnlock((Res)i);
        }

        bool IsUnlock(Res res)
        {
            List<Technologies> unlockingTechnologies = new List<Technologies>();
            switch (res)
            {
                case Res.None: return true;
                case Res.Sapling:       unlockingTechnologies.Add(Technologies.DroneComunication); break;
                case Res.Wood:          unlockingTechnologies.Add(Technologies.DroneComunication); break;
                case Res.Planks:        unlockingTechnologies.Add(Technologies.Planks); break;
                case Res.StoneOre:      unlockingTechnologies.Add(Technologies.DroneComunication); break;
                case Res.StoneBrick:    unlockingTechnologies.Add(Technologies.StoneBrick); break;
                case Res.CopperOreCtm:  unlockingTechnologies.Add(Technologies.DroneComunication); break;
                case Res.CopperOre:     unlockingTechnologies.Add(Technologies.CopperOre); break;
                case Res.CopperDust:    unlockingTechnologies.Add(Technologies.CopperDust); break;
                case Res.CopperPlate:   unlockingTechnologies.Add(Technologies.CopperPlate); break;
                case Res.CopperCable:   unlockingTechnologies.Add(Technologies.CopperCable); break;
                case Res.WoodenCircuit: unlockingTechnologies.Add(Technologies.WoodenCircuit); break;
                case Res.IronOre:       unlockingTechnologies.Add(Technologies.DroneComunication); break;
                case Res.IronDust:      unlockingTechnologies.Add(Technologies.IronDust); break;
                case Res.IronPlate:     unlockingTechnologies.Add(Technologies.IronPlate); break;
                case Res.IronGear:      unlockingTechnologies.Add(Technologies.IronGear); break;
                case Res.IronRod:       unlockingTechnologies.Add(Technologies.IronRod); break;
                case Res.Steel:         unlockingTechnologies.Add(Technologies.Steel); break;
                case Res.Coal:          unlockingTechnologies.Add(Technologies.DroneComunication); break;
                case Res.GraphiteRod:   unlockingTechnologies.Add(Technologies.GraphiteRod); break;
                case Res.ElectricEngine:unlockingTechnologies.Add(Technologies.ElectricEngine); break;
                case Res.Battery:       unlockingTechnologies.Add(Technologies.Battery); break;
                case Res.Grape:         unlockingTechnologies.Add(Technologies.Grape); break;
                case Res.Flax:          unlockingTechnologies.Add(Technologies.Flax); break;
                case Res.RubberPlant:   unlockingTechnologies.Add(Technologies.RubberPlant); break;
                case Res.Rubber:        unlockingTechnologies.Add(Technologies.Rubber); break;
                case Res.Biomass:       unlockingTechnologies.Add(Technologies.Biomass); break;
                case Res.Bag:           unlockingTechnologies.Add(Technologies.Bag); break;
                case Res.BagSand:       unlockingTechnologies.Add(Technologies.BagOfSand); break;
                case Res.Sand:          unlockingTechnologies.Add(Technologies.DroneComunication); break;
                case Res.Glass:         unlockingTechnologies.Add(Technologies.Glass); break;
                case Res.Silicon:       unlockingTechnologies.Add(Technologies.Silicon); break;
                case Res.BottleEmpty:   unlockingTechnologies.Add(Technologies.Bottle); break;
                case Res.BottleWater:   unlockingTechnologies.Add(Technologies.Pump); break;
                case Res.BottleEthanol: unlockingTechnologies.Add(Technologies.BottleEthanol); break;
                case Res.BottleOil:     unlockingTechnologies.Add(Technologies.Pump); break;
                case Res.Hydrogen:      unlockingTechnologies.Add(Technologies.SplitWater); break;
                case Res.Oxygen:        unlockingTechnologies.Add(Technologies.SplitWater); break;
                case Res.Sulfur:        unlockingTechnologies.Add(Technologies.Sulfur); break;
                case Res.SulfuricAcid:  unlockingTechnologies.Add(Technologies.SulfuricAcid); break;
                case Res.Plastic:       unlockingTechnologies.Add(Technologies.Plastic); break;
                case Res.PlasticCircuit:unlockingTechnologies.Add(Technologies.PlasticCircuit); break;
                case Res.Gunpowder:     unlockingTechnologies.Add(Technologies.Gunpowder); break;
                case Res.Dynamite:      unlockingTechnologies.Add(Technologies.Dynamite); break;
                case Res.Quarrel:       unlockingTechnologies.Add(Technologies.Quarrel); break;
                case Res.Quarrel2:      unlockingTechnologies.Add(Technologies.Quarrel2); break;
                case Res.GunMagazine:   unlockingTechnologies.Add(Technologies.GunMagazine); break;
                case Res.GunMagazine2:  unlockingTechnologies.Add(Technologies.GunMagazine2); break;
                case Res.Rocket:        unlockingTechnologies.Add(Technologies.Rocket); break;
                case Res.InsulatedCable:unlockingTechnologies.Add(Technologies.InsulatedCable); break;
                case Res.AdvancedCircuit: unlockingTechnologies.Add(Technologies.AdvancedCircuit); break;
                case Res.Drone:         unlockingTechnologies.Add(Technologies.Drone); break;
            }

            foreach (Technologies tech in unlockingTechnologies)
            {
                if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(tech)) { return true; }
            }

            return false;
        }
    }
    public bool IsResUnlock(Res res)
    {
        if (UnlockRes[(int)res]) { return true; }
        return false;
    }
    public ConnectionRecipe GetConnectionRecipe(Obj _obj)
    {
        for (int i = 0; i < connectionRecipes.Count; i++)
        {
            if (connectionRecipes[i].connection == _obj)
            {
                Obj rObj = connectionRecipes[i].connection;

                List<ItemRAQ> rIIRL = new List<ItemRAQ>();
                if (SpaceBaseMainSc.instance.NeedItemToBuild)
                {
                    for (int ii = 0; ii < connectionRecipes[i].neededItems.Count; ii++)
                    {
                        ItemRAQ item = new ItemRAQ(connectionRecipes[i].neededItems[ii].res, connectionRecipes[i].neededItems[ii].qua);
                        rIIRL.Add(item);
                    }
                }

                return new ConnectionRecipe(rObj, rIIRL, connectionRecipes[i].iTLMultiplayer, connectionRecipes[i].itemPerSecond, connectionRecipes[i].maxLenght, connectionRecipes[i].unlockingTech);
            }
        }

        Debug.Log("Error! " + _obj + " doesn't have ConnectionRecipe");
        return null;
    }
    public List<ItemRAQ> GetNeedItems(Obj _obj)
    {
        if (SpaceBaseMainSc.instance.NeedItemToBuild == false) { return new List<ItemRAQ>(); }

        if(_obj == Obj.Sapling) { return new List<ItemRAQ> { new ItemRAQ(Res.Sapling, 1) }; }

        foreach (BuildingRecipe recipe in buildingProductionRecipes)    { if (recipe.building == _obj) { return RewriteList(recipe.neededItems); } }
        foreach (BuildingRecipe recipe in buildingMilitaryRecipes)      { if (recipe.building == _obj) { return RewriteList(recipe.neededItems); } }
        foreach (BuildingRecipe recipe in buildingEnergyRecipes)        { if (recipe.building == _obj) { return RewriteList(recipe.neededItems); } }
        foreach (BuildingRecipe recipe in buildingOtherRecipes)         { if (recipe.building == _obj) { return RewriteList(recipe.neededItems); } }

        foreach (ConnectionRecipe recipe in connectionRecipes)          { if (recipe.connection == _obj) { return RewriteList(recipe.neededItems); } }

        Debug.Log(_obj + " doesn't have build recape");
        return new List<ItemRAQ>();

        List<ItemRAQ> RewriteList(List<ItemRAQ> list)
        {
            List<ItemRAQ> rIIRL = new List<ItemRAQ>();
            for (int ii = 0; ii < list.Count; ii++)
            {
                ItemRAQ item = new ItemRAQ(list[ii].res, list[ii].qua);
                rIIRL.Add(item);
            }
            return rIIRL;
        }
    }
    public int GetMaxHelthOfObj(Obj obj)
    {
        switch (obj)
        {
            case Obj.None:              return 0;
            case Obj.Locked:            return 0;
            case Obj.BuildingUnderConstruction:return 0;
            case Obj.BuildingUnderDemolition:return 0;
            case Obj.Warehouse1:          return 100;
            case Obj.Woodcuter:         return 100;
            case Obj.Quarry:            return 125;
            case Obj.Planter:           return 75;
            case Obj.Smelter:           return 150;
            case Obj.Pulverizer:        return 150;
            case Obj.Pump:              return 80;
            case Obj.Farm:              return 100;

            case Obj.Ballista:          return 100;
            case Obj.GunTurret:         return 200;
            case Obj.LaserTurret:       return 200;
            case Obj.RocketTurret:      return 250;

            case Obj.Junkyard:         return 150;
            case Obj.Warehouse2:        return 200;
            case Obj.BasicCrafter:      return 100;
            case Obj.Crafter:           return 150;
            case Obj.ChemicalPlant:     return 200;
            case Obj.DroneStation:      return 300;

            case Obj.Connector:         return 50;
            case Obj.FastConnector:     return 75;

            case Obj.Launchpad:         return 250;
            case Obj.BasicRequester:    return 100;
            case Obj.SpaceRequester:    return 250;

            case Obj.ConUnderConstruction:return 0;
            case Obj.ConUnderDemolition:return 0;
            case Obj.Connection1:       return 0;
            case Obj.Connection2:       return 0;
            case Obj.Connection3:       return 0;

            case Obj.TransmissionTower: return 75;
            case Obj.Battery:           return 150;
            case Obj.CombustionGenerator:return 150;
            case Obj.SteamGenerator:    return 200;
            case Obj.SolarPanel1:       return 50;
            case Obj.ElectricSmelter:   return 250;
            case Obj.Repairer:          return 200;

            case Obj.Wall0:             return 100;
            case Obj.Wall1:             return 300;
            case Obj.Wall2:             return 600;
            case Obj.Wall3:             return 200;

            case Obj.Tree:              return 60;
            case Obj.Sapling:           return 20;
            case Obj.StoneOre:          return 0;
            case Obj.CopperOre:         return 0;
            case Obj.IronOre:           return 0;
            case Obj.TerrainFertile:    return 0;
            case Obj.Farmland:          return 20;
            case Obj.FarmlandGrape:     return 20;
            case Obj.FarmlandFlax:      return 20;
            case Obj.FarmlandRubber:    return 20;
            case Obj.WaterSource:       return 0;
            case Obj.OilSource:         return 0;
            case Obj.Mountain:          return 2000;

            case Obj.EnemyCore:         return 300;
            case Obj.EnemySpawner:      return 150;
            case Obj.EnemyTurret:       return 200;
            case Obj.EnemyPlatform:     return 100;
            case Obj.EnemyWall:         return 200;
            case Obj.EnemysTerrain:     return 0;
        }
        Debug.Log(obj + "has not set default health");
        return -1;
    }
    public List<CraftRecipe> GetCraftRecipes(Obj obj)
    {
        switch (obj)
        {
            case Obj.BasicCrafter:      return basicCrafterRecipes;
            case Obj.Crafter:           return crafterRecipes;
            case Obj.Smelter:           return smelterRecipes;
            case Obj.ElectricSmelter:   return smelterRecipes;
            case Obj.Pulverizer:        return pulverizerRecipes;
            case Obj.Farm:              return farmRecipes;
            case Obj.ChemicalPlant:     return chemicalPlantRecipes;
        }

        return null;
    }

    //is?
    public bool IsItPlatform(Obj sObj)
    {
        if (sObj > Obj.StartOfBuildings && sObj < Obj.Wall0 && sObj != Obj.TransmissionTower && sObj != Obj.SolarPanel1) { return true; }
        return false;
    }
    public bool IsItTerrain(Obj sObj)
    {
        if (sObj > Obj.StartOfTerrain && sObj < Obj.EndOfTerrain || sObj == Obj.EnemysTerrain) { return true; }
        return false;
    }
    public bool IsItBuilding(Obj sObj)
    {
        if (sObj > Obj.StartOfBuildings && sObj < Obj.EndOfBuildings) { return true; }
        return false;
    }
    public bool IsItTurret(Obj sObj)
    {
        if (sObj == Obj.Ballista || sObj == Obj.GunTurret || sObj == Obj.LaserTurret || sObj == Obj.RocketTurret) { return true; }
        return false;
    }
    public bool IsItWall(Obj sObj)
    {
        if(sObj == Obj.Wall0 || sObj == Obj.Wall1 || sObj == Obj.Wall2 || sObj == Obj.Wall3) { return true; }
        return false;
    }
    public bool IsItTerrainObj(Obj sObj)
    {
        if (sObj == Obj.Tree || sObj == Obj.Sapling || sObj == Obj.Mountain) { return true; }
        return false;
    }
    public bool IsUsingEnergy(Obj sObj)
    {
        if (
            sObj == Obj.Crafter || sObj == Obj.ChemicalPlant || sObj == Obj.ElectricSmelter || sObj == Obj.Repairer || sObj == Obj.LaserTurret ||
            sObj == Obj.CombustionGenerator || sObj == Obj.SteamGenerator || sObj == Obj.SolarPanel1 ||
            sObj == Obj.Battery ||
            sObj == Obj.TransmissionTower
            ) { return true; }
        return false;
    }
    public bool IsItConnection(Obj sObj)
    {
        if (sObj == Obj.Connection1 || sObj == Obj.Connection2 || sObj == Obj.Connection3 || sObj == Obj.ConUnderConstruction || sObj == Obj.ConUnderDemolition) { return true; }
        return false;
    }
    public bool IsItOreObj(Obj sObj)
    {
        if (sObj == Obj.StoneOre || sObj == Obj.CopperOre || sObj == Obj.IronOre || sObj == Obj.CoalOre) { return true; }
        return false;
    }
    public bool IsItOreRes(Res sRes)
    {
        if (sRes == Res.StoneOre || sRes == Res.CopperOreCtm || sRes == Res.IronOre || sRes == Res.Coal) { return true; }
        return false;
    }
    public bool IsObjHaveCrafterNeedFuelSc(Obj sObj)
    {
        if (sObj == Obj.Smelter || sObj == Obj.BasicCrafter) return true;
        return false;
    }
    public bool IsObjHaveCrafterNeedEnergySc(Obj sObj)
    {
        if (sObj == Obj.Pulverizer || sObj == Obj.Crafter || sObj == Obj.ElectricSmelter || sObj == Obj.ChemicalPlant) return true;
        return false;
    }
}