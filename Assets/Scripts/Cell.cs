using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteAlways]
[System.Serializable]
public class Cell : MonoBehaviour, IHeapItem<Cell>
{
    public IntVector2 coord;
    public TileType tileType;
    [SerializeField] private GameObject wallObj;
    int heapIndex;
    public int HeapIndex {get{return heapIndex;} set {heapIndex = value;}}
    public enum TileType
    {
        Ground,
        Hole,
        Wall,
    }
    public int gCost, hCost; //gcost is the cost from start to given cell, hcost is cost from end to given cell
    public int fCost => gCost + hCost;
    public Cell prevCell;
    private GridManager Grid => GridManager.theGridManager;
    private InputManager Input => InputManager.theInputManager;
    private PathRequestManager PathReqManager => PathRequestManager.thePathReqManager;
    private MeshRenderer MeshRdr => GetComponent<MeshRenderer>();
    /// <summary>
    /// [0 = top left],
    /// [1 = top],
    /// [2 = top right],
    /// [3 = left],
    /// [4 = right],
    /// [5 = bot left],
    /// [6 = bot],
    /// [7 = bot right]
    /// </summary>

    public List<bool> coverPosCheck = new();
    // private MaterialPropertyBlock mpb;
    // public MaterialPropertyBlock Mpb
    // {
    //     get
    //     {
    //         if(mpb == null) mpb = new MaterialPropertyBlock();
    //         return mpb;
    //     }
    // }
    // public void SetCellColor(Color _color)
    // {
    //     //TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer);
    //     Mpb.SetColor("_BaseColor", _color);
    //     MeshRdr.SetPropertyBlock(Mpb);
    // }
    public bool HasUnitOnCell()
    {
        var listUnitsOnCell = from u in PhaseManager.phaseManager.units where u.UnitOnCell() == this select u.UnitOnCell();
        return listUnitsOnCell.Contains(this);
    }
    private void Start() //grid manager is null during execute always
    {
        bool CoverRayCastCheck(Vector3 _offserDir)
        {
            Vector3 offsetOrigin = _offserDir * Grid.cellSize/2 + transform.position;
            Ray ray = new Ray(offsetOrigin, Vector3.up);

            return Physics.Raycast(ray, 1, Grid.coverLayerMask);
        }
        if(Application.isPlaying)
        {
            coverPosCheck = new()
            {
                CoverRayCastCheck(new Vector3(-1, 0, 1)),
                CoverRayCastCheck(new Vector3(0, 0, 1)),
                CoverRayCastCheck(new Vector3(1, 0, 1)),
                CoverRayCastCheck(new Vector3(-1, 0, 0)),
                CoverRayCastCheck(new Vector3(1, 0, 0)),
                CoverRayCastCheck(new Vector3(-1, 0, -1)),
                CoverRayCastCheck(new Vector3(0, 0, -1)),
                CoverRayCastCheck(new Vector3(1, 0, -1)),
            };
        }
    }
    
    
    public int CompareTo(Cell _cellToCompare)
    {
        int compare = fCost.CompareTo(_cellToCompare.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(_cellToCompare.hCost);
        }
        return -compare;
    }
    public void SolveMoveConflict(List<Unit> _conflitingUnits)
    {
        //after getting conflicting units
        if(_conflitingUnits.Count >= 2)
        {
            //Can Probably be Optimized
            var allSum = _conflitingUnits.Select(u => u.RangeUsedSum);
            var lowestRangeUsed = _conflitingUnits.Where(u => u.RangeUsedSum <= allSum.Min()).ToList();

            Unit winner = lowestRangeUsed.Count() == 1 ? lowestRangeUsed[0] : null; //Chaser will always lose

            foreach(Unit u in _conflitingUnits)
            {
                if(u != winner)
                {
                    //remove last cell on pathFindCells
                    u.pathFindCells.RemoveAt(u.pathFindCells.Count - 1);
                }
            }
        }
    }
   
    private void OnMouseDown()
    {
        ChoiceModeStateMachine();
    }
    private void ChoiceModeStateMachine()
    {
        var SelectedPlayer = Input.SelectedPlayer;
        switch(Input.currentChoiceMode)
        {
            case InputManager.ChoiceMode.move:
                if(SelectedPlayer != null && IsWithinMovementRange(SelectedPlayer) && PathReqManager.AlreadyFinishedProcessing())
                {
                    if(SelectedPlayer.UnitOnCell() == this) return;
                    if(SelectedPlayer.ChaseTarget != null) return; //if selected player has a chase target, dont add this cell
                    if(SelectedPlayer.dashTarget != null)return;

                    var targetCells = SelectedPlayer.targetCells;

                    if(targetCells.Count > 0 && targetCells[targetCells.Count-1] == this) return; //return if this cell is the last target
                    targetCells.Add(this);

                    if(targetCells.Count == 1)
                    {
                        PathRequestManager.thePathReqManager.RequestPathFindings(SelectedPlayer.UnitOnCell(), targetCells, false, false, OnPathFound);
                    }
                    else if(SelectedPlayer.targetCells.Count > 1)
                    {
                        List<Cell> end = new List<Cell>(){targetCells[targetCells.Count - 1]};
                        Cell start = targetCells[targetCells.Count - 2];

                        PathRequestManager.thePathReqManager.RequestPathFindings(start, end, false, false, OnPathFound);
                    }
                }
                
            break;

            case InputManager.ChoiceMode.chase:
            break;

            case InputManager.ChoiceMode.dash:
            if(SelectedPlayer != null && IsWithinMovementRange(SelectedPlayer, true) && PathReqManager.AlreadyFinishedProcessing())
            {
                if(SelectedPlayer.ChaseTarget != null) SelectedPlayer.ChaseTarget = null;
                if(SelectedPlayer.dashTarget == this) return;

                SelectedPlayer.dashTarget = this;
                List<Cell> end = new(){SelectedPlayer.dashTarget};
                PathRequestManager.thePathReqManager.RequestPathFindings(SelectedPlayer.UnitOnCell(), end, SelectedPlayer.isAirbourne, true, OnDashPathFound);
            }
            break;
        }
        void OnPathFound(Cell[] _newPath, bool _pathSuccess)
        {
            if(_pathSuccess)
            {

                SelectedPlayer.pathFindV3.AddRange(CellsToWorldPos(_newPath)); //for line renderer
                SelectedPlayer.pathFindCells.AddRange(_newPath); //for follow requests
                SelectedPlayer.rangeUsed.Add(_newPath[_newPath.Length - 1].gCost); //add move range used

                SelectedPlayer.CheckInMovementRange(true);
            }
            else
            {
                Debug.Log("Path Not Found Bug Here");
                if(SelectedPlayer.targetCells.Any())
                SelectedPlayer.targetCells.Remove(SelectedPlayer.targetCells.Last());
            }
        }
        void OnDashPathFound(Cell[] _newPath, bool _pathSuccess)
        {
            if(_pathSuccess)
            {
                SelectedPlayer.pathFindV3 = CellsToWorldPos(_newPath).ToList(); //for line renderer
                SelectedPlayer.dashPathFindCells =_newPath.ToList(); //for follow requests
                //SelectedPlayer.rangeUsed.Add(_newPath[_newPath.Length - 1].gCost); //add move range used

                //SelectedPlayer.CheckInMovementRange(true);
            }
        }
    }
    private void OnMouseEnter()
    {
        this.gameObject.layer = 9;
    }
    private void OnMouseExit()
    {
        this.gameObject.layer = 10;
        
        if(Input.SelectedPlayer != null && Input.SelectedPlayer.targetCells.Count == 0) //if selected cells has not been decided
        Input.SelectedPlayer.lineRdr.enabled = false;
    }
    
    public bool IsWithinRangeWithUnit(Unit _inspectedUnit, bool _dash = false)
    {
        List<Cell> targetCells = _inspectedUnit.targetCells;
        
        Cell startCell;
        if(_dash == false)
        {
            startCell = targetCells.Count > 0? targetCells[targetCells.Count -1] : _inspectedUnit.UnitOnCell();
            return PathReqManager.pathFind.GetDistance(startCell, this) <= _inspectedUnit.MovementRangeLeft;
        }
        else
        {
            startCell = _inspectedUnit.UnitOnCell();
            return PathReqManager.pathFind.GetDistance(startCell, this) <= _inspectedUnit.dashMaxRange;
        }
    } 
    public bool IsWithinMovementRange(Unit _inspectedUnit, bool _dash = false)
    {
        if(!_dash) return _inspectedUnit.inMovementRangeCells.Contains(this);
        else return _inspectedUnit.dashableCells.Contains(this);
    }
    
   
    private void Update()
    {
        UpdateTileTypeVisual();
    }
    private void UpdateTileTypeVisual()
    {
        MeshRdr.enabled = tileType is not TileType.Hole;
        wallObj.SetActive(tileType is TileType.Wall);
    }
    public Vector3 ToWorldPos() => this.gameObject.transform.position + Vector3.up * Grid.cellSize/2;
    private Vector3[] CellsToWorldPos(Cell[] _cells)
    {
        Vector3[] v = new Vector3[_cells.Length];
        for(int i = 0; i < _cells.Length; i++)
        {
            v[i] = _cells[i].ToWorldPos();
        }
        return v;
    }
    public Vector3Int TileMapTilePos()
    {
        var tilePos = new Vector3(coord.x, coord.y, 0);
        var tilePosInt = Vector3Int.RoundToInt(tilePos);

        return tilePosInt;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Cell))]
public class Cell_Editor : Editor
{
    // public void OnSceneGUI()
    // {
    //     Cell inspectedCell = (Cell)target;

    //     Handles.color = Color.green;

    //     Handles.DrawWireCube(inspectedCell.transform.position, inspectedCell.transform.localScale);

    //     // run before drawing any user interactable handles to detect changes
    //     // EditorGUI.BeginChangeCheck();

    //     // draw the handle for the range and store the potential new value
    //     // float newRange = Handles.RadiusHandle(Quaternion.identity, linkedObject.transform.position, linkedObject.Range, false);

    //     // if the value was changed then update it and register with the undo system
    //     // if (EditorGUI.EndChangeCheck())
    //     // {
    //     //     Undo.RecordObject(target, "Update range");
    //     //     linkedObject.Range = newRange;
    //     // }
    // }
    // public override void OnInspectorGUI()
    // {
    // }
}
#endif