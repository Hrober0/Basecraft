using UnityEngine;

public class Planter : MonoBehaviour
{
    private float plantingTime = 12f;
    public int range = 3;

    private Vector2Int treePosition; public Vector2Int TreePosition { get => treePosition; set => treePosition = value; }

    private PlatformBehavior PlatformB;
    private Vector2Int myPos;

    void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();
        myPos = PlatformB.GetTabPos();

        PlatformB.canBeConnectedOut = false;

        PlatformB.taskTime = plantingTime;
        PlatformB.range = range;
        PlatformB.usingGuiType = PlatfotmGUIType.Procesing;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;

        PlatformB.SetAllCanInItem(false);
        PlatformB.itemOnPlatform[(int)Res.Sapling].maxQua = 50;
        PlatformB.itemOnPlatform[(int)Res.Sapling].canIn = true;
        PlatformB.itemOnPlatform[(int)Res.Sapling].canOut = false;
    }
    void Start()
    {
        InvokeRepeating("TryPlantTree", WorldMenager.instance.frequencyOfChecking, WorldMenager.instance.frequencyOfChecking);
    }
    private void TryPlantTree()
    {
        if (PlatformB.itemOnPlatform[(int)Res.Sapling].qua < 1 || PlatformB.working) { return; }
        treePosition = WorldMenager.instance.FindTheNearestObjectOnTerrain(Obj.None, Obj.TerrainFertile, myPos.x, myPos.y, range);
        if (treePosition.x == -1) { return; }

        PlatformB.startTaskTime = WorldMenager.instance.worldTime;
        PlatformB.working = true;
        Invoke("PlantTree", plantingTime);
    }
    private void PlantTree()
    {
        if(WorldMenager.instance.GetSquer(treePosition.x, treePosition.y) == Obj.None)
        {
            PlatformB.AddItem(Res.Sapling, -1);
            TerrainManager.instance.SpawnSapling(treePosition.x, treePosition.y, true);
        }

        PlatformB.working = false;
        TryPlantTree();
    }
}
