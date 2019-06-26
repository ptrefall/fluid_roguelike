
using System;
using System.Collections.Generic;
using FluidHTN;
using FluidHTN.Contexts;
using FluidHTN.Factory;

namespace Fluid.Roguelike.Dungeon
{
    public partial class DungeonContext : BaseContext
    {
        public override IFactory Factory { get; set; } = new DefaultFactory();
        public override List<string> MTRDebug { get; set; } = new List<string>();
        public override List<string> LastMTRDebug { get; set; } = new List<string>();
        public override bool DebugMTR { get; } = true;
        public override Queue<FluidHTN.Debug.IBaseDecompositionLogEntry> DecompositionLog { get; set; }
        public override bool LogDecomposition { get; } = true;

        public override byte[] WorldState { get; } = new byte[Enum.GetValues(typeof(DungeonWorldState)).Length];

        public bool HasState(DungeonWorldState state, bool value)
        {
            return HasState((int) state, (byte) (value ? 1 : 0));
        }

        public bool HasState(DungeonWorldState state, byte value)
        {
            return HasState((int)state, value);
        }

        public bool HasState(DungeonWorldState state)
        {
            return HasState((int) state, 1);
        }

        public void SetState(DungeonWorldState state, bool value, EffectType type)
        {
            SetState((int) state, (byte) (value ? 1 : 0), true, type);
        }

        public void SetState(DungeonWorldState state, byte value, EffectType type)
        {
            SetState((int)state, value, true, type);
        }

        public void SetState(DungeonWorldState state, int value, EffectType type)
        {
            SetState((int)state, (byte)value, true, type);
        }

        public byte GetState(DungeonWorldState state)
        {
            return GetState((int) state);
        }
    }
}