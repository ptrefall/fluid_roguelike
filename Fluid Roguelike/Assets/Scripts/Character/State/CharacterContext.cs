using System;
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using FluidHTN;
using FluidHTN.Contexts;
using Unity.Mathematics;

namespace Fluid.Roguelike.Character.State
{
    public partial class CharacterContext : BaseContext
    {
        public Character Self { get; }
        public Dungeon.Dungeon Dungeon { get; set; }
        public IBumpTarget CurrentBumpTarget { get; private set; }

        public Action<CharacterContext> OnKnownEnemiesUpdated { get; set; }

        public List<Character> KnownEnemies { get; set; } = new List<Character>();
        public List<Character> KnownFriends { get; set; } = new List<Character>();
        public List<Character> KnownNeutrals { get; set; } = new List<Character>();

        public Dictionary<int2, float> FieldOfView = new Dictionary<int2, float>();
        public List<int2> DiscoveredTiles = new List<int2>();

        public Character CurrentEnemyTarget { get; set; }

        public CharacterContext(Character self)
        {
            Self = self;

            base.Init();
        }

        public bool TrySetBumpTarget(IBumpTarget bumpTarget)
        {
            if (bumpTarget == null)
                return false;

            //TODO: Might want some rules as to "when" a bumpTarget is allowed to be replaced?
            CurrentBumpTarget = bumpTarget;
            SetState(CharacterWorldState.HasBumpTarget, true, EffectType.Permanent);
            return true;
        }
    }
}