using UnityEngine;


// MAIN DATA CLASS

[System.Serializable]
public class WorldData
{
    public GeneralWorldData General;
    public PlayerData Player;
    public TerrainData Terrain;
    public BuildingsData Buildings;
    public EnemyData Enemy;
    public UnitsData Units;

    public WorldData(GeneralWorldData general, PlayerData player, TerrainData terrain, BuildingsData buildings, EnemyData enemy, UnitsData units)
    {
        General = general;
        Player = player;
        Terrain = terrain;
        Buildings = buildings;
        Enemy = enemy;
        Units = units;
    }
}


[System.Serializable]
public class GeneralWorldData
{
    //info
    public string saveName;
    public string version;
    public GameState gameType;
    public string date;
    public string author;

    //map
    public int mapWidth;
    public int mapHeight;
    public int seed;
    public float worldTime;
    public Difficulty difficulty = Difficulty.Peaceful;
    public int transportTime = 120;
    public int colonyCooldown = 30;
    public Res[] detectedRes = new Res[0];

    [Range(0, 2)] public float mountainsSizes = 1f;
    [Range(0, 2)] public float sandSizes = 1f;

    [Range(0, 2)] public float oreSizes = 1f;
    [Range(0, 100)] public int copperFreqOfAppear = 20;
    [Range(0, 100)] public int ironFreqOfAppear = 20;
    [Range(0, 100)] public int coalFreqOfAppear = 10;

    [Range(0, 2)] public float forestSizes = 1f;
    [Range(0, 2)] public float plantingSizes = 1f;
    [Range(0, 100)] public int waterSourceFreqOfAppear = 2;
    [Range(0, 100)] public int oilSourceFreqOfAppear = 2;


    public GeneralWorldData(string saveName, string version, GameState gameType, string date, string author, int mapWidth, int mapHeight, int seed, float worldTime, Difficulty difficulty)
    {
        this.saveName = saveName;
        this.version = version;
        this.gameType = gameType;
        this.date = date;
        this.author = author;

        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.seed = seed;
        this.worldTime = worldTime;

        this.difficulty = difficulty;

        detectedRes = new Res[0];
        transportTime = 120;
        colonyCooldown = 30;
    }
}

[System.Serializable]
public class PlayerData
{
    //camera
    public float cameraX;
    public float cameraY;
    public float cameraScale;

    public bool choosedStartPlace = true;

    public PlayerData(float cameraX, float cameraY, float cameraScale, bool choosedStartPlace)
    {
        this.cameraX = cameraX;
        this.cameraY = cameraY;
        this.cameraScale = cameraScale;
        this.choosedStartPlace = choosedStartPlace;
    }
}

[System.Serializable]
public class TerrainData
{
    public int seed;
    public short[] terrainTiles;
    public short[] squaresVeribal; //parametr;  moutains = -1000;
    public ObjToGrow[] objToGrows;

    public TerrainData(int seed, short[] terrainTiles, short[] squaresVeribal, ObjToGrow[] objToGrows)
    {
        this.seed = seed;
        this.terrainTiles = terrainTiles;
        this.squaresVeribal = squaresVeribal;
        this.objToGrows = objToGrows;
    }

    [System.Serializable]
    public class TerrainObjectData
    {
        public int objectAndVeribal;
        public int x;
        public int y;

        public TerrainObjectData(int objectAndVeribal, int x, int y)
        {
            this.objectAndVeribal = objectAndVeribal;
            this.x = x;
            this.y = y;
        }
    }
}

[System.Serializable]
public class BuildingsData
{
    public ObjToBuildData[] objectsToBuild;
    public PlatformData[] platforms;
    public ConnectionData[] connections;
    public ObjectPlanData[] objectsPlan;

    public BuildingsData(ObjToBuildData[] objectsToBuild, PlatformData[] platforms, ConnectionData[] connections, ObjectPlanData[] objectsPlan)
    {
        this.objectsToBuild = objectsToBuild;
        this.platforms = platforms;
        this.connections = connections;
        this.objectsPlan = objectsPlan;
    }

    [System.Serializable]
    public class PlatformData
    {
        public int obj;
        public int x;
        public int y;
        public int health;
        public ItemData[] items;

        public float startTaskTime;
        public int useRecipe;
        public int[] veribals;

        public int membership;

        public bool keepAmountOfRequestedItems;
        public ItemRAQ[] requestItems;

        public PlatformData(int _obj, int _x, int _y, int _health, ItemData[] _items)
        {
            obj = _obj;
            x = _x;
            y = _y;
            health = _health;
            items = _items;
            veribals = new int[0];
            keepAmountOfRequestedItems = false;
            requestItems = new ItemRAQ[0];
        }
    }

    [System.Serializable]
    public class ConnectionData
    {
        public int type;
        public int sx;
        public int sy;
        public int ex;
        public int ey;
        public bool sendOff;
        public float avalibleTiemToSend;
        public int priority;
        public int[] movingRess;
        public float[] ressX;
        public float[] ressY;

        public int membership;

        public ConnectionData(int _type, int _sx, int _sy, int _ex, int _ey, bool _sendOff, float _avalibleTiemToSend, int _priority, int[] _movingRess, float[] _ressX, float[] _ressY)
        {
            type = _type;
            sx = _sx;
            sy = _sy;
            ex = _ex;
            ey = _ey;
            sendOff = _sendOff;
            avalibleTiemToSend = _avalibleTiemToSend;
            priority = _priority;
            movingRess = _movingRess;
            ressX = _ressX;
            ressY = _ressY;
        }
    }

    [System.Serializable]
    public class ObjectPlanData
    {
        public int objType;
        public int planType;
        public int sx;
        public int sy;
        public int ex;
        public int ey;
        public ItemRAQ[] needItems;
        public ItemRAQ[] keepItems;

        public int membership;

        public ObjectPlanData(int _objType, int _planType, int _sx, int _sy, int _ex, int _ey, ItemRAQ[] _needItems, ItemRAQ[] _keppItems)
        {
            objType = _objType;
            planType = _planType;
            sx = _sx;
            sy = _sy;
            ex = _ex;
            ey = _ey;
            needItems = _needItems;
            keepItems = _keppItems;
        }
    }

    [System.Serializable]
    public class ObjToBuildData
    {
        public Obj objectType;
        public ObjectPlanType planType;
        public int xTB;
        public int yTB;
        public ItemRAQ[] neededItems;
        public int StartPointRoadsX;
        public int StartPointRoadsY;
        public ObjToBuildData(Obj _objectType, ObjectPlanType _planType, int xtb, int ytb, ItemRAQ[] _NeededItems)
        {
            objectType = _objectType;
            planType = _planType;
            xTB = xtb;
            yTB = ytb;
            neededItems = _NeededItems;
        }
    }
}

[System.Serializable]
public class EnemyData
{
    public float attackTime;
    public float baseDevelopTime;
    public WaveOfEnemyData[] eWaves;
    public WaveOfEnemyData[] eSavesWaves;
    public EnemyBaseData[] eBasesControlers;

    public EnemyData(float attackTime, float baseDevelopTime, WaveOfEnemyData[] eWaves, WaveOfEnemyData[] eSavesWaves, EnemyBaseData[] eBasesControlers)
    {
        this.attackTime = attackTime;
        this.baseDevelopTime = baseDevelopTime;
        this.eWaves = eWaves;
        this.eSavesWaves = eSavesWaves;
        this.eBasesControlers = eBasesControlers;
    }

    

    [System.Serializable]
    public class EnemyBaseData
    {
        public float avaDevelopTime;
        public EnemyBuildingData[] buildings;

        public int[] developPosX;
        public int[] developPosY;
        public int[] wallsX;
        public int[] wallsY;
        public int[] wallsToBuildX;
        public int[] wallsToBuildY;
        public int[] wallsToRemoveX;
        public int[] wallsToRemoveY;

        public EnemyUnitData[] units;

        public EnemyBaseData(float avaDevelopTime, EnemyBuildingData[] buildings, Vector2Int[] developPos, Vector2Int[] walls, Vector2Int[] wallsToBuild, Vector2Int[] wallsToRemove, EnemyUnitData[] units)
        {
            this.avaDevelopTime = avaDevelopTime;
            this.buildings = buildings;

            developPosX = new int[developPos.Length];
            developPosY = new int[developPos.Length];
            for (int i = 0; i < developPos.Length; i++) { developPosX[i] = developPos[i].x; developPosY[i] = developPos[i].y; }

            wallsX = new int[walls.Length];
            wallsY = new int[walls.Length];
            for (int i = 0; i < walls.Length; i++) { wallsX[i] = walls[i].x; wallsY[i] = walls[i].y; }

            wallsToBuildX = new int[wallsToBuild.Length];
            wallsToBuildY = new int[wallsToBuild.Length];
            for (int i = 0; i < wallsToBuild.Length; i++) { wallsToBuildX[i] = wallsToBuild[i].x; wallsToBuildY[i] = wallsToBuild[i].y; }

            wallsToRemoveX = new int[wallsToRemove.Length];
            wallsToRemoveY = new int[wallsToRemove.Length];
            for (int i = 0; i < wallsToRemove.Length; i++) { wallsToRemoveX[i] = wallsToRemove[i].x; wallsToRemoveY[i] = wallsToRemove[i].y; }

            this.units = units;
        }
    }

    [System.Serializable]
    public class EnemyBuildingData
    {
        public int type;
        public int x;
        public int y;
        public int health;

        public int[] connectionsInX;
        public int[] connectionsInY;
        public int[] connectionsOutX;
        public int[] connectionsOutY;

        public float timeToShoot;
        public float dir;

        public EnemyBuildingData(int _type, int _x, int _y, int _health, Vector2Int[] _connectionsIn, Vector2Int[] _connectionsOut)
        {
            type = _type;
            x = _x;
            y = _y;
            health = _health;

            connectionsInX = new int[_connectionsIn.Length];
            connectionsInY = new int[_connectionsIn.Length];
            for (int i = 0; i < _connectionsIn.Length; i++) { connectionsInX[i] = _connectionsIn[i].x; connectionsInY[i] = _connectionsIn[i].y; }

            connectionsOutX = new int[_connectionsOut.Length];
            connectionsOutY = new int[_connectionsOut.Length];
            for (int i = 0; i < _connectionsOut.Length; i++) { connectionsOutX[i] = _connectionsOut[i].x; connectionsOutY[i] = _connectionsOut[i].y; }
        }
    }

    [System.Serializable]
    public class EnemyUnitData
    {
        public int type;
        public float x;
        public float y;
        public float rotate;
        public int task;
        public int nextTask;
        public int health;
        public float targetX;
        public float targetY;
        public float spawnX = -1;
        public float spawnY = -1;
        public int[] pathX = new int[0];
        public int[] pathY = new int[0];

        public EnemyUnitData(int type, float x, float y, float rotate, int task, int nextTask, int health, float targetX, float targetY)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            this.rotate = rotate;
            this.task = task;
            this.nextTask = nextTask;
            this.health = health;
            this.targetX = targetX;
            this.targetY = targetY;
        }
    }
}

[System.Serializable]
public class UnitsData
{
    public DroneData[] drons;
    public BulletData[] bullets;

    public UnitsData(DroneData[] drons, BulletData[] bullets)
    {
        this.drons = drons;
        this.bullets = bullets;
    }

    [System.Serializable]
    public class DroneData
    {
        public float dSPositinX;
        public float dSPositinY;
        public bool isFlying;
        public float dronX;
        public float dronY;
        public float targetX;
        public float targetY;
        public int pttpux;
        public int pttpuy;
        public int pttpdx;
        public int pttpdy;
        public Res resToPutUp;
        public Res keptRes;
        public BuildingsData.ObjToBuildData ObjToBuild;
        public int[] dronTasks;

        public int type;
        public float timeTo;
        public int membership;

        public DroneData
            (
            float _dSPositinX, float _dSPositinY, bool _isFlying, float _dronX, float _dronY, float _targetX, float _targetY,
            Vector2Int _pttpu, Vector2Int _pttpd,
            Res _resToPutUp, Res _keptRes, BuildingsData.ObjToBuildData _ObjToBuild, int[] _dronTasks
            )
        {

            dSPositinX = _dSPositinX;
            dSPositinY = _dSPositinY;
            isFlying = _isFlying;
            dronX = _dronX;
            dronY = _dronY;
            targetX = _targetX;
            targetY = _targetY;

            pttpux = _pttpu.x;
            pttpuy = _pttpu.y;
            pttpdx = _pttpd.x;
            pttpdy = _pttpd.y;

            resToPutUp = _resToPutUp;
            keptRes = _keptRes;
            ObjToBuild = _ObjToBuild;
            dronTasks = _dronTasks;
        }
    }

    [System.Serializable]
    public struct BulletData
    {
        public float myX;
        public float myY;
        public float rotate;
        public int bulletE;
        public int targetX;
        public int targetY;

        public BulletData(float _myX, float _myY, float _rotate, int _bulletE, int _targetX, int _targetY)
        {
            myX = _myX;
            myY = _myY;
            rotate = _rotate;
            bulletE = _bulletE;
            targetX = _targetX;
            targetY = _targetY;
        }
    }
}


// OTHER

[System.Serializable]
public struct ItemData
{
    public Res res;
    public int qua;
    public int maxQua;
    public bool canIn;
    public bool canOut;

    public ItemData(Res res, int qua, int maxQua, bool canIn, bool canOut)
    {
        this.res = res;
        this.qua = qua;
        this.maxQua = maxQua;
        this.canIn = canIn;
        this.canOut = canOut;
    }
}

[System.Serializable]
public struct WaveOfEnemyData
{
    public float timeToLaunch;
    public int[] type;
    public int[] number;

    public WaveOfEnemyData(float _timeToLaunch, int[] _type, int[] _number)
    {
        timeToLaunch = _timeToLaunch;
        type = _type;
        number = _number;
    }
}