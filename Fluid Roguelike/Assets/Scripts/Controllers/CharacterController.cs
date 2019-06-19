
using System;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Dungeon;
using UnityEngine;

namespace Fluid.Roguelike
{
    public enum MoveDirection { None, N, E, S, W };

    public abstract class CharacterController
    {
        private Character.Character _character;

        public void Set(Character.Character character)
        {
            _character = character;
        }

        public abstract void Tick(Dungeon.Dungeon dungeon);

        protected MoveResult Move(Dungeon.Dungeon dungeon, MoveDirection dir)
        {
            if (_character == null)
                return MoveResult.None;

            var move = DirectionToVec(dir);

            // Check status effect modifications
            move = _character.Modify(move, out var consumedMoveModification);
            if (consumedMoveModification)
            {
                if (Mathf.Approximately(move.sqrMagnitude, 0))
                {
                    return MoveResult.NoMoveStatusEffect;
                }
            }

            // Check collision in direction that prevent move
            var targetKey = new Tuple<int, int>(
                (int) (_character.transform.position.x + move.x),
                (int) (_character.transform.position.y + move.y));

            if (dungeon.ValueMap.ContainsKey(targetKey) == false)
            {
                return MoveResult.None;
            }

            if (dungeon.ValueMap[targetKey].Index == DungeonTile.Index.Wall)
            {
                return MoveResult.None;
            }

            // TODO: Check collision in direction that should trigger interaction instead

            _character.Translate(move);
            return MoveResult.Moved;
        }

        public static Vector3 DirectionToVec(MoveDirection dir)
        {
            Vector3 move = Vector3.zero;
            switch (dir)
            {
                case MoveDirection.N:
                    move.y += 1;
                    break;
                case MoveDirection.E:
                    move.x += 1;
                    break;
                case MoveDirection.S:
                    move.y -= 1;
                    break;
                case MoveDirection.W:
                    move.x -= 1;
                    break;
            }

            return move;
        }
    }
}
