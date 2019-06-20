
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character
    {
        private readonly List<Status> _permanentStatuses = new List<Status>();
        private readonly List<Status> _timedStatuses = new List<Status>();

        private Status Create(CharacterStatusType statusType)
        {
            Status status = null;
            switch (statusType)
            {
                case CharacterStatusType.Stunned:
                    status = new StunnedStatus();
                    break;
                case CharacterStatusType.Confused:
                    status = new ConfusedStatus();
                    break;
                case CharacterStatusType.Drunk:
                    status = new DrunkStatus();
                    break;
            }

            return status;
        }

        public bool HasStatus(CharacterStatusType statusType)
        {
            foreach (var s in _permanentStatuses)
            {
                if (s.Type == statusType)
                {
                    return true;
                }
            }

            foreach (var s in _timedStatuses)
            {
                if (s.Type == statusType)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddPermanentStatus(CharacterStatusType statusType)
        {
            foreach (var s in _permanentStatuses)
            {
                if (s.Type == statusType)
                {
                    return;
                }
            }

            var status = Create(statusType);
            if (status != null)
            {
                _permanentStatuses.Add(status);
                UpdateWorldState(statusType, true);
            }
        }

        public void RemovePermanentStatus(CharacterStatusType statusType)
        {
            foreach (var s in _permanentStatuses)
            {
                if (s.Type == statusType)
                {
                    _permanentStatuses.Remove(s);
                    UpdateWorldState(statusType, false);
                    return;
                }
            }
        }

        public void AddTimedStatus(CharacterStatusType statusType, int numTurns)
        {
            foreach (var s in _timedStatuses)
            {
                if (s.Type == statusType)
                {
                    if (s.Life < numTurns)
                        s.Life = numTurns;

                    return;
                }
            }

            var status = Create(statusType);
            if (status != null)
            {
                status.Life = numTurns;
                _timedStatuses.Add(status);
                UpdateWorldState(statusType, true);
            }
        }

        public void RemoveTimedStatus(CharacterStatusType statusType)
        {
            foreach (var s in _timedStatuses)
            {
                if (s.Type == statusType)
                {
                    _timedStatuses.Remove(s);
                    UpdateWorldState(statusType, false);
                    return;
                }
            }
        }

        public void TickTurn_Status()
        {
            for(var i = 0; i < _timedStatuses.Count; i++)
            {
                var s = _timedStatuses[i];
                s.Life--;
                if (s.Life <= 0)
                {
                    _timedStatuses.RemoveAt(i);
                    i--;

                    UpdateWorldState(s.Type, false);
                }
            }
        }

        public Vector3 Modify(Vector3 move, out bool consumed)
        {
            foreach (var s in _permanentStatuses)
            {
                move = s.Modify(move, out consumed);
                if (consumed) return move;
            }

            foreach (var s in _timedStatuses)
            {
                move = s.Modify(move, out consumed);
                if (consumed) return move;
            }

            consumed = false;
            return move;
        }

        private void UpdateWorldState(CharacterStatusType statusType, bool added)
        {
            switch (statusType)
            {
                case CharacterStatusType.Stunned:
                    Context.SetState(CharacterWorldState.IsStunned, added, EffectType.Permanent);
                    break;
                case CharacterStatusType.Confused:
                    Context.SetState(CharacterWorldState.IsConfused, added, EffectType.Permanent);
                    break;
                case CharacterStatusType.Drunk:
                    Context.SetState(CharacterWorldState.IsDrunk, added, EffectType.Permanent);
                    break;
            }
        }
    }
}