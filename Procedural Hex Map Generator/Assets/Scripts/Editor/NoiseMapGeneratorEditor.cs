using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseMapGenerator))]
public class NoiseMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NoiseMapGenerator noiseMapGen = (NoiseMapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (noiseMapGen.autoUpdate)
            {
                noiseMapGen.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            noiseMapGen.GenerateMap();
        }
    }
}
