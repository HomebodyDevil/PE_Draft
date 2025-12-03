using UnityEngine;

public class ConfigService : PersistantSingleton<ConfigService>
{
    public GameConfig GameConfig { get; private set; } = new();

    protected override void Awake()
    {
        base.Awake();
        Setup();
    }

    private void Setup()
    {
        SetFrameRate(GameConfig.FrameRate);
    }

    public void SetFrameRate(int FrameRate)
    {
        Application.targetFrameRate = GameConfig.FrameRate = FrameRate;
    }

    public void SaveGameConfig()
    {
        
    }

    public void LoadGameConfig()
    {
        
    }
}