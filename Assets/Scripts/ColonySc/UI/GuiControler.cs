using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;

public class GuiControler : MonoBehaviour
{
    public static GuiControler instance;
    
    [Header("Object to set")]
    [SerializeField] GameObject WallsC = null;
    [SerializeField] GameObject ResInfoPanle = null;
    [SerializeField] GameObject TurretBulletAndDmgPanel = null;

    [Header("Game panel")]
    [SerializeField] private Transform GameSpeedPanel = null;
        private GameObject GSPauseButton;
        private GameObject GSPlayButton;
        private GameObject GSSpeedx2Button;
    [SerializeField] private Transform GameSettingsPanel = null;
        private GameObject GSBuildButton;
        private GameObject GSConnectionButton;
        private GameObject GSActionButton;
        private GameObject GSRemoveButton;
        private GameObject GSChooseButton;
        private GameObject GSSelectButton;

    [Header("Build")]
    public BuildPanel BuildPanelSc;
    private Transform BuildPanelT;

    [Header("Platform opction")]
    [SerializeField] private Transform PlatformOptionT = null;
        private GameObject POButtons;
            private GameObject PORemovePlatformButton;
            private GameObject POMineButton;
            private GameObject POCutButton;
            private GameObject POPlantButton;
            private GameObject POConnectionButton;
            private GameObject POElectrickButton;
            private GameObject POBuildButton;
            private GameObject POSetStartPlaceButton;
            private GameObject POAddItemsButton;
        private GameObject PORemovingPanel;
        private Text POName;
        private GameObject POSubtitle;

    [Header("Storage")]
    public StoragePanel storagePanelSc;
        private Transform StoragePanelT = null;
        private Transform LaunchPanelT = null;
        private Transform DronStationpanelT = null;

    [Header("Request panel")]
    public RequestPanel RequestPanelSc;
    private Transform RequestItemPanelT;

    [Header("Build Con")]
    public BuildConPanel BuildConPanelSc;
    private Transform BuildConPanelT;

    [Header("Removing")]
    public RemovingPanel RemovingPanelSc;
    private Transform RemovingPanelT;

    [Header("Actions")]
    public ActionPanel ActionPanelSc;
    private Transform ActionPanelT;

    [Header("Connection")]
    [SerializeField] private Transform ConnectionPanelT = null;
        //buttons
            private GameObject RemoveConnectionButton;
            private GameObject ActiveRoadToggle;
            private GameObject PrioritySetings;
                private Text RoadPriorityText;
        private GameObject ConRemovingPanel;

    [Header("Procesing")]
    [SerializeField] private ProcesingPanel ProcesingPanelSc = null;
    private Transform ProcesingPanelT = null;
    private Transform FuelPanelT = null;

    [Header("Turret")]
    [SerializeField] private Transform TurretPanelT = null;
        private Transform TGAmmoPanel;
            private GameObject TGTurretAmmo;
            private GameObject TGPowerAmmo;
            private Slider TGAmmoSlider;
        private Transform TGStatsPanel;
            private Text TGStatsText;

    [Header("Electrick Gui")]
    [SerializeField] private Transform ElectrickPanelT = null;
        private Slider EGProductionSlider;
            private Text EGPText;
        private Slider EGRequestSlider;
            private Text EGRText;
        private Slider EGChargeSlider;
            private Text EGCText;

    [Header("Power generatr Gui")]
    [SerializeField] private Transform PowerGeneratorPanelT = null;
        private Slider PGGProductionSlider;
        private Slider PGGFuelSlider;
            private Image PGGFuelImage;
        private Slider PGGWaterSlider;
            private Text PGGWaterInText;
            private Text PGGWaterOutText;

    [Header("pause menu Gui")]
    [SerializeField] private GameObject PauseMenu = null;
        private Text PauseMenuText;
        //private Transform MenuPanel;
            private Button ResumeButton;
            private Button SaveButton;

    [Header("Object plan items")]
    [SerializeField] private Transform ObjectPlanItems = null;
        //private Transform OPIGrup = null;
            private Transform OPINeedItemGroup = null;
            private Transform OPIStoredItemGroup = null;
            private GameObject OPINeedItemsText = null;
            private GameObject OPIStoredItemsText = null;

    [Header("Info")]
    [SerializeField] private List<Transform> nowOpenPanels = new List<Transform>();
    public Obj useObj = Obj.None;
    public int useX = -1;
    public int useY = -1;
    public Vector2 startPointRoad;

    [Header("Setings")]
    public int resLenght;
    public int ObjLenght;
    public int TerrainLenght;
    [SerializeField] private float updateGuiDelay = 0.3f;
    [SerializeField] private Color disActiveButtonColor = new Color();
    [SerializeField] private Color activeButtonColor = new Color();
    private List<Transform> allPanelList;

    [Header("Sprites to set")]
    public Sprite resFog;

    [HideInInspector] public PlatformBehavior usePBSc;
    [HideInInspector] public Turret useTurretSc;
    [HideInInspector] public TransmissionTower useTTSc;
    [HideInInspector] public SteemGenerator useSteamGeneratorSc;
    [HideInInspector] public ElectricityUser useEleUserSc;

    private float timeToUpdateGui = 0.1f;
    private readonly float leftPanelOpenTime = 0.2f;
    private readonly float leftPanelCloseTime = 0.2f;

    void Awake()
    {
        if (instance != null) { Debug.Log("more the one GuiControler on scen"); return; }
        instance = this;

        resLenght = Enum.GetNames(typeof(Res)).Length;
        ObjLenght = Enum.GetNames(typeof(Obj)).Length;
        TerrainLenght = Enum.GetNames(typeof(Obj)).Length;

        SetObjects();
        allPanelList = new List<Transform> { PlatformOptionT, ConnectionPanelT, BuildPanelT, BuildConPanelT, RemovingPanelT, ActionPanelT, StoragePanelT, LaunchPanelT, RequestItemPanelT, ProcesingPanelT, FuelPanelT, DronStationpanelT, TurretPanelT, ElectrickPanelT, PowerGeneratorPanelT, ObjectPlanItems };
    }
    private void Start()
    {
        useObj = Obj.None;
        useX = -1;
        useY = -1;
        startPointRoad = new Vector2(-1, -1);

        HideAllPanels();

        GSPlayButton.GetComponent<Button>().interactable = true;
        GSSpeedx2Button.GetComponent<Button>().interactable = true;

        SetAllBuildButtons();
        SetAllRecipesListPanel();
        SetPlatformItemSelectPanel();
        SetAllDronSpaces();
    }
    private void Update()
    {
        if (timeToUpdateGui <= 0f)
        {
            timeToUpdateGui = updateGuiDelay;

            foreach (Transform trans in nowOpenPanels)
            {
                if      (trans == PlatformOptionT)   { UpdatePlatformOpction(); }
                else if (trans == TurretPanelT)        { UpdateAmmoList(); }
                else if (trans == ElectrickPanelT)     { UpdateElectrickGui(); }
                else if (trans == PowerGeneratorPanelT){ UpdatePowerGeneratorGui(); }
            }

            UpdateObjectItems();

            DisactiveClosePanles();
        }
        timeToUpdateGui -= Time.unscaledDeltaTime;
    }

    // display
    public String DisplayedNameOfObj(Obj obj)
    {
        string nameOfObj = Language.NameOfObj(obj);
        switch (obj)
        {
            case Obj.Quarry:
                switch(WorldMenager.instance.GetTerrainTile(useX, useY))
                {
                    case Obj.CopperOre: nameOfObj = "Copper " + nameOfObj; break;
                    case Obj.IronOre:   nameOfObj = "Iron " + nameOfObj;   break;
                    case Obj.StoneOre:  nameOfObj = "Stone " + nameOfObj;  break;
                }
                break;
            case Obj.Pump:
                switch (WorldMenager.instance.GetTerrainTile(useX, useY))
                {
                    case Obj.WaterSource: nameOfObj = "Water " + nameOfObj; break;
                    case Obj.OilSource:   nameOfObj = "Oil " + nameOfObj;   break;
                }
                break;
            
            case Obj.Farmland:
                if (useX<0 || useY<0) { break; }
                int state = WorldMenager.instance.squeresVeribal[useX, useY] % 10;
                int type = (WorldMenager.instance.squeresVeribal[useX, useY] - state) / 10;
                state = (state - 1) * 25;
                switch (type)
                {
                    case 1: return "Flax ("+state+"%)";
                    case 2: return "Vines (" + state + "%)";
                    case 3: return "Rubber plant (" + state + "%)";
                }
                break;
        }
        return nameOfObj;
    }

    // set
    private void SetObjects()
    {
        //Same speed panel
        Transform GSPGroup = GameSpeedPanel.Find("Group");
            GSPauseButton = GSPGroup.Find("PauseButton").gameObject;
            GSPlayButton = GSPGroup.Find("PlayButton").gameObject;
            GSSpeedx2Button = GSPGroup.Find("Speedx2Button").gameObject;

        //Game Settings Panel
        GSPGroup = GameSettingsPanel.Find("Group");
                GSBuildButton = GSPGroup.Find("BuildButton").gameObject;
                GSConnectionButton = GSPGroup.Find("ConnectionButton").gameObject;
                GSActionButton = GSPGroup.Find("ActionButton").gameObject;
                GSRemoveButton = GSPGroup.Find("RemoveButton").gameObject;
                GSChooseButton = GSPGroup.Find("ChooseButton").gameObject;
                GSSelectButton = GSPGroup.Find("SelectButton").gameObject;
            
        //build panel
        BuildPanelT = BuildPanelSc.BuildPanelT;

        //build connection
        BuildConPanelT = BuildConPanelSc.BuildConPanelT;

        //removing panel
        RemovingPanelT = RemovingPanelSc.RemovingPanelT;

        //action panel
        ActionPanelT = ActionPanelSc.ActionPanelT;

        //platform opction
            POButtons = PlatformOptionT.Find("Buttons").gameObject;
                PORemovePlatformButton = POButtons.transform.Find("RemovePlatformButton").gameObject;
                POMineButton = POButtons.transform.Find("MineButton").gameObject;
                POCutButton = POButtons.transform.Find("CutButton").gameObject;
                POPlantButton = POButtons.transform.Find("PlantButton").gameObject;
                POElectrickButton = POButtons.transform.Find("ElectrickButton").gameObject;
                POConnectionButton = POButtons.transform.Find("ConnectionButton").gameObject;
                POBuildButton = POButtons.transform.Find("BuildButton").gameObject;
                POSetStartPlaceButton = POButtons.transform.Find("SetStartPlaceButton").gameObject;
                POAddItemsButton = POButtons.transform.Find("AddItemsButton").gameObject;
            PORemovingPanel = PlatformOptionT.Find("RemovingPanel").gameObject;
            POName = PlatformOptionT.Find("PlatformName").GetComponent<Text>();
            POSubtitle = PlatformOptionT.Find("Subtitle").gameObject;

        //storage panel
        StoragePanelT = storagePanelSc.StoragePanelT;
            LaunchPanelT = storagePanelSc.LaunchPanelT;
            DronStationpanelT = storagePanelSc.DronStationPanelT;

        //Request items
        RequestItemPanelT = RequestPanelSc.RequestItemPanelT;

        //connection panel
        Transform CButtons = ConnectionPanelT.transform.Find("Buttons");
                RemoveConnectionButton = CButtons.Find("RemoveConnectionButton").gameObject;
                ActiveRoadToggle = CButtons.Find("ActiveRoadToggle").gameObject;
                PrioritySetings = CButtons.Find("PrioritySetings").gameObject;
                    RoadPriorityText = PrioritySetings.transform.Find("Text").GetComponent<Text>();
            ConRemovingPanel = ConnectionPanelT.transform.Find("RemovingPanel").gameObject;

        //procesing panel
        ProcesingPanelT = ProcesingPanelSc.ProcesingPanelT;

        //fuel panel
        FuelPanelT = ProcesingPanelSc.FuelPanelT;

        // Turret panel
            TGAmmoPanel = TurretPanelT.Find("AmmoPanel");
                TGTurretAmmo = TGAmmoPanel.Find("TurretAmmo").gameObject;
                TGPowerAmmo = TGAmmoPanel.Find("PowerS").gameObject;
                TGAmmoSlider = TGAmmoPanel.Find("AmmoSlider").GetComponent<Slider>();
            TGStatsPanel = TurretPanelT.Find("StatsPanel");
                TGStatsText = TGStatsPanel.Find("StatsPanelText").GetComponent<Text>();

        //electrick panel
            EGProductionSlider = ElectrickPanelT.Find("ProductionSlider").GetComponent<Slider>();
                EGPText = EGProductionSlider.transform.Find("Text").GetComponent<Text>();
            EGRequestSlider = ElectrickPanelT.Find("RequestSlider").GetComponent<Slider>();
                EGRText = EGRequestSlider.transform.Find("Text").GetComponent<Text>();
            EGChargeSlider = ElectrickPanelT.Find("ChargeSlider").GetComponent<Slider>();
                EGCText = EGChargeSlider.transform.Find("Text").GetComponent<Text>();

        //power gnerator panel
            PGGProductionSlider = PowerGeneratorPanelT.Find("ProductionSlider").GetComponent<Slider>();
            PGGFuelSlider = PowerGeneratorPanelT.Find("FuelSlider").GetComponent<Slider>();
                PGGFuelImage = PGGFuelSlider.transform.Find("Fuel").Find("Image").GetComponent<Image>();
            PGGWaterSlider = PowerGeneratorPanelT.Find("WaterSlider").GetComponent<Slider>();
                PGGWaterInText =   PGGWaterSlider.transform.Find("WaterIn").Find("Image").Find("Text").GetComponent<Text>();
                PGGWaterOutText = PGGWaterSlider.transform.Find("WaterOut").Find("Image").Find("Text").GetComponent<Text>();
        
        //Object plan items
            Transform OPIGrup = ObjectPlanItems.Find("Group");
                OPINeedItemsText = OPIGrup.Find("NeedItemsText").gameObject;
                OPINeedItemGroup = OPIGrup.Find("NeedItemGroup");
                OPIStoredItemsText = OPIGrup.Find("StoredItemsText").gameObject;
                OPIStoredItemGroup = OPIGrup.Find("StoredItemGroup");    

    //pause menu
    PauseMenuText = PauseMenu.transform.Find("Text").GetComponent<Text>();
            Transform MenuPanel = PauseMenu.transform.Find("MenuPanel");
                ResumeButton = MenuPanel.Find("ResumeButton").GetComponent<Button>();
                SaveButton = MenuPanel.Find("SaveButton").GetComponent<Button>();
    }
    public void SetAllBuildButtons()
    {
        bool wasOpen = BuildPanelSc.gameObject.activeSelf;
        if (!wasOpen) { BuildPanelSc.gameObject.SetActive(true); }
        BuildPanelSc.SetAllBuildButtons();
        if (!wasOpen) { BuildPanelSc.gameObject.SetActive(false); }

        wasOpen = BuildConPanelSc.gameObject.activeSelf;
        if (!wasOpen) { BuildConPanelSc.gameObject.SetActive(true); }
        BuildConPanelSc.SetAllBuildButtons();
        if (!wasOpen) { BuildConPanelSc.gameObject.SetActive(false); }
    }
    public void SetAllRecipesListPanel()
    {
        bool wasOpen = ProcesingPanelSc.gameObject.activeSelf;
        if (!wasOpen) { ProcesingPanelSc.gameObject.SetActive(true); }
        ProcesingPanelSc.SetAllRecipesListPanel();
        if (!wasOpen) { ProcesingPanelSc.gameObject.SetActive(false); }
    }
    public void SetPlatformItemSelectPanel()
    {
        bool wasOpen = storagePanelSc.gameObject.activeSelf;
        if (!wasOpen) { storagePanelSc.gameObject.SetActive(true); }
        SelectItemPanel.instance.SetPlatformItemSelectPanel();
        if (!wasOpen) { storagePanelSc.gameObject.SetActive(false); }
    }
    private void SetAllDronSpaces()
    {
        bool wasOpen = storagePanelSc.gameObject.activeSelf;
        if (!wasOpen) { storagePanelSc.gameObject.SetActive(true); }
        storagePanelSc.SetAllDronSpaces();
        if (!wasOpen) { storagePanelSc.gameObject.SetActive(false); }
    }

    // open one close
    public void CloseNowOpenGui()
    {
        if (nowOpenPanels.Count == 0) { return; }

        startPointRoad = new Vector2(-1, -1);

        ProcesingPanelSc.updateProgrsBarOn = false;
        ProcesingPanelSc.updateFuelProcPanel = false;

        HideResInfo();
        SelectItemPanel.instance.Hide();
        SelectResQuaPanel.instance.Hide();
        isObjectPlanItemsOpen = false; isPlatformItemsOpen = false;

        int l = nowOpenPanels.Count;
        for (int i = 0; i < l; i++)
        {
            HidePanel(nowOpenPanels[0]);
        }

        useX = -1;
        useY = -1;
        useObj = Obj.None;
        usePBSc = null;
    }
    private void ShowPanel(Transform trans)
    {
        if (nowOpenPanels.Contains(trans)) { return; }

        nowOpenPanels.Add(trans);
        trans.gameObject.SetActive(true);

        trans.DOKill();

        if (LeftPanel.instance.CanClosePanel()) { LeftPanel.instance.CloseNowOpenPanel(); }

        RectTransform rt = trans.GetComponent<RectTransform>();
        rt.DOAnchorPosX(0f, leftPanelOpenTime).SetUpdate(true);
    }
    private void HidePanel(Transform trans, bool anim=true)
    {
        nowOpenPanels.Remove(trans);

        if (trans.gameObject.activeSelf == false) { anim = false; }

        RectTransform rt = trans.GetComponent<RectTransform>();
        if (anim)
            rt.DOAnchorPosX(rt.rect.width, leftPanelCloseTime).SetUpdate(true);
        else
            rt.anchoredPosition = new Vector2(rt.rect.width + 10f, rt.anchoredPosition.y);

        if (trans == BuildConPanelT)
        {
            BuildConPanelSc.CancelBuilding();
            GSConnectionButton.GetComponent<Image>().color = disActiveButtonColor;
            GSConnectionButton.GetComponentInChildren<Text>().color = disActiveButtonColor;
            ClickMenager.instance.CancelSelectingConnection();
        }
        else if (trans == BuildPanelT)
        {
            BuildPanelSc.CancelBuilding();
            GSBuildButton.GetComponent<Image>().color = disActiveButtonColor;
            GSBuildButton.GetComponentInChildren<Text>().color = disActiveButtonColor;
        }
        else if (trans == RemovingPanelT)
        {
            RemovingPanelSc.Cancel();
            GSRemoveButton.GetComponent<Image>().color = disActiveButtonColor;
            GSRemoveButton.GetComponentInChildren<Text>().color = disActiveButtonColor;
        }
        else if (trans == ActionPanelT)
        {
            ActionPanelSc.Cancel();
            GSActionButton.GetComponent<Image>().color = disActiveButtonColor;
            GSActionButton.GetComponentInChildren<Text>().color = disActiveButtonColor;
        }
    }
    private void HideAllPanels()
    {
        HideResInfo();
        SelectItemPanel.instance.Hide();
        SelectResQuaPanel.instance.Hide();
        foreach (Transform trans in allPanelList) { HidePanel(trans, false); }
    }
    private void DisactiveClosePanles()
    {
        foreach (Transform trans in allPanelList) { Hide(trans); }
        if (CreativeUIController.instance != null) { Hide(CreativeUIController.instance.BuildGui); }

        void Hide(Transform tra)
        {
            if (tra.gameObject.activeSelf == false) { return; }
            if (nowOpenPanels.Contains(tra)) { return; }

            RectTransform rt = tra.GetComponent<RectTransform>();
            if (rt.anchoredPosition.x + 1f >= rt.rect.width) tra.gameObject.SetActive(false);
        }
    }

    // other
    private void SetTextContainerSize(Text text)
    {
        ContentSizeFitter UISizeFitterComponent = text.gameObject.GetComponent<ContentSizeFitter>();
        if (UISizeFitterComponent == null) { UISizeFitterComponent = text.gameObject.AddComponent(typeof(ContentSizeFitter)) as ContentSizeFitter; }

        //UISizeFitterComponent.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        UISizeFitterComponent.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        Canvas.ForceUpdateCanvases();
    }
    public void SetOpenPanelsYPos()
    {
        float space = 5f;
        float posY = 64f + space;
        for (int i = 0; i < nowOpenPanels.Count; i++)
        {
            RectTransform rt = nowOpenPanels[i].GetComponent<RectTransform>();
            rt.DOAnchorPosY(posY, leftPanelOpenTime).SetUpdate(true);
            posY += rt.rect.height;
            posY += space;
        }
    }
    public void SetInteractibleAllGSButtons(bool value)
    {
        GSBuildButton.GetComponent<Button>().interactable = value;
        GSConnectionButton.GetComponent<Button>().interactable = value;
        GSActionButton.GetComponent<Button>().interactable = value;
        GSRemoveButton.GetComponent<Button>().interactable = value;
        GSChooseButton.GetComponent<Button>().interactable = value;
        //GSSelectButton.GetComponent<Button>().interactable = value;

        GSPauseButton.GetComponent<Button>().interactable = value;
        GSPlayButton.GetComponent<Button>().interactable = value;
        GSSpeedx2Button.GetComponent<Button>().interactable = value;
    }
    public bool IsNowOpenPanelsContains(Transform trans) => nowOpenPanels.Contains(trans);
    public void AddToNowOpenPanels(Transform trans) => nowOpenPanels.Add(trans);
    public int GetCountOfNowOpenPanels() => nowOpenPanels.Count;
    public float GetUpPositionOfPanelIndexInNowOpenPanels(int index)
    {
        if (index < 0 || index >= nowOpenPanels.Count) return -1f;
        RectTransform rt = nowOpenPanels[index].GetComponent<RectTransform>();
        return rt.anchoredPosition.y + rt.rect.height;
    }

    /*
    public int GetIndexOfPanelFromNowOpenPanels(Transform trans) => nowOpenPanels.IndexOf(trans);
    public void SetPanelIndexInNowOpenPanels(Transform trans, int index)
    {
        if (nowOpenPanels.Contains(trans) == false || index >= nowOpenPanels.Count) return;

        if (nowOpenPanels.IndexOf(trans) < index) index--;

        if (index < 0) return;

        nowOpenPanels.Remove(trans);
        nowOpenPanels.Insert(index, trans);
    }
    */

    // Game Settings Panel
    public void ChangeSpeedButtons(int speed)
    {
        if (GSPauseButton == null) { return; }
        GSPauseButton.GetComponent<Image>().color = disActiveButtonColor;   GSPauseButton.GetComponentInChildren<Text>().color = disActiveButtonColor;
        GSPlayButton.GetComponent<Image>().color = disActiveButtonColor;    GSPlayButton.GetComponentInChildren<Text>().color = disActiveButtonColor;
        GSSpeedx2Button.GetComponent<Image>().color = disActiveButtonColor; GSSpeedx2Button.GetComponentInChildren<Text>().color = disActiveButtonColor;
        switch (speed)
        {
            case 0: GSPauseButton.GetComponent<Image>().color = activeButtonColor;   GSPauseButton.GetComponentInChildren<Text>().color = activeButtonColor;      break;
            case 1: GSPlayButton.GetComponent<Image>().color = activeButtonColor;    GSPlayButton.GetComponentInChildren<Text>().color = activeButtonColor;       break;
            case 2: GSSpeedx2Button.GetComponent<Image>().color = activeButtonColor; GSSpeedx2Button.GetComponentInChildren<Text>().color = activeButtonColor;    break;
        }
    }

    // Build panel
    public void ToggleBuildPanel()
    {
        if (BuildPanelT.gameObject.activeSelf) { CloseNowOpenGui(); }
        else { ShowBuildGui(); }
    }
    public void ShowBuildGui()
    {
        CloseNowOpenGui();
        ShowPanel(BuildPanelT);
        ClickMenager.instance.HidePointers(true);
        GSBuildButton.GetComponent<Image>().color = activeButtonColor; GSBuildButton.GetComponentInChildren<Text>().color = activeButtonColor;
    }

    // Platform Opction
    public void ShowPlatformOpction(Obj objToShow, Obj terrain, int x, int y, PlatformBehavior PBSc)
    {
        CloseNowOpenGui();
        //OpenGui in down        

        useX = x;
        useY = y;
        useObj = objToShow;
        usePBSc = PBSc;

        RectTransform rt = PlatformOptionT.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 110f);
        POButtons.SetActive(true);
        PORemovingPanel.SetActive(false);
        POSetStartPlaceButton.SetActive(false);

        bool[] objectOpction = ClickMenager.instance.GetObjectOpction(objToShow, terrain, x, y, PBSc);

        POConnectionButton.SetActive(ReadOpction(ClickMenager.ObjectOpction.conOn));
        PORemovePlatformButton.SetActive(ReadOpction(ClickMenager.ObjectOpction.removeOn));
        POBuildButton.SetActive(ReadOpction(ClickMenager.ObjectOpction.buildOn));
        POElectrickButton.SetActive(ReadOpction(ClickMenager.ObjectOpction.eleOn));
        POMineButton.SetActive(ReadOpction(ClickMenager.ObjectOpction.mineOn));
        POCutButton.SetActive(ReadOpction(ClickMenager.ObjectOpction.cutOn));
        POPlantButton.SetActive(ReadOpction(ClickMenager.ObjectOpction.plantOn));
        POAddItemsButton.SetActive(ReadOpction(ClickMenager.ObjectOpction.addItemsOn));

        //subtitle
        if (AllRecipes.instance.GetMaxHelthOfObj(objToShow) == 0) { POSubtitle.SetActive(false); }
        else { POSubtitle.SetActive(true); }

        //open
        if (usePBSc != null)
        {
            if (OpenPlatformPanels(usePBSc.usingGuiType, useObj) == false)
            { ShowPlatformItems(usePBSc); }
        }
        else if (useObj == Obj.BuildingUnderConstruction || useObj == Obj.BuildingUnderDemolition || useObj == Obj.ConUnderConstruction || useObj == Obj.ConUnderDemolition)
        { if (WorldMenager.instance.GetTransforOfObj(useX, useY).TryGetComponent(out ObjectPlan OPSc)) { ShowObjectPlanItems(OPSc); } }
        ShowPanel(PlatformOptionT);
        SetOpenPanelsYPos();

        UpdatePlatformOpction();

        bool ReadOpction(ClickMenager.ObjectOpction oo) { return objectOpction[(int)oo]; }
    }
    public void ChangePORemovingPanel()
    {
        bool bv = PORemovingPanel.activeSelf;
        PORemovingPanel.SetActive(!bv);
        POButtons.SetActive(bv);
        POSubtitle.SetActive(bv);
        
        RectTransform mRT = PlatformOptionT.GetComponent<RectTransform>();
        if (bv == false)
        {
            GameObject disassembleButtonGO = PORemovingPanel.transform.Find("DisassembleButton").gameObject;
            if (useObj == Obj.BuildingUnderDemolition || AllRecipes.instance.IsItTerrain(useObj) && useObj != Obj.Tree && useObj != Obj.Sapling && useObj != Obj.EnemysTerrain)
            { disassembleButtonGO.SetActive(false); }
            else
            {
                disassembleButtonGO.SetActive(true);
                Text disassembleButtonText = disassembleButtonGO.GetComponentInChildren<Text>();

                if (false)
                { }
                else
                { disassembleButtonText.text = "Disassemble (recover materials)"; }
            }

            GameObject demotitionButtonGO = PORemovingPanel.transform.Find("DemotitionButton").gameObject;
            if ((useObj==Obj.Tree || useObj==Obj.Sapling ) && SpaceBaseMainSc.instance.CreativeModeOn == false)
            { demotitionButtonGO.SetActive(false); }
            else
            {
                demotitionButtonGO.SetActive(true);
                Obj useTerrain = ClickMenager.instance.useTerrain;
                Text demotitionButtonText = demotitionButtonGO.GetComponentInChildren<Text>();

                if(useObj==Obj.BuildingUnderDemolition && AllRecipes.instance.IsItOreObj(useTerrain))
                { demotitionButtonText.text = "Stop mining"; }
                else
                { demotitionButtonText.text = "Rapid demolition (leaves materials)"; }
            }

            RectTransform rRT = PORemovingPanel.GetComponent<RectTransform>();
            ContentSizeFitter UISizeFitterComponent = rRT.gameObject.GetComponent<ContentSizeFitter>();
            if (UISizeFitterComponent == null) { UISizeFitterComponent = rRT.gameObject.AddComponent(typeof(ContentSizeFitter)) as ContentSizeFitter; }
            UISizeFitterComponent.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Canvas.ForceUpdateCanvases();
            mRT.sizeDelta = new Vector2(mRT.sizeDelta.x, rRT.sizeDelta.y + 6f);
        }
        else
        {
            mRT.sizeDelta = new Vector2(mRT.sizeDelta.x, 110f);
        }
        SetOpenPanelsYPos();
    }
    public void UpdatePlatformOpction()
    {
        if (SceneLoader.instance.canChooseStartPlace) { return; }

        useObj = WorldMenager.instance.GetSquer(useX, useY);

        //if(ClickMenager.instance.choosingConnection) { POSubtitle.SetActive(false); return; }

        //clicked square occupited by connection
        if (useObj == Obj.Connection1)
        {
            POName.text = "Connection";
            POSubtitle.SetActive(true); POSubtitle.GetComponent<Text>().text = string.Format("quantity: {0}", WorldMenager.instance.squeresVeribal[useX, useY]);
            return;
        }

        
        if (useObj == Obj.None) { useObj = ClickMenager.instance.useTerrain; }

        if (useObj == Obj.BuildingUnderConstruction)
        {
            Transform ObjT = WorldMenager.instance.GetTransforOfObj(useX, useY);
            if (ObjT == null) { Debug.Log("ERROR! Missing Obj In Squers Tab"); return; }
            Obj tObj = ObjT.GetComponent<ObjectPlan>().objName;
            POName.text = DisplayedNameOfObj(tObj) + " - plan";
        }
        else if(useObj == Obj.BuildingUnderDemolition)
        {
            Transform ObjT = WorldMenager.instance.GetTransforOfObj(useX, useY);
            if (ObjT == null) { Debug.Log("ERROR! Missing Obj In Squers Tab"); return; }
            Obj tObj = ObjT.GetComponent<ObjectPlan>().objName;

            if (AllRecipes.instance.IsItOreObj(tObj))
            { POName.text = DisplayedNameOfObj(tObj) + " - mining"; }
            else if (tObj == Obj.Tree)
            { POName.text = DisplayedNameOfObj(tObj) + " - cutting"; }
            else
            { POName.text = DisplayedNameOfObj(tObj) + " - dismantling"; }
            
        }
        else { POName.text = DisplayedNameOfObj(useObj); }

        if (POSubtitle.activeSelf)
        {
            POSubtitle.GetComponent<Text>().text = "";

            Square square = WorldGrid.GetSquare(useX, useY);
            if (square != null)
            {
                int maxHealth = AllRecipes.instance.GetMaxHelthOfObj(useObj);
                if (maxHealth == 0) { POSubtitle.SetActive(false); }
                else { POSubtitle.GetComponent<Text>().text += "HP: " + square.health + "/" + maxHealth; }
            }
            else
            {
                POSubtitle.SetActive(false);
            }

            if (useObj == Obj.TransmissionTower)
            {
                if (POSubtitle.GetComponent<Text>().text != "") { POSubtitle.GetComponent<Text>().text += " "; }
                int net = ElectricityManager.instance.GetNetNumOfTTPos(useX, useY);
                int charge = (int)ElectricityManager.instance.Networks[net].charge;
                int maxCharge = (int)ElectricityManager.instance.Networks[net].maxCharge;
                if (charge > maxCharge) { charge = maxCharge; }
                POSubtitle.GetComponent<Text>().text += "Charge: " + charge + "/" + maxCharge + " ";
                float production = ElectricityManager.instance.Networks[net].production;
                float request = ElectricityManager.instance.Networks[net].request;
                int delta = (int)(production - request);
                if (delta >= 0) { POSubtitle.GetComponent<Text>().text += "+"; }
                POSubtitle.GetComponent<Text>().text += delta + " kW";
            }
            else if (useObj == Obj.Battery)
            {
                if (POSubtitle.GetComponent<Text>().text != "") { POSubtitle.GetComponent<Text>().text += " "; }
                Battery batterySc = WorldMenager.instance.GetTransforOfObj(useX, useY).GetComponent<Battery>();
                float charge = batterySc.charge;
                if (charge > batterySc.Capacity) { charge = batterySc.Capacity; }
                POSubtitle.GetComponent<Text>().text += "Charge: " + (int)charge + "/" + (int)batterySc.Capacity + "kW";
            }
            else if (useObj == Obj.CombustionGenerator || useObj == Obj.SteamGenerator || useObj == Obj.SolarPanel1)
            {
                if (POSubtitle.GetComponent<Text>().text != "") { POSubtitle.GetComponent<Text>().text += " "; }
                Transform trans = WorldMenager.instance.GetTransforOfObj(useX, useY);
                ElectricityUser generatorSc = trans.GetComponent<ElectricityUser>();
                int prod = (int)generatorSc.maxEnergyPerSec;
                if (trans.TryGetComponent(out PlatformBehavior platSc)) { if (platSc.working == false) { prod = 0; } }
                POSubtitle.GetComponent<Text>().text += "Production: " + prod + " kW";
            }
            else if (useObj == Obj.ElectricSmelter || useObj == Obj.ChemicalPlant || useObj == Obj.LaserTurret || useObj == Obj.Repairer)
            {
                if (POSubtitle.GetComponent<Text>().text != "") { POSubtitle.GetComponent<Text>().text += " "; }
                Transform trans = WorldMenager.instance.GetTransforOfObj(useX, useY);
                ElectricityUser generatorSc = trans.GetComponent<ElectricityUser>();
                int prod = (int)generatorSc.maxEnergyPerSec;
                if (trans.TryGetComponent(out PlatformBehavior platSc)) { if (platSc.working == false) { prod = 0; } }
                POSubtitle.GetComponent<Text>().text += "Request: " + prod + " kW";
            }
        }
    }
    public void ShowChooseStartPlace(Obj obj)
    {
        ShowPanel(PlatformOptionT);

        POButtons.SetActive(true);
        PORemovingPanel.SetActive(false);
        POSetStartPlaceButton.SetActive(false);

        POConnectionButton.SetActive(false);
        PORemovePlatformButton.SetActive(false);
        POBuildButton.SetActive(false);
        POElectrickButton.SetActive(false);
        POMineButton.SetActive(false);
        POCutButton.SetActive(false);
        POPlantButton.SetActive(false);
        POAddItemsButton.SetActive(false);

        SetInteractibleAllGSButtons(false);

        POSetStartPlaceButton.SetActive(true);
        POName.text = "Choose start place";

        if (obj == Obj.Locked) { POSubtitle.SetActive(false); }
        else if (obj == Obj.None) { POSubtitle.SetActive(true); POSubtitle.GetComponent<Text>().text = "empty"; }
        else { POSubtitle.SetActive(true); POSubtitle.GetComponent<Text>().text = DisplayedNameOfObj(obj); }
    }
    private bool OpenPlatformPanels(PlatfotmGUIType platGUIType, Obj obj)
    {
        switch (platGUIType)
        {
            case PlatfotmGUIType.Storage:
                ShowStoragePanel();
                if (obj == Obj.Launchpad) { ShowLanchPanel(); }
                ShowRequestItemPanel();
                return true;
            case PlatfotmGUIType.Procesing:
                ProcesingPanelSc.SetProcesingScripts();
                if(AllRecipes.instance.IsObjHaveCrafterNeedFuelSc(obj) || AllRecipes.instance.IsObjHaveCrafterNeedEnergySc(obj) || AllRecipes.instance.IsUsingEnergy(obj)) { ShowFuelPanel(); }
                ShowProcesingPanel();
                ShowRequestItemPanel();
                return true;
            case PlatfotmGUIType.DronStation:
                ShowDronStationGui();
                ShowStoragePanel();
                ShowRequestItemPanel();
                return true;
            case PlatfotmGUIType.Turret:
                ShowTurretGui();
                ShowRequestItemPanel();
                return true;
            case PlatfotmGUIType.PowerGenerator:
                ShowPowerGeneratorGui();
                ShowRequestItemPanel();
                return true;
        }
        return false;
    }

    // Storage Gui
    public void ShowStoragePanel()
    {
        ShowPanel(StoragePanelT);
        storagePanelSc.ShowStorageGui();
    }
    public void ShowSetResQuaPanel(Res res, SelectResQuaPanel.SelectResQuaMode selectResMode) => SelectResQuaPanel.instance.Show(res, selectResMode);
    public void ChangeFilter(Res res) => storagePanelSc.ChangeFilter(res);

    // Launchpad
    private void ShowLanchPanel()
    {
        ShowPanel(LaunchPanelT);
        storagePanelSc.UpdateLauchPanel();
    }
    public void ClickLaunchItems()
    {
        Debug.Log("TODO: obsługa i animacje transportowców");
        for (int i = usePBSc.itemOnPlatform.Length - 1; i >= 0; i--)
        {
            int qua = usePBSc.itemOnPlatform[i].qua;
            if (qua <= 0) { continue; }
            int fp = SpaceBaseMainSc.instance.GetFreeSpaceOfStorage();
            Res res = (Res)i;
            if (qua >= fp)
            {
                usePBSc.AddItem(res, -fp);
                SpaceBaseMainSc.instance.AddResToStorage(res, fp, false);
                TaskManager.instance.AddLaunchedItem(res, fp);
                break;
            }
            else
            {
                usePBSc.AddItem(res, -qua);
                SpaceBaseMainSc.instance.AddResToStorage(res, qua, false);
                TaskManager.instance.AddLaunchedItem(res, qua);
            }
        }
        LeftPanel.instance.ShowStorageButtonBacklight();
        SpaceBaseMainSc.instance.SaveGeneralData();
        SpaceBaseMainSc.instance.SaveItems();
    }

    // Request res panel
    private void ShowRequestItemPanel()
    {
        ShowPanel(RequestItemPanelT);
        RequestPanelSc.Show();
    }

    // DronStation Gui
    private void ShowDronStationGui()
    {
        ShowPanel(DronStationpanelT);
        storagePanelSc.ShowDronStationGui();
    }

    // Connection
    public void ToggleConnectionPanel()
    {
        if (BuildConPanelT.gameObject.activeSelf) { CloseNowOpenGui(); }
        else { ShowBuildConPanel(); }
    }
    public void ShowBuildConPanel()
    {
        CloseNowOpenGui();
        ShowPanel(BuildConPanelT);
        ClickMenager.instance.HidePointers(true);
        ClickMenager.instance.SetClickMode(ClickMenager.ClickMode.Connection);
        GSConnectionButton.GetComponent<Image>().color = activeButtonColor; GSConnectionButton.GetComponentInChildren<Text>().color = activeButtonColor;
    }
    public void ShowConnectionGui(Obj connection, bool toggleIsOn, Vector2 startPos, Vector2 endPos)
    {
        useObj = connection;
        useX = (int)endPos.x;
        useY = (int)endPos.y;
        startPointRoad = startPos;

        CloseNowOpenGui();
        ShowPanel(ConnectionPanelT);
        SetOpenPanelsYPos();

        RemoveConnectionButton.SetActive(false);
        ActiveRoadToggle.SetActive(false);
        PrioritySetings.SetActive(false);

        Text platformName = ConnectionPanelT.GetComponentInChildren<Text>();

        RectTransform rt = ConnectionPanelT.GetComponent<RectTransform>();

        if (connection == Obj.None) { Debug.Log("Wrong connection type!"); return; }

        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 94f);

        int sx = (int)startPos.x; int sy = (int)startPos.y;
        int ex = (int)endPos.x; int ey = (int)endPos.y;
        string roadName = string.Format("{0}({1}, {2})({3}, {4})", connection, sx, sy, ex, ey);
        Transform PlatformT = WorldMenager.instance.GetTransforOfObj(sx, sy);
        Transform roadT = PlatformT.Find(roadName);
        if (roadT == null) { Debug.Log("ERROR! mising prefab"); return; }

        if (connection == Obj.ConUnderConstruction)
        {
            platformName.text = string.Format("{0} - plan", DisplayedNameOfObj(roadT.GetComponent<ObjectPlan>().objName));
            RemoveConnectionButton.SetActive(true);
        }
        else if (connection == Obj.ConUnderDemolition)
        {
            platformName.text = DisplayedNameOfObj(roadT.GetComponent<ObjectPlan>().objName);
            RemoveConnectionButton.SetActive(true);
        }
        else if(connection == Obj.Connection1 || connection == Obj.Connection2 || connection == Obj.Connection3)
        {
            platformName.text = DisplayedNameOfObj(connection);
            RemoveConnectionButton.SetActive(true);
            ActiveRoadToggle.SetActive(true);
            ActiveRoadToggle.GetComponent<Toggle>().isOn = toggleIsOn;
            PrioritySetings.SetActive(true);
            RoadPriorityText.text = roadT.GetComponent<RoadBehavior>().priority.ToString();
        }
       
        ConnectionPanelT.gameObject.SetActive(true);
    }
    public void UpdateRoadPriorityText(int qua)
    {
        RoadPriorityText.text = qua.ToString();
    }
    public void ChangeConRemovingPanel()
    {
        bool bv = ConRemovingPanel.activeSelf;
        ConRemovingPanel.SetActive(!bv);

        RemoveConnectionButton.SetActive(bv);
        ActiveRoadToggle.SetActive(bv);
        PrioritySetings.SetActive(bv);

        RectTransform rt = ConnectionPanelT.GetComponent<RectTransform>();
        if (bv==false)
        {
            RectTransform rrt = ConRemovingPanel.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rrt.sizeDelta.y);
            if (useObj == Obj.ConUnderDemolition) { ConRemovingPanel.transform.Find("DisassembleButton").GetComponent<Button>().interactable = false; }
            else { ConRemovingPanel.transform.Find("DisassembleButton").GetComponent<Button>().interactable = true; }
        }
        else
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 94f);
        }

        SetOpenPanelsYPos();
    }

    // Removing panel
    public void ToggleRemovingPanel()
    {
        if (RemovingPanelT.gameObject.activeSelf) { CloseNowOpenGui(); }
        else { ShowRemovingPanel(); }
    }
    public void ShowRemovingPanel()
    {
        CloseNowOpenGui();
        ShowPanel(RemovingPanelT);
        ClickMenager.instance.HidePointers(true);
        GSRemoveButton.GetComponent<Image>().color = activeButtonColor; GSRemoveButton.GetComponentInChildren<Text>().color = activeButtonColor;
    }

    // Action Panel
    public void ToggleActionPanel()
    {
        if (ActionPanelT.gameObject.activeSelf) { CloseNowOpenGui(); }
        else { ShowActionPanel(); }
    }
    public void ShowActionPanel()
    {
        CloseNowOpenGui();
        ShowPanel(ActionPanelT);
        ClickMenager.instance.HidePointers(true);
        GSActionButton.GetComponent<Image>().color = activeButtonColor; GSActionButton.GetComponentInChildren<Text>().color = activeButtonColor;
    }

    // Procesing Gui
    private void ShowProcesingPanel()
    {
        ShowPanel(ProcesingPanelT);
        UpdateProceingPanel();
    }
    public void UpdateProceingPanel() => ProcesingPanelSc.UpdateProceingPanel();

    // Fuel panel
    private void ShowFuelPanel()
    {
        ShowPanel(FuelPanelT);
        ProcesingPanelSc.UpdateFuelPanel();
    }

    // Turret Gui
    private void ShowTurretGui()
    {
        ShowPanel(TurretPanelT);

        Transform platT = WorldMenager.instance.GetTransforOfObj(useX, useY);
        useTurretSc = platT.GetComponent<Turret>();
        useEleUserSc = platT.GetComponent<ElectricityUser>();

        UpdateTurretGui();
        UpdateAmmoList();
    }
    private void UpdateTurretGui()
    {
        //chide ammo icons
        for (int i = 1; i < TGStatsPanel.childCount; i++) { TGStatsPanel.GetChild(i).gameObject.SetActive(false); }

        string stats;
        TGTurretAmmo.SetActive(false);
        if (useObj != Obj.LaserTurret) { TGPowerAmmo.SetActive(false); }
        else
        {
            TGPowerAmmo.SetActive(true);
            stats = "";
            stats += "Range: " + useTurretSc.rangeInt.ToString() + " fields";
            stats += "\nPower required: " + useTurretSc.needEnergy + "kW/shoot";
            stats += "\nFire rate: " + useTurretSc.fireRate.ToString() + " shoot/s";
            stats += "\nDamage: " + useTurretSc.dmg.ToString();
            TGStatsText.text = stats;
            return;
        }

        stats = "";
        stats += "Range: " + useTurretSc.rangeInt.ToString() + " fields";
        stats += "\nBullet capacity: " + useTurretSc.resistanceBulet.ToString();
        stats += "\nFireRate: " + useTurretSc.fireRate.ToString() + " shoot/s";

        if (useTurretSc.bulletType == Res.None)
        {
            stats += "\nUsing ammo: ";
            TGStatsText.text = stats;
            SetTextContainerSize(TGStatsText);
            for (int i = 0; i < useTurretSc.posibleAmmo.Length; i++)
            {
                Res bulletR = useTurretSc.posibleAmmo[i];
                BulletsE bulletE = BulletsE.None;
                switch (bulletR)
                {
                    case Res.Quarrel: bulletE = BulletsE.Quarrel; break;
                    case Res.Quarrel2: bulletE = BulletsE.Quarrel2; break;
                    case Res.GunMagazine: bulletE = BulletsE.GunB; break;
                    case Res.GunMagazine2: bulletE = BulletsE.GunB2; break;
                    case Res.Rocket: bulletE = BulletsE.Rocket; break;
                }

                if (bulletE == BulletsE.None) { Debug.Log("Wrong bullrt type: " + bulletR); }
                else 
                {
                    Transform BulletImage = TGStatsPanel.Find(bulletR.ToString());
                    if (BulletImage == null) { BulletImage = CreateBulletImage(bulletR, bulletE).transform; }
                    BulletImage.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            stats += "\nDamage: " + useTurretSc.dmg;
            stats += "\nUsing ammo: ";
            TGTurretAmmo.SetActive(true);
            TGTurretAmmo.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(useTurretSc.bulletType);
            stats += useTurretSc.bulletType.ToString();
            TGStatsText.text = stats;
        }

        GameObject CreateBulletImage(Res bullet, BulletsE bulletE)
        {
            GameObject newItem = Instantiate(TurretBulletAndDmgPanel, new Vector2(), Quaternion.identity);
            newItem.name = bullet.ToString();
            newItem.transform.Find("Image").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(bullet);
            newItem.transform.Find("Image").name = bullet.ToString();
            newItem.transform.Find("Text").GetComponent<Text>().text= string.Format("Dmg: {0}", BulletManager.instance.GetDmgOfBullet(bulletE));
            newItem.transform.SetParent(TGStatsPanel, false);
            newItem.transform.localScale = new Vector3(1, 1, 1);
            newItem.SetActive(true);
            return newItem;
        }
    }
    private float avaTimeToUpdateTurrGui = 0f;
    private void UpdateAmmoList()
    {
        if (useObj == Obj.LaserTurret)
        {
            if (useEleUserSc == null) { TGAmmoSlider.value = 0f; return; }
            float p = useEleUserSc.actCharge / useTurretSc.needEnergy;
            if (p > 1f) { p = 1f; }
            TGAmmoSlider.value = p;
            return;
        }

        int checkedQua;
        Res needAmmo = useTurretSc.bulletType;
        if (needAmmo == Res.None)
        {
            if (avaTimeToUpdateTurrGui <= WorldMenager.instance.worldTime) { avaTimeToUpdateTurrGui = WorldMenager.instance.worldTime + 1f; UpdateTurretGui(); }
            TGAmmoSlider.value = 0f;
        }
        else
        {
            checkedQua = usePBSc.itemOnPlatform[(int)needAmmo].qua;
            TGTurretAmmo.GetComponentInChildren<Text>().text = string.Format("{0}", checkedQua);
            float divider = useTurretSc.resistanceBulet;
            if (divider != 0f) { TGAmmoSlider.value = useTurretSc.nowResiBulet / divider; }
        }
    }

    // Electrick Gui
    public void ShowElectrickGui()
    {
        CloseNowOpenGui();
        ShowPanel(ElectrickPanelT);

        useTTSc = ElectricityManager.instance.GetTTOfMyPos(useX, useY);

        UpdateElectrickGui();
    }
    private void UpdateElectrickGui()
    {
        if (useTTSc == null) { MessageManager.instance.ShowMessage(Messages.NoTransmisonTower); CloseNowOpenGui(); return; }

        EleNetwork useNet = ElectricityManager.instance.Networks[useTTSc.network];

        float t = useNet.production; if (t > useNet.maxProduction) { t = useNet.maxProduction; }
        EGProductionSlider.value = t / useNet.maxProduction;
        EGPText.text = string.Format("{0}/{1} kW", (int)t, useNet.maxProduction);

        t = useNet.request; if (t > useNet.maxRequest) { t = useNet.maxRequest; }
        EGRequestSlider.value = t / useNet.maxRequest;
        EGRText.text = string.Format("{0}/{1} kW", (int)t, useNet.maxRequest);

        t = useNet.charge; if (t > useNet.maxCharge) { t = useNet.maxCharge; }
        EGChargeSlider.value = t / useNet.maxCharge;
        EGCText.text = string.Format("{0}/{1} kW", (int)t, useNet.maxCharge);
    }

    // Power Generator
    private void ShowPowerGeneratorGui()
    {
        ShowPanel(PowerGeneratorPanelT);

        Transform tra = WorldMenager.instance.GetTransforOfObj(useX, useY);

        useSteamGeneratorSc = tra.GetComponent<SteemGenerator>();
        useEleUserSc = tra.GetComponent<ElectricityUser>();

        UpdatePowerGeneratorGui();
    }
    private void UpdatePowerGeneratorGui()
    {
        if (useSteamGeneratorSc.useFuel == Res.None)
        {
            PGGFuelImage.gameObject.SetActive(false);
            PGGFuelSlider.value = 0;
        }
        else
        {
            PGGFuelImage.gameObject.SetActive(true);
            PGGFuelImage.sprite = ImageLibrary.instance.GetResImage(useSteamGeneratorSc.useFuel);
            PGGFuelImage.transform.Find("Text").GetComponent<Text>().text = usePBSc.itemOnPlatform[(int)useSteamGeneratorSc.useFuel].qua.ToString();
            PGGFuelSlider.value = useSteamGeneratorSc.percRemFuel;
        }

        if (useObj != Obj.SteamGenerator)
        {
            PGGWaterSlider.gameObject.SetActive(false);
        }
        else
        {
            PGGWaterSlider.gameObject.SetActive(true);
            PGGWaterInText.text = usePBSc.itemOnPlatform[(int)Res.BottleWater].qua.ToString();
            PGGWaterOutText.text = usePBSc.itemOnPlatform[(int)Res.BottleEmpty].qua.ToString();
            PGGWaterSlider.value = useSteamGeneratorSc.percRemWater;
        }

        if (useEleUserSc != null)
        {
            float percent = 0f;
            if(usePBSc.working && useEleUserSc.maxEnergyPerSec > 0f) { percent = useEleUserSc.actEnergyPerSec / useEleUserSc.maxEnergyPerSec; }
            PGGProductionSlider.value = percent;
        }
    }

    // wallsC
    public void SetWallsC(Vector2Int pos, Obj type)
    {
        Sprite wImage = ImageLibrary.instance.GetObjImages(type);
        if (wImage == null) { Debug.Log("ERROR! Building Image (" + type + ") is missing!"); return; }
        for (int i = 0; i < WallsC.transform.childCount; i++)
        {
            int x = pos.x; int y = pos.y;
            Transform child = WallsC.transform.GetChild(i);
            switch (child.name)
            {
                case "N1": y++; break;
                case "N2": y+=2; break;
                case "S1": y--; break;
                case "S2": y-=2; break;
                case "E1": x++; break;
                case "E2": x+=2; break;
                case "W1": x--; break;
                case "W2": x-=2; break;
            }
            if (WorldMenager.instance.GetSquer(x, y) == Obj.None)
            {
                child.GetComponent<Image>().sprite = wImage;
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }

        WallsC.transform.position = new Vector3(pos.x, pos.y, 0) * 10;
        WallsC.SetActive(true);

        useX = pos.x;
        useY = pos.y;
        useObj = type;
    }
    public void HideWallC() => WallsC.SetActive(false);
    public void ClickWallC()
    {
        int x = useX;
        int y = useY;
        Obj obj = useObj;
        string name = EventSystem.current.currentSelectedGameObject.name;
        switch (name)
        {
            case "N1": y++; break;
            case "N2": y++; break;
            case "S1": y--; break;
            case "S2": y--; break;
            case "E1": x++; break;
            case "E2": x++; break;
            case "W1": x--; break;
            case "W2": x--; break;
        }
        BuildMenager.instance.AddObjToBuild(obj, x, y, new Vector2Int());
        if (name[1] == '1') { return; }
        switch (name)
        {
            case "N2": y++; break;
            case "S2": y--; break;
            case "E2": x++; break;
            case "W2": x--; break;
        }
        BuildMenager.instance.AddObjToBuild(obj, x, y, new Vector2Int());
    }

    // Res Info
    public void ShowResInfo()
    {
        PointerEventData PED = new PointerEventData(EventSystem.current);
        PED.position = Input.mousePosition;

        List<RaycastResult> RResult = new List<RaycastResult>();
        EventSystem.current.RaycastAll(PED, RResult);
        if (RResult.Count == 0) { return; }
        GameObject clickedGO = RResult[0].gameObject;
        string name = clickedGO.name;
        if (Enum.TryParse(name, out Res checkedRes) == false) { return; }
        if (checkedRes == Res.None) { return; }

        ResInfoPanle.SetActive(true);

        Vector2 panelSize = ResInfoPanle.GetComponent<RectTransform>().sizeDelta;
        panelSize.x *= 0.6f * transform.lossyScale.x;
        panelSize.y *= 0.5f * transform.lossyScale.y;
        panelSize.y += clickedGO.GetComponent<RectTransform>().sizeDelta.y;

        Vector2 Pos = clickedGO.GetComponent<RectTransform>().position;
        if (Pos.x > Screen.width - panelSize.x) { Pos.x = Screen.width - panelSize.x; }
        else if (Pos.x < panelSize.x) { Pos.x = panelSize.x; }
        if (Pos.y > Screen.height / 2) { Pos.y -= panelSize.y * 0.8f; }
        else { Pos.y += panelSize.y * 0.8f; }
        ResInfoPanle.transform.position = Pos;

        ResInfoPanle.transform.Find("ResName").GetComponent<Text>().text = Language.NameOfRes(checkedRes);
        ResInfoPanle.transform.Find("MainItem").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(checkedRes);

        Obj prodObj = Obj.None;
        List<Res> needRess = new List<Res>();

        switch (checkedRes)
        {
            case Res.Wood: prodObj = Obj.Woodcuter; break;
            case Res.Sapling: prodObj = Obj.Woodcuter; break;
            case Res.StoneOre: prodObj = Obj.Quarry; break;
            case Res.CopperOreCtm: prodObj = Obj.Quarry; break;
            case Res.CopperOre: prodObj = Obj.Pulverizer; break;
            case Res.Sand: prodObj = Obj.Quarry; break;
            case Res.IronOre: prodObj = Obj.Quarry; break;
            case Res.BottleOil: prodObj = Obj.Pump; break;
            case Res.BottleWater: prodObj = Obj.Pump; break;
        }

        if (prodObj == Obj.None) { SetObj(); }

        string text = "";
        if (prodObj != Obj.None) { text += string.Format("Can be produced in {0}", DisplayedNameOfObj(prodObj)); }
        if (needRess.Count > 0) { text += "\nfrom:"; }
        ResInfoPanle.transform.Find("Text").GetComponent<Text>().text = text;

        Transform images = ResInfoPanle.transform.Find("Images");
        int nrC = needRess.Count;
        for (int i = 0; i < 5; i++)
        {
            if (i >= nrC) { images.GetChild(i).gameObject.SetActive(false); }
            else { images.GetChild(i).gameObject.SetActive(true); images.GetChild(i).GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(needRess[i]); }
        }

        void SetObj()
        {
            foreach (Obj objWCR in AllRecipes.instance.objectWithCraftRecipes)
            {
                foreach (CraftRecipe recipe in AllRecipes.instance.GetCraftRecipes(objWCR))
                {
                    if (ChechAndSet(recipe, objWCR)) { return; }
                }
            }

            bool ChechAndSet(CraftRecipe recipe, Obj obj)
            {
                foreach (ItemRAQ iir in recipe.ItemOut)
                {
                    if (iir.res == checkedRes) 
                    {
                        prodObj = obj;
                        for (int i = 0; i < recipe.ItemIn.Count; i++) { needRess.Add(recipe.ItemIn[i].res); }
                        return true;
                    }
                }
                return false;
            }
        }
    }
    public void HideResInfo()
    {
        ResInfoPanle.SetActive(false);
    }

    // Pause Menu
    public void ShowPauseMenu()
    {
        PauseMenuText.text = "PAUSE";
        ResumeButton.interactable = true;
        SaveButton.interactable = true;
        PauseMenu.SetActive(true);
    }
    public void HidePauseMenu()
    {
        PauseMenu.SetActive(false);
        GameSettingsPanel.gameObject.SetActive(true);
    }
    public void ShowLoseMenu()
    {
        CloseNowOpenGui();
        PlatformOptionT.gameObject.SetActive(false);
        ClickMenager.instance.HidePointers(true);

        PauseMenuText.text = "GAME OVER";
        ResumeButton.interactable = false;
        SaveButton.interactable = false;
        PauseMenu.SetActive(true);
    }

    // Object Items
    private bool isObjectPlanItemsOpen = false;
    private ObjectPlan OIOPSc = null;
    private bool isPlatformItemsOpen = false;
    private PlatformBehavior OIPBSc = null;
    private void ShowObjectPlanItems(ObjectPlan OPSc)
    {
        ShowPanel(ObjectPlanItems);
        isObjectPlanItemsOpen = true;
        OIOPSc = OPSc;
    }
    private void ShowPlatformItems(PlatformBehavior PBSc)
    {
        ShowPanel(ObjectPlanItems);
        isPlatformItemsOpen = true;
        OIPBSc = PBSc;
    }
    float OILastheight=0f;
    private void UpdateObjectItems()
    {
        if (isObjectPlanItemsOpen) { UpdateObjectPlanItems(); return; }
        if (isPlatformItemsOpen) { UpdateObjectPlatformSc(); return; }

        void UpdateObjectPlanItems()
        {
            float panelHeight = 25;

            foreach (Transform child in OPINeedItemGroup) { child.gameObject.SetActive(false); }
            foreach (Transform child in OPIStoredItemGroup) { child.gameObject.SetActive(false); }

            GameObject Item = OPINeedItemGroup.GetChild(0).gameObject;

            if (OIOPSc.needItems.Count == 0) { OPINeedItemsText.SetActive(false); }
            else
            {
                for (int i = 0; i < OIOPSc.needItems.Count; i++)
                {
                    Transform itemT;
                    if (i < OPINeedItemGroup.childCount) { itemT = OPINeedItemGroup.GetChild(i); }
                    else { itemT = CreateItem(OPINeedItemGroup, Item); }
                    ItemRAQ itemR = OIOPSc.needItems[i];
                    itemT.gameObject.SetActive(true);
                    itemT.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(itemR.res);
                    itemT.GetComponentInChildren<Text>().text = string.Format("x{0} {1}", itemR.qua, Language.NameOfRes(itemR.res));
                }
                panelHeight += 25;
                panelHeight += OIOPSc.needItems.Count * 40;
                OPINeedItemsText.SetActive(true);
            }

            if (OIOPSc.keptItems.Count == 0) { OPIStoredItemsText.SetActive(false); }
            else
            {
                for (int i = 0; i < OIOPSc.keptItems.Count; i++)
                {
                    Transform itemT;
                    if (i < OPIStoredItemGroup.childCount) { itemT = OPIStoredItemGroup.GetChild(i); }
                    else { itemT = CreateItem(OPIStoredItemGroup, Item); }
                    ItemRAQ itemR = OIOPSc.keptItems[i];
                    itemT.gameObject.SetActive(true);
                    itemT.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(itemR.res);
                    itemT.GetComponentInChildren<Text>().text = string.Format("x{0} {1}", itemR.qua, Language.NameOfRes(itemR.res));
                }
                panelHeight += 25;
                panelHeight += OIOPSc.keptItems.Count * 40;
                OPIStoredItemsText.gameObject.SetActive(true);
            }

            RectTransform PRT = ObjectPlanItems.GetComponent<RectTransform>();
            PRT.sizeDelta = new Vector2(PRT.sizeDelta.x, panelHeight);

            if (OILastheight != panelHeight)
            {
                OILastheight = panelHeight;
                SetOpenPanelsYPos();
            }
        }
        
        void UpdateObjectPlatformSc()
        {
            float panelHeight = 25;

            foreach (Transform child in OPINeedItemGroup) { child.gameObject.SetActive(false); }
            foreach (Transform child in OPIStoredItemGroup) { child.gameObject.SetActive(false); }

            GameObject Item = OPINeedItemGroup.GetChild(0).gameObject;

            OPINeedItemsText.SetActive(false);

            int numOfItem = 0;
            for (int i = 1; i < OIPBSc.itemOnPlatform.Length; i++)
            {
                int qua = OIPBSc.itemOnPlatform[i].qua;
                if (qua <= 0) { continue; }
                Transform itemT;
                if (i < OPIStoredItemGroup.childCount) { itemT = OPIStoredItemGroup.GetChild(numOfItem); }
                else { itemT = CreateItem(OPIStoredItemGroup, Item); }

                Res res = (Res)i;
                itemT.gameObject.SetActive(true);
                itemT.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
                itemT.GetComponentInChildren<Text>().text = string.Format("x{0} {1}", qua, Language.NameOfRes(res));
                numOfItem++;
            }
            panelHeight += 25;
            panelHeight += numOfItem * 40;
            OPIStoredItemsText.gameObject.SetActive(true);

            RectTransform PRT = ObjectPlanItems.GetComponent<RectTransform>();
            PRT.sizeDelta = new Vector2(PRT.sizeDelta.x, panelHeight);

            if (OILastheight != panelHeight)
            {
                OILastheight = panelHeight;
                SetOpenPanelsYPos();
            }
        }

        Transform CreateItem(Transform parent, GameObject item)
        {
            GameObject newItem = Instantiate(item);
            newItem.transform.SetParent(parent, false);
            newItem.name = "Item";
            return newItem.transform;
        }
    }
}