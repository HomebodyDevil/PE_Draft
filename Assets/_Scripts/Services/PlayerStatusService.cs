using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

public class PlayerStatusService : PersistantSingleton<PlayerStatusService>
{
    [field: SerializeField] public PlayerStatus PlayerStatus { get; private set; } = null;
    [SerializeField] private PlayerStatusData defaultPayerStatusData;

    protected override void Awake()
    {
        base.Awake();

        // PlayerStatus를 세팅.
        // 차후 저장된 데이터가 있다면, 그 데이터를 쓰도록 하자.
        if (defaultPayerStatusData == null)
        {
            SetupDefaultPlayerStatusData();
        }
        else
        {
            PlayerStatus = new(defaultPayerStatusData);
        }
    }

    private void SetupDefaultPlayerStatusData()
    {
        PlayerStatus = new PlayerStatus();
        PlayerStatus.MaxHealth = ConstValue.DEFAULT_PLAYER_HEALTH;
        PlayerStatus.CurrentHealth = ConstValue.DEFAULT_PLAYER_HEALTH;
        PlayerStatus.MaxCost = ConstValue.DEFAULT_PLAYER_COST;
    }

    public List<Character> GetPlayerCharacters()
    {
        return PlayerStatus.PlayerCharacters;
    }
}
