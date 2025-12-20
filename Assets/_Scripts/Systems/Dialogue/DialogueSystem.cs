using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : Singleton<DialogueSystem>
{
    [SerializeField] private Transform _dialoguePanel;
    [SerializeField] private Transform _backgroundPanel;
    [SerializeField] private Transform _selectButtonsPanel;
    [SerializeField] private Transform _speakerImages;
    [SerializeField] private Transform _speakerImageLeftPos;
    [SerializeField] private Transform _speakerImageRightPos;
    [SerializeField] private TextMeshProUGUI _speakerNameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private Button _clickCatcher;

    private Coroutine _playDialogueCoroutine;
    private string _currentDialogue;
    
    protected override void Awake()
    {
        base.Awake();
        SetVars();
    }

    private void Start()
    {
        _backgroundPanel.gameObject.SetActive(false);
        _selectButtonsPanel.gameObject.SetActive(false);
        _clickCatcher.onClick.AddListener(OnClickClockCatcher);
    }

    private void OnEnable()
    {
        DialogueService.Instance.OnSetCurrentDialogue += SetCurrentDialogue;
        DialogueService.Instance.OnSetSpeakerName += SetSpeakerName;
    }

    private void OnDisable()
    {
        DialogueService.Instance.OnSetCurrentDialogue -= SetCurrentDialogue;
        DialogueService.Instance.OnSetSpeakerName -= SetSpeakerName;
    }

    private void SetVars()
    {
        if (_dialoguePanel == null) transform.AssignChildVar<Transform>("DialoguePanel", ref _dialoguePanel);
        if (_backgroundPanel == null) transform.AssignChildVar<Transform>("BackgroundPanel", ref _backgroundPanel);
        if (_dialogueText == null) transform.AssignChildVar<TextMeshProUGUI>("DialogueText", ref _dialogueText);
        if (_speakerImages == null) transform.AssignChildVar<Transform>("SpeakerImages", ref _speakerImages);
        if (_speakerImageLeftPos == null) transform.AssignChildVar<Transform>("SpeakerImageLeftPos", ref _speakerImageLeftPos);
        if (_speakerImageRightPos == null) transform.AssignChildVar<Transform>("SpeakerImageRightPos", ref _speakerImageRightPos);
        if (_speakerNameText == null) transform.AssignChildVar<TextMeshProUGUI>("SpeakerNameText", ref _speakerNameText);
        if (_clickCatcher == null) transform.AssignChildVar<Button>("ClickCatcher", ref _clickCatcher);
        if (_selectButtonsPanel == null) transform.AssignChildVar<Transform>("SelectButtonsPanel", ref _selectButtonsPanel);
    }

    private void OnClickClockCatcher()
    {
        DialogueService.Instance.OnDialogueClick?.Invoke();
    }

    private void SetSpeakerImage()
    {
        
    }

    private void SetCurrentDialogue(string dialogue)
    {
        _currentDialogue = dialogue;
    }

    private void SetSpeakerName(string speakerName)
    {
        _speakerNameText.text = speakerName;
    }

    private void SetDialogueText(string dialogueText)
    {
        _dialogueText.text = dialogueText;
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
        _playDialogueCoroutine = null;
        
        yield break;
    }
}
