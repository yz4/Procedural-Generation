﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {

	public static MeshData generate_terrain_mesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f; 
        MeshData meshdata = new MeshData(width, height);
        int vertexIndex = 0;
        for(int h = 0; h < height; ++h)
        {
            for (int w = 0; w < width; ++w)
            {
                meshdata.vertices[vertexIndex] = new Vector3(topLeftX + w, heightCurve.Evaluate(heightMap[w, h]) * heightMultiplier, topLeftZ - h);
                meshdata.UVs[vertexIndex] = new Vector2(w / (float)width, h / (float)height);
                if(w < width - 1 && h < height - 1)
                {
                    meshdata.addTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshdata.addTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }
        }
        return meshdata;
    }    
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] UVs;
    int triangleIndex;

    public MeshData(int width, int height)
    {
        vertices = new Vector3[width * height];
        UVs = new Vector2[width * height];
        triangles = new int[(width - 1) * (height - 1) * 6];
    }

    public void addTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh createMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();
        return mesh;
    }
}