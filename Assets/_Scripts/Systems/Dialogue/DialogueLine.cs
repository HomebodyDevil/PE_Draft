using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public int Id { get; private set; }
    public string Speaker { get; private set; }
    public string DialogueText { get; private set; }
    public int NextDialogueId { get; private set; }
    public List<string> Condition { get; private set; }
    public List<string> Choices { get; private set; }
    public List<string> Actions { get; private set; }

    public DialogueLine(
        string id,
        string speaker,
        string dialogueText,
        string nextDialogueId,
        string condition,
        string choices,
        string actions
    )
    {
        Id = int.Parse(id);
        Speaker = speaker;
        DialogueText = dialogueText;
        NextDialogueId = int.Parse(nextDialogueId);
        // Condition = condition;
        // Choices = choices;
        // Actions = actions;
    }

    public DialogueLine(List<string> vars)
    {
        Id = int.Parse(vars[0]);
        Speaker = vars[1];
        DialogueText = vars[2];
        NextDialogueId = int.Parse(vars[3]);
        // Condition = condition;
        // Choices = choices;
        // Actions = actions;
    }
}