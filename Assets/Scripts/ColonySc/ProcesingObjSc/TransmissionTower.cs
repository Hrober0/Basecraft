using UnityEngine;

public class TransmissionTower : MonoBehaviour
{
    public int network = -1;

    private void Awake()
    {
        ElectricityManager.instance.AddTT(this);
    }
}
