using System.Collections.Generic;
using UnityEngine;

public class Farm : MonoBehaviour
{
    private Res plantingRes = Res.None;
    private Vector2Int foundPlaceToPlant; public Vector2Int FoundPlaceToPlant { get => foundPlaceToPlant; set => foundPlaceToPlant = value; }
    private Vector2Int foundPlaceToCollect; public Vector2Int FoundPlaceToCollect { get => foundPlaceToCollect; set => foundPlaceToCollect = value; }

    private float plantingTime = 10f;
    public int range = 3;
    public int nowUseRecipeNumber = 0;

    private List<ItemRAQ> ItemIn;
    private List<ItemRAQ> ItemOut;

    private PlatformBehavior platformB;
    private Vector2Int myPos;

    [Header("Energy")]
    [SerializeField] private float needEnergy = 2f;
    private ElectricityUser eleUSc;

    void Awake()
    {
        platformB = gameObject.GetComponent<PlatformBehavior>();

        myPos = platformB.GetTabPos();

        platformB.taskTime = plantingTime;
        platformB.range = range;
        platformB.usingGuiType = PlatfotmGUIType.Procesing;
        platformB.itemSendingType = PlatformItemSendingType.Procesing;

        //energy
        eleUSc = gameObject.GetComponent<ElectricityUser>();
        if (eleUSc == null) Debug.LogError(name + " dont have ElectricityUser script!");
        eleUSc.maxEnergyPerSec = needEnergy;
        eleUSc.actCharge = 0f;
        eleUSc.maxCharge = eleUSc.maxEnergyPerSec * 2f;

        SetResToCraft(0);
    }
    void Start()
    {
        ElectricityManager.instance.AddRequester(eleUSc);
        Invoke(nameof(TryDoTask), 1f);
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
            DoTask();
        }
    }

    private void TryDoTask()
    {
        if (platformB.working || platformB.itemOnPlatform[(int)plantingRes].qua > platformB.itemOnPlatform[(int)plantingRes].maxQua)
        {
            Invoke(nameof(TryDoTask), 1);
            return;
        }

        if (platformB.itemOnPlatform[(int)Res.BottleWater].qua > 0) { foundPlaceToPlant = WorldMenager.instance.FindTheNearestObjectOnTerrain(Obj.None, Obj.TerrainFertile, myPos.x, myPos.y, range); }
        else { foundPlaceToPlant = new Vector2Int(-1, -1); }

        Obj farmlandType = ResToObj(plantingRes);
        if (farmlandType == Obj.None) { foundPlaceToCollect = new Vector2Int(-1, -1); }
        else { foundPlaceToCollect = WorldMenager.instance.FindTheNearestObject(farmlandType, myPos.x,myPos.y, range); }

        if (foundPlaceToPlant.x == -1 && foundPlaceToCollect.x == -1)
        {
            Invoke(nameof(TryDoTask), 1);
            return;
        }

        platformB.startTaskTime = WorldMenager.instance.worldTime;
        platformB.taskTime = plantingTime;
        platformB.timeToEndCraft = plantingTime;
        platformB.working = true;

        Obj ResToObj(Res res)
        {
            switch (res)
            {
                case Res.Grape: return Obj.FarmlandGrape;
                case Res.Flax: return Obj.FarmlandFlax;
                case Res.RubberPlant: return Obj.FarmlandRubber;
            }
            return Obj.None;
        }
    }

    private void DoTask()
    {
        platformB.working = false;

        if (foundPlaceToPlant.x != -1)
        {
            if(WorldMenager.instance.GetSquer(foundPlaceToPlant.x, foundPlaceToPlant.y) == Obj.None)
            {
                int type = 0;
                switch (plantingRes)
                {
                    case Res.Flax: type = 1; break;
                    case Res.Grape: type = 2; break;
                    case Res.RubberPlant: type = 3; break;
                }
                platformB.AddItem(Res.BottleWater, -1);
                platformB.AddItem(Res.BottleEmpty, 1);
                TerrainManager.instance.SpawnFarmland(foundPlaceToPlant.x, foundPlaceToPlant.y, true, type);
            }
        }

        if(foundPlaceToCollect.x != -1)
        {
            Transform fCT = WorldMenager.instance.GetTransforOfObj(foundPlaceToCollect.x, foundPlaceToCollect.y);
            if (fCT != null)
            {
                WorldMenager.instance.RemoveObjFromGO(fCT.gameObject, foundPlaceToCollect.x, foundPlaceToCollect.y);
                platformB.AddItem(plantingRes, 1, true);
            }
        }

        TryDoTask();
    }

    public void SetResToCraft(int recipeNuber)
    {
        if (nowUseRecipeNumber == recipeNuber) { return; }

        if (recipeNuber > AllRecipes.instance.GetCraftRecipes(Obj.Farm).Count) { return; }

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
            ItemIn = new List<ItemRAQ>();
            ItemOut = new List<ItemRAQ>();
            return;
        }

        platformB.canGetRes = true;

        CraftRecipe nowUseRecipe = AllRecipes.instance.GetCraftRecipes(Obj.Farm)[nowUseRecipeNumber - 1];

        platformB.UpdateImageR(nowUseRecipe.ItemOut[0].res);

        for (int i = 0; i < nowUseRecipe.ItemIn.Count; i++)
        {
            int resIndex = (int)nowUseRecipe.ItemIn[i].res;
            int qua = nowUseRecipe.ItemIn[i].qua;
            platformB.itemOnPlatform[resIndex].maxQua = qua * 2;
            platformB.itemOnPlatform[resIndex].canIn = true;
            platformB.itemOnPlatform[resIndex].canOut = false;
        }
        ItemIn = nowUseRecipe.ItemIn;
        for (int i = 0; i < nowUseRecipe.ItemOut.Count; i++)
        {
            int resIndex = (int)nowUseRecipe.ItemOut[i].res;
            int qua = nowUseRecipe.ItemOut[i].qua;
            platformB.itemOnPlatform[resIndex].maxQua = qua * 2;
        }
        ItemOut = nowUseRecipe.ItemOut;

        //set planting res
        if (nowUseRecipe.ItemOut.Count > 0)
        { plantingRes = nowUseRecipe.ItemOut[0].res; }

        platformB.UpdateAvalibleResList();
        plantingTime = nowUseRecipe.exeTime;
        platformB.taskTime = plantingTime;
    }
}
