using System;
using Fluid.Roguelike.Character.State;
using FluidHTN;

namespace Fluid.Roguelike.AI.Effects
{
    public class IncrementWorldStateEffect : IEffect
    {
        public string Name { get; }
        public EffectType Type { get; }
        public CharacterWorldState State { get; }
        public byte Value { get; }

        public IncrementWorldStateEffect(CharacterWorldState state, EffectType type)
        {
            Name = $"IncrementState({state})";
            Type = type;
            State = state;
            Value = 1;
        }

        public IncrementWorldStateEffect(CharacterWorldState state, byte value, EffectType type)
        {
            Name = $"IncrementState({state})";
            Type = type;
            State = state;
            Value = value;
        }

        public void Apply(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                var currentValue = c.GetState(State);
                c.SetState(State, (byte)(currentValue + Value), Type);
                if (ctx.LogDecomposition) ctx.Log(Name, $"IncrementWorldStateEffect.Apply({State}:{currentValue}+{Value}:{Type})", ctx.CurrentDecompositionDepth + 1, this);
                return;
            }

            throw new Exception("Unexpected context type!");
        }
    }
}