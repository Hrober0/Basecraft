using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class BuildPanel : MonoBehaviour
{
    [Header("Build Gui")]
    public Transform BuildPanelT;
    [SerializeField] private GameObject BuildObjButton = null;
    [SerializeField] private GameObject ItemInBuildButton = null;
    [SerializeField] private GameObject BuildObjButtonBorder = null;
    [SerializeField] private GameObject ProductionTab = null;
    [SerializeField] private GameObject MilitaryTab = null;
    [SerializeField] private GameObject EnergyTab = null;
    [SerializeField] private GameObject OtherTab = null;
    [SerializeField] private GameObject ProductionButtons = null;
    [SerializeField] private GameObject MilitaryButtons = null;
    [SerializeField] private GameObject EnergyButtons = null;
    [SerializeField] private GameObject OtherButtons = null;

    [Header("Veribals")]
    [SerializeField] private Sprite CancelIcon = null;
    [SerializeField] private int selectedBuildPage = 1;
    private Transform selectedBuildPageT = null;
    private Transform selectedBuildButtonT = null;
    public Obj selectedObj = Obj.None;
    private bool complateChangeBuildPage = true;
    [SerializeField] private Color disactiveTab = new Color(1, 1, 1, 0.5f);
    [SerializeField] private Color activeTab = new Color(1, 1, 1, 1);

    [Header("BuildingPointer")]
    [SerializeField] private GameObject BuildingPointer = null;
    [SerializeField] private Color normalPColor = Color.white;
    [SerializeField] private Color errorPColor = Color.red;
    private Image PImage;
    private float lastCameraScale = 0;
    private bool isRed = false;

    void Start()
    {
        PImage = BuildingPointer.GetComponent<Image>();
        PImage.color = normalPColor;

        SetColorBuildTab(1, disactiveTab);
        SetColorBuildTab(2, disactiveTab);
        SetColorBuildTab(3, disactiveTab);
        SetColorBuildTab(4, disactiveTab);
        ProductionButtons.SetActive(false);
        MilitaryButtons.SetActive(false);
        EnergyButtons.SetActive(false);
        OtherButtons.SetActive(false);
        ChangeBuildPage(1);

        BuildObjButtonBorder.SetActive(false);
        BuildingPointer.SetActive(false);
    }
    void Update()
    {
        if (selectedObj != Obj.None)
        {
            if (lastCameraScale != CameraControler.instance.GetScale)
            {
                lastCameraScale = CameraControler.instance.GetScale;
                float scale = 55f / lastCameraScale;
                BuildingPointer.transform.localScale = new Vector2(scale, scale);
            }
            Vector2 screenPosition = Input.mousePosition;
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
                worldPosition.x += 5f; worldPosition.x = (int)(worldPosition.x / 10) * 10;
                worldPosition.y += 5f; worldPosition.y = (int)(worldPosition.y / 10) * 10;
                screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            }
            BuildingPointer.transform.position = screenPosition;
        }
    }

    public void SetAllBuildButtons()
    {
        bool productionTabActive = false;
        bool militaryTabActive = false;
        bool energyTabActive = false;
        bool otherTabActive = false;

        //building
        BuildObjButton.SetActive(false);
        foreach (Obj obj in AllRecipes.instance.ObjectThatCanBeBuilt)
        {
            BuildingRecipe nowUseRecipe = AllRecipes.instance.GetBuildRecipe(obj);

            if (nowUseRecipe.active == false) { continue; }

            //disacitve buttons to totrial
            if (SpaceBaseMainSc.instance.skippedTotorial == false)
            {
                if (obj == Obj.BasicCrafter && GameEventControler.complateGameEvent.Contains(GameEventControler.GameEvent.P1_12) == false) { continue; }
                if (obj == Obj.Warehouse1 && GameEventControler.complateGameEvent.Contains(GameEventControler.GameEvent.P2_2) == false) { continue; }
                if (obj == Obj.Launchpad && GameEventControler.complateGameEvent.Contains(GameEventControler.GameEvent.NoMoreTask) == false) { continue; }
            }

            Transform buttonParent = null;
            switch (nowUseRecipe.page)
            {
                case 1: buttonParent = ProductionButtons.transform; productionTabActive = true; break;
                case 2: buttonParent = MilitaryButtons.transform; militaryTabActive = true; break;
                case 3: buttonParent = EnergyButtons.transform; energyTabActive = true; break;
                case 4: buttonParent = OtherButtons.transform; otherTabActive = true; break;
                default: Debug.Log("Error cant set parent for build button! unknow page (" + nowUseRecipe.page + "), than sett dif page"); buttonParent = OtherButtons.transform; continue;
            }
            buttonParent = buttonParent.Find("Viewport").Find("Content");

            string name = nowUseRecipe.building.ToString();

            Transform buttonTrans = buttonParent.Find(name);
            if (buttonTrans != null) { buttonTrans.gameObject.SetActive(true); continue; }

            GameObject newButton = Instantiate(BuildObjButton, new Vector2(), Quaternion.identity);
            newButton.transform.Find("NameText").GetComponent<Text>().text = GuiControler.instance.DisplayedNameOfObj(nowUseRecipe.building);
            newButton.transform.Find("HPText").GetComponent<Text>().text = "Health: " + AllRecipes.instance.GetMaxHelthOfObj(obj);
            newButton.transform.Find("BuildingImage").GetComponent<Image>().sprite = ImageLibrary.instance.GetObjImages(obj);
            newButton.name = name;
            newButton.transform.SetParent(buttonParent, false);
            newButton.SetActive(true);

            Transform ItemToBuildPanel = newButton.transform.Find("ItemToBuildPanel");
            Transform ItemInBuildButtonTD = ItemToBuildPanel.Find("ItemInBuildButton");
            Destroy(ItemInBuildButtonTD.gameObject);
            for (int j = 0; j < nowUseRecipe.neededItems.Count; j++)
            {
                GameObject newItem = Instantiate(ItemInBuildButton, new Vector2(), Quaternion.identity);
                newItem.name = nowUseRecipe.neededItems[j].res.ToString();
                newItem.transform.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(nowUseRecipe.neededItems[j].res);
                newItem.transform.Find("Text").GetComponent<Text>().text = nowUseRecipe.neededItems[j].qua.ToString();
                newItem.transform.SetParent(ItemToBuildPanel, false);
                newItem.SetActive(true);
            }
        }

        //hide buildings tab
        ProductionTab.SetActive(productionTabActive);
        MilitaryTab.SetActive(militaryTabActive);
        EnergyTab.SetActive(energyTabActive);
        OtherTab.SetActive(otherTabActive);
    }
    public void ChangeBuildPage(int page)
    {
        if (complateChangeBuildPage == false) { return; }

        GameObject newPanel = GetPanel(page);
        newPanel.SetActive(true);

        SelectBuildButton(Obj.None);
        selectedBuildPageT = newPanel.transform;

        SetColorBuildTab(selectedBuildPage, disactiveTab);
        SetColorBuildTab(page, activeTab);

        if (page == selectedBuildPage) { return; }

        RectTransform newRT = newPanel.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();

        GameObject oldPanel = GetPanel(selectedBuildPage);
        oldPanel.SetActive(true);
        RectTransform oldRT = oldPanel.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();

        float slideTime = 0.2f;
        complateChangeBuildPage = false;

        if (page > selectedBuildPage)
        {
            newRT.anchoredPosition = new Vector2(newRT.sizeDelta.x, newRT.anchoredPosition.y);
            newRT.DOAnchorPosX(0, slideTime).SetUpdate(true);
            oldRT.anchoredPosition = new Vector2(0, oldRT.anchoredPosition.y);
            oldRT.DOAnchorPosX(-oldRT.sizeDelta.x, slideTime).SetUpdate(true).OnComplete(() => EndChange());
        }
        else
        {
            newRT.anchoredPosition = new Vector2(-newRT.sizeDelta.x, newRT.anchoredPosition.y);
            newRT.DOAnchorPosX(0, slideTime).SetUpdate(true);
            oldRT.anchoredPosition = new Vector2(0, oldRT.anchoredPosition.y);
            oldRT.DOAnchorPosX(oldRT.sizeDelta.x, slideTime).SetUpdate(true).OnComplete(() => EndChange());
        }

        selectedBuildPage = page;

        GameObject GetPanel(int _page)
        {
            switch (_page)
            {
                case 1: return ProductionButtons;
                case 2: return MilitaryButtons;
                case 3: return EnergyButtons;
                case 4: return OtherButtons;
            }
            return null;
        }

        void EndChange()
        {
            oldPanel.SetActive(false);
            complateChangeBuildPage = true;
        }
    }
    private void SetColorBuildTab(int tab, Color color)
    {
        GameObject oldButton = GetButton(tab);
        oldButton.transform.Find("Image").GetComponent<Image>().color = color;
        oldButton.GetComponentInChildren<Text>().color = color;

        GameObject GetButton(int _page)
        {
            switch (_page)
            {
                case 1: return ProductionTab;
                case 2: return MilitaryTab;
                case 3: return EnergyTab;
                case 4: return OtherTab;
            }
            return null;
        }
    }
    public void CancelBuilding() => SelectBuildButton(Obj.None);

    public void ClickBuildButon()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        if (Enum.TryParse(name, out Obj ObjName) == false) { return; }

        SelectBuildButton(ObjName);
    }
    public void ClickBuildingHelpButton()
    {
        string name = EventSystem.current.currentSelectedGameObject.transform.parent.name;
        if (Enum.TryParse(name, out Obj obj) == false) { return; }
        LeftPanel.instance.SetGlossarySelectedBuilding(obj);
    }

    public void ShowErrorOfPointer()
    {
        if (isRed) { return; }
        isRed = true;
        DOTween.Sequence()
            .Append(PImage.DOColor(errorPColor, 0.1f))
            .Append(PImage.DOColor(normalPColor, 0.1f))
            .OnComplete(() => { isRed = false; })
        ;
    }

    private void SelectBuildButton(Obj ObjName)
    {
        if (selectedBuildPageT == null) { return; }

        if (selectedBuildButtonT != null) { selectedBuildButtonT.Find("BuildingImage").GetComponent<Image>().sprite = ImageLibrary.instance.GetObjImages(selectedObj); }

        if (ObjName == selectedObj || ObjName == Obj.None)
        {
            ClickMenager.instance.SetClickMode(ClickMenager.ClickMode.Normal);
            BuildingPointer.SetActive(false);
            BuildObjButtonBorder.SetActive(false);
            selectedObj = Obj.None;
            selectedBuildButtonT = null;
            return;
        }

        Transform buttonT = selectedBuildPageT.Find("Viewport").Find("Content").Find(ObjName.ToString());
        if (buttonT == null) { return; }

        ClickMenager.instance.SetClickMode(ClickMenager.ClickMode.BuildingObject);

        selectedObj = ObjName;
        selectedBuildButtonT = buttonT;

        buttonT.Find("BuildingImage").GetComponent<Image>().sprite = CancelIcon;

        BuildObjButtonBorder.transform.SetParent(buttonT, false);
        BuildObjButtonBorder.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        BuildObjButtonBorder.SetActive(true);

        BuildingPointer.GetComponent<Image>().sprite = ImageLibrary.instance.GetObjImages(selectedObj);
        BuildingPointer.SetActive(true);
    }
}
