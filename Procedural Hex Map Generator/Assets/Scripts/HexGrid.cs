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

        CreateMap();
    }

    /// <summary>
    /// Triangulate cells for the grid. 
    /// </summary>
    private void Start()
    {
        hexMesh.Triangulate(cells);
    }

    public void CreateMap()
    {
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

        /*
         * Upon creation of the cell, do the following checks to assign neighbours:
         */

        // Check if the cell has any neighbours to the west. When x = 0 it is the leftmost cell.
        if (x > 0)
        {
            cell.SetNeighbour(HexDirection.W, cells[i - 1]);
        }

        // Check if the cell is on any row higher than the lowest one.
        if (z > 0)
        {
            // Using a bitwise AND operator, check if the row is even or odd.
            if ((z & 1) == 0)
            {
                // If the row is even, set the southeast neighbour of the cell.
                cell.SetNeighbour(HexDirection.SE, cells[i - width]);
                if (x > 0)
                {
                    // If it is also not the leftmost cell, set its southwest neighbour.
                    cell.SetNeighbour(HexDirection.SW, cells[i - width - 1]);
                }
            }
            else
            {
                // If the row is odd, set the southwest neighbour of the cell.
                cell.SetNeighbour(HexDirection.SW, cells[i - width]);
                if (x < width - 1)
                {
                    // Mirror the same logic for the odd row, setting the southeast neighbour.
                    cell.SetNeighbour(HexDirection.SE, cells[i - width + 1]);
                }
            }
        }

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();

        // Reposition UI labels when changing elevation.
        cell.uiRect = label.rectTransform;
    }

    /// <summary>
    /// Method to select individual cells.
    /// Convert cell coordinates to the appropriate array index.
    /// Return the index of the cells array.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        return cells[index];
    }

    /// <summary>
    /// Triangulate all cells when hitting a refresh.
    /// </summary>
    public void Refresh()
    {
        hexMesh.Triangulate(cells);
    }
}
