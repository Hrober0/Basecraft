using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrafterNeedFuel : MonoBehaviour
{
    [Header("To set")]
    [SerializeField] private Obj recipeObj = Obj.None;
    [SerializeField] private float speedMultiplayer = 1f;

    [Header("Variables")]
    public int nowUseRecipeNumber;
    private float craftTime = 1f;

    [Header("Fuel")]
    public Res useFuel = Res.None;
    [Range(0f, 1f)]
    public float percRemFuel = 0f;
    public int subFuel = 100;
    private float nextAvaTimeToUpdateFuel = 0f;
    public readonly int maxQuaOfFuel = 5;

    private PlatformBehavior platformB;

    private List<ItemRAQ> itemIn;
    private List<ItemRAQ> itemOut;

    void Awake()
    {
        platformB = gameObject.GetComponent<PlatformBehavior>();

        platformB.usingGuiType = PlatfotmGUIType.Procesing;
        platformB.itemSendingType = PlatformItemSendingType.Procesing;

        platformB.taskTime = craftTime;

        platformB.SetAllCanInItem(false);

        nowUseRecipeNumber = -1;
        SetResToCraft(0);
    }
    void Start()
    {
        Invoke(nameof(TryCraft), 1);
    }
    private void Update()
    {
        nextAvaTimeToUpdateFuel -= Time.deltaTime;
        if (percRemFuel <= 0f)
        {
            if (nextAvaTimeToUpdateFuel > WorldMenager.instance.worldTime) return;
            nextAvaTimeToUpdateFuel = WorldMenager.instance.worldTime + WorldMenager.instance.frequencyOfChecking;
            UpdateFuel();
            if (percRemFuel <= 0f) { platformB.working = false; return; }
            else if (CanCraftRes()) { platformB.working = true; platformB.startTaskTime = WorldMenager.instance.worldTime - (platformB.taskTime - platformB.timeToEndCraft); }
        }

        if (platformB.working == false) return;
        percRemFuel -= 10.00f / subFuel * Time.deltaTime;
        platformB.timeToEndCraft -= Time.deltaTime;
        if (platformB.timeToEndCraft <= 0)
        {
            Craft();
        }
    }

    public void SetResToCraft(int recipeNuber)
    {
        if (nowUseRecipeNumber == recipeNuber) { return; }

        if (recipeNuber > AllRecipes.instance.GetCraftRecipes(recipeObj).Count) { return; }

        nowUseRecipeNumber = recipeNuber;

        for (int i = 0; i < platformB.itemOnPlatform.Length; i++)
        {
            if ((Res)i != useFuel)
            {
                platformB.itemOnPlatform[i].maxQua = 0;
                platformB.itemOnPlatform[i].canOut = true;
            }
        }

        if (nowUseRecipeNumber == 0)
        {
            platformB.UpdateImageR(Res.None);
            platformB.working = false;
            platformB.SetAllCanInItem(false);
            if (useFuel != Res.None)
            {
                platformB.itemOnPlatform[(int)useFuel].canIn = true;
                platformB.itemOnPlatform[(int)useFuel].maxQua = maxQuaOfFuel;
            }
            else
            {
                for (int i = 0; i < AllRecipes.instance.fuelList.Count; i++)
                {
                    Res fuel = AllRecipes.instance.fuelList[i].fuelRes;
                    platformB.itemOnPlatform[(int)fuel].canIn = true;
                    platformB.itemOnPlatform[(int)fuel].maxQua = maxQuaOfFuel;
                }
            }
            platformB.UpdateAvalibleResList();
            itemIn = new List<ItemRAQ>();
            itemOut = new List<ItemRAQ>();
            return;
        }

        platformB.canGetRes = true;

        CraftRecipe nowUseRecipe = AllRecipes.instance.GetCraftRecipes(recipeObj)[nowUseRecipeNumber - 1];

        platformB.UpdateImageR(nowUseRecipe.ItemOut[0].res);

        for (int i = 0; i < nowUseRecipe.ItemIn.Count; i++)
        {
            int resIndex = (int)nowUseRecipe.ItemIn[i].res;
            int qua = nowUseRecipe.ItemIn[i].qua;
            platformB.itemOnPlatform[resIndex].maxQua = qua * 2;
            platformB.itemOnPlatform[resIndex].canIn = true;
            platformB.itemOnPlatform[resIndex].canOut = false;
        }
        itemIn = nowUseRecipe.ItemIn;
        for (int i = 0; i < nowUseRecipe.ItemOut.Count; i++)
        {
            int resIndex = (int)nowUseRecipe.ItemOut[i].res;
            int qua = nowUseRecipe.ItemOut[i].qua;
            platformB.itemOnPlatform[resIndex].maxQua = qua * 2;
        }
        itemOut = nowUseRecipe.ItemOut;

        platformB.UpdateAvalibleResList();
        craftTime = nowUseRecipe.exeTime / speedMultiplayer;
        platformB.taskTime = craftTime;
    }

    private void TryCraft()
    {
        if (nowUseRecipeNumber == 0 || platformB.working || CanCraftRes() == false || percRemFuel <= 0f)
        {
            Invoke(nameof(TryCraft), 1); 
            return; 
        }
        platformB.startTaskTime = WorldMenager.instance.worldTime;
        platformB.working = true;
        platformB.timeToEndCraft = craftTime;
    }
    private void Craft()
    {
        for (int i = 0; i < itemIn.Count; i++)
        {
            platformB.AddItem(itemIn[i].res, -itemIn[i].qua);
        }
        for (int i = 0; i < itemOut.Count; i++)
        {
            platformB.AddItem(itemOut[i].res, itemOut[i].qua, true);
        }

        platformB.working = false;
        TryCraft();
    }

    private bool CanCraftRes()
    {
        for (int i = 0; i < itemOut.Count; i++)
        {
            if (platformB.itemOnPlatform[(int)itemOut[i].res].qua + itemOut[i].qua > platformB.itemOnPlatform[(int)itemOut[i].res].maxQua)
            { return false; }
        }
        for (int i = 0; i < itemIn.Count; i++)
        {
            if (platformB.itemOnPlatform[(int)itemIn[i].res].qua < itemIn[i].qua)
            { return false; }
        }
        return true;
    }

    private void UpdateFuel()
    {
        if (percRemFuel > 0f) { return; }
        if (useFuel == Res.None || platformB.itemOnPlatform[(int)useFuel].qua <= 0)
        {
            Fuel foundFuel = FindAvaFuel();
            if (foundFuel == null)
            {
                //Debug.Log("dont found any fuel");
                for (int i = 0; i < AllRecipes.instance.fuelList.Count; i++)
                {
                    Res fuel = AllRecipes.instance.fuelList[i].fuelRes;
                    platformB.itemOnPlatform[(int)fuel].canIn = true;
                    if (IsFuelUseingInCrafting(fuel) == false)
                    {
                        platformB.itemOnPlatform[(int)fuel].maxQua = maxQuaOfFuel;
                        platformB.itemOnPlatform[(int)fuel].canOut = false;
                    }
                }
                useFuel = Res.None;
                percRemFuel = 0f;
            }
            else
            {
                //Debug.Log("found new fuel" + foundFuel.fuelRes);
                SetItemOnPlatformForFuel(foundFuel.fuelRes);

                useFuel = foundFuel.fuelRes;
                subFuel = foundFuel.energyValue;
                if (subFuel <= 0) { subFuel = 1; }
                platformB.AddItem(useFuel, -1);
                percRemFuel = 1f;
            }
        }
        else
        {
            //Debug.Log("uzupełniam paliwo");
            platformB.AddItem(useFuel, -1);
            percRemFuel = 1f;
        }

        Fuel FindAvaFuel()
        {
            int fuleQua = AllRecipes.instance.fuelList.Count - 1;
            for (int i = fuleQua; i >= 0; i--)
            {
                Res fuel = AllRecipes.instance.fuelList[i].fuelRes;
                if (platformB.itemOnPlatform[(int)fuel].qua > 0) { return AllRecipes.instance.fuelList[i]; }
            }
            return null;
        }
    }
    public void SetItemOnPlatformForFuel(Res foundFuelRes)
    {
        foreach (Fuel fuelC in AllRecipes.instance.fuelList)
        {
            Res fuel = fuelC.fuelRes;
            if (IsFuelUseingInCrafting(fuel) == false)
            {
                platformB.RemoveResFromAvalibleResList(fuel);
                platformB.itemOnPlatform[(int)fuel].maxQua = 0;
                platformB.itemOnPlatform[(int)fuel].canIn = false;
                platformB.itemOnPlatform[(int)fuel].canOut = true;
            }
        }

        platformB.itemOnPlatform[(int)foundFuelRes].canIn = true;
        if (IsFuelUseingInCrafting(foundFuelRes) == false)
        {
            platformB.itemOnPlatform[(int)foundFuelRes].maxQua = maxQuaOfFuel;
            platformB.itemOnPlatform[(int)foundFuelRes].canOut = false;
        }
    }
    private bool IsFuelUseingInCrafting(Res fuel)
    {
        for (int i = 0; i < itemIn.Count; i++)
        {
            if (itemIn[i].res == fuel) { return true; }
        }
        for (int i = 0; i < itemOut.Count; i++)
        {
            if (itemOut[i].res == fuel) { return true; }
        }
        return false;
    }
}
