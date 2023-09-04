using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class GridManager : MonoBehaviour
{
    public GameObject CellPrefab;
    public float cellSize;
    public IntVector2 gridDimention;
    public int gridSize => gridDimention.x * gridDimention.y;
    public Dictionary<IntVector2, Cell> cellDic;
    
    [Header("Gizmo")]
    public bool drawGridGizmo;
    public bool drawCellGizmo;
    public static GridManager theGridManager;
    private void Awake()
    {
        theGridManager = this;
        //pathFind = GetComponent<PathFind>();
        if(cellDic is null)
        {
            cellDic = new Dictionary<IntVector2, Cell>();
            foreach(Transform child in transform)
            {
                child.TryGetComponent<Cell>(out Cell cellComp);
                cellDic.Add(cellComp.coord, cellComp);
            }
        }
    }
    public void DrawCells()
    {
        for(int X = 0; X < gridDimention.x; X++)
        {
            for(int Y = 0; Y < gridDimention.y; Y++)
            {
                IntVector2 cellCoord = new IntVector2(X,Y);
                DrawCell(cellCoord);
            }
        }
    }
    void DrawCell(IntVector2 _coord)
    {
        Vector3 targetPos = new Vector3(cellSize/2 + (_coord.x * cellSize), 0, cellSize/2 + (_coord.y * cellSize));
        GameObject cell = Instantiate(CellPrefab, targetPos, Quaternion.identity);
        cell.transform.parent = this.transform;

        //get cell component
        Cell comCell = cell.GetComponent<Cell>();
        var gridCoord = new IntVector2(_coord.x, _coord.y);
        comCell.coord = gridCoord;
        Debug.Log(gridCoord.x +"//"+gridCoord.y);
    
        //Update Cell name to its coord
        var coordSt = (x : _coord.x.ToString(), y : _coord.y.ToString());
        cell.name = $"X{coordSt.x}, Y{coordSt.y}";

        //Add Cell into dictionary
        if(cellDic == null) cellDic = new Dictionary<IntVector2, Cell>();
        cellDic.Add(comCell.coord, comCell);
    }
    public void DeleteCells()
    {
        // foreach(Transform children in transform) //doesn't work as it only destroy half of all at a time
        // DestroyImmediate(children.gameObject);
        for (int i = this.transform.childCount; i > 0; --i)     //iterating at(0) instead of "i", 
        DestroyImmediate(this.transform.GetChild(0).gameObject);//Document says "never iterate through arrays and destroy the elements you are iterating over"

        if(cellDic is not null)
        cellDic.Clear();
    }
    
    private void OnDrawGizmos()
    {
        if(drawGridGizmo)
        {
            //draw outlines
            Gizmos.color = Color.cyan;
            for(int X = 0; X < gridDimention.x +1; X++)
            {
                //draw verticle lines
                var xorigin = new Vector3(X * cellSize, cellSize/2, 0);
                var xend = new Vector3(X * cellSize, cellSize/2, gridDimention.y * cellSize);
                Gizmos.DrawLine(xorigin, xend);
            }
            
            for(int Y = 0; Y < gridDimention.y +1; Y++)
            {
                //draw horizontal lines
                var yorigin = new Vector3(0, cellSize/2, Y * cellSize); 
                var yend = new Vector3(gridDimention.x * cellSize, cellSize/2, Y * cellSize);
                Gizmos.DrawLine(yorigin, yend);
            }
            //draw axis
            Handles.color = Color.red;
            Handles.ArrowHandleCap(0, transform.position, Quaternion.Euler(0, 90, 0),(gridDimention.x * cellSize) + 3, EventType.Repaint);
            
            Handles.color = Color.blue;
            Handles.ArrowHandleCap(0, transform.position, Quaternion.Euler(0, 0, 0),(gridDimention.x * cellSize) + 3, EventType.Repaint);
        }
    }
    public List<Cell> GetNeighbourCells3x3(Cell _currentCell)
    {
        //top left
        IntVector2 topLeft = new IntVector2(_currentCell.coord.x - 1, _currentCell.coord.y + 1);
        //top
        IntVector2 top = new IntVector2(_currentCell.coord.x, _currentCell.coord.y + 1);
        //top right
        IntVector2 topRight = new IntVector2(_currentCell.coord.x + 1, _currentCell.coord.y + 1);

        //mid left
        IntVector2 midLeft = new IntVector2(_currentCell.coord.x - 1, _currentCell.coord.y);
        //mid
        //IntVector2 mid = new IntVector2(_currentCell.coord.x, _currentCell.coord.y);
        //mid right
        IntVector2 midRight = new IntVector2(_currentCell.coord.x + 1, _currentCell.coord.y);

        //bot left
        IntVector2 botLeft = new IntVector2(_currentCell.coord.x - 1, _currentCell.coord.y - 1);
        //bot
        IntVector2 bot = new IntVector2(_currentCell.coord.x, _currentCell.coord.y - 1);
        //bot right
        IntVector2 botRight = new IntVector2(_currentCell.coord.x + 1, _currentCell.coord.y - 1);

        List<IntVector2> grid3x3 = new List<IntVector2>()
        {
            topLeft, top, topRight,
            midLeft,      midRight,
            botLeft, bot, botRight
        };
        List<Cell> returnList = new List<Cell>();
        for(int i = 0; i < grid3x3.Count; i++)
        {
            try
            {
                returnList.Add(cellDic[grid3x3[i]]);
               // Debug.Log(cellDic[grid3x3[i]].coord.x + "//" + cellDic[grid3x3[i]].coord.y);
            }
            catch(KeyNotFoundException)
            {
                continue;
            }
            
        }
        return returnList;
    }
    
}
#if UNITY_EDITOR
[CustomEditor(typeof(GridManager))]
public class GridManager_editor : Editor
{
    private void OnEnable()
    {
        
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GridManager inspectedGridMngrCmpnent = (GridManager)target; //the inspected component

        DrawDefaultInspector();

        //draw space for buttons to be placed on
        Rect r = EditorGUILayout.BeginHorizontal(GUI.skin.label);       //label is the default background color it seems
        EditorGUILayout.Space(25); //button space, the hieght of button
        EditorGUILayout.EndHorizontal();

        //draw button
        Rect drawButton = new Rect( r.width/2 , r.y , r.width/2, r.height);
        Rect delButton =  new Rect( 0 , r.y , r.width/2, r.height);

        if(GUI.Button(drawButton, "Draw Cells"))
        {
            inspectedGridMngrCmpnent.DrawCells();
            Debug.Log("Cells are drawn");
        }
        if(GUI.Button(delButton, "Delete All Cells"))
        {
            inspectedGridMngrCmpnent.DeleteCells();
            Debug.Log("Cells have been deleted");
        }
    }
}
#endif
