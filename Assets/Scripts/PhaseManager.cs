using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PhaseManager : MonoBehaviour
{
    public List<PhaseState> phaseOrder = new();
    public PhaseState currentPhase => phaseOrder[curPhaseIndex];
    public int curPhaseIndex = 0;
    public List<Unit> units = new();

    public int turnsElapsed = 0;

    public static PhaseManager phaseManager;
    void Awake()
    {
        turnsElapsed = 0;

        phaseManager = this;
        curPhaseIndex = 0;
        phaseOrder = new()
        {
            new PhaseState_Choice(),
            new PhaseState_Dash(),
            new PhaseState_Chase(),
            new PhaseState_Move()
        };

        units = FindObjectsByType<Unit>(FindObjectsSortMode.None).ToList();

        currentPhase.EnterPhase(this);
    }
    void Start()
    {
       
    }
    void Update()
    {
        currentPhase.DuringPhase(this);
        //Debug.Log(currentPhase);
    }

    public void NextPhase()
    {
        curPhaseIndex++;
        curPhaseIndex %= phaseOrder.Count;
        currentPhase.EnterPhase(this);
    }
}
[System.Serializable]
public abstract class PhaseState
{
    public abstract void EnterPhase(PhaseManager _phaseManager);
    public abstract void DuringPhase(PhaseManager _phaseManager);
    public virtual void ExitPhase(PhaseManager _phaseManager)
    {
        _phaseManager.NextPhase();
    }
}
public class PhaseState_GetDefaultInMovementRangeCells : PhaseState
{
    public override void EnterPhase(PhaseManager _phaseManager)
    {
       
    }
    public override void DuringPhase(PhaseManager _phaseManager)
    {
        
    }
}
public class PhaseState_Choice : PhaseState
{
    public override void EnterPhase(PhaseManager _phaseManager)
    {
        InputManager.theInputManager.SelectedPlayer = null;
        foreach(Unit u in _phaseManager.units)
        {
            Debug.Log("Enter + foreach");
            u.ClearAllInputs();
            u.CheckInMovementRange(false);
            
        }
    }
    public override void DuringPhase(PhaseManager _phaseManager)
    {
        InputManager.theInputManager.RightClicking();
    }
}
public class PhaseState_Dash : PhaseState
{
    public override void EnterPhase(PhaseManager _phaseManager)
    {
        //calc clash/ occupy
        IGrouping<Cell, Unit> FindAnyConflict()
        {
            var conflictGroups = _phaseManager.units
                .GroupBy(u => u.DashTargetDestination())
                .Where(g => g.Count() > 1).ToList();

            if(conflictGroups.Any())
            {
                return conflictGroups[0];
            }
            else return null;
        }

        void RecursionSolveConflict()
        {
            Debug.Log("ReSolve");

            var g = FindAnyConflict();
            while(g != null) //if conflict is found
            {
                var units = g.Select(u => u).ToList(); //get units from the single conflict group
                g.Key.SolveMoveConflict(units);  // solve the conflict
                g = FindAnyConflict(); //find conflict again, repeats loop again until no more conflicts
            }
        }
        RecursionSolveConflict();

        //after finish calc
        foreach(Unit u in _phaseManager.units)
        {
            u.EnqueueDashPath();
        }
    }
    public override void DuringPhase(PhaseManager _phaseManager)
    {
        bool FollowComplete(Unit _unit) => !_unit.isFollowing && !_unit.followReq.Any();
        if(_phaseManager.units.TrueForAll(FollowComplete))
        {
            ExitPhase(_phaseManager);
        }
    }
}
public class PhaseState_Chase: PhaseState
{
    public override void EnterPhase(PhaseManager _phaseManager)
    {
        foreach(Unit u in _phaseManager.units)
        {
            if(u.ChaseTarget!=null)
            {
                u.ChaseDownTarget(); //path find chase
            }
        }
    }

    public override void DuringPhase(PhaseManager _phaseManager)
    {
        if(PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        ExitPhase(_phaseManager);
    }
}
public class PhaseState_Move : PhaseState
{   
    public override void EnterPhase(PhaseManager _phaseManager)
    {
        Debug.Log("run");
        
        //calc clash/ occupy
        IGrouping<Cell, Unit> FindAnyConflict()
        {
            var conflictGroups = _phaseManager.units
                .GroupBy(u => u.TargetDestination())
                .Where(g => g.Count() > 1).ToList();

            if(conflictGroups.Any())
            {
                return conflictGroups[0];
            }
            else return null;
        }

        void RecursionSolveConflict()
        {
            Debug.Log("ReSolve");

            var g = FindAnyConflict();
            while(g != null) //if conflict is found
            {
                var units = g.Select(u => u).ToList(); //get units from the single conflict group
                g.Key.SolveMoveConflict(units);  // solve the conflict
                g = FindAnyConflict(); //find conflict again, repeats loop again until no more conflicts
            }
        }
        RecursionSolveConflict();

        //after finish calc
        foreach(Unit u in _phaseManager.units)
        {
            u.EnqueueFollowPath();
        }
    }
    public override void DuringPhase(PhaseManager _phaseManager)
    {
        bool FollowComplete(Unit _unit) => !_unit.isFollowing && !_unit.followReq.Any();
        if(_phaseManager.units.TrueForAll(FollowComplete))
        {
            ExitPhase(_phaseManager);
            _phaseManager.turnsElapsed++;
        }
    }
}