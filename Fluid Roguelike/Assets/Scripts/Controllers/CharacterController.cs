
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

        }
    }
}
