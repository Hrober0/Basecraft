﻿using UnityEngine;

public class SolarPanel : MonoBehaviour
{
    [SerializeField] private float productionPerSec = 5f;
    private float charge;
    private ElectricityUser eleUSc;
    void Start()
    {
        eleUSc = GetComponent<ElectricityUser>();
        eleUSc.maxEnergyPerSec = productionPerSec;
        charge = productionPerSec * 2;
        eleUSc.actCharge = charge;
        eleUSc.maxCharge = charge;
        ElectricityManager.instance.AddGenerator(eleUSc);
    }
    void Update()
    {
        eleUSc.actCharge = charge;
    }
}
