using System.Collections.Generic;
using UnityEngine;

public class RoadBehavior : MonoBehaviour
{
    public PlatformBehavior EPBSc;
    private GameObject myRes;
    private Vector2 startPos, endPos;

    public List<Res> movingRess = new List<Res>();
    private List<Transform> movingT = new List<Transform>();
    private List<Transform> avalibleT = new List<Transform>();

    [Header("Stats")]
    public Obj type = Obj.Connection1;
    public Vector2Int startRoadPoint, endRoadPoint;
    public bool sendOff = false;
    public float sendingDelay = 2f;
    public float transferSpeed = 20f;
    public float avalibleTiemToSend = 0f;
    public int priority = 0;

    void Awake()
    {
        myRes = transform.Find("Res").gameObject;
        myRes.SetActive(false);
    }
    private void Start()
    {
        startPos = transform.Find("Start").position;
        endPos = transform.Find("End").position;
        avalibleT.Add(myRes.transform);
    }
    private void Update()
    {
        if(movingRess.Count == 0) { return; }
        MoveRes();
    }
    private void MoveRes()
    {
        int numberOfDeliveredRes = 0;
        for (int i = 0; i < movingRess.Count; i++)
        {
            int index = i - numberOfDeliveredRes;
            Vector2 dir = endPos - (Vector2)movingT[index].position;
            float disThisFrame = transferSpeed * Time.deltaTime;

            if (dir.magnitude <= disThisFrame)
            {
                Res res = movingRess[index];
                EPBSc.itemOnPlatform[(int)res].qua++;
                EPBSc.AddResToAvalibleResList(res);

                TaskManager.instance.ActItem(res, 1, false);

                movingT[index].gameObject.SetActive(false);
                avalibleT.Add(movingT[index]);
                movingT.RemoveAt(index);
                movingRess.RemoveAt(index);
                numberOfDeliveredRes++;
            }
            else
            {
                movingT[index].Translate(dir.normalized * disThisFrame, Space.World);
            }
        }
    }

    public void SetEPBSc(PlatformBehavior ESc)
    {
        EPBSc = ESc;
    }

    public void RemoveRoad()
    {
        sendOff = true;
        EPBSc.roadListIn.Remove(this);

        RemoveOnlyRoad();
    }
    public void RemoveOnlyRoad()
    {
        //Debug.Log("usuwam droge" + name);
        
        //set squares under road
        Vector3[] tab = WorldMenager.instance.GetDisToPlaceofLine(startRoadPoint.x, startRoadPoint.y, endRoadPoint.x, endRoadPoint.y);
        float platRange = WorldMenager.instance.platformCheckingRadius;
        for (int i = 0; i < tab.Length; i++)
        {
            int ix = (int)tab[i].x;
            int iy = (int)tab[i].y;

            //Debug.Log(string.Format("set square x: {1} y: {2} (odl: {2}) at occupited by connection", ix, iy, tab[i].z));

            Obj iobj = WorldMenager.instance.GetSquer(ix, iy);

            if (iobj == Obj.None || iobj == Obj.Connection1)
            {
                if (tab[i].z <= platRange)
                {
                    WorldMenager.instance.squeresVeribal[ix, iy] -= 1;
                    if (WorldMenager.instance.squeresVeribal[ix, iy] <= 0)
                    {
                        WorldMenager.instance.squares[ix, iy] = Obj.None;
                    }
                }
            }
        }

        if (transform.parent == null) { return; }
        PlatformBehavior PBSc = GetComponentInParent<PlatformBehavior>();
        if (PBSc != null)
        {
            PBSc.roadListOut.Remove(this);
            PBSc.CreatePriorityTab();
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        name = string.Format("{0}-ToDestroy", name);
        Destroy(gameObject, 0.05f);
    }

    public void SendRes(Res resToSend)
    {
        avalibleTiemToSend = WorldMenager.instance.worldTime + sendingDelay;
        EPBSc.itemOnPlatform[0].qua--;

        if (avalibleT.Count == 0)
        {
            GameObject newResGo = Instantiate(myRes, new Vector2(), myRes.transform.rotation);
            newResGo.transform.SetParent(transform);
            newResGo.SetActive(false);
            avalibleT.Add(newResGo.transform);
        }

        Transform newResT = avalibleT[0];
        avalibleT.RemoveAt(0);
        newResT.position = startPos;
        newResT.rotation = new Quaternion();
        newResT.gameObject.SetActive(true);
        movingT.Add(newResT);
        movingRess.Add(resToSend);
    }
    public Res IsSpaceToSendRes(PlatformBehavior PBSc)
    {
        if (sendOff || WorldMenager.instance.worldTime < avalibleTiemToSend) { return Res.None; }
        if (EPBSc.itemSendingType==PlatformItemSendingType.Storage && EPBSc.itemOnPlatform[0].qua <= 0) { return Res.None; }
        int numberOfTry = PBSc.avalibleRes.Count;
        do
        {
            Res checkingRes = PBSc.FindRes();
            if (checkingRes == Res.None) { return Res.None; }
            if (CanSendRes(checkingRes)) { return checkingRes; }
            numberOfTry--;
        } while (numberOfTry > 0);

        return Res.None;

        bool CanSendRes(Res rts)
        {
            if (EPBSc.canGetRes == false) { return false; }

            int index = (int)rts;
            if (EPBSc.itemOnPlatform[index].qua >= EPBSc.itemOnPlatform[index].maxQua) { return false; }

            if (EPBSc.itemOnPlatform[index].canIn == false) { return false; }

            return true;
        }
    }

    public int[] GetMovingRes()
    {
        int[] mr = new int[movingRess.Count];
        for (int i = 0; i < movingRess.Count; i++) { mr[i] = (int)movingRess[i]; }
        return mr;
    }
    public float[] GetMovingResX()
    {
        float[] mr = new float[movingT.Count];
        for (int i = 0; i < movingT.Count; i++) { mr[i] = movingT[i].position.x; }
        return mr;
    }
    public float[] GetMovingResY()
    {
        float[] mr = new float[movingT.Count];
        for (int i = 0; i < movingT.Count; i++) { mr[i] = movingT[i].position.y; }
        return mr;
    }
    public void SetMovingRes(Res res, Vector2 pos)
    {
        GameObject newResGo = Instantiate(myRes, new Vector2(), Quaternion.identity);
        newResGo.transform.SetParent(transform);
        newResGo.transform.position = pos;
        newResGo.SetActive(true);
        movingT.Add(newResGo.transform);
        movingRess.Add(res);
    }
}