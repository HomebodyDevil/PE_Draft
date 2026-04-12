using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSkillData))]
public class CharacterSkillDataEditor : Editor
{
    private SerializedProperty skillImageProp;
    private SerializedProperty skillCostProp;
    private SerializedProperty skillNameProp;
    private SerializedProperty skillDescriptionProp;
    private SerializedProperty skillAbilitiesProp;

    private const float CardHeight = 120f;
    private const float OuterPadding = 10f;
    private const float InnerPadding = 8f;
    private const float ImageRatio = 0.28f; // 왼쪽 28%

    private void OnEnable()
    {
        skillImageProp = serializedObject.FindProperty("skillImage");
        skillCostProp = serializedObject.FindProperty("skillCost");
        skillNameProp = serializedObject.FindProperty("skillName");
        skillDescriptionProp = serializedObject.FindProperty("skillDescription");
        skillAbilitiesProp = serializedObject.FindProperty("skillAbilities");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawSkillPreviewCard();

        EditorGUILayout.Space(10);

        DrawMainFields();

        EditorGUILayout.Space(10);

        DrawAutoExpandedFields();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSkillPreviewCard()
    {
        Rect rect = GUILayoutUtility.GetRect(0, CardHeight, GUILayout.ExpandWidth(true));

        // 배경
        EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1f));

        // 바깥 라인
        Handles.BeginGUI();
        Handles.color = new Color(0.35f, 0.35f, 0.35f, 1f);
        Handles.DrawAAPolyLine(
            2f,
            new Vector3(rect.x, rect.y),
            new Vector3(rect.xMax, rect.y),
            new Vector3(rect.xMax, rect.yMax),
            new Vector3(rect.x, rect.yMax),
            new Vector3(rect.x, rect.y));
        Handles.EndGUI();

        Rect contentRect = new Rect(
            rect.x + OuterPadding,
            rect.y + OuterPadding,
            rect.width - OuterPadding * 2,
            rect.height - OuterPadding * 2);

        float imageWidth = contentRect.width * ImageRatio;

        Rect imageRect = new Rect(
            contentRect.x + InnerPadding,
            contentRect.y + InnerPadding,
            imageWidth - InnerPadding * 2,
            contentRect.height - InnerPadding * 2);

        Rect textRect = new Rect(
            contentRect.x + imageWidth + InnerPadding,
            contentRect.y + InnerPadding,
            contentRect.width - imageWidth - InnerPadding * 2,
            contentRect.height - InnerPadding * 2);

        DrawImageArea(imageRect);
        DrawTextArea(textRect);
    }

    private void DrawImageArea(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));

        Sprite sprite = skillImageProp.objectReferenceValue as Sprite;

        if (sprite != null)
        {
            Texture2D texture = AssetPreview.GetAssetPreview(sprite);
            if (texture == null)
                texture = AssetPreview.GetMiniThumbnail(sprite);

            if (texture != null)
            {
                ScaleMode scaleMode = ScaleMode.ScaleToFit;
                GUI.DrawTexture(rect, texture, scaleMode);
            }
            else
            {
                GUI.Label(rect, "Preview Loading...", GetCenteredMiniLabel());
            }
        }
        else
        {
            GUI.Label(rect, "No Image", GetCenteredMiniLabel());
        }
    }

    private void DrawTextArea(Rect rect)
    {
        string skillName = string.IsNullOrWhiteSpace(skillNameProp.stringValue)
            ? "Skill Name"
            : skillNameProp.stringValue;

        string skillDescription = string.IsNullOrWhiteSpace(skillDescriptionProp.stringValue)
            ? "Skill description will appear here."
            : skillDescriptionProp.stringValue;

        Rect nameRect = new Rect(rect.x, rect.y, rect.width, rect.height * 0.38f);
        Rect descRect = new Rect(rect.x, rect.y + rect.height * 0.42f, rect.width, rect.height * 0.58f);

        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            wordWrap = true,
            richText = true
        };

        GUIStyle descStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 11,
            wordWrap = true,
            richText = true
        };

        EditorGUI.LabelField(nameRect, skillName, nameStyle);
        EditorGUI.LabelField(descRect, skillDescription, descStyle);
    }

    private void DrawMainFields()
    {
        EditorGUILayout.LabelField("Core Skill Data", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(skillImageProp);
        EditorGUILayout.PropertyField(skillNameProp);
        EditorGUILayout.PropertyField(skillDescriptionProp);
        EditorGUILayout.PropertyField(skillCostProp);
        EditorGUILayout.PropertyField(skillAbilitiesProp, true);
    }

    private void DrawAutoExpandedFields()
    {
        EditorGUILayout.LabelField("Additional / Future Fields", EditorStyles.boldLabel);

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.name == "m_Script")
                continue;

            if (iterator.name == "skillImage" ||
                iterator.name == "skillName" ||
                iterator.name == "skillDescription" ||
                iterator.name == "skillCost" ||
                iterator.name == "skillAbilities")
                continue;

            EditorGUILayout.PropertyField(iterator, true);
        }
    }

    private GUIStyle GetCenteredMiniLabel()
    {
        return new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
    }
}