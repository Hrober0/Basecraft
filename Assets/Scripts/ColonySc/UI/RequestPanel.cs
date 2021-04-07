using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class RequestPanel : MonoBehaviour
{
    public Transform RequestItemPanelT = null;
    [SerializeField] private Toggle KeepToggle = null;
    [SerializeField] private Transform Group = null;
    [SerializeField] private GameObject ItemButton = null;
    [SerializeField] private GameObject AddButton = null;

    private readonly float updateGuiDelay = 0.3f;
    private float timeToUpdateGui = 0f;

    private void Update()
    {
        if (timeToUpdateGui <= 0f)
        {
            if (GuiControler.instance.IsNowOpenPanelsContains(RequestItemPanelT)) { UpdateRequestPanel(); }
            timeToUpdateGui = updateGuiDelay;
        }
        timeToUpdateGui -= Time.unscaledDeltaTime;
    }

    public void Show()
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        RequestItemPanelT.gameObject.SetActive(false);
        KeepToggle.isOn = usePBSc.keepAmountOfRequestedItems;
        RequestItemPanelT.gameObject.SetActive(true); ;
        UpdateRequestPanel();
    }
    private void UpdateRequestPanel()
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        int maxItem = 6;
        int ic = usePBSc.requestItems.Count;
        for (int i = 0; i < maxItem; i++)
        {
            Transform trans = GetItem(i);
            if (i >= ic) { trans.gameObject.SetActive(false); continue; }
            trans.gameObject.SetActive(true);
            ItemRAQ item = usePBSc.requestItems[i];
            trans.name = item.res.ToString();
            trans.GetChild(0).GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(item.res);
            trans.GetComponentInChildren<Text>().text = item.qua.ToString();
        }
        AddButton.gameObject.SetActive(ic < maxItem ? true : false);

        Transform GetItem(int index)
        {
            if (index < Group.childCount - 1) { return Group.GetChild(index); }
            GameObject newItem = Instantiate(ItemButton);
            newItem.transform.SetParent(Group, false);
            AddButton.transform.SetAsLastSibling();
            return newItem.transform;
        }
    }
    public void ToggleRIKeep(bool value) => GuiControler.instance.usePBSc.keepAmountOfRequestedItems = value;
    public void SetRIItem(Res res)
    {
        if (res == Res.None)
        {
            SelectItemPanel.instance.Show(SelectItemPanel.ItemSelectMode.RequestItem);
            return;
        }
        SelectResQuaPanel.instance.Show(res, SelectResQuaPanel.SelectResQuaMode.RequestRes);
    }
    public void ClickRIItem()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        if (Enum.TryParse(name, out Res res) == false) { Debug.Log("Wrong RIItem name"); return; }
        SetRIItem(res);
        GuiControler.instance.HideResInfo();
    }
    public void ClickRIAddItem() => SetRIItem(Res.None);
}
