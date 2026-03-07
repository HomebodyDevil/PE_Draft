using System;
using System.Collections;
using System.Collections.Generic;
using PEEnum;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private CharacterData _defaultCharacterData;
    [SerializeField] private CharacterVisual _characterVisual;
    [SerializeField] private Transform _blockAmountView;
    [SerializeField] private TextMeshProUGUI _blockAmountText;
    [SerializeField] private Transform _dialogueBubble;
    [SerializeField] private TextMeshProUGUI _dialogueBubbleText;
    [SerializeField] private Animator _animator;


    public Character Character { get; private set; }

    public Transform Text;

    private Coroutine _setCharacterViewCoroutine;

    private AsyncOperationHandle<GameObject> _characterViewHandle;

    private void Awake()
    {
        SetCharacter();
        SetVar();
    }

    private void Start()
    {
        if (Text != null)
            Text.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PEEvent.OnCharacterGainedBlock += SetBlockView;
        PEEvent.OnCharacterLostBlock += SetBlockView;
        PEEvent.OnSetDialogueBubble += SetDialogueBubble;

        PEEvent.OnPlayTriggerAnimation += SetAnimationTrigger;
    }

    private void OnDisable()
    {
        PEEvent.OnCharacterGainedBlock -= SetBlockView;
        PEEvent.OnCharacterLostBlock -= SetBlockView;
        PEEvent.OnSetDialogueBubble -= SetDialogueBubble;
        
        PEEvent.OnPlayTriggerAnimation -= SetAnimationTrigger;
    }

    private void SetAnimationTrigger(Character character, string param)
    {
        if (character != Character) return;
        if (_animator == null)
        {
            Debug.LogError($"No Animator : {Character.CharacterName}");
            return;
        }
        
        _animator.SetTrigger(param);
    }

    private void SetBlockView(Character character, int blockAmount)
    {
        if (Character != character) return;
        
        _blockAmountText.text = blockAmount.ToString();
        _blockAmountView.gameObject.SetActive(blockAmount > 0);
    }

    private void SetDialogueBubble(bool enable, Character character, string text)
    {
        if (Character != character) return;
        
        string dialogue = enable ? text : "";
        
        _dialogueBubbleText.text = dialogue;
        _dialogueBubble.gameObject.SetActive(enable);
    }

    private void OnDestroy()
    {
        if (_setCharacterViewCoroutine != null)
        {
            StopCoroutine(_setCharacterViewCoroutine);
            _setCharacterViewCoroutine = null;
        }

        if (_characterViewHandle.IsValid())
        {
            Debug.Log("Releasing");
            Addressables.Release(_characterViewHandle);
        }
    }

    // private void SetTestGA()
    // {
    //     if (Character != null && Character.TeamType.Team == Team.Enemy)
    //     {
    //         TestGAContext ctx = new("Reacting to TurnEnd", Text);
    //         TestGA ga = new(ctx);
    //
    //         Debug.Log("Test Reaction 등록");
    //         GameAbilitySystem.Instance.AddReaction<EndCharacterTurnGA>(
    //             ReactionTiming.Pre,
    //             Character,
    //             ga,
    //             ReactionTarget.Hostile,
    //             ConstValue.INFINITE_TURN_COUNT,
    //             false);
    //     }
    // }

    private void SetVar()
    {
        if (Text == null && Character != null && Character.TeamType.Team == Team.Enemy) 
            transform.AssignChildVar<Transform>("Panel", ref Text);
        if (_characterVisual == null)
            transform.AssignChildVar<CharacterVisual>("CharacterVisual", ref _characterVisual);
        if (_blockAmountView == null)
            transform.AssignChildVar<Transform>("BlockAmountView", ref _blockAmountView);
        if (_blockAmountText == null)
            transform.AssignChildVar<TextMeshProUGUI>("BlockAmountText", ref _blockAmountText);
        if (_dialogueBubble == null)
            transform.AssignChildVar<Transform>("DialogueBubble", ref _dialogueBubble);
        if (_dialogueBubbleText == null)
            transform.AssignChildVar<TextMeshProUGUI>("DialogueBubbleText", ref _dialogueBubbleText);
        if (_animator == null)
            transform.AssignChildVar<Animator>("CharacterAnimator", ref _animator);
    }

    public void SetCharacter(Character character = null)
    {
        //Debug.Log("DefaultCharacter를 넣을지 말지 고민중");
        // if (character == null && _defaultCharacterData != null)
        // {
        //     Character = new(_defaultCharacterData);
        //     return;
        // }

        Character = character;
    }

    public void SetCharacterView(Character character = null)
    {
        if (character == null)
        {
            Debug.Log("Character is null");
            return;
        }
        
        if (_setCharacterViewCoroutine != null)
        {
            Debug.Log("SetCharacterViewCoroutine");
            StopCoroutine(_setCharacterViewCoroutine);
            _setCharacterViewCoroutine = null;
        }

        _setCharacterViewCoroutine = StartCoroutine(SetCharacterViewCoroutine(character));
    }

    private IEnumerator SetCharacterViewCoroutine(Character character)
    {
        if (character == null)
        {
            Debug.Log("Character is null");
            yield break;
        }
        
        Character = character;

        string characterName = character.CharacterName;
        string team = character.TeamType.Team.ToString();

        // Sprite를 로드했던 흔적.
        // var locHandle = Addressables.LoadResourceLocationsAsync(
        //     new List<object>() { characterName, team, "CharacterVisual" },
        //     Addressables.MergeMode.Intersection,
        //     typeof(Sprite));
        
        var locHandle = Addressables.LoadResourceLocationsAsync(
            new List<object>() { characterName, team, "CharacterVisual" },
            Addressables.MergeMode.Intersection,
            typeof(RuntimeAnimatorController));
        
        yield return locHandle;
        if (locHandle.Status != AsyncOperationStatus.Succeeded ||
            locHandle.Result.Count == 0)
        {
            Debug.Log("Theres no location of asset");
            yield break;
        }

        var assetLoc = locHandle.Result[0];
        //var assetHandle = Addressables.LoadAssetAsync<Sprite>(assetLoc);
        // var assetHandle = Addressables.LoadAssetAsync<Animator>(assetLoc);
        var assetHandle = Addressables.LoadAssetAsync<RuntimeAnimatorController>(assetLoc);

        yield return assetHandle;
        if (assetHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Theres no asset");
            yield break;
        }
        
        var visualAsset = assetHandle.Result;
        _characterVisual.SetVisual(visualAsset);
        _characterVisual.SetOperationHandle(assetHandle);
        
        Debug.Log("SetCharacterViewCoroutine에서 TestGA를 등록함.");
        
        Addressables.Release(locHandle);
        
        _setCharacterViewCoroutine = null;
        yield break;
    }

    // public void SetCharacter(CharacterData characterData)
    // {
    //     _setCharacterCoroutine = StartCoroutine(SetCharacterCoroutine(characterData));
    // }
    //
    // public IEnumerator SetCharacterCoroutine(CharacterData characterData)
    // {
    //     // Character의 Data(스펙?)을 Setting.
    //     Character = new(characterData);
    //     yield return SetCharacterViewCoroutine(characterData);
    //
    //     SetTestGA();
    //     _setCharacterCoroutine = null;
    //
    //     yield break;
    // }

    // private IEnumerator SetCharacterViewCoroutine(CharacterData characterData)
    // {
    //     string characterName = characterData.CharacterName;
    //     string teamType = characterData.TeamType.Team.ToString();
    //     
    //     Debug.Log($"characterName: {characterName}, teamType: {teamType}");
    //
    //     var locHandle = Addressables.LoadResourceLocationsAsync(
    //         new List<object>() { characterName, teamType, "CharacterVisual" },
    //         Addressables.MergeMode.Intersection,
    //         typeof(Sprite));
    //
    //     yield return locHandle;
    //     if (locHandle.Status != AsyncOperationStatus.Succeeded ||
    //         locHandle.Result.Count == 0)
    //     {
    //         Debug.Log("Failed to load CharacterVisual Location");
    //         yield break;
    //     }
    //
    //     var assetLoc = locHandle.Result[0];
    //     var assetHandle = Addressables.LoadAssetAsync<Sprite>(assetLoc);
    //     
    //     yield return assetHandle;
    //     if (assetHandle.Status != AsyncOperationStatus.Succeeded)
    //     {
    //         Debug.Log("Failed to load CharacterVisual Asset");
    //         yield break;
    //     }
    //     
    //     var visualAsset = assetHandle.Result;
    //     
    //     _characterVisual.SetVisual(visualAsset);
    //     _characterVisual.SetOperationHandle(assetHandle);
    //     
    //     Addressables.Release(locHandle);
    // }
    //
    // public IEnumerator SetCharacterCoroutine(AssetReferenceT<CharacterData> characterDataRef)
    // {
    //     var handle = Addressables.LoadAssetAsync<CharacterData>(characterDataRef);
    //
    //     yield return handle;
    //
    //     if (handle.Status != AsyncOperationStatus.Succeeded)
    //     {
    //         Debug.Log("Failed to load character data");
    //         yield break;
    //     }
    //
    //     Character = new(handle.Result);
    //     _setCharacterCoroutine = null;
    // }

    public void SetHandle(AsyncOperationHandle<GameObject> handle)
    {
        _characterViewHandle = handle;
    }
}