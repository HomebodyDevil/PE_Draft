using UnityEngine;

public class TitleSceneManager : MonoBehaviour
{
    public void OnNewButtonPressed()
    {
        SceneService.Instance.ChangeScene(SceneType.EventMapScene);
    }
    
    public void OnLoadButtonPressed()
    {
        SceneService.Instance.ChangeScene(SceneType.MapScene);
    }
}
