
using System;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Dungeon;
using UnityEngine;

namespace Fluid.Roguelike
{
    public enum MoveDirection { None, N, E, S, W };

    public abstract class CharacterController : IBumpTarget
    {
        private Character.Character _character;

        public Tuple<int, int> Position => new Tuple<int, int>((int)_character.transform.position.x, (int)_character.transform.position.y);
        public Character.Character Character => _character;

        public void Set(Character.Character character)
        {
            _character = character;
        }

        public abstract void Tick(Dungeon.Dungeon dungeon);
        public void ConsumeTurn(Dungeon.Dungeon dungeon)
        {
            if (_character != null)
            {
                _character.TickTurn_Status();
            }
        }

        protected MoveResult Move(Dungeon.Dungeon dungeon, MoveDirection dir, bool isPlayer)
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
                return MoveResult.Collided;
            }

            if (dungeon.ValueMap[targetKey].Index == DungeonTile.Index.Wall)
            {
                return MoveResult.Collided;
            }

            // Check collision in direction that should trigger interaction instead
            var bumpTarget = dungeon.TryGetBumpTarget(targetKey, hitPlayer: !isPlayer);
            if (bumpTarget != null)
            {
                if (_character.Context.TrySetBumpTarget(bumpTarget))
                {
                    return MoveResult.Bump;
                }
                else
                {
                    return MoveResult.Collided;
                }
            }

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
