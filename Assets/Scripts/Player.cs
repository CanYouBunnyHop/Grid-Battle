using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class Player : Unit, IPointerClickHandler
{
    [Header("Player Exclusive Properties")]
    public LineRenderer lineRdr;
    public Sprite portrait;
    
    protected override void Update()
    {
        base.Update();
        DrawLineRndr();
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
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

                if(InputManager.theInputManager.SelectedPlayer.selectedAbility is Ability_Dash abd)
                {
                    CheckDashableCells(abd);
                }
                else
                CheckInMovementRange(true);
            }
        }
    }
    // public override void OnMouseDown()
    // {
    //     if(PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
    //     {
    //         if(InputManager.theInputManager.currentChoiceMode is InputManager.ChoiceMode.chase)
    //         {
    //             IsSelectedAsChaseTarget();
    //         } 
    //         else
    //         {
    //             InputManager.theInputManager.SelectedPlayer = this;
    //             AbilityUIManager.theAbilityUIManager.UpdatesAbilityUI();
    //             CheckInMovementRange(true);
    //         }
    //     }
    // }
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

            LineRendererProperties();
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
        Material m;
        float lineThickness;
        //default
        //Chase
        //Dash
        if(selectedAbility is Ability_Dash abd && abd.dashTarget != null)
        {
           m = StaticData.lineMats[1];
           lineThickness = 0.4f;
        }
        else
        {
            m = StaticData.lineMats[0];
            lineThickness = 0.2f;
        }
        lineRdr.widthMultiplier = lineThickness;
        lineRdr.material = m;
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
