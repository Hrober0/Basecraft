﻿using UnityEngine;

public class Pump : MonoBehaviour
{
    private float taskTime = 3f;
    private Res retRes;

    [Header("Energy")]
    [SerializeField] private float needEnergy = 2f;
    private ElectricityUser eleUSc;

    private PlatformBehavior platformB;

    void Awake()
    {
        platformB = gameObject.GetComponent<PlatformBehavior>();

        platformB.usingGuiType = PlatfotmGUIType.Procesing;
        platformB.itemSendingType = PlatformItemSendingType.Procesing;

        Obj terr = WorldMenager.instance.GetTerrainTile((int)(transform.position.x / 10), (int)(transform.position.y / 10));
        if (terr == Obj.OilSource) { retRes = Res.BottleOil; }
        else if (terr == Obj.WaterSource) { retRes = Res.BottleWater; }
        else { Debug.Log("Error! pump was placed on wrong terrain, detected: " + terr); return; }

        platformB.taskTime = taskTime;
        platformB.itemOnPlatform[(int)Res.BottleEmpty].maxQua = 10;
        platformB.itemOnPlatform[(int)Res.BottleEmpty].canIn = true;
        platformB.itemOnPlatform[(int)Res.BottleEmpty].canOut = false;
        platformB.itemOnPlatform[(int)retRes].maxQua = 10;

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
        Invoke(nameof(TryCraft), 1);
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
            Craft();
        }
    }
    private void TryCraft()
    {
        if (platformB.working || platformB.itemOnPlatform[(int)Res.BottleEmpty].qua < 1 || platformB.itemOnPlatform[(int)retRes].qua >= 10)
        {
            Invoke(nameof(TryCraft), 1);
            return;
        }

        platformB.startTaskTime = WorldMenager.instance.worldTime;
        platformB.taskTime = taskTime;
        platformB.timeToEndCraft = taskTime;
        platformB.working = true;
    }
    private void Craft()
    {
        platformB.AddItem(Res.BottleEmpty, -1);
        platformB.AddItem(retRes, 1, true);
        platformB.working = false;
        TryCraft();
    }
}
