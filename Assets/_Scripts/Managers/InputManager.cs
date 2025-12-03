using UnityEngine;

public class InputManager : PersistantSingleton<InputManager>
{
    // Scene에 루트 오브젝트에 부착하여 사용.(PersistantSingleton으로)
    // 이 (Input)Action들을 사용할 객체들은 알아서 이 Instance의 PlayerActions에 콜백을 등록 / 해제 하여 사용토록 함.
    
    public PlayerActions PlayerActions { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        PlayerActions = new PlayerActions();

        PlayerActions.Enable();
    }
}
