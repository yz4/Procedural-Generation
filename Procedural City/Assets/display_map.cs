using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class display_map : MonoBehaviour {
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {        
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshdata, Texture2D texture)
    {
        
        meshFilter.sharedMesh = meshdata.createMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        
    }
}
