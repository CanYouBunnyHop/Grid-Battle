using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUIManager : MonoBehaviour
{
    public AbilityButtonUI[] buttonPool;
    public static AbilityUIManager theAbilityUIManager;

    private void Awake()
    {
        theAbilityUIManager = this;
    }
    private void OnEnable()
    {
        UpdatesAbilityUI();
    }
    public void UpdatesAbilityUI()
    {
        var selectedPlayer = InputManager.theInputManager.SelectedPlayer;
        DisableAllButtons();
        for(int i = 0; i < selectedPlayer.abilities.Count; i++)
        {
            buttonPool[i].gameObject.SetActive(true);
            var ability = selectedPlayer.abilities[i];
            buttonPool[i].abilityCurrentAssigned = selectedPlayer.abilities[i];
            buttonPool[i].icon.sprite = ability.abilityIcon;
            buttonPool[i].button.onClick.RemoveAllListeners();
            buttonPool[i].button.onClick.AddListener(() => ability.SelectAbility(selectedPlayer));
            //buttonPool[i].button.onClick.AddListener(() => buttonPool[i].button.interactable = false);
            Debug.Log("ass");
        }
    }
    public void DisableAllButtons()
    {
        foreach(var b in buttonPool)
        {
            b.gameObject.SetActive(false);
        }
    }
    void Update()
    {
        
    }
}
