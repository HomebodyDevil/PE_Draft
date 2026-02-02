using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DialogueService : PersistantSingleton<DialogueService>
{
    public static Action<bool> OnSetDialogueVisible;
    public static Action<string> OnSetSpeakerName;
    public static Action<DialogueLine> OnSetCurrentDialogueLine;
    public static Action OnDialogueClick;
    public static Action OnDialogueEnd;
    public static Action OnPlayDialogue;
    public static Action<bool> OnEnableClickPreventer;
    //public static Action<List<DialogueLine>> OnSetChoices;
    public static Action OnMadeChoice;
    
    public List<DialogueLine> CurrentDialogueLines { get; private set; }
    private DialogueLine _currentDialogueLine;
    private int _currentDialogueLineId = -10;

    private void Start()
    {
        //Debug.Log("임시로 한 것. 나중에 바꾸기(DialogueService Start)");
        //EnableDialogue(true, "TestDialogue.csv");
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
        // id가 -10이라면, 아직 한 번도 사용?하지 않았다는 의미.
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

    public void EnableDialogue(bool enable=true, string path="", int dialogueLineId=0)
    {
        if (enable == false)
        {
            OnSetDialogueVisible?.Invoke(false);
            return;
        }
        
        if (String.IsNullOrEmpty(path))
        {
            Debug.Log("Invalid Dialogue Path");
            OnSetDialogueVisible?.Invoke(false);
            return;
        }
        
        DialogueService.OnEnableClickPreventer?.Invoke(true);
        OnSetDialogueVisible?.Invoke(enable);

        path = $"Assets/Medias/Dialogues/{path}.csv";
        
        Addressables.LoadAssetAsync<TextAsset>(path).Completed += handle =>
        {
            string currentDialogueLineText = handle.Result.text;
            
            CSVReader cr = new();
            Debug.Log(currentDialogueLineText);
            CurrentDialogueLines = cr.MakeDialogueLinesFromCSV(currentDialogueLineText);
            
            SetCurrentDialogueLineId(dialogueLineId);
            DialogueService.OnEnableClickPreventer?.Invoke(false);

            Addressables.Release(handle);
        };
    }

    public void SetCurrentDialogueLineId(int lineId)
    {
        // lineId가 -1이란 것은, 현재 Line이 마지막이란 뜻.
        if (lineId == -1)
        {
            OnSetDialogueVisible?.Invoke(false);
            OnDialogueEnd?.Invoke();
            return;
        }
        
        _currentDialogueLineId = lineId;
        SetCurrentDialogueLine(_currentDialogueLineId);
    }

    private void SetCurrentDialogueLine(int lineId)
    {
        _currentDialogueLine = FindDialogueLineByID(lineId);
        OnSetCurrentDialogueLine?.Invoke(_currentDialogueLine);
        OnPlayDialogue?.Invoke();
    }

    private DialogueLine FindDialogueLineByID(int lineId)
    {
        if (CurrentDialogueLines == null)
        {
            Debug.Log("CurrentDialogueLine is null");
            return null;
        }
        
        foreach (var dialogueLine in CurrentDialogueLines)
        {
            if (dialogueLine.Id == lineId)
            {
                return dialogueLine;
            }
        }

        Debug.Log($"Theres no DialogueLine with ID {lineId}");
        return null;
    }
}
