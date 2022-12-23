using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using FluidHTN;
using FluidHTN.Operators;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Fluid.Roguelike.AI.Operators
{
    public class MoveToOperator : IOperator
    {
        public enum TargetType
        {
            Enemy,
        }

        private TargetType _type;

        public MoveToOperator(TargetType type)
        {
            _type = type;
        }

        public TaskStatus Update(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                //TODO: Replace with A*
                //UnityEngine.Debug.Log("Move to target!");
                var dir = c.CurrentEnemyTarget.Position - c.Self.Position;
                if (UnityEngine.Mathf.Abs(dir.x) > UnityEngine.Mathf.Abs(dir.y))
                {
                    if (dir.x > 0) dir.x = 1; else dir.x = -1;
                    dir.y = 0;
                }
                else if(UnityEngine.Mathf.Abs(dir.x) < UnityEngine.Mathf.Abs(dir.y))
                {
                    dir.x = 0;
                    if (dir.y > 0) dir.y = 1; else dir.y = -1;
                }
                else if (math.lengthsq(dir) > 0)
                {
                    if (Random.value < 0.5f)
                    {
                        if (dir.x > 0) dir.x = 1; else dir.x = -1;
                        dir.y = 0;
                    }
                    else
                    {
                        dir.x = 0;
                        if (dir.y > 0) dir.y = 1; else dir.y = -1;
                    }
                }

                var result = c.Self.Move(dir, false);
                if (result == MoveResult.Moved)
                {
                    c.SetState(CharacterWorldState.HasConsumedTurn, true, EffectType.Permanent);
                    return TaskStatus.Success;
                }
                else
                {
                    return TaskStatus.Failure;
                }
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