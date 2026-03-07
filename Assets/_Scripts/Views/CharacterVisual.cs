using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterVisual : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Animator _animator;
    private AsyncOperationHandle<Sprite> _opHandle;
    private AsyncOperationHandle<RuntimeAnimatorController> _opHandleAnimator;

    private void Awake()
    {
        if (_sr == null)  _sr = GetComponent<SpriteRenderer>();
        if (_animator == null)  transform.AssignChildVar<Animator>("CharacterAnimator", ref _animator);
    }

    private void OnDestroy()
    {
        if (_opHandle.IsValid()) Addressables.Release(_opHandle);
    }

    public void SetVisual(Sprite sprite)
    {
        Debug.Log($"sprite를 설정중. 차후, 애니메이션으로 바뀌지 않을까 : {sprite.name}");
        _sr.sprite = sprite;
    }

    public void SetVisual(RuntimeAnimatorController animator)
    {
        _animator.runtimeAnimatorController = animator;
    }

    public void SetOperationHandle(AsyncOperationHandle<Sprite> opHandle)
    {
        _opHandle = opHandle;
    }
    
    public void SetOperationHandle(AsyncOperationHandle<RuntimeAnimatorController> opHandle)
    {
        _opHandleAnimator = opHandle;
    }
}
