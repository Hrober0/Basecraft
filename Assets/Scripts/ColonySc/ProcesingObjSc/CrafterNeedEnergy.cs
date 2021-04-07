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
    public float timeToEndCraft = 0f;

    [Header("Energy")]
    public float needEnergy = 2f;
    private ElectricityUser eleUSc;

    private PlatformBehavior PlatformB;

    private List<ItemRAQ> ItemIn;
    private List<ItemRAQ> ItemOut;

    void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();

        PlatformB.usingGuiType = PlatfotmGUIType.Procesing;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;

        PlatformB.taskTime = craftTime;

        //energy
        eleUSc = gameObject.GetComponent<ElectricityUser>();
        if (eleUSc == null) { Debug.LogError(name + " dont have ElectricityUser script!"); }
        eleUSc.maxEnergyPerSec = needEnergy;
        eleUSc.actCharge = 0f;
        eleUSc.maxCharge = eleUSc.maxEnergyPerSec * 2f;
        ElectricityManager.instance.AddRequester(eleUSc);

        nowUseRecipeNumber = -1;
        SetResToCraft(0);
    }
    void Start()
    {
        InvokeRepeating("TryCraft", WorldMenager.instance.frequencyOfChecking, WorldMenager.instance.frequencyOfChecking);
    }
    private void Update()
    {
        if (PlatformB.working == false) { eleUSc.actEnergyPerSec = 0f; return; }

        float prodPercent = eleUSc.actCharge / eleUSc.maxCharge;
        eleUSc.actEnergyPerSec = prodPercent * needEnergy;
        float percTime = Time.deltaTime * prodPercent;
        float energy = needEnergy * percTime;

        if (eleUSc.actCharge < energy) { PlatformB.working = false; eleUSc.actEnergyPerSec = 0f; return; }

        eleUSc.actCharge -= energy;
        timeToEndCraft -= percTime;
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
            PlatformB.itemOnPlatform[i].maxQua = 0;
            PlatformB.itemOnPlatform[i].canOut = true;
        }

        if (nowUseRecipeNumber == 0)
        {
            PlatformB.UpdateImageR(Res.None);
            PlatformB.working = false;
            PlatformB.SetAllCanInItem(false);

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

        PlatformB.startTaskTime = WorldMenager.instance.worldTime;
        timeToEndCraft = craftTime;
    }

    private void TryCraft()
    {
        if (nowUseRecipeNumber == 0 || PlatformB.working || CanCraftRes() == false) { return; }

        if (timeToEndCraft <= 0f)
        {
            PlatformB.startTaskTime = WorldMenager.instance.worldTime;
            timeToEndCraft = craftTime;
        }
        else
        {
            PlatformB.startTaskTime = WorldMenager.instance.worldTime - (PlatformB.taskTime - timeToEndCraft);
        }
        PlatformB.working = true;
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
        TryCraft();
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
}
