namespace PEEnum
{
    public enum ReactionTiming
    {
        Pre,
        Post,
    }

    // Reaction을 시전할 때, 시전자가 Reaction을 수행할 대상.
    // i.e. 임의의 Enemy가 Reaction을 등록한 상황이라면.
    // All : Enemy든 Player든 어떤 Action 모두가 Reaction Trigger의 대상.
    // Friendly : Enemy 기준, Enemy의 Friendly(Enemy)의 Action이 대상. 
    // Hostile : Enemy 기준, Enemy의 Hostile(Player)의 Action이 대상.
    public enum ReactionTarget
    {
        None,
        All,
        Caster,
        Friendly,
        Hostile,
    }

    public enum GAExecutor
    {
        Player,
        Enemy,
        Neutral,
    }
    
}