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
            mapGen.generateMap();
        }
    }
}
