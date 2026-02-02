using Unity.VisualScripting;
using UnityEngine;

public class ChoiceButton : PEButton
{
    private int _nextDialogueId;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public void SetChoiceButton(DialogueLine dialogueLine)
    {
        _nextDialogueId = dialogueLine.NextDialogueId;
        
        SetButtonText(dialogueLine.DialogueText);
        SetOnButtonClick(SetOnclick);
    }

    private void SetOnclick()
    {
        DialogueService.Instance.SetCurrentDialogueLineId(_nextDialogueId);
        DialogueService.OnMadeChoice?.Invoke();
    }
}
