using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFind : MonoBehaviour
{
    [SerializeField] List<Cell> debugWayPoints;
    PathRequestManager requestManager => PathRequestManager.thePathReqManager;
    private GridManager grid => GridManager.theGridManager;
    
    public IEnumerator StartFindPath(Cell _start, List<Cell> _targets)
    {
        for(int i = 0; i < _targets.Count;)
        {
            if(i == 0)
            yield return StartCoroutine(FindPath(_start, _targets[i]));

            else if (i > 0)
            yield return StartCoroutine(FindPath(_targets[i-1], _targets[i]));
            i++;
        }
    }
    public IEnumerator FindPath(Cell _start, Cell  _target)
    {
        Cell[] wayPoints = new Cell[0];
        bool pathSuccess = false;

        Heap<Cell> openSet = new Heap<Cell>(grid.gridSize);
        HashSet<Cell> closeSet = new HashSet<Cell>(); //hashSet is list with no duplicates
        openSet.Add(_start);
    
        while(openSet.Count > 0)
        {
            Cell curCell = openSet.RemoveFirst();
            closeSet.Add(curCell);

            if(curCell == _target)
            {
                pathSuccess = true;
                //Debug.Log("while loop break, path found");
                break;
            }

            foreach(Cell neighbour in GridManager.theGridManager.GetNeighbourCells3x3(curCell))
            {
                if(neighbour.tileType is Cell.TileType.Hole or Cell.TileType.Wall || closeSet.Contains(neighbour))
                continue;

                int newMoveCstToNeighbour = curCell.gCost + GetDistance(curCell, neighbour);
                if(newMoveCstToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))  //if new path to neighbour is shorter OR neighbour is not in OPEN
                {
                    //set neighbour's fcost
                    neighbour.gCost = newMoveCstToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, _target); //get distance from cur target cell
                    neighbour.prevCell = curCell;

                    if(!openSet.Contains(neighbour)) openSet.Add(neighbour);
                    //else openSet.UpdateItem(neighbour);
                }
            }
        }
        yield return null;
        if(pathSuccess)
        {
            wayPoints = RetracePath(_start,  _target); //retrace path from last target
        }
        requestManager.FinishedProcessingPath(wayPoints, pathSuccess);
    }
    Cell[] RetracePath(Cell _start, Cell _end)
    {
        List<Cell> path = new();
        var curCell = _end;

        while(curCell != _start)
        {
            path.Add(curCell);
            curCell = curCell.prevCell;
        }
        if(curCell == _start)
        {
            path.Add(curCell);
        }
        path.Reverse();
        return path.ToArray();
    }
    //This Doesn't work as intended
    // List<Cell> SimplifyPath(List<Cell> _path)
    // {
    //     List<Cell> simplePathing = new List<Cell>();
    //     Vector2 dirOld = Vector2.zero;

    //     for(int i = 1; i < _path.Count; i++)
    //     {
    //         Vector2 dirNew = new Vector2(_path[i-1].coord.x - _path[i].coord.x, _path[i-1].coord.y - _path[i].coord.y);
    //         if(dirNew != dirOld)
    //         {
    //             simplePathing.Add(_path[i]);
    //         }
    //         dirOld = dirNew;
    //     }
    //     List<Cell> p = new List<Cell>(simplePathing); 
    //     return simplePathing;
    // }
    public int GetDistance(Cell cellA, Cell cellB)
    {
        int dstX = Mathf.Abs(cellA.coord.x - cellB.coord.x);
        int dstY = Mathf.Abs(cellA.coord.y - cellB.coord.y);

        return dstX > dstY ? 14 * dstY + 10 * (dstX - dstY) : 14 * dstX + 10 * (dstY - dstX);
    }
}
