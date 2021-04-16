using System.Collections.Generic;
using UnityEngine;

class DSNetwork
{
    public List<Vector3Int> AllDSP; //pos + ALLDS index
    public List<PlatformBehavior> availablePBSc;
    public List<ObjectPlan> availableOPSc;
    public int[] availbleItemsN;    //number of item
    public float[] availbleItemsUT; //last uppdate time

    public DSNetwork()
    {
        AllDSP = new List<Vector3Int>();
        availbleItemsN = new int[GuiControler.instance.resLenght];
        availbleItemsUT = new float[GuiControler.instance.resLenght];
        availablePBSc = new List<PlatformBehavior>();
        availableOPSc = new List<ObjectPlan>();
    }

    public void UpdateAvaliblePBSc()
    {
        availablePBSc = new List<PlatformBehavior>();
        int rl = DronControler.instance.DSRange * DronControler.instance.DSRange;
        foreach (PlatformBehavior PBSc in DronControler.instance.GetAllPBSc)
        {
            Vector2Int tar = PBSc.GetTabPos();
            for (int n = 0; n < AllDSP.Count; n++)
            {
                Vector3Int checDS = AllDSP[n];
                //Debug.Log("cheking range otbx: " + tar.x + " otby: " + tar.y + " DS: " + checDS);
                if ((tar.x - checDS.x) * (tar.x - checDS.x) + (tar.y - checDS.y) * (tar.y - checDS.y) <= rl)
                { availablePBSc.Add(PBSc); break; }
            }
        }
    }
    public void UpdateAvalibleOPSc()
    {
        availableOPSc = new List<ObjectPlan>();
        int rl = DronControler.instance.DSRange * DronControler.instance.DSRange;
        List<ObjectPlan> objectPlans = DronControler.instance.GetAllOPSc;
        int l = objectPlans.Count;
        int r = 0;
        for (int i = 0; i < l; i++)
        {
            ObjectPlan oPSc = objectPlans[i - r];
            if (oPSc == null)
            {
                objectPlans.RemoveAt(i - r);
                r++;
                continue;
            }

            Vector2Int tar = oPSc.GetTabPos();
            for (int n = 0; n < AllDSP.Count; n++)
            {
                Vector3Int checDS = AllDSP[n];
                //Debug.Log("cheking range otbx: " + tar.x + " otby: " + tar.y + " DS: " + checDS);
                if ((tar.x - checDS.x) * (tar.x - checDS.x) + (tar.y - checDS.y) * (tar.y - checDS.y) <= rl)
                {
                    availableOPSc.Add(oPSc);
                    break;
                }
            }
        }
    }
    public void UpdateAvalibleItems(Res res)
    {
        //Debug.Log("UpdateAvalibleItems: " + res);
        int itemI = (int)res;
        availbleItemsN[itemI] = 0;
        availbleItemsUT[itemI] = WorldMenager.instance.worldTime;
        foreach (PlatformBehavior uAPBSc in availablePBSc)
        {
            if (!uAPBSc.canDronesGetRes) { continue; }
            switch (uAPBSc.itemSendingType)
            {
                case PlatformItemSendingType.Storage:
                    if (uAPBSc.itemOnPlatform[itemI].qua > 0) { availbleItemsN[itemI] += uAPBSc.itemOnPlatform[itemI].qua; }
                    break;
                case PlatformItemSendingType.Procesing:
                    if (uAPBSc.itemOnPlatform[itemI].qua > 0 && uAPBSc.itemOnPlatform[itemI].canOut) { availbleItemsN[itemI] += uAPBSc.itemOnPlatform[itemI].qua; }
                    break;
            }
        }

        foreach (ObjectPlan uAOPSc in availableOPSc)
        {
            foreach (ItemRAQ item in uAOPSc.keptItems)
            {
                if (uAOPSc.planType != ObjectPlanType.Disasemble) { continue; }
                if (item.res == res)
                {
                    if (item.qua > 0) { availbleItemsN[itemI] += item.qua; }
                    break;
                }
            }
        }
    }
    public int GetQuaOfItems(Res res)
    {
        if (availbleItemsUT[(int)res] + DronControler.instance.ItemsUpdateDelay < WorldMenager.instance.worldTime)
        {
            UpdateAvalibleItems(res);
        }
        return availbleItemsN[(int)res];
    }
}


public class DronControler : MonoBehaviour
{
    public static DronControler instance;
    void Awake()
    {
        if (instance != null) { Debug.Log("more the one DronControler on scen"); return; }
        instance = this;
    }

    private List<PlatformBehavior> allPBSc = new List<PlatformBehavior>();
    private List<ObjectPlan> allOPSc = new List<ObjectPlan>();
    public List<PlatformBehavior> GetAllPBSc { get => allPBSc; }
    public List<ObjectPlan> GetAllOPSc { get => allOPSc; }

    public List<DronStation> AllDS = new List<DronStation>();
    public List<DronBehavior> AllDrons = new List<DronBehavior>();
    private List<DSNetwork> AllDSNet = new List<DSNetwork>();

    public GameObject Dron;

    [SerializeField] private int numberOfDron = 0;
    [SerializeField] private int dSRange = 20;              public int DSRange { get => dSRange; }
    [SerializeField] private float itemsUpdateDelay = 2f;   public float ItemsUpdateDelay { get => itemsUpdateDelay; }

    public void SpownDron(Transform DronStationT, bool isInDS, UnitsData.DroneData droneData=null)
    {
        GameObject newDron = Instantiate(Dron, transform.position, Quaternion.identity);
        DronBehavior DBSc = newDron.GetComponent<DronBehavior>();
        DronStation DSSc = DronStationT.GetComponent<DronStation>();
        DBSc.SetDS(DSSc, DronStationT.position);
        
        newDron.transform.parent = transform;
        newDron.name = string.Format("Dron{0}", numberOfDron);
        newDron.SetActive(false);
        numberOfDron++;
        AllDrons.Add(DBSc);

        if (isInDS)
        {
            newDron.transform.position = DronStationT.position;
            DSSc.AddDron(DBSc);
        }

        if (droneData != null)
        {
            DBSc.SetDron(droneData);
        }
    }

    public void TryGetTask(DronBehavior DBSc)
    {
        int networkNumber = DBSc.operatingNetwork;

        if (networkNumber < 0 || networkNumber >= AllDSNet.Count) { Debug.LogError("This network of drons does not exist!"); return; }

        DSNetwork checkedNetwork = AllDSNet[networkNumber];
        
        //Build or Disasemble
        for (int i = 0; i < BuildMenager.instance.BuildQueue.Count; i++)
        {
            ObjToBuild checkingOTB = BuildMenager.instance.GetOTB(i);

            //check if is in range DS network
            bool found = false;
            int rl = dSRange * dSRange;
            for (int n = 0; n < checkedNetwork.AllDSP.Count; n++)
            {
                Vector3Int checDS = checkedNetwork.AllDSP[n];
                //Debug.Log("cheking range otbx: " + checkingOTB.xTB + " otby: " + checkingOTB.yTB + " DS: " + checDS);
                if((checkingOTB.xTB-checDS.x)*(checkingOTB.xTB - checDS.x) + (checkingOTB.yTB - checDS.y)* (checkingOTB.yTB - checDS.y) <= rl) { found = true; break; }
            }
            if (found == false) { continue; }

            //dismental building
            if (checkingOTB.planType==ObjectPlanType.Disasemble || checkingOTB.objectType==Obj.BuildingUnderDemolition || checkingOTB.objectType == Obj.ConUnderDemolition)
            {
                for (int ii = 0; ii < checkingOTB.neededItems.Count; ii++)
                {
                    Res res = checkingOTB.neededItems[ii].res;

                    //check free place
                    bool foundP = false;
                    int resIndex = (int)res;
                    foreach (PlatformBehavior uAPBSc in checkedNetwork.availablePBSc)
                    {
                        if (uAPBSc.itemOnPlatform[0].qua <= 0) { continue; }
                        if (uAPBSc.itemOnPlatform[resIndex].qua >= uAPBSc.itemOnPlatform[resIndex].maxQua) { continue; }
                        if (uAPBSc.itemOnPlatform[resIndex].canIn) { foundP = true; break; }
                    }
                    if (foundP == false) { continue; }
                    
                    checkingOTB.neededItems[ii].qua--;

                    //remove item from needItems (if is null)
                    if (checkingOTB.neededItems[ii].qua <= 0) { checkingOTB.neededItems.RemoveAt(ii); }

                    //remove OTB from build que (if is null)
                    if (checkingOTB.neededItems.Count == 0) { BuildMenager.instance.BuildQueue.Remove(checkingOTB); }

                    //send dron
                    DBSc.SetToDisasemble(checkingOTB.trans, res);
                    return;
                }
                continue;
            }

            //no need items building
            if (checkingOTB.neededItems.Count == 0)
            {
                //send dron
                DBSc.SetToTakeItemToBuild(checkingOTB, null, checkingOTB.trans, Res.None);

                //remove OTB from build que
                BuildMenager.instance.BuildQueue.Remove(checkingOTB);
                return;
            }

            //other building
            for (int ii = 0; ii < checkingOTB.neededItems.Count; ii++)
            {
                Res needRes = checkingOTB.neededItems[ii].res;
                if (checkedNetwork.GetQuaOfItems(needRes) <= 0) { continue; }

                //check avalible items
                Transform FoundObjetTrans = FindAvaPlatWithItem(needRes, DBSc.transform.position.x, DBSc.transform.position.y, checkingOTB.xTB, checkingOTB.yTB, networkNumber);
                if (FoundObjetTrans == null) { continue; }

                //if put dow item to object plan than remove res from that need items
                if(FoundObjetTrans.TryGetComponent(out ObjectPlan OPSc))
                {
                    Vector2Int pos = OPSc.GetTabPos();
                    int index = BuildMenager.instance.GetIndexOfBuildQue(OPSc.objName, pos.x, pos.y, OPSc.startRoadPoint);
                    if (index == -1) { Debug.Log("Object plan (" + OPSc.objName + " " + pos + ") desnt exist in build que!"); continue; }
                    else
                    {
                        ObjToBuild objToBuild = BuildMenager.instance.GetOTB(index);
                        for (int i3 = 0; i3 < objToBuild.neededItems.Count; i3++)
                        {
                            if (objToBuild.neededItems[i3].res == needRes)
                            {
                                objToBuild.neededItems[i3].qua--;
                                if (objToBuild.neededItems[i3].qua <= 0) { objToBuild.neededItems.RemoveAt(i3); }
                                break;
                            }
                        }
                        //remove OTB from build que (if is null)
                        if (objToBuild.neededItems.Count == 0)
                        {
                            BuildMenager.instance.BuildQueue.Remove(objToBuild);
                        }
                    }
                }

                checkedNetwork.availbleItemsN[(int)needRes] -= 1;
                checkingOTB.neededItems[ii].qua--;

                //remove item from needItems (if is null)
                if (checkingOTB.neededItems[ii].qua <= 0) { checkingOTB.neededItems.RemoveAt(ii); }

                //remove OTB from build que (if is null)
                if (checkingOTB.neededItems.Count == 0) {  BuildMenager.instance.BuildQueue.Remove(checkingOTB); }

                //send dron
                DBSc.SetToTakeItemToBuild(checkingOTB, FoundObjetTrans, checkingOTB.trans, needRes);
                return;
            }
        }

        //Deliver item
        //Debug.Log("Deliver item");
        foreach (PlatformBehavior pBSc in checkedNetwork.availablePBSc)
        {
            foreach (ItemRAQ item in pBSc.requestItems)
            {
                if (pBSc.keepAmountOfRequestedItems && pBSc.itemOnPlatform[(int)item.res].qua >= item.qua) { continue; }

                int qua = checkedNetwork.GetQuaOfItems(item.res);
                //Debug.Log("Checked " + item.res + " to deliver. In net: " + qua + " of ");

                if (qua > 0)
                {
                    //send to deliver item
                    Vector2Int pos = pBSc.GetTabPos();
                    Transform FoundObjetTrans = FindAvaPlatWithItem(item.res, DBSc.transform.position.x, DBSc.transform.position.y, pos.x, pos.y, networkNumber, pBSc.transform);
                    if (FoundObjetTrans == null) { continue; }

                    //send dron
                    DBSc.SetToDeliverItem(FoundObjetTrans, pBSc.transform, item.res);
                    return;
                }
            }
        }
    }
    public DronStation GetNearesDS(int network, int posX, int posY)
    {
        if (AllDSNet == null) { Debug.Log("Drons net doesnt exist!"); }
        //Debug.Log("AllDSNet: " + AllDSNet.Count + " net: " + network + " DScount: " + AllDS.Count);
        if (network >= 0 && network < AllDSNet.Count)
        {
            int shortdistance = int.MaxValue;
            int DSIndex = -1;
            for (int i = 0; i < AllDSNet[network].AllDSP.Count; i++)
            {
                Vector3Int checDS = AllDSNet[network].AllDSP[i];
                int distance = (posX - checDS.x) * (posX - checDS.x) + (posY - checDS.y) * (posY - checDS.y);
                if (distance < shortdistance) 
                { 
                    shortdistance = distance;
                    DSIndex = checDS.z;
                }
            }
            if (DSIndex == -1) { return null; }
            return AllDS[DSIndex];
        }

        Debug.Log("TODO: cant find find neares DS");
        return null;
    }

    public void AddDS(DronStation _DS)
    {
        AllDS.Add(_DS);
        if (WorldMenager.instance.loadingWorld == false) { ReloadDSNetwork(); }
    }
    public void RemoveDS(DronStation _DS)
    {
        AllDS.Remove(_DS);
        if (WorldMenager.instance.loadingWorld == false) { ReloadDSNetwork(); }
    }
    public void ReloadDSNetwork()
    {
        //Debug.Log("reload DS network" + " DScount: " + AllDS.Count);

        AllDSNet = new List<DSNetwork>();
        List<Vector3Int> tempP = new List<Vector3Int>();
        List<Vector3Int> pTA = new List<Vector3Int>();
        for (int i = 0; i < AllDS.Count; i++)
        {
            Vector2Int pos = AllDS[i].GetTabPos();
            tempP.Add(new Vector3Int(pos.x, pos.y, i));
        }

        int rl = (dSRange * 2) * (dSRange * 2);
        int net = -1;

        int pow = 100;

        while (tempP.Count > 0)
        {
            net++;
            Vector3Int mP = tempP[0];
            tempP.RemoveAt(0);
            pTA.Add(mP);
            //AllDSNet.Add(new List<Vector3Int> { mP });
            AllDSNet.Add(new DSNetwork());
            AllDSNet[net].AllDSP.Add(mP);

            while (pTA.Count > 0)
            {
                Vector3Int cP;
                int nDI = 0;
                mP = pTA[0];
                pTA.RemoveAt(0);

                for (int i = 0; i < tempP.Count; i++)
                {
                    cP = tempP[i - nDI];
                    if((mP.x-cP.x)*(mP.x - cP.x) + (mP.y - cP.y)* (mP.y - cP.y) <= rl)
                    {
                        tempP.RemoveAt(i - nDI);
                        nDI++;
                        pTA.Add(cP);
                        AllDSNet[net].AllDSP.Add(cP);
                    }
                }

                pow--;
                if (pow <= 0) { Debug.Log("ERROR! while reloading dron network"); return; }
            }
        }

        for (int n = 0; n < AllDSNet.Count; n++)
        {
            for (int w = 0; w < AllDSNet[n].AllDSP.Count; w++)
            {
                int trueIndex = AllDSNet[n].AllDSP[w].z;
                AllDS[trueIndex].operatingNetwork = n;
            }
        }

        UpdateAvaliblePBSc();
        UpdateAvalibleOPSc();
    }

    public Transform FindAvaPlatWithItem(Res res, float dronePosX, float dronePosY, int tPosX, int tPosY, int net, Transform bannedObject = null)
    {
        if (net < 0 || net >= AllDSNet.Count) { Debug.LogError("This network of drons does not exist!"); return null; }

        dronePosX /= 10;
        dronePosY /= 10;
        int itemI = (int)res;
        Transform foundTrans = null;
        float shortL = float.MaxValue;
        float dd, dt, dx, dy;

        //Debug.Log("szukam platformy z " + res + " DronX: " + dPosX + " DronY: " + dPosY + " TargetX: " + tPosX + " TargetY: " + tPosY);

        if (res == Res.None) { Debug.Log("Error! Wrong request! Can try find platform with none res"); return null; }

        if (AllDSNet[net].GetQuaOfItems(res) <= 0) { return null; }

        //distans to target
        dx = tPosX - dronePosX; if (dx < 0) { dx *= -1; }
        dy = tPosY - dronePosY; if (dy < 0) { dy *= -1; }
        if (dx > dy) { dt = dx; }
        else { dt = dy; }

        foreach (PlatformBehavior uAPBSc in AllDSNet[net].availablePBSc)
        {
            Vector2Int pos = uAPBSc.GetTabPos();
            if (pos.x==tPosX && pos.y==tPosY) { continue; }
            switch (uAPBSc.itemSendingType)
            {
                case PlatformItemSendingType.Storage:
                    if (uAPBSc.itemOnPlatform[itemI].qua > 0) { CheckAndSet(pos, uAPBSc.transform); }
                    break;
                case PlatformItemSendingType.Procesing:
                    if (uAPBSc.itemOnPlatform[itemI].qua > 0 && uAPBSc.itemOnPlatform[itemI].canOut) { CheckAndSet(pos, uAPBSc.transform); }
                    break;
            }
        }
        
        foreach (ObjectPlan uAOPSc in AllDSNet[net].availableOPSc)
        {
            foreach (ItemRAQ item in uAOPSc.keptItems)
            {
                if (uAOPSc.planType != ObjectPlanType.Disasemble) { continue; }
                if (item.res == res)
                {
                    if (item.qua > 0) { CheckAndSet(uAOPSc.GetTabPos(), uAOPSc.transform); }
                    break;
                }
            }
        }
        
        return foundTrans;

        void CheckAndSet(Vector2Int _pos, Transform trans)
        {
            if (trans == bannedObject) return;

            //distans to grab platform
            dx = _pos.x - dronePosX; if (dx < 0) { dx *= -1; }
            dy = _pos.y - dronePosY; if (dy < 0) { dy *= -1; }
            if ( dx > dy) { dd = dx; }
            else { dd = dy; }
            
            if (dd+dt < shortL)
            {
                shortL = dd + dt;
                foundTrans = trans;
            }
        }
    }
    public Transform FindPlatWithPlaceForItem(Res res, float dronePosX, float dronePosY, int net)
    {
        if (net < 0 || net >= AllDSNet.Count) { Debug.LogError("This network of drons does not exist!"); return null; }

        dronePosX /= 10;
        dronePosY /= 10;
        Transform foundTrans = null;
        float shortL = float.MaxValue;
        float dd, dx, dy;
        int resIndex = (int)res;

        //Debug.Log("szukam platformy z miejscem na  " + res + " DronX: " + dPosX + " DronY: " + dPosY + " TargetX: " + tPosX + " TargetY: " + tPosY);

        foreach (PlatformBehavior uAPBSc in AllDSNet[net].availablePBSc)
        {
            if (!uAPBSc.canDronesGetRes) { continue; }
            if (uAPBSc.itemOnPlatform[0].qua <= 0) { continue; }
            if (uAPBSc.itemOnPlatform[resIndex].qua >= uAPBSc.itemOnPlatform[resIndex].maxQua) { continue; }
            if (uAPBSc.itemOnPlatform[resIndex].canIn) { CheckAndSet(uAPBSc); }
        }

        return foundTrans;

        void CheckAndSet(PlatformBehavior uAPBSc)
        {
            //distans to grab platform
            Vector2 targetPos = uAPBSc.GetTabPos();
            dx = targetPos.x - dronePosX; if (dx < 0) { dx *= -1; }
            dy = targetPos.y - dronePosY; if (dy < 0) { dy *= -1; }
            if (dx > dy) { dd = dx; }
            else { dd = dy; }

            if (dd < shortL)
            {
                shortL = dd;
                foundTrans = uAPBSc.transform;
            }
        }
    }
    private void UpdateAvaliblePBSc()
    {
        foreach (DSNetwork net in AllDSNet)
        {
            net.UpdateAvaliblePBSc();
        }
    }
    private void UpdateAvalibleOPSc()
    {
        foreach (DSNetwork net in AllDSNet)
        {
            net.UpdateAvalibleOPSc();
        }
    }

    public void AddPBToList(PlatformBehavior PBSc)
    {
        allPBSc.Add(PBSc);
        UpdateAvaliblePBSc();
    }
    public void RemPBFromList(PlatformBehavior PBSc)
    {
        allPBSc.Remove(PBSc);
        UpdateAvaliblePBSc();
    }
    public void AddOPToList(ObjectPlan POSc)
    {
        allOPSc.Add(POSc);
        UpdateAvalibleOPSc();
    }
    public void RemOPFromList(ObjectPlan POSc)
    {
        if (allOPSc.Contains(POSc))
        {
            allOPSc.Remove(POSc);
            UpdateAvalibleOPSc();
        }
    }

    // other
    public int GetNumberOfResInObject(Obj obj, Res res)
    {
        int qua = 0;

        Vector2Int pos;
        foreach (PlatformBehavior pb in allPBSc)
        {
            pos = pb.GetTabPos();
            if(WorldMenager.instance.squares[pos.x, pos.y] == obj)
            {
                qua += pb.itemOnPlatform[(int)res].qua;
            }
        }

        return qua;
    }
}
