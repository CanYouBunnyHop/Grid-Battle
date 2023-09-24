using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public abstract class Unit : MonoBehaviour, IPointerClickHandler
{
    [Header("Gameplay Settings")]
    public float maxHealth;
    public float currentHealth;
    
    [Header("Follow Settings")]
    [SerializeField]float speed = 5;
    [SerializeField]public bool isFollowing = false;

    public int movementMaxRange = 60;
    public List<int> rangeUsed = new();
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
    public HashSet<Cell> inMovementRangeCells => targetCells.Any() ? newMRC : defaultMRC;
    private HashSet<Cell> defaultMRC = new();
    private HashSet<Cell> newMRC = new();
    public LayerMask cellLayer;
    private InputManager InputManager => InputManager.theInputManager;
    private GridManager Grid => GridManager.theGridManager;
    public Queue<IEnumerator> followReq = new();
    public List<Cell> pathFindCells = new();

    public Cell TargetDestination() => pathFindCells.Any() ? pathFindCells.Last() : UnitOnCell();
    
    public Unit ChaseTarget;
    public List<Vector3> pathFindV3 = new();

    //ability
    public List<Ability> abilities = new();
    public Ability selectedAbility;
    
    [Header("Dash Properties")]
    //public bool allowMovementAfterDash = false; //oz / kaigin???
    //public bool allowChaseAfterDash = false; //most characters are viable for this
    //public bool isAirbourne;
    //public Cell dashTarget;
    [SerializeField] private float dashSpeed = 15;
    //public int dashMaxRange = 14;
    private HashSet<Cell> dashSquareRangeCells = new();
    private HashSet<Cell> dashRangeCells = new();
    public HashSet<Cell> dashableCells= new();
    public List<Cell> dashPathFindCells = new();
    public Cell DashTargetDestination() => dashPathFindCells.Any() ? dashPathFindCells.Last() : UnitOnCell();
    // public Cell MovementStartCell()
    // {
    //     if(dashTarget == null) return UnitOnCell();
    //     else return dashTarget; // instead of returning dash target we need to get the solved conflict ver
    // }
    

    [Header("TileMap Properties")]
    public Tilemap tilemap;
    public TileBase overlay;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if(PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        {
           if(InputManager.theInputManager.currentChoiceMode is InputManager.ChoiceMode.chase)
            {
                IsSelectedAsChaseTarget();
            } 
        }
    }
    // public virtual void OnMouseDown() //mouse down on unit
    // {
        
    // }
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
        PathRequestManager.thePathReqManager.RequestPathFindings(start, end, false, false, OnPathFound);

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
        //If there is path to follow and all pathfind algorithm has finished processing
        if(!isFollowing && followReq.Any() && PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        StartCoroutine(StartFollowPath());
    }
    public virtual void ClearAllInputs()
    {
        targetCells.Clear();
        pathFindV3.Clear();
        pathFindCells.Clear();
        rangeUsed.Clear();

        ChaseTarget = null;
        dashPathFindCells.Clear();

        if(selectedAbility)
        selectedAbility.ClearAbilityInputs();
    }
    // public virtual void ClearMovementInputs()
    // {
    //     targetCells.Clear();
    //     pathFindV3.Clear();
    //     pathFindCells.Clear();
    //     rangeUsed.Clear();

    // }
    public void EnqueueFollowPath()
    {
        if(pathFindCells.Any())
        {
            followReq.Enqueue(FollowPath(pathFindCells, speed));
        }
    }
    public void EnqueueDashPath()
    {
        if(dashPathFindCells.Any())
        {
            followReq.Enqueue(FollowPath(dashPathFindCells, dashSpeed));
        }
    }

    public void GetInRangeCells(Ability_Dash _dash = null)
    {
        var squareRange = _dash? dashSquareRangeCells : inSquareRangeCells;
        var inRange = _dash? dashRangeCells : inRangeCells;
        var range = _dash? _dash.dashMaxRange : MovementRangeLeft;
        
        //draw square around unit's grid
        inRange.Clear();
        squareRange.Clear();

        //temporary solution, will break if player is able to back track to unitoncell
        UnitOnCell().gCost = 0;
        UnitOnCell().hCost = 0;

        int minX, maxX, minY, maxY;
        int dist = Mathf.RoundToInt(range/10);

        Cell start = targetCells.Any()? targetCells.Last() : UnitOnCell();

        minX = start.coord.x - dist < 0 ? 0 : start.coord.x - dist;
        maxX = start.coord.x + dist;
        minY = start.coord.y - dist < 0 ? 0 : start.coord.y - dist;
        maxY = start.coord.y + dist;

        for (int xval = minX; xval <= maxX; xval++)
        {
            for (int yval = minY; yval <= maxY; yval++)
            {
                IntVector2 i = new(xval, yval);
                try
                {
                    squareRange.Add(Grid.cellDic[i]);
                }
                catch
                {
                    continue;
                }
            }
        }
        foreach(var c in squareRange)
        {
            if(c.IsWithinRangeWithUnit(this, _dash))
            {
                inRange.Add(c);
            }
        }
    }
    public void CheckInMovementRange(bool _EnableOverlay, Ability_Dash _dash = null)
    {
        var squareRange = _dash != null? dashSquareRangeCells : inSquareRangeCells;
        var inRange = _dash != null? dashRangeCells : inRangeCells;
        var range = _dash != null? _dash.dashMaxRange : MovementRangeLeft;
        var selectableCells = _dash != null? dashableCells : inMovementRangeCells;

        int cost;
        selectableCells.Clear();
        tilemap.ClearAllTiles();
        GetInRangeCells(_dash);
        
        foreach(Cell c in inRange) //try coroutine, dont iterate the loop until the callback has done (doesn't seem like timing issue)
        {
            cost = 10000; //if pathfind failed, at least the cost would be too high to move to
            List<Cell> cs = new List<Cell>(){c};
            Cell start;

            if(_dash == false) start = targetCells.Any()? targetCells.Last() : UnitOnCell(); //pathfind from target cell to current in range cell
            else start =  UnitOnCell(); 

            bool isAirb = _dash ? _dash.isAirbourne : false;
            PathRequestManager.thePathReqManager.RequestPathFindings(start, cs, isAirb, _dash, AddToHashset); //makes sure the cell is reachable if there is obstacles
        }
        foreach(var c in inRangeCells)
        {
            c.gCost = 0;
            c.hCost = 0;
        }
        
        void AddToHashset(Cell[] _path, bool _success)
        {
            if(_success)
            {
                Cell lastCell = _path.Last();
                cost = lastCell.gCost;

                if(cost <= MovementRangeLeft)
                {
                   selectableCells.Add(lastCell);

                   //overlay
                   if(_EnableOverlay)
                   tilemap.SetTile(lastCell.TileMapTilePos(), overlay);
                }
            }
            else Debug.Log("path fail");
        }
    }
    public virtual void CheckDashableCells(Ability_Dash _dash)
    {
        CheckInMovementRange(true, _dash);
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
   
    protected virtual IEnumerator FollowPath(List<Cell> _pathToFollow, float _followSpeed) 
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
            transform.position = Vector3.MoveTowards(transform.position, currentWayPoint.ToWorldPos() + offset, _followSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
