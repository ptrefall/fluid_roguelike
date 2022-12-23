using Fluid.Roguelike.Character.State;
using FluidHTN;
using FluidHTN.Operators;
using Unity.Mathematics;

namespace Fluid.Roguelike.AI.Operators
{
    public class FindEnemyTargetOperator : IOperator
    {
        public TaskStatus Update(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                var lastTarget = c.CurrentEnemyTarget;
                bool hasTarget = c.CurrentEnemyTarget != null;

                c.CurrentEnemyTarget = null;

                if (c.KnownEnemies.Count == 0)
                {
                    c.SetState(CharacterWorldState.HasEnemyTarget, false, EffectType.Permanent);
                    return TaskStatus.Failure;
                }

                var bestDistance = (float)c.Self.Sight;
                foreach (var enemy in c.KnownEnemies)
                {
                    if (enemy.IsDead)
                        continue;

                    if (hasTarget == false || lastTarget != enemy)
                    {
                        if (!c.Dungeon.IsInFieldOfView(enemy.Position))
                            continue;
                    }

                    var distance = math.length(enemy.Position - c.Self.Position);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        c.CurrentEnemyTarget = enemy;
                    }
                }

                c.SetState(CharacterWorldState.HasEnemyTarget, c.CurrentEnemyTarget != null, EffectType.Permanent);
                if (c.CurrentEnemyTarget != null && !hasTarget)
                {
                    UnityEngine.Debug.Log("Found enemy target!");
                }
                else if(c.CurrentEnemyTarget == null && hasTarget)
                {
                    UnityEngine.Debug.Log("Lost enemy target!");
                }

                return c.CurrentEnemyTarget != null ? TaskStatus.Success : TaskStatus.Failure;
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