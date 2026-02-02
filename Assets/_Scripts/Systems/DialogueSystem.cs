using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class DialogueSystem : Singleton<DialogueSystem>
{
    [Header("Initialize Settings")] 
    [SerializeField] private bool _InitialVisible = false;
    [SerializeField] private AssetLabelReference _speakerPortraitLabel;
    [Space(10f)]
    
    private List<ChoiceButton> _choiceButtons = new(); 
    
    public DialogueActionExecutor DialogueActionExecutor { get; } = new();
    
    private Transform _dialogueCanvas;
    private Transform _dialoguePanel;
    private Transform _backgroundPanel;
    private Transform _choiceButtonsPanel;
    private Transform _speakerImages;
    private Transform _speakerImageLeftPos;
    private Transform _speakerImageRightPos;
    private Transform _speakerImageMiddlePos;
    private TextMeshProUGUI _speakerNameText;
    private TextMeshProUGUI _dialogueText;
    private Button _clickCatcher;
    private Transform _clickPreventer;

    
    private Coroutine _playDialogueCoroutine;
    private DialogueLine _currentDialogueLine;
    private string _currentDialogue;
    private Dictionary<int, List<SpeakerPortrait>> _speakerPortraits = new();
    
    protected override void Awake()
    {
        base.Awake();
        SetVars();
    }

    private void Start()
    {
        _backgroundPanel.gameObject.SetActive(false);
        _choiceButtonsPanel.gameObject.SetActive(false);
        _clickCatcher.onClick.AddListener(OnClickClockCatcher);
        
        SetDialogueVisible(_InitialVisible);
    }

    private void OnDestroy()
    {
        foreach (var portraitListDict in _speakerPortraits)
        {
            foreach (var portraits in portraitListDict.Value)
            {
                if (portraits == null) continue;
                Addressables.ReleaseInstance(portraits.gameObject);
            }   
        }
    }

    private void OnEnable()
    {
        DialogueService.OnSetCurrentDialogueLine += SetCurrentDialogueLine;
        DialogueService.OnSetSpeakerName += SetSpeakerName;
        DialogueService.OnSetDialogueVisible += SetDialogueVisible;
        DialogueService.OnEnableClickPreventer += SetClickPreventer;
        DialogueService.OnPlayDialogue += PlayDialogue;
        DialogueService.OnMadeChoice += OnMadeChoice;
    }

    private void OnDisable()
    {
        DialogueService.OnSetCurrentDialogueLine -= SetCurrentDialogueLine;
        DialogueService.OnSetSpeakerName -= SetSpeakerName;
        DialogueService.OnSetDialogueVisible -= SetDialogueVisible;
        DialogueService.OnEnableClickPreventer -= SetClickPreventer;
        DialogueService.OnPlayDialogue -= PlayDialogue;
        DialogueService.OnMadeChoice -= OnMadeChoice;
    }

    private void SetVars()
    {
        if (_dialogueCanvas == null) transform.AssignChildVar<Transform>("DialogueCanvas", ref _dialogueCanvas);
        if (_dialoguePanel == null) transform.AssignChildVar<Transform>("DialoguePanel", ref _dialoguePanel);
        if (_backgroundPanel == null) transform.AssignChildVar<Transform>("BackgroundPanel", ref _backgroundPanel);
        if (_dialogueText == null) transform.AssignChildVar<TextMeshProUGUI>("DialogueText", ref _dialogueText);
        if (_speakerImages == null) transform.AssignChildVar<Transform>("SpeakerImages", ref _speakerImages);
        if (_speakerImageLeftPos == null) transform.AssignChildVar<Transform>("SpeakerImageLeftPos", ref _speakerImageLeftPos);
        if (_speakerImageMiddlePos == null) transform.AssignChildVar<Transform>("SpeakerImageMiddlePos", ref _speakerImageMiddlePos);
        if (_speakerImageRightPos == null) transform.AssignChildVar<Transform>("SpeakerImageRightPos", ref _speakerImageRightPos);
        if (_speakerNameText == null) transform.AssignChildVar<TextMeshProUGUI>("SpeakerNameText", ref _speakerNameText);
        if (_clickCatcher == null) transform.AssignChildVar<Button>("ClickCatcher", ref _clickCatcher);
        if (_choiceButtonsPanel == null) transform.AssignChildVar<Transform>("ChoiceButtonsPanel", ref _choiceButtonsPanel);
        if (_clickPreventer == null) transform.AssignChildVar<Transform>("ClickPreventer", ref _clickPreventer);

        foreach (ChoiceButton button in _choiceButtonsPanel.GetComponentsInChildren<ChoiceButton>(includeInactive: true))
        {
            _choiceButtons.Add(button);
        }
    }

    private void SetClickPreventer(bool enable)
    {
        _clickPreventer.gameObject.SetActive(enable);
    }

    private void SetDialogueVisible(bool visible)
    {
        _dialogueCanvas.gameObject.SetActive(visible);
    }
    
    private void OnClickClockCatcher()
    {
        DialogueService.OnDialogueClick?.Invoke();
    }

    private void SetCurrentDialogueLine(DialogueLine dialogueLine)
    {
        if (dialogueLine == null) return;
        
        _currentDialogueLine =  dialogueLine;

        if (DialogueActionExecutor == null)
        {
            Debug.Log("DialogueActionExecutor is null");
        }
        else if (dialogueLine.Actions != null && dialogueLine.Actions.Count > 0)
        {
            foreach (var action in dialogueLine.Actions)
            {
                DialogueActionExecutor.ExecuteDialogueAction(
                    action.FunctionName,
                    action.Parameters);
            }
        }
        
        SetSpeakerName(_currentDialogueLine.Speaker);
        SetDialogueText(_currentDialogueLine.DialogueText);
    }

    private void SetSpeakerName(string speakerName)
    {
        _speakerNameText.text = speakerName;
    }

    private void SetDialogueText(string dialogueText)
    {
        _dialogueText.text = dialogueText;
    }

    private void DisableChoiceButtons()
    {
        foreach (var choiceButton in _choiceButtons)
            choiceButton.gameObject.SetActive(false);
    }
    
    private void OnMadeChoice()
    {
        _choiceButtonsPanel.gameObject.SetActive(false);
    }

    private void SetChoices(DialogueLine dialogueLine)
    {
        if (dialogueLine.Choices.Count == 0) return;

        DisableChoiceButtons();
        
        List<DialogueLine> choicesLine = new();
        for (int i = 0; i < dialogueLine.Choices.Count; i++)
        {
            int choiceLineId = dialogueLine.Choices[i];
            foreach (var line in DialogueService.Instance.CurrentDialogueLines)
            {
                if (line.Id == choiceLineId) choicesLine.Add(line);
            }
        }
        
        for (int i = 0; i < choicesLine.Count; i++)
        {
            _choiceButtons[i].gameObject.SetActive(true);
            _choiceButtons[i].SetChoiceButton(choicesLine[i]);
        }
    }

    private void PlayDialogue()
    {
        if (_playDialogueCoroutine != null)
        {
            StopCoroutine(_playDialogueCoroutine);
            _playDialogueCoroutine = null;
        }
        _playDialogueCoroutine = StartCoroutine(PlayDialogueCoroutine());
    }

    private IEnumerator PlayDialogueCoroutine()
    {
        if (_currentDialogueLine.Choices.Count > 0)
        {
            _choiceButtonsPanel.gameObject.SetActive(true);
            SetChoices(_currentDialogueLine);
        }
        
        _playDialogueCoroutine = null;
        
        yield break;
    }

    private Transform GetPos(int pos)
    {
        return pos switch
        {
            1 => _speakerImageMiddlePos,
            2 => _speakerImageRightPos,
            _ => _speakerImageLeftPos
        };
    }

    private string GetSpritePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Path is null or empty");
            return "Assets/Medias/TestMedias/TestImage.png";
        }

        return $"Assets/Medias/Dialogues/images/{path}.png";
    }

    /// <summary></summary>
    /// <param name="position">0~2까지의 값. 0=Left ~ 2=Right</param>
    /// <param name="characterName">SpeakerPortrait에 설정할 Name</param>
    /// <param name="initialPortraitSpritePath">Portrait에 사용할 이미지(Sprite)</param>
    public void AddPortrait(
        int position, 
        string characterName, 
        string initialPortraitSpritePath)
    {
        Transform pos = GetPos(position);
        initialPortraitSpritePath = GetSpritePath(initialPortraitSpritePath);

        Addressables.InstantiateAsync(_speakerPortraitLabel, pos, false).Completed += (h) =>
        {
            if (h.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Failed to create portrait prefab");
                return;
            }

            if (!_speakerPortraits.ContainsKey(position))
                _speakerPortraits[position] = new();
            
            _speakerPortraits[position].Add(h.Result.GetComponent<SpeakerPortrait>());

            Addressables.LoadAssetAsync<Sprite>(initialPortraitSpritePath).Completed += h_sp =>
            {
                if (h.Result.TryGetComponent(out SpeakerPortrait speakerPortrait))
                {
                    speakerPortrait.SetImage(h_sp.Result);
                    speakerPortrait.SetVisible(true);
                    
                    speakerPortrait.GameObjectOperationHandle = h;
                    speakerPortrait.SpriteOperationHandle = h_sp;
                }
            };
        };

        // Addressables.LoadAssetAsync<GameObject>(AssetPath.SpeakerPortraitPrefab).Completed += handle =>
        // {
        //     if (handle.Status != AsyncOperationStatus.Succeeded)
        //     {
        //         Debug.Log("Failed to load portrait prefab");
        //         Addressables.Release(handle);
        //         return;
        //     }
        //     
        //     GameObject go = Instantiate(handle.Result, pos, false);
        //     SpeakerPortrait speakerPortrait = go.GetComponent<SpeakerPortrait>();
        //
        //     Addressables.Release(handle);
        //     
        //     Addressables.LoadAssetAsync<Sprite>(initialPortraitSpritePath).Completed += spriteHandle =>
        //     {
        //         if (speakerPortrait == null)
        //         {
        //             Debug.Log("SpeakerPortrait is null");
        //             return;
        //         }
        //         
        //         if (spriteHandle.Status != AsyncOperationStatus.Succeeded)
        //         {
        //             Debug.Log("Failed to load portrait sprite");
        //             Addressables.Release(spriteHandle);
        //             return;
        //         }
        //         
        //         Sprite speakerSprite = Instantiate<Sprite>(spriteHandle.Result);
        //
        //         speakerPortrait.SetImage(speakerSprite);
        //         speakerPortrait.SetName(characterName);
        //
        //         if (!_speakerPortraits.ContainsKey(position))
        //         {
        //             _speakerPortraits[position] = new();
        //         }
        //         
        //         _speakerPortraits[position].Add(speakerPortrait);
        //         Addressables.Release(spriteHandle);
        //     };
        // };
    }
}
