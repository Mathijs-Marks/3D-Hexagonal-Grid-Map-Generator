using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Create Hex grid component composed of Hex cell prefabs.
/// Give the grid a width and height, use the cell prefab to fill the grid with cells.
/// </summary>
public class HexGrid : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    public HexCell cellPrefab;
    private HexCell[] cells;

    public Text cellLabelPrefab;
    private Canvas gridCanvas;

    /// <summary>
    /// Determine the size of the cell grid and fill with cells.
    /// Visualize cell coordinates using a canvas.
    /// </summary>
    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();

        cells = new HexCell[height * width];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    /// <summary>
    /// Each cell created will have their xz sizes equal 10, y size is zero to make it lay flat.
    /// Position the cell on the grid accordingly.
    /// For each cell, render a UI text element, showing the coordinates of the cell.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="i"></param>
    private void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = x * 10f;
        position.y = 0f;
        position.z = z * 10f;

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = x.ToString() + "\n" + z.ToString();
    }
}
