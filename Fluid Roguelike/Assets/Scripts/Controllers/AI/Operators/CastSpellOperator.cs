using Fluid.Roguelike.Character.State;
using FluidHTN;
using FluidHTN.Operators;

namespace Fluid.Roguelike.AI.Operators
{
    public class CastSpellOperator : IOperator
    {
        public enum TargetType
        {
            Enemy,
        }

        private TargetType _type;

        public CastSpellOperator(TargetType type)
        {
            _type = type;
        }

        public TaskStatus Update(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                if (!c.Dungeon.IsInFieldOfView(c.Self.Position))
                    return TaskStatus.Failure;

                if (_type == TargetType.Enemy)
                {
                    if (c.CurrentSpell != null && c.CurrentEnemyTarget != null &&
                        c.Self.TryCastSpell(c.CurrentSpell, c.CurrentEnemyTarget.Position))
                    {
                        c.SetState(CharacterWorldState.HasConsumedTurn, true, EffectType.Permanent);
                        return TaskStatus.Success;
                    }
                }
            }

            return TaskStatus.Failure;
        }

        public void Stop(IContext ctx)
        {
        }
    }
}