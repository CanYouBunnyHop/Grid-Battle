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

    public List<Unit> conflictingUnits = new List<Unit>();
    public bool solved = false;



    private MaterialPropertyBlock mpb;
    public MaterialPropertyBlock Mpb
    {
        get
        {
            if(mpb == null) mpb = new MaterialPropertyBlock();
            return mpb;
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
                    //var reversedQ = winner.pathFindCells1D.Reverse();
                    //remove last cell on pathFindCells
                    u.pathFindCells.RemoveAt(u.pathFindCells.Count - 1);
                    // if(u.pathFindCells1D.Last().solved) //need loop here
                    // {

                    // }
                }
            }
            //solved = true;
        }
    }
   
     private void OnMouseDown()
    {
        var SelectedPlayer = Input.SelectedPlayer;
        if(SelectedPlayer != null && IsWithinMovementRange(SelectedPlayer) && PathReqManager.AlreadyFinishedProcessing())
        {
            var targetCells = SelectedPlayer.targetCells;

            if(SelectedPlayer.unitOnCell == this) return;

            if(targetCells.Count > 0 && targetCells[targetCells.Count-1] == this) //return if this cell is the last target
            return;
            
            targetCells.Add(this);

            //update line render here, draw line is in unit
            SelectedPlayer.lineRdr.enabled = true;

            if(targetCells.Count == 1)
            {
                PathRequestManager.thePathReqManager.RequestPathFindings(SelectedPlayer.unitOnCell, targetCells, OnPathFound);
            }
            else if(SelectedPlayer.targetCells.Count > 1)
            {
                List<Cell> end = new List<Cell>(){targetCells[targetCells.Count - 1]};
                Cell start = targetCells[targetCells.Count - 2];

                PathRequestManager.thePathReqManager.RequestPathFindings(start, end, OnPathFound);
            }
           
            SelectedPlayer.CheckInMovementRange();
        }
         void OnPathFound(Cell[] _newPath, bool _pathSuccess)
        {
            if(_pathSuccess)
            {
                Input.SelectedPlayer.pathFindV3.AddRange(CellsToWorldPos(_newPath)); //for line renderer
                Input.SelectedPlayer.pathFindCells.AddRange(_newPath); //for follow requests
                Input.SelectedPlayer.rangeUsed.Add(_newPath[_newPath.Length - 1].gCost); //add move range used
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
    
    public bool IsWithinRangeWithUnit(Unit _inspectedUnit) //OPTIMISE
    {
        if(Input != null && Input.SelectedPlayer != null)
        {
            List<Cell> targetCells = Input.SelectedPlayer.targetCells;
            Cell _start = targetCells.Count > 0? targetCells[targetCells.Count -1] : Input.SelectedPlayer.unitOnCell;

            return PathReqManager.pathFind.GetDistance(_start, this) <= _inspectedUnit.MovementRangeLeft;
        }
        return false;
    } 
    public bool IsWithinMovementRange(Unit _inspectedUnit)
    {
        if(_inspectedUnit.inMovementRangeCells.Contains(this)) return true;
        else return false;
    }
    
    public void SetCellColor(Color _color)
    {
        //TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer);
        Mpb.SetColor("_BaseColor", _color);
        MeshRdr.SetPropertyBlock(Mpb);
    }
    private void Update()
    {
        UpdateTileTypeVisual();

        //temp
        // if(UnityEngine.Input.GetKeyDown(KeyCode.X))
        // {
        //     gCost = 0;
        //     hCost = 0;
        // }
    }
    private void UpdateTileTypeVisual()
    {
        MeshRdr.enabled = tileType is not TileType.Hole;
        wallObj.SetActive(tileType is TileType.Wall);
    }
    public Vector3 ToWorldPos() => this.gameObject.transform.position + Vector3.up * Grid.cellSize/2;
    public Vector3[] CellsToWorldPos(Cell[] _cells)
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