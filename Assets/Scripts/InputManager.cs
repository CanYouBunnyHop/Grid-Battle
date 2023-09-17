using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class InputManager : MonoBehaviour
{
    public static InputManager theInputManager;
    public Player SelectedPlayer;
    public AbilityUIManager abilityUIManager;
    public enum ChoiceMode
    {
        move,
        chase,
        dash,
    }
    public ChoiceMode currentChoiceMode;

    [Header("UI Settings")]
    [SerializeField] private Image portrait;
    [SerializeField] private Image bar;
    [SerializeField] private TextMeshProUGUI barText;
    private void Awake()
    {
        theInputManager = this;
    }
    private void Update()
    {
        portrait.gameObject.SetActive(SelectedPlayer != null);
        abilityUIManager.gameObject.SetActive(SelectedPlayer != null);

        if(SelectedPlayer != null)
        {
            currentChoiceMode = ChoiceModeState();

            portrait.sprite = SelectedPlayer.portrait;
            var x =  SelectedPlayer.currentHealth / SelectedPlayer.maxHealth;
            bar.fillAmount = x;
            //bar.rectTransform.localScale = new Vector3(x, 1, 1);
            barText.text = $"{SelectedPlayer.currentHealth}/{SelectedPlayer.maxHealth}";
        }
        
    }
    ChoiceMode ChoiceModeState()
    {
        if(SelectedPlayer.dashSelected)
        {
           return ChoiceMode.dash;
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            return ChoiceMode.chase;
        }
        else return ChoiceMode.move;
    }
    public void RightClicking()
    {
        //right click
        if(Input.GetKeyDown(KeyCode.Mouse1) && PathRequestManager.thePathReqManager.AlreadyFinishedProcessing())
        {
            if(SelectedPlayer == null) return;

            //if target cells exist. overlay defaultMRC
            if(SelectedPlayer.targetCells.Any() || SelectedPlayer.ChaseTarget != null)
            {
                SelectedPlayer.ClearAllInputs();
                SelectedPlayer.ToggleOverlay(true);
            }
            //if target cells are cleared, deselect player
            else if (SelectedPlayer.targetCells.Count == 0 || SelectedPlayer.ChaseTarget == null)
            {
                SelectedPlayer.ToggleOverlay(false);
                SelectedPlayer = null;
            }
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            foreach(var c in SelectedPlayer.inMovementRangeCells)
            {
                Debug.Log(c);
            }
        }
    }
}
