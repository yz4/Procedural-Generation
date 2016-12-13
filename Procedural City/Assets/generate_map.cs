using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class generate_map : MonoBehaviour {
    
    public GameObject[] buildings;
    public GameObject smaug, grass, tree, mushroom, sheep, boat;
    public enum DrawMode { NoiseMap, ColorMap, Mesh};
    public DrawMode draw_mode;
    
    [Range(0,2)]
    public int levelOfDetails;
    public int octaves, seed;
    public int villageSizeCutoff = 15;
    [Range(0, 1)]
    public float persistence, cityLow, cityHigh;

    public float scale, lacunarity;
    public bool auto_update;
    public TerrainTypes[] regions;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    private int mapChunkSize;

    void Start()
    {
        generateMap(false);
    }

    public static List<int[]> squares(int[,] livable, int cutoff)
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

        int[,] occupied = new int[100, 100];
        bool allSatisfied = true;
        // traverse S
        for (int i = 0; i < R; ++i)
        {
            for (int j = 0; j < C; ++j)
            {
                if (S[i, j] > 5)
                {
                    allSatisfied = true;
                    int[] coords = new int[4] { i, j - S[i, j] + 1, i - S[i, j] + 1, j };
                    Debug.Log("w started at " + i.ToString() + " and ended at " + (i - S[i, j]).ToString());
                    Debug.Log("h started at " + (j - S[i, j] + 1).ToString() + " and ended at " + (j - 1).ToString());
                    for (int w = i - S[i, j] + 1; w < i && allSatisfied; ++w)
                    {
                        for (int h = j - S[i, j] + 1; h < j && allSatisfied; ++h)
                        {
                            Debug.Log("got into loop");
                            if (occupied[w, h] == 1)
                            {
                                Debug.Log("Set to False");
                                allSatisfied = false;
                            }
                            else
                                occupied[w, h] = 1;
                        }
                    }
                    if (allSatisfied)
                        boxes.Add(coords);
                }
            }
        }

        return boxes;
    }

    public void generateMap(bool fill = true)
    {
        // have to force it to low value
        mapChunkSize = (fill ? 101 : 241);

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

            if (fill)
            {

                // instantiate a smaug model on the highest mountain!
                // plant some vegetations!
                // find the max value in map
                int max_w = -1, max_h = -1;
                float max = -1f;

                for (int w = 0; w < mapChunkSize; ++w)
                {
                    for (int h = 0; h < mapChunkSize; ++h)
                    {
                        if (map[w, h] > max)
                        {
                            max = map[w, h];
                            max_w = w;
                            max_h = h;
                        }
                        // generate some stuff on the ground
                        if (map[w, h] >= 0.5f && map[w, h] <= 0.7f)
                        {
                            float dice = Random.Range(0.0f, 1.0f);
                            if (dice < 0.05f)
                            {
                                Vector3 grassPos = new Vector3(w,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    mapChunkSize - h);
                                Instantiate(grass, grassPos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                            }
                            else if (dice < 0.08f)
                            {
                                Vector3 treePos = new Vector3(w,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    mapChunkSize - h);
                                Instantiate(tree, treePos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                            }
                            else if (dice < 0.12f)
                            {
                                Vector3 mushroomPos = new Vector3(w,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    mapChunkSize - h);
                                Instantiate(mushroom, mushroomPos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                            }
                            else if (dice < 0.15f)
                            {
                                Vector3 sheepPos = new Vector3(w,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    mapChunkSize - h);
                                Instantiate(sheep, sheepPos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                            }
                        }
                        else if (map[w, h] <= 0.35f)
                        {
                            float dice = Random.Range(0.0f, 1.0f);
                            if (dice < 0.005f)
                            {
                                Vector3 boatPos = new Vector3(w,
                                    meshHeightCurve.Evaluate(map[w, h]) * meshHeightMultiplier,
                                    mapChunkSize - h);
                                Instantiate(boat, boatPos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                            }
                        }
                    }
                }
                // create position vector for smaug
                Vector3 smaugPos = new Vector3(max_w,
                    meshHeightCurve.Evaluate(map[max_w, max_h]) * meshHeightMultiplier - 2.0f,
                    mapChunkSize - max_h - 2.0f);
                Instantiate(smaug, smaugPos, smaug.transform.rotation);

                // procedurally generate city
                List<int[]> coords = squares(livable, villageSizeCutoff);

                foreach (int[] i in coords)
                {
                    int cityWidth = Mathf.Abs(i[3] - i[1]);
                    int cityHeight = Mathf.Abs(i[0] - i[2]);
                    int buildingFootprint = 2;

                    int[,] citygrid = new int[cityWidth, cityHeight];
                    for (int h = 0; h < cityHeight; ++h)
                    {
                        for (int w = 0; w < cityWidth; ++w)
                        {
                            citygrid[w, h] = (int)(Mathf.PerlinNoise(w / 10.0f, h / 10.0f) * 10.0f);
                        }
                    }

                    // build city

                    float mapY = meshHeightCurve.Evaluate(map[i[0], i[1]]) * meshHeightMultiplier;
                    for (int h = 0; h < cityHeight; h++)
                    {
                        for (int w = 0; w < cityWidth; w++)
                        {

                            int result = citygrid[w, h];
                            Debug.Log(result);
                            Vector3 position = new Vector3(w * buildingFootprint + i[0], mapY, mapChunkSize - (h * buildingFootprint + i[1]));
                            if (w * buildingFootprint < cityWidth && h * buildingFootprint < cityHeight)
                            {
                                if (result < 2)
                                    Instantiate(buildings[0], position, Quaternion.Euler(-90.0f, 0.0f, 0.0f));
                                else if (result < 4)
                                    Instantiate(buildings[1], position, Quaternion.Euler(-90.0f, 0.0f, 0.0f));
                                else if (result < 6)
                                    Instantiate(buildings[2], position, Quaternion.Euler(-90.0f, 0.0f, 0.0f));
                                else if (result < 8)
                                    Instantiate(buildings[3], position, Quaternion.Euler(-90.0f, 0.0f, 0.0f));
                                else if (result < 10)
                                    Instantiate(buildings[4], position, Quaternion.Euler(-90.0f, 0.0f, 180.0f));
                            }
                        }
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
