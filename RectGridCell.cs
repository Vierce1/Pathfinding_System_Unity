using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class RectGridCell : Node<Vector3Int>
{
    public bool isWalkable { get;  set; }
    private RectGrid_Viz rectGrid_Viz;

    //construct the node
    //inherits the Vector3Int value from Node base class
    public RectGridCell(RectGrid_Viz gridMap, Vector3Int value) : base(value)
    {
        rectGrid_Viz = gridMap;
        isWalkable = true;
    }

    //get this cell's neighbors
    public override List<Node<Vector3Int>> GetNeighbors()
    {
        return rectGrid_Viz.GetNeighborCells(this);
    }
}
