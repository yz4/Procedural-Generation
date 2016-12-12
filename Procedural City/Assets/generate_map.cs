using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class generate_map : MonoBehaviour {
    
    public GameObject[] buildings;
    public GameObject xstreets;
    public GameObject zstreets;
    public GameObject crossroad;
    public enum DrawMode { NoiseMap, ColorMap, Mesh};
    public DrawMode draw_mode;
    public const int mapChunkSize = 100;
    [Range(0,6)]
    public int levelOfDetails;
    public int octaves, seed;

    [Range(0, 1)]
    public float persistence, cityLow, cityHigh;

    public float scale, lacunarity;
    public bool auto_update;
    public TerrainTypes[] regions;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    void Start()
    {
        generateMap();
    }

    public static List<int[]> squares(int[,] livable)
    {
        int R = livable.GetLength(0);
        int C = livable.GetLength(1);
        int[,] S = new int[R, C];
        for (int i = 0; i < R; ++i)
        {
            S[i, 0] = livable[i, 0];
        }
        for (int i = 0; i < C; ++i)
        {
            S[0, i] = livable[0, i];
        }
        for (int i = 1; i < C; ++i)
        {
            for (int j = 1; j < R; ++j)
            {
                if (livable[i, j] == 1)
                    S[i, j] = Mathf.Min(S[i, j - 1], S[i - 1, j], S[i - 1, j - 1]) + 1;
                else
                    S[i, i] = 0;
            }
        }

        // data structure for storing square coordinates
        List<int[]> boxes = new List<int[]>();
        // traverse S
        for (int i = 0; i < R; ++i)
        {
            for (int j = 0; j < C; ++j)
            {
                if (S[i, j] > 5)
                {
                    int[] coords = new int[4] { i, j - S[i, j] + 1, i - S[i, j] + 1, j };
                    boxes.Add(coords);
                    print("Bottom-left: (" + coords[0].ToString() + ", " + coords[1].ToString() + 
                        "); Top-right: (" + coords[2].ToString() + ", " + coords[3].ToString() + ")");
                }
            }
        }        
        
        // TO DO: detect box collision
        return boxes;
    }

    public void generateMap()
    {        
        float[,] map = perlinMap.generateMap(mapChunkSize, mapChunkSize, seed, scale, octaves, persistence, 
            lacunarity, offset);
        
        // store whether that area is livable
        int [,] livable = new int [mapChunkSize, mapChunkSize];

        Color[] colormap = new Color[mapChunkSize * mapChunkSize];
        for(int h = 0; h < mapChunkSize; ++h )
        {
            for(int w = 0; w < mapChunkSize; ++w)
            {
                // begin searching for livable areas
                if (map[w, h] <= cityHigh && map[w, h] >= cityLow)
                    livable[w, h] = 1;
                else
                    livable[w, h] = 0;         

                float currentHeight = map[w, h];
                for(int i = 0; i < regions.Length; ++i)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colormap[h * mapChunkSize + w] = regions[i].color;
                        break;
                    }
                }
            }
        }
                
        display_map display = FindObjectOfType<display_map>();

        if (draw_mode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(map));
        else if (draw_mode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colormap, mapChunkSize, mapChunkSize));
        else if (draw_mode == DrawMode.Mesh)
        {            
         
            display.DrawMesh(MeshGenerator.generate_terrain_mesh(map, meshHeightMultiplier,
                levelOfDetails, meshHeightCurve),
                TextureGenerator.TextureFromColorMap(colormap, mapChunkSize, mapChunkSize));
            
            //    instantiate a smaug model on the highest mountain!
            int [,] a = { { 1, 2, 3 }, { 4, 5, 6 } };
            //a.Max();

            List<int[]> coords = squares(livable);
            
            foreach (int[] i in coords)
            {
                int cityWidth = Mathf.Abs(i[3] - i[1]);
                int cityHeight = Mathf.Abs(i[0] - i[2]);

                Debug.Log(cityWidth);
                Debug.Log(cityHeight);

                int buildingFootprint = 1;
                int[,] citygrid = new int[cityWidth, cityHeight];
                for (int h = 0; h < cityHeight; ++h)
                {
                    for (int w = 0; w < cityWidth; ++w)
                    {
                        citygrid[w, h] = (int)(Mathf.PerlinNoise(w / 10.0f, h / 10.0f) * 10);
                    }
                }
               
                // build city

                //float mapY = meshHeightCurve.Evaluate(map[coords[0], coords[1]]) * meshHeightMultiplier;
                float mapY = meshHeightCurve.Evaluate(0.5f) * meshHeightMultiplier;
                for (int h = 0; h < cityHeight / buildingFootprint; ++h)
                {
                    for (int w = 0; w < cityWidth / buildingFootprint; ++w)
                    {

                        int result = citygrid[w, h];
                        Vector3 position = new Vector3(w * buildingFootprint + i[0], mapY, mapChunkSize - (h * buildingFootprint + i[1]));
                        if (result < 1)
                            Instantiate(buildings[0], position, Quaternion.identity);
                        else if (result < 2)
                            Instantiate(buildings[1], position, Quaternion.identity);
                        else if (result < 4)
                            Instantiate(buildings[2], position, Quaternion.identity);
                        else if (result < 6)
                            Instantiate(buildings[3], position, Quaternion.identity);
                        else if (result < 10)
                            Instantiate(buildings[4], position, Quaternion.identity);
                    }
                }
            }
        }
    }

    void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }
}

[System.Serializable]
public struct TerrainTypes {
    public string name;
    public float height;
    public Color color;
}
