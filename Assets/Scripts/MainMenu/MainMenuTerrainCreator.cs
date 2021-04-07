using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuTerrainCreator : MonoBehaviour
{
    [Header("veribal")]
    public Vector2Int mapSize;
    [Range(0, 99)] public int copperFreqOfAppear;
    [Range(0, 99)] public int ironFreqOfAppear;
    [Range(0, 99)] public int waterSourceFreqOfAppear;
    [Range(0, 99)] public int oilSourceFreqOfAppear;

    [Header("prefab to set")]
    public GameObject TerrainMining;
    public GameObject TerrainFertile;
    public GameObject Tree;
    public GameObject WaterSource;
    public GameObject OilSource;
    public GameObject CopperOre;
    public GameObject IronOre;

    public GameObject Mountain;
    public Sprite[] MoutainsSprites;

    Obj[,] terrTab;
    bool[,] moutainsTab;

    private int seed;

    void Start()
    {
        Vector2 size = MainMenuCameraControler.instance.GetCameraSize();
        mapSize = new Vector2Int((int)size.x / 5, (int)size.y / 5);

        seed = Random.Range(int.MinValue, int.MaxValue);
        CreateTerrain();
        MapGenerator.instance.CreateBecground(seed, mapSize.x, mapSize.y);
        MainMenuCameraControler.instance.SetCamera(mapSize);
    }

    private void CreateTerrain()
    {
        terrTab = MapGenerator.instance.GenerateTerrainTab(mapSize, seed, 1f, 1f);

        List<Vector2> AllTMTiles = new List<Vector2>();
        List<Vector2> AllTFTiles = new List<Vector2>();
        moutainsTab = MapGenerator.instance.CreateMoutains(1f);

        //terrain 
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                switch (terrTab[x, y])
                {
                    case Obj.StoneOre:
                        SpownTerrain(Obj.StoneOre, x, y);
                        AllTMTiles.Add(new Vector2(x, y));
                        break;
                    case Obj.TerrainFertile:
                        SpownTerrain(Obj.TerrainFertile, x, y);
                        AllTFTiles.Add(new Vector2(x, y));
                        break;
                    case Obj.Tree:
                        SpownTerrain(Obj.TerrainFertile, x, y);
                        if (!moutainsTab[x, y]) { SpownTerrain(Obj.Tree, x, y); }
                        break;
                }
            }
        }

        //mining ore
        for (int i = 0; i < AllTMTiles.Count; i++)
        {
            //spown copper ore
            if (Random.Range(0, 100) < copperFreqOfAppear)
            {
                int sx = (int)AllTMTiles[i].x;
                int sy = (int)AllTMTiles[i].y;
                SpownTerrain(Obj.CopperOre, sx, sy);
                terrTab[sx, sy] = Obj.CopperOre;
            }
            //spown iron ore
            else if (Random.Range(0, 100) < ironFreqOfAppear)
            {
                int sx = (int)AllTMTiles[i].x;
                int sy = (int)AllTMTiles[i].y;
                SpownTerrain(Obj.IronOre, sx, sy);
                terrTab[sx, sy] = Obj.IronOre;
            }
        }

        //fertile sources
        for (int i = 0; i < AllTFTiles.Count; i++)
        {
            //spown watter source
            if (Random.Range(0, 100) < waterSourceFreqOfAppear)
            {
                int sx = (int)AllTFTiles[i].x;
                int sy = (int)AllTFTiles[i].y;
                SpownTerrain(Obj.WaterSource, sx, sy);
            }
        }

        //moutains
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                if (moutainsTab[x, y])
                {
                    SpawnMountain(x, y, false);
                }
            }
        }
        UpdateAllMoutains();
    }

    //spawn
    private void SpownTerrain(Obj terr, int x, int y)
    {
        GameObject GO = TerrToGO(terr);
        if (GO == null) { Debug.Log("cant find" + terr + " prefab"); return; }
        GameObject NewObj = Instantiate(GO, new Vector2(x * 10, y * 10), Quaternion.identity);
        NewObj.name = string.Format("{0}({1}, {2})", terr, x, y);
        NewObj.transform.parent = transform;

        switch (terr)
        {
            case Obj.Tree:
                float scale = Random.Range(0.9f, 1.1f);
                NewObj.transform.localScale = new Vector3(scale, scale, 1f);
                terrTab[x, y] = Obj.TerrainFertile;
                break;
            case Obj.WaterSource:
                string WSN = string.Format("{0}({1}, {2})", Obj.TerrainFertile, x, y);
                Transform WST = transform.Find(WSN);
                if (WST != null) { Destroy(WST.gameObject); }
                break;
            case Obj.OilSource:
                WSN = string.Format("{0}({1}, {2})", Obj.TerrainFertile, x, y);
                WST = transform.Find(WSN);
                if (WST != null) { Destroy(WST.gameObject); }
                break;
        }
    }
    public void SpawnMountain(int x, int y, bool update)
    {
        GameObject NewMountain = Instantiate(Mountain, new Vector2(x * 10, y * 10), Quaternion.identity);
        NewMountain.name = string.Format("{0}({1}, {2})", Obj.Mountain, x, y);
        NewMountain.transform.parent = transform;

        if (update)
        {
            for (int ix = -1; ix <= 1; ix++)
            {
                for (int iy = -1; iy <= 1; iy++)
                {
                    UpdateMoutain(x + ix, y + iy);
                }
            }
        }
    }

    //other
    private GameObject TerrToGO(Obj terrain)
    {
        switch (terrain)
        {
            case Obj.StoneOre: return TerrainMining;
            case Obj.CopperOre: return CopperOre;
            case Obj.IronOre: return IronOre;
            case Obj.TerrainFertile: return TerrainFertile;
            case Obj.WaterSource: return WaterSource;
            case Obj.OilSource: return OilSource;
            case Obj.Tree: return Tree;
        }
        return null;
    }
    private bool IsMoutainOn(int x, int y)
    {
        if (x < 0 || x >= mapSize.x || y < 0 || y >= mapSize.y) { return false; }
        return moutainsTab[x, y];
    }

    //moutains
    public void UpdateAllMoutains()
    {
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                UpdateMoutain(x, y);
            }
        }
    }
    public void UpdateMoutain(int x, int y)
    {
        if (!IsMoutainOn(x,y)) { return; }

        Transform trans = transform.Find(string.Format("{0}({1}, {2})", Obj.Mountain, x, y));
        if (trans == null) { Debug.Log("ERROR! Cant get transfor of moutain on x:" + x + " y:" + y); return; }

        bool[,] tab = new bool[3, 3];
        for (int iy = -1; iy <= 1; iy++)
        {
            for (int ix = -1; ix <= 1; ix++)
            {
                if (IsMoutainOn(x + ix, y + iy)) { tab[ix + 1, iy + 1] = true; }
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
                if (tab[0, 0] && tab[0, 2] && tab[2, 2] && tab[2, 0]) { Set(0, i); return; }
                if (tab[0, 0] && tab[0, 2] && !tab[2, 2] && tab[2, 0]) { Set(1, i); return; }
                if (tab[0, 0] && !tab[0, 2] && !tab[2, 2] && tab[2, 0]) { Set(2, i); return; }
                if (!tab[0, 0] && tab[0, 2] && !tab[2, 2] && tab[2, 0]) { Set(3, i); return; }
                if (!tab[0, 0] && !tab[0, 2] && !tab[2, 2] && tab[2, 0]) { Set(4, i); return; }
                if (!tab[0, 0] && !tab[0, 2] && !tab[2, 2] && !tab[2, 0]) { Set(5, i); return; }
            }
            else if (tab[1, 0] && !tab[1, 2] && tab[0, 1] && tab[2, 1])
            {
                if (tab[0, 0] && tab[2, 0]) { Set(6, i); return; }
                if (!tab[0, 0] && !tab[2, 0]) { Set(7, i); return; }
                if (!tab[0, 0] && tab[2, 0]) { Set(8, i); return; }
                if (tab[0, 0] && !tab[2, 0]) { Set(9, i); return; }
            }
            else if (!tab[1, 0] && !tab[1, 2] && tab[0, 1] && tab[2, 1])
            {
                Set(10, i); return;
            }
            else if (tab[1, 0] && !tab[1, 2] && tab[0, 1] && !tab[2, 1])
            {
                if (tab[0, 0]) { Set(11, i); return; }
                if (!tab[0, 0]) { Set(12, i); return; }
            }
            else if (!tab[1, 0] && tab[1, 2] && !tab[0, 1] && !tab[2, 1])
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
            if (n == 0 || n == 14) { angleZ = Random.Range(0, 4) * 90; }
            else { angleZ = i * -90; }
            trans.localRotation = Quaternion.Euler(0, 0, angleZ);

            if (n == 0)
            {
                Obj terr = terrTab[x, y];
                if (terr != Obj.None)
                {
                    Transform terrT = transform.Find(string.Format("{0}({1}, {2})", terr, x, y));
                    if (terrT != null) { terrT.gameObject.SetActive(false); }
                }
            }
        }
    }
}
