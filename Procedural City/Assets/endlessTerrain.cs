using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endlessTerrain : MonoBehaviour {
    public const float max_view_dist = 300.0f;
    public Transform viewer;
    public static Vector2 viewPos;
    int chunkSize, chunkVisibleDist;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();

    void Start()
    {
        chunkSize = generate_map.mapChunkSize - 1;
        chunkVisibleDist = Mathf.RoundToInt(max_view_dist / chunkSize);
    }

    void Update()
    {
        viewPos = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewPos.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewPos.y / chunkSize);

        for(int yOffset = -chunkVisibleDist; yOffset <= chunkVisibleDist; ++yOffset)
        {
            for (int xOffset = -chunkVisibleDist; xOffset <= chunkVisibleDist; ++yOffset)
            {
                Vector2 viewerChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if(terrainChunkDict.ContainsKey(viewerChunkCoord))
                {
                    terrainChunkDict[viewerChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDict.Add(viewerChunkCoord, new TerrainChunk(viewerChunkCoord, chunkSize));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 pos;
        Bounds bounds;
        public TerrainChunk(Vector2 coord, int size)
        {
            pos = coord * size;
            bounds = new Bounds(pos, Vector2.one * size);
            Vector3 pos3D = new Vector3(pos.x, 0, pos.y);
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = pos3D;
            meshObject.transform.localScale = Vector3.one * size / 10.0f;
            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            float viewDistFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewPos));
            bool visible = viewDistFromEdge <= max_view_dist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
    }
}
