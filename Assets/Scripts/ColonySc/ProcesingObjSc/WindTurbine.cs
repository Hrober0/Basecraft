using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTurbine : MonoBehaviour
{
    public static readonly int minimumDistanceToAnotherTurbine = 8;

    [SerializeField] private float productionPerSec = 1f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Transform rotorTrans = null;
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
        rotorTrans.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
