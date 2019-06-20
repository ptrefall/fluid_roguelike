
using System;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Dungeon;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike
{
    public enum MoveDirection { None, N, E, S, W };

    public abstract class CharacterController
    {
        private Character.Character _character;

        public int2 Position => _character.Position;
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
            return Character.Move(move, isPlayer);
        }

        public static int2 DirectionToVec(MoveDirection dir)
        {
            var move = int2.zero;
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
