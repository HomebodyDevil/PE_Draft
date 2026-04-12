using UnityEditor;
using UnityEngine;

public class EnemyBehaviorPatternDataEditorWindow : EditorWindow
{
    private EnemyBehaviorPatternData targetData;
    private SerializedObject serializedObject;
    private Editor cachedEditor;

    public static void Open(EnemyBehaviorPatternData data)
    {
        if (data == null) return;

        var window = GetWindow<EnemyBehaviorPatternDataEditorWindow>("Enemy Pattern Tool");
        window.targetData = data;
        window.serializedObject = new SerializedObject(data);

        if (window.cachedEditor != null)
        {
            DestroyImmediate(window.cachedEditor);
        }

        window.cachedEditor = Editor.CreateEditor(data);
        window.Show();
    }

    private void OnDisable()
    {
        if (cachedEditor != null)
        {
            DestroyImmediate(cachedEditor);
            cachedEditor = null;
        }
    }

    private void OnGUI()
    {
        if (targetData == null)
        {
            EditorGUILayout.HelpBox("EnemyBehaviorPatternData asset is not selected.", MessageType.Info);
            return;
        }

        if (serializedObject == null)
        {
            serializedObject = new SerializedObject(targetData);
        }

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Enemy Behavior Pattern Tool", EditorStyles.boldLabel);
        EditorGUILayout.ObjectField("Target Data", targetData, typeof(EnemyBehaviorPatternData), false);

        EditorGUILayout.Space();

        // 기본 인스펙터를 그대로 툴 창 안에 렌더링
        if (cachedEditor == null)
        {
            cachedEditor = Editor.CreateEditor(targetData);
        }

        if (cachedEditor != null)
        {
            cachedEditor.OnInspectorGUI();
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetData);
        }
    }
}