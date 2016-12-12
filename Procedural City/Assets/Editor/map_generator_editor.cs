using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(generate_map))]
public class map_generator_editor : Editor {
    public override void OnInspectorGUI()
    {
        generate_map mapGen = (generate_map)target;
        if(DrawDefaultInspector())
        {
            if (mapGen.auto_update)
            {
                mapGen.generateMap();
            }
        }
        if (GUILayout.Button("Generate"))
        {
            // delete generated stuff first
            foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
            {
                if (o.tag != "important")
                    DestroyImmediate(o);
            }
            mapGen.generateMap();
        }
        if (GUILayout.Button("Clear"))
        {
            // delete generated stuff first
            foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
            {
                if (o.tag != "important")
                    DestroyImmediate(o);
            }
        }
    }
}
