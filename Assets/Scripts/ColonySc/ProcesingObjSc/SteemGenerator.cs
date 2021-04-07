using System.Collections.Generic;
using UnityEngine;

public class SteemGenerator : MonoBehaviour
{
    public Res useFuel = Res.None;
    [Range(0f, 1f)]
    public float percRemFuel = 0f;
    public int subFuel = 100;
    private float AvaTimeToUpdateFuel = 0f;
    private int maxQuaOfFuel = 10;

    [Range(0f, 1f)]
    public float percRemWater = 0f;
    public bool needWater = false;

    public float production = 10f;

    private PlatformBehavior PlatformB;
    private ElectricityUser eleUSc;

    private void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();
        PlatformB.usingGuiType = PlatfotmGUIType.PowerGenerator;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;
        PlatformB.SetAllCanInItem(false);

        if (needWater)
        { 
            PlatformB.itemOnPlatform[(int)Res.BottleWater].maxQua = 10;
            PlatformB.itemOnPlatform[(int)Res.BottleWater].canIn = true;
            PlatformB.itemOnPlatform[(int)Res.BottleWater].canOut = false;
            PlatformB.itemOnPlatform[(int)Res.BottleEmpty].maxQua = 10;
        }
        else
        {
            PlatformB.canBeConnectedOut = false;
        }

        //energy
        eleUSc = gameObject.GetComponent<ElectricityUser>();
        eleUSc.maxEnergyPerSec = production;
        eleUSc.actCharge = 0f;
        eleUSc.maxCharge = eleUSc.maxEnergyPerSec * 2f;
        ElectricityManager.instance.AddGenerator(eleUSc);
    }

    void Update()
    {
        //check fuel
        if (percRemFuel <= 0f)
        {
            if (AvaTimeToUpdateFuel > WorldMenager.instance.worldTime)
            {
                PlatformB.working = false; eleUSc.actEnergyPerSec = 0f; return;
            }
            AvaTimeToUpdateFuel = WorldMenager.instance.worldTime + WorldMenager.instance.frequencyOfChecking;
            UpdateFuel();
            if (percRemFuel <= 0f) { PlatformB.working = false; eleUSc.actEnergyPerSec = 0f; return; }
        }

        //check water
        if (needWater)
        {
            if(PlatformB.itemOnPlatform[(int)Res.BottleEmpty].qua >= 10) { PlatformB.working = false; eleUSc.actEnergyPerSec = 0f; return; }
            if (percRemWater <= 0f)
            {
                if (PlatformB.itemOnPlatform[(int)Res.BottleWater].qua > 0)
                {
                    PlatformB.AddItem(Res.BottleWater, -1);
                    PlatformB.AddItem(Res.BottleEmpty, 1);
                    percRemWater = 1f;
                }
                if (percRemWater <= 0f) { PlatformB.working = false; eleUSc.actEnergyPerSec = 0f; return; }
            }
        }

        //check energy
        if (eleUSc.actCharge > eleUSc.maxCharge || eleUSc.maxCharge <= 0f) { PlatformB.working = false; eleUSc.actEnergyPerSec = 0f; return; }

        //go
        float prodPercent = (1 - eleUSc.actCharge / eleUSc.maxCharge) * 2;
        if (prodPercent < 0.1f) { prodPercent = 0.1f; }
        else if (prodPercent > 1f) { prodPercent = 1f; }
        eleUSc.actEnergyPerSec = prodPercent * production;
        float multiplayer = Time.deltaTime * prodPercent;
        eleUSc.actCharge += production * multiplayer;
        if (needWater)
        {
            PlatformB.working = true;
            percRemWater -= 10.00f / 200 * multiplayer;
            percRemFuel -= 10.00f / subFuel * multiplayer;
        }
        else
        {
            PlatformB.working = true;
            percRemFuel -= 10.00f / subFuel * multiplayer;
        } 
    }

    private void UpdateFuel()
    {
        if (percRemFuel > 0f) { return; }
        if (useFuel == Res.None || PlatformB.itemOnPlatform[(int)useFuel].qua <= 0)
        {
            Fuel foundFuel = FindAvaFuel();
            if (foundFuel == null)
            {
                //Debug.Log("dont found any fuel");
                for (int i = 0; i < AllRecipes.instance.fuelList.Count; i++)
                {
                    Res fuel = AllRecipes.instance.fuelList[i].fuelRes;
                    PlatformB.itemOnPlatform[(int)fuel].canIn = true;
                    PlatformB.itemOnPlatform[(int)fuel].maxQua = maxQuaOfFuel;
                    PlatformB.itemOnPlatform[(int)fuel].canOut = false;
                }
                useFuel = Res.None;
                percRemFuel = 0f;
            }
            else
            {
                //Debug.Log("found new fuel" + foundFuel.fuelRes);
                for (int i = 0; i < AllRecipes.instance.fuelList.Count; i++)
                {
                    Res fuel = AllRecipes.instance.fuelList[i].fuelRes;
                    PlatformB.itemOnPlatform[(int)fuel].maxQua = 0;
                    PlatformB.itemOnPlatform[(int)fuel].canIn = false;
                    PlatformB.itemOnPlatform[(int)fuel].canOut = true;
                }
                Res foundFuelRes = foundFuel.fuelRes;
                PlatformB.itemOnPlatform[(int)foundFuelRes].maxQua = maxQuaOfFuel;
                PlatformB.itemOnPlatform[(int)foundFuelRes].canIn = true;
                PlatformB.itemOnPlatform[(int)foundFuelRes].canOut = false;

                useFuel = foundFuel.fuelRes;
                subFuel = foundFuel.energyValue;
                if (subFuel <= 0) { subFuel = 1; }
                PlatformB.AddItem(useFuel, -1);
                percRemFuel = 1f;
            }
        }
        else
        {
            //Debug.Log("uzupełniam paliwo");
            PlatformB.AddItem(useFuel, -1);
            percRemFuel = 1f;
        }

        Fuel FindAvaFuel()
        {
            for (int i = AllRecipes.instance.fuelList.Count - 1; i >= 0; i--)
            {
                Res fuel = AllRecipes.instance.fuelList[i].fuelRes;
                if (PlatformB.itemOnPlatform[(int)fuel].qua > 0) { return AllRecipes.instance.fuelList[i]; }
            }
            return null;
        }
    }
}
