using UnityEngine;

public class Battery : MonoBehaviour
{
    public readonly float Capacity = 500f;
    public float charge = 0f;
    public int x;
    public int y;

    private PlatformBehavior PlatformB;

    private void Awake()
    {
        x = (int)transform.position.x / 10;
        y = (int)transform.position.y / 10;

        PlatformB = gameObject.GetComponent<PlatformBehavior>();
        PlatformB.usingGuiType = PlatfotmGUIType.None;
        PlatformB.itemSendingType = PlatformItemSendingType.None;
        PlatformB.SetAllCanInItem(false);
        PlatformB.canBeConnectedOut = false;
        PlatformB.SetMaxEmptySpace(100);

        ElectricityManager.instance.AddBattery(this);
    }
}
