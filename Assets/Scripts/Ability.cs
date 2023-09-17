using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ability : MonoBehaviour
{
    public Sprite abilityIcon;
    //public int abilityCoolDown;
    //public int currentCoolDown;
    public virtual void SelectAbility(Unit _selectedUnit)
    {
        //if(currentCoolDown > 0)return;
        _selectedUnit.selectedAbility = this;
    }
    public virtual void TargetBehavior()
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
}
public class Ability_Blast : Ability
{
    public bool dmgDealt;
}

