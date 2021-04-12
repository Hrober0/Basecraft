using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class ClickMenager : MonoBehaviour
{
    public static ClickMenager instance;
    void Awake()
    {
        if (instance != null) { Debug.Log("more the one ClickMenager on scen"); return; }
        instance = this;
    }

    public enum ClickMode { Normal, BuildingObject, Connection, Removing, OtherActions }
    public enum ObjectOpction { conOn, removeOn, buildOn, eleOn, mineOn, cutOn, plantOn, addItemsOn }

    [Header("Obj to set")]
    [SerializeField] private GameObject Pointer = null;
    [SerializeField] private GameObject PlatformRange = null;
    [SerializeField] private GameObject ConnectionPointer = null;
    [SerializeField] private SpriteRenderer MoveingConnectionPointer = null;

    [Header("Opctions")]
    public bool CreateBacground = true;
    public bool DebugClickPoint = false;
    public bool EnemyAttackOn = true;
    public bool SpawnEnemyUnits = true;
    public bool RandomBasePos = true;
    public bool StartAmmoInTuret = false;

    [Header("Veribals")]
    public Vector2 ClickedPoint;
    private int x, y;
    private GameObject MyPointer;
    private Vector2 lCP;
    public Obj useObj = Obj.Locked;
    public Obj useTerrain;
    [SerializeField] private ClickMode clickMode = ClickMode.Normal;
    [SerializeField] private Color normalMCPColor = Color.white;
    [SerializeField] private Color errorMCPColor = Color.red;

    public bool selectedFirstBuildingToBuildCon = false;
    private Vector2Int startConPoint;
    private Vector2Int endPintRoad;
    private PlatformBehavior usePBSc;
    private RoadBehavior useRoadB;
    private bool isRed = false;

    private GameObject MyConnectionPointer;
    private Transform rangesParent;
    private List<GameObject> rangesList = new List<GameObject>();
    private int activeRangesCount;

    public int saveGameSpeed;

    private void Start()
    {
        MyPointer = Instantiate(Pointer, transform.parent);
        MyPointer.transform.SetParent(transform);
        MyPointer.transform.position = new Vector2(-5f, -5f);
        MyPointer.name = "Pointer";
        MyPointer.SetActive(false);

        MyConnectionPointer = Instantiate(ConnectionPointer, transform.parent);
        MyConnectionPointer.transform.SetParent(transform);
        MyConnectionPointer.name = "ConnectionPointer";
        MyConnectionPointer.SetActive(false);

        MoveingConnectionPointer.gameObject.SetActive(false);

        rangesParent = transform.Find("Ranges");

        useObj = Obj.Locked;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) { lCP = Camera.main.transform.position; }
        if (CameraControler.instance.usingMenu) { lCP.x = -1; }
        if (Input.GetMouseButtonUp(0)) { Click(); }

        if (selectedFirstBuildingToBuildCon)
        {
            Vector2 endConPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 startConPos = startConPoint * 10;
            Vector2 relatve = endConPos - startConPos;
            Vector2 midPointVector = relatve / 2f + startConPos;

            float angle = Mathf.Atan2(relatve.y, relatve.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

            MoveingConnectionPointer.transform.localRotation = q;
            MoveingConnectionPointer.transform.position = midPointVector;

            float conLenght = (relatve.magnitude) / 5f - 1f;
            if (conLenght < 0f) { conLenght = 0f; }
            MoveingConnectionPointer.size = new Vector2(conLenght, MoveingConnectionPointer.size.y);
        }
    }

    //Click
    private void Click()
    {
        ClickedPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int tx = Zaok(ClickedPoint.x);
        int ty = Zaok(ClickedPoint.y);

        //klikniecie na ui
        if (EventSystem.current.IsPointerOverGameObject()) { return; }

        //przeciagniecie
        Vector2 cCP = Camera.main.transform.position;
        if (cCP.x - 1 > lCP.x || cCP.x + 1 < lCP.x || cCP.y - 1 > lCP.y || cCP.y + 1 < lCP.y) { return; }
        
        //kliknieto poza mape
        if (tx < 0 || tx >= WorldMenager.instance.mapSize.x || ty < 0 || ty >= WorldMenager.instance.mapSize.y) { /*Debug.Log("Kliknięto poza mapę!");*/ return; }

        x = tx;
        y = ty;

        HidePointers(false);

        //debug clicked point
        if (DebugClickPoint) { Debug.Log("Clicked " + WorldMenager.instance.GetSquer(x, y) + "(" + x + ", " + y + ") on terrain: "+ WorldMenager.instance.GetTerrainTile(x,y)); }

        useObj = WorldMenager.instance.GetSquer(x, y);
        usePBSc = null;
        useTerrain = WorldMenager.instance.terrainTiles[x, y];

        switch (clickMode)
        {
            case ClickMode.Normal: NormalClick(); break;
            case ClickMode.BuildingObject: BuildObjectClick(); break;
            case ClickMode.Connection: ConnectionClick(); break;
            case ClickMode.Removing: RemovingClick(); break;
            case ClickMode.OtherActions: OtherActionClick(); break;
        }
    }
    public void SetClickMode(ClickMode clickM) { clickMode = clickM; }
    private void NormalClick()
    {
        //creativ, building
        if (SpaceBaseMainSc.instance.CreativeModeOn && CreativeUIController.instance != null)
        {
            if (!GuiControler.instance.IsNowOpenPanelsContains(CreativeUIController.instance.BuildGui)) { CreativeUIController.instance.selectobject = Obj.None; }
            else
            {
                Obj obj = CreativeUIController.instance.selectobject;
                if (obj != Obj.None)
                {
                    BuildObjectOnCreative(obj, x, y);
                    return;
                }
            }
        }

        //pointer
        SetMyPointer(new Vector2(x * 10, y * 10));

        //chose start place
        if (SceneLoader.instance.canChooseStartPlace) { GuiControler.instance.ShowChooseStartPlace(useObj); return; }

        // transform
        Transform useTrans = WorldMenager.instance.GetTransforOfObj(x, y);

        //set platform bahavior
        if (AllRecipes.instance.IsItPlatform(useObj) && useTrans.TryGetComponent(out PlatformBehavior tUsePBSc)) { usePBSc = tUsePBSc; }
        else { usePBSc = null; }

        //platform opction gui
        GuiControler.instance.ShowPlatformOpction(useObj, useTerrain, x, y, usePBSc);

        //show range
        if (useObj == Obj.DroneStation)
        {
            int range = DronControler.instance.DSRange;
            foreach (DronStation DS in DronControler.instance.AllDS)
            {
                ShowRange(DS.GetTabPos(), range);
            }
        }
        if (AllRecipes.instance.IsItTurret(useObj))
        {
            foreach (Vector3Int turr in WorldMenager.instance.TurretPos)
            {
                ShowRange(new Vector2Int(turr.x, turr.y), turr.z);
            }
        }
        else if (useObj == Obj.EnemyTurret)
        {
            int eTRange = EnemyControler.instance.eTurretRange + 1;
            foreach (EnemyBaseControler EBC in EnemyControler.instance.baseControlerList)
            {
                Vector2Int[] poss = EBC.GetAllPoss(Obj.EnemyTurret);
                foreach (Vector2Int pos in poss)
                {
                    ShowRange(new Vector2Int(pos.x, pos.y), eTRange);
                }
            }
        }
        else if (useTrans != null && useTrans.TryGetComponent(out TransmissionTower _))
        {
            int range = ElectricityManager.instance.tTRange;
            foreach (TransmissionTower tTSc in ElectricityManager.instance.AllTransTowers)
            {
                Vector2 pos = tTSc.transform.position;
                ShowRange(new Vector2Int((int)pos.x / 10, (int)pos.y / 10), range);
            }
        }
        else if (usePBSc != null)
        {
            if (usePBSc.range != -1)
            {
                //pokaz zasieng
                int range = usePBSc.range;
                ShowRange(new Vector2Int(x, y), range);
            }
        }
        
    }
    private void BuildObjectClick()
    {
        Obj ObjName = GuiControler.instance.BuildPanelSc.selectedObj;

        if (ObjName == Obj.None) { clickMode = ClickMode.Normal; }

        if (WorldMenager.instance.GetSquer(x, y) != Obj.None)
        {
            MessageManager.instance.ShowMessage(Messages.CantBuildItHere, (int)ObjName);
            GuiControler.instance.BuildPanelSc.ShowErrorOfPointer();
            return;
        }

        BuildingRecipe useBR = AllRecipes.instance.GetBuildRecipe(ObjName);
        if (useBR == null)
        {
            MessageManager.instance.ShowMessage(Messages.CantBuildItHere, (int)ObjName);
            GuiControler.instance.BuildPanelSc.ShowErrorOfPointer();
            return;
        }
        bool CanBuildIt = false;
        if (useBR.whereItCanBeBuild.Count != 0)
        {
            for (int i = 0; i < useBR.whereItCanBeBuild.Count; i++)
            { if (useBR.whereItCanBeBuild[i] == WorldMenager.instance.terrainTiles[x, y]) { CanBuildIt = true; } }
        }
        else
        { CanBuildIt = true; }

        if (CanBuildIt == false)
        {
            MessageManager.instance.ShowMessage(Messages.CantBuildItHere, (int)ObjName);
            GuiControler.instance.BuildPanelSc.ShowErrorOfPointer();
            return;
        }

        BuildMenager.instance.AddObjToBuild(ObjName, x, y, new Vector2Int());
    }
    private void ConnectionClick()
    {
        //if desnt click on platform
        if (AllRecipes.instance.IsItPlatform(useObj) == false)
        {
            MessageManager.instance.ShowMessage(Messages.CantConnectThisObj);
            ShowErrorOfBuildingPointer();
            return;
        }

        //if desnt selected first platform
        if (selectedFirstBuildingToBuildCon == false)
        { 
            startConPoint = new Vector2Int(x, y);
            GuiControler.instance.BuildConPanelSc.UpdateDoText();
            selectedFirstBuildingToBuildCon = true;
            MoveingConnectionPointer.gameObject.SetActive(true);
            return;
        }

        Transform clickedTra = WorldMenager.instance.GetTransforOfObj(x, y);
        usePBSc = clickedTra.GetComponent<PlatformBehavior>();

        //if cant connect this platform
        if (usePBSc.canBeConectedIn == false && usePBSc.IsConnection(startConPoint.x, startConPoint.y) == Obj.None)
        {
            MessageManager.instance.ShowMessage(Messages.CantConnectThisObj);
            ShowErrorOfBuildingPointer();
            return;
        }

        //if clicked the same platform
        if (startConPoint == new Vector2(x, y))
        {
            selectedFirstBuildingToBuildCon = false;
            GuiControler.instance.BuildConPanelSc.UpdateDoText();
            MoveingConnectionPointer.gameObject.SetActive(false);
            return;
        }

        //select connectio
        int sx = x; int sy = y;
        int ex = startConPoint.x; int ey = startConPoint.y;
        Obj selectConType = usePBSc.IsConnection(ex, ey);

        //check other side connection if does not found
        if (selectConType == Obj.None)
        {
            sx = startConPoint.x; sy = startConPoint.y;
            ex = x; ey = y;
            selectConType = WorldMenager.instance.GetTransforOfObj(sx, sy).GetComponent<PlatformBehavior>().IsConnection(ex, ey);
        }
        
        Obj buildingConnType = GuiControler.instance.BuildConPanelSc.SelectedObj;

        //open connection panel
        if (selectConType != Obj.None)
        {
            if (buildingConnType != Obj.None)
            { MessageManager.instance.ShowMessage(Messages.ConnectionIsAlreadyBuilt); return; }

            startConPoint = new Vector2Int(sx, sy);
            endPintRoad = new Vector2Int(ex, ey);
            useObj = selectConType;

            SetMyConnectionPointer(startConPoint, endPintRoad);

            if (selectConType == Obj.ConUnderConstruction || selectConType == Obj.ConUnderDemolition)
            { GuiControler.instance.ShowConnectionGui(selectConType, false, startConPoint, endPintRoad); return; }

            string roadName = string.Format("{0}({1}, {2})({3}, {4})", selectConType, sx, sy, ex, ey);
            Transform roadT = WorldMenager.instance.GetTransforOfObj(sx, sy).Find(roadName);
            if (roadT == null) { Debug.LogError("Error! Missing road"); return; }
            useRoadB = roadT.GetComponent<RoadBehavior>();
            GuiControler.instance.ShowConnectionGui(selectConType, !useRoadB.sendOff, startConPoint, endPintRoad);
            return;
        }

        if (buildingConnType == Obj.None)
        {
            MessageManager.instance.ShowMessage(Messages.DoesntSelectConToBuild);
            ShowErrorOfBuildingPointer();
            return;
        }

        //spr building on road
        Vector3[] tab = WorldMenager.instance.GetDisToPlaceofLine(sx, sy, ex, ey);
        float platRange = WorldMenager.instance.platformCheckingRadius;
        for (int i = 0; i < tab.Length; i++)
        {
            int ix = (int)tab[i].x;
            int iy = (int)tab[i].y;

            Obj iobj = WorldMenager.instance.GetSquer(ix, iy);
            if (iobj != Obj.None && iobj != Obj.Connection1 && AllRecipes.instance.IsItTerrain(iobj) == false)
            {
                if (tab[i].z <= platRange)
                {
                    Debug.Log(string.Format("building({0}) on x: {1} y: {2}, is too close connection (odl: {3})", iobj, ix, iy, tab[i].z));
                    MessageManager.instance.ShowMessage(Messages.CantBuildConnectionThroughObj);
                    ShowErrorOfBuildingPointer();
                    return;
                }
            }
            else if (iobj == Obj.Mountain)
            {
                if (tab[i].z <= 0.4f)
                {
                    Debug.Log(string.Format("building({0}) on x: {1} y: {2}, is too close connection (odl: {3})", iobj, ix, iy, tab[i].z));
                    MessageManager.instance.ShowMessage(Messages.CantBuildConnectionThroughObj);
                    ShowErrorOfBuildingPointer();
                    return;
                }
            }
        }

        //build
        MyConnectionPointer.SetActive(false);
        BuildMenager.instance.AddObjToBuild(buildingConnType, x, y, startConPoint);
        selectedFirstBuildingToBuildCon = false;
        MoveingConnectionPointer.gameObject.SetActive(false);

        void ShowErrorOfBuildingPointer()
        {
            if (isRed) { return; }
            isRed = true;
            DOTween.Sequence()
                .Append(MoveingConnectionPointer.DOColor(errorMCPColor, 0.15f))
                .Append(MoveingConnectionPointer.DOColor(normalMCPColor, 0.10f))
                .OnComplete(() => { isRed = false; })
            ;
        }
    }
    private void RemovingClick()
    {
        RemovingPanel.RemoveOpction ro = GuiControler.instance.RemovingPanelSc.removeOpction;
        if (ro == RemovingPanel.RemoveOpction.None) { return; }

        if (AllRecipes.instance.IsItPlatform(useObj) && WorldMenager.instance.GetTransforOfObj(x, y).TryGetComponent(out PlatformBehavior PBSc)) { usePBSc = PBSc; }
        bool[] opction = GetObjectOpction(useObj, useTerrain, x, y, usePBSc);
        switch (ro)
        {
            case RemovingPanel.RemoveOpction.Demolition:
                if (opction[(int)ObjectOpction.removeOn]) { DemolitionObj(); break; }
                GuiControler.instance.RemovingPanelSc.ShowErrorOfPointer();
                MessageManager.instance.ShowMessage(Messages.CantDemolitionObj);
                break;
            case RemovingPanel.RemoveOpction.Disasemble:
                if (opction[(int)ObjectOpction.removeOn]) { DisasembleObj(); break; }
                GuiControler.instance.RemovingPanelSc.ShowErrorOfPointer();
                MessageManager.instance.ShowMessage(Messages.CantDisasembleObj);
                break;
        }
    }
    private void OtherActionClick()
    {
        ActionPanel.ActionOpction ao = GuiControler.instance.ActionPanelSc.selectedOpction;
        if (ao == ActionPanel.ActionOpction.None) { return; }

        switch (ao)
        {
            case ActionPanel.ActionOpction.Mine:
                if (AllRecipes.instance.IsItOreObj(useTerrain))
                {
                    Res res = GetResOfOre(useTerrain);
                    if (res == Res.None) break;
                    BuildMenager.instance.AddMiningTask(useTerrain, res, x, y, 1);
                    break;
                }
                GuiControler.instance.ActionPanelSc.ShowErrorOfPointer();
                MessageManager.instance.ShowMessage(Messages.CantMineObj);
                break;
            case ActionPanel.ActionOpction.Plant:
                if (useObj == Obj.None && useTerrain == Obj.TerrainFertile) { BuildMenager.instance.AddObjToBuild(Obj.Sapling, x, y, new Vector2Int()); break; }
                GuiControler.instance.ActionPanelSc.ShowErrorOfPointer();
                MessageManager.instance.ShowMessage(Messages.CantPlantHere);
                break;
            case ActionPanel.ActionOpction.Cut:
                if (useObj == Obj.Tree || useObj == Obj.Sapling) { DisasembleObj(); break; }
                GuiControler.instance.ActionPanelSc.ShowErrorOfPointer();
                MessageManager.instance.ShowMessage(Messages.CantCutObj);
                break;
            case ActionPanel.ActionOpction.Repair:
                Debug.Log("TODO: repair");
                break;
        }
    }

    //other
    public void CancelSelectingConnection()
    {
        clickMode = ClickMode.Normal;
        MoveingConnectionPointer.gameObject.SetActive(false);
        selectedFirstBuildingToBuildCon = false;
    }
    private int Zaok(float a)
    {
        int b = (int)a-5;
        b /= 10;
        a /= 10;
        //Debug.Log(a+" "+b);
        if ((float)b < a) { b++; }
        if (((float)b - a) > 5f) { b--; }
        return b;
    }
    private void BuildObjectOnCreative(Obj obj, int x, int y)
    {
        //Debug.Log("Try build " + obj + " of page " + CreativeUIController.instance.useBuildPage + " at " + x + " " + y);
        if (WorldMenager.instance.GetSquer(x, y) != Obj.None) { return; }
        switch (CreativeUIController.instance.useBuildPage)
        {
            case 1:
                //Debug.Log("building");
                BuildMenager.instance.BuildObj(obj, x, y, null, new Vector2Int());
                break;
            case 2:
                //Debug.Log("terrain");
                Obj terr = WorldMenager.instance.GetTerrainTile(x, y);
                if (terr != Obj.None && obj != Obj.Mountain) { return; }

                switch (obj)
                {
                    case Obj.TerrainFertile:    TerrainManager.instance.SimpleSpawnTerrain(Obj.TerrainFertile, x, y); break;
                    case Obj.Tree:              TerrainManager.instance.SimpleSpawnTerrain(Obj.TerrainFertile, x, y); TerrainManager.instance.SpawnTree(x, y); break;
                    case Obj.Sapling:           TerrainManager.instance.SimpleSpawnTerrain(Obj.TerrainFertile, x, y); TerrainManager.instance.SpawnSapling(x, y, false); break;
                    case Obj.WaterSource:       TerrainManager.instance.SimpleSpawnTerrain(Obj.WaterSource, x, y); break;
                    case Obj.OilSource:         TerrainManager.instance.SimpleSpawnTerrain(Obj.OilSource, x, y); break;
                    case Obj.FarmlandFlax:      TerrainManager.instance.SpawnFarmland(x, y, true, 1); break;
                    case Obj.FarmlandGrape:     TerrainManager.instance.SpawnFarmland(x, y, true, 2); break;
                    case Obj.FarmlandRubber:    TerrainManager.instance.SpawnFarmland(x, y, true, 3); break;

                    case Obj.StoneOre:      TerrainManager.instance.SimpleSpawnTerrain(Obj.StoneOre, x, y); break;
                    case Obj.IronOre:       TerrainManager.instance.SimpleSpawnTerrain(Obj.IronOre, x, y); break;
                    case Obj.CopperOre:     TerrainManager.instance.SimpleSpawnTerrain(Obj.CopperOre, x, y);  break;

                    case Obj.Mountain: TerrainManager.instance.SpawnMountain(x, y, true); break;
                }
                break;
            case 3:
                //Debug.Log("enemy base");
                break;
        }
    }
    public bool[] GetObjectOpction(Obj obj, Obj terrain, int x, int y, PlatformBehavior PBSc)
    {
        int numOfObjectOpction = Enum.GetNames(typeof(ObjectOpction)).Length;
        bool[] opction = new bool[numOfObjectOpction];
        for (int i = 0; i < numOfObjectOpction; i++) { opction[i] = false; }

        if (AllRecipes.instance.IsUsingEnergy(obj)) { SetOpct(ObjectOpction.eleOn, true); }

        if (obj == Obj.Tree || obj == Obj.Sapling)
        {
            SetOpct(ObjectOpction.cutOn, true);
            if (SpaceBaseMainSc.instance.CreativeModeOn) { SetOpct(ObjectOpction.removeOn, true); }
        }

        if (obj == Obj.None)
        {
            SetOpct(ObjectOpction.buildOn, true);
            obj = terrain;
            if (AllRecipes.instance.IsItOreObj(terrain)) { SetOpct(ObjectOpction.mineOn, true); }
            else if (terrain == Obj.TerrainFertile) { SetOpct(ObjectOpction.plantOn, true); }

            if (terrain != Obj.None && SpaceBaseMainSc.instance.CreativeModeOn) { SetOpct(ObjectOpction.removeOn, true); }
        }
        else if (PBSc != null)
        {
            if (PBSc.canBeConnectedOut) { SetOpct(ObjectOpction.conOn, true); }
            SetOpct(ObjectOpction.removeOn, true);

            if (SpaceBaseMainSc.instance.CreativeModeOn) { SetOpct(ObjectOpction.addItemsOn, true); }
        }
        else if (obj == Obj.Mountain)
        {
            if (SpaceBaseMainSc.instance.CreativeModeOn) { SetOpct(ObjectOpction.removeOn, true); }
        }
        else if (obj == Obj.BuildingUnderConstruction) { SetOpct(ObjectOpction.removeOn, true); }
        else if (obj == Obj.BuildingUnderDemolition)
        {
            if (AllRecipes.instance.IsItOreObj(terrain)) { SetOpct(ObjectOpction.mineOn, true); }

            SetOpct(ObjectOpction.removeOn, true);
            checkIfCantRemove();

            void checkIfCantRemove()
            {
                if (SpaceBaseMainSc.instance.CreativeModeOn) { return; }
                Transform uT = WorldMenager.instance.GetTransforOfObj(x, y);
                if (uT == null) { return; }
                ObjectPlan oP = uT.GetComponent<ObjectPlan>();
                if (oP == null) { return; }
                Obj disasemblingObj = oP.objName;
                if (disasemblingObj == Obj.Tree || disasemblingObj == Obj.Sapling) { SetOpct(ObjectOpction.removeOn, false); }
            }
        }
        else if (AllRecipes.instance.IsItWall(obj)) { SetOpct(ObjectOpction.removeOn, true); }
        else if (obj == Obj.TransmissionTower || obj == Obj.WindTurbine1 || obj == Obj.WindTurbine2 || obj == Obj.SolarPanel1) { SetOpct(ObjectOpction.removeOn, true); }
        else if (obj == Obj.Connection1) { /*occupited square by connection*/ }

        if (obj == Obj.DroneStation && DronControler.instance.AllDS.Count <= 1) { SetOpct(ObjectOpction.removeOn, false); }

        return opction;

        void SetOpct(ObjectOpction objectOpction, bool value) { opction[(int)objectOpction] = value; }
    }
    private void DisasembleObj()
    {
        if (useObj == Obj.BuildingUnderConstruction)
        {
            Transform TT = WorldMenager.instance.GetTransforOfObj(x, y);
            if (TT == null) { Debug.Log("ERROR! cant find obj on: " + x + ", " + y); return; }
            ObjectPlan BI = TT.GetComponent<ObjectPlan>();
            List<ItemRAQ> IIR = new List<ItemRAQ>();
            for (int i = 0; i < BI.keptItems.Count; i++)
            {
                IIR.Add(new ItemRAQ(BI.keptItems[i].res, BI.keptItems[i].qua));
            }
            Obj objToDis = BI.objName;

            BuildMenager.instance.RemoveBuildingPlan(x, y);
            if (IIR.Count > 0) { BuildMenager.instance.CreateDisasembleObject(objToDis, x, y, IIR); }
        }
        else if (useObj == Obj.Tree)
        {
            BuildMenager.instance.AddCuttingTreeTask(x, y);
        }
        else if (useObj == Obj.Sapling)
        {
            WorldMenager.instance.RemoveObjFromPos(x, y);
            List<ItemRAQ> IIR = new List<ItemRAQ> { new ItemRAQ(Res.Sapling, 1) };
            BuildMenager.instance.CreateDisasembleObject(useObj, x, y, IIR);
        }
        else if (usePBSc != null)
        {
            List<ItemRAQ> IIR = AllRecipes.instance.GetNeedItems(useObj);
            if (AllRecipes.instance.IsItPlatform(useObj))
            {
                for (int i = 0; i < usePBSc.avalibleRes.Count; i++)
                {
                    Res res = usePBSc.avalibleRes[i];
                    IIR.Add(new ItemRAQ(res, usePBSc.itemOnPlatform[(int)res].qua));
                }
            }
            usePBSc.RemovePlatform();
            if (IIR.Count > 0) { BuildMenager.instance.CreateDisasembleObject(useObj, x, y, IIR); }
        }
        else if (useObj == Obj.None)
        {
            Obj tObj = WorldMenager.instance.GetTerrainTile(x, y);
            if (AllRecipes.instance.IsItTerrain(tObj)) { TerrainManager.instance.RemoveTerrain(tObj, x, y); }
        }
        else
        {
            WorldMenager.instance.RemoveObjFromPos(x, y);
            List<ItemRAQ> IIR = AllRecipes.instance.GetNeedItems(useObj);
            if (IIR != null && IIR.Count > 0) { BuildMenager.instance.CreateDisasembleObject(useObj, x, y, IIR); }
        }
    }
    private void DemolitionObj()
    {
        if (useObj == Obj.BuildingUnderConstruction)        { BuildMenager.instance.RemoveBuildingPlan(x, y); }
        else if (useObj == Obj.BuildingUnderDemolition)     { BuildMenager.instance.RemoveBuildingPlan(x, y); }
        else if (usePBSc != null)                           { usePBSc.RemovePlatform(); }
        else if (useObj == Obj.None)
        {
            Obj tObj = WorldMenager.instance.GetTerrainTile(x, y);
            if (AllRecipes.instance.IsItTerrain(tObj)) { TerrainManager.instance.RemoveTerrain(tObj, x, y); }
        }
        else                                                { WorldMenager.instance.RemoveObjFromPos(x, y); }
    }
    private Res GetResOfOre(Obj useTerrain)
    {
        if (useTerrain == Obj.StoneOre)         { return Res.StoneOre; }
        else if (useTerrain == Obj.CopperOre)   { return Res.CopperOreCtm; }
        else if (useTerrain == Obj.IronOre)     { return Res.IronOre; }
        else if (useTerrain == Obj.CoalOre)     { return Res.Coal; }
        return Res.None;
    }

    
    //pointers
    private void SetMyPointer(Vector2 pos)
    {
        MyPointer.transform.position = pos;
        MyPointer.SetActive(true);
    }
    private void SetMyConnectionPointer(Vector2 StartRoadPos, Vector2 EndRoadPos)
    {
        Vector2 relatve = EndRoadPos - StartRoadPos;

        Vector2 MidPointVector = (relatve / 2 + startConPoint) * 10;
        float angle = Mathf.Atan2(relatve.y, relatve.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

        MyConnectionPointer.transform.rotation = q;
        MyConnectionPointer.transform.position = MidPointVector;

        float RoadLenght = (relatve.magnitude - 1) * 2;
        int ik = (int)(RoadLenght / 0.2f) + 1;
        RoadLenght = ik * 0.2f;
        MyConnectionPointer.GetComponent<SpriteRenderer>().size = new Vector2(RoadLenght, 0.3f);

        MyConnectionPointer.SetActive(true);
    }
    private void ShowRange(Vector2Int pos, int range)
    {
        if (activeRangesCount >= rangesList.Count) { SpownNewRange(); }
        if (activeRangesCount >= rangesList.Count) { return; }

        GameObject rangeGO = rangesList[activeRangesCount];
        activeRangesCount++;

        //float rangeF = (range + 1) * 2f;

        rangeGO.transform.position = new Vector2(pos.x * 10, pos.y * 10);
        rangeGO.transform.localScale = new Vector3(range * 2, range * 2, 0);
        rangeGO.SetActive(true);

        void SpownNewRange()
        {
            GameObject newRange = Instantiate(PlatformRange, transform.parent);
            newRange.transform.SetParent(rangesParent);
            newRange.name = "Range";
            newRange.SetActive(false);
            rangesList.Add(newRange);
        }
    }
    public void HidePointers(bool disableAllPointerOperation)
    {
        if (disableAllPointerOperation)
        {
            if (GuiControler.instance.BuildPanelSc.gameObject.activeSelf) { GuiControler.instance.BuildPanelSc.CancelBuilding(); }
            selectedFirstBuildingToBuildCon = false;
            MoveingConnectionPointer.gameObject.SetActive(false);
        }
        MyPointer.SetActive(false);
        MyConnectionPointer.SetActive(false);
        GuiControler.instance.HideWallC();

        foreach (GameObject item in rangesList) { item.SetActive(false); }
        activeRangesCount = 0;
    }

    //GameSettingsPanel
    public void ClickSetGameTimeOn0() => WorldMenager.instance.SetGameSpeed(0);
    public void ClickSetGameTimeOn1() => WorldMenager.instance.SetGameSpeed(1);
    public void ClickSetGameTimeOn2() => WorldMenager.instance.SetGameSpeed(2);
    public void ClickShowPauseMenu()
    {
        saveGameSpeed = WorldMenager.instance.GameSpeed;
        WorldMenager.instance.SetGameSpeed(0);
        GuiControler.instance.ShowPauseMenu();
    }

    //platform opction
    public void ClickShowBuildGui()
    {
        if (SpaceBaseMainSc.instance.CreativeModeOn && CreativeUIController.instance != null) { CreativeUIController.instance.ShowBuildGui(); HidePointers(true); }
        else { GuiControler.instance.ShowBuildGui(); }
    }
    public void ClickShowElectrickGui()
    {
        GuiControler.instance.ShowElectrickGui();
    }
    public void ClickShowPlatformConnection()
    {
        startConPoint = new Vector2Int(x, y);
        GuiControler.instance.ShowBuildConPanel();
        GuiControler.instance.BuildConPanelSc.UpdateDoText();
        clickMode = ClickMode.Connection;
        selectedFirstBuildingToBuildCon = true;
        MoveingConnectionPointer.gameObject.SetActive(true);
    }
    public void ClickDisassemblePlatform()
    {
        useObj = WorldMenager.instance.GetSquer(x, y);
        GuiControler.instance.CloseNowOpenGui();
        DisasembleObj();
    }
    public void ClickDemotitionPlatform()
    {
        useObj = WorldMenager.instance.GetSquer(x, y);
        GuiControler.instance.CloseNowOpenGui();
        DemolitionObj();
    }
    public void ClickMineButton()
    {
        Res res = GetResOfOre(useTerrain);
        if (res == Res.None) return;
        BuildMenager.instance.AddMiningTask(useTerrain, res, x, y, 1);

        GuiControler.instance.ShowPlatformOpction(Obj.BuildingUnderDemolition, useTerrain, x, y, usePBSc);
    }
    public void ClickPlantTree()
    {
        GuiControler.instance.CloseNowOpenGui();
        BuildMenager.instance.AddObjToBuild(Obj.Sapling, x, y, new Vector2Int());
    }
    public void ClickChooseStartPlace()
    {
        if (useObj != Obj.None) { MessageManager.instance.ShowMessage(Messages.CantStartHere); }
        else
        {
            GuiControler.instance.CloseNowOpenGui();
            GuiControler.instance.SetInteractibleAllGSButtons(true);
            WorldMenager.instance.CreateBasicBuilding(x, y);
        }
    }
    public void ClickAddItemsToPlatform()
    {
        GuiControler.instance.CloseNowOpenGui(false);
        GuiControler.instance.ShowStoragePanel();
        GuiControler.instance.SetOpenPanelsYPos();
    }

    //conection
    public void ClickDemotitionConnection()
    {
        MyConnectionPointer.SetActive(false);
        GuiControler.instance.CloseNowOpenGui();

        if (useObj == Obj.ConUnderConstruction)
        { BuildMenager.instance.RemoveConnectionPlan(startConPoint, endPintRoad); }
        else if (useObj == Obj.ConUnderDemolition)
        { BuildMenager.instance.RemoveConnectionPlan(startConPoint, endPintRoad); }
        else
        { useRoadB.RemoveRoad(); }
    }
    public void ClickDisassembleConnection()
    {
        MyConnectionPointer.SetActive(false);
        GuiControler.instance.CloseNowOpenGui();

        int sx = startConPoint.x;
        int sy = startConPoint.y;
        int ex = endPintRoad.x;
        int ey = endPintRoad.y;
        Transform PT = WorldMenager.instance.GetTransforOfObj(sx, sy);
        if (PT == null) { Debug.Log("ERROR! cant find obj on: " + sx + ", " + sy); return; }
        Transform TT = PT.Find(string.Format("{0}({1}, {2})({3}, {4})", useObj, sx, sy, ex, ey));
        if (TT == null)
        {
            sx = endPintRoad.x;
            sy = endPintRoad.y;
            ex = startConPoint.x;
            ey = startConPoint.y;
            PT = WorldMenager.instance.GetTransforOfObj(sx, sy);
            if (PT == null) { Debug.Log("ERROR! cant find obj on: " + sx + ", " + sy); return; }
            TT = PT.Find(string.Format("{0}({1}, {2})({3}, {4})", useObj, sx, sy, ex, ey));

            if (TT == null) { Debug.Log("ERROR! cant find connection: " + string.Format("{0}({1}, {2})({3}, {4})", useObj, sx, sy, ex, ey)); return; }
        }
        

        if (useObj == Obj.ConUnderConstruction)
        {
            //Debug.Log("TODO: Get building connection items");
            ObjectPlan BI = TT.GetComponent<ObjectPlan>();
            List<ItemRAQ> IIR = new List<ItemRAQ>();
            for (int i = 0; i < BI.keptItems.Count; i++)
            {
                IIR.Add(new ItemRAQ(BI.keptItems[i].res, BI.keptItems[i].qua));
            }
            BuildMenager.instance.RemoveConnectionPlan(new Vector2Int(sx, sy), new Vector2Int(ex, ey));
            if (IIR.Count > 0)
            {
                BuildMenager.instance.BuildObj(Obj.ConUnderDemolition, ex, ey, IIR, new Vector2Int(sx,sy));
            }
        }
        else
        {
            //Debug.Log("TODO: Get Connection items + build items");
            useRoadB.RemoveRoad();
            List<ItemRAQ> IIR = AllRecipes.instance.GetNeedItems(useObj);
            if (IIR.Count > 0)
            {
                BuildMenager.instance.BuildObj(Obj.ConUnderDemolition, ex, ey, IIR, new Vector2Int(sx, sy));
            }
        }
    }
    public void ClickChangeConnectionActive(bool isOn)
    {
        useRoadB.sendOff = !isOn;
    }
    public void ClickSubPButton()
    {
        if (useRoadB == null) { Debug.Log("mising useRoadB"); return; }
        useRoadB.priority--;
        GuiControler.instance.UpdateRoadPriorityText(useRoadB.priority);
        useRoadB.GetComponentInParent<PlatformBehavior>().SortPriorityTab();
    }
    public void ClickAddPButton()
    {
        if (useRoadB == null) { Debug.Log("mising useRoadB"); return; }
        useRoadB.priority++;
        GuiControler.instance.UpdateRoadPriorityText(useRoadB.priority);
        useRoadB.GetComponentInParent<PlatformBehavior>().SortPriorityTab();
    }

    //pricesing gui
    public void ClickRecipeButton()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        int recipeNumber = 0;
        int.TryParse(name, out recipeNumber);
        //Debug.Log(useObj + " " + recipeNumber);
        if (AllRecipes.instance.IsObjHaveCrafterNeedFuelSc(useObj))
        {
            WorldMenager.instance.GetTransforOfObj(x, y).GetComponent<CrafterNeedFuel>().SetResToCraft(recipeNumber);
        }
        if (AllRecipes.instance.IsObjHaveCrafterNeedEnergySc(useObj))
        {
            WorldMenager.instance.GetTransforOfObj(x, y).GetComponent<CrafterNeedEnergy>().SetResToCraft(recipeNumber);
        }
        else
        {
            switch (useObj)
            {
                case Obj.Farm: WorldMenager.instance.GetTransforOfObj(x, y).GetComponent<Farm>().SetResToCraft(recipeNumber); break;
            }
        }
        GuiControler.instance.UpdateProceingPanel();
        GuiControler.instance.HideResInfo();
    }
    public void ClickChangeCraftingButton()
    {
        if (AllRecipes.instance.IsObjHaveCrafterNeedFuelSc(useObj))
        {
            WorldMenager.instance.GetTransforOfObj(x, y).GetComponent<CrafterNeedFuel>().SetResToCraft(0);
        }
        if (AllRecipes.instance.IsObjHaveCrafterNeedEnergySc(useObj))
        {
            WorldMenager.instance.GetTransforOfObj(x, y).GetComponent<CrafterNeedEnergy>().SetResToCraft(0);
        }
        else
        {
            switch (useObj)
            {
                case Obj.Farm: WorldMenager.instance.GetTransforOfObj(x, y).GetComponent<Farm>().SetResToCraft(0); break;
            }
        }
        GuiControler.instance.UpdateProceingPanel();
    }
}