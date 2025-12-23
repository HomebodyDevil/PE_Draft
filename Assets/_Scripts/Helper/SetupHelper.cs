using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public static class SetupHelper
{
    /// <summary>
    ///  해당하는 이름의 child를 변수에 할당.
    /// </summary>
    /// <param name="self">본인</param>
    /// <param name="childName">child의 이름</param>
    /// /// <param name="childVar">찾은 것을 할당할 child 변수의 이름</param>
    public static bool AssignChildVar<T>(
        this Transform self, 
        string childName, 
        ref T childVar) where T: Component
    {
        if (!CanSetup() || self == null || string.IsNullOrEmpty(childName)) return false;
    
        foreach (var tr in self.GetComponentsInChildren<Transform>(true)) // true : 비활성도 포함.
        {
            if (tr == self) continue;
            if (!tr.name.Equals(childName)) continue;

            if (tr.TryGetComponent<T>(out var comp))
            {
                childVar = comp;
                return true;
            }

            Debug.Log("해당 이름의 Child는 있으나, Comp는 없음.");
            return false;
        }

        Debug.Log("해당 이름의 Child가 없음.");
        return false;
    }

    public static bool GetChildByName<T>(
        this Transform self,
        string childName,
        out T var) where T: Component
    {
        var = default(T);
        
        if (!CanSetup() || self == null || string.IsNullOrEmpty(childName))
        {
            return false;
        }
    
        foreach (var tr in self.GetComponentsInChildren<Transform>(true))
        {
            if (tr == self) continue;
            if (!tr.name.Equals(childName)) continue;
    
            if (tr.TryGetComponent<T>(out var comp))
            {
                var = comp;
                return true;
            }
        }
        
        return false;
    }

    // public static void GetChildrenComponents<T>(
    //     this Transform self,
    //     ref List<T> list) where T: Component
    // {
    //     if (self == null) return;
    //
    //     list ??= new();
    //     list.AddRange(self.GetComponentsInChildren<T>(true));
    //
    //     if (self.TryGetComponent<T>(out var comp))
    //     {
    //         list.Remove(comp);
    //     }
    // }

    /// <summary>
    /// 루트 오브젝트로의 자식 오브젝트를 변수로 할당.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="rootObjName">Hierarchy에서의 루트 오브젝트의 이름</param>
    /// <param name="childName">루트 오브젝트의 자식 오브젝트의 이름</param>
    /// <param name="childVar">변수</param>
    /// <typeparam name="T">할당할 변수의 타입</typeparam>
    /// <returns></returns>
    public static bool AssignRootsChildVar<T>(
        this Transform self, 
        string rootObjName, 
        string childName, 
        ref T childVar
    )
        where T: Component
    {
        if (!CanSetup() || 
            self == null || 
            string.IsNullOrEmpty(rootObjName) || 
            string.IsNullOrEmpty(childName))
        {
            Debug.Log($"AssignVarForRootsChild Fail\nself : {self.name}\nrootObjName : {rootObjName}\nchildName : {childName}");
            return false;
        }

        var scene = self.gameObject.scene;  // self의 scene을 얻어온다.
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.Log($"{self.name}'s Scene is not Valid or not loaded.");
            return false;
        }
        
        Debug.Log($"Doing");
        
        foreach (var rootGO in scene.GetRootGameObjects())
        {
            if (rootGO.name.Equals(rootObjName))
            {
                foreach (var tr in rootGO.GetComponentsInChildren<Transform>(true))
                {
                    if (tr == rootGO.transform) continue;
                    if (tr.name.Equals(childName) && tr.TryGetComponent<T>(out var comp))
                    {
                        childVar = comp;
                        return true;
                    }
                }
            }
        }

        Debug.Log($"{rootObjName}라는 이름의 root 오브젝트가 존재하지 않음.");
        return false;
    }

    private static bool CanSetup()
    {
        #if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isCompiling ||
            UnityEditor.EditorApplication.isUpdating)
            return false;
        #endif
        
        return true;
    }
}
