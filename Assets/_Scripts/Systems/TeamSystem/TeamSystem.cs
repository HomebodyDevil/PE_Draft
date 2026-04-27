using System.Collections.Generic;
using PEEnum;
using UnityEngine;

public class TeamSystem : Singleton<TeamSystem>
{
    public List<Character> GetTeamCharactersByType(Character caster, PEEnum.TargetType targetType)
    {
        List<Character> targets = new();

        if (caster == null)
        {
            Debug.LogError("Caster is Null");
            return targets;
        }

        switch (targetType)
        {
            case TargetType.Caster:
                targets.Add(caster);
                break;
            case TargetType.Hostile:
                targets.AddRange(GetHostileTeamCharacters(caster.TeamType.Team));
                break;
            case TargetType.Friendly:
                targets.AddRange(GetFriendlyTeamCharacters(caster.TeamType.Team));
                break;
            case TargetType.All:
                targets.AddRange(GetAllCharacters());
                break;
        }

        return targets;
    }
    
    public List<Character> GetTeamCharacters(Team team)
    {
        List<Character> agents = new List<Character>();
        switch (team)
        {
            case Team.PlayerCharacter:
                agents.AddRange(PlayerSystem.Instance.PlayerCharacters);
                break;
            case Team.Enemy:
                agents.AddRange(EnemySystem.Instance.EnemyCharacters);
                break;
        }
        
        return agents;
    }
    
    public List<Character> GetHostileTeamCharacters(Team myTeam)
    {
        Team hostileTeam = myTeam switch
        {
            Team.PlayerCharacter => Team.Enemy,
            Team.Enemy  => Team.PlayerCharacter,
            _           => Team.None
        };

        return GetTeamCharacters(hostileTeam);
    }
    
    public List<Character> GetFriendlyTeamCharacters(Team myTeam)
    {
        return GetTeamCharacters(myTeam);
    }

    private List<Character> GetAllCharacters()
    {
        List<Character> characters = new();
        characters.AddRange(PlayerSystem.Instance.PlayerCharacters);
        characters.AddRange(EnemySystem.Instance.EnemyCharacters);

        return characters;
    }
}
