using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ability : MonoBehaviour
{
    public Sprite abilityIcon;
    //public int abilityCoolDown;
    //public int currentCoolDown;
    public virtual void OnSelectAbility(Unit _selectedUnit)
    {
        //if(currentCoolDown > 0)return;
        //_selectedUnit.selectedAbility = this;
        _selectedUnit.selectedAbility.ExitAbility(_selectedUnit, this);
        //_selectedUnit.selectedAbility = this;
    }
    public virtual void TargetBehavior()
    {

    }
    public virtual void ExitAbility(Unit _selectedUnit, Ability _abToSwitch = null)
    {
        _selectedUnit.selectedAbility = _abToSwitch;
    }
    public virtual void ClearAbilityInputs()
    {

    }
}
public class Ability_Prep : Ability
{
    
}
public class Ability_Dash : Ability
{
    public bool isAirbourne;
    public Cell dashTarget;
    public int dashMaxRange;

    public override void OnSelectAbility(Unit _selectedUnit)
    {
        _selectedUnit.selectedAbility = this;
        _selectedUnit.ClearAllInputs();
        _selectedUnit.CheckDashableCells(this);
    }

    public override void ExitAbility(Unit _selectedUnit, Ability _abToSwitch = null)
    {
        dashTarget = null;
        _selectedUnit.ClearAllInputs();
        _selectedUnit.CheckInMovementRange(true);
        base.ExitAbility(_selectedUnit, _abToSwitch);
    }
    public override void ClearAbilityInputs()
    {
        dashTarget = null;
    }
}
public class Ability_Blast : Ability
{
    public bool dmgDealt;
}

