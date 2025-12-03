using UnityEngine;

public static class DebugHelper
{
    public static void Log(string str)
    {
        #if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isCompiling || UnityEditor.EditorApplication.isUpdating)
            return;
        
        Debug.Log(str);
        #endif
    }
}
