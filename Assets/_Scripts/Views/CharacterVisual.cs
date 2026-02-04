using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterVisual : MonoBehaviour
{
    private SpriteRenderer _sr;
    private AsyncOperationHandle<Sprite> _opHandle;

    private void Awake()
    {
        if (_sr == null)  _sr = GetComponent<SpriteRenderer>();
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

    public void SetOperationHandle(AsyncOperationHandle<Sprite> opHandle)
    {
        _opHandle = opHandle;
    }
}
