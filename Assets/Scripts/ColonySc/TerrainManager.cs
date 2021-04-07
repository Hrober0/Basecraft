using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject TerrainFertile;
    public GameObject Tree;
    public GameObject Sapling;
    public GameObject Farmland;
    public GameObject WaterSource;
    public GameObject OilSource;
    public Sprite[] FarmlandSprite = new Sprite[6];

    public GameObject StoneOre;
    public GameObject CopperOre;
    public GameObject IronOre;
    public GameObject CoalOre;

    public GameObject Mountain;
    public Sprite[] MoutainsSprites;

    [Header("Veribal to set")]
    public float treeSaplingGrowingTime = 70f;
    public float farmlandGrowingTime = 30f;

    [Range(0, 100)] public int copperFreqOfAppear;
    [Range(0, 100)] public int ironFreqOfAppear;
    [Range(0, 100)] public int coalFreqOfAppear;

    [Range(0, 100)] public int waterSourceFreqOfAppear;
    [Range(0, 100)] public int oilSourceFreqOfAppear;

    [Header("Veribal")]
    private float nerGrowTime = -1f;
    public List<ObjToGrow> ObjToGrows = new List<ObjToGrow>();


    static public TerrainManager instance;
    private void Awake()
    {
        if (instance != null) { return; }
        instance = this;
    }

    public void SetVeribalsFromSL()
    {
        GeneralWorldData GWD = SceneLoader.instance.generalWorldData;
        copperFreqOfAppear = GWD.copperFreqOfAppear;
        ironFreqOfAppear = GWD.ironFreqOfAppear;
        coalFreqOfAppear = GWD.coalFreqOfAppear;

        waterSourceFreqOfAppear = GWD.waterSourceFreqOfAppear;
        oilSourceFreqOfAppear = GWD.oilSourceFreqOfAppear;
    }
    public void SpawnAllTerrain()
    {
        List<Vector2> AllTMTiles = new List<Vector2>();
        List<Vector2> AllTFTiles = new List<Vector2>();
        bool[,] moutainTab = MapGenerator.instance.CreateMoutains(SceneLoader.instance.generalWorldData.mountainsSizes);

        //terrain 
        for (int y = 0; y < WorldMenager.instance.mapSize.y; y++)
        {
            for (int x = 0; x < WorldMenager.instance.mapSize.x; x++)
            {
                WorldMenager.instance.squeresVeribal[x, y] = 0;
                switch (WorldMenager.instance.terrainTiles[x, y])
                {
                    case Obj.StoneOre:
                        SimpleSpawnTerrain(Obj.StoneOre, x, y);
                        AllTMTiles.Add(new Vector2(x, y));
                        break;
                    case Obj.TerrainFertile:
                        SimpleSpawnTerrain(Obj.TerrainFertile, x, y);
                        AllTFTiles.Add(new Vector2(x, y));
                        break;
                    case Obj.Tree:
                        SimpleSpawnTerrain(Obj.TerrainFertile, x, y);
                        if (!moutainTab[x, y]) { SpawnTree(x, y); }
                        break;
                    case Obj.Mountain:
                        SpawnMountain(x, y, false);
                        break;
                }
            }
        }

        //mining ore
        for (int i = 0; i < AllTMTiles.Count; i++)
        {
            int random = Random.Range(0, 100);

            //spown copper ore
            if (random < copperFreqOfAppear)
            {
                int sx = (int)AllTMTiles[i].x;
                int sy = (int)AllTMTiles[i].y;
                SimpleSpawnTerrain(Obj.CopperOre, sx, sy);
                continue;
            }
            random -= copperFreqOfAppear;

            //spown iron ore
            if (random < ironFreqOfAppear)
            {
                int sx = (int)AllTMTiles[i].x;
                int sy = (int)AllTMTiles[i].y;
                SimpleSpawnTerrain(Obj.IronOre, sx, sy);
                continue;
            }
            random -= ironFreqOfAppear;

            //spown coal ore
            if (random < coalFreqOfAppear)
            {
                int sx = (int)AllTMTiles[i].x;
                int sy = (int)AllTMTiles[i].y;
                SimpleSpawnTerrain(Obj.CoalOre, sx, sy);
                continue;
            }
        }

        //fertile sources
        for (int i = 0; i < AllTFTiles.Count; i++)
        {
            int random = Random.Range(0, 100);

            //spown watter source
            if (random < waterSourceFreqOfAppear)
            {
                int sx = (int)AllTFTiles[i].x;
                int sy = (int)AllTFTiles[i].y;
                SimpleSpawnTerrain(Obj.WaterSource, sx, sy);
                string WSN = string.Format("{0}({1}, {2})", Obj.TerrainFertile, sx, sy);
                Transform WST = transform.Find(WSN);
                if (WST == null) { Debug.Log("nie znaleziono: " + WSN); }
                else { Destroy(WST.gameObject); }
                continue;
            }
            random -= waterSourceFreqOfAppear;

            //spown oil source
            if (random < oilSourceFreqOfAppear)
            {
                int sx = (int)AllTFTiles[i].x;
                int sy = (int)AllTFTiles[i].y;
                SimpleSpawnTerrain(Obj.OilSource, sx, sy);
                string WSN = string.Format("{0}({1}, {2})", Obj.TerrainFertile, sx, sy);
                Transform WST = transform.Find(WSN);
                if (WST == null) { Debug.Log("nie znaleziono: " + WSN); }
                else { Destroy(WST.gameObject); }
                continue;
            }
        }

        //moutains
        for (int y = 0; y < WorldMenager.instance.mapSize.y; y++)
        {
            for (int x = 0; x < WorldMenager.instance.mapSize.x; x++)
            {
                if (moutainTab[x, y])
                {
                    SpawnMountain(x, y, false);
                }
            }
        }
        UpdateAllMoutains();
    }
    private GameObject ObjToGO(Obj obj)
    {
        switch (obj)
        {
            case Obj.StoneOre: return StoneOre;
            case Obj.CopperOre: return CopperOre;
            case Obj.IronOre: return IronOre;
            case Obj.CoalOre: return CoalOre;

            case Obj.TerrainFertile: return TerrainFertile;
            case Obj.Tree: return Tree;
            case Obj.Sapling: return Sapling;
            case Obj.Farmland: return Farmland;
            case Obj.FarmlandRubber: return Farmland;
            case Obj.FarmlandFlax: return Farmland;
            case Obj.FarmlandGrape: return Farmland;
            case Obj.WaterSource: return WaterSource;
            case Obj.OilSource: return OilSource;

            case Obj.Mountain: return Mountain;
        }
        return null;
    }

    //spawn
    public void SimpleSpawnTerrain(Obj terr, int spownX, int spownY)
    {
        GameObject NewObj = Instantiate(ObjToGO(terr), new Vector2(spownX * 10, spownY * 10), Quaternion.identity);
        NewObj.name = string.Format("{0}({1}, {2})", terr, spownX, spownY);
        NewObj.transform.SetParent(transform);
        WorldMenager.instance.terrainTiles[spownX, spownY] = terr;
    }
    public void SpawnTree(int spownX, int spownY)
    {
        GameObject NewTree = Instantiate(Tree, new Vector2(spownX * 10, spownY * 10), Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        NewTree.name = string.Format("{0}({1}, {2})", Obj.Tree, spownX, spownY);
        NewTree.transform.parent = transform;
        float scale = Random.Range(0.9f, 1.1f);
        NewTree.transform.localScale = new Vector3(scale, scale, 1f);
        WorldMenager.instance.squares[spownX, spownY] = Obj.Tree;
        WorldGrid.SetSquare(new Square(Obj.Tree, spownX, spownY, AllRecipes.instance.GetMaxHelthOfObj(Obj.Tree), NewTree.transform));
    }
    public void SpawnSapling(int spownX, int spownY, bool grow)
    {
        GameObject NewSapling = Instantiate(Sapling, new Vector2(spownX * 10, spownY * 10), Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        NewSapling.name = string.Format("{0}({1}, {2})", Obj.Sapling, spownX, spownY);
        NewSapling.transform.parent = transform;
        WorldMenager.instance.squares[spownX, spownY] = Obj.Sapling;
        WorldGrid.SetSquare(new Square(Obj.Sapling, spownX, spownY, AllRecipes.instance.GetMaxHelthOfObj(Obj.Sapling), NewSapling.transform));
        if (grow == true)
        {
            ObjToGrows.Add(new ObjToGrow(WorldMenager.instance.worldTime + treeSaplingGrowingTime, Obj.Sapling, spownX, spownY));
            if (nerGrowTime == -1f) { CheckGrow(); }
        }
    }
    public void SpawnFarmland(int spownX, int spownY, bool grow, int flType)
    {

        GameObject NewFarmaland = Instantiate(Farmland, new Vector2(spownX * 10, spownY * 10), Quaternion.identity);
        NewFarmaland.name = string.Format("{0}({1}, {2})", Obj.Farmland, spownX, spownY);
        NewFarmaland.transform.parent = transform;
        WorldMenager.instance.squares[spownX, spownY] = Obj.Farmland;
        WorldMenager.instance.squeresVeribal[spownX, spownY] = (short)(flType * 10 + 1);
        WorldGrid.SetSquare(new Square(Obj.Farmland, spownX, spownY, AllRecipes.instance.GetMaxHelthOfObj(Obj.Farmland), NewFarmaland.transform));
        if (grow == true)
        {
            ObjToGrows.Add(new ObjToGrow(WorldMenager.instance.worldTime + farmlandGrowingTime, Obj.Farmland, spownX, spownY));
            if (nerGrowTime == -1f) { CheckGrow(); }
        }

        Transform terrFertileT = transform.Find(string.Format("{0}({1}, {2})", Obj.TerrainFertile, spownX, spownY));
        if (terrFertileT != null) { terrFertileT.gameObject.SetActive(false); }

        /*
         * type
         * 1 - Flax
         * 2 - Grape
         * 3 - RubberPlant
         */
    }
    public void SpawnMountain(int spawnX, int spawnY, bool update)
    {
        GameObject NewMountain = Instantiate(Mountain, new Vector2(spawnX * 10, spawnY * 10), Quaternion.identity);
        NewMountain.name = string.Format("{0}({1}, {2})", Obj.Mountain, spawnX, spawnY);
        NewMountain.transform.parent = transform;
        WorldMenager.instance.squares[spawnX, spawnY] = Obj.Mountain;
        WorldGrid.SetSquare(new Square(Obj.Mountain, spawnX, spawnY, AllRecipes.instance.GetMaxHelthOfObj(Obj.Mountain), NewMountain.transform));

        if (update)
        {
            for (int ix = -1; ix <= 1; ix++)
            {
                for (int iy = -1; iy <= 1; iy++)
                {
                    UpdateMoutain(spawnX + ix, spawnY + iy);
                }
            }
        }
    }

    //grow
    public void GrowObj(ObjToGrow OTG, bool setNewGrowTime)
    {
        switch (OTG.obj)
        {
            case Obj.Sapling:
                WorldMenager.instance.RemoveObjFromPos(OTG.x, OTG.y);
                SpawnTree(OTG.x, OTG.y);
                break;
            case Obj.Farmland:
                GrowFarmalnd();
                break;
        }

        void GrowFarmalnd()
        {
            Transform farmlandT = WorldMenager.instance.GetTransforOfObj(OTG.x, OTG.y);
            if (farmlandT == null) { return; }
            Sprite farmlandSprite = farmlandT.GetComponent<SpriteRenderer>().sprite;

            if (farmlandSprite == FarmlandSprite[5])
            {
                WorldMenager.instance.RemoveObjFromGO(farmlandT.gameObject, OTG.x, OTG.y);
                return;
            }

            if (setNewGrowTime)
            {
                ObjToGrows.Add(new ObjToGrow(WorldMenager.instance.worldTime + farmlandGrowingTime * 5, Obj.Farmland, OTG.x, OTG.y));
            }

            if (farmlandSprite == FarmlandSprite[3])
            {
                farmlandT.GetComponent<SpriteRenderer>().sprite = FarmlandSprite[4];

                Obj obj = Obj.None;
                int state = WorldMenager.instance.squeresVeribal[OTG.x, OTG.y] % 10;
                int type = (WorldMenager.instance.squeresVeribal[OTG.x, OTG.y] - state) / 10;
                switch (type)
                {
                    case 1: obj = Obj.FarmlandFlax; break;
                    case 2: obj = Obj.FarmlandGrape; break;
                    case 3: obj = Obj.FarmlandRubber; break;
                }
                if (obj == Obj.None) { Debug.Log("ERROR! wrong type of farmland to grow"); return; }
                farmlandT.name = string.Format("{0}({1}, {2})", obj, OTG.x, OTG.y);
                Square squer = WorldGrid.GetSquare(OTG.x, OTG.y);
                squer.obj = obj;
                WorldGrid.SetSquare(squer);
                WorldMenager.instance.squares[OTG.x, OTG.y] = obj;
                WorldMenager.instance.squeresVeribal[OTG.x, OTG.y] += 1;
                return;
            }
            if (farmlandSprite == FarmlandSprite[4])
            {
                farmlandT.GetComponent<SpriteRenderer>().sprite = FarmlandSprite[5];
                WorldMenager.instance.squeresVeribal[OTG.x, OTG.y] += 1;
                return;
            }
            for (int i = 0; i < 3; i++)
            {
                if (farmlandSprite == FarmlandSprite[i])
                {
                    farmlandT.GetComponent<SpriteRenderer>().sprite = FarmlandSprite[i + 1];
                    WorldMenager.instance.squeresVeribal[OTG.x, OTG.y] += 1;
                    return;
                }
            }
        }
    }
    public void CheckGrow()
    {
        nerGrowTime = float.MaxValue;
        int del = 0;
        int pow = ObjToGrows.Count;
        float time;
        ObjToGrow otg;
        for (int i = 0; i < pow; i++)
        {
            otg = ObjToGrows[i - del];
            time = otg.time;
            if (time <= WorldMenager.instance.worldTime)
            {
                ObjToGrows.RemoveAt(i - del);
                GrowObj(otg, true);
                del++;
            }
            else if (time < nerGrowTime)
            { nerGrowTime = time; }
        }
        if (nerGrowTime == float.MaxValue) { nerGrowTime = -1; }
        if (nerGrowTime > 0f) { Invoke("CheckGrow", nerGrowTime - WorldMenager.instance.worldTime); }
    }

    //moutains
    public void UpdateAllMoutains()
    {
        for (int y = 0; y < WorldMenager.instance.mapSize.y; y++)
        {
            for (int x = 0; x < WorldMenager.instance.mapSize.x; x++)
            {
                UpdateMoutain(x, y);
            }
        }
    }
    public void UpdateMoutain(int x, int y)
    {
        if (WorldMenager.instance.GetSquer(x, y) != Obj.Mountain) { return; }

        Transform trans = WorldMenager.instance.GetTransforOfObj(x, y);
        if (trans == null) { Debug.Log("ERROR! Cant get transfor of moutain on x:" + x + " y:" + y); return; }

        bool[,] tab = new bool[3, 3];
        for (int iy = -1; iy <= 1; iy++)
        {
            for (int ix = -1; ix <= 1; ix++)
            {
                if(WorldMenager.instance.GetSquer(x+ix, y+iy) == Obj.Mountain) { tab[ix + 1, iy + 1] = true; }
                else { tab[ix + 1, iy + 1] = false; }
            }
        }

        if (!tab[0, 1] || !tab[1, 0]) { tab[0, 0] = false; }
        if (!tab[2, 1] || !tab[1, 0]) { tab[2, 0] = false; }
        if (!tab[0, 1] || !tab[1, 2]) { tab[0, 2] = false; }
        if (!tab[2, 1] || !tab[1, 2]) { tab[2, 2] = false; }

        for (int i = 0; i < 4; i++)
        {
            if (tab[1, 0] && tab[1, 2] && tab[0, 1] && tab[2, 1])
            {
                if ( tab[0, 0] &&  tab[0, 2] &&  tab[2, 2] &&  tab[2, 0]) { Set(0, i); return; }
                if ( tab[0, 0] &&  tab[0, 2] && !tab[2, 2] &&  tab[2, 0]) { Set(1, i); return; }
                if ( tab[0, 0] && !tab[0, 2] && !tab[2, 2] &&  tab[2, 0]) { Set(2, i); return; }
                if (!tab[0, 0] &&  tab[0, 2] && !tab[2, 2] &&  tab[2, 0]) { Set(3, i); return; }
                if (!tab[0, 0] && !tab[0, 2] && !tab[2, 2] &&  tab[2, 0]) { Set(4, i); return; }
                if (!tab[0, 0] && !tab[0, 2] && !tab[2, 2] && !tab[2, 0]) { Set(5, i); return; }
            }
            else if (tab[1, 0] && !tab[1, 2] && tab[0, 1] && tab[2, 1])
            {
                if ( tab[0, 0] &&  tab[2, 0]) { Set(6, i); return; }
                if (!tab[0, 0] && !tab[2, 0]) { Set(7, i); return; }
                if (!tab[0, 0] &&  tab[2, 0]) { Set(8, i); return; }
                if ( tab[0, 0] && !tab[2, 0]) { Set(9, i); return; }
            }
            else if (!tab[1, 0] && !tab[1, 2] &&  tab[0, 1] &&  tab[2, 1])
            {
                Set(10, i); return;
            }
            else if ( tab[1, 0] && !tab[1, 2] &&  tab[0, 1] && !tab[2, 1])
            {
                if ( tab[0, 0]) { Set(11, i); return; }
                if (!tab[0, 0]) { Set(12, i); return; }
            }
            else if (!tab[1, 0] &&  tab[1, 2] && !tab[0, 1] && !tab[2, 1])
            {
                Set(13, i); return;
            }
            else if (!tab[1, 0] && !tab[1, 2] && !tab[0, 1] && !tab[2, 1])
            {
                Set(14, i); return;
            }

            //turn
            bool f = tab[0, 0];
            tab[0, 0] = tab[0, 2];
            tab[0, 2] = tab[2, 2];
            tab[2, 2] = tab[2, 0];
            tab[2, 0] = f;
            f = tab[0, 1];
            tab[0, 1] = tab[1, 2];
            tab[1, 2] = tab[2, 1];
            tab[2, 1] = tab[1, 0];
            tab[1, 0] = f;
        }
        Debug.Log("Dont found moutain state! on x: " + x + " y:" + y);

        void Set(int n, int i)
        {
            //string s1 = string.Format("{0}-{1}-{2}", tab[0, 0], tab[1, 0], tab[2, 0]); string s2 = string.Format("{0}-{1}-{2}", tab[0, 1], tab[1, 1], tab[2, 1]); string s3 = string.Format("{0}-{1}-{2}", tab[0, 2], tab[1, 2], tab[2, 2]); Debug.Log("Found moutine state n: " + n + " i: " + i + " on x: " + x + " y: " + y + "\n" + s1 + "\n" + s2 + "\n" + s3);
            Sprite sprite = MoutainsSprites[n];
            if (sprite == null) { Debug.Log("ERROR! Mising sprite of moutine " + n); return; }
            trans.GetComponent<SpriteRenderer>().sprite = sprite;

            float angleZ;
            if(n == 0 || n == 14) { angleZ = Random.Range(0, 4) * 90; }
            else { angleZ = i * -90; }
            trans.localRotation = Quaternion.Euler(0, 0, angleZ);

            if(n == 0)
            {
                Obj terr = WorldMenager.instance.GetTerrainTile(x, y);
                if (terr != Obj.None)
                {
                    Transform terrT = transform.Find(string.Format("{0}({1}, {2})", terr, x, y));
                    if (terrT != null) { terrT.gameObject.SetActive(false); }
                }
            }
        }
    }

    //images
    public Sprite GetTerrainImages(Obj obj)
    {

        GameObject objGO = ObjToGO(obj);

        if (objGO == null) { return null; }

        return objGO.GetComponent<SpriteRenderer>().sprite;;
    }

    //remove terrain
    public void RemoveTerrain(Obj obj, int x, int y)
    {
        Debug.Log("Removing terrain " + obj + " on " + x + " " + y);
        Transform trans = transform.Find(string.Format("{0}({1}, {2})", obj, x, y));
        if (trans == null) { Debug.Log("terrain not found " + obj + " on " + x + " " + y); return; }
        Destroy(trans.gameObject);
        WorldMenager.instance.terrainTiles[x, y] = Obj.None;
        if(obj == Obj.CopperOre || obj == Obj.IronOre)
        {
            trans = transform.Find(string.Format("{0}({1}, {2})", Obj.StoneOre, x, y));
            if (trans == null) { Debug.Log("terrain not found " + obj + " on " + x + " " + y); return; }
            Destroy(trans.gameObject);
        }
    }
}
