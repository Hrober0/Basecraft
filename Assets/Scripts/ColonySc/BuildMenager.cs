using System.Collections.Generic;
using UnityEngine;

public class BuildMenager : MonoBehaviour
{

    public static BuildMenager instance;
    void Awake()
    {
        if (instance != null) { Debug.Log("more the one BuildMenager on scen"); return; }
        instance = this;
    }

    [Header("Buildings prefab to set")]
    [SerializeField] private GameObject ConstructionBuilding = null;
    [SerializeField] private GameObject DisasembleBuilding = null;
    [SerializeField] private GameObject RoadBorder = null;

    [SerializeField] private GameObject Connection1 = null;
    [SerializeField] private GameObject Connection2 = null;
    [SerializeField] private GameObject Connection3 = null;

    [SerializeField] private GameObject Warehouse = null;
    [SerializeField] private GameObject Warehouse2 = null;
    [SerializeField] private GameObject WoodCuter = null;
    [SerializeField] private GameObject Quarry = null;
    [SerializeField] private GameObject Planter = null;
    [SerializeField] private GameObject Smelter = null;
    [SerializeField] private GameObject Crusher = null;
    [SerializeField] private GameObject Pump = null;
    [SerializeField] private GameObject Farm = null;
    [SerializeField] private GameObject ScrapHeap = null;

    [SerializeField] private GameObject Balista = null;
    [SerializeField] private GameObject GunTurret = null;
    [SerializeField] private GameObject LaserTurret = null;
    [SerializeField] private GameObject RocketTurret = null;

    [SerializeField] private GameObject Wall0 = null;
    [SerializeField] private GameObject Wall0Connector = null;
    [SerializeField] private GameObject Wall1 = null;
    [SerializeField] private GameObject Wall1Connector = null;
    [SerializeField] private GameObject Wall2 = null;
    [SerializeField] private GameObject Wall2Connector = null;
    [SerializeField] private GameObject Wall3 = null;
    [SerializeField] private GameObject Wall3Connector = null;

    [SerializeField] private GameObject BasicCrafter = null;
    [SerializeField] private GameObject Crafter = null;
    [SerializeField] private GameObject ChemicalPlant = null;
    [SerializeField] private GameObject DronStation = null;

    [SerializeField] private GameObject Connector = null;
    [SerializeField] private GameObject FastConnector = null;

    [SerializeField] private GameObject TransmissionTower = null;
    [SerializeField] private GameObject WindTurbine1 = null;
    [SerializeField] private GameObject WindTurbine2 = null;
    [SerializeField] private GameObject CombustionGenerator = null;
    [SerializeField] private GameObject SteemGenerator = null;
    [SerializeField] private GameObject Battery1 = null;
    [SerializeField] private GameObject SolarPanel1 = null;

    [SerializeField] private GameObject ElectricSmelter = null;
    [SerializeField] private GameObject Repairer = null;

    [SerializeField] private GameObject Launchpad = null;
    [SerializeField] private GameObject SpaceRequester = null;

    [Header("Platform item imaga prefab to set")]
    public GameObject PlatformItemImage;

    [Header("BuildQueue")]
    public List<ObjToBuild> BuildQueue = new List<ObjToBuild>();

    //creating smt
    public void AddCuttingTreeTask(int x, int y)
    {
        if(WorldMenager.instance.GetSquer(x, y) != Obj.Tree) { return; }
        int numOfSupling = 1;
        int randomInt = UnityEngine.Random.Range(0, 11);
        if (randomInt < 2) { numOfSupling = 0; }
        else if (randomInt > 7) { numOfSupling = 2; }
        WorldMenager.instance.RemoveObjFromPos(x, y);
        List<ItemRAQ> IIR = new List<ItemRAQ>();
        IIR.Add(new ItemRAQ(Res.Wood, UnityEngine.Random.Range(1, 5)));
        if (numOfSupling > 0) { IIR.Add(new ItemRAQ(Res.Sapling, numOfSupling)); }
        CreateDisasembleObject(Obj.Tree, x, y, IIR);
    }
    public void AddMiningTask(Obj obj, Res res, int x, int y, int qua)
    {
        if (WorldMenager.instance.GetSquer(x, y) == Obj.None)
        {
            CreateDisasembleObject(obj, x, y, new List<ItemRAQ> { new ItemRAQ(res, qua) });
        }
        else
        {
            Transform trans = WorldMenager.instance.GetTransforOfObj(x, y);
            if (trans == null) { return; }
            ObjectPlan OP = trans.GetComponent<ObjectPlan>();

            //building que
            int queIndex = GetIndexOfBuildQue(obj, x, y, new Vector2Int());
            if (queIndex == -1)
            {
                BuildQueue.Add(new ObjToBuild(obj, ObjectPlanType.Disasemble, x, y, new List<ItemRAQ> { new ItemRAQ(res, qua) }, trans));
                //show
                OP.AddItem(res, qua);
            }
            else
            {
                ObjToBuild ObjTB = GetOTB(queIndex);
                for (int i = 0; i < ObjTB.neededItems.Count; i++)
                {
                    if (ObjTB.neededItems[i].res == res)
                    {
                        ObjTB.neededItems[i].qua++;
                        //show
                        OP.AddItem(res, qua);
                        break;
                    }
                }
            }
        }
    }
    public void AddObjToBuild(Obj objTB, int px, int py, Vector2Int v2)
    {
        List<ItemRAQ> NeededItems = AllRecipes.instance.GetNeedItems(objTB);
        if (NeededItems == null) { NeededItems = new List<ItemRAQ>(); }

        if (AllRecipes.instance.IsItConnection(objTB)) {
            int calcMultiplier = CalcConItemMultiplayer(objTB, v2, new Vector2Int(px, py));
            for (int i = 0; i < NeededItems.Count; i++) { NeededItems[i].qua *= calcMultiplier; }
        }

        Transform trans;
        if(AllRecipes.instance.IsItConnection(objTB))
        {
            trans = CreateConPlan(objTB, v2.x, v2.y, px, py, AllRecipes.instance.GetNeedItems(objTB));
        }
        else
        {
            if (WorldMenager.instance.GetSquer(px, py) != Obj.None) { return; }
            trans = CreateBuildingPlan( objTB, px, py, AllRecipes.instance.GetNeedItems(objTB));
        }
        ObjToBuild OTB = new ObjToBuild(objTB, ObjectPlanType.Building, px, py, NeededItems, trans);
        if (AllRecipes.instance.IsItConnection(objTB))
        {
            OTB.startPointRoadsX = v2.x;
            OTB.startPointRoadsY = v2.y;
        }
        BuildQueue.Add(OTB);

        if (objTB == Obj.Wall0 || objTB == Obj.Wall1 || objTB == Obj.Wall2 || objTB == Obj.Wall3)
        {
            ClickMenager.instance.HidePointers(true);
            GuiControler.instance.SetWallsC(new Vector2Int(px, py), objTB);
        }
    }
    public Transform CreateBuildingPlan(Obj ObjToBuild, int wx, int wy, List<ItemRAQ> items)
    {
        if (ObjToBuild == Obj.BuildingUnderDemolition) { Debug.Log("ERROR! Cant build: " + ObjToBuild + " because this function does not support this. Use another function (Create ....)"); return null; }

        GameObject NewObj = Instantiate(ConstructionBuilding, new Vector2(wx * 10, wy * 10), Quaternion.identity);
        NewObj.transform.SetParent(transform);
        NewObj.SetActive(true);

        ObjectPlan OPSc = NewObj.GetComponent<ObjectPlan>();
        OPSc.objName = ObjToBuild;
        OPSc.planType = ObjectPlanType.Building;
        OPSc.startRoadPoint = new Vector2Int(wx, wy);
        OPSc.endRoadPoint = new Vector2Int(wx, wy);

        NewObj.name = string.Format("{0}({1}, {2})", Obj.BuildingUnderConstruction, wx, wy);
        WorldMenager.instance.squares[wx, wy] = Obj.BuildingUnderConstruction;
        WorldGrid.SetSquare(new Square(Obj.BuildingUnderConstruction, wx, wy, 1, NewObj.transform));
        OPSc.needItems = items;

        DronControler.instance.AddOPToList(OPSc);

        SpriteRenderer image = NewObj.transform.Find("Image").GetComponent<SpriteRenderer>();

        //act stats
        TaskManager.instance.ActBuilding(Obj.BuildingUnderConstruction, 1);

        Sprite sprite = ImageLibrary.instance.GetObjImages(ObjToBuild);
        if (sprite == null) { image.enabled = false; }
        else { image.sprite = sprite; image.enabled = true; }

        return NewObj.transform;
    }
    public Transform CreateDisasembleObject(Obj ObjToDisasemble, int wx, int wy, List<ItemRAQ> items)
    {
        GameObject NewObj = Instantiate(DisasembleBuilding, new Vector2(wx * 10, wy * 10), Quaternion.identity);
        NewObj.transform.SetParent(transform);
        NewObj.SetActive(true);

        ObjectPlan OPSc = NewObj.GetComponent<ObjectPlan>();
        OPSc.objName = ObjToDisasemble;
        OPSc.planType = ObjectPlanType.Disasemble;
        OPSc.startRoadPoint = new Vector2Int(wx, wy);
        OPSc.endRoadPoint = new Vector2Int(wx, wy);

        NewObj.name = string.Format("{0}({1}, {2})", Obj.BuildingUnderDemolition, wx, wy);
        WorldMenager.instance.squares[wx, wy] = Obj.BuildingUnderDemolition;
        WorldGrid.SetSquare(new Square(Obj.BuildingUnderDemolition, wx, wy, 1, NewObj.transform));
        OPSc.keptItems = items;

        DronControler.instance.AddOPToList(OPSc);

        SpriteRenderer image = NewObj.transform.Find("Image").GetComponent<SpriteRenderer>();
        if (ObjToDisasemble == Obj.StoneOre || ObjToDisasemble == Obj.CopperOre || ObjToDisasemble == Obj.IronOre || ObjToDisasemble == Obj.CoalOre)
        { image.enabled = false; }
        else
        {
            Sprite sprite = ImageLibrary.instance.GetObjImages(ObjToDisasemble);
            if (sprite == null) { image.enabled = false; }
            else { image.sprite = sprite; image.enabled = true; }
        }

        //act stats
        TaskManager.instance.ActBuilding(Obj.BuildingUnderDemolition, 1);

        //fix items
        if (items != null)
        {
            ItemRAQ item;
            List<ItemRAQ> newItems = new List<ItemRAQ>();
            for (int i = 0; i < items.Count; i++)
            {
                item = items[i];
                newItems.Add(new ItemRAQ(item.res, item.qua));
            }
            BuildQueue.Add(new ObjToBuild(ObjToDisasemble, ObjectPlanType.Disasemble, wx, wy, newItems, NewObj.transform));
        }

        return NewObj.transform;
    }
    public Transform CreateConPlan(Obj ObjTB, int sx, int sy, int ex, int ey, List<ItemRAQ> items)
    {
        GameObject NewObj = Instantiate(RoadBorder, new Vector2(sx * 10, sy * 10), Quaternion.identity);
        Transform SPB = WorldMenager.instance.GetTransforOfObj(sx, sy);

        Vector2Int EndRoadPos = new Vector2Int(ex, ey);
        Vector2Int StartRoadPos = new Vector2Int(sx, sy);
        Vector2 relatve = EndRoadPos - StartRoadPos;
        Vector2 MidPointVector = relatve / 2 * 10;
        float angle = Mathf.Atan2(relatve.y, relatve.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

        float RoadLenght = (relatve.magnitude - 1) * 2;
        int ik = (int)(RoadLenght / 0.2f) + 1;
        RoadLenght = ik * 0.2f;

        NewObj.transform.SetParent(SPB);
        
        NewObj.SetActive(true);
        NewObj.transform.localRotation = q;
        NewObj.transform.localPosition = MidPointVector;
        NewObj.GetComponent<SpriteRenderer>().size = new Vector2(RoadLenght, 0.2f);

        ObjectPlan OPSc = NewObj.GetComponent<ObjectPlan>();
        OPSc.objName = ObjTB;
        OPSc.startRoadPoint = StartRoadPos;
        OPSc.endRoadPoint = EndRoadPos;
        List<ItemRAQ> NeedICopy = items;
        if (NeedICopy == null) { NeedICopy = new List<ItemRAQ>(); }
        int calcMultiplier = CalcConItemMultiplayer(ObjTB, StartRoadPos, EndRoadPos);
        for (int i = 0; i < NeedICopy.Count; i++) { NeedICopy[i].qua *= calcMultiplier; }

        DronControler.instance.AddOPToList(OPSc);

        if (ObjTB==Obj.ConUnderDemolition)
        {
            NewObj.name = string.Format("{0}({1}, {2})({3}, {4})", Obj.ConUnderDemolition, sx, sy, ex, ey);
            OPSc.keptItems = NeedICopy;
            OPSc.planType = ObjectPlanType.Disasemble;

            //act stats
            TaskManager.instance.ActBuilding(Obj.ConUnderDemolition, 1);
        }
        else
        {
            NewObj.name = string.Format("{0}({1}, {2})({3}, {4})", Obj.ConUnderConstruction, sx, sy, ex, ey);
            OPSc.needItems = NeedICopy;
            OPSc.planType = ObjectPlanType.Building;

            //act stats
            TaskManager.instance.ActBuilding(Obj.ConUnderConstruction, 1);
        }

        PlatformBehavior EPBSc = WorldMenager.instance.GetTransforOfObj(ex, ey).GetComponent<PlatformBehavior>();
        EPBSc.roadBorderListIn.Add(OPSc);

        //set squares under road
        SetSquaresUnderConnection(sx, sy, ex, ey, true);

        return NewObj.transform;
    }

    private int CalcConItemMultiplayer(Obj conType, Vector2Int EndRoadPos, Vector2Int StartRoadPos)
    {
        float multiPM = 0.2f;
        ConnectionRecipe recipe = AllRecipes.instance.GetConnectionRecipe(conType);
        if (recipe != null) { multiPM = recipe.iTLMultiplayer; }
        Vector2 relatve = EndRoadPos - StartRoadPos;
        float RoadLenght = (relatve.magnitude - 1) * 2;
        int multiplayer = (int)(RoadLenght * multiPM) + 1;
        //Debug.Log(multiPM + "*" + RoadLenght + "=" + multiplayer);
        return multiplayer;
    }
    private void SetSquaresUnderConnection(int sx, int sy, int ex, int ey, bool add)
    {
        Vector3[] tab = WorldMenager.instance.GetDisToPlaceofLine(sx, sy, ex, ey);
        float platRange = WorldMenager.instance.platformCheckingRadius;
        for (int i = 0; i < tab.Length; i++)
        {
            int ix = (int)tab[i].x;
            int iy = (int)tab[i].y;

            //Debug.Log(string.Format("set square x: {0} y: {1} (odl: {2}) at occupited by connection", ix, iy, tab[i].z));

            Obj iobj = WorldMenager.instance.GetSquer(ix, iy);

            if (iobj == Obj.None)
            {
                if (tab[i].z <= platRange)
                {
                    WorldMenager.instance.squares[ix, iy] = Obj.Connection1;
                    if (add && WorldMenager.instance.loadingWorld == false) { WorldMenager.instance.squeresVeribal[ix, iy] = 1; }
                }
            }
            else if(iobj == Obj.Connection1)
            {
                if (tab[i].z <= platRange)
                {
                    WorldMenager.instance.squares[ix, iy] = Obj.Connection1;
                    if (add && WorldMenager.instance.loadingWorld == false) { WorldMenager.instance.squeresVeribal[ix, iy] += 1; }
                }
            }
        }
    }

    public void BuildObj(Obj ObjTB, int wx, int wy, List<ItemRAQ> NeededItems, Vector2Int startPointCon)
    {
        if(wx < 0 || wx >= WorldMenager.instance.mapSize.x || wy < 0 || wy >= WorldMenager.instance.mapSize.y) { Debug.Log("ERROR! Cant build object out of map. Ob: " + ObjTB + " x:" + wx + " y: " + wy); return; }

        if (ObjTB == Obj.BuildingUnderConstruction || ObjTB == Obj.ConUnderConstruction || ObjTB == Obj.BuildingUnderDemolition)
        {
            Debug.Log("Error! Cant build: " + ObjTB + " because this function does not support this. Use another function (Create ....)");
            return;
        }

        if (ObjTB == Obj.ConUnderDemolition)
        {
            List<ItemRAQ> tl = new List<ItemRAQ>();
            ItemRAQ iir;
            for (int i = 0; i < NeededItems.Count; i++)
            {
                iir = NeededItems[i];
                tl.Add(new ItemRAQ(iir.res, iir.qua));
            }
            Transform trans = CreateConPlan(ObjTB, startPointCon.x, startPointCon.y, wx, wy, tl);

            int mul = CalcConItemMultiplayer(ObjTB, new Vector2Int(wx, wy), startPointCon);
            for (int i = 0; i < NeededItems.Count; i++)
            {
                NeededItems[i].qua *= mul;
            }
            ObjToBuild OTB = new ObjToBuild(ObjTB, ObjectPlanType.Disasemble, wx, wy, NeededItems, trans);
            OTB.startPointRoadsX = startPointCon.x;
            OTB.startPointRoadsY = startPointCon.y;
            BuildQueue.Add(OTB);
            return;
        }

        if (ObjTB == Obj.Connection1 || ObjTB == Obj.Connection2 || ObjTB == Obj.Connection3)
        {
            BuildConnection(ObjTB, startPointCon.x, startPointCon.y, wx, wy);
            return;
        }

        //remove object plan
        Transform ObjectPlan = WorldMenager.instance.GetTransforOfObj(wx, wy);
        if (ObjectPlan != null)
        {
            if (ObjectPlan.TryGetComponent(out ObjectPlan OPSc)) { DronControler.instance.RemOPFromList(OPSc); }
            Destroy(ObjectPlan.gameObject);
            TaskManager.instance.ActBuilding(Obj.BuildingUnderConstruction, -1);
        }

        //close gui
        if (GuiControler.instance.useObj == Obj.BuildingUnderConstruction && GuiControler.instance.useX == wx && GuiControler.instance.useY == wy)
        { GuiControler.instance.CloseNowOpenGui(); }

        //act stats
        TaskManager.instance.ActBuilding(ObjTB, 1);

        //dif objects
        if (ObjTB == Obj.Sapling) { TerrainManager.instance.SpawnSapling(wx, wy, true); return; }

        //create
        GameObject GO = ObjToGO(ObjTB);
        if (GO == null) { Debug.Log("ERROR! Trying build(" + ObjTB + ") but missing prefab"); return; }
        GameObject NewObj = Instantiate(GO, new Vector2(wx * 10, wy * 10), Quaternion.identity);
        NewObj.transform.parent = transform;
        NewObj.name = string.Format("{0}({1}, {2})", ObjTB, wx, wy);

        WorldMenager.instance.squares[wx, wy] = ObjTB;
        WorldGrid.SetSquare(new Square(ObjTB, wx, wy, AllRecipes.instance.GetMaxHelthOfObj(ObjTB), NewObj.transform));

        //add as avalibe platform & creata recipe image
        if (NewObj.tag == WorldMenager.instance.TagEnumToString(TagsEnum.Platform))
        {
            PlatformBehavior PBSc = NewObj.GetComponent<PlatformBehavior>();
            if (PBSc != null) { DronControler.instance.AddPBToList(PBSc); }

            //Recipe image
            if (AllRecipes.instance.objectWithCraftRecipes.Contains(ObjTB))
            { PBSc.SpownImage(); }
        }

        //wall
        if (ObjTB == Obj.Wall0 || ObjTB == Obj.Wall1 || ObjTB == Obj.Wall2 || ObjTB == Obj.Wall3)
        {
            BuildWallConnector(ObjTB);
        }

        //turrets
        if(ObjTB == Obj.Ballista || ObjTB == Obj.GunTurret || ObjTB == Obj.LaserTurret || ObjTB == Obj.RocketTurret)
        {
            Turret turrSc = NewObj.GetComponent<Turret>();
            if (turrSc != null)
            {
                WorldMenager.instance.TurretPos.Add(new Vector3Int(wx, wy, turrSc.rangeInt));
            }
        }

        void BuildConnection(Obj type, int sx, int sy, int ex, int ey)
        {
            if (!(type == Obj.Connection1 || type == Obj.Connection2 || type == Obj.Connection3)) { Debug.Log("ERROR! " + type + " isn't connection"); return; }

            if (WorldMenager.instance.loadingWorld == false)
            {
                //remove connection border
                Transform PlatformT = WorldMenager.instance.GetTransforOfObj(sx, sy);
                string name = string.Format("{0}({1}, {2})({3}, {4})", Obj.ConUnderConstruction, sx, sy, ex, ey);
                Transform ConPlanT = PlatformT.Find(name);
                if (ConPlanT != null)
                {
                    ObjectPlan OPSc = ConPlanT.GetComponent<ObjectPlan>();
                    WorldMenager.instance.GetTransforOfObj(ex, ey).GetComponent<PlatformBehavior>().roadBorderListIn.Remove(OPSc);
                    DronControler.instance.RemOPFromList(OPSc);
                    Destroy(ConPlanT.gameObject);
                    TaskManager.instance.ActBuilding(Obj.BuildingUnderConstruction, -1);
                }
                if (GuiControler.instance.useObj == Obj.ConUnderConstruction && GuiControler.instance.useX == ex && GuiControler.instance.useY == ey && GuiControler.instance.startPointRoad == new Vector2Int(sx, sy))
                {
                    GuiControler.instance.CloseNowOpenGui();
                    ClickMenager.instance.HidePointers(true);
                } 
            }

            //build connection
            GameObject NewCon = Instantiate(ObjToGO(type), transform.position, Quaternion.identity);
            NewCon.transform.parent = WorldMenager.instance.GetTransforOfObj(sx, sy);
            NewCon.name = string.Format("{0}({1}, {2})({3}, {4})", type, sx, sy, ex, ey);

            Vector2 EndRoadPos = new Vector2(ex, ey);
            Vector2 StartRoadPos = new Vector2(sx, sy);

            Vector2 relatve = EndRoadPos - StartRoadPos;

            Vector2 MidPointVector = relatve / 2 * 10;
            float angle = Mathf.Atan2(relatve.y, relatve.x) * Mathf.Rad2Deg - 180;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

            NewCon.transform.localRotation = q;
            NewCon.transform.localPosition = MidPointVector;

            float RoadLenght = (relatve.magnitude - 1);
            SpriteRenderer ConSR = NewCon.transform.Find("Main").GetComponent<SpriteRenderer>();
            ConSR.size = new Vector2(RoadLenght * 10f + 2f, ConSR.size.y);

            NewCon.transform.Find("Start").Translate((RoadLenght - 1f) * 5f + 1f, 0, 0);
            NewCon.transform.Find("End").Translate(-(RoadLenght - 1f) * 5f - 1f, 0, 0);


            RoadBehavior RBSc = NewCon.GetComponent<RoadBehavior>();
            PlatformBehavior SPBSc = WorldMenager.instance.GetTransforOfObj(sx, sy).GetComponent<PlatformBehavior>();
            PlatformBehavior EPBSc = WorldMenager.instance.GetTransforOfObj(ex, ey).GetComponent<PlatformBehavior>();

            RBSc.SetEPBSc(EPBSc);
            RBSc.type = type;
            RBSc.startRoadPoint = new Vector2Int(sx, sy);
            RBSc.endRoadPoint = new Vector2Int(ex, ey);

            EPBSc.roadListIn.Add(RBSc);
            SPBSc.roadListOut.Add(RBSc);
            SPBSc.CreatePriorityTab();

            //act stats
            TaskManager.instance.ActBuilding(type, 1);

            //set squares under road
            SetSquaresUnderConnection(sx, sy, ex, ey, false);
        }

        void BuildWallConnector(Obj type)
        {
            TASWallCon( 0,  1);
            TASWallCon( 0, -1);
            TASWallCon( 1,  0);
            TASWallCon(-1,  0);

            void TASWallCon(int px, int py)
            {
                Obj wT = WorldMenager.instance.GetSquer(wx+px, wy+py);
                if(ObjTB!=Obj.Wall3 && (wT == Obj.Wall0 || wT == Obj.Wall1 || wT == Obj.Wall2) || wT == Obj.Wall3)
                {
                    if ((int)wT > (int)ObjTB) { wT = ObjTB; }
                    GameObject Connector = null;
                    switch (wT)
                    {
                        case Obj.Wall0: Connector = Wall0Connector; break;
                        case Obj.Wall1: Connector = Wall1Connector; break;
                        case Obj.Wall2: Connector = Wall2Connector; break;
                        case Obj.Wall3: Connector = Wall3Connector; break;
                    }
                    if (Connector == null) { Debug.Log("ERROR! Missing Prefab"); return; }

                    Connector = Instantiate(Connector, new Vector2(wx * 10 + 5 * px, wy * 10 + 5 * py), Quaternion.AngleAxis(90 * py, Vector3.forward));
                    Connector.transform.SetParent(NewObj.transform);
                    Connector.name = string.Format("Connector({0}, {1})", wx+px, wy+py);
                }
            }
        }
    }

    //diffrent
    public ObjToBuild GetOTB(int i)
    {
        if (BuildQueue.Count <= i) { return null; }
        return BuildQueue[i];
    }
    public ObjToBuild[] GetAllOTB()
    {
        ObjToBuild[] otbT = new ObjToBuild[BuildQueue.Count];
        for (int i = 0; i < BuildQueue.Count; i++)
        {
            otbT[i] = BuildQueue[i];
        }
        return otbT;
    }
    
    private GameObject ObjToGO(Obj obj)
    {
        switch (obj)
        {
            case Obj.Connection1: return Connection1;
            case Obj.Connection2: return Connection2;
            case Obj.Connection3: return Connection3;

            case Obj.Warehouse1: return Warehouse;
            case Obj.Warehouse2: return Warehouse2;
            case Obj.Woodcuter: return WoodCuter;
            case Obj.Quarry: return Quarry;
            case Obj.Planter: return Planter;
            case Obj.Smelter: return Smelter;
            case Obj.Pulverizer: return Crusher;
            case Obj.Pump: return Pump;
            case Obj.Farm: return Farm;
            case Obj.Junkyard: return ScrapHeap;

            case Obj.Ballista: return Balista;
            case Obj.GunTurret: return GunTurret;
            case Obj.LaserTurret: return LaserTurret;
            case Obj.RocketTurret: return RocketTurret;

            case Obj.Wall0: return Wall0;
            case Obj.Wall1: return Wall1;
            case Obj.Wall2: return Wall2;
            case Obj.Wall3: return Wall3;

            case Obj.BasicCrafter: return BasicCrafter;
            case Obj.Crafter: return Crafter;
            case Obj.ChemicalPlant: return ChemicalPlant;
            case Obj.DroneStation: return DronStation;

            case Obj.Connector: return Connector;
            case Obj.FastConnector: return FastConnector;

            case Obj.TransmissionTower: return TransmissionTower;
            case Obj.WindTurbine1: return WindTurbine1;
            case Obj.WindTurbine2: return WindTurbine2;
            case Obj.Battery: return Battery1;
            case Obj.CombustionGenerator: return CombustionGenerator;
            case Obj.SteamGenerator: return SteemGenerator;
            case Obj.SolarPanel1: return SolarPanel1;

            case Obj.ElectricSmelter: return ElectricSmelter;
            case Obj.Repairer: return Repairer;

            case Obj.Launchpad: return Launchpad;
            case Obj.SpaceRequester: return SpaceRequester;
        }
        Debug.Log("missing prefab of: " + obj);
        return null;
    }

    //removing smt
    public void RemoveBuildingPlan(int x, int y)
    {
        Transform ObjT = WorldMenager.instance.GetTransforOfObj(x, y);
        if (ObjT == null) { return; }
        ObjectPlan OPSc = ObjT.GetComponent<ObjectPlan>();
        if (OPSc == null) { return; }

        Obj objName = OPSc.objName;

        DronControler.instance.RemOPFromList(OPSc);

        //remove from buildQue
        for (int i = 0; i < BuildQueue.Count; i++)
        {
            if (BuildQueue[i].objectType == objName)
            {
                if (BuildQueue[i].xTB == x && BuildQueue[i].yTB == y)
                {
                    BuildQueue.RemoveAt(i);
                    break;
                }
            }
        }

        //return dron
        foreach (DronBehavior drone in DronControler.instance.AllDrons)
        {
            if (drone.transToPutDownItem == ObjT || drone.transToPutUpItem == ObjT) { drone.SetToReturn(); }
        }
        
        //destroy border
        WorldMenager.instance.RemoveObjFromPos(x, y);
    }
    public void RemoveConnectionPlan(Vector2Int startPointRoad, Vector2Int endPintRoad)
    {
        int sx = startPointRoad.x; int sy = startPointRoad.y;
        int ex = endPintRoad.x; int ey = endPintRoad.y;

        string roadName;
        Transform PlatformT = WorldMenager.instance.GetTransforOfObj(sx, sy);
        if (PlatformT == null) { return; }

        roadName = string.Format("{0}({1}, {2})({3}, {4})", Obj.ConUnderConstruction, sx, sy, ex, ey);
        Transform roadT = PlatformT.Find(roadName);
        if (roadT == null) 
        {
            roadName = string.Format("{0}({1}, {2})({3}, {4})", Obj.ConUnderDemolition, sx, sy, ex, ey);
            roadT = PlatformT.Find(roadName);
            if (roadT == null) { return; }
        }

        ObjectPlan OPSc = roadT.GetComponent<ObjectPlan>();
        WorldMenager.instance.GetTransforOfObj(ex, ey).GetComponent<PlatformBehavior>().roadBorderListIn.Remove(OPSc);

        Obj objName = OPSc.objName;

        DronControler.instance.RemOPFromList(OPSc);

        //remove from buildQue
        for (int i = 0; i < BuildQueue.Count; i++)
        {
            if (BuildQueue[i].objectType == objName)
            {
                if (BuildQueue[i].xTB == ex && BuildQueue[i].yTB == ey && BuildQueue[i].startPointRoadsX == startPointRoad.x && BuildQueue[i].startPointRoadsY == startPointRoad.y)
                { BuildQueue.RemoveAt(i); break; }
            }
        }

        //return dron
        foreach (DronBehavior drone in DronControler.instance.AllDrons)
        {
            if(drone.transToPutDownItem == roadT || drone.transToPutUpItem == roadT) { drone.SetToReturn(); }
        }
        
        //set squares under road
        Vector3[] tab = WorldMenager.instance.GetDisToPlaceofLine(sx, sy, ex, ey);
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

        //destroy border
        Destroy(roadT.gameObject, 0.1f);
    }

    public int GetIndexOfBuildQue(Obj objectType, int xTB, int yTB, Vector2Int startPointCon)
    {
        if (objectType == Obj.Connection1 || objectType == Obj.Connection2 || objectType == Obj.Connection3 || objectType == Obj.ConUnderConstruction || objectType == Obj.ConUnderDemolition)
        {
            for (int i = 0; i < BuildQueue.Count; i++)
            {
                ObjToBuild OTB = BuildQueue[i];
                if (OTB.objectType == objectType)
                {
                    if (OTB.xTB == xTB && OTB.yTB == yTB && OTB.startPointRoadsX == startPointCon.x && OTB.startPointRoadsY == startPointCon.y)
                    { return i; }
                }
            }
        }
        else
        {
            for (int i = 0; i < BuildQueue.Count; i++)
            {
                ObjToBuild OTB = BuildQueue[i];
                if (OTB.objectType == objectType)
                {
                    if (OTB.xTB == xTB && OTB.yTB == yTB)
                    { return i; }
                }
            }
        }
        
        return -1;
    }
}
