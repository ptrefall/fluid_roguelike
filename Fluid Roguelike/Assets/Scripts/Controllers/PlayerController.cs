
using Fluid.Roguelike.Actions;
using UnityEngine;

namespace Fluid.Roguelike
{
    public class PlayerController : CharacterController
    {
        public override void Tick(Dungeon.Dungeon dungeon)
        {
            MoveResult result = MoveResult.None;
            MoveDirection dir = MoveDirection.None;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                dir = MoveDirection.N;
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                dir = MoveDirection.E;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                dir = MoveDirection.S;
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                dir = MoveDirection.W;
            }

            if (dir != MoveDirection.None)
            {
                result = Move(dungeon, dir);
            }

            // We did not consume a turn
            if (result == MoveResult.None)
            {
                return;
            }

            if (result == MoveResult.Interaction)
            {
                // TODO: Do interaction in dir
            }

            // TODO: Consume a turn and trigger AI
        }
    }
}
