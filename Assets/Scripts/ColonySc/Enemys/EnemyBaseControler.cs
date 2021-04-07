using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseControler : MonoBehaviour
{
    class EnemyBuilding
    {
        public Obj obj;
        public int x;
        public int y;
        public List<Vector2Int> conIn = new List<Vector2Int>();
        public List<Vector2Int> conOut = new List<Vector2Int>();

        public EnemyBuilding(Obj _obj, int _x, int _y)
        {
            obj = _obj;
            x = _x;
            y = _y;
        }
    }
    class PathAndTarget
    {
        public Vector2Int startPos;
        public Transform targetT;
        public List<Vector2Int> path;
        public float updateTime;

        public PathAndTarget(Vector2Int startPos, Transform targetT, List<Vector2Int> path)
        {
            this.startPos = startPos;
            this.targetT = targetT;
            this.path = path;
            updateTime = WorldMenager.instance.worldTime;
        }
    }

    private List<EnemyBuilding> buildings = new List<EnemyBuilding>();
    public  List<Vector2Int> toDevelop = new List<Vector2Int>();
    private Vector2Int mapCenter;
    public Vector2Int baseCenterPos;

    private Transform BuildingsParent;
    private Transform BuildingsConParent;
    private Transform UnitsParent;

    public float developTime = 120f;
    public float avaDevelopTime;
    public int coreCount = 0;
    public int turretCount = 0;
    public int spawnerCount = 0;

    private List<Vector2Int> wallsToBuild = new List<Vector2Int>();
    private List<Vector2Int> wallsToRemove = new List<Vector2Int>();
    public List<Vector2Int> walls = new List<Vector2Int>();

    private List<EUnitFlying> unitsFList = new List<EUnitFlying>();
    private List<EUnitMovement> unitsTList = new List<EUnitMovement>();
    private List<EUnitMovement> unitsBList = new List<EUnitMovement>();
    private List<EUnitMovement> unitsRList = new List<EUnitMovement>();

    private int[,] sTab;
    private Vector2Int targetPos;
    public List<Vector3Int> squaresToUpdate;
    public List<Vector2Int> pathSquers;
    public float agro = 1f;
    private float tendencToDestroy = 1f;
    private List<EUnitMovement> queTOGetNewTarget = new List<EUnitMovement>();
    private int framTogetNewTarget = 0;
    private List<PathAndTarget> lastPaths = new List<PathAndTarget>();

    private void Awake()
    {
        BuildingsParent = new GameObject().transform;
        BuildingsParent.transform.parent = transform;
        BuildingsParent.name = "Buildings";

        BuildingsConParent = new GameObject().transform;
        BuildingsConParent.transform.parent = transform;
        BuildingsConParent.name = "BuildingsCon";

        UnitsParent = new GameObject().transform;
        UnitsParent.transform.parent = transform;
        UnitsParent.name = "Units";

        coreCount = 0;
        turretCount = 0;
        spawnerCount = 0;

        if (agro <= 0) { tendencToDestroy = 10000f; }
        else { tendencToDestroy = 1f / agro; }
        
        mapCenter = new Vector2Int(WorldMenager.instance.mapSize.x / 2, WorldMenager.instance.mapSize.y / 2);
    }

    private void Update()
    {
        if (queTOGetNewTarget.Count > 0)
        {
            framTogetNewTarget--;
            if (framTogetNewTarget <= 0)
            {
                framTogetNewTarget = 10;
                SetPathInUnit(queTOGetNewTarget[0]);
                queTOGetNewTarget.RemoveAt(0);
            }
        }      
    }

    private void IncreaseBase()
    {
        bool foundbase;
        for (int ii = 0; ii < buildings.Count; ii++)
        {
            Vector2Int actPlatPos = toDevelop[0];
            toDevelop.RemoveAt(0);
            toDevelop.Add(actPlatPos);
            foundbase = false;
            foreach (EnemyBuilding actBuilding in buildings)
            {
                if (actBuilding.x == actPlatPos.x && actBuilding.y == actPlatPos.y)
                {
                    foundbase = true;
                    if (actBuilding.conOut.Count >= 3) { break; }

                    Vector2Int tDir = EnemyControler.instance.GetDir(actPlatPos.x, actPlatPos.y, mapCenter.x, mapCenter.y);
                    Vector2Int mainDir = tDir;
                    int sp; if (Random.Range(0, 2) == 0) { sp = -1; } else { sp = 1; }
                    for (int i = 0; i < 3; i++)
                    {
                        //set dir
                        if (i > 0)
                        {
                            if (tDir.x == 0) { mainDir.x = sp; mainDir.y = 0; }
                            else if (tDir.y == 0) { mainDir.x = 0; mainDir.y = sp; }
                            else { mainDir.x = sp * -1; mainDir.y = sp; }
                            sp *= -1;
                        }

                        Vector2Int newPos = mainDir + new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2)) + actPlatPos;
                        if (ItIsSpace(newPos.x, newPos.y))
                        {
                            EnemyBuilding newBuilding = CreateNewPlatform(newPos.x, newPos.y);
                            newBuilding.conIn.Add(actPlatPos);
                            actBuilding.conOut.Add(newPos);
                            BuildConnection(actPlatPos, newPos);
                            return;
                        }
                    }
                    break;
                }
            }
            if (foundbase == false) { toDevelop.Remove(actPlatPos); }
        }

        bool ItIsSpace(int x, int y)
        {
            if (WorldMenager.instance.GetSquer(x, y) != Obj.EnemysTerrain) { return false; }
            for (int ix = -1; ix <= 1; ix++)
            {
                for (int iy = -1; iy <= 1; iy++)
                {
                    if(ix == 0 && iy == 0) { continue; }
                    if(WorldMenager.instance.GetSquer(x + ix, y + iy) != Obj.EnemysTerrain) { return false; }
                }
            }
            return true;
        }
    }
    private void DevelopBase()
    {
        float timeToInvoke = developTime;
        if (turretCount <= 0) { timeToInvoke = developTime / 10f; }
        avaDevelopTime = WorldMenager.instance.worldTime + timeToInvoke;
        Invoke("DevelopBase", timeToInvoke);

        int builingCount = buildings.Count;
        int coreMaxIndex = (int)(builingCount * 0.3);
        int turrMinIndex = (int)(builingCount * 0.85);
        int spawnerMaxCount = (int)(builingCount * 0.1);
        EnemyBuilding EB;

        //build wall
        if (wallsToBuild.Count > 0)
        {
            BuildEBuilding(Obj.EnemyWall, wallsToBuild[0].x, wallsToBuild[0].y);
            wallsToBuild.RemoveAt(0);
            return;
        }

        //remove wall
        if (wallsToRemove.Count > 0)
        {
            WorldMenager.instance.RemoveObjFromPos(wallsToRemove[0].x, wallsToRemove[0].y);
            wallsToRemove.RemoveAt(0);
            return;
        }

        //turret
        for (int i = builingCount - 1; i > turrMinIndex; i--)
        {
            EB = buildings[i];
            if (EB.obj != Obj.EnemyTurret) { ChangeBuildingType(EB, Obj.EnemyTurret); return; }
        }

        //clear turret
        for (int i = 0; i < turrMinIndex; i++)
        {
            EB = buildings[i];
            if (EB.obj == Obj.EnemyTurret) { ChangeBuildingType(EB, Obj.EnemyPlatform); return; }
        }

        //core
        for (int i = 0; i < coreMaxIndex; i++)
        {
            EB = buildings[i];
            if (EB.obj == Obj.EnemyPlatform) { ChangeBuildingType(EB, Obj.EnemyCore); return; }
        }

        //spawner
        if (spawnerCount < spawnerMaxCount)
        {
            int loseIndex = Random.Range(coreMaxIndex + 1, turrMinIndex);
            for (int i = loseIndex; i < turrMinIndex; i++)
            {
                EB = buildings[i];
                if (EB.obj == Obj.EnemyPlatform) { ChangeBuildingType(EB, Obj.EnemySpawner); return; }
            }
            for (int i = coreMaxIndex + 1; i < loseIndex; i++)
            {
                EB = buildings[i];
                if (EB.obj == Obj.EnemyPlatform) { ChangeBuildingType(EB, Obj.EnemySpawner); return; }
            }
        }

        IncreaseBase();
    }

    //get
    private EnemyBuilding GetEnemyBuilding(int x, int y)
    {
        foreach (EnemyBuilding EB in buildings)
        {
            if (EB.x == x && EB.y == y)
            {
                return EB;
            }
        }
        return null;
    }
    public Vector2Int GetNearestEnemyBuilding(int mx, int my)
    {
        Vector2Int nBuilding = new Vector2Int(-1, -1);
        int minDist = int.MaxValue;
        int dist;
        int deltaX;
        int deltaY;

        foreach (Vector2Int pos in toDevelop)
        {
            deltaX = pos.x - mx; if (deltaX < 0) { deltaX *= -1; }
            deltaY = pos.y - my; if (deltaY < 0) { deltaY *= -1; }
            dist = deltaX + deltaY;
            if (dist < minDist)
            {
                nBuilding = pos;
                minDist = dist;
            }
        }
        foreach (Vector2Int pos in walls)
        {
            deltaX = pos.x - mx; if (deltaX < 0) { deltaX *= -1; }
            deltaY = pos.y - my; if (deltaY < 0) { deltaY *= -1; }
            dist = deltaX + deltaY;
            if (dist < minDist)
            {
                nBuilding = pos;
                minDist = dist;
            }
        }
        
        return nBuilding;
    }
    public Vector2Int[] GetAllPoss(Obj obj)
    {
        List<Vector2Int> poss = new List<Vector2Int>();
        int number = -1;
        switch (obj)
        {
            case Obj.EnemySpawner: number = spawnerCount; break;
            case Obj.EnemyTurret: number = turretCount; break;
            case Obj.EnemyCore: number = coreCount; break;
        }
        if (number == -1) { return poss.ToArray(); }

        foreach (EnemyBuilding EB in buildings)
        {
            if (EB.obj == obj)
            {
                poss.Add(new Vector2Int(EB.x, EB.y));
                number--;
                if (number <= 0) { break; }
            }
        }
        return poss.ToArray();
    }
    public Transform GetNearestEUnit(Vector2 sPos)
    {
        Transform nearestT = null;
        float minDist = float.MaxValue;
        float dx, dy, dist;
        //tank
        foreach (EUnitMovement unit in unitsTList)
        {
            dist = CalcDist(unit.transform.position);
            if (dist < minDist) { minDist = dist; nearestT = unit.transform; }
        }
        //ball
        foreach (EUnitMovement unit in unitsBList)
        {
            dist = CalcDist(unit.transform.position);
            if (dist < minDist) { minDist = dist; nearestT = unit.transform; }
        }
        //flaying
        foreach (EUnitFlying unit in unitsFList)
        {
            dist = CalcDist(unit.transform.position);
            if (dist < minDist) { minDist = dist; nearestT = unit.transform; }
        }
        //range
        foreach (EUnitMovement unit in unitsTList)
        {
            dist = CalcDist(unit.transform.position);
            if (dist < minDist) { minDist = dist; nearestT = unit.transform; }
        }

        return nearestT;

        float CalcDist(Vector2 ePos)
        {
            dx = ePos.x - sPos.x;
            dy = ePos.y - sPos.y;
            return dx * dx + dy * dy;
        }
    }

    //unit
    public GameObject SpawnEnemyUnit(EnemyType type, Vector2 pos, float dir)
    {
        GameObject unit = EnemyControler.instance.GetEnemyPrefabfromType(type);
        if (unit == null) { return null; }

        unit = Instantiate(unit, pos, Quaternion.Euler(0f, 0f, dir));
        unit.transform.parent = UnitsParent;

        switch (type)
        {
            case EnemyType.Dif: break;
            case EnemyType.Tank:   EUnitMovement scT = unit.GetComponent<EUnitMovement>();   if (scT != null) { unitsTList.Add(scT); scT.myEBC = this; scT.myType = type; } break;
            case EnemyType.Ball:   EUnitMovement scB = unit.GetComponent<EUnitMovement>();   if (scB != null) { unitsBList.Add(scB); scB.myEBC = this; scB.myType = type; } break;
            case EnemyType.Range:  EUnitMovement scR = unit.GetComponent<EUnitMovement>();   if (scR != null) { unitsRList.Add(scR); scR.myEBC = this; scR.myType = type; } break;
            case EnemyType.Flying: EUnitFlying scF = unit.GetComponent<EUnitFlying>();       if (scF != null) { unitsFList.Add(scF); scF.myEBC = this; scF.myType = type; } break;
        }
        return unit;
    }
    public void AddToQueToGetNewTarget(EUnitMovement EUM)
    {
        if (queTOGetNewTarget.Contains(EUM) == false) { queTOGetNewTarget.Add(EUM); }
    }
    private void SetPathInUnit(EUnitMovement EUM)
    {
        if (EUM == null) { return; }

        //fix
        int lenght = lastPaths.Count;
        int mi = 0;
        float time = WorldMenager.instance.worldTime - 5f;
        for (int i = 0; i < lenght; i++)
        {
            int index = i - mi;
            PathAndTarget pathAT = lastPaths[index];
            if(pathAT.targetT==null || pathAT.updateTime < time) { mi++; lastPaths.RemoveAt(index); continue; }
        }

        //check
        Vector2Int unitPos = WorldMenager.instance.GetTabPos(EUM.transform.position);
        foreach (PathAndTarget pathAT in lastPaths)
        {
            if (pathAT.startPos == unitPos)
            {
                Debug.Log("I found an old path. I'm sending a unit");
                EUM.SetToMainAttack(pathAT.targetT, RewriteThePath(pathAT.path));
                return;
            }
        }

        //found new path
        FindNewTargetAndAddToList(unitPos);
        Transform targetT = WorldMenager.instance.GetTransforOfObj(targetPos.x, targetPos.y);
        if (targetT == null) { return; }
        EUM.SetToMainAttack(targetT, RewriteThePath(pathSquers));
    }
    public Transform GetNearestPlatform(Vector2Int sPos)
    {
        List<Vector3Int> targets = new List<Vector3Int>();
        int maxRange = 40;
        int sx, sy;
        int hr = 1;
        for (; hr <= maxRange; hr++)
        {
            sx = sPos.x - hr + 1;
            sy = sPos.y + hr;
            int tmp = sx + hr * 2;
            for (; sx < tmp; sx++) { CheckAndSet(); }
            sx--; sy--;
            tmp = sy - hr * 2;
            for (; sy > tmp; sy--) { CheckAndSet(); }
            sx--; sy++;
            tmp = sx - hr * 2;
            for (; sx > tmp; sx--) { CheckAndSet(); }
            sx++; sy++;
            tmp = sy + hr * 2;
            for (; sy < tmp; sy++) { CheckAndSet(); }

            if (targets.Count > 0) { break; }
        }

        if (targets.Count == 0) { return null; }

        Vector3Int nearest = targets[0];
        for (int i = 1; i < targets.Count; i++)
        {
            if(targets[i].z < nearest.z) { nearest = targets[i]; }
        }
        targets = new List<Vector3Int> { nearest };

        int dx = Mathf.Abs(targets[0].x - sPos.x);
        int dy = Mathf.Abs(targets[0].y - sPos.y);
        int ar;
        if (dx < dy) { ar = dx; } else { ar = dy; }
        ar = (int)(ar * 0.4f);

        maxRange = hr + ar;
        for (; hr <= maxRange; hr++)
        {
            sx = sPos.x - hr + 1;
            sy = sPos.y + hr;
            int tmp = sx + hr * 2;
            for (; sx < tmp; sx++) { CheckAndSet(); }
            sx--; sy--;
            tmp = sy - hr * 2;
            for (; sy > tmp; sy--) { CheckAndSet(); }
            sx--; sy++;
            tmp = sx - hr * 2;
            for (; sx > tmp; sx--) { CheckAndSet(); }
            sx++; sy++;
            tmp = sy + hr * 2;
            for (; sy < tmp; sy++) { CheckAndSet(); }
        }

        nearest = targets[0];
        for (int i = 1; i < targets.Count; i++)
        {
            if (targets[i].z < nearest.z) { nearest = targets[i]; }
        }
        //Debug.Log("mypos: " + startPos + " found: " + nearest);

        return WorldMenager.instance.GetTransforOfObj(nearest.x, nearest.y);

        void CheckAndSet()
        {
            Obj obj = WorldMenager.instance.GetSquer(sx, sy);
            if (AllRecipes.instance.IsItPlatform(obj))
            {
                int odl = (sx - sPos.x) * (sx - sPos.x) + (sy - sPos.y) * (sy - sPos.y);
                Vector3Int t = new Vector3Int(sx, sy, odl);
                targets.Add(t);
            }
        }
    }
    public void RemoveUnitFromList(GameObject eUnit, EnemyType eType)
    {
        EUnitMovement eUM;
        EUnitFlying eUF;
        switch (eType)
        {
            case EnemyType.Ball:   eUM = eUnit.GetComponent<EUnitMovement>(); if (eUM != null) { unitsBList.Remove(eUM); } break;
            case EnemyType.Tank:   eUM = eUnit.GetComponent<EUnitMovement>(); if (eUM != null) { unitsTList.Remove(eUM); } break;
            case EnemyType.Range:  eUM = eUnit.GetComponent<EUnitMovement>(); if (eUM != null) { unitsRList.Remove(eUM); } break;
            case EnemyType.Flying: eUF = eUnit.GetComponent<EUnitFlying>();   if (eUF != null) { unitsFList.Remove(eUF); } break;
        }
    }
    public void TriggerAtack()
    {
        SetBaseCenterPos();

        FindNewTargetAndAddToList(baseCenterPos);

        Transform target = WorldMenager.instance.GetTransforOfObj(targetPos.x, targetPos.y);
        if (target == null) { return; }

        foreach (EUnitMovement item in unitsTList) { if (item.mainTargetT != null) { continue; } item.SetToMainAttack(target, RewriteThePath(pathSquers)); }
        foreach (EUnitMovement item in unitsBList) { if (item.mainTargetT != null) { continue; } item.SetToMainAttack(target, RewriteThePath(pathSquers)); }
        foreach (EUnitMovement item in unitsRList) { if (item.mainTargetT != null) { continue; } item.SetToMainAttack(target, RewriteThePath(pathSquers)); }

        target = GetNearestPlatform(baseCenterPos);
        foreach (EUnitFlying item in unitsFList)   { if (item.targetT != null)     { continue; } item.SetToAttack(target); }
    }

    //set
    void ChangeBuildingType(EnemyBuilding currEB, Obj newType)
    {
        //remove building
        Square square = WorldGrid.GetSquare(currEB.x, currEB.y);
        switch (currEB.obj)
        {
            case Obj.EnemySpawner: spawnerCount--; break;
            case Obj.EnemyCore: coreCount--; break;
            case Obj.EnemyTurret: turretCount--; RemoveWalls(currEB.x, currEB.y); break;
        }
        Destroy(square.trans.gameObject);

        //create new building
        GameObject newBase = BuildEBuilding(newType, currEB.x, currEB.y);
        square.health = AllRecipes.instance.GetMaxHelthOfObj(newType);
        square.trans = newBase.transform;
        square.obj = newType;
        currEB.obj = newType;
        switch (newType)
        {
            case Obj.EnemySpawner: spawnerCount++; break;
            case Obj.EnemyCore: coreCount++; break;
            case Obj.EnemyTurret: turretCount++; CreateWalls(currEB.x, currEB.y); break;
        }

        void CreateWalls(int x, int y)
        {
            Vector2Int dir = EnemyControler.instance.GetDir(x, y, baseCenterPos.x, baseCenterPos.y) * -1;
            int ix = x + dir.x;
            int iy = y + dir.y;
            if (WorldMenager.instance.GetSquer(ix, iy) == Obj.EnemysTerrain) { wallsToBuild.Add(new Vector2Int(ix, iy)); }

            if (dir.x == 0)
            {
                ix = x + 1;
                if (WorldMenager.instance.GetSquer(ix, iy) == Obj.EnemysTerrain) { wallsToBuild.Add(new Vector2Int(ix, iy)); }
                ix = x - 1;
                if (WorldMenager.instance.GetSquer(ix, iy) == Obj.EnemysTerrain) { wallsToBuild.Add(new Vector2Int(ix, iy)); }
            }
            else if(dir.y == 0)
            {
                iy = y + 1;
                if (WorldMenager.instance.GetSquer(ix, iy) == Obj.EnemysTerrain) { wallsToBuild.Add(new Vector2Int(ix, iy)); }
                iy = y - 1;
                if (WorldMenager.instance.GetSquer(ix, iy) == Obj.EnemysTerrain) { wallsToBuild.Add(new Vector2Int(ix, iy)); }
            }
            else
            {
                ix = x;
                iy = y + dir.y;
                if (WorldMenager.instance.GetSquer(ix, iy) == Obj.EnemysTerrain) { wallsToBuild.Add(new Vector2Int(ix, iy)); }
                ix = x + dir.x;
                iy = y;
                if (WorldMenager.instance.GetSquer(ix, iy) == Obj.EnemysTerrain) { wallsToBuild.Add(new Vector2Int(ix, iy)); }
            }

        }
        void RemoveWalls(int x, int y)
        {
            for (int ix = x - 1; ix <= x + 1; ix++)
            {
                for (int iy = y - 1; iy <= y + 1; iy++)
                {
                    if (ix == x && iy == y) { continue; }
                    if (CanRemoveWall(ix, iy)) { wallsToRemove.Add(new Vector2Int(ix, iy)); }
                }
            }
            bool CanRemoveWall(int _x, int _y)
            {
                if(WorldMenager.instance.GetSquer(_x, _y) != Obj.EnemyWall) { return false; }
                int n = 0;
                for (int _ix = _x - 1; _ix <= _x + 1; _ix++)
                {
                    for (int _iy = _y - 1; _iy <= _y + 1; _iy++)
                    {
                        if (_ix == _x && _iy == _y) { continue; }
                        if (WorldMenager.instance.GetSquer(_ix, _iy) == Obj.EnemyTurret) { n++;  if (n >= 2) { return false; } }
                    }
                }
                return true;
            }
        }
    }
    private EnemyBuilding CreateNewPlatform(int x, int y)
    {
        GameObject newBuilding = BuildEBuilding(Obj.EnemyPlatform, x, y);
        EnemyBuilding building = new EnemyBuilding(Obj.EnemyPlatform, x, y);
        buildings.Add(building);
        toDevelop.Add(new Vector2Int(x, y));
        WorldGrid.SetSquare(new Square(Obj.EnemyPlatform, x, y, AllRecipes.instance.GetMaxHelthOfObj(Obj.EnemyPlatform), newBuilding.transform));
        return building;
    }
    public void DisconnectBuilding(int x, int y)
    {
        Obj obj = WorldMenager.instance.GetSquer(x, y);
        //Debug.Log("disconect: " + obj);

        if (obj == Obj.EnemyWall)
        {
            Transform wallT = WorldMenager.instance.GetTransforOfObj(x, y);
            if (wallT == null) { return; }

            walls.Remove(new Vector2Int(x, y));

            if (WorldMenager.instance.GetSquer(x, y + 1) == Obj.EnemyWall) { RemoveConector(x, y, x, y + 1); }
            if (WorldMenager.instance.GetSquer(x, y - 1) == Obj.EnemyWall) { RemoveConector(x, y, x, y - 1); }
            if (WorldMenager.instance.GetSquer(x + 1, y) == Obj.EnemyWall) { RemoveConector(x, y, x + 1, y); }
            if (WorldMenager.instance.GetSquer(x - 1, y) == Obj.EnemyWall) { RemoveConector(x, y, x - 1, y); }

            void RemoveConector(int sx, int sy, int ex, int ey)
            {
                Transform trans = wallT.Find(string.Format("WCon({0},{1})({2},{3})", sx, sy, ex, ey));
                if (trans != null) { Destroy(trans.gameObject); return; }

                Transform nextWallT = WorldMenager.instance.GetTransforOfObj(ex, ey);
                if (nextWallT == null) { return; }
                trans = nextWallT.Find(string.Format("WCon({0},{1})({2},{3})", ex, ey, sx, sy));
                if (trans != null) { Destroy(trans.gameObject); return; }
                Debug.Log("nie moge znaleźć " + string.Format("WCon({0},{1})({2},{3})", sx, sy, ex, ey));
            }
        }
        else
        {
            EnemyBuilding EBuilding = null;

            //remove from list
            toDevelop.Remove(new Vector2Int(x, y));
            foreach (EnemyBuilding EB in buildings)
            {
                if (EB.x == x && EB.y == y)
                {
                    buildings.Remove(EB);
                    EBuilding = EB;
                    break;
                }
            }

            if (EBuilding != null)
            {
                //destroy connection
                Transform conT;
                Vector2Int myPos = new Vector2Int(x, y);
                Vector2Int nextPos;
                EnemyBuilding nextEB;
                for (int i = 0; i < EBuilding.conIn.Count; i++)
                {
                    nextPos = EBuilding.conIn[i];
                    conT = BuildingsConParent.Find(string.Format("Con({0}, {1})({2}, {3})", nextPos.x, nextPos.y, x, y));
                    if (conT == null) { conT = BuildingsConParent.Find(string.Format("Con({0}, {1})({2}, {3})", x, y, nextPos.x, nextPos.y)); }
                    if (conT != null)
                    {
                        Destroy(conT.gameObject);
                        nextEB = GetEnemyBuilding(nextPos.x, nextPos.y);
                        if (nextEB != null)
                        {
                            nextEB.conOut.Remove(myPos);
                        }
                    }
                }
                for (int i = 0; i < EBuilding.conOut.Count; i++)
                {
                    nextPos = EBuilding.conOut[i];
                    conT = BuildingsConParent.Find(string.Format("Con({0}, {1})({2}, {3})", x, y, nextPos.x, nextPos.y));
                    if (conT == null) { conT = BuildingsConParent.Find(string.Format("Con({0}, {1})({2}, {3})", nextPos.x, nextPos.y, x, y)); }
                    if (conT != null)
                    {
                        Destroy(conT.gameObject);
                        nextEB = GetEnemyBuilding(nextPos.x, nextPos.y);
                        if (nextEB != null)
                        {
                            nextEB.conOut.Remove(myPos);
                        }
                    }
                }
            }

            //set terrain around
            for (int ix = x - 3; ix <= x + 3; ix++)
            {
                for (int iy = y - 3; iy <= y + 3; iy++)
                {
                    if (WorldMenager.instance.GetSquer(ix, iy) != Obj.EnemysTerrain) { continue; }
                    if (CanSetTerrain(ix, iy)) { WorldMenager.instance.squares[ix, iy] = Obj.None; }
                    else { WorldMenager.instance.squares[ix, iy] = Obj.EnemysTerrain; }
                }
            }
        }

        //set count
        switch (obj)
        {
            case Obj.EnemyCore: coreCount--; break;
            case Obj.EnemyTurret: turretCount--; break;
            case Obj.EnemySpawner: spawnerCount--; break;
        }

        bool CanSetTerrain(int _x, int _y)
        {
            int n = 0;
            Obj sObj;
            for (int _ix = _x - 3; _ix <= _x + 3; _ix++)
            {
                for (int _iy = _y - 3; _iy <= _y + 3; _iy++)
                {
                    if (_ix == _x && _iy == _y) { continue; }
                    sObj = WorldMenager.instance.GetSquer(_ix, _iy);
                    if (sObj == Obj.EnemyTurret || sObj == Obj.EnemySpawner || sObj == Obj.EnemyCore || sObj == Obj.EnemyPlatform) { n++; if (n >= 2) { return false; } }
                }
            }
            return true;
        }
    }
    private void SetBaseCenterPos()
    {
        int sx = 0;
        int sy = 0;
        foreach (EnemyBuilding building in buildings)
        {
            sx += building.x;
            sy += building.y;
        }
        int del = buildings.Count;
        sx /= del;
        sy /= del;
        baseCenterPos = new Vector2Int(sx, sy);
    }

    //build
    private void BuildConnection(Vector2Int startPoint, Vector2Int endPoint)
    {
        GameObject NewRoad = Instantiate(EnemyControler.instance.GetGameObject(Obj.ConUnderConstruction), transform.position, Quaternion.identity);
        NewRoad.transform.parent = BuildingsConParent;
        NewRoad.name = string.Format("Con({0}, {1})({2}, {3})", startPoint.x, startPoint.y, endPoint.x, endPoint.y);

        Vector2 relatve = endPoint - startPoint;

        Vector2 MidPointVector = (relatve / 2 + startPoint) * 10;
        float angle = Mathf.Atan2(relatve.y, relatve.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

        NewRoad.transform.localRotation = q;
        NewRoad.transform.position = MidPointVector;

        float RoadLenght = (relatve.magnitude - 0.5f);
        NewRoad.transform.localScale = new Vector2(RoadLenght, 1);
    }
    private GameObject BuildEBuilding(Obj obj, int x, int y)
    {
        GameObject newBuilding = EnemyControler.instance.GetGameObject(obj);
        if (newBuilding == null) { return null; }
        newBuilding = Instantiate(newBuilding, new Vector2(x, y) * 10, Quaternion.identity);
        newBuilding.transform.parent = BuildingsParent;
        newBuilding.name = string.Format("{0}({1},{2})", obj, x, y);
        WorldMenager.instance.squares[x, y] = obj;

        if (obj == Obj.EnemyWall)
        {
            walls.Add(new Vector2Int(x, y));
            WorldMenager.instance.squares[x, y] = Obj.EnemyWall;

            //conectors
            WorldGrid.SetSquare(new Square(Obj.EnemyWall, x, y, AllRecipes.instance.GetMaxHelthOfObj(Obj.EnemyWall), newBuilding.transform));
            if (WorldMenager.instance.GetSquer(x, y + 1) == Obj.EnemyWall) { SpownConector(x, y, x, y + 1); }
            if (WorldMenager.instance.GetSquer(x, y - 1) == Obj.EnemyWall) { SpownConector(x, y, x, y - 1); }
            if (WorldMenager.instance.GetSquer(x + 1, y) == Obj.EnemyWall) { SpownConector(x, y, x + 1, y); }
            if (WorldMenager.instance.GetSquer(x - 1, y) == Obj.EnemyWall) { SpownConector(x, y, x - 1, y); }

            void SpownConector(int sx, int sy, int ex, int ey)
            {
                GameObject newConnector = EnemyControler.instance.GetGameObject(Obj.Connection1);
                if (newConnector == null) { return; }
                newConnector = Instantiate(newConnector, new Vector2(x, y) * 10 + new Vector2Int(ex - sx, ey - sy) * 5, Quaternion.identity);
                newConnector.transform.parent = newBuilding.transform;
                newConnector.name = string.Format("WCon({0},{1})({2},{3})", sx, sy, ex, ey);
            }
        }
        else
        {
            for (int ix = -3; ix <= 3; ix++)
            {
                for (int iy = -3; iy <= 3; iy++)
                {
                    if (WorldMenager.instance.GetSquer(x + ix, y + iy) == Obj.None)
                    {
                        WorldMenager.instance.squares[x + ix, y + iy] = Obj.EnemysTerrain;
                    }
                }
            }
        }

        return newBuilding;
    }
    

    //path finding
    private void FindNewTargetAndAddToList(Vector2Int startPos)
    {
        //Debug.Log("I am looking for a new path and add to the list");
        targetPos = FindPathToNearesPlatform(startPos);
        if (targetPos.x == -1) { return; }
        CreatePath(targetPos, startPos);
        OptimizePath();

        //free up memory
        squaresToUpdate = new List<Vector3Int>();
        sTab = null;

        Transform targetT = WorldMenager.instance.GetTransforOfObj(targetPos.x, targetPos.y);
        if (targetT == null) { return; }

        PathAndTarget pathAT = new PathAndTarget(startPos, targetT, pathSquers);
        lastPaths.Add(pathAT);
    }
    private void OptimizePath()
    {
        //if (pathSquers.Count == 0) { return; }
        //pathSquers.RemoveAt(0);

        int pathlenght = pathSquers.Count;
        if (pathlenght <= 1) { return; }

        List<Vector2Int> newPath = new List<Vector2Int>();

        Vector2Int lastPos = pathSquers[0];
        Vector2Int actPos = pathSquers[1];
        Vector2Int actDir = EnemyControler.instance.GetDir(lastPos.x, lastPos.y, actPos.x, actPos.y);
        Vector2Int lastDir;

        newPath.Add(pathSquers[0]);
        

        for (int i = 2; i < pathlenght; i++)
        {
            lastDir = actDir;
            lastPos = actPos;
            actPos = pathSquers[i];
            actDir = EnemyControler.instance.GetDir(lastPos.x, lastPos.y, actPos.x, actPos.y);
            if (lastDir != actDir)
            {
                newPath.Add(pathSquers[i - 1]);
            }
        }
        newPath.Add(pathSquers[pathSquers.Count - 1]);

        pathSquers = new List<Vector2Int>();
        int newpathlenght = newPath.Count - 2;
        for (int i = newpathlenght; i >= 0; i--)
        {
            pathSquers.Add(newPath[i]);
        }
    }
    private void CreatePath(Vector2Int target, Vector2Int startPos)
    {
        //Debug.Log("CreatePath. Start on: " + target);

        pathSquers = new List<Vector2Int>();
        Vector2Int actPos = target;
        Vector2Int lowPos = target;
        pathSquers.Add(lowPos);

        int mapWidth = WorldMenager.instance.mapSize.x;
        int mapHeight = WorldMenager.instance.mapSize.y;

        int operation = 0;
        while (true)
        {
            if (operation > 1000) { Debug.Log("Cant create path. Operation: " + operation);  return; }
            operation++;

            int lowCost = int.MaxValue;
            for (int ix = -1; ix <= 1; ix++)
            {
                for (int iy = -1; iy <= 1; iy++)
                {
                    if (ix == 0 && iy == 0) { continue; }

                    int cx = actPos.x + ix;
                    int cy = actPos.y + iy;

                    if (cx < 0 || cx >= mapWidth || cy < 0 || cy >= mapHeight) { continue; }
                    int value = sTab[cx, cy];

                    if (value >= 0 && value < lowCost)
                    {
                        lowCost = value;
                        lowPos = new Vector2Int(cx, cy);
                    }
                }
            }

            pathSquers.Add(lowPos);
            actPos = lowPos;
            if (lowPos == startPos) { return; }
        }
    }
    private Vector2Int FindPathToNearesPlatform(Vector2Int mPos)
    {
        //Debug.Log("Finding Neares Platform. Start on: " + mPos);
        sTab = new int[WorldMenager.instance.mapSize.x, WorldMenager.instance.mapSize.y];
        for (int x = 0; x < WorldMenager.instance.mapSize.x; x++)
        {
            for (int y = 0; y < WorldMenager.instance.mapSize.y; y++)
            {
                sTab[x, y] = -1;
            }
        }

        squaresToUpdate = new List<Vector3Int>();

        //add ferst
        squaresToUpdate.Add(new Vector3Int(mPos.x, mPos.y, 0));
        sTab[mPos.x, mPos.y] = 0;

        int operation = 0;
        while (squaresToUpdate.Count > 0)
        {
            if (operation > 100000) { Debug.Log("Cant find nerest platform. Operation: " + operation); return new Vector2Int(-1, -1); }
            operation++;

            //find low cost square
            Vector3Int lowCostSquare = squaresToUpdate[0];
            foreach (Vector3Int checkedSquare in squaresToUpdate)
            {
                if (checkedSquare.z < lowCostSquare.z) { lowCostSquare = checkedSquare; }
            }

            //remove
            squaresToUpdate.Remove(lowCostSquare);

            //develop
            for (int ix = -1; ix <= 1; ix++)
            {
                for (int iy = -1; iy <= 1; iy++)
                {
                    if (ix == 0 && iy == 0) { continue; }

                    int cx = lowCostSquare.x + ix;
                    int cy = lowCostSquare.y + iy;

                    Obj obj = WorldMenager.instance.GetSquer(cx, cy);
                    if (obj == Obj.Locked || (obj >= Obj.EnemyCore && obj < Obj.EnemysTerrain)) { continue; }
                    if (AllRecipes.instance.IsItPlatform(obj)) { return new Vector2Int(cx, cy); }

                    if (sTab[cx, cy] >= 0) { continue; }

                    int hp = 0;
                    Square square = WorldGrid.GetSquare(cx, cy);
                    if (square != null) { hp = square.health; }

                    hp = (int)(hp * tendencToDestroy);
                    int cost = GetDistance(lowCostSquare.x, lowCostSquare.y, cx, cy) + hp + lowCostSquare.z;
                    squaresToUpdate.Add(new Vector3Int(cx, cy, cost));
                    sTab[cx, cy] = cost;
                }
            }
        }
        return new Vector2Int(-1, -1);
    }
    private int GetDistance(int sx, int sy, int ex, int ey)
    {
        int distX = Mathf.Abs(sx - ex);
        int distY = Mathf.Abs(sy - ey);

        if (distX > distY) { return 14 * distY + 10 * (distX - distY); }
        return 14 * distX + 10 * (distY - distX);
    }
    private List<Vector2Int> RewriteThePath(List<Vector2Int> _path)
    {
        List<Vector2Int> newPath = new List<Vector2Int>();
        foreach (Vector2Int pos in _path)
        {
            newPath.Add(new Vector2Int(pos.x, pos.y));
        }
        return newPath;
    }


    //Data
    public void InitBase()
    {
        //Debug.Log("InitBase");

        int mx = (int)transform.position.x / 10;
        int my = (int)transform.position.y / 10;

        EnemyBuilding EB = CreateNewPlatform(mx, my);
        ChangeBuildingType(EB, Obj.EnemySpawner);

        SetBaseCenterPos();

        Invoke("DevelopBase", 2f);
        avaDevelopTime = WorldMenager.instance.worldTime + developTime;
    }
    public EnemyData.EnemyBaseData GetEnemyBaseData()
    {
        EnemyData.EnemyBuildingData[] EBuildingDTable = new EnemyData.EnemyBuildingData[buildings.Count];
        for (int i = 0; i < buildings.Count; i++)
        {
            EnemyBuilding EB = buildings[i];
            Square square = WorldGrid.GetSquare(EB.x, EB.y);
            EnemyData.EnemyBuildingData EBD = new EnemyData.EnemyBuildingData((int)EB.obj, EB.x, EB.y, square.health, EB.conIn.ToArray(), EB.conOut.ToArray());

            if (square.obj == Obj.EnemyTurret)
            {
                EnemyTurret TurretSc = square.trans.GetComponent<EnemyTurret>();
                if (TurretSc != null)
                {
                    EBD.timeToShoot = TurretSc.avaShootTime - WorldMenager.instance.worldTime;
                    EBD.dir = TurretSc.TurretUp.rotation.eulerAngles.z;
                }
            }
            EBuildingDTable[i] = EBD;
        }

        int uTL = unitsTList.Count;
        int uBL = unitsBList.Count;
        int uRL = unitsRList.Count;
        int uFL = unitsFList.Count;
        EnemyData.EnemyUnitData[] unitsData = new EnemyData.EnemyUnitData[uTL + uBL + uRL + uFL];
        int index = 0;

        foreach (EUnitMovement unit in unitsTList) { AddEUnitMovmet(unit); }
        foreach (EUnitMovement unit in unitsBList) { AddEUnitMovmet(unit); }
        foreach (EUnitMovement unit in unitsRList) { AddEUnitMovmet(unit); }
        foreach (EUnitFlying unit in unitsFList)   { AddEUnitFlying(unit); }

        return new EnemyData.EnemyBaseData
            (
            avaDevelopTime,
            EBuildingDTable,
            toDevelop.ToArray(),
            walls.ToArray(),
            wallsToBuild.ToArray(),
            wallsToRemove.ToArray(),
            unitsData
            );

        void AddEUnitMovmet(EUnitMovement unit)
        {
            Vector2 tPos = new Vector2(-1, -1);
            if (unit.mainTargetT != null) { tPos = unit.mainTargetT.position; }
            EnemyData.EnemyUnitData unitD = new EnemyData.EnemyUnitData
                (
                    EnemyControler.instance.IntFromEnemyType(unit.myType),
                    unit.transform.position.x, unit.transform.position.y,
                    unit.transform.rotation.eulerAngles.z,
                    (int)unit.myDo, (int)unit.nextDo,
                    unit.health,
                    tPos.x, tPos.y
                );
            int pathLenght = unit.path.Count;
            int[] pathX = new int[pathLenght];
            int[] pathY = new int[pathLenght];
            for (int i = 0; i < pathLenght; i++) { pathX[i] = unit.path[i].x; pathY[i] = unit.path[i].y; }
            unitD.pathX = pathX;
            unitD.pathY = pathY;

            unitsData[index] = unitD;
            index++;
        }
        void AddEUnitFlying(EUnitFlying unit)
        {
            Vector2 tPos = new Vector2(-1, -1);
            if (unit.targetT != null) { tPos = unit.targetT.position; }
            EnemyData.EnemyUnitData unitD = new EnemyData.EnemyUnitData
                (
                    EnemyControler.instance.IntFromEnemyType(unit.myType),
                    unit.transform.position.x, unit.transform.position.y,
                    unit.transform.rotation.eulerAngles.z,
                    (int)unit.myDo, (int)unit.nextDo,
                    unit.health,
                    tPos.x, tPos.y
                );
            unitD.spawnX = unit.spawnerPos.x;
            unitD.spawnY = unit.spawnerPos.y;

            unitsData[index] = unitD;
            index++;
        }
    }
    public void SetEnemyBase(EnemyData.EnemyBaseData eBaseD)
    {
        //walls
        for (int i = 0; i < eBaseD.wallsX.Length; i++)         { BuildEBuilding(Obj.EnemyWall, eBaseD.wallsX[i], eBaseD.wallsY[i]); }
        for (int i = 0; i < eBaseD.wallsToBuildX.Length; i++)  { wallsToBuild.Add (new Vector2Int(eBaseD.wallsToBuildX[i],  eBaseD.wallsToBuildY[i])); }
        for (int i = 0; i < eBaseD.wallsToRemoveX.Length; i++) { wallsToRemove.Add(new Vector2Int(eBaseD.wallsToRemoveX[i], eBaseD.wallsToRemoveY[i])); }

        //to develop
        for (int i = 0; i < eBaseD.developPosX.Length; i++) { toDevelop.Add(new Vector2Int(eBaseD.developPosX[i], eBaseD.developPosY[i])); }

        //building
        foreach (EnemyData.EnemyBuildingData eBuildingD in eBaseD.buildings)
        {
            Obj type = (Obj)eBuildingD.type;
            GameObject buildingGO = BuildEBuilding(type, eBuildingD.x, eBuildingD.y);

            EnemyBuilding buildingC = new EnemyBuilding(type, eBuildingD.x, eBuildingD.y);
            buildings.Add(buildingC);
            WorldGrid.SetSquare(new Square(type, eBuildingD.x, eBuildingD.y, eBuildingD.health, buildingGO.transform));

            switch (type)
            {
                case Obj.EnemySpawner: spawnerCount++; break;
                case Obj.EnemyCore: coreCount++; break;
                case Obj.EnemyTurret: turretCount++; break;
            }

            //conection
            for (int i = 0; i < eBuildingD.connectionsInX.Length; i++)  { buildingC.conIn.Add (new Vector2Int(eBuildingD.connectionsInX[i],  eBuildingD.connectionsInY[i])); }
            for (int i = 0; i < eBuildingD.connectionsOutX.Length; i++)
            {
                Vector2Int ePos = new Vector2Int(eBuildingD.connectionsOutX[i], eBuildingD.connectionsOutY[i]);
                Vector2Int sPos = new Vector2Int(eBuildingD.x, eBuildingD.y);
                buildingC.conOut.Add(ePos);
                BuildConnection(sPos, ePos);
            }
        }

        //units
        foreach (EnemyData.EnemyUnitData unitD in eBaseD.units)
        {
            EnemyType type = (EnemyType)(unitD.type % 10);
            GameObject unitGO = SpawnEnemyUnit(type, new Vector2(unitD.x, unitD.y), unitD.rotate);
            switch (type)
            {
                case EnemyType.Tank: SetEUnitMovmet(unitGO, unitD); break;
                case EnemyType.Ball: SetEUnitMovmet(unitGO, unitD); break;
                case EnemyType.Range: SetEUnitMovmet(unitGO, unitD); break;
                case EnemyType.Flying: SetEUnitFlying(unitGO, unitD); break;
            }
        }
        
        //set develop
        Invoke("DevelopBase", eBaseD.avaDevelopTime - WorldMenager.instance.worldTime);

        SetBaseCenterPos();

        void SetEUnitMovmet(GameObject unitGO, EnemyData.EnemyUnitData _unitD)
        {
            EUnitMovement unitSc = unitGO.GetComponent<EUnitMovement>();
            if (unitSc == null) { Debug.Log("Cant set unit, because doesnt have EUnitMovement"); return; }

            if((EnemyDo)_unitD.task == EnemyDo.Attacking || (EnemyDo)_unitD.nextTask == EnemyDo.Attacking)
            {
                if (_unitD.targetX == -1) { unitSc.SetToMainAttack(null, null); }
                else
                {
                    Vector2Int pos = WorldMenager.instance.GetTabPos(new Vector2(_unitD.targetX, _unitD.targetY));
                    Transform targetT = WorldMenager.instance.GetTransforOfObj(pos.x, pos.y);
                    List<Vector2Int> path = new List<Vector2Int>();
                    for (int i = 0; i < _unitD.pathX.Length; i++) { path.Add(new Vector2Int(_unitD.pathX[i], _unitD.pathY[i])); }
                    unitSc.SetToMainAttack(targetT, path);
                }
            }
            else
            {
                unitSc.SetToWait();
            }
        }
        void SetEUnitFlying(GameObject unitGO, EnemyData.EnemyUnitData _unitD)
        {
            EUnitFlying unitSc = unitGO.GetComponent<EUnitFlying>();
            if (unitSc == null) { Debug.Log("Cant set unit, because doesnt have EUnitFlying"); return; }

            if ((EnemyDo)_unitD.task == EnemyDo.Attacking || (EnemyDo)_unitD.nextTask == EnemyDo.Attacking)
            {
                if (_unitD.targetX == -1) { unitSc.SetToAttack(null); }
                else
                {
                    Vector2Int pos = WorldMenager.instance.GetTabPos(new Vector2(_unitD.targetX, _unitD.targetY));
                    Transform targetT = WorldMenager.instance.GetTransforOfObj(pos.x, pos.y);
                    unitSc.SetToAttack(targetT);
                }
            }
            else
            {
                unitSc.SetToWait();
            }
        }
    }
}
