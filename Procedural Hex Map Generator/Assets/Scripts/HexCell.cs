using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HexCell object that defines what attributes each cell needs to have.
/// </summary>
public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;

    public Color color;

    [SerializeField] private HexCell[] neighbours;

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
}
