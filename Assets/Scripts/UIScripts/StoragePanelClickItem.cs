using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class StoragePanelClickItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool pointerDown = false;
    private float pointerDownTime = 0f;
    private float holdingTime = 0.4f;
    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Set();
    }

    private void Update()
    {
        if (pointerDown)
        {
            pointerDownTime += Time.unscaledDeltaTime;
            if (pointerDownTime > holdingTime) Set();
        }
    }

    private void Click()
    {
        if (Enum.TryParse(name, out Res checkedRes) == false) return; 
        if(GuiControler.instance!=null)  GuiControler.instance.ChangeFilter(checkedRes);
    }
    private void Hold()
    {
        if (Enum.TryParse(name, out Res checkedRes) == false) return;
        if (GuiControler.instance != null) GuiControler.instance.ShowSetResQuaPanel(checkedRes, SelectResQuaPanel.SelectResQuaMode.SetMaxQua);
        //if (SpaceBaseMainSc.instance.CreativeModeOn) { GuiControler.instance.ShowAddItemPanel(checkedRes); }
    }

    private void Set()
    {
        if (!pointerDown) return;

        if (pointerDownTime > holdingTime) Hold();
        else Click();

        pointerDown = false;
        pointerDownTime = 0f;
    }
}
