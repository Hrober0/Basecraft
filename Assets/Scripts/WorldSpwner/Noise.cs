using UnityEngine;

public static class Noise
{
    public static float maxNoiseHeight;
    public static float minlNoicsHeight;

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int seed, int octaves, float persistance, float lacunarity, Vector2 offset, bool local)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octavesOffsets = new Vector2[octaves];

        float maxPosibleHeight = 0;
        float amplitude = 1;
        float frecquency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octavesOffsets[i] = new Vector2(offsetX, offsetY);

            maxPosibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0) { scale = 0.0001f; }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoicsHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for(int y=0; y < mapHeight; y++)
        {
            for(int x=0; x < mapWidth; x++)
            {
                amplitude = 1;
                frecquency = 1;
                float noiseheight = 0;

                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = (x- halfWidth + octavesOffsets[i].x) / scale * frecquency;
                    float sampleY = (y-halfHeight + octavesOffsets[i].y) / scale * frecquency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseheight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frecquency *= lacunarity;
                }

                if (noiseheight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseheight;
                }
                else if(noiseheight < minLocalNoicsHeight)
                {
                    minLocalNoicsHeight = noiseheight;
                }

                noiseMap[x, y] = noiseheight;
            }
        }
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (local) { noiseMap[x, y] = Mathf.InverseLerp(minLocalNoicsHeight, maxLocalNoiseHeight, noiseMap[x, y]); }
                else { noiseMap[x, y] = (noiseMap[x, y] + 1) / (2f * maxPosibleHeight / 1.2f); }
            }
        }

        minlNoicsHeight = minLocalNoicsHeight;
        maxNoiseHeight = maxLocalNoiseHeight;

        return noiseMap;
    }
}
