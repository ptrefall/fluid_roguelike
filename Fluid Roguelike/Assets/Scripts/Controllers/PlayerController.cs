﻿
using System;
using Fluid.Roguelike.Actions;
using UnityEngine;

namespace Fluid.Roguelike
{
    public class PlayerController : CharacterController
    {
        private bool forceKeyDown = false;
        private float nextSameKeyTime = 0f;
        private KeyCode lastKey = KeyCode.Escape;

        private const float keyPause = 0.15f;

        public override void Tick(Dungeon.Dungeon dungeon)
        {
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

            if (result == MoveResult.Interaction)
            {
                // TODO: Do interaction in dir
                var move = DirectionToVec(dir);
                var pos = Position;
                var targetPosition = new Tuple<int, int>(pos.Item1 + (int) move.x, pos.Item2 + (int) move.y);
                var interactible = dungeon.TryGetInteractible(targetPosition, hitPlayer: false);
                if (interactible != null)
                {
                    Debug.Log("Get drunk!");
                    Character.AddPermanentStatus(CharacterStatusType.Drunk);
                    forceKeyDown = true;
                }
                else
                {
                    Debug.LogError("This should never happen");
                }
            }

            // Consume a turn and trigger AI
            ConsumeTurn(dungeon);
            dungeon.TickAI();
        }

        private MoveDirection CheckMoveInput()
        {
            if (forceKeyDown)
            {
                forceKeyDown = false;
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

            return dir;
        }
    }
}
