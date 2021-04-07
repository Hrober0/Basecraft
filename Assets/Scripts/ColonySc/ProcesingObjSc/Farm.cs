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

    private PlatformBehavior PlatformB;
    Vector2Int myPos;

    void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();

        myPos = PlatformB.GetTabPos();

        PlatformB.taskTime = plantingTime;
        PlatformB.range = range;
        PlatformB.usingGuiType = PlatfotmGUIType.Procesing;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;

        SetResToCraft(0);
    }
    void Start()
    {
        Invoke("TryDoTask", WorldMenager.instance.frequencyOfChecking);
    }

    private void TryDoTask()
    {
        if (PlatformB.working || PlatformB.itemOnPlatform[(int)plantingRes].qua > PlatformB.itemOnPlatform[(int)plantingRes].maxQua)
        { Invoke("TryDoTask", WorldMenager.instance.frequencyOfChecking); return; }

        if (PlatformB.itemOnPlatform[(int)Res.BottleWater].qua > 0) { foundPlaceToPlant = WorldMenager.instance.FindTheNearestObjectOnTerrain(Obj.None, Obj.TerrainFertile, myPos.x, myPos.y, range); }
        else { foundPlaceToPlant = new Vector2Int(-1, -1); }

        Obj farmlandType = ResToObj(plantingRes);
        if (farmlandType == Obj.None) { foundPlaceToCollect = new Vector2Int(-1, -1); }
        else { foundPlaceToCollect = WorldMenager.instance.FindTheNearestObject(farmlandType, myPos.x,myPos.y, range); }

        if (foundPlaceToPlant.x == -1 && foundPlaceToCollect.x == -1) { Invoke("TryDoTask", WorldMenager.instance.frequencyOfChecking); return; }

        PlatformB.startTaskTime = WorldMenager.instance.worldTime;
        PlatformB.working = true;
        Invoke("DoTask", plantingTime);

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
        PlatformB.working = false;

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
                PlatformB.AddItem(Res.BottleWater, -1);
                PlatformB.AddItem(Res.BottleEmpty, 1);
                TerrainManager.instance.SpawnFarmland(foundPlaceToPlant.x, foundPlaceToPlant.y, true, type);
            }
        }

        if(foundPlaceToCollect.x != -1)
        {
            Transform fCT = WorldMenager.instance.GetTransforOfObj(foundPlaceToCollect.x, foundPlaceToCollect.y);
            if (fCT != null)
            {
                WorldMenager.instance.RemoveObjFromGO(fCT.gameObject, foundPlaceToCollect.x, foundPlaceToCollect.y);
                PlatformB.AddItem(plantingRes, 1, true);
            }
        }

        TryDoTask();
    }

    public void SetResToCraft(int recipeNuber)
    {
        if (nowUseRecipeNumber == recipeNuber) { return; }

        if (recipeNuber > AllRecipes.instance.GetCraftRecipes(Obj.Farm).Count) { return; }

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

        CraftRecipe nowUseRecipe = AllRecipes.instance.GetCraftRecipes(Obj.Farm)[nowUseRecipeNumber - 1];

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

        //set planting res
        if (nowUseRecipe.ItemOut.Count > 0)
        { plantingRes = nowUseRecipe.ItemOut[0].res; }

        PlatformB.UpdateAvalibleResList();
        plantingTime = nowUseRecipe.exeTime;
        PlatformB.taskTime = plantingTime;
    }
}
