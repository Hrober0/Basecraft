using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class StoragePanel : MonoBehaviour
{
    [Header("Storage panel")]
    public Transform StoragePanelT = null;
    [SerializeField] private Text FreeSpaceText = null;
    [SerializeField] private Toggle ShowNotStoredTogle = null;
    [SerializeField] private Toggle AllowDronesToTakeTogle = null;
    [SerializeField] private Slider FilterSlider = null;
    [SerializeField] private Transform ItemListPanel = null;
    [SerializeField] private GameObject ItemInStorage = null;
    [SerializeField] private GameObject AddItemsButton = null;

    [Header("Launch panel")]
    public Transform LaunchPanelT = null;

    [Header("Dron station panel")]
    public Transform DronStationPanelT = null;
    [SerializeField] private GameObject DronSpace = null;
    private GameObject[] DronImages;
    private readonly float updateGuiDelay = 0.3f;
    private float timeToUpdateGui = 0f;
    [SerializeField] private Color bacgroundFillColor = new Color();
    private Color disactiveItemPanelColor = new Color(0.15f, 0.15f, 0.15f);
    private Color disactiveItemImageColor = new Color(0.4f, 0.4f, 0.4f);

    private void Awake()
    {
        allItem = new GameObject[GuiControler.instance.resLenght];
    }

    void Update()
    {
        if (timeToUpdateGui <= 0f)
        {
            if (GuiControler.instance.IsNowOpenPanelsContains(StoragePanelT)) { UpdateItemList(); }
            if (GuiControler.instance.IsNowOpenPanelsContains(DronStationPanelT)) { UpdateDronSpaces(); }
            timeToUpdateGui = updateGuiDelay;
        }
        timeToUpdateGui -= Time.unscaledDeltaTime;
    }

    //Storage Gui
    private GameObject[] allItem;
    public void ShowStorageGui()
    {
        AddItemsButton.SetActive(SpaceBaseMainSc.instance.CreativeModeOn);

        StoragePanelT.gameObject.SetActive(false);
        AllowDronesToTakeTogle.isOn = GuiControler.instance.usePBSc.canDronesGetRes;
        StoragePanelT.gameObject.SetActive(true);

        UpdateItemList();
        UpdateFilters();
        UpdateFiltrSlider();
    }
    private void UpdateItemList()
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        PlatformBehavior.ROP item;
        for (int i = 1; i < GuiControler.instance.resLenght; i++)
        {
            item = usePBSc.itemOnPlatform[i];
            if ((ShowNotStoredTogle.isOn && AllRecipes.instance.IsResUnlock((Res)i)) || item.qua > 0)
            {
                if (allItem[i] == null) { CreateItem((Res)i); }
                if (item.qua < 0) { item.qua = 0; }
                string text;
                if (usePBSc.itemOnPlatform[i].maxQua != usePBSc.MaxEmptySpace) { text = string.Format("{0}/{1}", item.qua, item.maxQua); }
                else { text = string.Format("{0}", item.qua); }
                allItem[i].transform.Find("Text").GetComponent<Text>().text = text;
                allItem[i].SetActive(true);
            }
            else
            {
                if (allItem[i] != null) { allItem[i].SetActive(false); }
            }
        }
        int freePlaces = usePBSc.itemOnPlatform[0].qua;
        if (freePlaces < 0) { freePlaces = 0; }
        FreeSpaceText.text = string.Format("Free Space: {0}", freePlaces);

        if (AddItemsButton.activeSelf) { AddItemsButton.transform.SetSiblingIndex(ItemListPanel.childCount); }

        void CreateItem(Res res)
        {
            GameObject newItem = Instantiate(ItemInStorage, new Vector2(), Quaternion.identity);
            newItem.transform.SetParent(ItemListPanel, false);
            newItem.name = res.ToString();
            newItem.SetActive(false);
            newItem.transform.Find("Image").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            allItem[(int)res] = newItem;
        }
    }
    public void TogleShowNoStoredItems(bool vale) { UpdateItemList(); UpdateFilters(); }
    public void TogleAllowDronesToTake(bool vale) { GuiControler.instance.usePBSc.canDronesGetRes = vale; }
    public void SliderFilterOnPlatform(float value)
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        switch ((int)value)
        {
            case 0: for (int i = 1; i < GuiControler.instance.resLenght; i++) { usePBSc.itemOnPlatform[i].canIn = false; } UpdateFilters(); break;
            case 1: break;
            case 2: for (int i = 1; i < GuiControler.instance.resLenght; i++) { usePBSc.itemOnPlatform[i].canIn = true; } UpdateFilters(); break;
        }
    }
    private void UpdateFiltrSlider()
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        bool isAllowAll = true;
        bool isBlockAll = true;
        for (int i = 1; i < GuiControler.instance.resLenght; i++)
        {
            if (usePBSc.itemOnPlatform[i].canIn) { isBlockAll = false; } else { isAllowAll = false; }
            if (!isBlockAll && !isAllowAll) { break; }
        }
        if (isAllowAll) { FilterSlider.value = 2; }
        else if (isBlockAll) { FilterSlider.value = 0; }
        else { FilterSlider.value = 1; }
    }
    private void UpdateFilters()
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        for (int i = 1; i < GuiControler.instance.resLenght; i++)
        {
            if (allItem[i] == null) { continue; }

            if (usePBSc.itemOnPlatform[i].canIn)
            {
                allItem[i].GetComponent<Image>().color = bacgroundFillColor;
                allItem[i].transform.Find("Image").GetComponent<Image>().color = Color.white;
            }
            else
            {
                allItem[i].GetComponent<Image>().color = disactiveItemPanelColor;
                allItem[i].transform.Find("Image").GetComponent<Image>().color = disactiveItemImageColor;
            }
        }
    }
    public void ChangeFilter(Res res)
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        int index = (int)res;
        usePBSc.itemOnPlatform[index].canIn = !usePBSc.itemOnPlatform[(int)res].canIn;
        if (usePBSc.itemOnPlatform[index].canIn)
        {
            allItem[index].GetComponent<Image>().color = bacgroundFillColor;
            allItem[index].transform.Find("Image").GetComponent<Image>().color = Color.white;
        }
        else
        {
            allItem[index].GetComponent<Image>().color = disactiveItemPanelColor;
            allItem[index].transform.Find("Image").GetComponent<Image>().color = disactiveItemImageColor;
        }
        UpdateFiltrSlider();
    }

    //launch panel
    public void UpdateLauchPanel()
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        int freePlaces = usePBSc.itemOnPlatform[0].qua;
        int itemToSend = usePBSc.MaxEmptySpace - freePlaces;
        if (itemToSend < 0) { itemToSend = 0; }
        LaunchPanelT.Find("RightText").GetComponent<Text>().text = "Items to send: " + itemToSend;

        int space = SpaceBaseMainSc.instance.GetFreeSpaceOfStorage();
        if (space < 0) { space = 0; }
        LaunchPanelT.Find("LeftText").GetComponent<Text>().text = "Space in station: " + space;

        //Debug.Log("TODO: liczba transporyowców");
        LaunchPanelT.Find("DownText").GetComponent<Text>().text = "Available transporters: 1/1";
    }

    //drone station panel
    private readonly int numOfdronsInDS = 12;
    private DronStation useDronStationSc;
    public void ShowDronStationGui()
    {
        useDronStationSc = WorldMenager.instance.GetTransforOfObj(GuiControler.instance.useX, GuiControler.instance.useY).GetComponent<DronStation>();
        DronImages = new GameObject[numOfdronsInDS];
        Transform DronsPanel = DronStationPanelT.Find("DronsPanel");
        for (int i = 0; i < numOfdronsInDS; i++)
        {
            Transform DronSpace = DronsPanel.GetChild(i + 1);
            if (DronSpace == null) { break; }
            GameObject DronImage = DronSpace.Find("DronImage").gameObject;
            DronImage.SetActive(false);
            DronImages[i] = DronImage;
        }

        UpdateDronSpaces();
    }
    private void UpdateDronSpaces()
    {
        if (useDronStationSc == null) { return; }

        int avalibleDronsQua = useDronStationSc.availableDrons.Count;
        if (avalibleDronsQua > useDronStationSc.maxDronsQua) { avalibleDronsQua = useDronStationSc.maxDronsQua; }
        for (int i = 0; i < avalibleDronsQua; i++)
        {
            DronImages[i].GetComponent<Image>().sprite = useDronStationSc.availableDrons[i].GetDronImage();
            DronImages[i].SetActive(true);
        }
        for (int i = avalibleDronsQua; i < numOfdronsInDS; i++)
        {
            DronImages[i].SetActive(false);
        }
    }
    public void SetAllDronSpaces()
    {
        Transform DronsPanelT = DronStationPanelT.Find("DronsPanel");
        for (int i = 0; i < numOfdronsInDS; i++)
        {
            GameObject newDronsSpace = Instantiate(DronSpace, new Vector2(), Quaternion.identity);
            newDronsSpace.transform.SetParent(DronsPanelT, false);
            newDronsSpace.name = string.Format("DronSpace {0}", i);
            newDronsSpace.SetActive(true);
        }
    }
}
