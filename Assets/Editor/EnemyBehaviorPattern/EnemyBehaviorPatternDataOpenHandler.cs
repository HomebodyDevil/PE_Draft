using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class EnemyBehaviorPatternDataOpenHandler
{
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceID);

        if (obj is EnemyBehaviorPatternData data)
        {
            EnemyBehaviorPatternDataEditorWindow.Open(data);
            return true;
        }

        return false;
    }
}