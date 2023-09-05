using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : Unit
{
    //public bool checkedMoveRange = false;
    void Awake()
    {
        //lineRdr = GetComponent<LineRenderer>();
    }
    protected override void Update()
    {
        base.Update();
    }
    public override void OnMouseDown()
    {
        if(PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                IsSelectedAsChaseTarget();
            } 
            else
            {
                InputManager.theInputManager.SelectedPlayer = this;
                CheckInMovementRange(true);
            }
        }
    }
    protected override IEnumerator FollowPath(List<Cell> _path)
    {
        yield return base.FollowPath(_path);
        //lineRdr.enabled = false;
    }
}
