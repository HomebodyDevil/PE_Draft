using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CSVReader
{
    // public async Task<string> ReadCSV(string path)
    // {
    //     AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(path);
    //     TextAsset ta = await handle.Task;
    //
    //     string loadedAsset = ta.text;
    //     Addressables.Release(handle);
    //
    //     return loadedAsset;
    // }

    public List<DialogueLine> MakeDialogueLinesFromCSV(string csvStr)
    {
        List<DialogueLine> dialogueLines = new();
        
        StringReader sr = new(csvStr);
        StringBuilder sb = new();
        
        // 미리 한 번 ReadLine해놓는다.
        // 첫 Line은 Category 텍스트이기 때문.
        string line = sr.ReadLine();
        List<string> vars = new();
        // while ((line = sr.ReadLine()) != null)
        // {
        //     bool inQuotes = false;
        //     vars.Clear();
        //     sb.Clear();
        //
        //     int i = 0;
        //     for (i = 0; i < line.Length; i++)
        //     {
        //         char c = line[i];
        //         if (c == '"')
        //         {
        //             // " ABC"" CDEF "와 같은 상황.
        //             if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
        //             {
        //                 sb.Append('"');
        //                 i++;
        //             }
        //             else // 첫, 마지막 "를 만났을 경우.
        //             {
        //                 inQuotes = !inQuotes;
        //             }
        //         }
        //         else if (c == ',' && !inQuotes)
        //         {
        //             vars.Add(sb.ToString().Trim());
        //             sb.Clear();
        //         }
        //         else
        //         {
        //             sb.Append(c);
        //         }
        //     }
        //     
        //     vars.Add(sb.ToString().Trim());
        //         
        //     //Debug.Log(vars[1]);
        //     dialogueLines.Add(new(vars));
        // }
        
        int ich = 0;
        bool inQuotes = false;
        while ((ich = sr.Read()) != -1)
        {
            char c = (char)ich;
            if (c == '"')
            {
                if (inQuotes)
                {
                    int next = sr.Peek();
                    if (next == '"')
                    {
                        // "를 문자열에 추가하기 위함.
                        sr.Read();
                        sb.Append('"');
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    inQuotes = true;
                }

                continue;
            }

            if (c == ',' && !inQuotes)
            {
                vars.Add(sb.ToString().Trim());
                sb.Clear();
                continue;
            }
            
            // 대사 문자열도 아닌데 개행 등이 왔다면.
            // 바로 종료.
            if (!inQuotes && (c == '\n' || c == '\r'))
            {
                if (c == '\r' && sr.Peek() == '\n')
                    sr.Read();
                
                vars.Add(sb.ToString().Trim());
                sb.Clear();

                if (!IsAllEmpty(vars))
                {
                    for (int k = 0; k < vars.Count; k++)
                        Debug.Log($"[CSV] col{k} = <{vars[k]}>");
                    
                    dialogueLines.Add(new DialogueLine(new List<string>(vars)));
                }

                vars.Clear();
                continue;
            }
            sb.Append(c);
        }

        // 남은 거 처리.
        if (sb.Length > 0 || vars.Count > 0)
        {
            vars.Add(sb.ToString().Trim());
            if (!IsAllEmpty(vars))
            {
                for (int k = 0; k < vars.Count; k++)
                    Debug.Log($"[CSV] col{k} = <{vars[k]}>");
                
                dialogueLines.Add(new DialogueLine(new List<string>(vars)));
            }
        }
        
        return dialogueLines;
    }

    private bool IsAllEmpty(List<string> vars)
    {
        for (int i = 0; i < vars.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(vars[i]))
                return false;
        }

        return true;
    }
}
