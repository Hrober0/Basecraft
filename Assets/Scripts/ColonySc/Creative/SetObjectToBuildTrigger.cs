using UnityEngine;

public class SetObjectToBuildTrigger : MonoBehaviour
{
    public void Click()
    {
        CreativeUIController.instance.ClickObjectButton(gameObject);
    }
}
