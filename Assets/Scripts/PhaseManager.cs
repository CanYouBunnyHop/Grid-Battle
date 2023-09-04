using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PhaseManager : MonoBehaviour
{
    public List<PhaseState> phaseOrder = new()
    {
        new PhaseState_Choice(),
        new PhaseState_Move()
    };
    public PhaseState currentPhase => phaseOrder[curPhaseIndex];
    public int curPhaseIndex = 0;
    //public static PhaseManager thePhaseManager;
    public List<Unit> units = new();
    void Awake()
    {
        units = FindObjectsByType<Unit>(FindObjectsSortMode.None).ToList();
        //thePhaseManager = this;
        currentPhase.EnterPhase(this);
    }
    void Start()
    {
       
    }
    void Update()
    {
        currentPhase.DuringPhase(this);
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
public class PhaseState_Choice : PhaseState
{
    public override void EnterPhase(PhaseManager _phaseManager)
    {
        InputManager.theInputManager.SelectedPlayer = null;
        foreach(Unit u in _phaseManager.units)
        {
            u.ClearAllInputs();
        }
    }
    public override void DuringPhase(PhaseManager _phaseManager)
    {
        InputManager.theInputManager.RightClicking();
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
        }
    }
}
public class PhaseState_Chase : PhaseState //move this back into move, calc chase b4 conflict,
{
    public override void EnterPhase(PhaseManager _phaseManager)
    {
        //calc chase
        var unitsWithChase = from u in _phaseManager.units where u.ChaseTarget is not null select u; 
        foreach(Unit u in unitsWithChase)
        {
            //start = u.unitOnCell
            //end = u.chaseTarget.TargetDestination
            //PathRequestManager.thePathReqManager.RequestPathFindings(start, end, OnPathFound);
            void OnPathFound(Cell[] _newPath, bool _pathSuccess)
            {
                //Shave off the cells that are out of movement range
                var path = _newPath.ToList();
                var chasePath = path.Intersect(u.inMovementRangeCells);
                u.pathFindCells = chasePath.ToList();
                u.rangeUsed.Add(u.pathFindCells[u.pathFindCells.Count - 1].gCost);
            }
        }
        
    }
    public override void DuringPhase(PhaseManager _phaseManager)
    {
       
    }
}