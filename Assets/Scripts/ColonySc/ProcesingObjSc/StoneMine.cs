using UnityEngine;

public class StoneMine : MonoBehaviour
{
    private float miningTime = 6f;
    private PlatformBehavior PlatformB;
    private Res miningRes;

    void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();

        miningRes = Res.None;
        Vector2Int terpos = PlatformB.GetTabPos();
        switch (WorldMenager.instance.GetTerrainTile(terpos.x, terpos.y))
        {
            case Obj.StoneOre: miningRes = Res.StoneOre; break;
            case Obj.CopperOre: miningRes = Res.CopperOreCtm; break;
            case Obj.IronOre: miningRes = Res.IronOre; break;
            case Obj.SandOre: miningRes = Res.Sand; break;
            case Obj.CoalOre: miningRes = Res.Coal; break;
        }

        PlatformB.usingGuiType = PlatfotmGUIType.Procesing;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;
        PlatformB.canBeConectedIn = false;
        PlatformB.SetAllCanInItem(false);
        PlatformB.taskTime = miningTime;
        PlatformB.itemOnPlatform[(int)miningRes].maxQua = 20;
    }
    void Start()
    {
        InvokeRepeating("TryMine", WorldMenager.instance.frequencyOfChecking, WorldMenager.instance.frequencyOfChecking);
    }
    private void TryMine()
    {
        if (PlatformB.working) { return; }
        if (PlatformB.itemOnPlatform[(int)miningRes].qua > PlatformB.itemOnPlatform[(int)miningRes].maxQua) { return; }

        PlatformB.startTaskTime = WorldMenager.instance.worldTime;
        PlatformB.taskTime = miningTime;
        PlatformB.working = true;
        Invoke("Mine", miningTime);
    }
    private void Mine()
    {
        PlatformB.AddItem(miningRes, 1, true);
        PlatformB.working = false;
    }
}
