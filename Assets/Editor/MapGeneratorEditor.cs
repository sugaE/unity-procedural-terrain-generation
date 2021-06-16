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

        if (DrawDefaultInspector() && mapGenerator.autoUpdate)
        {
            mapGenerator.DrawMapInEditor();
        }
        if (GUILayout.Button("Generate"))
        {
            mapGenerator.DrawMapInEditor();
        }

    }

}
