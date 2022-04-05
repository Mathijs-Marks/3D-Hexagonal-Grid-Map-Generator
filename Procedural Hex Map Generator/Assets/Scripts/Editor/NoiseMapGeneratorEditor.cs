using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor that features a "generate" button for generating a new map.
/// </summary>
[CustomEditor(typeof(NoiseMapGenerator))]
public class NoiseMapGeneratorEditor : Editor
{
    /// <summary>
    /// Get a reference to the map generator.
    /// Draw the default inspector.
    /// If the autoUpdate bool is true, generate the map in realtime.
    /// Add a button to generate the map when it's pressed.
    /// </summary>
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
