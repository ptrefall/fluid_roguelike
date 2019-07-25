namespace Fluid.Roguelike.Character.State
{
    public enum CharacterWorldState
    {
        HasConsumedTurn,
        HasBumpTarget,
        HasEnemyTarget,
        HasEnemyTargetInMeleeRange,
        HasEnemyTargetAtSpellCastRange,
        IsStunned,
        IsConfused,
        IsDrunk,
        IsDead,
        LastMoveResult,
        Health,
        Mana,
    }
}