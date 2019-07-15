
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Dungeon;
using FluidHTN;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character
    {
        public MoveResult Move(int2 dir, bool isPlayer)
        {
            if (IsDead)
                return MoveResult.None;

            var result = OnMove(dir, isPlayer);
            Context.SetState(CharacterWorldState.LastMoveResult, (int) result, EffectType.Permanent);
            return result;
        }

        private MoveResult OnMove(int2 dir, bool isPlayer)
        { 

        // Check status effect modifications
            dir = Modify(dir, out var consumedMoveModification);
            if (consumedMoveModification)
            {
                if (Mathf.Approximately(math.lengthsq(dir), 0))
                {
                    return MoveResult.NoMoveStatusEffect;
                }
            }

            // Check collision in direction that prevent move
            var targetPosition = Position + dir;

            if (Context.Dungeon.ValueMap.ContainsKey(targetPosition) == false)
            {
                return MoveResult.Collided;
            }

            if (Context.Dungeon.ValueMap[targetPosition].Index == DungeonTile.Index.Wall)
            {
                return MoveResult.Collided;
            }

            // Check collision in direction that should trigger interaction instead
            var bumpTarget = Context.Dungeon.TryGetBumpTarget(targetPosition, hitPlayer: !isPlayer);
            if (bumpTarget != null)
            {
                if (Context.TrySetBumpTarget(bumpTarget))
                {
                    if (bumpTarget is Character c)
                    {
                        Context.CurrentEnemyTarget = c; // TODO: Check alignment/friendliness
                        Context.SetState(CharacterWorldState.HasEnemyTargetInMeleeRange, true, EffectType.Permanent);
                    }

                    return MoveResult.Bump;
                }
                else
                {
                    return MoveResult.Collided;
                }
            }

            Translate(dir);
            return MoveResult.Moved;
        }
    }
}