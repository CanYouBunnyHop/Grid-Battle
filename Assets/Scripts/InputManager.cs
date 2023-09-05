using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InputManager : MonoBehaviour
{
    public static InputManager theInputManager;
    public Player SelectedPlayer;
    private void Awake()
    {
        theInputManager = this;
    }
    public void RightClicking()
    {
        //right click
        if(Input.GetKeyDown(KeyCode.Mouse1) && PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        {
            if(SelectedPlayer == null) return;

            if(SelectedPlayer.targetCells.Count > 0 || SelectedPlayer.ChaseTarget != null)
            {
                SelectedPlayer.ClearAllInputs();
                SelectedPlayer.CheckInMovementRange(true);
            }
            else if (SelectedPlayer.targetCells.Count == 0 || SelectedPlayer.ChaseTarget == null)
            {
                SelectedPlayer.ToggleOverlay(false);
                SelectedPlayer = null;
            }
        }
    }
}
