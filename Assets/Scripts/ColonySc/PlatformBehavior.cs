using System.Collections.Generic;
using UnityEngine;

public class PlatformBehavior : MonoBehaviour
{
    private readonly int defaultEmptySpace = 30;
    private int numberOfLastCheckingRes = 0;
    private SpriteRenderer image;
    private bool imageOn = false;

    private bool isRemoving = false;

    [Header("Info")]
    public bool canBeConectedIn = true;
    public bool canBeConnectedOut = true;
    public bool canGetRes = true;
    public bool canDronesGetRes = true;
    public bool keepAmountOfRequestedItems = false;
    public bool working = false;

    public float startTaskTime;
    public float taskTime = 0;
    public PlatfotmGUIType usingGuiType = PlatfotmGUIType.Storage;
    public PlatformItemSendingType itemSendingType = PlatformItemSendingType.Storage;
    public int range = -1;
    [SerializeField]private int maxEmptySpace = -1;


    [Header("List")]
    public ROP[] itemOnPlatform;
    
    public List<Res> avalibleRes = new List<Res>();
    public List<RoadBehavior> roadListOut = new List<RoadBehavior>();
    public List<RoadBehavior> roadListIn = new List<RoadBehavior>();
    public List<ObjectPlan> roadBorderListIn = new List<ObjectPlan>();

    public List<ItemRAQ> requestItems = new List<ItemRAQ>();

    public Vector2Int[] priorityTab; // x - priority,  y - road index in RoadListOut

    [System.Serializable]
    public class ROP
    {
        public int qua = 0;
        public int maxQua = 0;
        public bool canIn = true;
        public bool canOut = true;
    }

    private void Awake()
    {
        itemOnPlatform = new ROP[GuiControler.instance.resLenght];
        for (int i = 0; i < itemOnPlatform.Length; i++) { itemOnPlatform[i] = new ROP(); }
        if (maxEmptySpace == -1) { maxEmptySpace = defaultEmptySpace; }

        SetMaxEmptySpace(maxEmptySpace);

        startTaskTime = WorldMenager.instance.worldTime;
    }
    private void Start()
    {
        InvokeRepeating("TrySendRess", 1f, 0.5f);
        if (usingGuiType == PlatfotmGUIType.Storage) { SpownImage(); UpdateImage(); }
    }

    public void UpdateAvalibleResList()
    {
        avalibleRes = new List<Res>();
        for (int i = 1; i < itemOnPlatform.Length; i++)
        {
            if(itemOnPlatform[i].qua>0 && itemOnPlatform[i].canOut == true)
            {
                avalibleRes.Add((Res)i);
            }
        }

        UpdateImage();
    }
    public Res FindRes()
    {
        if (numberOfLastCheckingRes >= avalibleRes.Count-1) { numberOfLastCheckingRes = 0; } else { numberOfLastCheckingRes++; }
        for (int i = numberOfLastCheckingRes; i < avalibleRes.Count; i++)
        {
            int resIndex = (int)avalibleRes[i];
            if (itemOnPlatform[resIndex].qua > 0) { return (Res)resIndex; }
            else { UpdateAvalibleResList(); }
        }
        for (int i = 0; i < numberOfLastCheckingRes; i++)
        {
            int resIndex = (int)avalibleRes[i];
            if (itemOnPlatform[resIndex].qua > 0) { return (Res)resIndex; }
            else { UpdateAvalibleResList(); }
        }
        return Res.None;
    }
    public void CreatePriorityTab()
    {
        priorityTab = new Vector2Int[roadListOut.Count];
        for (int i = 0; i < priorityTab.Length; i++)
        {
            priorityTab[i].x = roadListOut[i].priority;
            priorityTab[i].y = i;
        }
        SortPriorityTab();
    }
    public void SortPriorityTab()
    {
        for (int i = 0; i < priorityTab.Length; i++)
        {
            priorityTab[i].x = roadListOut[priorityTab[i].y].priority;
        }

        for (int iz = 0; iz < priorityTab.Length - 1; iz++)
        {
            int lastCheckedIndex = priorityTab.Length - 1 - iz;
            int indexToSwap = lastCheckedIndex;
            int minQua = priorityTab[lastCheckedIndex].x;
            for (int iw = 0; iw < lastCheckedIndex + 1; iw++)
            {
                if (priorityTab[iw].x < minQua) { indexToSwap = iw; minQua = priorityTab[iw].x; }
            }
            Vector2Int tmpV2I = priorityTab[indexToSwap];
            priorityTab[indexToSwap] = priorityTab[lastCheckedIndex];
            priorityTab[lastCheckedIndex] = tmpV2I;
        }
    }
    private void TrySendRess()
    {
        if (priorityTab.Length == 0) { return; }
        List<int> IndexsToMoveDown = new List<int>();
        for (int i = 0; i < priorityTab.Length; i++)
        {
            int indexRoad = priorityTab[i].y;
            Res checkedRes = roadListOut[indexRoad].IsSpaceToSendRes(this);
            if (checkedRes != Res.None)
            {
                //send res
                roadListOut[indexRoad].SendRes(checkedRes);
                AddItem(checkedRes, -1);
                IndexsToMoveDown.Add(i);
                if (maxEmptySpace - itemOnPlatform[0].qua <= 0) { break; }
            }
        }

        for (int i = 0; i < IndexsToMoveDown.Count; i++)
        {
            MoveDownInPriorityTab(IndexsToMoveDown[i]);
        }

        void MoveDownInPriorityTab(int indexToMove)
        {
            int downIndex = indexToMove;
            int usePriority = priorityTab[indexToMove].x;
            while (downIndex + 1 < priorityTab.Length)
            {
                if (priorityTab[downIndex + 1].x == usePriority)
                { downIndex++; }
                else
                { break; }
            }
            if (downIndex > indexToMove)
            {
                //Debug.Log("move in priority from " + indexToMove + " to " + downIndex + " | m:" + indexToMove + " d:" + downIndex);
                Vector2Int v2iToMove = priorityTab[indexToMove];
                for (int i = indexToMove; i < downIndex; i++)
                {
                    priorityTab[i] = priorityTab[i + 1];
                }
                priorityTab[downIndex] = v2iToMove;
            }
        }
    }
    public void RemoveResFromAvalibleResList(Res res)
    {
        if (avalibleRes.Contains(res)) { avalibleRes.Remove(res); }

        UpdateImage();
    }
    public void AddResToAvalibleResList(Res res)
    {
        if (itemOnPlatform[(int)res].canOut == false) { return; }
        if (avalibleRes.Contains(res)) { return; }
        avalibleRes.Add(res);

        UpdateImage();
    }

    public void AddItem(Res res, int qua, bool isNew=false)
    {
        itemOnPlatform[(int)res].qua += qua;
        itemOnPlatform[0].qua -= qua;
        if (qua > 0)
        {
            if (itemOnPlatform[(int)res].canOut == true) { AddResToAvalibleResList(res); }
            if (!keepAmountOfRequestedItems) { foreach (ItemRAQ item in requestItems) { if (item.res == res) { item.qua--; if (item.qua <= 0) { requestItems.Remove(item); } break; } } }
        }
        else if (qua < 0)
        {
            if (itemOnPlatform[(int)res].qua <= 0) { RemoveResFromAvalibleResList(res); }
        }

        TaskManager.instance.ActItem(res, qua, isNew);
    }

    public void SpownImage()
    {
        if (image != null) { return; }
        
        GameObject newObj = Instantiate(BuildMenager.instance.PlatformItemImage);
        newObj.transform.parent = transform;
        newObj.name = "Image";
        newObj.SetActive(false);
        newObj.transform.localPosition  =  new Vector3(0f, 0f, 0f);
        newObj.transform.localScale = new Vector3(4.5f, 4.5f, 1f);
        image = newObj.GetComponent<SpriteRenderer>();

        GameObject bg = newObj.transform.GetChild(0).gameObject;
        bg.name = "Image";
        bg.transform.localPosition = new Vector3(0f, 0f, 0f);
        bg.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        bg.SetActive(true);

        imageOn = false;
    }
    private void UpdateImage()
    {
        if (image == null) { return; }
        if (usingGuiType == PlatfotmGUIType.Storage)
        {
            if (avalibleRes.Count == 1)
            {
                image.sprite = ImageLibrary.instance.GetResImage(avalibleRes[0]);
                imageOn = true;
                SetVisableImage(true);
            }
            else if(imageOn == true)
            {
                imageOn = false; SetVisableImage(false); image.sprite = null;
            }
        }
    }
    public void UpdateImageR(Res res)
    {
        if (image == null) { return; }
        if (res == Res.None) { image.sprite = null; imageOn = false;  SetVisableImage(false);  }
        else { image.sprite = ImageLibrary.instance.GetResImage(res); imageOn = true; SetVisableImage(true);  }
    }
    public void SetVisableImage(bool value, bool force=false)
    {
        if (image == null) { return; }
        if (value && !imageOn) { image.gameObject.SetActive(false); return; }
        if (force == false && SettingsManager.instance.GetResImageInBuilding() == false) { image.gameObject.SetActive(false); return; }
        image.gameObject.SetActive(value);
    }

    public void SetAllCanInItem(bool value)
    {
        for (int i = 1; i < itemOnPlatform.Length; i++)
        {
            itemOnPlatform[i].canIn = value;
        }
    }

    public ItemRAQ GetRequestItem(Res res)
    {
        foreach (ItemRAQ item in requestItems)
        {
            if (item.res == res) { return item; }
        }
        return new ItemRAQ(Res.None, -1);
    }
    public void SetRequestItem(Res res, int qua)
    {
        ItemRAQ item = GetRequestItem(res);
        if (item.res == Res.None) { if (qua > 0) { requestItems.Add(new ItemRAQ(res, qua)); } return; }
        if (qua <= 0) { requestItems.Remove(item); return; }
        item.qua = qua;
    }

    public void RemovePlatform()
    {
        if (isRemoving) { return; }
        isRemoving = true;

        Vector2Int mpos = GetTabPos();
        Obj mobj = WorldMenager.instance.GetSquer(mpos.x, mpos.y);

        canGetRes = false;

        //remove conn in
        int odj = 0;
        RoadBehavior RD;
        for (int i = 0; i < roadListIn.Count; i++) { RD = roadListIn[i - odj]; if (RD == null) { roadListIn.RemoveAt(i-odj); odj++; } else { RD.RemoveOnlyRoad(); } }
        odj = 0;
        ObjectPlan RBI;
        for (int i = 0; i < roadBorderListIn.Count; i++) { RBI = roadBorderListIn[i - odj]; if (RBI == null) { roadBorderListIn.RemoveAt(i - odj); odj++; } else { BuildMenager.instance.RemoveConnectionPlan(RBI.startRoadPoint, RBI.endRoadPoint); } }

        //remove conn out
        TagsEnum tag;
        foreach (Transform child in transform)
        {
            tag = WorldMenager.instance.TagToTagEnum(child.tag);
            if (tag == TagsEnum.Connection)
            {
                RD = child.GetComponent<RoadBehavior>();
                if (RD == null) { continue; }
                RD.RemoveRoad();
            }
            else if (tag == TagsEnum.ConBorder)
            {
                RBI = child.GetComponent<ObjectPlan>();
                if (RBI == null) { continue; }
                BuildMenager.instance.RemoveConnectionPlan(RBI.startRoadPoint, RBI.endRoadPoint);
            }
        }

        DronControler.instance.RemPBFromList(this);
        name = string.Format("{0}-ToDestroy", name);

        if (mobj == Obj.DroneStation) { DronControler.instance.RemoveDS(GetComponent<DronStation>()); }

        WorldMenager.instance.RemoveObjFromGO(gameObject, mpos.x, mpos.y); 
    }

    public Vector2Int GetTabPos() { return new Vector2Int((int)(transform.position.x/10), (int)(transform.position.y/10)); }
    public Transform GetTransform() => transform;
    public Obj IsConnection(int ex, int ey)
    {
        int sx = (int)(transform.position.x / 10);
        int sy = (int)(transform.position.y / 10);
        if (transform.Find(string.Format("{0}({1}, {2})({3}, {4})", Obj.Connection1, sx, sy, ex, ey)) != null) { return Obj.Connection1; }
        if (transform.Find(string.Format("{0}({1}, {2})({3}, {4})", Obj.Connection2, sx, sy, ex, ey)) != null) { return Obj.Connection2; }
        if (transform.Find(string.Format("{0}({1}, {2})({3}, {4})", Obj.Connection3, sx, sy, ex, ey)) != null) { return Obj.Connection3; }
        if (transform.Find(string.Format("{0}({1}, {2})({3}, {4})", Obj.ConUnderConstruction, sx, sy, ex, ey)) != null) { return Obj.ConUnderConstruction; }
        if (transform.Find(string.Format("{0}({1}, {2})({3}, {4})", Obj.ConUnderDemolition, sx, sy, ex, ey)) != null) { return Obj.ConUnderDemolition; }
        return Obj.None;
    }

    public void SetMaxEmptySpace(int qua)
    {
        if (WorldMenager.instance.loadingWorld) { return; }
        maxEmptySpace = qua;
        int freeSpace = qua;
        for (int i = 1; i < itemOnPlatform.Length; i++)
        {
            freeSpace -= itemOnPlatform[i].qua;
        }
        itemOnPlatform[0].qua = freeSpace;

        if (itemSendingType == PlatformItemSendingType.Storage)
        { for (int i = 0; i < itemOnPlatform.Length; i++) { itemOnPlatform[i].maxQua = qua; } }
    }
    public int MaxEmptySpace => maxEmptySpace;
}