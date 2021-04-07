using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class BuildConPanel : MonoBehaviour
{
    public Transform BuildConPanelT;
    [SerializeField] private Transform ButtonGroup = null;
    [SerializeField] private GameObject BuildConButton = null;
    [SerializeField] private GameObject ItemInBuildButton = null;
    [SerializeField] private GameObject BuildObjButtonBorder = null;
    [SerializeField] private Text DoText = null;

    public Obj SelectedObj = Obj.None;

    void Awake()
    {
        BuildConButton.SetActive(false);
    }

    public void SetAllBuildButtons()
    {
        int numberOfActiveButtons = 0;
        foreach (ConnectionRecipe nowUseRecipe in AllRecipes.instance.connectionRecipes)
        {
            if (nowUseRecipe.active == false) { continue; }

            string name = nowUseRecipe.connection.ToString();

            numberOfActiveButtons++;

            Transform buttonTrans = ButtonGroup.Find(name);
            if (buttonTrans != null) { buttonTrans.gameObject.SetActive(true); continue; }

            GameObject newButton = Instantiate(BuildConButton);
            newButton.name = name;
            newButton.transform.SetParent(ButtonGroup, false);
            newButton.SetActive(true);
            newButton.transform.Find("NameText").GetComponent<Text>().text = GuiControler.instance.DisplayedNameOfObj(nowUseRecipe.connection);
            newButton.transform.Find("StatsText").GetComponent<Text>().text = string.Format("Transfer: {0}i/s Cost: x{1}/m", nowUseRecipe.itemPerSecond, nowUseRecipe.iTLMultiplayer);
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

        float height = 68f + 6 * 2f + (56f + 2f) * numberOfActiveButtons;
        RectTransform rt = BuildConPanelT.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
    }
    public void CancelBuilding() => SelectBuildButton(Obj.None);

    public void ClickBuildButon()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        if (Enum.TryParse(name, out Obj ObjName) == false) { return; }

        SelectBuildButton(ObjName);
    }

    private void SelectBuildButton(Obj ObjName)
    {
        if (ObjName == SelectedObj || ObjName == Obj.None)
        {
            BuildObjButtonBorder.SetActive(false);
            SelectedObj = Obj.None;
            UpdateDoText();
            return;
        }

        Transform buttonT = ButtonGroup.Find(ObjName.ToString());
        if (buttonT == null) { return; }

        SelectedObj = ObjName;
        UpdateDoText();

        BuildObjButtonBorder.transform.SetParent(buttonT, false);
        BuildObjButtonBorder.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        BuildObjButtonBorder.SetActive(true);
    }
    public void UpdateDoText()
    {
        if (ClickMenager.instance.selectedFirstBuildingToBuildCon)
        {
            if (SelectedObj == Obj.None) { DoText.text = "Choose second building, with already built connection"; }
            else { DoText.text = "Choose second building, connection will be sending items into this"; }
        }
        else
        {
            if (SelectedObj == Obj.None) { DoText.text = "Select connection to build type\nor select already built connection"; }
            else { DoText.text = "Choose first building, connection will be pulling items out of this"; }
        }
    }
}
