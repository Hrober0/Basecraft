using UnityEngine;

public class Planter : MonoBehaviour
{
    private float plantingTime = 12f;
    public int range = 3;

    [Header("Energy")]
    [SerializeField] private float needEnergy = 2f;
    private ElectricityUser eleUSc;
    private Vector2Int treePosition; public Vector2Int TreePosition { get => treePosition; set => treePosition = value; }

    private PlatformBehavior platformB;
    private Vector2Int myPos;

    void Awake()
    {
        platformB = gameObject.GetComponent<PlatformBehavior>();
        myPos = platformB.GetTabPos();

        platformB.canBeConnectedOut = false;

        platformB.taskTime = plantingTime;
        platformB.range = range;
        platformB.usingGuiType = PlatfotmGUIType.Procesing;
        platformB.itemSendingType = PlatformItemSendingType.Procesing;

        platformB.SetAllCanInItem(false);
        platformB.itemOnPlatform[(int)Res.Sapling].maxQua = 50;
        platformB.itemOnPlatform[(int)Res.Sapling].canIn = true;
        platformB.itemOnPlatform[(int)Res.Sapling].canOut = false;

        //energy
        eleUSc = gameObject.GetComponent<ElectricityUser>();
        if (eleUSc == null) Debug.LogError(name + " dont have ElectricityUser script!");
        eleUSc.maxEnergyPerSec = needEnergy;
        eleUSc.actCharge = 0f;
        eleUSc.maxCharge = eleUSc.maxEnergyPerSec * 2f;
    }
    void Start()
    {
        ElectricityManager.instance.AddRequester(eleUSc);
        Invoke(nameof(TryPlantTree), 1);
    }
    private void Update()
    {
        if (platformB.working == false) return;

        float prodPercent = eleUSc.actCharge / eleUSc.maxCharge;
        float percTime = Time.deltaTime * prodPercent;
        float energy = needEnergy * percTime;

        if (eleUSc.actCharge < energy) { platformB.working = false; return; }

        eleUSc.actCharge -= energy;
        platformB.timeToEndCraft -= percTime;
        if (platformB.timeToEndCraft <= 0)
        {
            PlantTree();
        }
    }
    private void TryPlantTree()
    {
        if (platformB.itemOnPlatform[(int)Res.Sapling].qua < 1 || platformB.working)
        {
            Invoke(nameof(TryPlantTree), 1);
            return;
        }
        treePosition = WorldMenager.instance.FindTheNearestObjectOnTerrain(Obj.None, Obj.TerrainFertile, myPos.x, myPos.y, range);
        if (treePosition.x == -1)
        {
            Invoke(nameof(TryPlantTree), 1);
            return;
        }

        platformB.startTaskTime = WorldMenager.instance.worldTime;
        platformB.taskTime = plantingTime;
        platformB.timeToEndCraft = plantingTime;
        platformB.working = true;
    }
    private void PlantTree()
    {
        if(WorldMenager.instance.GetSquer(treePosition.x, treePosition.y) == Obj.None)
        {
            platformB.AddItem(Res.Sapling, -1);
            TerrainManager.instance.SpawnSapling(treePosition.x, treePosition.y, true);
        }

        platformB.working = false;
        TryPlantTree();
    }
}
