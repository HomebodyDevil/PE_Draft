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
        while ((line = sr.ReadLine()) != null)
        {
            bool inQuotes = false;
            vars.Clear();
            sb.Clear();
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    // " ABC"" CDEF "와 같은 상황.
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else // 첫, 마지막 "를 만났을 경우.
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    vars.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            
            vars.Add(sb.ToString().Trim());
            //Debug.Log(vars[1]);
            dialogueLines.Add(new(vars));
        }
        
        return dialogueLines;
    }
}
