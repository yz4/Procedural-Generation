using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class perlinMap {
    public static float [,] generateMap(int width, int height, int seed, 
        float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octave_offsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octave_offsets[i] = new Vector2(offsetX, offsetY);
        }
        if (scale <= 0.0f) scale = 0.01f;

        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;                
        float[,] map = new float[width, height];

        for(int w = 0; w < width; ++w)
        {
            for(int h = 0; h < height; ++h)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int o = 0; o < octaves; ++o)
                {
                    // alow perlin value to be from [-1, 1]
                    float perlinValue = Mathf.PerlinNoise((w - width / 2f) / scale * frequency + octave_offsets[o].x, 
                        (h - height / 2f) / scale * frequency + octave_offsets[o].y) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxHeight)
                    maxHeight = noiseHeight;
                else if (noiseHeight < minHeight)
                    minHeight = noiseHeight;
                map[w, h] = noiseHeight;    
            }
        }
        // normalize the map value back to [0,1] before returning
        for (int w = 0; w < width; ++w)
        {
            for (int h = 0; h < height; ++h)
            {
                map[w, h] = Mathf.InverseLerp(minHeight, maxHeight, map[w, h]);
            }
        }
        return map;
    }
}
