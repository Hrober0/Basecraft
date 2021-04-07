using System.Collections.Generic;
using UnityEngine;

public class EleNetwork
{
    public List<Vector3Int> TransTowersP = new List<Vector3Int>(); //pos + TT index
    public List<ElectricityUser> Generators = new List<ElectricityUser>();
    public List<ElectricityUser> Requesters = new List<ElectricityUser>();
    public List<Battery> Batteries = new List<Battery>();

    public float production = 0f;
    public float maxProduction = 0f;
    public float charge = 0f;
    public float maxCharge = 0f;
    public float request = 0f;
    public float maxRequest = 0f;
}

public class ElectricityManager : MonoBehaviour
{
    public static ElectricityManager instance;
    void Awake()
    {
        if (instance != null) { Debug.Log("more the one ElectricityManager on scen"); return; }
        instance = this;
    }

    public List<EleNetwork> Networks = new List<EleNetwork>();
    private List<ElectricityUser> AllGenerators = new List<ElectricityUser>();
    private List<ElectricityUser> AllRequesters = new List<ElectricityUser>();
    public List<TransmissionTower> AllTransTowers = new List<TransmissionTower>();
    private List<Battery> AllBatteries = new List<Battery>();

    public int tTRange = 7;
    [SerializeField]private float updateDelay = 0.1f;
    private float timeToUpdate = 0.1f;

    private void Update()
    {
        if (timeToUpdate <= 0f)
        {
            timeToUpdate = updateDelay;
            UpdateCharge();
        }
        timeToUpdate -= Time.deltaTime;
    }

    private void UpdateCharge()
    {
        for (int n = 0; n < Networks.Count; n++)
        {
            EleNetwork net = Networks[n];
            ElectricityUser chG;

            net.production = 0f;
            net.request = 0f;

            //requesters
            float toLoad = 0f;
            int qua = 0;
            for (int i = 0; i < net.Requesters.Count; i++)
            {
                chG = net.Requesters[i];
                net.request += chG.actEnergyPerSec;
                if (chG.actCharge < chG.maxCharge)
                {
                    float energy = chG.maxEnergyPerSec * updateDelay;
                    toLoad += energy;
                    qua++;
                }
            }
            if (toLoad > net.charge) { toLoad = net.charge; }
            float avrageLoad = toLoad / qua;
            net.charge -= toLoad;
            for (int i = 0; i < net.Requesters.Count; i++)
            {
                chG = net.Requesters[i];
                if (chG.actCharge < chG.maxCharge)
                {
                    chG.actCharge += avrageLoad;
                }
            }

            //generators
            float maxCharge = net.maxCharge + 10f;
            for (int i = 0; i < net.Generators.Count; i++)
            {
                chG = net.Generators[i];
                net.production += chG.actEnergyPerSec;
                if(net.charge < maxCharge)
                {
                    float energy = chG.maxEnergyPerSec * updateDelay * 1.1f;
                    if (chG.actCharge > energy)
                    {
                        chG.actCharge -= energy;
                        net.charge += energy;
                    }
                }
            }

            //baterys
            float chargePerBatery = net.charge / net.Batteries.Count;
            for (int i = 0; i < net.Batteries.Count; i++)
            {
                net.Batteries[i].charge = chargePerBatery;
            }
        }
    }


    public int GetNetNumOfTTPos(int x, int y)
    {
        for (int n = 0; n < Networks.Count; n++)
        {
            for (int w = 0; w < Networks[n].TransTowersP.Count; w++)
            {
                if (Networks[n].TransTowersP[w].x == x && Networks[n].TransTowersP[w].y == y)
                { return n; }
            }
        }
        return -1;
    }
    public int GetNetNumOfMyPos(int x, int y)
    {
        int rl = tTRange * tTRange;
        int o;
        EleNetwork network;
        Vector3Int ttp;

        for (int n = 0; n < Networks.Count; n++)
        {
            network = Networks[n];
            for (int w = 0; w < network.TransTowersP.Count; w++)
            {
                ttp = network.TransTowersP[w];
                o = (ttp.x - x) * (ttp.x - x) + (ttp.y - y) * (ttp.y - y);
                if (0 <= rl) { return n; }
            }
        }
        return -1;
    }
    public TransmissionTower GetTTOfMyPos(int x, int y)
    {
        int rl = tTRange * tTRange;
        int o;
        EleNetwork network;
        Vector3Int ttp;

        for (int n = 0; n < Networks.Count; n++)
        {
            network = Networks[n];
            for (int w = 0; w < network.TransTowersP.Count; w++)
            {
                ttp = network.TransTowersP[w];
                o = (ttp.x - x) * (ttp.x - x) + (ttp.y - y) * (ttp.y - y);
                if (o <= rl) { return AllTransTowers[ttp.z]; }
            }
        }
        return null;
    }

    public void AddRequester(ElectricityUser GenSc)
    {
        AllRequesters.Add(GenSc);
        if (WorldMenager.instance.loadingWorld == false) { ReloadNetwork(); }
    }
    public void RemoveRequester(ElectricityUser GenSc)
    {
        AllRequesters.Remove(GenSc);
        if (WorldMenager.instance.loadingWorld == false) { ReloadNetwork(); }
    }
    private void AddRequesterToNet(ElectricityUser GenSc)
    {
        int n = GetNetNumOfMyPos(GenSc.x, GenSc.y);
        if (n == -1) { return; }
        Networks[n].Requesters.Add(GenSc);
        Networks[n].maxRequest += GenSc.maxEnergyPerSec;
    }

    public void AddGenerator(ElectricityUser GenSc)
    {
        AllGenerators.Add(GenSc);
        if (WorldMenager.instance.loadingWorld == false) { ReloadNetwork(); }
    }
    public void RemoveGenerator(ElectricityUser GenSc)
    {
        AllGenerators.Remove(GenSc);
        if (WorldMenager.instance.loadingWorld == false) { ReloadNetwork(); }
    }
    private void AddGeneratorToNet(ElectricityUser GenSc)
    {
        int n = GetNetNumOfMyPos(GenSc.x, GenSc.y);
        if (n == -1) { return; }
        Networks[n].Generators.Add(GenSc);
        Networks[n].maxProduction += GenSc.maxEnergyPerSec;
    }

    public void AddBattery(Battery BatSc)
    {
        AllBatteries.Add(BatSc);
        if (WorldMenager.instance.loadingWorld == false) { ReloadNetwork(); }
    }
    public void RemoveBattery(Battery BatSc)
    {
        AllBatteries.Remove(BatSc);
        if (WorldMenager.instance.loadingWorld == false) { ReloadNetwork(); }
    }
    private void AddBatteryToNet(Battery BatSc)
    {
        int n = GetNetNumOfMyPos(BatSc.x, BatSc.y);
        if (n == -1) { return; }
        Networks[n].Batteries.Add(BatSc);
        Networks[n].maxCharge += BatSc.Capacity;
    }

    public void AddTT(TransmissionTower tt)
    {
        AllTransTowers.Add(tt);
        if (WorldMenager.instance.loadingWorld == false) { ReloadNetwork(); }
    }
    public void RemoveTT(TransmissionTower tt)
    {
        AllTransTowers.Remove(tt);
        if (WorldMenager.instance.loadingWorld == false) { ReloadNetwork(); }
    }
    public void ReloadNetwork()
    {
        //Debug.Log("reload Electricity network");

        Networks = new List<EleNetwork>();
        List<Vector3Int> tempP = new List<Vector3Int>();
        List<Vector3Int> pTA = new List<Vector3Int>();
        for (int i = 0; i < AllTransTowers.Count; i++)
        {
            Vector2Int pos = new Vector2Int((int)AllTransTowers[i].transform.position.x / 10, (int)AllTransTowers[i].transform.position.y / 10);
            tempP.Add(new Vector3Int(pos.x, pos.y, i));
        }

        int rl = (tTRange * 2) * (tTRange * 2);
        int net = -1;

        int pow = 100;

        while (tempP.Count > 0)
        {
            net++;
            Vector3Int mP = tempP[0];
            tempP.RemoveAt(0);
            pTA.Add(mP);
            Networks.Add(new EleNetwork());
            Networks[net].TransTowersP.Add(mP);

            while (pTA.Count > 0)
            {
                Vector3Int cP;
                int nDI = 0;
                mP = pTA[0];
                pTA.RemoveAt(0);

                for (int i = 0; i < tempP.Count; i++)
                {
                    cP = tempP[i - nDI];
                    if ((mP.x - cP.x) * (mP.x - cP.x) + (mP.y - cP.y) * (mP.y - cP.y) <= rl)
                    {
                        tempP.RemoveAt(i - nDI);
                        nDI++;
                        pTA.Add(cP);
                        Networks[net].TransTowersP.Add(cP);
                    }
                }

                pow--;
                if (pow <= 0) { Debug.Log("ERROR! while reloading electricity network"); return; }
            }
        }

        //Debug.Log("Created Electricity network: " + AllNet.Count);

        //reload all transmison towers
        for (int n = 0; n < Networks.Count; n++)
        {
            for (int w = 0; w < Networks[n].TransTowersP.Count; w++)
            {
                int trueIndex = Networks[n].TransTowersP[w].z;
                AllTransTowers[trueIndex].network = n;
            }
        }

        //reload all generators
        for (int i = 0; i < AllGenerators.Count; i++)
        {
            AddGeneratorToNet(AllGenerators[i]);
        }

        //reload all requesters
        for (int i = 0; i < AllRequesters.Count; i++)
        {
            AddRequesterToNet(AllRequesters[i]);
        }

        //reload all batteries
        for (int i = 0; i < AllBatteries.Count; i++)
        {
            AddBatteryToNet(AllBatteries[i]);
        }

        //set charge in nettworks
        for (int n = 0; n < Networks.Count; n++)
        {
            float charge = 0f;
            EleNetwork network = Networks[n];
            for (int i = 0; i < network.Batteries.Count; i++)
            {
                charge += network.Batteries[i].charge;
            }
            network.charge = charge;
        }
    }
}
