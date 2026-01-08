using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineSystem : Singleton<TimelineSystem>
{
    [SerializeField] private PlayableDirector _playableDirector;

    public bool canPlay = true;

    private Coroutine _waitPlayableCoroutine;
    
    protected override void Awake()
    {
        base.Awake();
        
        transform.AssignChildVar<PlayableDirector>("PlayableDirector", ref _playableDirector);
    }

    private void OnEnable()
    {
        DialogueService.Instance.OnDialogueEnd += OnDialogueEnd;
    }

    private void OnDisable()
    {
        DialogueService.Instance.OnDialogueEnd -= OnDialogueEnd;
    }

    private void Start()
    {
        
    }

    public void SetBinding()
    {
        
    }

    private void OnDialogueEnd()
    {
        _playableDirector.Play();
    }

    public void SetPlayable(string playablePath)
    {
        canPlay = false;
        StopPlayable();
        
        var op = Addressables.LoadAssetAsync<TimelineAsset>(playablePath);
        op.Completed += handle =>
        {
            _playableDirector.playableAsset = op.Result;
            canPlay = true;
        };
    }

    public void StopPlayable()
    {
        Debug.Log("Stopping playable");
        _playableDirector.Stop();
    }
    
    public void StartPlayable()
    {
        Debug.Log("Starting playable");
        _playableDirector.Play();
    }

    public void PausePlayable()
    {
        Debug.Log("Pausing playable");
        _playableDirector.Pause();
    }

    IEnumerator WaitPlayableCoroutine()
    {
        int loopCnt = 0;
        while (!canPlay && loopCnt < ConstValue.MAX_LOOP)
        {
            yield return new WaitForSeconds(0.05f);
            loopCnt++;
        }
    }
}
