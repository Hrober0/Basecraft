using UnityEngine;
using UnityEngine.EventSystems;

public class ShowItemInfoPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GuiControler.instance != null) GuiControler.instance.ShowResInfo();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
       if(GuiControler.instance!=null) GuiControler.instance.HideResInfo();
    }
}
