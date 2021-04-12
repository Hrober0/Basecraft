using System.Collections.Generic;
using UnityEngine;

public class WorldMenager : MonoBehaviour
{
    public static WorldMenager instance;
    void Awake()
    {
        if (instance != null) { Debug.Log("more the one WorldMenager on scen"); return; }
        instance = this;

        loadingWorld = true;
        SetGameSpeed(0);
    }

    [Header("Map")]
    public Vector2Int mapSize = new Vector2Int(20, 20);
    private int mapWidth, mapHeight;
    public Obj[,] squares;
    public Obj[,] terrainTiles;
    public short[,] squeresVeribal;

    [Header("Veribal to set")]
    public float frequencyOfChecking = 1f;
    public float platformCheckingRadius = 0.8f;

    [Header("Info")]
    public float worldTime = 0f;
    private Vector3Int timeV3 = new Vector3Int(0,0,0);
    [Range(0, 2)] public int GameSpeed = 1;
    public int minDistansToCenter = 30;
    public bool loadingWorld = true;

    [Header("List")]
    public List<Vector3Int> TurretPos = new List<Vector3Int>();

    private Vector2Int centralPoint;
    private Vector2Int dronStationPlace;
    private Vector2Int platformPlace;

    private Transform PlatformManagerTrans;
    private Transform TerrainManagerTrans;

    void Start()
    {
        LeftPanel.instance.SetCloseButton();

        PlatformManagerTrans = transform.Find("PlatformManager");
        TerrainManagerTrans = transform.Find("TarrainManager");

        if (SceneLoader.instance.gameLoadingMode == SceneLoader.GameLoadingMode.CreateNew)
        {
            CreateDefultWorld();
            return;
        }
        if (SceneLoader.instance.gameLoadingMode == SceneLoader.GameLoadingMode.LoadFromWorldData)
        {
            LoadWorldData(SceneLoader.instance.worldData);
            return;
        }
    }
    private void Update()
    {
        worldTime += Time.deltaTime;
    }

    //world set
    private void CreateDefultWorld()
    {
        Debug.Log("Creating new world... ");

        SceneLoader.instance.canChooseStartPlace = true;
        SceneLoader.instance.worldData = null;

        GeneralWorldData generalWorldData = SceneLoader.instance.generalWorldData;

        //map
        SceneLoader.instance.SetPostscript("General");
        mapSize = new Vector2Int(generalWorldData.mapWidth, generalWorldData.mapHeight);
        mapWidth = mapSize.x;
        mapHeight = mapSize.y;
        squares = new Obj[mapWidth, mapHeight];
        transform.GetComponent<WorldGrid>().CreateTab(mapWidth, mapHeight);
        SetGameSpeed(0);

        //create terrain
        SceneLoader.instance.SetPostscript("Terrain");
        TerrainManager.instance.SetVeribalsFromSL();
        generalWorldData.seed = Random.Range(int.MinValue, int.MaxValue);
        terrainTiles = MapGenerator.instance.GenerateTerrainTab(mapSize, generalWorldData.seed, generalWorldData.oreSizes, generalWorldData.forestSizes);
        squeresVeribal = new short[mapWidth, mapHeight];
        TerrainManager.instance.SpawnAllTerrain();
        if (ClickMenager.instance.CreateBacground) { MapGenerator.instance.CreateBecground(generalWorldData.seed, mapWidth, mapHeight); }

        //set central of map
        centralPoint = new Vector2Int(mapWidth / 2, mapHeight / 2);
        if(GetSquer(centralPoint.x, centralPoint.y) != Obj.None)
        {
            centralPoint = FindTheNearestObject(Obj.None, centralPoint.x, centralPoint.y, mapWidth/2);
        }

        //enemy
        SceneLoader.instance.SetPostscript("Enemy");
        if (generalWorldData.difficulty != Difficulty.Peaceful)
        {
            if (ClickMenager.instance.RandomBasePos)
            {
                if (mapHeight < mapWidth)
                { if (mapWidth / 2 - minDistansToCenter < 1) { minDistansToCenter = mapWidth / 2 - 1; } }
                else
                { if (mapHeight / 2 - minDistansToCenter < 1) { minDistansToCenter = mapHeight / 2 - 1; } }
                int randomi = Random.Range(1, 4);
                int losx=0, losy=0;
                switch (randomi)
                {
                    case 1:
                        losx = Random.Range(0, mapWidth - 1);
                        losy = Random.Range(0, mapHeight / 2 - minDistansToCenter);
                        break;
                    case 2:
                        losx = Random.Range(0, mapWidth / 2 - minDistansToCenter);
                        losy = Random.Range(0, mapHeight - 1);
                        break;
                    case 3:
                        losx = Random.Range(0, mapWidth - 1);
                        losy = Random.Range(mapHeight / 2 + minDistansToCenter, mapHeight - 1);
                        break;
                    case 4:
                        losx = Random.Range(mapWidth / 2 + minDistansToCenter, mapWidth - 1);
                        losy = Random.Range(0, mapHeight - 1);
                        break;
                }
                Vector2Int lpos = FindTheNearestObject(Obj.None, losx, losy, mapWidth / 2);
                if (lpos.x != -1)
                { EnemyControler.instance.CreateNewEnemyBase(lpos); }
                else
                { Debug.Log("zle cordynaty enemy base"); }
            }
            else
            {
                Vector2Int lpos = FindTheNearestObject(Obj.None, centralPoint.x+5, centralPoint.y+15, mapWidth / 2);
                if (lpos.x != -1)
                { EnemyControler.instance.CreateNewEnemyBase(lpos); }
                else
                { Debug.Log("zle cordynaty enemy base"); }
            }
        }

        //set waves of enemys
        EnemyControler.instance.SetDefaultWaves();
        EnemyControler.instance.SetWaveToLaunch(-1f);

        //finish loading
        SceneLoader.instance.SetPostscript("Starting");
        Debug.Log("Finish loading");

        //choose start place
        SetToChoosingStartPlace();

        InvokeRepeating("IncreaseTimeText", 1f, 1f);
        loadingWorld = false;
    }

    private void LoadWorldData(WorldData worldData)
    {
        string errorLog = "";

        if (worldData == null || worldData.General == null || worldData.General.mapWidth==0)
        {
            Debug.LogWarning("World data in SceneLoader is equal null! Then try create defult world");
            errorLog = "Error occurred when trying to load the world file.\n\n-save file is missing or corrupted.";
            LeftPanel.instance.OpenConfirmPanelReadSaveError(errorLog);
            CreateDefultWorld();
            return;
        }
        Debug.Log("Loading... ");

        //general
        SceneLoader.instance.SetPostscript("General");
        if (worldData.General == null)
        {
            Debug.LogError("Missing General data");
            errorLog = "Error occurred when trying to load the world file.\n\n-\"General\" data file is missing or corrupted.";
            LeftPanel.instance.OpenConfirmPanelReadSaveError(errorLog);
            CreateDefultWorld();
            return;
        }
        else
        {
            //map
            mapSize = new Vector2Int(worldData.General.mapWidth, worldData.General.mapHeight);
            mapWidth = mapSize.x;
            mapHeight = mapSize.y;
            squares = new Obj[mapWidth, mapHeight];
            transform.GetComponent<WorldGrid>().CreateTab(mapWidth, mapHeight);

            SceneLoader.instance.generalWorldData = worldData.General;

            //time
            SetGameSpeed(0);
            worldTime = worldData.General.worldTime;
            int timeInt = Mathf.FloorToInt(worldTime);
            timeV3.z = timeInt % 60;
            timeV3.y = ((timeInt - timeV3.z) / 60) % 60;
            timeV3.x = (timeInt - timeV3.y * 60 - timeV3.z) / 3600;
            float tTNextSek = worldTime - timeInt;
            //GuiControler.instance.SetTimeText(timeV3);
            InvokeRepeating("IncreaseTimeText", tTNextSek, 1f);
        }

        //player
        SceneLoader.instance.SetPostscript("Player");
        if (worldData.Player == null)
        {
            Debug.LogError("Missing Player data");
            errorLog += "-\"Player\" data file is missing or corrupted.\n";
            CameraControler.instance.SetCameraSize(40f);
            CameraControler.instance.MoveCameraToPoint(new Vector2(worldData.General.mapWidth * 5, worldData.General.mapHeight * 5));
            SceneLoader.instance.canChooseStartPlace = true;
        }
        else
        {
            //set camera position
            CameraControler.instance.SetCameraSize(worldData.Player.cameraScale);
            CameraControler.instance.MoveCameraToPoint(new Vector2(worldData.Player.cameraX, worldData.Player.cameraY));

            SceneLoader.instance.canChooseStartPlace = !worldData.Player.choosedStartPlace;
        }

        //terrain
        SceneLoader.instance.SetPostscript("Terrain");
        TerrainManager.instance.SetVeribalsFromSL();
        if (worldData.Terrain == null)
        { 
            Debug.LogError("Missing Terrain data");
            errorLog += "-\"Terrain\" data file is missing or corrupted.\n";
            terrainTiles = MapGenerator.instance.GenerateTerrainTab(mapSize, SceneLoader.instance.generalWorldData.seed, SceneLoader.instance.generalWorldData.oreSizes, SceneLoader.instance.generalWorldData.forestSizes);
            squeresVeribal = new short[mapWidth, mapHeight];
            TerrainManager.instance.SpawnAllTerrain();
            if (ClickMenager.instance.CreateBacground) { MapGenerator.instance.CreateBecground(SceneLoader.instance.generalWorldData.seed, mapWidth, mapHeight); }
        }
        else
        {
            SceneLoader.instance.generalWorldData.seed = worldData.Terrain.seed;
            MapGenerator.instance.CreateBecground(worldData.Terrain.seed, mapWidth, mapHeight);
            terrainTiles = new Obj[mapWidth, mapHeight];
            squeresVeribal = new short[mapWidth, mapHeight];
            int TIndex = 0;
            Obj terr;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    terr = (Obj)worldData.Terrain.terrainTiles[TIndex];
                    terrainTiles[x, y] = terr;
                    squeresVeribal[x, y] = worldData.Terrain.squaresVeribal[TIndex];

                    switch (terr)
                    {
                        case Obj.TerrainFertile:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.TerrainFertile, x, y);
                            break;
                        case Obj.Tree:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.TerrainFertile, x, y);
                            TerrainManager.instance.SpawnTree(x, y);
                            break;
                        case Obj.Sapling:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.TerrainFertile, x, y);
                            TerrainManager.instance.SpawnSapling(x, y, false);
                            break;
                        case Obj.WaterSource:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.WaterSource, x, y);
                            break;
                        case Obj.OilSource:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.OilSource, x, y);
                            break;
                        case Obj.Farmland:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.TerrainFertile, x, y);
                            int state = squeresVeribal[x, y] % 10;
                            int type = (squeresVeribal[x, y] - state) / 10;
                            TerrainManager.instance.SpawnFarmland(x, y, false, type);
                            ObjToGrow otg = new ObjToGrow(0f, Obj.Farmland, x, y);
                            for (short i = 0; i < state; i++) { TerrainManager.instance.GrowObj(otg, false); }
                            break;

                        case Obj.StoneOre:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.StoneOre, x, y);
                            break;
                        case Obj.IronOre:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.IronOre, x, y);
                            break;
                        case Obj.CopperOre:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.CopperOre, x, y);
                            break;
                        case Obj.CoalOre:
                            TerrainManager.instance.SimpleSpawnTerrain(Obj.CoalOre, x, y);
                            break;
                    }

                    switch (squeresVeribal[x, y])
                    {
                        case -1000: TerrainManager.instance.SpawnMountain(x, y, false); break;
                    }

                    TIndex++;
                }
            }

            //objecs to grow
            for (int i = 0; i < worldData.Terrain.objToGrows.Length; i++)
            {
                TerrainManager.instance.ObjToGrows.Add(worldData.Terrain.objToGrows[i]);
            }
        }

        //buildings
        SceneLoader.instance.SetPostscript("Buildings");
        if (worldData.Buildings == null)
        {
            Debug.LogError("Missing Buildings data");
            errorLog += "-\"Buildings\" data file is missing or corrupted.\n";
            SceneLoader.instance.canChooseStartPlace = true;
        }
        else
        {
            //spown platform
            for (int i = 0; i < worldData.Buildings.platforms.Length; i++)
            {
                BuildingsData.PlatformData platformD = worldData.Buildings.platforms[i];
                if (platformD != null)
                {
                    BuildMenager.instance.BuildObj((Obj)platformD.obj, platformD.x, platformD.y, null, new Vector2Int());
                    GameObject platformGO = GetTransforOfObj(platformD.x, platformD.y).gameObject;
                    PlatformBehavior platformSc = platformGO.GetComponent<PlatformBehavior>();

                    // set health
                    Square square = WorldGrid.GetSquare(platformD.x, platformD.y);
                    square.health = platformD.health;

                    WorldGrid.SetSquare(square);

                    switch ((Obj)platformD.obj)
                    {
                        case Obj.Battery:
                            Battery battery = platformGO.GetComponent<Battery>();
                            battery.charge = ReadVeribal(0, 0) / 100.00f;
                            break;
                    }

                    if (platformSc == null) { continue; }

                    Obj objType = (Obj)platformD.obj;

                    platformSc.startTaskTime = platformD.startTaskTime;
                    platformSc.timeToEndCraft = platformD.timeToEndCraft;
                    if (platformD.timeToEndCraft > 0) platformSc.working = true;

                    if (AllRecipes.instance.IsObjHaveCrafterNeedFuelSc(objType))
                    {
                        CrafterNeedFuel crafterFuel = platformGO.GetComponent<CrafterNeedFuel>();
                        crafterFuel.SetResToCraft(platformD.useRecipe);
                        Res fuel = (Res)ReadVeribal(0, 0);
                        crafterFuel.useFuel = fuel;
                        crafterFuel.percRemFuel = ReadVeribal(1, 0) / 100.00f;
                        crafterFuel.SetItemOnPlatformForFuel(fuel);
                    }
                    else if (AllRecipes.instance.IsObjHaveCrafterNeedEnergySc(objType))
                    {
                        CrafterNeedEnergy crafterEnergy = platformGO.GetComponent<CrafterNeedEnergy>();
                        crafterEnergy.SetResToCraft(platformD.useRecipe);
                        platformSc.startTaskTime = platformD.startTaskTime;
                        platformGO.GetComponent<ElectricityUser>().actCharge = ReadVeribal(0, 0) / 100.00f;
                    }
                    else
                    {
                        switch (objType)
                        {
                            case Obj.Farm:
                                Farm farmrSc = platformGO.GetComponent<Farm>();
                                farmrSc.SetResToCraft(platformD.useRecipe);
                                platformSc.startTaskTime = platformD.startTaskTime;
                                if (platformD.timeToEndCraft > 0)
                                {
                                    farmrSc.FoundPlaceToPlant = new Vector2Int(ReadVeribal(0, 0), ReadVeribal(1, 0));
                                    farmrSc.FoundPlaceToCollect = new Vector2Int(ReadVeribal(2, 0), ReadVeribal(3, 0));
                                }
                                break;
                            case Obj.Woodcuter:
                                WoodCuter woodCuterSc = platformGO.GetComponent<WoodCuter>();
                                platformSc.startTaskTime = platformD.startTaskTime;
                                if (platformD.timeToEndCraft > 0)
                                {
                                    woodCuterSc.TreePosition = new Vector2Int(ReadVeribal(0, 0), ReadVeribal(1, 0));
                                }
                                break;
                            case Obj.Quarry:
                                Quarry mineSc = platformGO.GetComponent<Quarry>();
                                platformSc.startTaskTime = platformD.startTaskTime;
                                break;
                            case Obj.Pump:
                                Pump pumpSc = platformGO.GetComponent<Pump>();
                                platformSc.startTaskTime = platformD.startTaskTime;
                                break;
                            case Obj.Planter:
                                Planter planterSc = platformGO.GetComponent<Planter>();
                                platformSc.startTaskTime = platformD.startTaskTime;
                                if (platformD.timeToEndCraft > 0)
                                {
                                    planterSc.TreePosition = new Vector2Int(ReadVeribal(0, 0), ReadVeribal(1, 0));
                                }
                                break;
                            case Obj.CombustionGenerator:
                                platformGO.GetComponent<ElectricityUser>().actCharge = ReadVeribal(0, 0) / 100.00f;
                                break;
                            case Obj.SteamGenerator:
                                platformGO.GetComponent<ElectricityUser>().actCharge = ReadVeribal(0, 0) / 100.00f;
                                break;
                        }
                    }

                    switch (platformSc.usingGuiType)
                    {
                        case PlatfotmGUIType.Procesing:
                            break;
                        case PlatfotmGUIType.Storage:
                            platformSc.canDronesGetRes = ReadVeribal(0, 1)==1 ? true : false;
                            break;
                        case PlatfotmGUIType.Turret:
                            Turret turretSc = platformGO.GetComponent<Turret>();
                            turretSc.nowResiBulet = ReadVeribal(0, 0);
                            turretSc.avaShootTime = ReadVeribal(1, 0) / 100.00f;
                            turretSc.TurretUp.rotation = Quaternion.Euler(0f, 0f, ReadVeribal(2, 0) / 100.00f);
                            if (platformGO.TryGetComponent(out ElectricityUser eleUser)) { eleUser.actCharge = ReadVeribal(3, 0) / 100.00f; }
                            break;
                    }

                    //items
                    int lenght = platformD.items.Length;
                    for (int i2 = 0; i2 < lenght; i2++)
                    {
                        ItemData itemD = platformD.items[i2];
                        int resI = (int)itemD.res;
                        platformSc.itemOnPlatform[resI].qua = itemD.qua;
                        platformSc.itemOnPlatform[resI].maxQua = itemD.maxQua;
                        platformSc.itemOnPlatform[resI].canIn = itemD.canIn;
                        platformSc.itemOnPlatform[resI].canOut = itemD.canOut;
                    }
                    platformSc.UpdateAvalibleResList();

                    //requested items
                    platformSc.keepAmountOfRequestedItems = platformD.keepAmountOfRequestedItems;
                    if (platformD.requestItems != null) { foreach (ItemRAQ item in platformD.requestItems) { platformSc.requestItems.Add(item); } }

                    int ReadVeribal(int index, int def) { if (platformD.veribals.Length > index) { return platformD.veribals[index]; } else { Debug.Log("Missing veribal in " + (Obj)platformD.obj + " number:" + index + ". Setting:" + def); return def; } }
                }
            }

            //spown connections
            for (int i = 0; i < worldData.Buildings.connections.Length; i++)
            {
                BuildingsData.ConnectionData roadD = worldData.Buildings.connections[i];
                BuildMenager.instance.BuildObj((Obj)roadD.type, roadD.ex, roadD.ey, null, new Vector2Int(roadD.sx, roadD.sy));

                Transform startPlatformT = GetTransforOfObj(roadD.sx, roadD.sy);
                string roadName = string.Format("{0}({1}, {2})({3}, {4})", (Obj)roadD.type, roadD.sx, roadD.sy, roadD.ex, roadD.ey);
                GameObject roadGO = startPlatformT.Find(roadName).gameObject;
                RoadBehavior roadSc = roadGO.GetComponent<RoadBehavior>();
                //res
                for (int i2 = 0; i2 < roadD.movingRess.Length; i2++)
                {
                    roadSc.SetMovingRes((Res)roadD.movingRess[i2], new Vector2(roadD.ressX[i2], roadD.ressY[i2]));
                }

                //property
                roadSc.priority = roadD.priority;
                roadSc.sendOff = roadD.sendOff;
            }

            //spown plans
            for (int i = 0; i < worldData.Buildings.objectsPlan.Length; i++)
            {
                BuildingsData.ObjectPlanData planD = worldData.Buildings.objectsPlan[i];
                Obj obj = (Obj)planD.objType;
                ObjectPlan BI;
                if (obj == Obj.Connection1 || obj == Obj.Connection2 || obj == Obj.Connection3 || obj == Obj.ConUnderDemolition)
                {
                    BuildMenager.instance.CreateConPlan(obj, planD.sx, planD.sy, planD.ex, planD.ey, null);
                    Transform platT = GetTransforOfObj(planD.sx, planD.sy);
                    if (platT == null) { Debug.LogError("dont found building at (" + planD.sx + "," + planD.sy + ")"); continue; }
                    string roadName;
                    if (obj == Obj.ConUnderDemolition) { roadName = string.Format("{0}({1}, {2})({3}, {4})", Obj.ConUnderDemolition, planD.sx, planD.sy, planD.ex, planD.ey); }
                    else { roadName = string.Format("{0}({1}, {2})({3}, {4})", Obj.ConUnderConstruction, planD.sx, planD.sy, planD.ex, planD.ey); }
                    Transform roadT = platT.Find(roadName);
                    if (roadT == null) { Debug.LogError("doont found connection " + roadName); continue; }
                    BI = roadT.GetComponent<ObjectPlan>();
                }
                else
                {
                    int x = planD.sx;
                    int y = planD.sy;

                    ObjectPlanType planType = (ObjectPlanType)planD.planType;
                    if (planType == ObjectPlanType.Disasemble) { BuildMenager.instance.CreateDisasembleObject(obj, x, y, null); }
                    else if (planType == ObjectPlanType.Building) { BuildMenager.instance.CreateBuildingPlan(obj, x, y, null); }
                    else { Debug.LogError("Wrong ObjectPlanType:" + planD.planType + "on: (" + x + "," + y + ")"); continue; }

                    Transform platT = GetTransforOfObj(x, y);
                    if (platT == null) { Debug.LogError("Platform dont found on: (" + x + "," + y + ")"); continue; }
                    BI = platT.GetComponent<ObjectPlan>();
                }
                BI.objName = obj;
                BI.startRoadPoint = new Vector2Int(planD.sx, planD.sy);
                BI.endRoadPoint = new Vector2Int(planD.ex, planD.ey);
                BI.needItems = new List<ItemRAQ>();
                for (int i2 = 0; i2 < planD.needItems.Length; i2++) { BI.needItems.Add(planD.needItems[i2]); }
                BI.keptItems = new List<ItemRAQ>();
                for (int i2 = 0; i2 < planD.keepItems.Length; i2++) { BI.keptItems.Add(planD.keepItems[i2]); }
            }

            //objects to build
            for (int i = 0; i < worldData.Buildings.objectsToBuild.Length; i++)
            {
                BuildingsData.ObjToBuildData OTBD = worldData.Buildings.objectsToBuild[i];
                Obj obj = (Obj)OTBD.objectType;

                Transform trans = null;
                if (AllRecipes.instance.IsItConnection(obj))
                {
                    Obj trueObj;
                    if (OTBD.planType == ObjectPlanType.Building) { trueObj = Obj.ConUnderConstruction; }
                    else { trueObj = Obj.ConUnderDemolition; }
                    Transform platT = GetTransforOfObj(OTBD.StartPointRoadsX, OTBD.StartPointRoadsY);
                    if (platT == null) { Debug.LogError("Try set OTB but not found connection parent"); }
                    else { trans = platT.Find(string.Format("{0}({1}, {2})({3}, {4})", trueObj, OTBD.StartPointRoadsX, OTBD.StartPointRoadsY, OTBD.xTB, OTBD.yTB)); }
                }
                else
                {
                    trans = GetTransforOfObj(OTBD.xTB, OTBD.yTB);
                }
                if (trans == null) { Debug.LogError("Try set OTB but not found transform (" + obj + ")"); }

                List<ItemRAQ> neededItems = new List<ItemRAQ>();
                foreach (ItemRAQ item in OTBD.neededItems) { neededItems.Add(item); }

                ObjToBuild OTB = new ObjToBuild(obj, OTBD.planType, OTBD.xTB, OTBD.yTB, neededItems, trans);
                OTB.startPointRoadsX = OTBD.StartPointRoadsX;
                OTB.startPointRoadsY = OTBD.StartPointRoadsY;
                BuildMenager.instance.BuildQueue.Add(OTB);
            }
        }

        //enemy
        SceneLoader.instance.SetPostscript("Enemy");
        if (worldData.Enemy == null)
        {
            Debug.LogError("Missing Enemy data");
            errorLog += "-\"Enemy\" data file is missing or corrupted.\n";
        }
        else
        {
            EnemyControler.instance.SetWaves(worldData.Enemy.eWaves);
            EnemyControler.instance.SetSavesWaves(worldData.Enemy.eSavesWaves);
            foreach (EnemyData.EnemyBaseData eBase in worldData.Enemy.eBasesControlers) { EnemyControler.instance.LoadEnemyBase(eBase); }
            EnemyControler.instance.SetWaveToLaunch(worldData.Enemy.attackTime - worldTime);
        }

        //units
        SceneLoader.instance.SetPostscript("Units");
        if (worldData.Units == null)
        {
            Debug.LogError("Missing Units data");
            errorLog += "-\"Units\" data file is missing or corrupted.\n";
        }
        else
        {
            //spown drons
            for (int i = 0; i < worldData.Units.drons.Length; i++)
            {
                UnitsData.DroneData dronD = worldData.Units.drons[i];
                int dsx = (int)(dronD.dSPositinX / 10);
                int dsy = (int)(dronD.dSPositinY / 10);
                Transform DST = GetTransforOfObj(dsx, dsy);
                if (DST == null) { Debug.Log("nie znaleziono stacji dronów na (" + dsx + "," + dsy + ")"); continue; }
                if (DST.GetComponent<DronStation>() == null) { Debug.Log("obiekt na (" + dsx + "," + dsy + ") to nie stacja dronów"); continue; }

                if (!dronD.isFlying)
                {
                    //drons are adding to DS is spanw platforms function
                    //DronControler.instance.SpownDron(DST, true);
                }
                else
                {
                    DronControler.instance.SpownDron(DST, false, dronD);
                }
            }

            //spown bullets
            for (int i = 0; i < worldData.Units.bullets.Length; i++)
            {
                UnitsData.BulletData BulletD = worldData.Units.bullets[i];
                Transform targetT = GetTransforOfObj(BulletD.targetX, BulletD.targetY);
                BulletManager.instance.SpownBullet(new Vector2(BulletD.myX, BulletD.myY), BulletD.rotate, targetT, (BulletsE)BulletD.bulletE);
            }
        }

        //Reload
        SceneLoader.instance.SetPostscript("Starting");
        TerrainManager.instance.UpdateAllMoutains();
        

        //finish loading
        Invoke(nameof(FinishLoading), 0.01f);

        //wass error
        if (errorLog != "")
        {
            errorLog = "Error occurred when trying to load the world file.\n\n" + errorLog;
            LeftPanel.instance.OpenConfirmPanelReadSaveError(errorLog);

            if (SceneLoader.instance.canChooseStartPlace)
            { SetToChoosingStartPlace(false); }
            else
            { SetGameSpeed(0); }
        }
        else
        {
            if (SceneLoader.instance.canChooseStartPlace)
            { SetToChoosingStartPlace(); }
            else
            { SetGameSpeed(1); }
        }    
    }
    private void FinishLoading()
    {
        Debug.Log("Finish loading");
        loadingWorld = false;
        DronControler.instance.ReloadDSNetwork();
        ElectricityManager.instance.ReloadNetwork();
        TerrainManager.instance.CheckGrow();
        SceneLoader.instance.worldData = null;
    }

    private void SetToChoosingStartPlace(bool check=true)
    {
        Debug.Log("Set to choosing start place");

        loadingWorld = false;

        if (check && worldTime > 1)
        { Debug.Log("ERROR! Canceling the option to choose a starting place because world time is greater then 1 second");
            SceneLoader.instance.canChooseStartPlace = false;
            SetGameSpeed(1);
            return;
        }

        //set game speed
        SetGameSpeed(0);

        Vector2Int cPoint = new Vector2Int(mapWidth / 2, mapHeight / 2);

        //set camera position
        CameraControler.instance.MoveCameraToPoint(cPoint * 10);
        CameraControler.instance.SetCameraSize(80);

        GuiControler.instance.ShowChooseStartPlace(Obj.Locked); 
    }
    public void CreateBasicBuilding(int x, int y)
    {
        Debug.Log("Choosed start place on " + x + " " + y);

        //spown dron station
        dronStationPlace = FindTheNearestObject(Obj.None, x, y, mapWidth / 2);
        BuildMenager.instance.BuildObj(Obj.DroneStation, dronStationPlace.x, dronStationPlace.y, null, new Vector2Int());

        //spown platform
        x = Random.Range(dronStationPlace.x - 3, dronStationPlace.x + 3);
        y = Random.Range(dronStationPlace.y - 3, dronStationPlace.y + 3);
        platformPlace = FindTheNearestObject(Obj.None, x, y, mapWidth / 2);
        BuildMenager.instance.BuildObj(Obj.Warehouse2, platformPlace.x, platformPlace.y, null, new Vector2Int());

        Invoke("AddDefaultItem", 0.1f);
        SetGameSpeed(1);

        SceneLoader.instance.canChooseStartPlace = false;

        DronControler.instance.ReloadDSNetwork();
    }
    private void AddDefaultItem()
    {
        //add default item to platform
        PlatformBehavior PB = GetTransforOfObj(platformPlace.x, platformPlace.y).GetComponent<PlatformBehavior>();
        Transform DST = GetTransforOfObj(dronStationPlace.x, dronStationPlace.y);

        switch (SceneLoader.instance.numberOfStartItems)
        {
            case 0:
                DST.GetComponent<PlatformBehavior>().AddItem(Res.Drone, 1);
                break;
            case 1:
                PB.AddItem(Res.Wood, 5);
                PB.AddItem(Res.StoneOre, 12);
                DST.GetComponent<PlatformBehavior>().AddItem(Res.Drone, 1);
                break;
            case 2:
                DST.GetComponent<PlatformBehavior>().AddItem(Res.Drone, 1);
                PB.AddItem(Res.Wood, 8);
                PB.AddItem(Res.StoneOre, 15);
                PB.AddItem(Res.StoneBrick, 6);
                PB.AddItem(Res.BottleEmpty, 1);
                break;
            case 3:
                DST.GetComponent<PlatformBehavior>().AddItem(Res.Drone, 2);
                PB.AddItem(Res.Wood, 12);
                PB.AddItem(Res.StoneOre, 17);
                PB.AddItem(Res.StoneBrick, 8);
                PB.AddItem(Res.WoodenCircuit, 2);
                PB.AddItem(Res.IronGear, 4);
                PB.AddItem(Res.BagSand, 2);
                PB.AddItem(Res.BottleEmpty, 2);
                break;
            case 4:
                DST.GetComponent<PlatformBehavior>().AddItem(Res.Drone, 3);
                PB.AddItem(Res.Wood, 15);
                PB.AddItem(Res.StoneOre, 20);
                PB.AddItem(Res.StoneBrick, 8);
                PB.AddItem(Res.WoodenCircuit, 2);
                PB.AddItem(Res.IronGear, 4);
                PB.AddItem(Res.BagSand, 5);
                PB.AddItem(Res.BottleEmpty, 2);
                PB.AddItem(Res.Plastic, 10);
                PB.AddItem(Res.ElectricEngine, 2);
                PB.AddItem(Res.PlasticCircuit, 3);
                break;
        }
    }

    //test
    private void Test()
    {
        Debug.Log("test");
        //EnemyControler.instance.CallAttack();
    }

    //time
    private void IncreaseTimeText()
    {
        timeV3.z++;
        if (timeV3.z > 59) { timeV3.y++; timeV3.z = 0; }
        if (timeV3.y > 59) { timeV3.x++; timeV3.y = 0; }
        //GuiControler.instance.SetTimeText(timeV3);
    }
    public void SetGameSpeed(int speed)
    {
        switch (speed)
        {
            case 0: Time.timeScale = 0f; GameSpeed = 0; break;
            case 1: Time.timeScale = 1f; GameSpeed = 1; Time.fixedDeltaTime = 1f; break;
            case 2: Time.timeScale = 10f; GameSpeed = 10; Time.fixedDeltaTime = 1f; break;
        }
        GuiControler.instance.ChangeSpeedButtons(speed);
    }

    //difrent
    public Obj GetSquer(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) { return Obj.Locked; }
        return squares[x, y];
    }
    public Obj GetTerrainTile(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) { return Obj.None; }
        return terrainTiles[x, y];
    }
    public string TagEnumToString(TagsEnum tagE)
    {
        switch (tagE)
        {
            case TagsEnum.Dron:             return "Dron";
            case TagsEnum.EnemyUnit:            return "Enemy";
            case TagsEnum.EnemyBase:        return "EnemyBase";
            case TagsEnum.Platform:         return "Platform";
            case TagsEnum.ObjectPlan:   return "PlatformBorder";
            case TagsEnum.Connection:       return "Connection";
            case TagsEnum.ConBorder:        return "ConBorder";
            case TagsEnum.Wall:             return "Wall";
            case TagsEnum.Electricity:      return "Electricity";
            case TagsEnum.TerrainObj:       return "TerrainObj";
        }
        return "";
    }
    public TagsEnum TagToTagEnum(string tagS)
    {
        switch (tagS)
        {
            case "Dron": return TagsEnum.Dron;
            case "Enemy": return TagsEnum.EnemyUnit;
            case "EnemyBase": return TagsEnum.EnemyBase;
            case "Platform": return TagsEnum.Platform;
            case "PlatformBorder": return TagsEnum.ObjectPlan;
            case "Connection": return TagsEnum.Connection;
            case "ConBorder": return TagsEnum.ConBorder;
            case "Wall": return TagsEnum.Wall;
            case "Electricity": return TagsEnum.Electricity;
            case "TerrainObj": return TagsEnum.TerrainObj;
        }
        return TagsEnum.Different;
    }
    public Vector2Int GetTabPos(Vector2 pos)
    {
        return new Vector2Int(Zaok(pos.x), Zaok(pos.y));
        int Zaok(float a)
        {
            int b = (int)a - 5;
            b /= 10;
            a /= 10;
            if ((float)b < a) { b++; }
            if (((float)b - a) > 5f) { b--; }
            return b;
        }
    }

    public void RemoveObjFromPos(int x, int y)
    {
        Transform tTrans = GetTransforOfObj(x, y);
        if (tTrans == null) { Debug.Log("ERROR! Missing obj in squers tab - " + GetSquer(x, y) + " on: " + x + " " + y); return; }

        RemoveObjFromGO(tTrans.gameObject, x, y);
    }
    public void RemoveObjFromGO(GameObject GOToRemove, int x, int y)
    {
        if (GOToRemove == null) { Debug.Log("ERROR! Game object missing"); }

        Obj objToRemove = squares[x, y];
        //Debug.Log("removing obj:" + objToRemove);
        if (objToRemove == Obj.Farmland || objToRemove == Obj.FarmlandGrape || objToRemove == Obj.FarmlandFlax || objToRemove == Obj.FarmlandRubber)
        {
            Transform terrFertileT = TerrainManagerTrans.Find(string.Format("{0}({1}, {2})", Obj.TerrainFertile, x, y));
            if (terrFertileT != null) { terrFertileT.gameObject.SetActive(true); } else { Debug.Log("nie znaleziono terrFertile na: " + x + " " + y); }
            squeresVeribal[x, y] = 0;
        }
        else if(objToRemove == Obj.Mountain)
        {
            squares[x, y] = Obj.None;
            for (int ix = -1; ix <= 1; ix++)
            {
                for (int iy = -1; iy <= 1; iy++)
                {
                    int tx = x + ix;
                    int ty = y + iy;
                    TerrainManager.instance.UpdateMoutain(tx, ty);
                    Obj terr = GetTerrainTile(tx, ty);
                    Transform terrFertileT = TerrainManagerTrans.Find(string.Format("{0}({1}, {2})", terr, tx, ty));
                    if (terrFertileT != null) { terrFertileT.gameObject.SetActive(true); }
                }
            }
        }
        else if (objToRemove == Obj.BuildingUnderConstruction || objToRemove == Obj.BuildingUnderDemolition || objToRemove == Obj.ConUnderConstruction || objToRemove == Obj.ConUnderDemolition)
        {
            GOToRemove.SetActive(false);
            if(GOToRemove.TryGetComponent(out ObjectPlan OPSc)) { DronControler.instance.RemOPFromList(OPSc); }
        }
        else if(objToRemove == Obj.Wall0 || objToRemove == Obj.Wall1 || objToRemove == Obj.Wall2 || objToRemove == Obj.Wall3)
        {
            HARWallCon(x + 1, y);
            HARWallCon(x - 1, y);
            HARWallCon(x, y + 1);
            HARWallCon(x, y - 1);

            void HARWallCon(int px, int py)
            {
                Obj chO = GetSquer(px, py);
                if(chO == Obj.Wall0 || chO == Obj.Wall1 || chO == Obj.Wall2 || chO == Obj.Wall3)
                {
                    Transform wT = GetTransforOfObj(px, py).Find(string.Format("Connector({0}, {1})", x, y));
                    if (wT == null) { return; }
                    Destroy(wT.gameObject);
                }
                
            }
        }
        else if (AllRecipes.instance.IsEnergyGenerator(objToRemove))
        {
            ElectricityManager.instance.RemoveGenerator(GOToRemove.GetComponent<ElectricityUser>());
        }
        else if (AllRecipes.instance.IsEnergyRequester(objToRemove))
        {
            ElectricityManager.instance.RemoveRequester(GOToRemove.GetComponent<ElectricityUser>());
        }
        else if (objToRemove == Obj.Battery)
        {
            ElectricityManager.instance.RemoveBattery(GOToRemove.GetComponent<Battery>());
        }
        else if (objToRemove == Obj.EnemyWall || objToRemove == Obj.EnemySpawner || objToRemove == Obj.EnemyTurret || objToRemove == Obj.EnemyCore || objToRemove == Obj.EnemyPlatform)
        {
            Transform parent = GOToRemove.transform.parent.parent;
            EnemyBaseControler EBC = parent.GetComponent<EnemyBaseControler>();
            if (EBC != null) { EBC.DisconnectBuilding(x, y); }
            else { Debug.Log("cant find parent"); }
        }

        if(GOToRemove.TryGetComponent(out TransmissionTower transmissionTower))
        {
            ElectricityManager.instance.RemoveTT(transmissionTower);
        }

        if (squares[x, y] == objToRemove) { squares[x, y] = Obj.None; }

        //act stats
        TaskManager.instance.ActBuilding(objToRemove, -1);

        GOToRemove.SetActive(false);
        WorldGrid.RemoveSquare(x, y);

        //close gui
        if (GuiControler.instance.useObj == objToRemove)
        {
            if (GuiControler.instance.useX == x && GuiControler.instance.useY == y) { GuiControler.instance.CloseNowOpenGui(); }
        }

        Destroy(GOToRemove);

        Invoke("CheckPlayerOpction", 1f);
    }
    public Transform GetTransforOfObj(int x, int y)
    {
        Obj searchObj = GetSquer(x, y);
        //Debug.Log("szukam: " + searchObj);
        if (searchObj == Obj.None || searchObj == Obj.Connection1 || searchObj == Obj.EnemysTerrain) { return null; }
        Transform ToReturn = null;
        
        Square square = WorldGrid.GetSquare(x, y);
        if (square != null)
        {
            ToReturn = square.trans;
        }
        if (ToReturn != null)
        {
            return ToReturn;
        }
        
        Debug.Log("obiektu: " + searchObj+" nie ma w grid");
        if (AllRecipes.instance.IsItTerrain(searchObj))
        { ToReturn = TerrainManagerTrans.Find(string.Format("{0}({1}, {2})", searchObj, x, y)); }
        else
        { ToReturn = PlatformManagerTrans.Find(string.Format("{0}({1}, {2})", searchObj, x, y)); }

        return ToReturn;
    }

    //find
    public Vector2Int FindTheNearestObject(Obj sObj, int startX, int startY, int maxRang)
    {
        if (GetSquer(startX, startY) == sObj) { return new Vector2Int(startX, startY); }
        int sx, sy, tmp;
        for (int r = 1; r < maxRang; r++)
        {
            sx = startX - r + 1;
            sy = startY + r;
            tmp = sx + r * 2;
            for (; sx < tmp; sx++) { if (GetSquer(sx, sy) == sObj) { return new Vector2Int(sx, sy); } }
            sx--; sy--;
            tmp = sy - r * 2;
            for (; sy > tmp; sy--) { if (GetSquer(sx, sy) == sObj) { return new Vector2Int(sx, sy); } }
            sx--; sy++;
            tmp = sx - r * 2;
            for (; sx > tmp; sx--) { if (GetSquer(sx, sy) == sObj) { return new Vector2Int(sx, sy); } }
            sx++; sy++;
            tmp = sy + r * 2;
            for (; sy < tmp; sy++) { if (GetSquer(sx, sy) == sObj) { return new Vector2Int(sx, sy); } }
        }
        return new Vector2Int(-1, -1);
    }
    public Vector2Int FindTheNearestObjectOnTerrain(Obj sObj, Obj sTerrain, int startX, int startY, int maxRang)
    {
        if (GetSquer(startX, startY) == sObj) { return new Vector2Int(startX, startY); }
        int sx, sy, tmp;
        for (int r = 1; r < maxRang; r++)
        {
            sx = startX - r + 1;
            sy = startY + r;
            tmp = sx + r * 2;
            for (; sx < tmp; sx++) { if (GetSquer(sx, sy) == sObj && GetTerrainTile(sx, sy) == sTerrain) { return new Vector2Int(sx, sy); } }
            sx--; sy--;
            tmp = sy - r * 2;
            for (; sy > tmp; sy--) { if (GetSquer(sx, sy) == sObj && GetTerrainTile(sx, sy) == sTerrain) { return new Vector2Int(sx, sy); } }
            sx--; sy++;
            tmp = sx - r * 2;
            for (; sx > tmp; sx--) { if (GetSquer(sx, sy) == sObj && GetTerrainTile(sx, sy) == sTerrain) { return new Vector2Int(sx, sy); } }
            sx++; sy++;
            tmp = sy + r * 2;
            for (; sy < tmp; sy++) { if (GetSquer(sx, sy) == sObj && GetTerrainTile(sx, sy) == sTerrain) { return new Vector2Int(sx, sy); } }
        }
        return new Vector2Int(-1, -1);
    }
    public Vector3[] GetDisToPlaceofLine(int sx, int sy, int ex, int ey)
    {
        int x1, y1, x2, y2;
        if (sx > ex) { x1 = ex; y1 = ey; x2 = sx; y2 = sy; }
        else { x1 = sx; y1 = sy; x2 = ex; y2 = ey; }

        float t = (x2 - x1);

        if (t == 0)
        {
            if (y1 > y2) { int p = y1; y1 = y2; y2 = p; }

            Vector3[] tab = new Vector3[y2 - y1 - 1];
            for (int y = y1 + 1; y < y2; y++)
            {
                int i = y - y1 - 1;
                tab[i] = new Vector3(x1, y, 0);
            }
            return tab;
        }
        else
        {
            float a = (y2 - y1) / t;
            float b = y1 - a * x1;
            float del = Mathf.Sqrt(1 + a * a);

            float s, dist;
            int iy, ix;

            int dy = y2 - y1; if (dy < 0) { dy *= -1; }
            List<Vector3> list = new List<Vector3>();
            if (t > dy)
            {
                for (ix = x1 + 1; ix < x2; ix++)
                {
                    s = a * ix + b;

                    iy = Mathf.FloorToInt(s);
                    dist = CalcD();
                    list.Add(new Vector3(ix, iy, dist));
                    if (s % 1 != 0)
                    {
                        iy = Mathf.CeilToInt(s);
                        dist = CalcD();
                        list.Add(new Vector3(ix, iy, dist));
                    }
                }
                return list.ToArray();
            }

            if (sy > ey) { x1 = ex; y1 = ey; x2 = sx; y2 = sy; }
            else { x1 = sx; y1 = sy; x2 = ex; y2 = ey; }

            for (iy = y1 + 1; iy < y2; iy++)
            {
                s = (iy - b) / a;

                ix = Mathf.FloorToInt(s);
                dist = CalcD();
                list.Add(new Vector3(ix, iy, dist));
                if (s % 1 != 0)
                {
                    ix = Mathf.CeilToInt(s);
                    dist = CalcD();
                    list.Add(new Vector3(ix, iy, dist));
                }   
            }
            return list.ToArray();

            float CalcD()
            {
                float g = a * ix + -1 * iy + b;
                if (g < 0) { g *= -1; }
                return g / del;
            }
        }
    }
    public bool IsInDistance(int x1, int y1, int x2, int y2, int range)
    {
        int rs = range * range;
        int dx = x1 - x2;
        int dy = y1 - y2;
        int o = dx * dx + dy * dy;
        if (o <= rs) return true;
        return false;
    }


    private void CheckPlayerOpction()
    {
        if (DronControler.instance.AllDS.Count == 0)
        {
            //GuiControler.instance.ShowLoseMenu();
            MessageManager.instance.ShowMessage(Messages.NoDronStation);
        }
    }

    
    //word data
    public WorldData CrateWorldDate(string saveName)
    {
        //general
        GeneralWorldData GeneralD;
        {
            string actDate = System.DateTime.Now.ToString("hh:mm") + " " + System.DateTime.Now.ToString("MM/dd/yyyy");
            GeneralD = SceneLoader.instance.generalWorldData;

            GeneralD.saveName = saveName;
            GeneralD.version = SettingsManager.instance.gameVersion;
            GeneralD.date = actDate;
            GeneralD.mapWidth = mapSize.x;
            GeneralD.mapHeight = mapSize.y;
            GeneralD.worldTime = worldTime;
        }

        //player
        PlayerData PlayerD;
        {
            PlayerD = new PlayerData
                (
                CameraControler.instance.transform.position.x,
                CameraControler.instance.transform.position.y,
                CameraControler.instance.GetScale,
                !SceneLoader.instance.canChooseStartPlace
                );
        }

        //terrain
        TerrainData TerrainD;
        {
            short[] terTiles = new short[terrainTiles.Length];
            short[] squaresValue = new short[terrainTiles.Length];
            int tIndex = 0;
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    terTiles[tIndex] = (short)terrainTiles[x, y];
                    squaresValue[tIndex] = squeresVeribal[x, y];
                    switch (squares[x, y])
                    {
                        case Obj.Tree: terTiles[tIndex] = (short)Obj.Tree; break;
                        case Obj.Sapling: terTiles[tIndex] = (short)Obj.Sapling; break;
                        case Obj.Farmland: terTiles[tIndex] = (short)Obj.Farmland; break;
                        case Obj.FarmlandFlax: terTiles[tIndex] = (short)Obj.Farmland; break;
                        case Obj.FarmlandGrape: terTiles[tIndex] = (short)Obj.Farmland; break;
                        case Obj.FarmlandRubber: terTiles[tIndex] = (short)Obj.Farmland; break;

                        case Obj.Mountain: squaresValue[tIndex] = -1000; break;
                    }

                    tIndex++;
                }
            }
            TerrainD = new TerrainData
                (
                SceneLoader.instance.generalWorldData.seed,
                terTiles,
                squaresValue,
                TerrainManager.instance.ObjToGrows.ToArray()
                );
        }

        //buildings
        BuildingsData BuildingsD;
        {
            //platforms
            int platformsCount = PlatformManagerTrans.childCount;
            BuildingsData.PlatformData[] platforms = new BuildingsData.PlatformData[platformsCount];
            List<RoadBehavior> connectionsSc = new List<RoadBehavior>();
            List<ObjectPlan> bordersSc = new List<ObjectPlan>();
            for (int i = 0; i < platformsCount; i++)
            {
                GameObject child = PlatformManagerTrans.GetChild(i).gameObject;
                TagsEnum tag = TagToTagEnum(child.tag);

                if (tag == TagsEnum.ObjectPlan)
                {
                    ObjectPlan BISc = child.GetComponent<ObjectPlan>();
                    bordersSc.Add(BISc);
                    platforms[i] = null;
                }
                else if (tag == TagsEnum.Wall || tag == TagsEnum.Electricity)
                {
                    int posX = (int)child.transform.position.x / 10;
                    int posY = (int)child.transform.position.y / 10;
                    Obj obj = GetSquer(posX, posY);
                    BuildingsData.PlatformData platform = new BuildingsData.PlatformData
                        (
                        (int)obj,
                        posX,
                        posY,
                        WorldGrid.GetSquare(posX, posY).health,
                        null,
                        0,
                        -1
                        );

                    switch (obj)
                    {
                        case Obj.TransmissionTower:
                            break;
                        case Obj.SolarPanel1:
                            break;
                        case Obj.Repairer:
                            Debug.Log("TODO: save and load reperier");
                            break;
                    }

                    platforms[i] = platform;
                }
                else if (tag == TagsEnum.Platform)
                {
                    PlatformBehavior platformSc = child.GetComponent<PlatformBehavior>();
                    Vector2Int objPos = platformSc.GetTabPos();
                    Obj objType = squares[objPos.x, objPos.y];

                    //add roads to list
                    for (int i4 = 0; i4 < platformSc.roadListOut.Count; i4++) { connectionsSc.Add(platformSc.roadListOut[i4]); }
                    for (int i4 = 0; i4 < platformSc.roadBorderListIn.Count; i4++) { bordersSc.Add(platformSc.roadBorderListIn[i4]); }

                    //set1
                    switch (objType)
                    {
                        case Obj.DroneStation:
                            DronStation droneStationSc = child.GetComponent<DronStation>();
                            platformSc.itemOnPlatform[(int)Res.Drone].qua += droneStationSc.availableDrons.Count;
                            platformSc.itemOnPlatform[0].qua -= droneStationSc.availableDrons.Count;
                            droneStationSc.availableDrons = new List<DronBehavior>();
                            break;
                    }

                    //items
                    int itemsCount = platformSc.itemOnPlatform.Length;
                    ItemData[] items = new ItemData[itemsCount];
                    for (int i2 = 0; i2 < itemsCount; i2++)
                    {
                        //Debug.Log("savin gitem:" + res +" qua:"+ platformSc.ItemOnPlatform[(int)res].qua + " in:" + child.name);
                        items[i2] = new ItemData
                            (
                            (Res)i2,
                            platformSc.itemOnPlatform[i2].qua,
                            platformSc.itemOnPlatform[i2].maxQua,
                            platformSc.itemOnPlatform[i2].canIn,
                            platformSc.itemOnPlatform[i2].canOut
                            );
                    }

                    //declarate
                    BuildingsData.PlatformData platform = new BuildingsData.PlatformData
                        (
                        (int)objType,
                        objPos.x,
                        objPos.y,
                        WorldGrid.GetSquare(objPos.x, objPos.y).health,
                        items,
                        platformSc.startTaskTime,
                        platformSc.timeToEndCraft
                        );

                    //veribals
                    int[] veribals = new int[0];
                    if (AllRecipes.instance.IsObjHaveCrafterNeedFuelSc(objType))
                    {
                        veribals = new int[2];
                        CrafterNeedFuel crafterFuel = child.GetComponent<CrafterNeedFuel>();
                        platform.useRecipe = crafterFuel.nowUseRecipeNumber;
                        veribals[0] = (int)crafterFuel.useFuel;
                        veribals[1] = (int)(crafterFuel.percRemFuel * 100f);
                    }
                    else if (AllRecipes.instance.IsObjHaveCrafterNeedEnergySc(objType))
                    {
                        veribals = new int[1];
                        CrafterNeedEnergy crafterEnergy = child.GetComponent<CrafterNeedEnergy>();
                        platform.useRecipe = crafterEnergy.nowUseRecipeNumber;
                        veribals[0] = (int)(child.GetComponent<ElectricityUser>().actCharge * 100f);
                    }
                    else
                    {
                        switch (objType)
                        {
                            case Obj.Farm:
                                Farm farmrSc = child.GetComponent<Farm>();
                                platform.useRecipe = farmrSc.nowUseRecipeNumber;
                                veribals = new int[4];
                                veribals[0] = farmrSc.FoundPlaceToPlant.x;
                                veribals[1] = farmrSc.FoundPlaceToPlant.y;
                                veribals[2] = farmrSc.FoundPlaceToCollect.x;
                                veribals[3] = farmrSc.FoundPlaceToCollect.y;
                                break;

                            case Obj.Woodcuter:
                                WoodCuter woodCuterSc = child.GetComponent<WoodCuter>();
                                veribals = new int[2];
                                veribals[0] = woodCuterSc.TreePosition.x;
                                veribals[1] = woodCuterSc.TreePosition.y;
                                break;

                            case Obj.Planter:
                                Planter planterSc = child.GetComponent<Planter>();
                                veribals = new int[2];
                                veribals[0] = planterSc.TreePosition.x;
                                veribals[1] = planterSc.TreePosition.y;
                                break;

                            case Obj.Battery:
                                Battery batterySc = child.GetComponent<Battery>();
                                veribals = new int[1];
                                veribals[0] = (int)(batterySc.charge * 100f);
                                break;

                            case Obj.CombustionGenerator:
                            case Obj.SteamGenerator:
                                veribals = new int[1];
                                veribals[0] = (int)(child.GetComponent<ElectricityUser>().actCharge * 100f);
                                break;
                        }
                    }
                    
                    switch (platformSc.usingGuiType)
                    {
                        case PlatfotmGUIType.Procesing:
                            platform.startTaskTime = platformSc.startTaskTime;
                            break;
                        case PlatfotmGUIType.Storage:
                            veribals = new int[1];
                            veribals[0] = platformSc.canDronesGetRes ? 1 : 0;
                            break;
                        case PlatfotmGUIType.Turret:
                            Turret turretSc = child.GetComponent<Turret>();
                            veribals = new int[4];
                            veribals[0] = turretSc.nowResiBulet;
                            veribals[1] = (int)(turretSc.avaShootTime * 100.00f);
                            veribals[2] = (int)(turretSc.TurretUp.rotation.eulerAngles.z * 100.00f);
                            if (child.TryGetComponent(out ElectricityUser eleUser)) { veribals[3] = (int)(eleUser.actCharge * 100f); }
                            break;
                    }
                    platform.veribals = veribals;

                    //requested items
                    platform.keepAmountOfRequestedItems = platformSc.keepAmountOfRequestedItems;
                    platform.requestItems = platformSc.requestItems.ToArray();

                    //save
                    platforms[i] = platform;
                }
                else
                {
                    Debug.Log("ERROR! Saving.. tag " + child.tag + " was not defined in saving script! Object: " + child.name);
                }
            }

            //connections
            BuildingsData.ConnectionData[] connections = new BuildingsData.ConnectionData[connectionsSc.Count];
            for (int i = 0; i < connectionsSc.Count; i++)
            {
                RoadBehavior savingRoadSc = connectionsSc[i];

                BuildingsData.ConnectionData savingRoad = new BuildingsData.ConnectionData
                    (
                    (int)savingRoadSc.type,
                    savingRoadSc.startRoadPoint.x,
                    savingRoadSc.startRoadPoint.y,
                    savingRoadSc.endRoadPoint.x,
                    savingRoadSc.endRoadPoint.y,
                    savingRoadSc.sendOff,
                    savingRoadSc.avalibleTiemToSend,
                    savingRoadSc.priority,
                    savingRoadSc.GetMovingRes(),
                    savingRoadSc.GetMovingResX(),
                    savingRoadSc.GetMovingResY()
                    );

                connections[i] = savingRoad;
            }

            //object plans
            BuildingsData.ObjectPlanData[] objectPlans = new BuildingsData.ObjectPlanData[bordersSc.Count];
            for (int i = 0; i < bordersSc.Count; i++)
            {
                ObjectPlan savingPlanSc = bordersSc[i];
                ItemRAQ[] ni = new ItemRAQ[savingPlanSc.needItems.Count];
                for (int i2 = 0; i2 < savingPlanSc.needItems.Count; i2++) { ni[i2] = savingPlanSc.needItems[i2]; }

                ItemRAQ[] ki = new ItemRAQ[savingPlanSc.keptItems.Count];
                for (int i2 = 0; i2 < savingPlanSc.keptItems.Count; i2++) { ki[i2] = savingPlanSc.keptItems[i2]; }
                BuildingsData.ObjectPlanData savingPlan = new BuildingsData.ObjectPlanData
                    (
                    (int)savingPlanSc.objName,
                    (int)savingPlanSc.planType,
                    savingPlanSc.startRoadPoint.x,
                    savingPlanSc.startRoadPoint.y,
                    savingPlanSc.endRoadPoint.x,
                    savingPlanSc.endRoadPoint.y,
                    ni,
                    ki
                    );

                objectPlans[i] = savingPlan;
            }

            //Object to build
            ObjToBuild[] OTBs = BuildMenager.instance.GetAllOTB();
            BuildingsData.ObjToBuildData[] OTBDs = new BuildingsData.ObjToBuildData[OTBs.Length];
            for (int i = 0; i < OTBs.Length; i++)
            {
                BuildingsData.ObjToBuildData OTBD = null;
                ObjToBuild OTB = OTBs[i];
                if (OTB != null)
                {
                    OTBD = new BuildingsData.ObjToBuildData(OTB.objectType, OTB.planType, OTB.xTB, OTB.yTB, OTB.neededItems.ToArray());
                    OTBD.StartPointRoadsX = OTB.startPointRoadsX;
                    OTBD.StartPointRoadsY = OTB.startPointRoadsY;
                }
                OTBDs[i] = OTBD;
            }

            BuildingsD = new BuildingsData
                (
                OTBDs,
                platforms,
                connections,
                objectPlans
                );
        }

        //enemy
        EnemyData EnemyD;
        {
            EnemyD = new EnemyData
                (
                EnemyControler.instance.attackTime,
                EnemyControler.instance.baseDevelopTime,
                EnemyControler.instance.waves.ToArray(),
                EnemyControler.instance.savesWaves.ToArray(),
                EnemyControler.instance.GetEBaseData()
                );
        }

        //units
        UnitsData UnitsD;
        {
            //drons
            int dronsCount = DronControler.instance.transform.childCount;
            UnitsData.DroneData[] drons = new UnitsData.DroneData[dronsCount];
            for (int i = 0; i < dronsCount; i++)
            {
                GameObject child = DronControler.instance.transform.GetChild(i).gameObject;
                TagsEnum tag = TagToTagEnum(child.tag);
                if (tag != TagsEnum.Dron)
                { Debug.Log(child.name + " have a wrong tag! need Dron"); return null; }
                DronBehavior dronSc = child.GetComponent<DronBehavior>();
                drons[i] = dronSc.GetDronData();
            }

            //bullets
            int bulletsCount = BulletManager.instance.transform.childCount;
            UnitsData.BulletData[] bullets = new UnitsData.BulletData[bulletsCount];
            for (int i = 0; i < bulletsCount; i++)
            {
                Transform BT = BulletManager.instance.transform.GetChild(i);
                BulletBehavior BBSc = BT.GetComponent<BulletBehavior>();
                Vector2 target;
                if (BBSc.TargetT == null) { target = new Vector2(); Debug.Log("Saving: BulletBehavior is missing!"); }
                else { target = BBSc.TargetT.position / 10; }
                if (BBSc == null) { return null; }
                UnitsData.BulletData bullet = new UnitsData.BulletData
                (
                    BT.position.x,
                    BT.position.y,
                    BT.rotation.eulerAngles.z,
                    (int)BBSc.type,
                    (int)target.x,
                    (int)target.y
                );
                bullets[i] = bullet;
            }

            UnitsD = new UnitsData
                (
                drons,
                bullets
                );
        }


        WorldData data = new WorldData(GeneralD, PlayerD, TerrainD, BuildingsD, EnemyD, UnitsD);
        return data;
    }
}