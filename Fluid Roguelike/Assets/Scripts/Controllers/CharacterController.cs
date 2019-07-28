
using System.Runtime.Remoting.Messaging;
using Fluid.Roguelike.Actions;
using Unity.Mathematics;

namespace Fluid.Roguelike
{
    public enum MoveDirection { None, N, E, S, W };

    public abstract class CharacterController
    {
        private Character.Character _character;

        public int2 Position => _character.Position;
        public Character.Character Character => _character;

        public virtual void Set(Character.Character character)
        {
            _character = character;
        }

        public virtual void Unset(Character.Character character)
        {
            if (_character == character)
            {
                _character = null;
            }
        }

        public abstract void Tick(Dungeon.Dungeon dungeon);
        public void ConsumeTurn(Dungeon.Dungeon dungeon)
        {
            if (_character != null)
            {
                _character.TickTurn_Status();
                _character.Tick_StatRegen();
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

        public static MoveDirection VecToDirection(int2 dir)
        {
            if (UnityEngine.Mathf.Abs(dir.x) > UnityEngine.Mathf.Abs(dir.y))
            {
                if (dir.x > 0) return MoveDirection.E;
                else return MoveDirection.W;
            }
            else if (UnityEngine.Mathf.Abs(dir.x) < UnityEngine.Mathf.Abs(dir.y))
            {
                if (dir.y > 0) return MoveDirection.N;
                else return MoveDirection.S;
            }
            else if (math.lengthsq(dir) > 0)
            {
                if (UnityEngine.Random.value < 0.5f)
                {
                    if (dir.x > 0) return MoveDirection.E;
                    else return MoveDirection.W;
                }
                else
                {
                    if (dir.y > 0) return MoveDirection.N;
                    else return MoveDirection.S;
                }
            }

            return MoveDirection.None;
        }
    }
}
