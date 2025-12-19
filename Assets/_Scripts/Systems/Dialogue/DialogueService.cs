using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct DialogueLine
{
    public int Id;
    public string Speaker;
    public string DialogueText;
    public int NextDialogueId;
    public string Condition;
    public string Choices;
    public string Actions;
}

public class DialogueService : PersistantSingleton<DialogueService>
{
    public Action<string> OnSetSpeakerName;
    public Action<string> OnSetCurrentDialogue;

    private List<DialogueLine> _currentDialogueLines;
    
    private void Start()
    {
        ReadCSV("Assets/Medias/P&E_Dialogue.csv");
    }

    public void ReadCSV(string path)
    {
        Addressables.LoadAssetAsync<TextAsset>(path).Completed += handle =>
        {
            string currentDialogueLineText = handle.Result.text;
            GetDialogueLineFromCSV(currentDialogueLineText);
            Debug.Log(currentDialogueLineText);
        };
    }

    private void GetDialogueLineFromCSV(string csvStr)
    {
        CSVReader cr = new();
        _currentDialogueLines = cr.MakeDialogueLineFromCSV(csvStr);
    }
}
