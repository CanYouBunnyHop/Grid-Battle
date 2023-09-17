using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class Player : Unit
{
    [Header("Player Exclusive Properties")]
    public LineRenderer lineRdr;
    public Sprite portrait;

    //abilities
    public bool dashSelected = false; // when dash ability is selected
    
    protected override void Update()
    {
        base.Update();
        DrawLineRndr();
    }
    public override void OnMouseDown()
    {
        if(PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        {
            if(InputManager.theInputManager.currentChoiceMode is InputManager.ChoiceMode.chase)
            {
                IsSelectedAsChaseTarget();
            } 
            else
            {
                InputManager.theInputManager.SelectedPlayer = this;
                AbilityUIManager.theAbilityUIManager.UpdatesAbilityUI();
                CheckInMovementRange(true);
            }
        }
    }
    // protected override IEnumerator FollowPath(List<Cell> _path)
    // {
    //     yield return base.FollowPath(_path);
    // }

    //LineRenderer And Toggle TileMaps function here
    public void DrawLineRndr()
    {
        if(pathFindV3.Any())
        {
             //update line render here, draw line is in unit
            lineRdr.enabled = true;
            pathFindV3.RemoveAll(v3 => v3 == Vector3.zero); //for some reason is adding one extra slot per pathfiding
            var vector3sa = pathFindV3.ToArray();
        
            lineRdr.positionCount = vector3sa.Length;
            lineRdr.SetPositions(vector3sa);

            Material m = dashTarget != null? StaticData.lineMats[1] : StaticData.lineMats[0];
            lineRdr.material = m;
        }
        else if(ChaseTarget != null)
        {
            lineRdr.enabled = true;
            lineRdr.positionCount = 2;
            lineRdr.SetPosition(0, UnitOnCell().ToWorldPos());
            lineRdr.SetPosition(1, ChaseTarget.UnitOnCell().ToWorldPos());
        }
        else
        {
            lineRdr.enabled = false;
        }
    }
    private void LineRendererProperties()
    {
        Material m = dashTarget != null? StaticData.lineMats[1] : StaticData.lineMats[0];
        float lineThickness; //0.2 for arrowed, 0.1 for default
        //lineRdr.widthMultiplier
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
    public override void ClearAllInputs()
    {
        base.ClearAllInputs();
        ToggleOverlay(false);
    }
}
