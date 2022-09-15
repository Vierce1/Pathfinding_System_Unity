using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


//This class needs to be attached to an empty GameObject
//NOTE: Everywhere you see Vector3Int can be changed to Vector2Int
// if you wish to use this in 2D.
// You will probably also need to edit some of the Vector input
// values in the PathfindingUnit class
public class RectGrid_Viz : MonoBehaviour 
{
    //fill these two variables in the editor or programatically
    public int maxNumOfColumns;
    public int maxNumOfRows;

    //attach the prefab in the editor. Can make the sprites invisible after testing
    [SerializeField] GameObject rectGridCellPrefab;
    public GameObject[,] rGCGameObjects;

    //2d array of Vector3Int = stores 2d indices of the grid cells
    Vector3Int[,] mIndices;
    public RectGridCell[,] rGCells;

    public bool finishedGrid = false;

    //adjust gridSize to make the cells larger. Useful on larger, more spread out maps    
    public int gridSize = 4;


    //create the grid
    protected void ConstructGrid(int numX, int numZ)
    {
        maxNumOfRows = numX;
        maxNumOfColumns = numZ;

        //create a rectangular grid based on max num of rows and columns
        //using [,] like this means all rows are the same size. [][] you can create
        //different size rows, but they all have to be initiated individually
        mIndices = new Vector3Int[numX,  numZ];
        rGCGameObjects = new GameObject[numX, numZ];
        rGCells = new RectGridCell[numX, numZ];

        //create grid cells (index data), plus the gameobject visual cells
        for (int i = 0; i < numX; i++)
        {
            for (int j = 0; j < numZ; j++)
            {
                mIndices[i,  j] = new Vector3Int(i, 2, j); //lift them off the ground

                //game object visual representations of the grid cells.
                rGCGameObjects[i, j] = Instantiate(rectGridCellPrefab, new Vector3(i * gridSize, 0.1f, j * gridSize), Quaternion.identity);
                rGCGameObjects[i, j].transform.SetParent(transform);
                rGCGameObjects[i, j].transform.localScale = Vector3.one * 8 * gridSize;
                rGCGameObjects[i, j].name = "cell_" + i + "_" + j;
                rGCGameObjects[i, j].transform.Rotate(new Vector3(90, 0, 0));

                //create the rect grid cells. mIndices reference is passing the Vector3Int
                //These are the 'virtual' cells that units will pathfind with, vs. the
                //physical representations we spawned above
                rGCells[i, j] = new RectGridCell(this, mIndices[i , j]);                

                //set ref to the RectGrid cell visualization if needed
                RectGridCell_Viz rGC_viz = rGCGameObjects[i, j].GetComponent<RectGridCell_Viz>();
                if (rGC_viz != null)
                {
                    //assign the RectGridCell to the RectGridCell_Viz reference
                    rGC_viz.rgc = rGCells[i, j];                    
                }

            }
        }

        finishedGrid = true;
        //now safe to start pathfinding
    }


    private void Start()
    {
        ConstructGrid(maxNumOfRows, maxNumOfColumns);
    }


    //get neighbors for a cell - if you don't want the unit 'cutting corners'
    // just comment out the diagonal functions below (e.g., 'check Top-Right')
    public List<Node<Vector3Int>> GetNeighborCells(Node<Vector3Int> location)
    {
        List<Node<Vector3Int>> neighbors = new List<Node<Vector3Int>> { };
        int x = location.Value.x; 
        int z = location.Value.z;

        //check UP
        if (z < maxNumOfColumns - 1)
        {
            int i = x;
            int j = z + 1;

            if (rGCells[i, j].isWalkable)
            {
                neighbors.Add(rGCells[i, j]);
            }
        }


        //check Top-Right
        if (z < maxNumOfColumns - 1 && x < maxNumOfRows - 1)
        {
            int i = x + 1;
            int j = z + 1;
            if (rGCells[i,  j].isWalkable)
            {
                neighbors.Add(rGCells[i,  j]);
            }
        }

        //check right
        if (x < maxNumOfRows - 1)
        {
            int i = x + 1;
            int j = z;
            if (rGCells[i, j].isWalkable)
            {
                neighbors.Add(rGCells[i, j]);
            }
        }

        ////check right-down
        if (x < maxNumOfRows - 1 && z > 0)
        {
            int i = x + 1;
            int j = z - 1;
            if (rGCells[i,  j].isWalkable)
            {
                neighbors.Add(rGCells[i,  j]);
            }
        }

        //check down
        if (z > 0)
        {
            int i = x;
            int j = z - 1;
            if (rGCells[i, j].isWalkable)
            {
                neighbors.Add(rGCells[i, j]);
            }
        }

        //check down left
        if (x > 0 && z > 0)
        {
            int i = x - 1;
            int j = z - 1;
            if (rGCells[i, j].isWalkable)
            {
                neighbors.Add(rGCells[i,j]);
            }
        }

        //check left
        if (x > 0)
        {
            int i = x - 1;
            int j = z;
            if (rGCells[i, j].isWalkable)
            {
                neighbors.Add(rGCells[i, j]);
            }
        }

        //check left up
        if (x > 0 && z < maxNumOfColumns - 1)
        {
            int i = x - 1;
            int j = z + 1;
            if (rGCells[i,  j].isWalkable)
            {
                neighbors.Add(rGCells[i, j]);
            }
        }

        return neighbors;
        //this returns all possible neighbors, except those that are marked unwalkable

    }


    #region RayCast toggling
    //comment this IN if you want to click on a cell and toggle it's walkable state
    //may need some other event system tweaking

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        RayCastToggleWalk();
    //    }
    //}
    //public void RayCastToggleWalk()
    //{
    //    Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
    //    RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0);
    //}
    #endregion


    //toggle whether cell is walkable. Call this in whatever method determines
    // which cells should be traversable, e.g. your terrain creation process
    public void ToggleWalkable(RectGridCell_Viz cell)
    {
        if(cell == null) { Debug.Log("Cell null"); return; }
        int x = (int)cell.rgc.Value.x;
        int y = (int)cell.rgc.Value.y;
       
        cell.rgc.isWalkable  =   !cell.rgc.isWalkable;

        //change the visual color of the cell to show isWalkable state
        //comment this out if you don't want to see the visual cell
        if (!cell.rgc.isWalkable)
        { 
            cell.SetInnerColor(cell.nonWalkableCol);
        }
        else 
        { 
            cell.ResetColor();
        }
    }


    public static float GetManhattanCost(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public static float GetEuclideanCost(Vector3Int a, Vector3Int b)
    {
        return GetCostBetweenTwoCells(a, b);
    }
    public static float GetCostBetweenTwoCells(Vector3Int a, Vector3Int b)
    {
        return Mathf.Sqrt((a.x - b.x) * (a.x - b.x) + 
                         (a.y - b.y) * (a.y - b.y));
    }

    public RectGridCell GetRGC(int x, int z)
    {
        x = x / gridSize;
        z = z / gridSize;
        if (x  >= 0 && x  < maxNumOfRows && z  >= 0 && z  < maxNumOfColumns)
        {
            return rGCells[x, z];
        }
        return null;
    }
}
