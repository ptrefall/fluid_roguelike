using System;
using System.Collections.Generic;
using FluidHTN;
using FluidHTN.Factory;

namespace Fluid.Roguelike.Character.State
{
    public partial class CharacterContext
    {
        public override IFactory Factory { get; set; } = new DefaultFactory();
        public override List<string> MTRDebug { get; set; } = new List<string>();
        public override List<string> LastMTRDebug { get; set; } = new List<string>();
        public override bool DebugMTR { get; } = true;
        public override Queue<FluidHTN.Debug.IBaseDecompositionLogEntry> DecompositionLog { get; set; }
        public override bool LogDecomposition => LogDecompositionBool;
        public bool LogDecompositionBool { get; set; } = false;

        public override byte[] WorldState { get; } = new byte[Enum.GetValues(typeof(CharacterWorldState)).Length];

        public bool HasState(CharacterWorldState state, bool value)
        {
            return HasState((int)state, (byte)(value ? 1 : 0));
        }

        public bool HasState(CharacterWorldState state, byte value)
        {
            return HasState((int)state, value);
        }

        public bool HasState(CharacterWorldState state)
        {
            return HasState((int)state, 1);
        }

        public void SetState(CharacterWorldState state, bool value, EffectType type)
        {
            SetState((int)state, (byte)(value ? 1 : 0), true, type);
        }

        public void SetState(CharacterWorldState state, byte value, EffectType type)
        {
            var oldValue = WorldState[(int) state];
            SetState((int)state, value, true, type);

            // We do some special handling here that we can support changing certain stats through planner effects.
            if (ContextState == ContextState.Executing && Self != null && oldValue != value)
            {
                switch (state)
                {
                    case CharacterWorldState.Mana:
                        Self.Mana = value;
                        break;
                }
            }
        }

        public void SetState(CharacterWorldState state, int value, EffectType type)
        {
            SetState((int)state, (byte)value, true, type);
        }

        public byte GetState(CharacterWorldState state)
        {
            return GetState((int)state);
        }
    }
}