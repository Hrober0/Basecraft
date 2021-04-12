using UnityEngine;

public class Quarry : MonoBehaviour
{
    [SerializeField] private Transform rotorTrans = null;
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Energy")]
    [SerializeField] private float needEnergy = 2f;
    private ElectricityUser eleUSc;

    private readonly float miningTime = 6f;
    private PlatformBehavior platformB;
    private Res miningRes;

    void Awake()
    {
        platformB = gameObject.GetComponent<PlatformBehavior>();

        miningRes = Res.None;
        Vector2Int terpos = platformB.GetTabPos();
        switch (WorldMenager.instance.GetTerrainTile(terpos.x, terpos.y))
        {
            case Obj.StoneOre: miningRes = Res.StoneOre; break;
            case Obj.CopperOre: miningRes = Res.CopperOreCtm; break;
            case Obj.IronOre: miningRes = Res.IronOre; break;
            case Obj.SandOre: miningRes = Res.Sand; break;
            case Obj.CoalOre: miningRes = Res.Coal; break;
        }

        platformB.usingGuiType = PlatfotmGUIType.Procesing;
        platformB.itemSendingType = PlatformItemSendingType.Procesing;
        platformB.canBeConectedIn = false;
        platformB.SetAllCanInItem(false);
        platformB.taskTime = miningTime;
        platformB.itemOnPlatform[(int)miningRes].maxQua = 20;

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
        Invoke(nameof(TryMine), 1);
    }

    private void Update()
    {
        if (platformB.working == false) return;

        float prodPercent = eleUSc.actCharge / eleUSc.maxCharge;
        float percTime = Time.deltaTime * prodPercent;
        float energy = needEnergy * percTime;

        rotorTrans.Rotate(Vector3.forward * percTime * rotationSpeed);

        if (eleUSc.actCharge < energy) { platformB.working = false; return; }

        eleUSc.actCharge -= energy;
        platformB.timeToEndCraft -= percTime;
        if (platformB.timeToEndCraft <= 0)
        {
            Mine();
        }
    }
    private void TryMine()
    {
        if (platformB.working || platformB.itemOnPlatform[(int)miningRes].qua > platformB.itemOnPlatform[(int)miningRes].maxQua)
        {
            Invoke(nameof(TryMine), 1);
            return;
        }

        platformB.startTaskTime = WorldMenager.instance.worldTime;
        platformB.taskTime = miningTime;
        platformB.timeToEndCraft = miningTime;
        platformB.working = true;
    }


    private void Mine()
    {
        platformB.AddItem(miningRes, 1, true);
        platformB.working = false;
        TryMine();
    }
}
