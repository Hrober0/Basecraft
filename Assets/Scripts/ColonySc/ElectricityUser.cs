using UnityEngine;

public class ElectricityUser : MonoBehaviour
{
    public float actEnergyPerSec = 0f;
    public float maxEnergyPerSec = 0f;
    public int x;
    public int y;
    public float actCharge = 0f;
    public float maxCharge = 0f;

    private void Awake()
    {
        x = (int)transform.position.x / 10;
        y = (int)transform.position.y / 10;
    }
}
