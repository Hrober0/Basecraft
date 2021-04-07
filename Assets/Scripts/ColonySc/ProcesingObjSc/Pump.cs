using UnityEngine;

public class Pump : MonoBehaviour
{
    private float taskTime = 3f;
    private Res retRes;

    private PlatformBehavior PlatformB;

    void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();

        PlatformB.usingGuiType = PlatfotmGUIType.Procesing;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;

        Obj terr = WorldMenager.instance.GetTerrainTile((int)(transform.position.x / 10), (int)(transform.position.y / 10));
        if (terr == Obj.OilSource) { retRes = Res.BottleOil; }
        else if (terr == Obj.WaterSource) { retRes = Res.BottleWater; }
        else { Debug.Log("Error! pump was placed on wrong terrain, detected: " + terr); return; }

        PlatformB.taskTime = taskTime;
        PlatformB.itemOnPlatform[(int)Res.BottleEmpty].maxQua = 10;
        PlatformB.itemOnPlatform[(int)Res.BottleEmpty].canIn = true;
        PlatformB.itemOnPlatform[(int)Res.BottleEmpty].canOut = false;
        PlatformB.itemOnPlatform[(int)retRes].maxQua = 10;
    }
    void Start()
    {
        Invoke("TryCraft", WorldMenager.instance.frequencyOfChecking);
    }
    private void TryCraft()
    {
        if (PlatformB.working || PlatformB.itemOnPlatform[(int)Res.BottleEmpty].qua < 1 || PlatformB.itemOnPlatform[(int)retRes].qua >= 10)
        { Invoke("TryCraft", WorldMenager.instance.frequencyOfChecking); return; }

        PlatformB.startTaskTime = WorldMenager.instance.worldTime;
        PlatformB.working = true;
        Invoke("Craft", taskTime);
    }
    private void Craft()
    {
        PlatformB.AddItem(Res.BottleEmpty, -1);
        PlatformB.AddItem(retRes, 1, true);
        PlatformB.working = false;
        TryCraft();
    }
}
