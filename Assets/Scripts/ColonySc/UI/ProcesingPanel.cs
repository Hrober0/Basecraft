using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcesingPanel : MonoBehaviour
{
    [Header("Procesing")]
    public Transform ProcesingPanelT = null;
    [SerializeField] private GameObject RecipeButton = null;
    [SerializeField] private GameObject ItemInProcesing= null;
    [SerializeField] private GameObject ItemInRecipe = null;
    [SerializeField] private Transform ResInPanel = null;
    [SerializeField] private Transform ResOutPanel = null;
    [SerializeField] private Image ProcesingProgresArrow = null;
    [SerializeField] private Text ProcesingProgresText = null;
    [SerializeField] private GameObject ChangeCraftingButton = null;
    [SerializeField] private GameObject DefaultRecipesListPanel = null;
    private GameObject BasicCrafterRecipesListPanel = null;
    private GameObject CrafterRecipesListPanel = null;
    private GameObject SmelterRecipesListPanel = null;
    private GameObject PulverizerRecipesListPanel = null;
    private GameObject FarmRecipesListPanel = null;
    private GameObject ChemicalPlantRecipesListPanel = null;

    [Header("Fuel")]
    public Transform FuelPanelT = null;
    [SerializeField] private Sprite powerS = null;
    [SerializeField] private GameObject ProcesingFuelImage = null;
    [SerializeField] private Slider ProcesingFuelSlider = null;

    [Header("veribals")]
    public bool updateProgrsBarOn = false;
    public bool updateFuelProcPanel = false;
    private int nowUseRecipe = -1;
    private CrafterNeedFuel useCrafterNeedFuelSc;
    private CrafterNeedEnergy useEleCrafterNeedEnergySc;
    private List<Text> TextResPanelIn;
    private List<Text> TextResPanelOut;
    private List<Res> ProcesingResIn;
    private List<Res> ProcesingResOut;

    private readonly float updateGuiDelay = 0.3f;
    private float timeToUpdateGui = 0f;

    void Update()
    {
        if (updateProgrsBarOn)
        {
            PlatformBehavior usePBSc = GuiControler.instance.usePBSc;

            float per = (usePBSc.taskTime - usePBSc.timeToEndCraft) / usePBSc.taskTime;

            if (usePBSc.working == false)
            {
                if (per >= 1f || per < 0f) { per = 0f; }

                ProcesingProgresArrow.fillAmount = per;
                ProcesingProgresText.text = string.Format("{0}%", (int)(per * 100.00f));
            }
            else
            {
                if (per == -1f)
                {
                    float taskTime = usePBSc.taskTime;
                    per = (WorldMenager.instance.worldTime - usePBSc.startTaskTime) / taskTime;
                    if (per > 1) { per = 1; }
                }
                ProcesingProgresText.text = string.Format("{0}%", (int)(per * 100.00f));
                ProcesingProgresArrow.fillAmount = per;
            }
        }

        if (updateFuelProcPanel)
        {
            if (AllRecipes.instance.IsObjHaveCrafterNeedFuelSc(GuiControler.instance.useObj)) { ProcesingFuelSlider.value = useCrafterNeedFuelSc.percRemFuel; }
        }

        if (timeToUpdateGui <= 0f)
        {
            if (GuiControler.instance.IsNowOpenPanelsContains(ProcesingPanelT)) { UpdateProceingPanel(); }
            if (GuiControler.instance.IsNowOpenPanelsContains(FuelPanelT)) { UpdateFuelPanel(); }
            timeToUpdateGui = updateGuiDelay;
        }
        timeToUpdateGui -= Time.unscaledDeltaTime;
    }

    public void SetAllRecipesListPanel()
    {
        List<Obj> panelsList = new List<Obj> { Obj.BasicCrafter, Obj.Crafter, Obj.Smelter, Obj.Pulverizer, Obj.Farm, Obj.ChemicalPlant };
        foreach (Obj obj in panelsList)
        {
            GameObject MainPanel = GetRecipeListPanle(obj);

            if (MainPanel == null) { Debug.LogError("Cant set recipe list panel! of " + obj); continue; }

            List<CraftRecipe> useRecipes = AllRecipes.instance.GetCraftRecipes(obj);
            Transform Parent = MainPanel.transform.Find("BLViewport").Find("BLContener");
            for (int i = 0; i < useRecipes.Count; i++)
            {
                CraftRecipe useRecipe = useRecipes[i];

                if (useRecipe.active == false) { continue; }

                bool discoeverAllItemIn = true;
                foreach (ItemRAQ item in useRecipe.ItemIn)
                {
                    if (AllRecipes.instance.IsResUnlock(item.res) == false) { discoeverAllItemIn = false; break; }
                }
                if (discoeverAllItemIn == false) { continue; }

                string name = string.Format("{0}", i + 1);

                Transform repTrans = Parent.Find(name);
                if (repTrans != null) { repTrans.gameObject.SetActive(true); continue; }

                GameObject newButton = Instantiate(RecipeButton);
                newButton.transform.SetParent(Parent, false);
                newButton.name = name;
                newButton.SetActive(true);

                foreach (ItemRAQ item in useRecipe.ItemIn)
                {
                    GameObject newIICRL = Instantiate(ItemInRecipe, new Vector2(), Quaternion.identity);
                    newIICRL.transform.SetParent(newButton.transform.Find("ItemsIn"), false);
                    newIICRL.name = string.Format("{0}", item.res);
                    newIICRL.SetActive(true);
                    newIICRL.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(item.res);
                    newIICRL.transform.Find("Text").GetComponent<Text>().text = string.Format("{0}", item.qua);
                }
                foreach (ItemRAQ item in useRecipe.ItemOut)
                {
                    GameObject newIICRL = Instantiate(ItemInRecipe, new Vector2(), Quaternion.identity);
                    newIICRL.transform.SetParent(newButton.transform.Find("ItemsOut"), false);
                    newIICRL.name = string.Format("{0}", item.res);
                    newIICRL.SetActive(true);
                    newIICRL.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(item.res);
                    newIICRL.transform.Find("Text").GetComponent<Text>().text = string.Format("{0}", item.qua);
                }
                Transform Arrow = newButton.transform.Find("ArrowImage");

                if (useRecipe.ItemIn.Count > 3)
                {
                    float mfp = (useRecipe.ItemIn.Count - 3) * 48f - 20f;
                    Vector3 v3 = new Vector3(mfp, 0, 0);
                    Arrow.Translate(v3);
                    newButton.transform.Find("ItemsOut").Translate(v3);
                }
                Arrow.GetComponentInChildren<Text>().text = useRecipe.exeTime.ToString() + "s";
            }
        }

        
    }
    public void SetProcesingScripts()
    {
        Obj useObj = GuiControler.instance.useObj;

        Transform platT = WorldMenager.instance.GetTransforOfObj(GuiControler.instance.useX, GuiControler.instance.useY);

        if (AllRecipes.instance.IsObjHaveCrafterNeedFuelSc(useObj))
        {
            useCrafterNeedFuelSc = platT.GetComponent<CrafterNeedFuel>();
        }
        else if (AllRecipes.instance.IsObjHaveCrafterNeedEnergySc(useObj))
        {
            useEleCrafterNeedEnergySc = platT.GetComponent<CrafterNeedEnergy>();
        }

        if (AllRecipes.instance.IsUsingEnergy(useObj)) { GuiControler.instance.useEleUserSc = platT.GetComponent<ElectricityUser>(); }
    }
    public void UpdateProceingPanel()
    {
        BasicCrafterRecipesListPanel.SetActive(false);
        CrafterRecipesListPanel.SetActive(false);
        SmelterRecipesListPanel.SetActive(false);
        PulverizerRecipesListPanel.SetActive(false);
        FarmRecipesListPanel.SetActive(false);
        ChemicalPlantRecipesListPanel.SetActive(false);

        updateProgrsBarOn = true;
        ChangeCraftingButton.SetActive(false);

        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        Obj useObj = GuiControler.instance.useObj;
        int useX = GuiControler.instance.useX;
        int useY = GuiControler.instance.useY;

        nowUseRecipe = -1;
        CraftRecipe recipe = null;

        if (AllRecipes.instance.IsObjHaveCrafterNeedFuelSc(useObj))
        {
            nowUseRecipe = WorldMenager.instance.GetTransforOfObj(useX, useY).GetComponent<CrafterNeedFuel>().nowUseRecipeNumber;
            if (nowUseRecipe == 0) { GetRecipeListPanle(useObj).SetActive(true); updateProgrsBarOn = false; }
            else { recipe = AllRecipes.instance.GetCraftRecipes(useObj)[nowUseRecipe - 1]; }
            ChangeCraftingButton.SetActive(true);
        }
        else if (AllRecipes.instance.IsObjHaveCrafterNeedEnergySc(useObj))
        {
            nowUseRecipe = WorldMenager.instance.GetTransforOfObj(useX, useY).GetComponent<CrafterNeedEnergy>().nowUseRecipeNumber;
            if (nowUseRecipe == 0) { GetRecipeListPanle(useObj).SetActive(true); updateProgrsBarOn = false; }
            else { recipe = AllRecipes.instance.GetCraftRecipes(useObj)[nowUseRecipe - 1]; }
            ChangeCraftingButton.SetActive(true);
        }
        else
        {
            switch (useObj)
            {
                case Obj.Farm:
                    nowUseRecipe = WorldMenager.instance.GetTransforOfObj(useX, useY).GetComponent<Farm>().nowUseRecipeNumber;
                    if (nowUseRecipe == 0) { FarmRecipesListPanel.SetActive(true); updateProgrsBarOn = false; }
                    else { recipe = AllRecipes.instance.GetCraftRecipes(Obj.Farm)[nowUseRecipe - 1]; }
                    ChangeCraftingButton.SetActive(true);
                    break;
            }
        }

        for (int i = 0; i < ResInPanel.childCount; i++) { ResInPanel.GetChild(i).gameObject.SetActive(false); }
        for (int i = 0; i < ResOutPanel.childCount; i++) { ResOutPanel.GetChild(i).gameObject.SetActive(false); }

        TextResPanelIn = new List<Text>();
        TextResPanelOut = new List<Text>();
        ProcesingResIn = new List<Res>();
        ProcesingResOut = new List<Res>();

        if (nowUseRecipe == 0) { return; }

        if (recipe != null)
        {
            for (int i = 0; i < recipe.ItemIn.Count; i++)
            {
                Res res = recipe.ItemIn[i].res;
                Transform cRTIn = ResInPanel.Find(res.ToString());
                if (cRTIn == null) { cRTIn = CreateResIn(res); }
                cRTIn.gameObject.SetActive(true);
                TextResPanelIn.Add(cRTIn.Find("Text").GetComponent<Text>());
                ProcesingResIn.Add(res);
            }
            for (int i = 0; i < recipe.ItemOut.Count; i++)
            {
                Res res = recipe.ItemOut[i].res;
                Transform cRTOut = ResOutPanel.Find(res.ToString());
                if (cRTOut == null) { cRTOut = CreateResOut(res); }
                cRTOut.gameObject.SetActive(true);
                TextResPanelOut.Add(cRTOut.Find("Text").GetComponent<Text>());
                ProcesingResOut.Add(res);
            }
        }
        else
        {
            for (int i = 1; i < usePBSc.itemOnPlatform.Length; i++)
            {
                Res res = (Res)i;
                Transform cRTIn = ResInPanel.Find(res.ToString());
                Transform cRTOut = ResOutPanel.Find(res.ToString());
                if (usePBSc.itemOnPlatform[i].canOut)
                {
                    if (usePBSc.itemOnPlatform[i].maxQua > 0)
                    {
                        if (cRTOut == null) { cRTOut = CreateResOut(res); }
                        cRTOut.gameObject.SetActive(true);
                        TextResPanelOut.Add(cRTOut.Find("Text").GetComponent<Text>());
                        ProcesingResOut.Add(res);
                    }
                }
                else
                {
                    if (usePBSc.itemOnPlatform[i].canIn)
                    {
                        if (cRTIn == null) { cRTIn = CreateResIn(res); }
                        cRTIn.gameObject.SetActive(true);
                        TextResPanelIn.Add(cRTIn.Find("Text").GetComponent<Text>());
                        ProcesingResIn.Add(res);
                    }
                }
            }
        }

        UpdateProcesingResPanel();

        Transform CreateResIn(Res res)
        {
            GameObject newResIn = Instantiate(ItemInProcesing, new Vector2(), Quaternion.identity);
            newResIn.transform.SetParent(ResInPanel, false);
            newResIn.name = res.ToString();
            newResIn.SetActive(false);
            newResIn.transform.Find("Image").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            return newResIn.transform;
        }
        Transform CreateResOut(Res res)
        {
            GameObject newResOut = Instantiate(ItemInProcesing, new Vector2(), Quaternion.identity);
            newResOut.transform.SetParent(ResOutPanel, false);
            newResOut.name = res.ToString();
            newResOut.SetActive(false);
            newResOut.transform.Find("Image").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            return newResOut.transform;
        }
    }
    private void UpdateProcesingResPanel()
    {
        if (nowUseRecipe == 0) { return; }

        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        Obj useObj = GuiControler.instance.useObj;

        for (int i = 0; i < TextResPanelIn.Count; i++)
        {
            int index = (int)ProcesingResIn[i];
            int showQua = usePBSc.itemOnPlatform[index].qua;
            if (showQua < 0) { showQua = 0; }
            int visableMaxQua = calc(usePBSc.itemOnPlatform[index].maxQua);
            TextResPanelIn[i].text = string.Format("{0}/{1}", showQua, visableMaxQua);
        }
        for (int i = 0; i < TextResPanelOut.Count; i++)
        {
            int index = (int)ProcesingResOut[i];
            int showQua = usePBSc.itemOnPlatform[index].qua;
            if (showQua < 0) { showQua = 0; }
            int visableMaxQua = calc(usePBSc.itemOnPlatform[index].maxQua);
            TextResPanelOut[i].text = string.Format("{0}/{1}", showQua, visableMaxQua);
        }

        int calc(int value)
        {
            if (AllRecipes.instance.objectWithCraftRecipes.Contains(useObj)) { return value / 2; }
            return value;
        }
    }

    private GameObject GetRecipeListPanle(Obj obj)
    {
        switch (obj)
        {
            case Obj.BasicCrafter: 
                if (BasicCrafterRecipesListPanel == null) { BasicCrafterRecipesListPanel = CreateNewPanel(obj); }
                return BasicCrafterRecipesListPanel;
            case Obj.Smelter:
                if (SmelterRecipesListPanel == null) { SmelterRecipesListPanel = CreateNewPanel(obj); }
                return SmelterRecipesListPanel;
            case Obj.ElectricSmelter:
                if (SmelterRecipesListPanel == null) { SmelterRecipesListPanel = CreateNewPanel(obj); } 
                return SmelterRecipesListPanel;
            case Obj.Crafter:
                if (CrafterRecipesListPanel == null) { CrafterRecipesListPanel = CreateNewPanel(obj); }
                return CrafterRecipesListPanel;
            case Obj.Pulverizer:
                if (PulverizerRecipesListPanel == null) { PulverizerRecipesListPanel = CreateNewPanel(obj); }
                return PulverizerRecipesListPanel;
            case Obj.Farm:
                if (FarmRecipesListPanel == null) { FarmRecipesListPanel = CreateNewPanel(obj); }
                return FarmRecipesListPanel;
            case Obj.ChemicalPlant:
                if (ChemicalPlantRecipesListPanel == null) { ChemicalPlantRecipesListPanel = CreateNewPanel(obj); }
                return ChemicalPlantRecipesListPanel;
        }
        return null;

        GameObject CreateNewPanel(Obj panel)
        {
            GameObject newPanel = Instantiate(DefaultRecipesListPanel);
            newPanel.transform.SetParent(ProcesingPanelT, false);
            newPanel.name = panel.ToString() + "RecipePanel";
            newPanel.SetActive(false);
            return newPanel;
        }
    }

    //fuell
    public void UpdateFuelPanel()
    {
        Obj useObj = GuiControler.instance.useObj;
        if (AllRecipes.instance.IsObjHaveCrafterNeedFuelSc(useObj))
        {
            updateFuelProcPanel = true;
            if (useCrafterNeedFuelSc.useFuel == Res.None)
            { ProcesingFuelImage.SetActive(false); }
            else
            {
                int qua = GuiControler.instance.usePBSc.itemOnPlatform[(int)useCrafterNeedFuelSc.useFuel].qua;
                if (qua < 0) qua = 0;
                ProcesingFuelImage.SetActive(true);
                ProcesingFuelImage.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(useCrafterNeedFuelSc.useFuel);
                ProcesingFuelImage.transform.Find("Text").GetComponent<Text>().text = qua.ToString();
            }
        }

        //update energy
        if (AllRecipes.instance.IsUsingEnergy(useObj))
        {
            ProcesingFuelImage.SetActive(true);
            ProcesingFuelImage.GetComponent<Image>().sprite = powerS;
            ProcesingFuelImage.transform.Find("Text").GetComponent<Text>().text = "";

            ElectricityUser useEleUserSc = GuiControler.instance.useEleUserSc;
            if (useEleUserSc == null) 
                ProcesingFuelSlider.value = 0f;
            else
            {
                float percent = 0f;
                if (useEleUserSc.maxCharge > 0f) percent = useEleUserSc.actCharge / useEleUserSc.maxCharge;
                ProcesingFuelSlider.value = percent;
            }
        }
    }
}
