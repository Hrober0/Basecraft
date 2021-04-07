using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class CreativeUIController : MonoBehaviour
{
    public Transform BuildGui;
    public GameObject BGObjectButton;
    public GameObject Border;
    public Transform InfoPanel;  
        private Text IPText;
        private Transform IPItemPanel;
    public GameObject BGBuildingsPanel; //1
    public GameObject BGTerrainPanel;   //2
    public GameObject BGEnemyPanel;     //3
    

    public static CreativeUIController instance;
    private void Awake()
    {
        if (instance != null) { return; }
        instance = this;
    }
    private void Start()
    {
        BGObjectButton.SetActive(false);
        Border.SetActive(false);

        SetAllObjects();
        CreateAllBuildButtons();

        HideAllGui();

        selectobject = Obj.None;
        ChangeObjectPage(1);
    }

    private void SetAllObjects()
    {
        IPText = InfoPanel.Find("Text").GetComponent<Text>();
        IPItemPanel = InfoPanel.Find("ItemPanel");
    }
    private void HideAllGui()
    {
        Hide(BuildGui);

        void Hide(Transform trans)
        {
            if (trans == BuildGui)
            {
                RectTransform rt = trans.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(rt.rect.width, rt.anchoredPosition.y);
            }
        }
    }

    //build 
    public Obj selectobject = Obj.None;
    public int useBuildPage = 1;
    public void ShowBuildGui()
    {
        BuildGui.gameObject.SetActive(true);
        RectTransform rt = BuildGui.GetComponent<RectTransform>();
        rt.DOAnchorPosX(0f, 0.3f).SetUpdate(true);

        SelectBuildingObject(Obj.None);

        GuiControler.instance.CloseNowOpenGui();
        GuiControler.instance.AddToNowOpenPanels(BuildGui.transform);
    }
    private void CreateAllBuildButtons()
    {
        //buildings
        foreach (Obj obj in AllRecipes.instance.ObjectThatCanBeBuilt)
        {
            CreateButton(obj, 1);
        }

        //terrain
        CreateButton(Obj.StoneOre, 2);
        CreateButton(Obj.CopperOre, 2);
        CreateButton(Obj.IronOre, 2);

        CreateButton(Obj.TerrainFertile, 2);
        CreateButton(Obj.Tree, 2);
        CreateButton(Obj.Sapling, 2);
        CreateButton(Obj.FarmlandRubber, 2);
        CreateButton(Obj.FarmlandFlax, 2);
        CreateButton(Obj.FarmlandGrape, 2);
        CreateButton(Obj.WaterSource, 2);
        CreateButton(Obj.OilSource, 2);

        CreateButton(Obj.Mountain, 2);


        //enemy
        CreateButton(Obj.EnemyPlatform, 3);
        CreateButton(Obj.EnemySpawner, 3);
        CreateButton(Obj.EnemyTurret, 3);
        CreateButton(Obj.EnemyWall, 3);
        CreateButton(Obj.EnemyCore, 3);


        void CreateButton(Obj obj, int panelN)
        {
            GameObject panel;
            Sprite sprite= null;
            if (panelN == 1) { panel = BGBuildingsPanel; sprite = ImageLibrary.instance.GetObjImages(obj); }
            else if (panelN == 2) { panel = BGTerrainPanel; sprite = TerrainManager.instance.GetTerrainImages(obj); }
            else if (panelN == 3) { panel = BGEnemyPanel; sprite = EnemyControler.instance.GetEnemyBaseImages(obj); }
            else { return; }

            Transform parent = panel.transform.Find("Viewport").Find("Content");
            GameObject newButton = Instantiate(BGObjectButton);
            newButton.transform.SetParent(parent, false);
            newButton.SetActive(true);
            newButton.name = obj.ToString();

            if (sprite != null)
            {
                Image BGPlatImg = newButton.transform.Find("Image").GetComponent<Image>();

                if (sprite == null) { BGPlatImg.enabled = false; }
                else { BGPlatImg.sprite = sprite; BGPlatImg.enabled = true; }
            }
        }
    }
    public void ClickObjectButton(GameObject gameObject)
    {
        if(Enum.TryParse(gameObject.name, out Obj obj) == false) { return; }

        Border.SetActive(true);
        Border.transform.SetParent(gameObject.transform, false);
        RectTransform rectTransform = Border.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector2(0, 0);

        SelectBuildingObject(obj);
    }
    private void SelectBuildingObject(Obj obj)
    {
        if (selectobject == obj || obj == Obj.None || obj == Obj.Locked)
        {
            selectobject = Obj.None;
            Border.SetActive(false);
            IPText.text = "not selected";
            Transform ItemP = InfoPanel.Find("ItemPanel");
            foreach (Transform child in ItemP) { child.gameObject.SetActive(false); }
            return;
        }

        selectobject = obj;

        IPText.text = GuiControler.instance.DisplayedNameOfObj(obj);
        BuildingRecipe recipe = AllRecipes.instance.GetBuildRecipe(obj);
        Transform ItemPanel = InfoPanel.Find("ItemPanel");
        foreach (Transform child in ItemPanel) { child.gameObject.SetActive(false); }
        if (recipe != null)
        {
            foreach (ItemRAQ item in recipe.neededItems)
            {
                Transform itemT = ItemPanel.Find(item.res.ToString());
                if (itemT == null) { itemT = CreateItem(item.res); }
                itemT.Find("Text").GetComponent<Text>().text = item.qua.ToString();
                itemT.gameObject.SetActive(true);
            }
        }


        Transform CreateItem(Res res)
        {
            Transform imgT = ItemPanel.Find("Item");
            GameObject newItem = Instantiate(imgT.gameObject, new Vector2(), Quaternion.identity);
            newItem.name = res.ToString();
            newItem.transform.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            newItem.transform.SetParent(ItemPanel, false);
            newItem.SetActive(true);
            return newItem.transform;
        }
    }
    public void ChangeObjectPage(int page)
    {
        if      (page == 1) { useBuildPage = page; BGBuildingsPanel.SetActive(true); BGTerrainPanel.SetActive(false); BGEnemyPanel.SetActive(false); }
        else if (page == 2) { useBuildPage = page; BGBuildingsPanel.SetActive(false); BGTerrainPanel.SetActive(true); BGEnemyPanel.SetActive(false); }
        else if (page == 3) { useBuildPage = page; BGBuildingsPanel.SetActive(false); BGTerrainPanel.SetActive(false); BGEnemyPanel.SetActive(true); }

        SelectBuildingObject(Obj.None);
    }
}
