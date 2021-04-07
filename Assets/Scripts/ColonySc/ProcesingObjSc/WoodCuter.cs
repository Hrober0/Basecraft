using UnityEngine;

public class WoodCuter : MonoBehaviour
{
    private float cuttingTime = 15f;
    public int range = 4;

    private Vector2Int treePosition; public Vector2Int TreePosition { get => treePosition; set => treePosition = value; }

    private PlatformBehavior PlatformB;
    private Vector2Int myPos;

    void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();

        myPos = PlatformB.GetTabPos();

        PlatformB.usingGuiType = PlatfotmGUIType.Procesing;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;

        PlatformB.canBeConectedIn = false;
        PlatformB.SetAllCanInItem(false);

        PlatformB.taskTime = cuttingTime;
        PlatformB.range = range;

        PlatformB.itemOnPlatform[(int)Res.Sapling].maxQua = 10;
        PlatformB.itemOnPlatform[(int)Res.Wood].maxQua = 20;
    }
    void Start()
    {
        InvokeRepeating("TryCutTree", WorldMenager.instance.frequencyOfChecking, WorldMenager.instance.frequencyOfChecking);
    }

    private void TryCutTree()
    {
        Vector2Int htp = CanCutTree();
        if (htp.x == -1) { return; }

        treePosition = htp;
        PlatformB.startTaskTime = WorldMenager.instance.worldTime;
        PlatformB.working = true;
        Invoke("CuttTree", cuttingTime);
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
            PlatformB.AddItem(Res.Sapling, saplingDrop, true);
            PlatformB.AddItem(Res.Wood, woodDrop, true);

            WorldMenager.instance.RemoveObjFromGO(treeT.gameObject, treePosition.x, treePosition.y);
        }
        else
        {
            //Debug.Log("nie znalazłem drzewa, próbuję jeszcze raz");
            Vector2Int htp = CanCutTree();
            if (htp.x != -1)
            {
                treePosition = htp;
                CanCutTree();
            }
        }

        PlatformB.working = false;
    }
    private Vector2Int CanCutTree()
    {
        if(PlatformB.itemOnPlatform[0].qua <= 0 || PlatformB.working) { return new Vector2Int(-1, -1); }

        if (PlatformB.itemOnPlatform[(int)Res.Sapling].qua > PlatformB.itemOnPlatform[(int)Res.Sapling].maxQua
         || PlatformB.itemOnPlatform[(int)Res.Wood].qua > PlatformB.itemOnPlatform[(int)Res.Wood].maxQua) { return new Vector2Int(-1, -1); }

        Vector2Int htp = WorldMenager.instance.FindTheNearestObject(Obj.Tree, myPos.x, myPos.y, range);
        if (treePosition.x == -1) { return new Vector2Int(-1, -1); }
        
        return htp;
    }
}
