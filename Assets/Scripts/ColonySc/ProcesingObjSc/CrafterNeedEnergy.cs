using System.Collections.Generic;
using UnityEngine;

public class CrafterNeedEnergy : MonoBehaviour
{
    [Header("To set")]
    [SerializeField] private Obj recipeObj = Obj.None;
    [SerializeField] private float speedMultiplayer = 1f;

    [Header("Variables")]
    public int nowUseRecipeNumber;
    private float craftTime = 1f;

    [Header("Energy")]
    [SerializeField] private float needEnergy = 2f;
    private ElectricityUser eleUSc;

    private PlatformBehavior platformB;

    private List<ItemRAQ> itemIn;
    private List<ItemRAQ> itemOut;

    void Awake()
    {
        platformB = gameObject.GetComponent<PlatformBehavior>();

        platformB.usingGuiType = PlatfotmGUIType.Procesing;
        platformB.itemSendingType = PlatformItemSendingType.Procesing;

        platformB.taskTime = craftTime;

        nowUseRecipeNumber = -1;
        SetResToCraft(0);

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

    public void SetResToCraft(int recipeNuber)
    {
        if (nowUseRecipeNumber == recipeNuber) { return; }

        if (recipeNuber > AllRecipes.instance.GetCraftRecipes(recipeObj).Count) { return; }

        nowUseRecipeNumber = recipeNuber;

        for (int i = 0; i < platformB.itemOnPlatform.Length; i++)
        {
            platformB.itemOnPlatform[i].maxQua = 0;
            platformB.itemOnPlatform[i].canOut = true;
        }

        if (nowUseRecipeNumber == 0)
        {
            platformB.UpdateImageR(Res.None);
            platformB.working = false;
            platformB.SetAllCanInItem(false);

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

        platformB.startTaskTime = WorldMenager.instance.worldTime;
        platformB.timeToEndCraft = craftTime;
    }

    private void TryCraft()
    {
        if (nowUseRecipeNumber == 0 || platformB.working || CanCraftRes() == false)
        {
            Invoke(nameof(TryCraft), 1); 
            return;
        }

        if (platformB.timeToEndCraft <= 0f)
        {
            platformB.startTaskTime = WorldMenager.instance.worldTime;
            platformB.timeToEndCraft = craftTime;
        }
        else
        {
            platformB.startTaskTime = WorldMenager.instance.worldTime - (platformB.taskTime - platformB.timeToEndCraft);
        }
        platformB.working = true;
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
}
