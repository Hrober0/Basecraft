using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class SelectItemPanel : MonoBehaviour
{
    [SerializeField] private GameObject selectItemPanelGO = null;
    [SerializeField] private GameObject itemSelectButtonGO = null;
    [SerializeField] private Transform selectItemContentT = null;

    public enum ItemSelectMode { None, Additem, RequestItem }
    private ItemSelectMode storageItemSelectMode;

    public static SelectItemPanel instance;
    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    public void Show(ItemSelectMode itemSelectMode)
    {
        storageItemSelectMode = itemSelectMode;

        if (itemSelectMode == ItemSelectMode.None)
            Hide();
        else
        {
            SelectResQuaPanel.instance.Hide();

            RectTransform rt = selectItemPanelGO.GetComponent<RectTransform>();
            int index = GuiControler.instance.GetCountOfNowOpenPanels() - 2;
            float yPos = GuiControler.instance.GetUpPositionOfPanelIndexInNowOpenPanels(index);
            if (yPos < 0f) return;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, yPos);

            selectItemPanelGO.SetActive(true);
        }
            
    }
    public void Hide()
    {
        selectItemPanelGO.SetActive(false);
    }

    public void SetPlatformItemSelectPanel()
    {
        for (int i = 0; i < GuiControler.instance.resLenght; i++)
        {
            Res res = (Res)i;
            if (!SpaceBaseMainSc.instance.CreativeModeOn && !AllRecipes.instance.IsResUnlock(res)) { continue; }
            Transform buttonTrans = selectItemContentT.Find(res.ToString());
            if (buttonTrans == null) { CreateRes(res); }

        }

        void CreateRes(Res res)
        {
            GameObject newPanel = Instantiate(itemSelectButtonGO);
            newPanel.transform.SetParent(selectItemContentT, false);
            newPanel.name = res.ToString();
            newPanel.SetActive(true);
            newPanel.transform.Find("Image").GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
        }
    }
    public void ClickSelectAddItemToPlatform() => GuiControler.instance.ShowSetResQuaPanel(Res.None, SelectResQuaPanel.SelectResQuaMode.AddRes);
    public void ClickItemSelectButton()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        if (Enum.TryParse(name, out Res res) == false) { return; }

        switch (storageItemSelectMode)
        {
            case ItemSelectMode.Additem: SelectResQuaPanel.instance.SetPGSetResQua(res); break;
            case ItemSelectMode.RequestItem: GuiControler.instance.RequestPanelSc.SetRIItem(res); break;
        }

        GuiControler.instance.HideResInfo();
        Hide();
    }
}
