using UnityEngine;

public class GameConfig
{
    public int FrameRate { get; set; } = 60;

    public GameConfig(
        int frameRate = 60)
    {
        FrameRate = frameRate;
    }
}
