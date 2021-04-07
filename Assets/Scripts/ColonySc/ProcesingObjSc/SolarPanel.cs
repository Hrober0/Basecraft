using UnityEngine;

public class SolarPanel : MonoBehaviour
{
    public float productionPerSec = 5f;
    private float charge;
    private ElectricityUser eleUSc;
    void Start()
    {
        eleUSc = GetComponent<ElectricityUser>();
        eleUSc.actEnergyPerSec = productionPerSec;
        eleUSc.maxEnergyPerSec = productionPerSec;
        charge = eleUSc.maxEnergyPerSec * 2;
        eleUSc.actCharge = charge;
        eleUSc.maxCharge = charge;
        ElectricityManager.instance.AddGenerator(eleUSc);
    }
    void Update()
    {
        eleUSc.actCharge = charge;
        eleUSc.maxCharge = charge;
    }
}
