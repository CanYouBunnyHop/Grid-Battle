using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    protected override void OnMouseDown()
    {
        if(PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        {
            InputManager.theInputManager.SelectedPlayer = this;
            CheckInMovementRange();  
        }
    }
    protected override IEnumerator FollowPath(List<Cell> _path)
    {
        yield return base.FollowPath(_path);
        //lineRdr.enabled = false;
    }
}
