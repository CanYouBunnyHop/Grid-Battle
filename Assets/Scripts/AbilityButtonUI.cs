using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityButtonUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI text;
    public Button button;
    public Ability abilityCurrentAssigned;
    public void Update()
    {
        if( InputManager.theInputManager.SelectedPlayer != null)
       button.interactable = abilityCurrentAssigned != InputManager.theInputManager.SelectedPlayer.selectedAbility;
    }
}
