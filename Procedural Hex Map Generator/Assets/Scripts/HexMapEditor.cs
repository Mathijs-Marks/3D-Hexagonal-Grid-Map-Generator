using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// In-game editor to edit properties of each cell.
/// </summary>
public class HexMapEditor : MonoBehaviour
{
    public Color[] colors;
    public HexGrid HexGrid;
    private Color activeColor;
    private int activeElevation;

    private void Awake()
    {
        SelectColor(0);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && 
            !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
    }

    /// <summary>
    /// Shoot a ray into the scene from the mouse position to "touch" a cell.
    /// Edit a cell after touching it.
    /// </summary>
    private void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            EditCell(HexGrid.GetCell(hit.point));
        }
    }

    /// <summary>
    /// Method to edit selected cell.
    /// Change colour to active colour in the editor.
    /// Change elevation to active elevation height in the editor.
    /// Refresh the grid.
    /// </summary>
    /// <param name="cell"></param>
    private void EditCell(HexCell cell)
    {
        cell.color = activeColor;
        cell.Elevation = activeElevation;
        HexGrid.Refresh();
    }

    /// <summary>
    /// Change the cell's colour to the active colour in the editor.
    /// </summary>
    /// <param name="index"></param>
    public void SelectColor(int index)
    {
        activeColor = colors[index];
    }

    /// <summary>
    /// Change the cell's height to the active elevation height in the editor.
    /// </summary>
    /// <param name="elevation"></param>
    public void SetElevation(float elevation)
    {
        activeElevation = (int) elevation;
    }
}
