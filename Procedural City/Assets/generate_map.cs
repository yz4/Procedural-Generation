using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generate_map : MonoBehaviour {
    
    public GameObject[] buildings;
    public GameObject xstreets;
    public GameObject zstreets;
    public GameObject crossroad;
    public enum DrawMode { NoiseMap, ColorMap, Mesh};
    public DrawMode draw_mode;

    public int width, height, octaves, seed;

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
                if (S[i, j] >= 4)
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
        
        float[,] map = perlinMap.generateMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);
        
        // store whether that area is livable
        int [,] livable = new int [width, height];

        Color[] colormap = new Color[width * height];
        for(int h = 0; h < height; ++h )
        {
            for(int w = 0; w < width; ++w)
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
                        colormap[h * width + w] = regions[i].color;
                        break;
                    }
                }
            }
        }
        
        
        display_map display = FindObjectOfType<display_map>();

        if (draw_mode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(map));
        else if (draw_mode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colormap, width, height));
        else if (draw_mode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.generate_terrain_mesh(map, meshHeightMultiplier, meshHeightCurve),
                TextureGenerator.TextureFromColorMap(colormap, width, height));

            List<int[]> coords = squares(livable);


            ///////////////////////////////////////////////////////////////////////////////////////
            int cityWidth = Mathf.Abs(coords[0][3] - coords[0][1]);
            int cityHeight = Mathf.Abs(coords[0][0] - coords[0][2]);

            Debug.Log(cityWidth);
            Debug.Log(cityHeight);

            int buildingFootprint = -3;
            int[,] citygrid = new int[cityWidth, cityHeight];
            for (int h = 0; h < cityHeight; ++h)
            {
                for (int w = 0; w < cityWidth; ++w)
                {
                    citygrid[w, h] = (int)(Mathf.PerlinNoise(w / 10.0f, h / 10.0f) * 10);
                }
            }
            // build horizontal streets
            int x = 0;
            for (int n = 0; n < 50; ++n)
            {
                for (int h = 0; h < cityHeight; ++h)
                {
                    citygrid[x, h] = -1;
                }
                x += Random.Range(2, 10);
                if (x >= cityWidth) break;
            }
            // build vertical streets
            int z = 0;
            for (int n = 0; n < 10; ++n)
            {
                for (int w = 0; w < cityWidth; ++w)
                {
                    if (citygrid[w, z] == -1)
                        citygrid[w, z] = -3;
                    else
                        citygrid[w, z] = -2;
                }
                z += Random.Range(2, 10);
                if (z >= cityHeight) break;
            }

            // build city

            //float mapY = meshHeightCurve.Evaluate(map[coords[0], coords[1]]) * meshHeightMultiplier;
            float mapY = meshHeightCurve.Evaluate(0.5f) * meshHeightMultiplier;
            for (int h = 0; h < cityHeight; ++h)
            {
                for (int w = 0; w < cityWidth; ++w)
                {

                    int result = citygrid[w, h];
                    Vector3 position = new Vector3(w * buildingFootprint + coords[0][0], mapY, h * buildingFootprint + coords[0][1]);
                    if (result < -2)
                        Instantiate(crossroad, position, crossroad.transform.rotation);
                    else if (result < -1)
                        Instantiate(xstreets, position, xstreets.transform.rotation);
                    else if (result < 0)
                        Instantiate(zstreets, position, zstreets.transform.rotation);
                    else if (result < 1)
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




            /*
            foreach (int[] i in coords)
            {
                int cityWidth = Mathf.Abs(i[3] - i[1]);
                int cityHeight = Mathf.Abs(i[0] - i[2]);

                Debug.Log(cityWidth);
                Debug.Log(cityHeight);

                int buildingFootprint = 3;
                int[,] citygrid = new int[cityWidth, cityHeight];
                for (int h = 0; h < cityHeight; ++h)
                {
                    for (int w = 0; w < cityWidth; ++w)
                    {
                        citygrid[w, h] = (int)(Mathf.PerlinNoise(w / 10.0f, h / 10.0f) * 10);
                    }
                }
                // build horizontal streets
                int x = 0;
                for (int n = 0; n < 50; ++n)
                {
                    for (int h = 0; h < cityHeight; ++h)
                    {
                        citygrid[x, h] = -1;
                    }
                    x += Random.Range(2, 10);
                    if (x >= cityWidth) break;
                }
                // build vertical streets
                int z = 0;
                for (int n = 0; n < 10; ++n)
                {
                    for (int w = 0; w < cityWidth; ++w)
                    {
                        if (citygrid[w, z] == -1)
                            citygrid[w, z] = -3;
                        else
                            citygrid[w, z] = -2;
                    }
                    z += Random.Range(2, 10);
                    if (z >= cityHeight) break;
                }

                // build city

                //float mapY = meshHeightCurve.Evaluate(map[coords[0], coords[1]]) * meshHeightMultiplier;
                float mapY = meshHeightCurve.Evaluate(0.5f) * meshHeightMultiplier;
                for (int h = 0; h < cityHeight; ++h)
                {
                    for (int w = 0; w < cityWidth; ++w)
                    {

                        int result = citygrid[w, h];
                        Vector3 position = new Vector3(w * buildingFootprint + i[0], mapY, h * buildingFootprint + i[1]);
                        if (result < -2)
                            Instantiate(crossroad, position, crossroad.transform.rotation);
                        else if (result < -1)
                            Instantiate(xstreets, position, xstreets.transform.rotation);
                        else if (result < 0)
                            Instantiate(zstreets, position, zstreets.transform.rotation);
                        else if (result < 1)
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
            }*/
        }
    }

    void OnValidate()
    {
        if (width < 1) width = 1;
        if (height < 1) height = 1;
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
