using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HexCell object that defines what attributes each cell needs to have.
/// </summary>
public class HexCell : MonoBehaviour
{
    /// <summary>
    /// Public property that changes the elevation of a cell.
    /// Value is supplied through the Map Editor elevation slider.
    /// Change the Y value of the cell, according to the elevation value.
    /// Update the cell's position.
    /// Also retrieve the Y position of the coordinate UI element of the cell.
    /// Change the Z value of the UI element to match the elevation of the cell.
    /// Update the UI element's position.
    /// </summary>
    public int Elevation
    {
        get { return elevation; }
        set
        {
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = elevation * -HexMetrics.elevationStep;
            uiRect.localPosition = uiPosition;
        }
    }
    
    public HexCoordinates coordinates;

    public Color color;

    public RectTransform uiRect;

    [SerializeField] private HexCell[] neighbours;

    private int elevation;

    /// <summary>
    /// Retrieve a cell's neighbours in one direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns>A value from 0 to 5 for each of the six neighbours</returns>
    public HexCell GetNeighbour(HexDirection direction)
    {
        return neighbours[(int) direction];
    }

    /// <summary>
    /// Set a cell's neighbour in both directions.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="cell"></param>
    public void SetNeighbour(HexDirection direction, HexCell cell)
    {
        neighbours[(int) direction] = cell;
        cell.neighbours[(int) direction.Opposite()] = this;
    }

    /// <summary>
    /// Retrieve a cell's edge type connection in a certain direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(
            elevation, neighbours[(int)direction].elevation
        );
    }
}
