using UnityEngine;

public class WoodCuter : MonoBehaviour
{
    private float cuttingTime = 15f;
    public int range = 4;

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

        platformB.usingGuiType = PlatfotmGUIType.Procesing;
        platformB.itemSendingType = PlatformItemSendingType.Procesing;

        platformB.canBeConectedIn = false;
        platformB.SetAllCanInItem(false);

        platformB.taskTime = cuttingTime;
        platformB.range = range;

        platformB.itemOnPlatform[(int)Res.Sapling].maxQua = 10;
        platformB.itemOnPlatform[(int)Res.Wood].maxQua = 20;

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
        Invoke(nameof(TryCutTree), 1);
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
            CuttTree();
        }
    }
    private void TryCutTree()
    {
        Vector2Int htp = FindTreeToCut();
        if (htp.x == -1)
        {
            Invoke(nameof(TryCutTree), 1);
            return;
        }

        treePosition = htp;
        platformB.startTaskTime = WorldMenager.instance.worldTime;
        platformB.taskTime = cuttingTime;
        platformB.timeToEndCraft = cuttingTime;
        platformB.working = true;
    }
    private void CuttTree()
    {
        Transform treeT = WorldMenager.instance.GetTransforOfObj(treePosition.x, treePosition.y);
        if (treeT != null)
        {
            int saplingDrop;
            int los = Random.Range(0, 11);
            if (los >= 7) { saplingDrop = 2; } else { saplingDrop = 1; }
            int woodDrop = Random.Range(2, 5);
            platformB.AddItem(Res.Sapling, saplingDrop, true);
            platformB.AddItem(Res.Wood, woodDrop, true);

            WorldMenager.instance.RemoveObjFromGO(treeT.gameObject, treePosition.x, treePosition.y);
            TryCutTree();
        }
        else
        {
            //Debug.Log("nie znalazłem drzewa, próbuję jeszcze raz");
            Vector2Int htp = FindTreeToCut();
            if (htp.x != -1)
            {
                treePosition = htp;
                CuttTree();
            }
        }

        platformB.working = false;
    }
    private Vector2Int FindTreeToCut()
    {
        if(platformB.itemOnPlatform[0].qua <= 0 || platformB.working) return new Vector2Int(-1, -1);

        if (platformB.itemOnPlatform[(int)Res.Sapling].qua > platformB.itemOnPlatform[(int)Res.Sapling].maxQua
         || platformB.itemOnPlatform[(int)Res.Wood].qua > platformB.itemOnPlatform[(int)Res.Wood].maxQua) return new Vector2Int(-1, -1);

        Vector2Int htp = WorldMenager.instance.FindTheNearestObject(Obj.Tree, myPos.x, myPos.y, range);
        if (treePosition.x == -1) return new Vector2Int(-1, -1);
        
        return htp;
    }
}
