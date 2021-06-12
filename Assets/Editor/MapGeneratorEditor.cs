using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        MapGenerator mapGenerator = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            mapGenerator.GenerateMap();
        }
        if (GUILayout.Button("Generate"))
        {
            mapGenerator.GenerateMap();
        }

    }
}