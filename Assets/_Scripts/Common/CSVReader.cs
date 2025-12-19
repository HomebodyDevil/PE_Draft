using System.Collections.Generic;
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

    public List<DialogueLine> MakeDialogueLineFromCSV(string csvStr)
    {
        return null;
    }
}
