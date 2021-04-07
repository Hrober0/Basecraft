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
    public float timeToEndCraft = 0f;

    [Header("Fuel")]
    public Res useFuel = Res.None;
    [Range(0f, 1f)]
    public float percRemFuel = 0f;
    public int subFuel = 100;
    private float nextAvaTimeToUpdateFuel = 0f;
    public readonly int maxQuaOfFuel = 5;

    private PlatformBehavior PlatformB;

    private List<ItemRAQ> ItemIn;
    private List<ItemRAQ> ItemOut;

    void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();

        PlatformB.usingGuiType = PlatfotmGUIType.Procesing;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;

        PlatformB.taskTime = craftTime;

        PlatformB.SetAllCanInItem(false);

        nowUseRecipeNumber = -1;
        SetResToCraft(0);
    }
    void Start()
    {
        InvokeRepeating("TryCraft", WorldMenager.instance.frequencyOfChecking, WorldMenager.instance.frequencyOfChecking);
    }
    private void Update()
    {
        nextAvaTimeToUpdateFuel -= Time.deltaTime;
        if (percRemFuel <= 0f)
        {
            if (nextAvaTimeToUpdateFuel > WorldMenager.instance.worldTime)
            {
                return;
            }
            nextAvaTimeToUpdateFuel = WorldMenager.instance.worldTime + WorldMenager.instance.frequencyOfChecking;
            UpdateFuel();
            if (percRemFuel <= 0f) { PlatformB.working = false; return; }
            else if (CanCraftRes()) { PlatformB.working = true; PlatformB.startTaskTime = WorldMenager.instance.worldTime - (PlatformB.taskTime - timeToEndCraft); }
        }

        if (PlatformB.working == false) { return; }
        percRemFuel -= 10.00f / subFuel * Time.deltaTime;
        timeToEndCraft -= Time.deltaTime;
        if (timeToEndCraft <= 0)
        {
            Craft();
        }
    }

    public void SetResToCraft(int recipeNuber)
    {
        if (nowUseRecipeNumber == recipeNuber) { return; }

        if (recipeNuber > AllRecipes.instance.GetCraftRecipes(recipeObj).Count) { return; }

        nowUseRecipeNumber = recipeNuber;

        for (int i = 0; i < PlatformB.itemOnPlatform.Length; i++)
        {
            if ((Res)i != useFuel)
            {
                PlatformB.itemOnPlatform[i].maxQua = 0;
                PlatformB.itemOnPlatform[i].canOut = true;
            }
        }

        if (nowUseRecipeNumber == 0)
        {
            PlatformB.UpdateImageR(Res.None);
            PlatformB.working = false;
            PlatformB.SetAllCanInItem(false);
            if (useFuel != Res.None)
            {
                PlatformB.itemOnPlatform[(int)useFuel].canIn = true;
                PlatformB.itemOnPlatform[(int)useFuel].maxQua = maxQuaOfFuel;
            }
            else
            {
                for (int i = 0; i < AllRecipes.instance.fuelList.Count; i++)
                {
                    Res fuel = AllRecipes.instance.fuelList[i].fuelRes;
                    PlatformB.itemOnPlatform[(int)fuel].canIn = true;
                    PlatformB.itemOnPlatform[(int)fuel].maxQua = maxQuaOfFuel;
                }
            }
            PlatformB.UpdateAvalibleResList();
            ItemIn = new List<ItemRAQ>();
            ItemOut = new List<ItemRAQ>();
            return;
        }

        PlatformB.canGetRes = true;

        CraftRecipe nowUseRecipe = AllRecipes.instance.GetCraftRecipes(recipeObj)[nowUseRecipeNumber - 1];

        PlatformB.UpdateImageR(nowUseRecipe.ItemOut[0].res);

        for (int i = 0; i < nowUseRecipe.ItemIn.Count; i++)
        {
            int resIndex = (int)nowUseRecipe.ItemIn[i].res;
            int qua = nowUseRecipe.ItemIn[i].qua;
            PlatformB.itemOnPlatform[resIndex].maxQua = qua * 2;
            PlatformB.itemOnPlatform[resIndex].canIn = true;
            PlatformB.itemOnPlatform[resIndex].canOut = false;
        }
        ItemIn = nowUseRecipe.ItemIn;
        for (int i = 0; i < nowUseRecipe.ItemOut.Count; i++)
        {
            int resIndex = (int)nowUseRecipe.ItemOut[i].res;
            int qua = nowUseRecipe.ItemOut[i].qua;
            PlatformB.itemOnPlatform[resIndex].maxQua = qua * 2;
        }
        ItemOut = nowUseRecipe.ItemOut;

        PlatformB.UpdateAvalibleResList();
        craftTime = nowUseRecipe.exeTime / speedMultiplayer;
        PlatformB.taskTime = craftTime;
    }

    private void TryCraft()
    {
        if (nowUseRecipeNumber == 0 || PlatformB.working || CanCraftRes() == false || percRemFuel <= 0f) { return; }
        PlatformB.startTaskTime = WorldMenager.instance.worldTime;
        PlatformB.working = true;
        timeToEndCraft = craftTime;
    }
    private void Craft()
    {
        for (int i = 0; i < ItemIn.Count; i++)
        {
            PlatformB.AddItem(ItemIn[i].res, -ItemIn[i].qua);
        }
        for (int i = 0; i < ItemOut.Count; i++)
        {
            PlatformB.AddItem(ItemOut[i].res, ItemOut[i].qua, true);
        }

        PlatformB.working = false;
    }

    private bool CanCraftRes()
    {
        for (int i = 0; i < ItemOut.Count; i++)
        {
            if (PlatformB.itemOnPlatform[(int)ItemOut[i].res].qua + ItemOut[i].qua > PlatformB.itemOnPlatform[(int)ItemOut[i].res].maxQua)
            { return false; }
        }
        for (int i = 0; i < ItemIn.Count; i++)
        {
            if (PlatformB.itemOnPlatform[(int)ItemIn[i].res].qua < ItemIn[i].qua)
            { return false; }
        }
        return true;
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
                    if (IsFuelUseingInCrafting(fuel) == false)
                    {
                        PlatformB.itemOnPlatform[(int)fuel].maxQua = maxQuaOfFuel;
                        PlatformB.itemOnPlatform[(int)fuel].canOut = false;
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
            int fuleQua = AllRecipes.instance.fuelList.Count - 1;
            for (int i = fuleQua; i >= 0; i--)
            {
                Res fuel = AllRecipes.instance.fuelList[i].fuelRes;
                if (PlatformB.itemOnPlatform[(int)fuel].qua > 0) { return AllRecipes.instance.fuelList[i]; }
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
                PlatformB.RemoveResFromAvalibleResList(fuel);
                PlatformB.itemOnPlatform[(int)fuel].maxQua = 0;
                PlatformB.itemOnPlatform[(int)fuel].canIn = false;
                PlatformB.itemOnPlatform[(int)fuel].canOut = true;
            }
        }

        PlatformB.itemOnPlatform[(int)foundFuelRes].canIn = true;
        if (IsFuelUseingInCrafting(foundFuelRes) == false)
        {
            PlatformB.itemOnPlatform[(int)foundFuelRes].maxQua = maxQuaOfFuel;
            PlatformB.itemOnPlatform[(int)foundFuelRes].canOut = false;
        }
    }
    private bool IsFuelUseingInCrafting(Res fuel)
    {
        for (int i = 0; i < ItemIn.Count; i++)
        {
            if (ItemIn[i].res == fuel) { return true; }
        }
        for (int i = 0; i < ItemOut.Count; i++)
        {
            if (ItemOut[i].res == fuel) { return true; }
        }
        return false;
    }
}
