using System;
using Fluid.Roguelike.Character.State;
using FluidHTN;

namespace Fluid.Roguelike.AI.Effects
{
    public class SetWorldStateEffect : IEffect
    {
        public string Name { get; }
        public EffectType Type { get; }
        public CharacterWorldState State { get; }
        public byte Value { get; }

        public SetWorldStateEffect(CharacterWorldState state, EffectType type)
        {
            Name = $"SetState({state})";
            Type = type;
            State = state;
            Value = 1;
        }

        public SetWorldStateEffect(CharacterWorldState state, bool value, EffectType type)
        {
            Name = $"SetState({state})";
            Type = type;
            State = state;
            Value = (byte)(value ? 1 : 0);
        }

        public SetWorldStateEffect(CharacterWorldState state, byte value, EffectType type)
        {
            Name = $"SetState({state})";
            Type = type;
            State = state;
            Value = value;
        }

        public void Apply(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                c.SetState(State, Value, Type);
                return;
            }

            throw new Exception("Unexpected context type!");
        }
    }
}