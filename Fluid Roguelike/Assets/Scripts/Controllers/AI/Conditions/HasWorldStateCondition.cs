﻿using System;
using Fluid.Roguelike.Character.State;
using FluidHTN;
using FluidHTN.Conditions;

namespace Fluid.Roguelike.AI.Conditions
{
    public class HasWorldStateCondition : ICondition
    {
        public string Name { get; }
        public CharacterWorldState State { get; }
        public byte Value { get; }

        public HasWorldStateCondition(CharacterWorldState state)
        {
            Name = $"HasState({state})";
            State = state;
            Value = 1;
        }

        public HasWorldStateCondition(CharacterWorldState state, byte value)
        {
            Name = $"HasState({state})";
            State = state;
            Value = value;
        }

        public bool IsValid(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                return c.HasState(State, Value);
            }

            throw new Exception("Unexpected context type!");
        }
    }
}