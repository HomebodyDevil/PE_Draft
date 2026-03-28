using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterSkillButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillDescription;
    [SerializeField] private Image skillImage;
    [SerializeField] private Button skillButton;

    private Character playerCharacter;
    private List<GameAbility> characterSkills;

    private Coroutine executeSkillsCoroutine;
    
    private void Awake()
    {
        if (skillName == null) transform.AssignChildVar<TextMeshProUGUI>("SkillName", ref skillName);
        if (skillDescription == null)
            transform.AssignChildVar<TextMeshProUGUI>("SkillDescription", ref skillDescription);
        if (skillImage == null) transform.AssignChildVar<Image>("SkillImage", ref skillImage);
        if (skillButton == null) skillButton = GetComponent<Button>();
    }

    private void OnDestroy()
    {
        skillButton.onClick.RemoveAllListeners();
        if (executeSkillsCoroutine != null)
            StopCoroutine(executeSkillsCoroutine);
    }

    public void SetButton(Character character)
    {
        playerCharacter = character;
        CharacterSkill characterSkill = character.CharacterSkill;
        if (characterSkill == null)
        {
            Debug.LogError("CharacterSkill is null");
            return;
        }

        SetCharacterSkills(new(characterSkill.SkillAbilities));

        skillName.text = characterSkill.SkillName;
        skillDescription.text = characterSkill.SkillDescription;
        skillImage.sprite = characterSkill.SkillImage;

        skillButton.onClick.AddListener(ExecuteSkills);
    }

    public void SetCharacterSkills(List<GameAbility> _characterSkills)
    {
        characterSkills = _characterSkills;
    }

    public void ExecuteSkills()
    {
        executeSkillsCoroutine = StartCoroutine(ExecuteSkillsCoroutine());
    }

    IEnumerator ExecuteSkillsCoroutine()
    {
        foreach (var skill in characterSkills)
        {
            if (skill is TargetGameAbility targetSkill)
            {
                Debug.Log("Should SetTarget");
                yield return SetTargetCoroutine(targetSkill);
            }
            
            GameAbilitySystem.Instance.RequestPerformGameAbility(playerCharacter, new() { skill });
        }
    }

    IEnumerator SetTargetCoroutine(TargetGameAbility targetGameAbility = null)
    {
        Debug.Log("SettingTarget");
        
        PlayerActions playerActions = InputManager.Instance.PlayerActions;
        if (playerActions == null)
        {
            Debug.LogError("PlayerActions is null");
            yield break;
        }

        Character targetCharacter = null;
        
        while (true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                yield return null;
                continue;
            }
            
            GameObject targetObject = hit.collider.gameObject;

            if (!targetGameAbility.IsValidTarget(targetObject))
            {
                yield return null;
                continue;
            }

            if (targetObject.TryGetComponent<CharacterView>(out CharacterView targetCharacterView))
            {
                targetCharacter = targetCharacterView.Character;
            }
            else
                Debug.Log("targetObject is not Character Object");

            if (playerActions.Default.MouseLeftButton.WasPressedThisFrame())
            {
                targetGameAbility.SetTargets(new() {targetCharacter});
                Debug.Log($"Target Selected : {targetCharacter.CharacterName}");
                yield break;
            }

            yield return null;
        }
    }
}