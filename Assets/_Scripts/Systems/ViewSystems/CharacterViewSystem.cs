using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterViewSystem : Singleton<EnemyCharacterViewSystem>
{
    [SerializeField] protected List<Transform> _characterPositions = new();
    public List<CharacterView> CharacterViews { get; private set; } = new();
    protected Coroutine _makeCharacterViewsCoroutine;
    
    protected virtual void MakeCharacterViews(Team team)
    {
        if (_makeCharacterViewsCoroutine != null)
        {
            Debug.Log("MakeEnemyCharacterViewsCoroutine is not null");
            StopCoroutine(_makeCharacterViewsCoroutine);
            _makeCharacterViewsCoroutine = null;
        }
        
        _makeCharacterViewsCoroutine = StartCoroutine(MakeCharacterViewsCoroutine(team));
    }
    
    protected IEnumerator MakeCharacterViewsCoroutine(Team team)
    {
        string teamStr = team.ToString();
        Debug.Log($"teamStr :  {teamStr}");
        var locHandle = Addressables.LoadResourceLocationsAsync(
            new List<object>(){"Default", "CharacterView", teamStr},
            Addressables.MergeMode.Intersection,
            typeof(GameObject));
        yield return locHandle;
    
        if (locHandle.Status != AsyncOperationStatus.Succeeded || 
            locHandle.Result.Count == 0)
        {
            Debug.Log("Failed to load character view asset ref");
            yield break;
        }
    
        var characterViewAssetLoc = locHandle.Result[0];
        
        var assetHandle = Addressables.LoadAssetAsync<GameObject>(characterViewAssetLoc);
        yield return assetHandle;
    
        if (assetHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Failed to load character view asset");
            yield break;
        }
        
        var characterViewAsset = assetHandle.Result;
    
        for (int i = 0; i < _characterPositions.Count; i++)
        {
            var characterViewInstance = 
                Instantiate(characterViewAsset, 
                    _characterPositions[i],
                    false);
    
            CharacterView charView = characterViewInstance.GetComponent<CharacterView>();
            charView.SetHandle(assetHandle);
            
            CharacterViews.Add(charView);
        }
        
        Addressables.Release(locHandle);

        _makeCharacterViewsCoroutine = null;
    }

    protected virtual void SetCharacterPositions()
    {
        
    }

    protected void SetEnableCharacterView(Character character, bool enable)
    {
        foreach (var characterView in CharacterViews)
            if (characterView.Character == character)
                characterView.gameObject.SetActive(enable);
    }

    public CharacterView GetCharacterView(Character character)
    {
        foreach (var characterView in CharacterViews)
        {
            if (characterView.Character == character) return characterView;
        }

        Debug.Log("CharacterView not found");
        return null;
    }
}
