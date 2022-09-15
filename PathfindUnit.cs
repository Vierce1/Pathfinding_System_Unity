using System;
using Pathfinding;

//attach this to a GameObject to make it pathfind
public class PathfindUnit
{

    ///add these to the unit's initialization function
    ///        //pathfinding functions
    //rg_viz = FindObjectOfType<RectGrid_Viz>();
    //pathFinder.onSuccess = OnSuccessPathFinding;
    //pathFinder.HeuristicCost = RectGrid_Viz.GetManhattanCost;
    //pathFinder.NodeTraversalCost = RectGrid_Viz.GetEuclideanCost;

    public PathFinder<Vector3Int> pathFinder = new GreedyPathFinder<Vector3Int>();
    public RectGrid_Viz rg_viz;
    public List<Vector3Int> wayPoints = new List<Vector3Int> { };
    public bool pathFindingSuccessful = false;
    public bool reachedWaypoint = false;
    public bool unitCurrentlyMoving = false;
    public List<Vector3Int> previousPathCells = new List<Vector3Int>(); //add cells to this as generate paths. whether final destination is good or not


    //Call this in Update() when (pathFinder.Status != PathFinderStatus.RUNNING && !unitCurrentlyMoving)
    //Get goalLocation as a RectGridCell by calling RectGrid_Viz.GetRGC(Vector3Int, 0, Vector3Int)
    public void PathFind(RectGridCell goalLocation)
    {
        Debug.Log("Beginning Pathfind");
        reachedWaypoint = false;
        SetDestination(goalLocation);
    }


    public void SetDestination(RectGridCell destination)
    {
        if (pathFinder.Status == PathFinderStatus.RUNNING)
        {
            return;
        }

        wayPoints.Clear();
        RectGridCell start = rg_viz.GetRGC(
             Mathf.RoundToInt(transform.position.x),
             Mathf.RoundToInt(transform.position.z));
        Debug.Log("Start = " + start.Value);
        if (start == null) { return; }

        //initialize and start the alogorithm
        pathFinder.Initialize(start, destination);
        StartCoroutine(FindPathSteps());
    }

    IEnumerator FindPathSteps()
    {
        while (pathFinder.Status == PathFinderStatus.RUNNING)
        {
            pathFinder.Step();
        }
        yield return null;
    }


    //if a successful path is found, this method is invoked
    void OnSuccessPathFinding()
    {
        PathFinder<Vector3Int>.PathFinderNode node = pathFinder.CurrentNode;
        List<Vector3Int> reverseIndices = new List<Vector3Int> { };
        while (node != null)
        {
            //need to take gridSize into account
            reverseIndices.Add(node.Location.Value * rg_viz.gridSize);
            node = node.Parent;
        }
        //reverse indices starts with goal node and works back through parent nodes. So we feed it to the queue backwards
        for (int i = reverseIndices.Count - 1; i >= 0; i--)
        {
            AddWayPoint(new Vector3Int(
                reverseIndices[i].x, 2, reverseIndices[i].z));
        }

        //we should only reach here at the end
        finishedPathFinding = true;
        pathFindingSuccessful = true;
        StartCoroutine(MoveTo());
    }

    public void AddWayPoint(Vector3Int pt)
    {
        wayPoints.Add(pt);
        //Debug.Log("Adding pt " + pt.x + "x  " + pt.y + "y");
    }

    public IEnumerator MoveTo()
    {
        unitCurrentlyMoving = true;
        for (int i = 0; i < wayPoints.Count; i++)
        {
            reachedWaypoint = false;
            StartCoroutine(MoveToPoint(wayPoints[i]));
            yield return new WaitUntil(() => reachedWaypoint);
        }
        //finished moving. Can use this bool to trigger actions if needed
        unitCurrentlyMoving = false;
    }

    IEnumerator MoveToPoint(Vector3 pt)
    {
        Vector3 endP = new Vector3(pt.x, 2, pt.z);
        StartCoroutine(MoveOverTime(endP));
        yield return null;
    }

    //This method moves the unit directly
    //Could also rb.AddForce in the direction of the goal
    IEnumerator MoveOverTime(Vector3 end)
    {
        float time = 0;
        Vector3 startingPos = transform.position;
        Vector3 currentPos = transform.position;
        while (currentPos != end)
        {
            transform.position = Vector3.Lerp(startingPos, end, time);
            time += Time.deltaTime * 4;
            currentPos = transform.position;
            yield return null;
        }
        reachedWaypoint = true;
    }
}
