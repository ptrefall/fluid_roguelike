
using UnityEngine;

namespace Fluid.Roguelike
{
    public enum MoveDirection { N, E, S, W };

    public abstract class CharacterController
    {
        private Transform _character;

        public void Set(Transform character)
        {
            _character = character;
        }

        public abstract void Tick();

        protected void Move(MoveDirection dir)
        {
            if (_character == null)
                return;

            Vector3 move = Vector3.zero;
            switch(dir)
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

            _character.Translate(move);
        }
    }
}
