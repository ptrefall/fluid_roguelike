using System;
using Fluid.Roguelike.Actions;
using UnityEngine;
using Random = System.Random;

namespace Fluid.Roguelike.Character
{
    public abstract class Status
    {
        public abstract CharacterStatusType Type { get; }
        public int Life { get; set; } = -1;

        public virtual Vector3 Modify(Vector3 move, out bool consume)
        {
            consume = false;
            return move;
        }
    }

    public class StunnedStatus : Status
    {
        public override CharacterStatusType Type => CharacterStatusType.Stunned;

        public override Vector3 Modify(Vector3 move, out bool consume)
        {
            consume = true;
            return Vector3.zero;
        }
    }

    public class ConfusedStatus : Status
    {
        public override CharacterStatusType Type => CharacterStatusType.Confused;

        public override Vector3 Modify(Vector3 move, out bool consume)
        {
            consume = false;
            var dir = (MoveDirection)UnityEngine.Random.Range((int)MoveDirection.N, (int)MoveDirection.W + 1);
            return CharacterController.DirectionToVec(dir);
        }
    }

    public class DrunkStatus : Status
    {
        public override CharacterStatusType Type => CharacterStatusType.Drunk;

        public override Vector3 Modify(Vector3 move, out bool consume)
        {
            consume = false;
            var dir = (MoveDirection) UnityEngine.Random.Range((int) MoveDirection.N, (int) MoveDirection.W + 1);
            return CharacterController.DirectionToVec(dir);
        }
    }
}