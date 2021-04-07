using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance;
    void Awake()
    {
        if (instance != null) { Debug.Log("more the one MapGenerator on scen"); return; }
        instance = this;
    }

    public enum DrowMode { None, NoiseMap, ColourMap, Background, Textures }
    public DrowMode drowMode;

    [Header("Values")]
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public Vector2 offset;
    public int octaves;
    [Range(0,1)]public float persistance;
    public float lacunarity;

    [Header("Map")]
    public int seed;
    public int blure = 2;

    public TerrainType[] regions;
    public BacgroundZone[] bacgroundZones;
    public float miningMin;
    public float miningMax;

    private Obj[,] terrTab;

    [Header("Moutains")]
    public Color MountainsColor;
    [Range(0f, 0.6f)] public float MountainsHeight;

    [Header("Update")]
    public bool autoUpdate;

    public Obj[,] GenerateTerrainTab(Vector2 mapSize, int _seed, float oreSizes, float forestSizes)
    {
        mapWidth = (int)mapSize.x;
        mapHeight = (int)mapSize.y;
        seed = _seed;

        float osM = 1.4f;
        oreSizes = (1f - oreSizes) * (osM - 1f) + 1f;

        float fsM = 1.8f;
        forestSizes = (forestSizes - 1f) * (fsM - 1f) + 1f;

        regions[0].height *= forestSizes;
        regions[1].height *= forestSizes;
        regions[2].height *= oreSizes;

        bacgroundZones[0].height *= forestSizes;
        bacgroundZones[1].height *= forestSizes;
        bacgroundZones[2].height *= oreSizes;
        bacgroundZones[3].height *= oreSizes;

        CreateTerrainTiles();

        return terrTab;
    }

    public void GenerateMap()
    {
        switch (drowMode)
        {
            case DrowMode.NoiseMap: CreateTerrainTiles(); break;
            case DrowMode.ColourMap: CreateTerrainTiles(); break;
            case DrowMode.Background: CreateBackgroundWithoutTextures(); break;
            case DrowMode.Textures: CreateBackgroundWithTextures(); break;
        }
    }
    private void CreateTerrainTiles()
    {
        terrTab = new Obj[mapWidth, mapHeight];
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, seed, octaves, persistance, lacunarity, offset, true);

        //map
        Color[] coloursMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        coloursMap[y * mapWidth + x] = regions[i].colour;
                        terrTab[x, y] = regions[i].terrenEnum;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (display == null) { return; }
        switch (drowMode)
        {
            case DrowMode.NoiseMap: display.DrowTexture(TextureFromHeightMap(noiseMap)); break;
            case DrowMode.ColourMap: display.DrowTexture(TextureFromColorMap(coloursMap, mapWidth, mapHeight)); break;
        }
    }
    public void CreateBecground(int _seed, int _mapWidth, int _mapHeight)
    {
        seed = _seed;
        mapWidth = _mapWidth;
        mapHeight = _mapHeight;

        //CreateBackgroundWithTextures();
        CreateBackgroundWithoutTextures();
    }
    private void CreateBackgroundWithTextures()
    {
        terrTab = new Obj[mapWidth, mapHeight];
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, seed, octaves, persistance, lacunarity, offset, true);

        int size = mapWidth * mapHeight;

        Color[] miningRegion = new Color[size];
        Color[] coloursMap = new Color[size];
        Color currColor = new Color();
        int index;
        float currentHeight;
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                currentHeight = noiseMap[x, y];
                
                for (int i = 0; i < bacgroundZones.Length; i++)
                {
                    if (currentHeight <= bacgroundZones[i].height)
                    {
                        currColor = bacgroundZones[i].colour;
                        break;
                    }
                }

                index = y * mapWidth + x;
                coloursMap[index] = currColor;

                if (currentHeight > miningMin && currentHeight <= miningMax)
                {
                    miningRegion[index] = Color.white;
                }
                else
                {
                    miningRegion[index] = Color.clear;
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (display == null) { return; }
        display.DrowTexture2(Blur(TextureFromColorMap(coloursMap, mapWidth, mapHeight), blure), Blur(TextureFromColorMap(miningRegion, mapWidth, mapHeight), 1));
    }
    private void CreateBackgroundWithoutTextures()
    {
        float[,] tilesNoiseMap = Noise.GenerateNoiseMap(this.mapWidth, this.mapHeight, noiseScale, seed, octaves, persistance, lacunarity, offset, true);

        Color[] bacgroundMap = new Color[mapWidth * mapHeight];
        Color currColor = new Color();
        float currentHeight;

        for (int y = 0; y < this.mapHeight; y++)
        {
            for (int x = 0; x < this.mapWidth; x++)
            {
                currentHeight = tilesNoiseMap[x, y];
                for (int i = 0; i < bacgroundZones.Length; i++)
                {
                    if (currentHeight <= bacgroundZones[i].height)
                    {
                        currColor = bacgroundZones[i].colour;
                        break;
                    }
                }

                bacgroundMap[y * mapWidth + x] = currColor;
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (display == null) { return; }
        display.DrowTexture(Blur(TextureFromColorMap(bacgroundMap, mapWidth, mapHeight), blure));
    }
    public bool[,] CreateMoutains(float size)
    {
        MountainsHeight *= size;

        bool[,] tab = new bool[mapWidth, mapHeight];
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale * 3f, seed, octaves, persistance, lacunarity, offset, true);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                if (currentHeight < MountainsHeight) { tab[x, y] = true; }
                else { tab[x, y] = false; }
            }
        }
        return tab;
    }

    private float ReMap(float value, float s1, float e1, float s2, float e2)
    {
        return s2 + (e2 - s2) * ((value - s1) / (e1 - s1));
    }

    public static Texture2D TextureFromColorMap(Color[] colours, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colours);
        texture.Apply();
        return texture;
    }
    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colourMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }
        return TextureFromColorMap(colourMap, width, height);
    }

    private void OnValidate()
    {
        if (mapWidth < 1) { mapWidth = 1; }
        if (mapHeight < 1) { mapHeight = 1; }
        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 0) { octaves = 0; }

    }

    [System.Serializable]
    public struct TerrainType
    {
        public float height;
        public Color colour;
        public Obj terrenEnum;
    }

    [System.Serializable]
    public struct BacgroundZone
    {
        public float height;
        public Color colour;
    }

    private Texture2D Blur(Texture2D image, int blurSize)
    {
        Texture2D blurred = new Texture2D(image.width, image.height);

        float avgR, avgG, avgB, avgA;
        int blurPixelCount, sx, ex, sy, ey;

        // look at every pixel in the blur rectangle
        for (int xx = 0; xx < image.width; xx++)
        {
            for (int yy = 0; yy < image.height; yy++)
            {
                avgR = 0; avgG = 0; avgB = 0; avgA = 0;
                blurPixelCount = 0;

                sx = xx - blurSize; if (sx < 0) { sx = 0; }
                ex = xx + blurSize; if (ex > image.width) { ex = image.width; }
                sy = yy - blurSize; if (sy < 0) { sy = 0; }
                ey = yy + blurSize; if (ey > image.height) { ey = image.height; }

                //for (int x = xx; x < xx + blurSize && x < image.width; x++)
                //{
                //    for (int y = yy; y < yy + blurSize && y < image.height; y++)
                for (int x = sx; x <= ex; x++)
                {
                    for (int y = sy; y <= ey; y++)
                    {
                        Color pixel = image.GetPixel(x, y);

                        avgR += pixel.r;
                        avgG += pixel.g;
                        avgB += pixel.b;
                        avgA += pixel.a;

                        blurPixelCount++;
                    }
                }

                avgR = avgR / blurPixelCount;
                avgG = avgG / blurPixelCount;
                avgB = avgB / blurPixelCount;
                avgA = avgA / blurPixelCount;

                blurred.SetPixel(xx, yy, new Color(avgR, avgG, avgB, avgA));
            }
        }
        blurred.Apply();
        return blurred;
    }
}
