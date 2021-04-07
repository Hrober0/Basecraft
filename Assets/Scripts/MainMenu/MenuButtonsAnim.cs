using UnityEngine;
using UnityEngine.UI;

public class MenuButtonsAnim : MonoBehaviour
{
    public bool active = true;
    public Text text;
    public Color defColor;
    public Color hoverColor;

    private void Start()
    {
        if (active) { text.color = defColor; }
        else { text.color = new Color(defColor.r, defColor.g, defColor.b, 0.25f); }
    }
    public void PointerEnter()
    {
        if (active) { text.color = hoverColor; }
    }
    public void PointerExit()
    {
        if (active) { text.color = defColor; }
    }
}
