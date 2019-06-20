namespace Fluid.Roguelike.Character.State
{
    public enum CharacterWorldState
    {
        HasConsumedTurn,
        HasBumpTarget,
        HasEnemyTarget,
        HasEnemyTargetInMeleeRange,
        IsStunned,
        IsConfused,
        IsDrunk,
        LastMoveResult,
    }
}