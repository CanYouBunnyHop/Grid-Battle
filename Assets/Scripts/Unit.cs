using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;
using System;

public abstract class Unit : MonoBehaviour
{
    [SerializeField]float speed = 5;
    [SerializeField]public bool isFollowing = false;

    public int movementMaxRange = 60;
    public List<int> rangeUsed = new List<int>();
    public int RangeUsedSum => rangeUsed.Any()? rangeUsed.Sum() : 0;
    public int MovementRangeLeft => movementMaxRange - RangeUsedSum;

    ///<summary> index of cell in path, Cell[] </summary>
    [SerializeField]int targetIndex;
    public Cell UnitOnCell() 
    {
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1, cellLayer))
        return hit.collider.GetComponent<Cell>();
        
        else return null;
    } 
    public List<Cell> targetCells = new();
    private HashSet<Cell> inSquareRangeCells = new();
    private HashSet<Cell> inRangeCells = new();
    public HashSet<Cell> inMovementRangeCells = new(); //=> targetCells.Any()? newInMovementRangeCells : defaultInMovementRangeCells;
    //public HashSet<Cell> defaultInMovementRangeCells = new();
    //public HashSet<Cell> newInMovementRangeCells = new();
    public LayerMask cellLayer;
    private InputManager InputManager => InputManager.theInputManager;
    private GridManager Grid => GridManager.theGridManager;
    public Queue<IEnumerator> followReq = new();
    public List<Cell> pathFindCells = new();

    //public bool isProcessingCells;
    public Cell TargetDestination() => pathFindCells.Any() ? pathFindCells.Last() : UnitOnCell();
    
    public Unit ChaseTarget;
    
    public List<Vector3> pathFindV3 = new();

    public LineRenderer lineRdr;

    [Header("TileMap Properties")]
    public Tilemap tilemap;
    public TileBase overlay;

    public virtual void OnMouseDown() //mouse down on unit
    {
        if(PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        {
           IsSelectedAsChaseTarget();
        }
    }
    protected void IsSelectedAsChaseTarget()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            Unit selectedPlayer = InputManager.SelectedPlayer;
            //selected unit is not this, and there is no targetcells
            if(selectedPlayer != this && !selectedPlayer.targetCells.Any()) 
            {
                selectedPlayer.ChaseTarget = this;
            }
        }
    }
    public void ChaseDownTarget()
    {
        Cell start = UnitOnCell();
        var endCell = ChaseTarget.ChaseTarget == this? ChaseTarget.UnitOnCell() : ChaseTarget.TargetDestination(); 
        var end = new List<Cell>(){endCell};
        PathRequestManager.thePathReqManager.RequestPathFindings(start, end, OnPathFound);

        void OnPathFound(Cell[] _newPath, bool _pathSuccess)
        {
            if(_pathSuccess)
            {
                //Shave off the cells that are out of movement range
                var path = _newPath.ToList();

                //if chasing each other, meet in middle, cut list by half, rounding down if odd number (Clash)
                if(ChaseTarget.ChaseTarget == this) 
                {
                    int x = path.Count / 2;
                    int i = path.Count - x;
                    path.RemoveRange(i,x);
                }
                //get the elements appear in both lists
                var chasePath = path.Intersect(inMovementRangeCells).ToList(); 
                
                //If able track down target to destination, lose this encounter
                if(chasePath[chasePath.Count - 1] == endCell)
                {
                    chasePath.RemoveAt(chasePath.Count - 1);
                }

                pathFindCells = chasePath;

                Debug.Log("chase path found");
                rangeUsed.Add(pathFindCells[pathFindCells.Count - 1].gCost);
            }
            
        }
    }
    protected virtual void Update()
    {
        Debug.Log($"{gameObject.name}:"+ inRangeCells.Count);
       
        //temp
        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(ChaseTarget!=null && InputManager.SelectedPlayer == this)
            ChaseDownTarget();
        }

        //If there is path to follow and all pathfind algorithm has finished processing
        if(!isFollowing && followReq.Any() && PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        StartCoroutine(StartFollowPath());

        //get current cell unit is standing
        // if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1, cellLayer))
        // unitOnCell = hit.collider.GetComponent<Cell>();

        DrawLineRndr();
    }
    public void ClearAllInputs()
    {
        targetCells.Clear();
        pathFindV3.Clear();
        pathFindCells.Clear();
        rangeUsed.Clear();

        ChaseTarget = null;
        ToggleOverlay(false);
    }
    public void EnqueueFollowPath()
    {
        if(pathFindCells.Any())
        {
            Debug.Log("start follow");
            followReq.Enqueue(FollowPath(pathFindCells));
        }
    }
    public void ToggleOverlay(bool _enable)
    {
        if(_enable)
        {
            tilemap.ClearAllTiles();

            foreach(var c in inMovementRangeCells)
            {
                tilemap.SetTile(c.TileMapTilePos(), overlay);
            }
        }
        else
        {
            tilemap.ClearAllTiles();
        }
    }

    public void GetInRangeCells()
    {
        Debug.Log("Get InRangeCells");
        //draw square around unit's grid
        inRangeCells.Clear();
        inSquareRangeCells.Clear();

        //temporary solution, will break if player is able to back track to unitoncell
        UnitOnCell().gCost = 0;
        UnitOnCell().hCost = 0;

        int minX, maxX, minY, maxY;
        int dist = Mathf.RoundToInt(MovementRangeLeft/10);

        Cell start = targetCells.Count > 0? targetCells.Last() : UnitOnCell();

        minX = start.coord.x - dist;
        maxX = start.coord.x + dist;
        minY = start.coord.y - dist;
        maxY = start.coord.y + dist;

        for (int xval = minX; xval <= maxX; xval++)
        {
            for (int yval = minY; yval <= maxY; yval++)
            {
                IntVector2 i = new IntVector2(xval, yval);
                try
                {
                    inSquareRangeCells.Add(Grid.cellDic[i]);
                }
                catch
                {
                    continue;
                }
                
            }
        }
        foreach(var c in inSquareRangeCells)
        {
            if(c.IsWithinRangeWithUnit(this))
            {
                inRangeCells.Add(c);
            }
        }
    }
    public void CheckInMovementRange(bool _EnableOverlay)
    {
        int cost;
        inMovementRangeCells.Clear();
        tilemap.ClearAllTiles();

        GetInRangeCells();

        foreach(Cell c in inRangeCells)
        {
            cost = 1000 + movementMaxRange; //if pathfind failed, at least the cost would be too high to move to
            List<Cell> cs = new List<Cell>(){c};

            Cell _start = targetCells.Count > 0? targetCells.Last() : UnitOnCell(); //pathfind from target cell to current in range cell
            PathRequestManager.thePathReqManager.RequestPathFindings(_start, cs, AddToHashset); //makes sure the cell is reachable if there is obstacles
        }

        void AddToHashset(Cell[] _path, bool _success)
        {
            if(_success)
            {
                Cell lastCell = _path.Last();
                cost = lastCell.gCost;

                if(cost <= MovementRangeLeft)
                {
                   inMovementRangeCells.Add(lastCell);

                   //overlay
                   if(_EnableOverlay)
                   tilemap.SetTile(lastCell.TileMapTilePos(), overlay);
                }

                foreach(var c in inRangeCells)
                {
                    c.gCost = 0;
                    c.hCost = 0;
                }
            }
        }
    }
    public void DrawLineRndr()
    {
        pathFindV3.RemoveAll(v3 => v3 == Vector3.zero); //for some reason is adding one extra slot per pathfiding
        var vector3sa = pathFindV3.ToArray();
       
        lineRdr.positionCount = vector3sa.Length;
        lineRdr.SetPositions(vector3sa);
    }
   
    private IEnumerator StartFollowPath()
    {
        isFollowing = true;
        
        while(followReq.Any())
        {
            targetIndex = 0;
            yield return followReq.Dequeue();
        }
        isFollowing = false;
    }
   
    protected virtual IEnumerator FollowPath(List<Cell> _pathToFollow)
    {
        Vector3 offset = Vector3.up * GridManager.theGridManager.cellSize/2;
        //if there is no path to follow, break
        if(!_pathToFollow.Any())
        {
            yield break;
        } 
        Cell currentWayPoint = _pathToFollow[0];
        
        while(true)
        {
            //if the unit's position is the same as the currentWaypoint's World Pos + offset
            if(transform.position == currentWayPoint.ToWorldPos() + offset)
            {
                targetIndex++;

                //when unit reaches the end
                if(targetIndex >= _pathToFollow.Count)
                {
                    yield break;
                }

                currentWayPoint = _pathToFollow[targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWayPoint.ToWorldPos() + offset, speed * Time.deltaTime);
            yield return null;
        }
    }
}
