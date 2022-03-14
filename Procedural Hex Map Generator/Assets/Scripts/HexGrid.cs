using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Create Hex grid component composed of Hex cell prefabs.
/// Give the grid a width and height, use the cell prefab to fill the grid with cells.
/// Each cell can be touched, which shows the cell coordinates and colors them magenta.
/// </summary>
public class HexGrid : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    public Color defaultColor = Color.white;
    //public Color touchedColor = Color.magenta;

    public HexCell cellPrefab;
    private HexCell[] cells;

    public Text cellLabelPrefab;
    private Canvas gridCanvas;

    private HexMesh hexMesh;

    /// <summary>
    /// Determine the size of the cell grid and fill with cells.
    /// Visualize cell coordinates using a canvas.
    /// Retrieve the hex mesh.
    /// </summary>
    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

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
    /// Triangulate cells for the grid. 
    /// </summary>
    private void Start()
    {
        hexMesh.Triangulate(cells);
    }

    /// <summary>
    /// Each cell created will have their xz sizes equal 10, y size is zero to make it lay flat.
    /// Position the cell on the grid accordingly.
    /// For each cell, render a UI text element, showing the coordinates of the cell.
    /// Use HexCoordinates struct to display the correct cell coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="i"></param>
    private void CreateCell(int x, int z, int i)
    {
        /*
         * Distance between adjacent hexagon cells in the X direction is equal to twice the inner radius.
         * Distance to the next row of cells should be 1.5 times te outer radius.
         * Hexagon rows are not stacked directly above one another.
         * Instead each row is offset along the X axis by the inner radius.
         * To prevent each row offsetting only to the right, every second row all cells should be moved back one additional step.
         */
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
    }

    /// <summary>
    /// Once the ray hits the cell,
    /// convert cell coordinates to the appropriate array index.
    /// Then grab the cell, change its color, triangulate the mesh again.
    /// </summary>
    /// <param name="position"></param>
    public void ColorCell(Vector3 position, Color color)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        HexCell cell = cells[index];
        cell.color = color;
        hexMesh.Triangulate(cells);
    }
}
