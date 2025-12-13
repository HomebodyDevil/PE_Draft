using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleSystem : Singleton<BattleSystem>
{
    [SerializeField] private Camera _battleViewCamera;

    public Action<string> OnFindTarget;
    public Action<Character> OnFoundTarget;
    public Action<Character> OnLostTarget;
    public Action OnClearTargets;
    
    public List<Character> CurrentTargets { get; private set; } = new();
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
            Debug.Log($"TragetAdded : {target.name}");
            CurrentTargets.Add(target);
        }
    }

    private void RemoveTarget(Character target)
    {
        if (target != null) CurrentTargets.Remove(target);
    }

    private void ClearTargets()
    {
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
                if (hit.transform.TryGetComponent(out Character character) && !CurrentTargets.Contains(character))
                {
                    OnFoundTarget?.Invoke(character);
                }
                else
                {
                    OnClearTargets?.Invoke();
                }
            }
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneService.Instance?.ChangeScene(sceneName);
    }
}
