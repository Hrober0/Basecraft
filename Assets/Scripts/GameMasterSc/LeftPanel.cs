using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
using System;

public class LeftPanel : MonoBehaviour
{
    [Header("Main buttons")]
    [SerializeField] private GameObject ExitButton = null;
    [SerializeField] private GameObject CloseButon = null;
    [SerializeField] private GameObject TransportButton = null;
    [SerializeField] private GameObject StorageButton = null;
    [SerializeField] private GameObject ScienceButton = null;
    [SerializeField] private GameObject GlossaryButton = null;
    [SerializeField] private GameObject SettingsButon = null;

    [Header("Transport")]
    [SerializeField] private Transform TransportPanel = null;

    [Header("Storage")]
    [SerializeField] private Transform StoragePanel = null;
    [SerializeField] private GameObject StorageItemPanel = null;
    [SerializeField] private Text StorageItemButtonText = null;
    [SerializeField] private Text StorageDumpsterButtonText = null;
    [SerializeField] private GameObject StorageRemoveItemPanel = null;
    //storage
    [SerializeField] private GameObject ItemInStorage = null;
    [SerializeField] private Toggle StorageRemoveItemsToggle = null;
    [SerializeField] private Transform StorageItemGroup = null;
    [SerializeField] private GameObject StorageNoItemInMagazineText = null;
    //dumpster
    [SerializeField] private GameObject ItemInDumpster = null;
    [SerializeField] private GameObject DumpsterPanel = null;
    [SerializeField] private Transform DumpsterItemGroup = null;
    [SerializeField] private Slider DumpsterRemogingItemSlider = null;
    [SerializeField] private GameObject StorageNoItemIsDumpsterText = null;

    [Header("Science")]
    [SerializeField] private Transform SciencePanel = null;
    [SerializeField] private Transform TechGroup = null;
    [SerializeField] private GameObject TechCurtain = null;
    [SerializeField] private Sprite TechCurainImage = null;
    [SerializeField] private GameObject TechDoneImg = null;
    [SerializeField] private GameObject TechInfoPanel = null;
    [SerializeField] private Color TechTextDefColor = new Color();
    [SerializeField] private Color TechTextMisColor = new Color();
    [SerializeField] private Color TechDisactivColor = new Color();
    [SerializeField] private GameObject TechInfoNeedItemsPanel = null;
    [SerializeField] private GameObject TechInfoNeedTechText = null;
    private RectTransform infoPanleRT;
    private GameObject NeedItemsGroup;
    private GameObject NeedTechGroup;
    private GameObject NeedTechText;
    private GameObject RButtonGO;
    private RectTransform DescryptionRT;

    [Header("Task")]
    [SerializeField] private Transform TaskPanel = null;

    [Header("Glossary")]
    [SerializeField] private Transform GlossaryPanel = null;
    [SerializeField] private Text GlossaryItemsButtonText = null;
    [SerializeField] private Text GlossaryBuildingsButtonText = null;
    //items
    [SerializeField] private GameObject GlossaryItemBorder = null;
    [SerializeField] private GameObject GlossaryItemsPanel = null;
    [SerializeField] private GameObject ItemInGlossary = null;
    [SerializeField] private GameObject ItemInGlossaryRecipe = null;
    [SerializeField] private Transform GlossaryItemGroup = null;
    [SerializeField] private Transform GlossaryItemInfoPanel = null;
    //buildings
    [SerializeField] private GameObject GlossaryBuildingsPanel = null;
    [SerializeField] private Transform GlossaryBuildingsGroup = null;
    [SerializeField] private GameObject BuildingInGlossary = null;
    [SerializeField] private Text GlossaryBuildingDescriptionText = null;
    [SerializeField] private Transform GlossaryProductsPanel = null;
    [SerializeField] private Text GlossaryStatsText = null;

    [Header("Settings")]
    [SerializeField] private Transform SettingsPanel = null;
    [SerializeField] private Toggle resImageInBuildingToggl = null;
    [SerializeField] private Toggle fullscreenToggl = null;
    [SerializeField] private Dropdown autoSaveFreqDropdown = null;
    [SerializeField] private Dropdown usingLanguageDropdown = null;
    [SerializeField] private Slider MusicVolumeSlider = null;
    [SerializeField] private Slider SoundsVolumeSlider = null;
    [SerializeField] private GameObject SPShowTutorialButton = null;
    [SerializeField] private GameObject SPLeaveColonyButton = null;
    [SerializeField] private GameObject SPExitGameButton = null;
    [SerializeField] private GameObject SPBackToMenuButton = null;

    [Header("ConfirmPanel")]
    [SerializeField] private GameObject ConfirmPanel = null;
    [SerializeField] private Text ConfirmPanelText = null;
    [SerializeField] private GameObject ConfirmPanelCancelButton = null;
    [SerializeField] private GameObject ConfirmPanelBackToMenuButton = null;
    [SerializeField] private GameObject ConfirmPanelExitButton = null;
    [SerializeField] private GameObject ConfirmPanelLeaveColonyButton = null;
    [SerializeField] private GameObject ConfirmPanelResetProgresButton = null;

    [Header("Other")]
    [SerializeField] private GameObject GameSavingPanel = null;
    [SerializeField] private GameObject UpdateButtonLed = null;
    [SerializeField] private Color ledOnColor = new Color();
    [SerializeField] private Color ledOfColor = new Color();
    [SerializeField] private Color activeColor = new Color();
    [SerializeField] private Color disactiveColor = new Color();
    [SerializeField] private Color disactiveTextColor = new Color();
    private bool panelNeedBeClose = false;
    private float timeToClose = 0f;
    [SerializeField] private float updateUiDelay = 0.5f;
    private float timeToUpdateGui = 0.5f;
    public Transform nowOpenPanel = null;
    private Image nowActiveButton = null;

    public static LeftPanel instance;
    private bool willBeDestroy = false;
    private void Awake()
    {
        if (instance != null) { Debug.LogWarning("More then one leftPnael on scne!"); willBeDestroy = true; return; }
        instance = this;
    }
    private void Start()
    {
        if (willBeDestroy) { return; }
        SetObjects();

        HideAllPanels();

        SetStoragePanel();
        SetGlossaryPanel();
        SetSciencePanel();
        UpdateSettingsPanel();

        SetActiveGameSavingPanel(false);
    }
    private void Update()
    {
        if (panelNeedBeClose)
        {
            timeToClose -= Time.unscaledDeltaTime;
            if (timeToClose <= 0)
            {
                panelNeedBeClose = false;
                if (nowOpenPanel != null) { CloseNowOpenPanel(); }
            }
        }

        if (timeToUpdateGui <= 0f)
        {
            timeToUpdateGui = updateUiDelay;
            UpdateStorageUI();
            DisactiveClosePanel();
        }
        timeToUpdateGui -= Time.unscaledDeltaTime;
    }

    //open and close
    public void OpenPanel(Transform trans)
    {
        nowOpenPanel = trans;
        
        if (trans == SciencePanel || trans == TaskPanel)
        {
            trans.DOScale(Vector2.one, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);
            trans.GetComponent<CanvasGroup>().DOFade(1f, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);
        }
        else 
        {
            RectTransform rt = trans.GetComponent<RectTransform>();
            rt.DOAnchorPosX(0f, 0.4f).SetUpdate(true);
        }

        if (GuiControler.instance != null)
        {
            if (CanClosePanel()) GuiControler.instance.CloseNowOpenGui();
        }
        if (ClickMenager.instance != null)
        {
            ClickMenager.instance.HidePointers(true);
        }

        CloseButon.GetComponentInChildren<Text>().text = "Close";
        CloseButon.SetActive(true); ExitButton.SetActive(false);
    }
    public void CloseNowOpenPanel()
    {
        if (nowOpenPanel == TaskPanel)
        {
            TaskManager.instance.SetTaskPanelAsClose();
            MessageManager.instance.OffHighlightBorderOfTask();
        }

        if (nowOpenPanel != null)
        {
            if (nowOpenPanel == SciencePanel || nowOpenPanel == TaskPanel)
            {
                nowOpenPanel.DOScale(Vector2.zero, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);
                nowOpenPanel.GetComponent<CanvasGroup>().DOFade(0f, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);
            }
            else
            {
                RectTransform rt = nowOpenPanel.GetComponent<RectTransform>();
                rt.DOAnchorPosX(-rt.rect.width, 0.4f).SetUpdate(true);
            }
            nowOpenPanel = null;
        }

        if (nowActiveButton != null)
        {
            nowActiveButton.color = disactiveColor;
            nowActiveButton.GetComponentInChildren<Text>().color = disactiveTextColor;
            nowActiveButton = null;
        }

        MessageManager.instance.SetYMessOffset(0);

        SetCloseButton();
    }
    private void HideAllPanels()
    {
        HideDifOpen(SciencePanel); HideDifOpen(TaskPanel);
        Hide(StoragePanel); Hide(GlossaryPanel); Hide(SettingsPanel); Hide(TransportPanel);

        void Hide(Transform trans)
        {
            RectTransform rt = trans.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(64f - rt.rect.width, rt.anchoredPosition.y);
        }
        void HideDifOpen(Transform trans)
        {
            trans.localScale = Vector2.zero;
            trans.GetComponent<CanvasGroup>().alpha = 0f;
        }
    }
    private void DisactiveClosePanel()
    {
        HideDifOpen(SciencePanel); HideDifOpen(TaskPanel);
        Hide(StoragePanel); Hide(GlossaryPanel); Hide(SettingsPanel); Hide(TransportPanel);

        void Hide(Transform trans)
        {
            if (trans.gameObject.activeSelf == false) { return; }
            if (trans == nowOpenPanel) { return; }

            RectTransform rt = trans.GetComponent<RectTransform>();
            if (rt.anchoredPosition.x <= 70f - rt.rect.width) { trans.gameObject.SetActive(false); }
        }
        void HideDifOpen(Transform trans)
        {
            if (trans.gameObject.activeSelf == false) { return; }
            if (trans == nowOpenPanel) { return; }

            if (trans.localScale.x <= 0.1f) { trans.gameObject.SetActive(false); trans.GetComponent<CanvasGroup>().alpha = 0f; }
        }
    }
    public bool CanClosePanel()
    {
        if (nowOpenPanel == TaskPanel || nowOpenPanel == GlossaryPanel || nowOpenPanel == StoragePanel) { return false; }
        return true;
    }

    //other
    private void SetTextContainerSize(Text text)
    {
        ContentSizeFitter UISizeFitterComponent = text.gameObject.GetComponent<ContentSizeFitter>();
        if (UISizeFitterComponent == null) { UISizeFitterComponent = text.gameObject.AddComponent(typeof(ContentSizeFitter)) as ContentSizeFitter; }

        UISizeFitterComponent.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        UISizeFitterComponent.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        Canvas.ForceUpdateCanvases();
    }
    private void SetObjects()
    {
        infoPanleRT = TechInfoPanel.GetComponent<RectTransform>();
        NeedItemsGroup = TechInfoPanel.transform.Find("NeeditemsGroup").gameObject;
        NeedTechGroup = TechInfoPanel.transform.Find("NeedTechGroup").gameObject;
        NeedTechText = TechInfoPanel.transform.Find("NeedTechText").gameObject;
        RButtonGO = TechInfoPanel.transform.Find("Button").gameObject;
        DescryptionRT = TechInfoPanel.transform.Find("Descryption").GetComponent<RectTransform>();
    }
    private void ShowButtonBacklight(Transform parent)
    {
        if (nowActiveButton != null && nowActiveButton.transform == parent) { return; }

        Transform led = parent.Find("UpdateLed");
        if (led == null)
        {
            led = Instantiate(UpdateButtonLed).transform;
            led.SetParent(parent, false);
            led.name = "UpdateLed";
        }
        
        led.gameObject.SetActive(true);

        float mrygTime = 0.5f;
        Image image = led.GetComponent<Image>();

        led.DOScale(new Vector3(1.1f, 1.1f, 1f), mrygTime).SetEase(Ease.InQuad).SetUpdate(true);
        image.DOColor(ledOnColor, mrygTime).SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(() => makeOff());

        void makeOn()
        {
            if (led.gameObject.activeSelf == false) { return; }
            led.DOScale(new Vector3(1.1f, 1.1f, 1f), mrygTime).SetEase(Ease.InQuad).SetDelay(0.3f).SetUpdate(true);
            image.DOColor(ledOnColor, mrygTime).SetEase(Ease.OutQuad).SetDelay(0.3f).SetUpdate(true).OnComplete(() => makeOff());
        }
        void makeOff()
        {
            if (led.gameObject.activeSelf == false) { return; }
            led.DOScale(new Vector3(1f, 1f, 1f), mrygTime).SetEase(Ease.OutQuad).SetDelay(0.5f).SetUpdate(true);
            image.DOColor(ledOfColor, mrygTime).SetEase(Ease.InQuad).SetDelay(0.5f).SetUpdate(true).OnComplete(() => makeOn());
        }
    }
    private void OffButtonLed(Transform parent)
    {
        Transform led = parent.Find("UpdateLed");
        if (led == null) { return; }
        
        led.gameObject.SetActive(false);
    }
    public void SetActiveGameSavingPanel(bool active) => GameSavingPanel.SetActive(active);

    //Close Button
    public void ClickCloseButton()
    {
        if (nowOpenPanel != null) { CloseNowOpenPanel(); }
        else { BackToMenu(); }
    }
    public void BackToMenu() => SpaceBaseMainSc.instance.BackToMenu();
    public void SetCloseButton()
    {
        CloseButon.GetComponentInChildren<Text>().text = "Menu";
        if (SceneLoader.instance.gameMode == GameState.SpaceStation)
        {
            CloseButon.SetActive(false);
            ExitButton.SetActive(true);

            SPShowTutorialButton.SetActive(false);
            SPBackToMenuButton.SetActive(false);
            SPLeaveColonyButton.SetActive(false);
            SPExitGameButton.SetActive(true);
        }
        else
        {
            CloseButon.SetActive(true);
            ExitButton.SetActive(false);

            SPShowTutorialButton.SetActive(true);
            SPBackToMenuButton.SetActive(true);
            SPLeaveColonyButton.SetActive(true);
            SPExitGameButton.SetActive(false);
        }
    }
    public void QuitGame(bool saveGeneralData=true)
    {
        Debug.Log("Exiting..");
        if (saveGeneralData) { SpaceBaseMainSc.instance.SaveGeneralData(); }
        Application.Quit();
    }

    //Transport
    public void ClickTransportButton()
    {
        if (nowOpenPanel == TransportPanel) { CloseNowOpenPanel(); return; }
        else { OpenTransportPanel(); }
    }
    public void OpenTransportPanel()
    {
        if (nowOpenPanel == TransportPanel) { return; }
        CloseNowOpenPanel();
        OpenPanel(TransportPanel);
        //MessageManager.instance.SetVisibilityMess(false);
        MessageManager.instance.SetMaxNumOfMess(4);
        TransportPanel.gameObject.SetActive(true);
        nowActiveButton = TransportButton.GetComponent<Image>();
        nowActiveButton.color = activeColor;
        nowActiveButton.GetComponentInChildren<Text>().color = activeColor;
    }
    public void ShowTransportButtonBacklight() => ShowButtonBacklight(TransportButton.transform);

    //Storage
    private int storageSelectPage = 1;
    private GameObject[] ItemsInStorageGO;
    private GameObject[] ItemsInDumpsterGO;
    [SerializeField] private bool remveItemFromStorage = false;
    private Res storageSelectRes = Res.None;
    private int storageSelectValue = 0;
    public void ClickStorageButton()
    {
        if (nowOpenPanel == StoragePanel) { CloseNowOpenPanel(); return; }
        OpenStoragePanel();
    }
    public void OpenStoragePanel()
    {
        if (nowOpenPanel == StoragePanel) { return; }
        CloseNowOpenPanel();
        OpenPanel(StoragePanel);
        //MessageManager.instance.SetVisibilityMess(false);
        MessageManager.instance.SetMaxNumOfMess(4);
        OffButtonLed(StorageButton.transform);
        StoragePanel.gameObject.SetActive(true);
        nowActiveButton = StorageButton.GetComponent<Image>();
        nowActiveButton.color = activeColor;
        nowActiveButton.GetComponentInChildren<Text>().color = activeColor;

        StorageRemoveItemsToggle.gameObject.SetActive(false);
        StorageRemoveItemsToggle.isOn = false;
        StorageRemoveItemsToggle.gameObject.SetActive(true);
        SetStorageRemovingOption(false);
        StorageRemoveItemPanel.SetActive(false);
        UpdateStorageUI();
    }
    private void SetStoragePanel()
    {
        ItemInStorage.SetActive(false);
        ItemInDumpster.SetActive(false);
        int NumberOfRes = SpaceBaseMainSc.instance.numberOfRes;
        ItemsInStorageGO = new GameObject[NumberOfRes];
        ItemsInDumpsterGO = new GameObject[NumberOfRes];

        ClickStoragePage(1);
    }
    public void SetStorageRemovingOption(bool value)
    {
        remveItemFromStorage = value;
        foreach (Transform trans in StorageItemGroup)
        {
            trans.Find("RemoveIcon").gameObject.SetActive(value);
        }
    }
    public void UpdateStorageUI()
    {
        if (nowOpenPanel != StoragePanel) { return; }

        switch (storageSelectPage)
        {
            case 1: UpdateStorage(); break;
            case 2: UpdateDumpster(); break;
        }
    }
    private void UpdateStorage()
    {
        int numberOfRes = SpaceBaseMainSc.instance.numberOfRes;
        for (int i = 1; i < numberOfRes; i++)
        {
            Res res = (Res)i;
            int qua = SpaceBaseMainSc.instance.GetResQuaOfStorage(res);
            if (qua <= 0) { if (ItemsInStorageGO[i] != null) { ItemsInStorageGO[i].SetActive(false); } }
            else
            {
                if (ItemsInStorageGO[i] == null) { CreateNewItem(res); }
                ItemsInStorageGO[i].SetActive(true);
                ItemsInStorageGO[i].GetComponentInChildren<Text>().text = qua.ToString();
            }
        }

        int maxSpace = SpaceBaseMainSc.instance.GetMaxEmptySpaceOfStorage();
        int actItems = maxSpace - SpaceBaseMainSc.instance.GetFreeSpaceOfStorage();
        if (actItems <= 0) { actItems = 0; StorageNoItemInMagazineText.SetActive(true); }
        else { StorageNoItemInMagazineText.SetActive(false); }
        StorageItemPanel.transform.Find("AvaSpaceText").GetComponent<Text>().text = string.Format("Item in magazine: {0}/{1}", actItems, maxSpace);

        void CreateNewItem(Res res)
        {
            int qua = SpaceBaseMainSc.instance.GetResQuaOfStorage(res);
            GameObject newItem = Instantiate(ItemInStorage, new Vector2(), Quaternion.identity);
            newItem.transform.SetParent(StorageItemGroup, false);
            newItem.name = res.ToString();
            newItem.transform.Find("Image").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            newItem.transform.Find("Text").GetComponent<Text>().text = qua.ToString();
            if (qua <= 0) { newItem.SetActive(false); }
            else { newItem.SetActive(true); }
            ItemsInStorageGO[(int)res] = newItem;
        }
    }
    private void UpdateDumpster()
    {
        int numberOfRes = SpaceBaseMainSc.instance.numberOfRes;
        for (int i = 1; i < numberOfRes; i++)
        {
            Res res = (Res)i;
            int qua = SpaceBaseMainSc.instance.GetResQuaOfDumpster(res);
            if (qua <= 0) { if (ItemsInDumpsterGO[i] != null) { ItemsInDumpsterGO[i].SetActive(false); } }
            else
            {
                if (ItemsInDumpsterGO[i] == null) { CreateNewItem(res); }
                ItemsInDumpsterGO[i].SetActive(true);
                ItemsInDumpsterGO[i].GetComponentInChildren<Text>().text = qua.ToString();
            }
        }

        int maxSpace = SpaceBaseMainSc.instance.GetMaxEmptySpaceOfDumpster();
        int actItems = maxSpace - SpaceBaseMainSc.instance.GetFreeSpaceOfDumpster();
        if (actItems <= 0) { actItems = 0; StorageNoItemIsDumpsterText.SetActive(true); }
        else { StorageNoItemIsDumpsterText.SetActive(false); }
        DumpsterPanel.transform.Find("AvaSpaceText").GetComponent<Text>().text = string.Format("Item in dumpster: {0}/{1}", actItems, maxSpace);
        DumpsterPanel.transform.Find("TimeToRemoveText").GetComponent<Text>().text = string.Format("Removing item time: {0}s", SpaceBaseMainSc.instance.dumpsterItemRemovingTime);
        if (SpaceBaseMainSc.instance.activeRemovingBar) {
            float maxValue = SpaceBaseMainSc.instance.dumpsterItemRemovingTime;
            DumpsterRemogingItemSlider.value = maxValue - SpaceBaseMainSc.instance.avaTimeToRemoveItem;
            DumpsterRemogingItemSlider.maxValue = maxValue;
        }
        else { DumpsterRemogingItemSlider.value = 0f; }

        void CreateNewItem(Res res)
        {
            int qua = SpaceBaseMainSc.instance.GetResQuaOfStorage(res);
            GameObject newItem = Instantiate(ItemInDumpster, new Vector2(), Quaternion.identity);
            newItem.transform.SetParent(DumpsterItemGroup, false);
            newItem.name = res.ToString();
            newItem.transform.Find("Image").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            newItem.transform.Find("Text").GetComponent<Text>().text = qua.ToString();
            if (qua <= 0) { newItem.SetActive(false); }
            else { newItem.SetActive(true); }
            ItemsInDumpsterGO[(int)res] = newItem;
        }
    }
    public void ClickStoragePage(int page)
    {
        switch (page)
        {
            case 1:
                storageSelectPage = page;
                StorageItemPanel.SetActive(true); StorageItemButtonText.color = activeColor;
                DumpsterPanel.SetActive(false); StorageDumpsterButtonText.color = disactiveColor;
                break;
            case 2:
                storageSelectPage = page;
                StorageItemPanel.SetActive(false); StorageItemButtonText.color = disactiveColor;
                DumpsterPanel.SetActive(true); StorageDumpsterButtonText.color = activeColor;
                break;
        }
        UpdateStorageUI();
    }
    public void ClickStorageItem()
    {
        switch (storageSelectPage)
        {
            case 1:
                if (remveItemFromStorage)
                { OpenStorageRemoveItemPanel(); }
                else
                { ClickGlossaryItemButton(); }
                break;
            case 2:
                OpenStorageRemoveItemPanel();
                break;
        }
    }
    //storage removing item panel
    private void OpenStorageRemoveItemPanel()
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

        StorageRemoveItemPanel.SetActive(true);
        storageSelectRes = checkedRes;

        Transform MainPanel = StorageRemoveItemPanel.transform.Find("Panel");

        Text ResText = MainPanel.Find("ResText").GetComponent<Text>();
        ResText.text = Language.NameOfRes(checkedRes);
        Image ResImg = MainPanel.Find("ResImage").GetComponent<Image>();
        ResImg.sprite = ImageLibrary.instance.GetResImage(checkedRes);

        Text MainInfoText = MainPanel.Find("MainInfoText").GetComponent<Text>();
        Text InfoText = MainPanel.Find("InfoText").GetComponent<Text>();

        if (storageSelectPage == 1)
        {
            MainInfoText.text = "Remove items";
            InfoText.text = "Items will be move to dumpster.";
        }
        else
        {
            MainInfoText.text = "Restore items";
            InfoText.text = "Items will be move to storage.";
        }

        UpdateStorageRemoveItemPanel(0);
    }
    public void UpdateStorageRemoveItemPanel(float value)
    {
        int num = (int)value;
        if (num < 0) { num = 0; }
        storageSelectValue = num;

        Transform MainPanel = StorageRemoveItemPanel.transform.Find("Panel");

        Text FreeSpaceText = MainPanel.Find("FreeSpaceText").GetComponent<Text>();
        Text NumberText = MainPanel.Find("NumberText").GetComponent<Text>();
        Slider Slider = MainPanel.Find("Slider").GetComponent<Slider>();
        int items;
        int freeSpace;
        int maxFreeSpace;
        if (storageSelectPage == 1)
        {
            items = SpaceBaseMainSc.instance.GetResQuaOfStorage(storageSelectRes);
            maxFreeSpace = SpaceBaseMainSc.instance.GetFreeSpaceOfDumpster();
            freeSpace = maxFreeSpace - num;
            if (freeSpace < 0) { freeSpace = 0; }
            FreeSpaceText.text = "Space in dumpster: "+ freeSpace;
        }
        else
        {
            items = SpaceBaseMainSc.instance.GetResQuaOfDumpster(storageSelectRes);
            maxFreeSpace = SpaceBaseMainSc.instance.GetFreeSpaceOfStorage();
            freeSpace = maxFreeSpace - num;
            if (freeSpace < 0) { freeSpace = 0; }
            FreeSpaceText.text = "Space in storage: " + freeSpace;
        }
        
        if (items > maxFreeSpace) { Slider.maxValue = maxFreeSpace; }
        else { Slider.maxValue = items; }
        Slider.value = num;
        NumberText.text = num.ToString();
    }
    public void ClickStorageRemovingItemPanelConfirm()
    {
        StorageRemoveItemPanel.SetActive(false);
        if (storageSelectValue == 0) { return; }
        
        if (storageSelectPage == 1)
        {
            bool ok = SpaceBaseMainSc.instance.MoveItemToDumpster(storageSelectRes, storageSelectValue);
            if (ok == false) { Debug.Log("Error! cant move items to dumpster from storage"); }
        }
        else
        {
            bool ok = SpaceBaseMainSc.instance.MoveItemToStorage(storageSelectRes, storageSelectValue);
            if (ok == false) { Debug.Log("Error! cant move items to storage form dumpster"); }
        }
    }
    public void ShowStorageButtonBacklight() => ShowButtonBacklight(StorageButton.transform);

    //Science
    private TechnologySc[] techs;
    private float lastTechViewMove = 0;
    private TechnologySc usingTechSc;
    public void ClickScienceButton()
    {
        if (nowOpenPanel == SciencePanel) { CloseNowOpenPanel(); return; }
        OpenSciencePanel();
    }
    public void OpenSciencePanel()
    {
        if (nowOpenPanel == SciencePanel) { return; }
        CloseNowOpenPanel();
        OpenPanel(SciencePanel);
        //MessageManager.instance.SetVisibilityMess(false);
        MessageManager.instance.SetMaxNumOfMess(8);
        OffButtonLed(ScienceButton.transform);
        SciencePanel.gameObject.SetActive(true);
        nowActiveButton = ScienceButton.GetComponent<Image>();
        nowActiveButton.color = activeColor;
        nowActiveButton.GetComponentInChildren<Text>().color = activeColor;

        TechInfoPanel.SetActive(false);
    }
    private void SetSciencePanel()
    {
        TechDoneImg.SetActive(true);
        TechCurtain.SetActive(true);

        techs = new TechnologySc[Enum.GetNames(typeof(Technologies)).Length];
        foreach (Transform childTrans in TechGroup)
        {
            TechnologySc techSc = childTrans.GetComponent<TechnologySc>();
            if (techSc == null) { continue; }

            int index = (int)techSc.thisTechnology;
            if (techs[index] != null) { Debug.Log(techSc.thisTechnology + " technology have multiple declaration"); continue; }

            techs[index] = techSc;
            if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(techSc.thisTechnology) == false)
            {
                techSc.discovered = false;
                AddCurtain(childTrans);
            }
            else
            {
                techSc.discovered = true;
                AddDoneImg(childTrans);
            }
        }

        ActLocks();

        TechGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(-22.5f, 300f);

        TechCurtain.SetActive(false);

        void AddCurtain(Transform parent)
        {
            Transform curtTrans = parent.Find("Curtain");
            if (curtTrans != null) { curtTrans.gameObject.SetActive(true); return; }

            GameObject newCurtain = Instantiate(TechCurtain);
            newCurtain.transform.SetParent(parent, false);
            newCurtain.GetComponent<RectTransform>().localPosition = new Vector2();
            newCurtain.name = "Curtain";
        }
        void AddDoneImg(Transform parent)
        {
            Transform curtTrans = parent.Find("TechDone");
            if (curtTrans != null) { curtTrans.gameObject.SetActive(true); return; }

            GameObject newCurtain = Instantiate(TechDoneImg);
            newCurtain.transform.SetParent(parent, false);
            newCurtain.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1f, 1f);
            newCurtain.name = "TechDone";
        }
    }
    public void ResetSciencePanel()
    {
        int count = TechGroup.childCount;
        for (int i = 1; i < count; i++)
        {
            Transform childTrans = TechGroup.GetChild(i);

            Transform curtTrans = childTrans.Find("Curtain");
            if (curtTrans != null) { Destroy(curtTrans.gameObject); }

            Transform doneTrans = childTrans.Find("TechDone");
            if (doneTrans != null) { Destroy(doneTrans.gameObject); }
        }

        Invoke("SetSciencePanel", 1f);
    }
    public void UnlockTech(TechnologySc techSc, bool actLocks=true)
    {
        if (techSc != null)
        {
            Transform CurtainTrans = techSc.transform.Find("Curtain");
            if (CurtainTrans != null) { Destroy(CurtainTrans.gameObject); }

            Transform TechDone = techSc.transform.Find("TechDone");
            if (TechDone != null) { TechDone.gameObject.SetActive(true); }
            else
            {
                GameObject newCurtain = Instantiate(TechDoneImg);
                newCurtain.transform.SetParent(techSc.transform, false);
                newCurtain.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1f, 1f);
                newCurtain.name = "TechDone";
                newCurtain.SetActive(true);
            }
        }
        if (actLocks) { ActLocks(); }
    }
    private void ActLocks()
    {
        foreach (TechnologySc techSc in techs)
        {
            if (techSc == null || techSc.discovered) { continue; }
            SetLock(techSc);
        }

        void SetLock(TechnologySc techSc)
        {
            if (techSc.needItems.Count == 0) { return; }

            foreach (Technologies nt in techSc.needTechnology)
            {
                if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(nt) == false) { return; }
            }
            Transform CurtainTrans = techSc.transform.Find("Curtain");
            if (CurtainTrans == null) { return; }
            CurtainTrans.GetComponent<Image>().sprite = TechCurainImage;
        }
    }
    public void OpenTechInfoPanel(TechnologySc techSc)
    {
        usingTechSc = techSc;

        TechInfoPanel.SetActive(true);
        TechInfoPanel.transform.Find("NameText").GetComponent<Text>().text = Language.NameOfTech(techSc.thisTechnology);
        TechInfoPanel.transform.Find("ComplateText").gameObject.SetActive(techSc.discovered);
        TechInfoPanel.transform.Find("NeedItemsText").gameObject.SetActive(!techSc.discovered);
        NeedTechText.SetActive(!techSc.discovered);
        NeedItemsGroup.SetActive(!techSc.discovered);
        NeedTechGroup.SetActive(!techSc.discovered);
        RButtonGO.SetActive(!techSc.discovered);

        Vector2 pos = new Vector2(10f, -55f);

        if (techSc.discovered == false)
        {
            //need items
            foreach (Transform child in NeedItemsGroup.transform) { child.gameObject.SetActive(false); }
            for (int i = 0; i < techSc.needItems.Count; i++)
            {
                Transform child;
                if (i >= NeedItemsGroup.transform.childCount)
                {
                    child = Instantiate(TechInfoNeedItemsPanel).transform;
                    child.name = "Item";
                    child.SetParent(NeedItemsGroup.transform, false);
                }
                else { child = NeedItemsGroup.transform.GetChild(i); }
                child.gameObject.SetActive(true);
                Res needItemRes = techSc.needItems[i].res;
                int needItemQua = techSc.needItems[i].qua;
                int haveItemQua = SpaceBaseMainSc.instance.GetResQuaOfStorage(needItemRes);
                Text text = child.GetComponentInChildren<Text>();
                text.text = Language.NameOfRes(needItemRes) + " " + haveItemQua + "/" + needItemQua;
                if (needItemQua <= haveItemQua) { text.color = TechTextDefColor; }
                else { text.color = TechTextMisColor; }
                child.GetComponentInChildren<Image>().sprite = ImageLibrary.instance.GetResImage(needItemRes);
            }
            pos.y -= 30f * techSc.needItems.Count;

            //need technologies
            pos.y -= 5f;
            NeedTechText.GetComponent<RectTransform>().anchoredPosition = pos;
            pos.y -= 20f;
            NeedTechGroup.GetComponent<RectTransform>().anchoredPosition = pos;
            foreach (Transform child in NeedTechGroup.transform) { child.gameObject.SetActive(false); }
            for (int i = 0; i < techSc.needTechnology.Count; i++)
            {
                Transform child;
                if(i >= NeedTechGroup.transform.childCount)
                {
                    child = Instantiate(TechInfoNeedTechText).transform;
                    child.name = "Tech";
                    child.SetParent(NeedTechGroup.transform, false);
                }
                else { child = NeedTechGroup.transform.GetChild(i); }
                child.gameObject.SetActive(true);
                Text text = child.GetComponent<Text>();
                text.text = Language.NameOfTech(techSc.needTechnology[i]);
                if (SpaceBaseMainSc.instance.IsTechnologyDiscovered(techSc.needTechnology[i])) { text.color = TechTextDefColor; }
                else { text.color = TechTextMisColor; }
            }
            pos.y -= 20f * techSc.needTechnology.Count;

            //button
            pos.y -= 40f;
            RButtonGO.GetComponent<RectTransform>().anchoredPosition = pos;
            Image img = RButtonGO.transform.Find("Image").GetComponent<Image>();
            Button RButtonSc = RButtonGO.GetComponent<Button>();
            if (SpaceBaseMainSc.instance.CanDiscoverTechnology(techSc)) { img.color = activeColor; img.fillCenter = false;  RButtonSc.interactable = true; }
            else { img.color = TechDisactivColor; img.fillCenter = true; RButtonSc.interactable = false; }
        }

        //description
        pos.y -= 30f;
        string desName = "TechDes." + techSc.thisTechnology;
        string description = Language.GetText(desName);
        if (description == "" || description == "null") { description = "Unlocks " + Language.NameOfTech(techSc.thisTechnology); }
        Text desText = DescryptionRT.gameObject.GetComponent<Text>();
        desText.text = description;
        SetTextContainerSize(desText);
        DescryptionRT.anchoredPosition = pos;

        //panel size
        float panelHeight = -pos.y + DescryptionRT.sizeDelta.y + 10f; 
        infoPanleRT.sizeDelta = new Vector2(300f, panelHeight);

        //panel position
        Vector2 panelSize = infoPanleRT.sizeDelta;
        panelSize.x *= 0.5f * transform.lossyScale.x;
        panelSize.y *= 0.5f * transform.lossyScale.y;
        Vector2 borderSize = new Vector2(Screen.width, Screen.height - 40f * transform.lossyScale.y);
        Vector2 Pos = techSc.gameObject.GetComponent<RectTransform>().position;
        if (Pos.y > borderSize.y - panelSize.y) { Pos.y = borderSize.y - panelSize.y; }
        else if (Pos.y < panelSize.y) { Pos.y = panelSize.y; }
        if (Pos.x > borderSize.x / 2) { Pos.x -= panelSize.x; }
        else { Pos.x += panelSize.x; }
        TechInfoPanel.transform.position = Pos;
    }
    public void MoveTechView(Vector2 move)
    {
        float delta = move.x - lastTechViewMove;
        lastTechViewMove = move.x;
        if (delta < 0) { delta = -delta; }
        if (delta < 0.001f) { return; }
        TechInfoPanel.SetActive(false);
    }
    public void ClickResarchButton()
    {
        SpaceBaseMainSc.instance.DiscoverTechnology(usingTechSc);
        TechInfoPanel.SetActive(false);
    }
    public void ShowScienceButtonBacklight() => ShowButtonBacklight(ScienceButton.transform);

    //Task
    public void OpenTaskPanel()
    {
        if (nowOpenPanel == TaskPanel) { return; }

        CloseNowOpenPanel();
        OpenPanel(TaskPanel);
        TaskPanel.gameObject.SetActive(true);
        //MessageManager.instance.SetVisibilityMess(false);
        MessageManager.instance.SetMaxNumOfMess(6);
    }

    //Glossary
    private int glossarySelectPage = 1;
    private struct ItemRecipeStruct
    {
        public CraftRecipe craftRecipe;
        public Obj obj;

        public ItemRecipeStruct(CraftRecipe craftRecipe, Obj obj)
        {
            this.craftRecipe = craftRecipe;
            this.obj = obj;
        }
    }
    private List<ItemRecipeStruct> itemsRecipeList;
    private int showingRecipeNumber = 0;
    public void ClickGlossaryButton()
    {
        if (nowOpenPanel == GlossaryPanel) { CloseNowOpenPanel(); return; }
        OpenGlossaryPanel();
    }
    public void OpenGlossaryPanel()
    {
        if (nowOpenPanel == GlossaryPanel) { return; }
        CloseNowOpenPanel();
        OpenPanel(GlossaryPanel);
        //MessageManager.instance.SetVisibilityMess(false);
        MessageManager.instance.SetMaxNumOfMess(3);
        OffButtonLed(GlossaryButton.transform);
        GlossaryPanel.gameObject.SetActive(true);
        nowActiveButton = GlossaryButton.GetComponent<Image>();
        nowActiveButton.color = activeColor;
        nowActiveButton.GetComponentInChildren<Text>().color = activeColor;
    }
    private void SetGlossaryPanel()
    {
        ClickGlossaryPage(1);

        ItemInGlossaryRecipe.SetActive(false);
        ItemInGlossary.SetActive(false);
        int NumberOfRes = SpaceBaseMainSc.instance.numberOfRes;
        for (int i = 1; i < NumberOfRes; i++)
        {
            Res res = (Res)i;
            GameObject newItem = Instantiate(ItemInGlossary, new Vector2(), Quaternion.identity);
            newItem.transform.SetParent(GlossaryItemGroup, false);
            newItem.name = res.ToString();
            newItem.transform.Find("Image").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            newItem.SetActive(true);
        }

        BuildingInGlossary.SetActive(false);
        foreach (Obj obj in AllRecipes.instance.ObjectThatCanBeBuilt)
        {
            GameObject newItem = Instantiate(BuildingInGlossary, new Vector2(), Quaternion.identity);
            newItem.transform.SetParent(GlossaryBuildingsGroup, false);
            newItem.name = obj.ToString();
            newItem.transform.Find("Image").GetComponent<Image>().sprite = ImageLibrary.instance.GetObjImages(obj);
            newItem.SetActive(true);
        }

        UpdateGlossaryItemCurtains();
    }
    public void UpdateGlossaryItemCurtains()
    {
        foreach (Transform child in GlossaryBuildingsGroup)
        {
            Transform curtain = child.Find("Curtain");
            if (curtain == null) { continue; }

            if (Enum.TryParse(child.name, out Obj obj) == false) { continue; }
            
            BuildingRecipe recipe = AllRecipes.instance.GetBuildRecipe(obj);

            if (recipe.active)
            {
                Destroy(curtain.gameObject);
            }
        }
        foreach (Transform child in GlossaryItemGroup)
        {
            Transform curtain = child.Find("Curtain");
            if (curtain == null) { continue; }

            if (Enum.TryParse(child.name, out Res res) == false) { continue; }
            
            if (AllRecipes.instance.IsResUnlock(res))
            {
                Destroy(curtain.gameObject);
            }
        }
    }
    public void ClickGlossaryPage(int page)
    {
        switch (page)
        {
            case 1: 
                glossarySelectPage = page;
                GlossaryItemsPanel.SetActive(true);         GlossaryItemsButtonText.color = activeColor;
                GlossaryBuildingsPanel.SetActive(false);    GlossaryBuildingsButtonText.color = disactiveColor;
                break;
            case 2:
                glossarySelectPage = page;
                GlossaryItemsPanel.SetActive(false);        GlossaryItemsButtonText.color = disactiveColor;
                GlossaryBuildingsPanel.SetActive(true);     GlossaryBuildingsButtonText.color = activeColor;
                break;
        }
    }
    public void ClickGlossaryItemButton()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;

        if (Enum.TryParse(name, out Res res) == false) { return; }
        if (res == Res.None) { return; }

        SetGlossarySelectedItem(res);
    }
    public void SetGlossarySelectedItem(Res res)
    {
        OpenGlossaryPanel();
        ClickGlossaryPage(1);

        //border
        Transform trans = GlossaryItemGroup.Find(res.ToString());
        if (trans == null) { Debug.LogError("Dont found " + res + " in recipe"); GlossaryItemBorder.SetActive(false); return; }
        else { GlossaryItemBorder.SetActive(true); GlossaryItemBorder.transform.SetParent(trans, false); GlossaryItemBorder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0); }

        //info
        GlossaryItemInfoPanel.Find("SlectResText").GetComponent<Text>().text = "Selected item: " + Language.NameOfRes(res);
        GlossaryItemInfoPanel.Find("ResImage").gameObject.SetActive(true);
        GlossaryItemInfoPanel.Find("ResImage").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);

        showingRecipeNumber = 0;
        itemsRecipeList = new List<ItemRecipeStruct>();

        CraftRecipe cRecipe;
        switch (res)
        {
            case Res.Wood: itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.Tree)); itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.Woodcuter)); break;
            case Res.Sapling: itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.Tree)); itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.Woodcuter)); break;
            case Res.StoneOre: itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.StoneOre)); itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.Quarry)); break;
            case Res.CopperOreCtm: itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.CopperOre)); itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.Quarry)); break;
            case Res.Sand: itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.SandOre)); itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.Quarry)); break;
            case Res.IronOre: itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.IronOre)); itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.Quarry)); break;
            case Res.Coal: itemsRecipeList.Add(new ItemRecipeStruct(null, Obj.CoalOre)); break;
            case Res.BottleOil: cRecipe = new CraftRecipe(new List<ItemRAQ> { new ItemRAQ(Res.BottleEmpty, 1) }, new List<ItemRAQ> { new ItemRAQ(Res.BottleOil, 1) }, 3f, Technologies.Pump); cRecipe.active = true; itemsRecipeList.Add(new ItemRecipeStruct(cRecipe, Obj.Pump)); break;
            case Res.BottleWater: cRecipe = new CraftRecipe(new List<ItemRAQ> { new ItemRAQ(Res.BottleEmpty, 1) }, new List<ItemRAQ> { new ItemRAQ(Res.BottleWater, 1) }, 3f, Technologies.Pump); cRecipe.active = true; itemsRecipeList.Add(new ItemRecipeStruct(cRecipe, Obj.Pump)); break;
        }

        foreach (Obj objWCR in AllRecipes.instance.objectWithCraftRecipes)
        {
            foreach (CraftRecipe recipe in AllRecipes.instance.GetCraftRecipes(objWCR))
            {
                foreach (ItemRAQ iir in recipe.ItemOut)
                {
                    if (iir.res == res)
                    {
                        itemsRecipeList.Add(new ItemRecipeStruct(recipe, objWCR));
                    }
                }
            }
        }

        UpdateGlossaryResInfoPanle(0);
    }
    private void UpdateGlossaryResInfoPanle(int n)
    {
        if(n < 0 || n >= itemsRecipeList.Count) { return; }
        ItemRecipeStruct useItemRecipeStruct = itemsRecipeList[n];

        string prodText;
        switch (useItemRecipeStruct.obj)
        {
            case Obj.IronOre: case Obj.CopperOre: case Obj.SandOre: case Obj.CoalOre: case Obj.StoneOre:
                prodText = "You can obtain it by digging " + Language.NameOfObj(useItemRecipeStruct.obj); break;
            case Obj.Tree:
                prodText = "Can be obtained from cutted " + Language.NameOfObj(Obj.Tree); break;
            default:
                prodText = "Can be produced in " + Language.NameOfObj(useItemRecipeStruct.obj); break;
        }

        GlossaryItemInfoPanel.Find("ProdText").GetComponent<Text>().text = prodText;
        GlossaryItemInfoPanel.Find("PageText").GetComponent<Text>().text = string.Format("Page {0}/{1}", n+1, itemsRecipeList.Count);

        Transform Recipe = GlossaryItemInfoPanel.Find("Recipe");
        Transform ItemsIn = Recipe.Find("ItemsIn");
        Transform ItemsOut = Recipe.Find("ItemsOut");
        Transform Arrow = Recipe.Find("ArrowImage");

        foreach (Transform child in ItemsIn) { child.gameObject.SetActive(false); }
        foreach (Transform child in ItemsOut) { child.gameObject.SetActive(false); }

        if (useItemRecipeStruct.craftRecipe != null)
        {
            if (useItemRecipeStruct.craftRecipe.ItemIn != null) { 
                foreach (ItemRAQ item in useItemRecipeStruct.craftRecipe.ItemIn)
                {
                    Transform itemT = ItemsIn.Find(item.res.ToString());
                    if (itemT == null) { itemT = CreateItem(item.res); itemT.SetParent(ItemsIn, false); }
                    itemT.Find("Text").GetComponent<Text>().text = item.qua.ToString();
                    itemT.gameObject.SetActive(true);
                } 
            }
            if (useItemRecipeStruct.craftRecipe.ItemOut != null) {
                foreach (ItemRAQ item in useItemRecipeStruct.craftRecipe.ItemOut)
                {
                    Transform itemT = ItemsOut.Find(item.res.ToString());
                    if (itemT == null) { itemT = CreateItem(item.res); itemT.SetParent(ItemsOut, false); }
                    itemT.Find("Text").GetComponent<Text>().text = item.qua.ToString();
                    itemT.gameObject.SetActive(true);
                }   
            }
            //set arrow
            float mfp = 0;
            if (useItemRecipeStruct.craftRecipe.ItemIn.Count > 3) { mfp = (useItemRecipeStruct.craftRecipe.ItemIn.Count - 3) * 48f - 20f; }
            Arrow.GetComponent<RectTransform>().anchoredPosition = new Vector2(mfp, 4.85f);
            ItemsOut.GetComponent<RectTransform>().anchoredPosition = new Vector2(mfp + 160f, 0);
            Arrow.gameObject.SetActive(true);
            Arrow.GetComponentInChildren<Text>().text = useItemRecipeStruct.craftRecipe.exeTime.ToString() + "s";
        }
        else { Arrow.gameObject.SetActive(false); }

        Transform curtain = GlossaryItemInfoPanel.Find("Curtain");
        if (curtain != null) 
        {
            if (useItemRecipeStruct.craftRecipe != null)
            {
                if (useItemRecipeStruct.craftRecipe.active == false) { curtain.gameObject.SetActive(true); }
                else
                {
                    foreach (ItemRAQ item in useItemRecipeStruct.craftRecipe.ItemIn)
                    { if (AllRecipes.instance.IsResUnlock(item.res) == false) { curtain.gameObject.SetActive(true); return; } }
                    foreach (ItemRAQ item in useItemRecipeStruct.craftRecipe.ItemOut)
                    { if (AllRecipes.instance.IsResUnlock(item.res) == false) { curtain.gameObject.SetActive(true); return; } }
                    curtain.gameObject.SetActive(false);
                }
            }
            else { curtain.gameObject.SetActive(false); }
        }

        Transform CreateItem(Res res)
        {
            GameObject newItem = Instantiate(ItemInGlossaryRecipe, new Vector2(), Quaternion.identity);
            newItem.name = res.ToString();
            newItem.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            newItem.SetActive(true);
            return newItem.transform;
        }
    }
    public void ClickGlossaryNextPage()
    {
        if (itemsRecipeList == null) { return; }
        showingRecipeNumber++;
        if(showingRecipeNumber >= itemsRecipeList.Count) { showingRecipeNumber = 0; }
        UpdateGlossaryResInfoPanle(showingRecipeNumber);
    }
    public void ClickGlossaryBackPage()
    {
        if (itemsRecipeList == null) { return; }
        showingRecipeNumber--;
        if (showingRecipeNumber < 0) { showingRecipeNumber = itemsRecipeList.Count - 1; }
        UpdateGlossaryResInfoPanle(showingRecipeNumber);
    }
    //buildings
    public void ClickGlossaryBuildingButton()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        if (Enum.TryParse(name, out Obj obj) == false) { return; }
        if (obj == Obj.None) { return; }

        SetGlossarySelectedBuilding(obj);
    }
    public void SetGlossarySelectedBuilding(Obj obj)
    {
        OpenGlossaryPanel();
        ClickGlossaryPage(2);

        //border
        Transform trans = GlossaryBuildingsGroup.Find(obj.ToString());
        if (trans == null) { GlossaryItemBorder.SetActive(false); return; }
        else { GlossaryItemBorder.SetActive(true); GlossaryItemBorder.transform.SetParent(trans, false); GlossaryItemBorder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0); }

        GlossaryBuildingsPanel.GetComponentInChildren<Text>().text = Language.NameOfObj(obj);
        Transform img = GlossaryBuildingsPanel.transform.Find("BuildingImg");
        img.gameObject.SetActive(true);
        img.GetComponent<Image>().sprite = ImageLibrary.instance.GetObjImages(obj);

        BuildingRecipe BR = AllRecipes.instance.GetBuildRecipe(obj);

        string descKey = "ObjDes." + obj;
        string description = Language.GetText(descKey);
        if (BR.products != null && BR.products.Count > 0)
        {
            if (AllRecipes.instance.IsItTurret(obj)) { description += "\n\nTypes of ammunition:"; }
            else { description += "\n\nProducts:"; }
        }
        GlossaryBuildingDescriptionText.text = description;

        SetTextContainerSize(GlossaryBuildingDescriptionText);
        if (BR.products.Count == 0) { GlossaryProductsPanel.gameObject.SetActive(false); }
        else
        {
            GlossaryProductsPanel.gameObject.SetActive(true);
            foreach (Transform child in GlossaryProductsPanel) { child.gameObject.SetActive(false); }
            foreach (Res item in BR.products)
            {
                Transform itemsTrans = GlossaryProductsPanel.Find(item.ToString());
                if (itemsTrans == null) { SpownRes(item); }
                else { itemsTrans.gameObject.SetActive(true); }
            }

            int n = (BR.products.Count / 8) + 1;
            float heightp = n * 42f + 10 + (n - 1) * 5f;
            RectTransform RT = GlossaryProductsPanel.GetComponent<RectTransform>();
            RT.sizeDelta = new Vector2(RT.rect.width, heightp);
        }
        GlossaryStatsText.text = BR.stats;

        void SpownRes(Res res)
        {
            GameObject newItem = Instantiate(ItemInGlossaryRecipe);
            newItem.name = res.ToString();
            newItem.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            Destroy(newItem.transform.Find("Text").gameObject);
            newItem.transform.SetParent(GlossaryProductsPanel, false);
            newItem.SetActive(true);
        }
    }
    public void ShowGlossaryBacklight() => ShowButtonBacklight(GlossaryButton.transform);

    //Settings
    public void ClickSettingsButton()
    {
        if (nowOpenPanel == SettingsPanel) { CloseNowOpenPanel(); return; }
        OpenSettingsPanel();
    }
    public void OpenSettingsPanel()
    {
        if(nowOpenPanel == SettingsPanel) { return; }
        CloseNowOpenPanel();
        OpenPanel(SettingsPanel);
        //MessageManager.instance.SetVisibilityMess(false);
        MessageManager.instance.SetMaxNumOfMess(4);
        SettingsPanel.gameObject.SetActive(true);
        nowActiveButton = SettingsButon.GetComponent<Image>();
        nowActiveButton.color = activeColor;
        nowActiveButton.GetComponentInChildren<Text>().color = activeColor;
    }
    public void UpdateSettingsPanel()
    {
        SettingsPanel.gameObject.SetActive(true);
        resImageInBuildingToggl.isOn = SettingsManager.instance.GetResImageInBuilding();
        fullscreenToggl.isOn = SettingsManager.instance.GetFullscreen();
        autoSaveFreqDropdown.value = SettingsManager.instance.GetAutoSaveValue();
        usingLanguageDropdown.value = SettingsManager.instance.GetUsingLanguageAsInt();
        MusicVolumeSlider.value = SettingsManager.instance.GetMusicVolume();
        SoundsVolumeSlider.value = SettingsManager.instance.GetSoundscVolume();
    }

    //confirm panel
    private void OpenConfirmPanel()
    {
        ConfirmPanel.SetActive(true);
        Transform trans = ConfirmPanel.transform.Find("Panel");
        trans.localScale = Vector2.zero;
        trans.DOScale(Vector2.one, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);
        trans.GetComponent<CanvasGroup>().DOFade(1f, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);

        ConfirmPanelCancelButton.SetActive(false);
        ConfirmPanelExitButton.SetActive(false);
        ConfirmPanelBackToMenuButton.SetActive(false);
        ConfirmPanelLeaveColonyButton.SetActive(false);
        ConfirmPanelResetProgresButton.SetActive(false);
    }
    public void CloseConfirmPanel()
    {
        Transform trans = ConfirmPanel.transform.Find("Panel");
        trans.DOScale(Vector2.zero, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);
        trans.GetComponent<CanvasGroup>().DOFade(0f, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true).OnComplete(() => ConfirmPanel.SetActive(false));
    }
    public void OpenConfirmPanelReadSaveError(string errorLog)
    {
        OpenConfirmPanel();
        ConfirmPanelText.text = errorLog;

        ConfirmPanelCancelButton.SetActive(true);
        ConfirmPanelBackToMenuButton.SetActive(true);
    }
    public void OpenConfirmPanelExit()
    {
        OpenConfirmPanel();
        ConfirmPanelText.text = "Are you sure you want to exit?";

        ConfirmPanelCancelButton.SetActive(true);
        ConfirmPanelExitButton.SetActive(true);
    }
    public void OpenConfirmPanelLeaveColony()
    {
        OpenConfirmPanel();
        ConfirmPanelText.text = "Are you sure you want to leave this colony?\n\nThe colony area will be renovated and all buildings will be destroyed.";

        ConfirmPanelCancelButton.SetActive(true);
        ConfirmPanelLeaveColonyButton.SetActive(true);
    }
    public void OpenConfirmPanelResetProgres()
    {
        OpenConfirmPanel();
        ConfirmPanelText.text = "Are you sure you want to delete all game progress?\n\nAll progress including research, colonies and resources will be lost.\n\nRestart the game to correctly apply these changes.";

        ConfirmPanelCancelButton.SetActive(true);
        ConfirmPanelResetProgresButton.SetActive(true);
    }
    public void ClickCPBackToMenu()
    {
        CloseConfirmPanel();
        SpaceBaseMainSc.instance.BackToMenu(false);
    }
    public void ClickCPLeaveColony()
    {
        CloseConfirmPanel();
        CloseNowOpenPanel();
        int colonyNumber = SpaceBaseMainSc.instance.selectedColony;
        if (colonyNumber == -1) { SPLeaveColonyButton.SetActive(false); Debug.Log("ERROR! Cant delete colony because selected colony is equal null"); return; }

        ColonyNames colonyName = (ColonyNames)colonyNumber;
        SpaceBaseMainSc.instance.LeaveColony(colonyName);
    }
    public void ClickCPResetGameProgrs()
    {
        CloseConfirmPanel();
        CloseNowOpenPanel();
        SpaceBaseMainSc.instance.ResetAllGameProgrs();
    }
}