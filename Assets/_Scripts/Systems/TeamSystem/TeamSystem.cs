using System.Collections.Generic;
using UnityEngine;

public class TeamSystem : Singleton<TeamSystem>
{
    public List<Character> GetTeamAgents(Team team)
    {
        List<Character> agents = new List<Character>();
        switch (team)
        {
            case Team.Player:
                agents.AddRange(PlayerSystem.Instance.PlayerCharacters);
                break;
            case Team.Enemy:
                agents.AddRange(EnemySystem.Instance.EnemyCharacters);
                break;
        }
        
        return agents;
    }
    
    public List<Character> GetHostileTeamAgents(Team myTeam)
    {
        Team hostileTeam = myTeam switch
        {
            Team.Player => Team.Enemy,
            Team.Enemy  => Team.Player,
            _           => Team.None
        };

        return GetTeamAgents(hostileTeam);
    }
    
    public List<Character> GetFriendlyTeamAgents(Team myTeam)
    {
        return GetTeamAgents(myTeam);
    }
}
