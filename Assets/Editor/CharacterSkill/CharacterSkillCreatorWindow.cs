using UnityEditor;
using UnityEngine;
using System.IO;

public class CharacterSkillCreatorWindow : EditorWindow
{
    private string folderPath = "Assets";
    private string fileName = "NewCharacterSkill";

    [MenuItem("Tools/Skill/Character Skill Creator")]
    public static void Open()
    {
        GetWindow<CharacterSkillCreatorWindow>("Skill Creator");
    }

    private void OnGUI()
    {
        GUILayout.Space(8);
        EditorGUILayout.LabelField("Character Skill Asset Creator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "새 CharacterSkillData ScriptableObject를 생성합니다.",
            MessageType.Info);

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (GUILayout.Button("Select", GUILayout.Width(70)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    folderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Folder",
                        "Assets 폴더 내부만 선택할 수 있습니다.",
                        "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        fileName = EditorGUILayout.TextField("File Name", fileName);

        GUILayout.Space(12);

        GUI.backgroundColor = new Color(0.35f, 0.85f, 0.45f);
        if (GUILayout.Button("Create Character Skill Asset", GUILayout.Height(30)))
        {
            CreateSkillAsset();
        }
        GUI.backgroundColor = Color.white;
    }

    private void CreateSkillAsset()
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Error", "유효한 Assets 폴더 경로가 아닙니다.", "OK");
            return;
        }

        CharacterSkillData asset = ScriptableObject.CreateInstance<CharacterSkillData>();

        string safeFileName = string.IsNullOrWhiteSpace(fileName) ? "NewCharacterSkill" : fileName.Trim();
        string assetPath = Path.Combine(folderPath, safeFileName + ".asset");
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        EditorGUIUtility.PingObject(asset);
    }
}