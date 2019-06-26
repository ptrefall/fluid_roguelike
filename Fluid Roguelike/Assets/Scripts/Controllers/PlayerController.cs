
using Cinemachine;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike
{
    public class PlayerController : CharacterController
    {
        private bool forceKeyDown = false;
        private float nextSameKeyTime = 0f;
        private KeyCode lastKey = KeyCode.Escape;

        private const float keyPause = 0.15f;

        public override void Set(Character.Character character)
        {
            base.Set(character);

            var cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (cameraBrain != null && cameraBrain.ActiveVirtualCamera != null)
            {
                cameraBrain.ActiveVirtualCamera.LookAt = character.transform;
                cameraBrain.ActiveVirtualCamera.Follow = character.transform;
            }
        }

        public override void Tick(Dungeon.Dungeon dungeon)
        {
            if (Character.IsDead)
                return;

            // Cheats
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Character.AddTimedStatus(CharacterStatusType.Stunned, 2);
                Debug.Log($"{Character.name} got stunned!");
                return;
            }

            // Logic

            MoveResult result = MoveResult.None;
            MoveDirection dir = CheckMoveInput();

            if (dir != MoveDirection.None)
            {
                result = Move(dungeon, dir, true);
            }

            // We did not consume a turn
            if (result == MoveResult.None)
            {
                return;
            }

            if (result == MoveResult.Collided)
            {
                forceKeyDown = true;
                return;
            }

            if (result == MoveResult.Bump)
            {
                if (Character.Context.CurrentBumpTarget != null)
                {
                    if (Character.Context.CurrentBumpTarget is Character.Character c)
                    {
                        Character.Melee(c);
                    }
                    forceKeyDown = true;
                }
                else
                {
                    Debug.LogError("This should never happen");
                }
            }

            // We tick the AI first, so that we're not updating status effects until NPCs have had the chance to add new ones.
            dungeon.TickAI();

            ConsumeTurn(dungeon);
            Character.TickTurn_Sensors();

            UpdateVisibility(dungeon);
        }

        public void UpdateVisibility(Dungeon.Dungeon dungeon)
        {
            if (Character != null)
            {
                foreach (var tile in dungeon.Tiles)
                {
                    if (Character.Context.FieldOfView.ContainsKey(tile.Key))
                    {
                        tile.Value.Visibility(true);
                    }
                    else
                    {
                        tile.Value.Visibility(false);
                    }
                }

                foreach (var character in dungeon.Characters)
                {
                    if (character == Character)
                        continue;

                    if (Character.Context.FieldOfView.ContainsKey(character.Position))
                    {
                        character.Visibility(true);
                    }
                    else
                    {
                        character.Visibility(false);
                    }
                }
            }
        }

        private MoveDirection CheckMoveInput()
        {
            if (forceKeyDown)
            {
                return CheckMoveInputKeyDown();
            }

            var dir = MoveDirection.None;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                if (lastKey == KeyCode.W)
                {
                    if (Time.time < nextSameKeyTime)
                        return MoveDirection.None;
                }

                lastKey = KeyCode.W;
                nextSameKeyTime = Time.time + keyPause;
                dir = MoveDirection.N;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                if (lastKey == KeyCode.D)
                {
                    if (Time.time < nextSameKeyTime)
                        return MoveDirection.None;
                }

                lastKey = KeyCode.D;
                nextSameKeyTime = Time.time + keyPause;
                dir = MoveDirection.E;
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                if (lastKey == KeyCode.S)
                {
                    if (Time.time < nextSameKeyTime)
                        return MoveDirection.None;
                }

                lastKey = KeyCode.S;
                nextSameKeyTime = Time.time + keyPause;
                dir = MoveDirection.S;
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                if (lastKey == KeyCode.A)
                {
                    if (Time.time < nextSameKeyTime)
                        return MoveDirection.None;
                }

                lastKey = KeyCode.A;
                nextSameKeyTime = Time.time + keyPause;
                dir = MoveDirection.W;
            }

            return dir;
        }

        private MoveDirection CheckMoveInputKeyDown()
        {
            var dir = MoveDirection.None;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                lastKey = KeyCode.W;
                nextSameKeyTime = Time.time + keyPause;
                forceKeyDown = false;
                dir = MoveDirection.N;
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                lastKey = KeyCode.D;
                nextSameKeyTime = Time.time + keyPause;
                forceKeyDown = false;
                dir = MoveDirection.E;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                lastKey = KeyCode.S;
                nextSameKeyTime = Time.time + keyPause;
                forceKeyDown = false;
                dir = MoveDirection.S;
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                lastKey = KeyCode.A;
                nextSameKeyTime = Time.time + keyPause;
                forceKeyDown = false;
                dir = MoveDirection.W;
            }

            return dir;
        }
    }
}
