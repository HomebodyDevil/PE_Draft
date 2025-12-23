using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleTargetsSystem : Singleton<BattleTargetsSystem>
{
    [SerializeField] private Camera _battleViewCamera;

    public Action<string> OnFindTarget;
    public Action<Character> OnFoundTarget;
    public Action<Character> OnLostTarget;
    public Action OnClearTargets;
    
    // Player가 Target으로 지정한 것
    public List<Character> CurrentTargets { get; private set; } = new();
    
    // PreReaction은 CurrentGA보다 우선적으로 동작할텐데, CurrentTarget을 사용하면 안 되므로
    // (Pre)Reaction을 위한 Target(들)에 대한 List를 별도로 두도록 한다.
    public List<Character> ReactionAbilityTargets { get; private set; } = new();

    protected override void Awake()
    {
        base.Awake();
        
        if (_battleViewCamera == null) transform.AssignChildVar<Camera>("BattleViewCamera", ref _battleViewCamera);
    }

    private void OnEnable()
    {
        OnFindTarget += FindTargetUnderMouse;
        OnFoundTarget += AddTarget;
        OnLostTarget += RemoveTarget;
        OnClearTargets += ClearTargets;
    }

    private void OnDisable()
    {
        OnFindTarget -= FindTargetUnderMouse;
        OnFoundTarget -= AddTarget;
        OnLostTarget -= RemoveTarget;
        OnClearTargets -= ClearTargets;
    }

    private void AddTarget(Character target)
    {
        if (target != null && !CurrentTargets.Contains(target))
        {
            //Debug.Log($"TragetAdded : {target.name}");
            Debug.Log("target Added");
            CurrentTargets.Add(target);
        }
        else
        {
            Debug.Log("target Not Added");
        }
    }

    private void RemoveTarget(Character target)
    {
        if (target != null)
        {
            Debug.Log("Remove Target");
            CurrentTargets.Remove(target);
        }
    }

    private void ClearTargets()
    {
        Debug.Log("Clear Target");
        CurrentTargets?.Clear();
    }

    private void FindTargetUnderMouse(string tag)
    {
        Ray ray = _battleViewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        int layerMask = LayerMask.GetMask("Character", "Targetable");
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            if (hit.collider.CompareTag(tag))
            {
                Debug.Log($"Gotcha. tag : {tag}");
                if (hit.transform.TryGetComponent(out CharacterView characterView) && 
                    !CurrentTargets.Contains(characterView.Character))
                {
                    OnFoundTarget?.Invoke(characterView.Character);
                }
            }
            else
            {
                OnClearTargets?.Invoke();
            }
        }
        else
        {
            OnClearTargets?.Invoke();
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneService.Instance?.ChangeScene(sceneName);
    }
}
