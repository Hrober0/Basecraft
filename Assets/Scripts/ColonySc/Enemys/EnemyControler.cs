using System.Collections.Generic;
using UnityEngine;

public class EnemyControler : MonoBehaviour
{
    [Header("Info")]
    public int eTurretRange = 8;
    public float baseDevelopTime = 90f;
    private Vector2Int[] circleRange;

    [Header("Base Controler")]
    public GameObject BaseControler;

    [Header("Buildings to set")]
    public GameObject EnemySpawner;
    public GameObject EnemyCore;
    public GameObject EnemyTurret;
    public GameObject EnemyWall;
    public GameObject EnemyWalls;
    public GameObject EnemyPlatform;
    public GameObject EnemyConnection;

    [Header("Unit to set")]
    public GameObject EnemyF;
    public GameObject EnemyT;
    public GameObject EnemyR;
    public GameObject EnemyB;

    public float attackTime;
    public List<EnemyBaseControler> baseControlerList = new List<EnemyBaseControler>();
    public List<WaveOfEnemyData> waves = new List<WaveOfEnemyData>();
    public List<WaveOfEnemyData> savesWaves = new List<WaveOfEnemyData>();

    private float timeToSpawnUnit = 0;
    private int unitsToSpawnCount = 0;
    private bool useSpawnersList; //false - list 1, true - list 2
    public List<Vector3Int> spawnerPoss1 = new List<Vector3Int>(); // z - baseControlerList index
    public List<Vector3Int> spawnerPoss2 = new List<Vector3Int>(); // z - baseControlerList index

    public static EnemyControler instance;
    private void Awake()
    {
        if (instance != null) { Debug.Log("more the one EnemyControler on scen"); return; }
        instance = this;

        CreateCircleRange();
    }

    private void CreateCircleRange()
    {
        int sideL = 0;
        int sth = 0;
        int b = 0;
        int x, y, i, e;
        int mx = 0;
        int my = 0;
        List<Vector2Int> listv2i = new List<Vector2Int>();

        for (int ar = 1; ar < eTurretRange + 1; ar++)
        {
            x = -b + mx;
            y = ar + my;
            e = b + 1 + mx;
            for (; x < e; x++) { AddToList(); }

            x = ar + mx;
            y = b + my;
            e = -b - 1 + my;
            for (; y > e; y--) { AddToList(); }

            x = b + mx;
            y = -ar + my;
            e = -b - 1 + mx;
            for (; x > e; x--) { AddToList(); }

            x = -ar + mx;
            y = -b + my;
            e = b + 1 + my;
            for (; y < e; y++) { AddToList(); }


            x = b + mx;
            y = ar + my;
            for (i = 0; i < sideL; i++) { x++; y--; AddToList(); }

            x = ar + mx;
            y = -b + my;
            for (i = 0; i < sideL; i++) { x--; y--; AddToList(); }

            x = -b + mx;
            y = -ar + my;
            for (i = 0; i < sideL; i++) { x--; y++; AddToList(); }

            x = -ar + mx;
            y = b + my;
            for (i = 0; i < sideL; i++) { x++; y++; AddToList(); }

            sideL++;
            if (sth > 0) { sth--; }
            else
            {
                sth = 2;
                b++;

                x = b + mx;
                y = ar + my;
                for (i = 0; i < sideL; i++) { AddToList(); x++; y--; }

                x = ar + mx;
                y = -b + my;
                for (i = 0; i < sideL; i++) { AddToList(); x--; y--; }

                x = -b + mx;
                y = -ar + my;
                for (i = 0; i < sideL; i++) { AddToList(); x--; y++; }

                x = -ar + mx;
                y = b + my;
                for (i = 0; i < sideL; i++) { AddToList(); x++; y++; }

                sideL--;
            }
        }

        circleRange = listv2i.ToArray();

        void AddToList()
        {
            listv2i.Add(new Vector2Int(x, y));
        }
    }


    //waves
    public void SetDefaultWaves()
    {
        waves = new List<WaveOfEnemyData>
        {
            new WaveOfEnemyData(
                850f,
                new int[1]{ 1 },
                new int[1]{ 1 }
            ),
            new WaveOfEnemyData(
                240f,
                new int[1]{ 1 },
                new int[1]{ 2 }
            ),
            new WaveOfEnemyData(
                200f,
                new int[1]{ 1 },
                new int[1]{ 4 }
            ),
            new WaveOfEnemyData(
                230f,
                new int[1]{ 2 },
                new int[1]{ 3 }
            ),
            new WaveOfEnemyData(
                220f,
                new int[1]{ 2 },
                new int[1]{ 5 }
            ),
            new WaveOfEnemyData(
                250f,
                new int[2]{ 1, 2 },
                new int[2]{ 3, 2 }
            ),
            new WaveOfEnemyData(
                230f,
                new int[2]{ 1, 2 },
                new int[2]{ 3, 4 }
            ),
             new WaveOfEnemyData(
                220f,
                new int[2]{ 1, 2 },
                new int[2]{ 1, 6 }
            ),
            new WaveOfEnemyData(
                260f,
                new int[2]{ 1, 2 },
                new int[2]{ 7, 1 }
            ),
            new WaveOfEnemyData(
                60f,
                new int[1]{ 2 },
                new int[1]{ 5 }
            ),
            new WaveOfEnemyData(
                200f,
                new int[1]{ 2 },
                new int[1]{ 6 }
            ),
            new WaveOfEnemyData(
                150f,
                new int[2]{ 1, 2 },
                new int[2]{ 4, 4 }
            ),
            new WaveOfEnemyData(
                210f,
                new int[2]{ 1, 2 },
                new int[2]{ 4, 5 }
            ),
            new WaveOfEnemyData(
                150f,
                new int[1]{ 4 },
                new int[1]{ 1 }
            ),
            new WaveOfEnemyData(
                300,
                new int[1]{ 4 },
                new int[1]{ 4 }
            ),
            new WaveOfEnemyData(
                120,
                new int[2]{ 1, 4 },
                new int[2]{ 4, 4 }
            ),
            new WaveOfEnemyData(
                280,
                new int[2]{ 1, 2 },
                new int[2]{ 2, 8 }
            ),
        };

        SetSavesWaves(waves.ToArray());
    }
    public void SetSavesWaves(WaveOfEnemyData[] _waves)
    {
        savesWaves = new List<WaveOfEnemyData>();
        foreach (WaveOfEnemyData swave in _waves)
        {
            int[] numbers = new int[swave.number.Length];
            for (int i = 0; i < swave.number.Length; i++)
            {
                numbers[i] = swave.number[i];
            }
            savesWaves.Add(new WaveOfEnemyData(swave.timeToLaunch, swave.type, numbers));
        }
    }
    public void SetWaves(WaveOfEnemyData[] _waves)
    {
        waves = new List<WaveOfEnemyData>();
        foreach (WaveOfEnemyData wwave in _waves)
        {
            waves.Add(wwave);
        }
    }
    public void CreateMoretWaves(bool shortenTimeFirstWave)
    {
        Debug.Log("Create more waves");

        int numOfWaves = savesWaves.Count;
        if (numOfWaves <= 0) { Debug.Log("Cant create more because waves was no set"); return; }
        int lastNum = 0;
        int actNum = 0;
        int averageIncreaseNumberOfUnits = 0;
        int averageNumberOfUnits = 0;
        for (int i = 0; i < numOfWaves; i++)
        {
            actNum = 0;
            for (int ii = 0; ii < savesWaves[i].number.Length; ii++)
            {
                actNum += savesWaves[i].number[ii];
            }
            int delta = actNum - lastNum;
            averageNumberOfUnits += actNum;
            //Debug.Log(delta);
            averageIncreaseNumberOfUnits += delta;
        }
        averageIncreaseNumberOfUnits /= numOfWaves;
        averageNumberOfUnits /= numOfWaves;
        //Debug.Log("average number of units: " + averageNumberOfUnits);

        for (int i = 0; i < numOfWaves; i++)
        {
            WaveOfEnemyData savedWave = savesWaves[i];
            float ttl = savedWave.timeToLaunch;
            if(shortenTimeFirstWave && i == 0)
            {
                if(i+1 < numOfWaves && ttl > savesWaves[i + 1].timeToLaunch * 2f) { ttl = savesWaves[i + 1].timeToLaunch; }
            }

            int n = savedWave.number.Length;
            int[] numbers = new int[n];
            int addNum = averageNumberOfUnits / n + 1;
            //Debug.Log("Increase: +" + addNum + " n: " + n);
            for (int ii = 0; ii < n; ii++)
            {
                numbers[ii] = addNum + savedWave.number[ii];
            }

            waves.Add(new WaveOfEnemyData(ttl, savedWave.type, numbers));
        }

        SetSavesWaves(waves.ToArray());
    }
    public void SetWaveToLaunch(float timeToLaunch)
    {
        if (waves.Count == 0) { CreateMoretWaves(true); }
        if (waves.Count == 0) { Debug.Log("cant create more waves of enemies"); return; }

        if (timeToLaunch < 0f) { timeToLaunch = waves[0].timeToLaunch; }

        spawnerPoss1 = new List<Vector3Int>();
        spawnerPoss2 = new List<Vector3Int>();
        useSpawnersList = false;
        for(int i=0; i < baseControlerList.Count; i++)
        {
            EnemyBaseControler EBC = baseControlerList[i];
            Vector2Int[] poss = EBC.GetAllPoss(Obj.EnemySpawner);
            foreach (Vector2Int pos in poss)
            {
                Vector3Int posAndIndex = new Vector3Int(pos.x, pos.y, 0);
                spawnerPoss1.Add(posAndIndex);
            } 
        }
        if (spawnerPoss1.Count == 0)
        {
            Debug.Log("no spawners");
            return;
        }

        unitsToSpawnCount = 0;
        foreach (int number in waves[0].number)
        {
            unitsToSpawnCount += number;
        }

        timeToSpawnUnit = timeToLaunch / (unitsToSpawnCount+1);
        Invoke("SpawnUnitWithWave", timeToSpawnUnit);


        Debug.Log("Set wave to launch. The attack will occur in " + timeToLaunch + " seconds");
        attackTime = WorldMenager.instance.worldTime + timeToLaunch;
        Invoke("LaunchWave", timeToLaunch);

        float timeToDisplayWarning = timeToLaunch - Random.Range(20f, 80f);
        if (timeToDisplayWarning > 0) { Invoke("DisplayAttackWarning", timeToDisplayWarning); }
        else if (timeToLaunch > 20f) { Invoke("DisplayAttackWarning", 20f); }
    }
    private void SpawnUnitWithWave()
    {
        if (waves.Count == 0) { Debug.Log("ERROR! mising wave"); return; }

        int unitType = -1;
        int typeLenght = waves[0].type.Length;
        for (int i = 0; i < typeLenght; i++)
        {
            if (waves[0].number[i] > 0)
            {
                unitType = waves[0].type[i];
                waves[0].number[i]--;
                break;
            }
        }
        if (unitType == -1) { Debug.Log("no enemy to spawn"); return; }

        unitsToSpawnCount--;

        Vector3Int spawnerPos = new Vector3Int(-1, -1, -1);
        if (useSpawnersList)
        {
            int listLenght = spawnerPoss2.Count;
            if (listLenght == 0) { Debug.Log("ERROR! mising spawner pos in list 2"); useSpawnersList = false; return; }
            int index = Random.Range(0, listLenght);
            spawnerPos = spawnerPoss2[index];
            spawnerPoss2.RemoveAt(index);
            spawnerPoss1.Add(spawnerPos);
            if (listLenght <= 1) { useSpawnersList = false; }
        }
        else
        {
            int listLenght = spawnerPoss1.Count;
            if (listLenght == 0) { Debug.Log("ERROR! mising spawner pos in list 1"); useSpawnersList = true; return; }
            int index = Random.Range(0, listLenght);
            spawnerPos = spawnerPoss1[index];
            spawnerPoss1.RemoveAt(index);
            spawnerPoss2.Add(spawnerPos);
            if (listLenght <= 1) { useSpawnersList = true; }
        }

        Vector2Int shift = new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
        Vector2 spawnPos = (Vector2Int)spawnerPos * 10 + shift * 3;

        int EBCindex = spawnerPos.z;
        if(EBCindex<0 || EBCindex > baseControlerList.Count) { Debug.Log("spawner base index(" + EBCindex + ") is wrong"); return; }
        EnemyBaseControler EBC = baseControlerList[EBCindex];
        EBC.SpawnEnemyUnit(EnemyTypeFromInt(unitType), spawnPos, 0);

        if(unitsToSpawnCount > 0)
        {
            Invoke("SpawnUnitWithWave", timeToSpawnUnit);
        }
    }
    private void DisplayAttackWarning()
    {
        MessageManager.instance.ShowMessage(Messages.AttackIsComing, -1);
    }
    private void LaunchWave()
    {
        if (waves.Count == 0) { Debug.Log("ERROR! missing wave of enemies"); return; }

        WaveOfEnemyData wave = waves[0];
        if(wave.type.Length != wave.number.Length) { Debug.Log("ERROR! Wrong number of arguments in wave"); return; }

        MessageManager.instance.ShowMessage(Messages.YouAreUnderAttack, -1);

        foreach (EnemyBaseControler EBC in baseControlerList)
        {
            EBC.TriggerAtack();
        }

        waves.RemoveAt(0);
        SetWaveToLaunch(-1f);
    }

    //get
    public Vector2Int GetNearestEnemyBuilding(int mx, int my)
    {
        Vector2Int nearesEBuilding = new Vector2Int(-1, -1);
        Vector2Int pos;
        int minDist = int.MaxValue;
        int dist;
        int deltaX;
        int deltaY;

        foreach (EnemyBaseControler eBase in baseControlerList)
        {
            pos = eBase.GetNearestEnemyBuilding(mx, my);
            if (pos.x == -1) { continue; }
            deltaX = pos.x - mx; if (deltaX < 0) { deltaX *= -1; }
            deltaY = pos.y - my; if (deltaY < 0) { deltaY *= -1; }
            dist = deltaX + deltaY;
            if (dist < minDist)
            {
                nearesEBuilding = pos;
                minDist = dist;
            }
        }

        return nearesEBuilding;
    }
    public Vector2Int GetNearestBuilding(Vector2Int myPos)
    {
        foreach (Vector2Int deltaPos in circleRange)
        {
            Vector2Int realPos = deltaPos + myPos;
            if (AllRecipes.instance.IsItBuilding(WorldMenager.instance.GetSquer(realPos.x, realPos.y)))
            {
                return realPos;
            }
        }
        return new Vector2Int(-1, -1);
    }
    public GameObject GetGameObject(Obj obj)
    {
        switch (obj)
        {
            case Obj.EnemySpawner: return EnemySpawner;
            case Obj.EnemyTurret: return EnemyTurret;
            case Obj.EnemyPlatform: return EnemyPlatform;
            case Obj.EnemyCore: return EnemyCore;
            case Obj.EnemyWall: return EnemyWall;
            case Obj.ConUnderConstruction: return EnemyConnection;
            case Obj.Connection1: return EnemyWalls;
        }
        return null;
    }
    public GameObject GetEnemyPrefabfromType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Tank: return EnemyT;
            case EnemyType.Ball: return EnemyB;
            case EnemyType.Range: return EnemyR;
            case EnemyType.Flying: return EnemyF;
        }
        return null;
    }
    public Vector2Int GetDir(int mx, int my, int tx, int ty)
    {
        Vector2Int mv = new Vector2Int();
        int dx = tx - mx;
        int dy = ty - my;

        if (dx < 0) { mv.x = -1; dx *= -1; }
        else if (dx == 0) { mv.x = 0; }
        else { mv.x = 1; }

        if (dy < 0) { mv.y = -1; dy *= -1; }
        else if (dy == 0) { mv.y = 0; }
        else { mv.y = 1; }

        if (dx > dy)
        {
            if (dx > dy * 2) { mv.y = 0; }
        }
        else
        {
            if (dy > dx * 2) { mv.x = 0; }
        }

        return mv;
    }
    public EnemyType EnemyTypeFromInt(int type)
    {
        switch (type)
        {
            case 1: return EnemyType.Tank;
            case 2: return EnemyType.Ball;
            case 3: return EnemyType.Range;
            case 4: return EnemyType.Flying;
        }
        Debug.Log("Wrong enemy unit type");
        return EnemyType.Dif; ;
    }
    public int IntFromEnemyType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Tank: return 1;
            case EnemyType.Ball: return 2;
            case EnemyType.Range: return 3;
            case EnemyType.Flying: return 4;
        }
        return -1;
    }
    public Transform GetNearesEUnit(Vector2 sPos, float range)
    {
        Transform nearestT = null;
        float minDist = float.MaxValue;
        float dx, dy, dist;
        float rl = range * range;

        foreach (EnemyBaseControler eBase in baseControlerList)
        {
            Transform trans = eBase.GetNearestEUnit(sPos);
            if (trans == null) { continue; }
            dist = CalcDist(trans.position);
            if (dist < minDist) { minDist = dist; nearestT = trans; }
        }
        //if (nearestT != null) { Debug.Log(nearestT.position + " " + sPos + " " + minDist + " " + rl); }
        
        if (minDist <= rl) { return nearestT; }
        else { return null; }

        float CalcDist(Vector2 ePos)
        {
            dx = ePos.x - sPos.x;
            dy = ePos.y - sPos.y;
            return dx * dx + dy * dy;
        }
    }
    public Sprite GetEnemyBaseImages(Obj obj)
    {
        GameObject objGO = GetGameObject(obj);

        if (objGO == null) { return null; }

        return objGO.GetComponent<SpriteRenderer>().sprite;
    }


    //base
    public void CreateNewEnemyBase(Vector2Int pos)
    {
        GameObject newBase = Instantiate(BaseControler, new Vector2(pos.x, pos.y) * 10, Quaternion.identity);
        newBase.transform.parent = transform;
        newBase.name = string.Format("BaseControler");
        EnemyBaseControler EBC = newBase.GetComponent<EnemyBaseControler>();
        EBC.developTime = baseDevelopTime;
        EBC.InitBase();
        baseControlerList.Add(EBC);
    }
    public EnemyData.EnemyBaseData[] GetEBaseData()
    {
        int num = baseControlerList.Count;
        EnemyData.EnemyBaseData[] eBaseDataTab = new EnemyData.EnemyBaseData[num];
        for (int i = 0; i < num; i++)
        {
            eBaseDataTab[i] = baseControlerList[i].GetEnemyBaseData();
        }

        return eBaseDataTab;
    }
    public void LoadEnemyBase(EnemyData.EnemyBaseData eBaseD)
    {
        GameObject newBase = Instantiate(BaseControler, transform);
        newBase.transform.parent = transform;
        newBase.name = string.Format("BaseControler");
        EnemyBaseControler EBC = newBase.GetComponent<EnemyBaseControler>();
        EBC.developTime = baseDevelopTime;
        EBC.SetEnemyBase(eBaseD);
        baseControlerList.Add(EBC);
    }
}
