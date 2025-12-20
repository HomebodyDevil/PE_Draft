using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DialogueService : PersistantSingleton<DialogueService>
{
    public Action<bool> OnSetDialogueVisible;
    public Action<string> OnSetSpeakerName;
    public Action<DialogueLine> OnSetCurrentDialogueLine;
    public Action OnDialogueClick;

    private List<DialogueLine> _currentDialogueLines;
    private DialogueLine _currentDialogueLine;
    private int _currentDialogueLineId = -10;
    
    private void Start()
    {
        Debug.Log("임시로 한 것. 나중에 바꾸기(DialogueService Start)");
        EnableDialogue(true, "Assets/Medias/P&E_Dialogue.csv");
    }

    private void OnEnable()
    {
        OnDialogueClick += DialogueClick;
    }

    private void OnDisable()
    {
        OnDialogueClick -= DialogueClick;
    }

    private void DialogueClick()
    {
        Debug.Log("임시로 한 것. 나중에 바꾸기(DialogueService DialogueClick)");
        if (_currentDialogueLineId == -10)
        {
            SetCurrentDialogueLineId(0);
        }
        else
        {
            int nextDialogueId = _currentDialogueLine.NextDialogueId;
            SetCurrentDialogueLineId(nextDialogueId);
        }
    }

    // public void GetDialogueLines(string path)
    // {
    //     Addressables.LoadAssetAsync<TextAsset>(path).Completed += handle =>
    //     {
    //         string currentDialogueLineText = handle.Result.text;
    //         //GetDialogueLineFromCSV(currentDialogueLineText);
    //         
    //         CSVReader cr = new();
    //         Debug.Log(currentDialogueLineText);
    //         _currentDialogueLines = cr.MakeDialogueLinesFromCSV(currentDialogueLineText);
    //         //Debug.Log(_currentDialogueLines.Count);
    //     };
    // }

    public void EnableDialogue(bool enable=true, string path="", int dialogueLineId=0)
    {
        if (String.IsNullOrEmpty(path))
        {
            Debug.Log("Invalid Dialogue Path");
            OnSetDialogueVisible?.Invoke(false);
            return;
        }
        
        OnSetDialogueVisible?.Invoke(enable);
        
        Addressables.LoadAssetAsync<TextAsset>(path).Completed += handle =>
        {
            string currentDialogueLineText = handle.Result.text;
            
            CSVReader cr = new();
            Debug.Log(currentDialogueLineText);
            _currentDialogueLines = cr.MakeDialogueLinesFromCSV(currentDialogueLineText);
            
            SetCurrentDialogueLineId(dialogueLineId);
        };
    }

    public void SetCurrentDialogueLineId(int lineId)
    {
        if (lineId == -1)
        {
            OnSetDialogueVisible?.Invoke(false);
            return;
        }
        
        _currentDialogueLineId = lineId;
        _currentDialogueLine = FindDialogueLineByID(lineId);
        OnSetCurrentDialogueLine?.Invoke(_currentDialogueLine);
    }

    private DialogueLine FindDialogueLineByID(int lineId)
    {
        if (_currentDialogueLines == null)
        {
            Debug.Log("CurrentDialogueLine is null");
            return null;
        }
        
        foreach (var dialogueLine in _currentDialogueLines)
        {
            if (dialogueLine.Id == lineId)
            {
                return dialogueLine;
            }
        }

        Debug.Log($"Theres no DialogueLine with ID {lineId}");
        return null;
    }

    // private void GetDialogueLineFromCSV(string csvStr)
    // {
    //     CSVReader cr = new();
    //     _currentDialogueLines = cr.MakeDialogueLinesFromCSV(csvStr);
    // }
}
