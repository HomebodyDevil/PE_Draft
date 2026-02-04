using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class EventSceneSystem : Singleton<EventSceneSystem>
{
    [SerializeField] private Transform _eventPanel;
    [SerializeField] private Transform _eventTextPanel;
    [SerializeField] private TextMeshProUGUI _eventText;
    [SerializeField] private Transform _eventChoicesPanel;
    [SerializeField] private Transform _imagePanel;
    [SerializeField] private List<Button> _choiceButtons = new();

    private AsyncOperationHandle<EventData> _eventDataHandle;
    
    protected override void Awake()
    {
        base.Awake();
        SetVars();
    }

    private void Start()
    {
        _eventPanel.gameObject.SetActive(false);
        InitializeEventScene();
    }

    private void OnDestroy()
    {
        if (_eventDataHandle.IsValid())
            Addressables.Release(_eventDataHandle);
    }

    private void SetVars()
    {
        if (_eventPanel == null) transform.AssignChildVar<Transform>("EventPanel", ref _eventPanel);
        if (_eventTextPanel == null) transform.AssignChildVar<Transform>("EventTextPanel", ref _eventTextPanel);
        if (_eventText == null) transform.AssignChildVar<TextMeshProUGUI>("EventText", ref _eventText);
        if (_eventChoicesPanel == null) transform.AssignChildVar<Transform>("EventChoicesPanel", ref _eventChoicesPanel);
        if (_imagePanel == null) transform.AssignChildVar<Transform>("ImagePanel", ref _imagePanel);
        
        _choiceButtons.Clear();
        foreach (var button in _eventChoicesPanel.GetComponentsInChildren<Button>(includeInactive: true))
            if (button != null) _choiceButtons.Add(button);
    }

    public void OnMapButtonClick()
    {
        SceneService.Instance.ChangeScene(SceneType.MapScene);
    }
    
    private void InitializeEventScene()
    {
        Debug.Log("InitializeEventScene. 여기서 EventScene의 초기설정을 진행. Image는 아직 설정 X");
        _eventPanel.gameObject.SetActive(true);

        AssetReferenceT<EventData> eventDataRef = PlayerStatusService.Instance.CurrentMapNodeStatus.EventData;
        if (string.IsNullOrEmpty(eventDataRef.AssetGUID))
        {
            Debug.LogError("EventDataRef is empty string. Ref.");
            return;
        }
        LoadEventData(eventDataRef);
    }

    private Coroutine _loadEventDataCoroutine;
    private void LoadEventData(AssetReferenceT<EventData> eventDataRef)
    {
        if (_loadEventDataCoroutine != null)
        {
            Debug.Log("LoadEventDataCoroutine is running");
            StopCoroutine(_loadEventDataCoroutine);
            _loadEventDataCoroutine = null;
        }
        
        _loadEventDataCoroutine = StartCoroutine(LoadEventDataCoroutine(eventDataRef));
    }
    
    private IEnumerator LoadEventDataCoroutine(AssetReferenceT<EventData> eventDataRef)
    {
        if (eventDataRef == null)
        {
            Debug.LogError("LoadEventDataCoroutine is null");
            yield break;
        }
        
        Debug.Log($"EventDataRef : {eventDataRef.AssetGUID}");
        
        var handle = eventDataRef.LoadAssetAsync<EventData>();
        yield return handle;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.Log("LoadEventDataCoroutine failed");
            yield break;
        }

        _eventDataHandle = handle;
        
        EventData eventData = handle.Result;
        _eventText.text = eventData.EventText;

        Debug.Log($"EventData Text : {eventData.EventText}");
        
        _loadEventDataCoroutine = null;
        _eventPanel.gameObject.SetActive(true);
    }
}
