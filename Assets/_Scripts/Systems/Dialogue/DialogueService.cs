using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DialogueService : PersistantSingleton<DialogueService>
{
    public Action<string> OnSetSpeakerName;
    public Action<string> OnSetCurrentDialogue;
    public Action OnDialogueClick;

    private List<DialogueLine> _currentDialogueLines;
    
    private void Start()
    {
        ReadCSV("Assets/Medias/P&E_Dialogue.csv");
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
        Debug.Log("DialogueClick");
    }

    public void ReadCSV(string path)
    {
        Addressables.LoadAssetAsync<TextAsset>(path).Completed += handle =>
        {
            string currentDialogueLineText = handle.Result.text;
            //GetDialogueLineFromCSV(currentDialogueLineText);
            
            CSVReader cr = new();
            Debug.Log(currentDialogueLineText);
            _currentDialogueLines = cr.MakeDialogueLinesFromCSV(currentDialogueLineText);
            //Debug.Log(_currentDialogueLines.Count);
        };
    }

    // private void GetDialogueLineFromCSV(string csvStr)
    // {
    //     CSVReader cr = new();
    //     _currentDialogueLines = cr.MakeDialogueLinesFromCSV(csvStr);
    // }
}
