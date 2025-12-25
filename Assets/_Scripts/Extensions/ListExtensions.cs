using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(0, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public static T GetRandom<T>(this List<T> list)
    {
        if (list.Count == 0) return default;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static List<T> PickN<T>(this List<T> list, int n, bool replacement=false)
    {
        if (list == null || list.Count == 0)
        {
            Debug.Log("Empty or Null List");
            return null;
        }

        if (replacement)
        {
            List<T> res = new();
            
            for (int i = 0; i < n; i++)
            {
                res.Add(list[UnityEngine.Random.Range(0, list.Count)]);
            }
            
            return res;
        }
        else
        {
            int cnt = Mathf.Min(list.Count, n);
            
            List<T> tmp = new (list);
            tmp.Shuffle();
            
            return tmp.GetRange(0, cnt);
        }
    }
}
