using UnityEngine;
using System.Collections;

public class generate_city : MonoBehaviour {

    // hold all buildings
    public GameObject[] buildings;
    public GameObject xstreets;
    public GameObject zstreets; // two perpendicular street types
    public GameObject crossroad;
    public int mapWidth = 20;
    public int mapHeight = 20;
    // space buildings out
    public int buildingFootprint = 3;
    int[,] mapgrid;
    

	// Use this for initialization
	void Start () {
        mapgrid = new int[mapWidth, mapHeight];
        // fill in the map data into the grid
        for(int h = 0; h < mapHeight; ++h)
        {
            for(int w = 0; w < mapWidth; ++w)
            {
                mapgrid[w, h] = (int)(Mathf.PerlinNoise(w / 10.0f, h / 10.0f) * 10);
            }
        }
        // build x-streets
        int x = 0;
        for(int n = 0; n < 50; ++n)
        {
            for(int h = 0; h < mapHeight; ++h)
            {
                mapgrid[x, h] = -1; // -1 means horizontal streets
            }
            x += Random.Range(2, 10);
            if (x >= mapWidth) break;
        }
        // build z-streets
        int z = 0;
        for (int n = 0; n < 10; ++n)
        {
            for (int w = 0; w < mapWidth; ++w)
            {
                if (mapgrid[w, z] == -1) // already have a x-street here
                    mapgrid[w, z] = -3;  // then put a crossroad here
                else
                    mapgrid[w, z] = -2;
            }
            z += Random.Range(2, 10);
            if (z >= mapHeight) break;
        }
        // build city
        for (int h = 0; h < mapHeight; ++h)
        {
            for(int w = 0; w < mapWidth; ++w)
            {
                int result = mapgrid[w, h];
                Vector3 pos = new Vector3(w * buildingFootprint, 0, h * buildingFootprint);
                if (result < -2)
                    Instantiate(crossroad, pos, crossroad.transform.rotation);
                else if(result < -1)
                    Instantiate(xstreets, pos, xstreets.transform.rotation);
                else if (result < -1)
                    Instantiate(xstreets, pos, xstreets.transform.rotation);
                else if (result < 0)
                    Instantiate(zstreets, pos, zstreets.transform.rotation);
                else if (result < 1)
                    Instantiate(buildings[0], pos, Quaternion.identity);
                else if (result < 2)
                    Instantiate(buildings[1], pos, Quaternion.identity);
                else if (result < 4)
                    Instantiate(buildings[2], pos, Quaternion.identity);
                else if (result < 6)
                    Instantiate(buildings[3], pos, Quaternion.identity);
                else if (result < 7)
                    Instantiate(buildings[4], pos, Quaternion.identity);
                else if (result < 10)
                    Instantiate(buildings[5], pos, Quaternion.identity);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
