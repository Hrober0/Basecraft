using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DronBehavior : MonoBehaviour
{
    private DronStation DSSc;
    Vector2 DSPositin;

    private Vector2 target;
    public Transform transToPutUpItem = null;
    public Transform transToPutDownItem = null;
    public ObjToBuild OTB = null;
    private Res resToPutUp = Res.None;

    private float mySpeed = 40f;
    private float rotateSpeed = 6f;
    private Quaternion rotate;

    public Res keptRes = Res.None;
    public bool isFlying = false;
    public int operatingNetwork = 0;

    private Vector3 hideSize = new Vector3(0f, 0f, 1f);
    private Vector3 shownSize = new Vector3(1f, 1f, 1f);
    private float startingTime = 0.7f;

    public List<DroneTask> myTasks = new List<DroneTask>();

    void Update()
    {
        if (myTasks.Count == 0) { return; }
        if (myTasks[0] == DroneTask.None) { return; }

        transform.rotation = Quaternion.Slerp(transform.rotation, rotate, rotateSpeed * Time.deltaTime);
        //if (transform.rotation != rotateQ) { return; }

        if (myTasks[0] == DroneTask.Intlet || myTasks[0] == DroneTask.Outlet) { return; }

        Vector2 dir = target - (Vector2)transform.position;
        float disThisFrame = mySpeed * Time.deltaTime;

        if (dir.magnitude <= disThisFrame)
        {
            DroneTask finishingTask = myTasks[0];
            //Debug.Log(name + ": Ending task:" + finishingTask);
            myTasks.RemoveAt(0);
            switch (finishingTask)
            {
                case DroneTask.ReturnToDS:
                    SetToInlet();
                    return;
                case DroneTask.PutUpItem:
                    PutUpItem();
                    return;
                case DroneTask.PutDownItem:
                    PutDowItem();
                    return;
            }
            return;
        }
        transform.Translate(dir.normalized * disThisFrame, Space.World);
    }

    private void FindNewTask()
    {
        //Debug.Log(name + ": try find new task");
        if (myTasks.Count == 0) { DronControler.instance.TryGetTask(this); }
        if (myTasks.Count == 0) { SetToReturn(); }
    }
    private Transform FindPlatformWithFreePlace()
    {
        Transform trans = DronControler.instance.FindPlatWithPlaceForItem(keptRes, transform.position.x, transform.position.y, operatingNetwork);
        if (trans == null)
        {
            Debug.Log("Error! cant find avaplace to put down kept item, dron is deleting item and go back");
            keptRes = Res.None;
            SetToReturn();
            return null;
        }
        return trans.transform;
    }
    

    public void SetDS(DronStation _DSSc, Vector2 _DSPosition)
    {
        DSSc = _DSSc;
        DSPositin = _DSPosition;
    }
    public Sprite GetDronImage()
    {
        return GetComponent<SpriteRenderer>().sprite;
    }

    public void SetToReturn()
    {
        OTB = null;

        if (myTasks.Count > 0)
        {
            if (myTasks[0] == DroneTask.Outlet) { myTasks = new List<DroneTask> { DroneTask.Outlet }; }
            else { myTasks = new List<DroneTask>(); }
        }

        if (keptRes != Res.None) { SetToPutDownItem(transToPutUpItem); return; }

        myTasks.Add(DroneTask.ReturnToDS);

        DronStation DS = DronControler.instance.GetNearesDS(operatingNetwork, (int)transform.position.x / 10, (int)transform.position.y / 10);
        if (DS != null)
        {
            DSSc = DS;
            DSPositin = DSSc.transform.position;
            SetNewTarget((int)DSPositin.x / 10, (int)DSPositin.y / 10);
        }
        else
        {
            Debug.Log("TODO: dron behavior without DS");
            myTasks = new List<DroneTask>();
        }
        
    }
    
    
    //put down item
    private void SetToPutDownItem(Transform tTPDI)
    {
        if (tTPDI == null || keptRes == Res.None)
        {
            Debug.Log(name + ": wrong set to put down item try find new task");
            keptRes = Res.None;
            transToPutDownItem = null;
            FindNewTask();
            return;
        }
        int lenght = myTasks.Count;
        if (lenght == 0 || lenght > 0 && myTasks[lenght - 1] != DroneTask.PutDownItem) { myTasks.Add(DroneTask.PutDownItem); }
        transToPutDownItem = tTPDI;

        SetNewTarget((int)transToPutDownItem.position.x / 10, (int)transToPutDownItem.position.y / 10);
    }
    private void PutDowItem()
    {
        //Debug.Log("put down");
        if (transToPutDownItem == null)
        {
            Debug.Log(name + ": missing plat to put down item, try find new");
            Transform newTRansToPutDown = FindPlatformWithFreePlace();
            if (newTRansToPutDown == null) { return; }
            SetToPutDownItem(newTRansToPutDown);
            return;
        }

        Transform temTrans = transToPutDownItem;
        if (temTrans.TryGetComponent(out PlatformBehavior PBSc)) { if (keptRes != Res.None) { PBSc.AddItem(keptRes, 1); } }
        else if (temTrans.TryGetComponent(out ObjectPlan OPSc))  { if (keptRes != Res.None) { OPSc.AddItem(keptRes, 1); } TryBuild(); }
        keptRes = Res.None;
        resToPutUp = Res.None;
        transToPutDownItem = null;
        FindNewTask();
    }
    private void TryBuild()
    {
        if (OTB == null) { Debug.Log("OTB is not set doesnt exist"); return; }
        Transform trans = OTB.trans;
        if (trans == null) { Debug.Log("OTB on (" + OTB.xTB + "," + OTB.yTB + ") doesnt exist"); return; }
        ObjectPlan OPSc = trans.GetComponent<ObjectPlan>();
        if (OPSc == null) { Debug.Log("OTB on (" + OTB.xTB + "," + OTB.yTB + ") hasnt border info script"); return; }

        if (OPSc.needItems == null || OPSc.needItems.Count == 0)
        {
            BuildMenager.instance.BuildObj(OTB.objectType, OTB.xTB, OTB.yTB, OTB.neededItems, new Vector2Int(OTB.startPointRoadsX, OTB.startPointRoadsY));
        }
        OTB = null;
    }

    //put up item
    public void SetToDisasemble(Transform trans, Res res)
    {
        //Debug.Log("set to disasemble" + name+" "+_item.res);
        myTasks = new List<DroneTask>();
        if (isFlying == false) { SetToOutlet(); }
        myTasks.Add(DroneTask.PutUpItem);

        transToPutUpItem = trans;
        transToPutDownItem = null;
        OTB = null;
        if (transToPutUpItem == null) { SetToReturn(); return; }
        SetNewTarget((int)transToPutUpItem.position.x / 10, (int)transToPutUpItem.position.y / 10);
        resToPutUp = res;
        myTasks.Add(DroneTask.PutDownItem);
    }
    public void SetToTakeItemToBuild(ObjToBuild newOTB, Transform transToPutUp, Transform transToPutDown, Res res)
    {
        myTasks = new List<DroneTask>();
        OTB = null;

        if (newOTB!=null && res==Res.None && transToPutDown != null)
        {
            //Debug.Log(name + ": Set to build object without idtems");
            transToPutUpItem = null;
            transToPutDownItem = transToPutDown;
            SetNewTarget((int)transToPutDown.position.x / 10, (int)transToPutDown.position.y / 10);
            resToPutUp = res;
            OTB = RecreateOTB(newOTB);
            if (isFlying == false) { SetToOutlet(); }
            myTasks.Add(DroneTask.PutDownItem);
            return;
        }

        if (res == Res.None || transToPutUp == null || transToPutDown == null)
        {
            Debug.Log(name + ": ERROR! Cant set to take item to build");
            resToPutUp = Res.None;
            transToPutUpItem = null;
            SetToReturn();
            return;
        }

        if (isFlying == false) { SetToOutlet(); }

        myTasks.Add(DroneTask.PutUpItem);
        myTasks.Add(DroneTask.PutDownItem);
        transToPutUpItem = transToPutUp;
        SetNewTarget((int)transToPutUpItem.position.x / 10, (int)transToPutUpItem.position.y / 10);
        resToPutUp = res;
        transToPutDownItem = transToPutDown;
        OTB = RecreateOTB(newOTB);
    }
    public void SetToDeliverItem(Transform transToPutUp, Transform transToPutDown, Res res)
    {
        myTasks = new List<DroneTask>();
        OTB = null;

        if (res == Res.None || transToPutUp == null || transToPutDown == null)
        {
            Debug.Log(name + ": ERROR! Cant set to take item to deliver");
            resToPutUp = Res.None;
            transToPutUpItem = null;
            SetToReturn();
            return;
        }

        if (isFlying == false) { SetToOutlet(); }

        myTasks.Add(DroneTask.PutUpItem);
        myTasks.Add(DroneTask.PutDownItem);
        transToPutUpItem = transToPutUp;
        SetNewTarget((int)transToPutUpItem.position.x / 10, (int)transToPutUpItem.position.y / 10);
        resToPutUp = res;
        transToPutDownItem = transToPutDown;
    }
    private void DestroyObjectPlanIfHaveNoItems(ObjectPlan OPSc)
    {
        if (OPSc.keptItems.Count == 0)
        {
            if (AllRecipes.instance.IsItConnection(OPSc.objName))
            { BuildMenager.instance.RemoveConnectionPlan(OPSc.startRoadPoint, OPSc.endRoadPoint); }
            else
            { Vector2Int pos = OPSc.GetTabPos(); BuildMenager.instance.RemoveBuildingPlan(pos.x, pos.y); }
        }
    }
    private void PutUpItem()
    {
        //Debug.Log("put up");
        if (myTasks.Count == 0) { keptRes = Res.None; resToPutUp = Res.None; SetToReturn(); return; }
        if (transToPutUpItem == null) { AddOTBToBuildQueue(); return; }

        if (transToPutDownItem == null && OTB!=null && OTB.trans!=null) { transToPutDownItem = OTB.trans; }
        if (transToPutDownItem == null) { transToPutDownItem = DronControler.instance.FindPlatWithPlaceForItem(resToPutUp, transform.position.x, transform.position.y, operatingNetwork); }
        if (transToPutDownItem == null) { Debug.Log(name + ": i cant put up item because i dont have place to put it down! I will find new task."); AddOTBToBuildQueue(); return; }

        Transform temTrans = transToPutUpItem;
        transToPutUpItem = null;
        if (temTrans.TryGetComponent(out PlatformBehavior PBSc))
        {
            PBSc.AddItem(resToPutUp, -1);
        }
        else if (temTrans.TryGetComponent(out ObjectPlan OPSc))
        {
            OPSc.TakeItem(resToPutUp, 1);
            if(AllRecipes.instance.IsItOreObj(OPSc.objName) || OPSc.objName == Obj.Tree) { TaskManager.instance.AddNewItem(resToPutUp, 1); }
            DestroyObjectPlanIfHaveNoItems(OPSc);
        }
        else
        {
            Debug.Log("ERROR! Object to put up item desnot have correct script");
            keptRes = Res.None;
            AddOTBToBuildQueue();
            return;
        }
        
        keptRes = resToPutUp;
        resToPutUp = Res.None;

        if (OTB != null) { SetTargetToBuild(); return; }

        if (myTasks[0] == DroneTask.PutDownItem)
        { 
            if(transToPutDownItem != null) { SetToPutDownItem(transToPutDownItem); }
            else { SetToPutDownItem(FindPlatformWithFreePlace()); }
            return;
        }

        Debug.Log(name + ": put up item and doesnt know what to do");
        SetToReturn(); return;
    }
    private void SetTargetToBuild()
    {
        //set target
        if (OTB.objectType == Obj.Connection1 || OTB.objectType == Obj.Connection2 || OTB.objectType == Obj.Connection3)
        {
            Vector2 EndRoadPos = new Vector2(OTB.xTB, OTB.yTB);
            Vector2 StartRoadPos = new Vector2(OTB.startPointRoadsX, OTB.startPointRoadsY);
            Vector2 relatve = EndRoadPos - StartRoadPos;
            Vector2 MidPointVector = relatve / 2 + StartRoadPos;
            target = MidPointVector * 10;
        }
        else
        {
            target = new Vector2(OTB.xTB * 10, OTB.yTB * 10);
        }

        //set rotate
        Vector2 dir = target - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rotate = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    private void AddOTBToBuildQueue() //Set After Missing Platform To Put Up Item
    {
        if (OTB == null || OTB.objectType == Obj.None || resToPutUp == Res.None) { SetToReturn(); return; }

        Debug.Log(name + "Adding res to Build que and try get new task");

        int index = BuildMenager.instance.GetIndexOfBuildQue(OTB.objectType,OTB.xTB,OTB.yTB, new Vector2Int(OTB.startPointRoadsX,OTB.startPointRoadsY));
        if (index >= 0)
        {
            bool dodalo = false;
            ObjToBuild otb = BuildMenager.instance.GetOTB(index);
            for (int i = 0; i < otb.neededItems.Count; i++)
            {
                if (otb.neededItems[i].res == resToPutUp)
                {
                    otb.neededItems[i].qua += 1;
                    dodalo = true;
                }
            }
            if (dodalo == false)
            {
                otb.neededItems.Add(new ItemRAQ(resToPutUp, 1));
            }
        }
        FindNewTask();
    }


    private void SetNewTarget(int x, int y)
    {
        //Debug.Log("set target: " + x + " " + y);
        //set position
        target = new Vector2(x * 10, y * 10);

        //set rotate
        Vector2 dir = target - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rotate = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void SetToOutlet()
    {
        if (isFlying == true) { return; }

        transform.position = DSPositin;

        DSSc.TryOpen();
        DSSc.RemoveDron(this);
        myTasks.Add(DroneTask.Outlet);
        gameObject.SetActive(true);
        transform.localScale = hideSize;
        transform.DOScale(shownSize, startingTime).OnComplete(() => GoOutletDron());
        operatingNetwork = DSSc.operatingNetwork;
        isFlying = true;
    }
    private void GoOutletDron()
    {
        if (myTasks.Count==0) { Debug.Log("wrong outlet doment in: " + gameObject.name); return; }
        myTasks.RemoveAt(0);
    }

    private void SetToInlet()
    {
        if (isFlying == false) { return; }

        keptRes = Res.None;
        resToPutUp = Res.None;
        transToPutDownItem = null;
        transToPutUpItem = null;
        OTB = null;

        myTasks = new List<DroneTask>();
        myTasks.Add(DroneTask.Intlet);
        DSSc.TryOpen();
        transform.DOScale(hideSize, startingTime).OnComplete(() => GoInletDron());
    }
    private void GoInletDron()
    {
        myTasks = new List<DroneTask>();

        gameObject.SetActive(false);
        DSSc.AddDron(this);
        isFlying = false;
    }

    public Vector2Int GetDSPos() => new Vector2Int((int)DSPositin.x / 10, (int)DSPositin.y / 10);
    public UnitsData.DroneData GetDronData()
    {
        int[] dronTasks = new int[myTasks.Count];
        for (int i = 0; i < myTasks.Count; i++) { dronTasks[i] = (int)myTasks[i]; }

        Vector2Int pttpu;
        if (transToPutUpItem == null) { pttpu = new Vector2Int(-1, -1); }
        else { pttpu = new Vector2Int((int)(transToPutUpItem.transform.position.x / 10), (int)(transToPutUpItem.transform.position.y / 10)); }
        Vector2Int pttpd;
        if (transToPutDownItem == null) { pttpd = new Vector2Int(-1, -1); }
        else { pttpd = new Vector2Int((int)(transToPutDownItem.transform.position.x / 10), (int)(transToPutDownItem.transform.position.y / 10)); }

        BuildingsData.ObjToBuildData OTBD = null;
        if (OTB != null)
        {
            if (OTB.neededItems == null) { OTB.neededItems = new List<ItemRAQ>(); }
            OTBD = new BuildingsData.ObjToBuildData(OTB.objectType, OTB.planType, OTB.xTB, OTB.yTB, OTB.neededItems.ToArray());
            OTBD.StartPointRoadsX = OTB.startPointRoadsX;
            OTBD.StartPointRoadsY = OTB.startPointRoadsY;
        }
        

        UnitsData.DroneData dronData = new UnitsData.DroneData
            (
            DSPositin.x,
            DSPositin.y,
            isFlying,
            transform.position.x,
            transform.position.y,
            target.x,
            target.y,

            pttpu,
            pttpd,

            resToPutUp,
            keptRes,
            OTBD,
            dronTasks
            );
        return dronData;
    }
    public void SetDron(UnitsData.DroneData dronD)
    {
        int DSX = (int)dronD.dSPositinX / 10;
        int DSY = (int)dronD.dSPositinY / 10;
        DSPositin = new Vector2(dronD.dSPositinX, dronD.dSPositinY);
        Transform DST = WorldMenager.instance.GetTransforOfObj(DSX, DSY);
        if (DST == null) { Debug.Log(name + ": ERROR! Cant find DS for dron: " + transform.name); }
        else { operatingNetwork = DST.GetComponent<DronStation>().operatingNetwork; }

        if (dronD.isFlying)
        {
            gameObject.SetActive(true);
            isFlying = true;

            transform.position = new Vector2(dronD.dronX, dronD.dronY);
            target = new Vector2(dronD.targetX, dronD.targetY);

            transToPutUpItem = WorldMenager.instance.GetTransforOfObj(dronD.pttpux, dronD.pttpuy);
            transToPutDownItem = WorldMenager.instance.GetTransforOfObj(dronD.pttpdx, dronD.pttpdy);

            if (dronD.ObjToBuild == null) { OTB = null; }
            else
            {
                BuildingsData.ObjToBuildData OTBD = dronD.ObjToBuild;
                Obj obj = (Obj)OTBD.objectType;

                Transform trans = null;
                if (AllRecipes.instance.IsItConnection(obj))
                {
                    Obj trueObj;
                    if (OTBD.planType == ObjectPlanType.Building) { trueObj = Obj.ConUnderConstruction; }
                    else { trueObj = Obj.ConUnderDemolition; }
                    Transform platT = WorldMenager.instance.GetTransforOfObj(OTBD.StartPointRoadsX, OTBD.StartPointRoadsY);
                    if (platT == null) { Debug.Log(name + ": ERROR! Try set OTB but not found connection parent"); }
                    else { trans = platT.Find(string.Format("{0}({1}, {2})({3}, {4})", trueObj, OTBD.StartPointRoadsX, OTBD.StartPointRoadsY, OTBD.xTB, OTBD.yTB)); }
                }
                else
                {
                    trans = WorldMenager.instance.GetTransforOfObj(OTBD.xTB, OTBD.yTB);
                }
                if (trans == null) { Debug.Log(name + ": ERROR! OTB transform is null"); }

                transToPutDownItem = trans;

                List<ItemRAQ> neededItems = new List<ItemRAQ>();
                foreach (ItemRAQ item in OTBD.neededItems) { neededItems.Add(item); }

                OTB = new ObjToBuild(obj, OTBD.planType, OTBD.xTB, OTBD.yTB, neededItems, trans);
                OTB.startPointRoadsX = OTBD.StartPointRoadsX;
                OTB.startPointRoadsY = OTBD.StartPointRoadsY;
            }

            resToPutUp = dronD.resToPutUp;
            keptRes = dronD.keptRes;

            for (int i = 0; i < dronD.dronTasks.Length; i++)
            {
                myTasks.Add((DroneTask)dronD.dronTasks[i]);
            }

            if (myTasks.Count == 0) { SetToReturn(); }

            if (myTasks.Count > 0)
            {
                if (myTasks[0] == DroneTask.Outlet) { myTasks.RemoveAt(0); }
                else
                {
                    if (myTasks[0] == DroneTask.Intlet) { SetToInlet(); }
                }
            }

            //Debug.Log("TODO: finish loading dron!");
        }
    }
    private ObjToBuild RecreateOTB(ObjToBuild otb)
    {
        ObjToBuild notb = new ObjToBuild(otb.objectType, otb.planType, otb.xTB, otb.yTB, new List<ItemRAQ>(), otb.trans);
        notb.startPointRoadsX = otb.startPointRoadsX;
        notb.startPointRoadsY = otb.startPointRoadsY;
        return notb;
    }
}