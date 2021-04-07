using System.Collections.Generic;
using UnityEngine;

public class TechnologySc : MonoBehaviour
{
    public Technologies thisTechnology = Technologies.Dif;
    public List<Technologies> needTechnology = new List<Technologies>();
    public List<ItemRAQ> needItems = new List<ItemRAQ>();
    public bool discovered = false;

    public void ClickTechnology()
    {
        LeftPanel.instance.OpenTechInfoPanel(this);
    }
}
