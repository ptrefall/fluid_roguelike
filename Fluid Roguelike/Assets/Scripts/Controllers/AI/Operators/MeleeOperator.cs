using System.Diagnostics;
using Fluid.Roguelike.Character.State;
using FluidHTN;
using FluidHTN.Operators;

namespace Fluid.Roguelike.AI.Operators
{
    public class MeleeOperator : IOperator
    {
        public enum TargetType
        {
            Enemy,
        }

        private TargetType _type;

        public MeleeOperator(TargetType type)
        {
            _type = type;
        }

        public TaskStatus Update(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                c.Self.Melee(c.CurrentEnemyTarget);

                c.SetState(CharacterWorldState.HasConsumedTurn, true, EffectType.Permanent);
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }

        public void Stop(IContext ctx)
        {
        }

        public void Aborted(IContext ctx)
        {
        }
    }
}