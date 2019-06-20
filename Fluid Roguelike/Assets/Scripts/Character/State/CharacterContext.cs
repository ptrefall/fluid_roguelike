using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using FluidHTN;
using FluidHTN.Contexts;

namespace Fluid.Roguelike.Character.State
{
    public partial class CharacterContext : BaseContext
    {
        public Character Self { get; }
        public Dungeon.Dungeon Dungeon { get; set; }
        public IBumpTarget CurrentBumpTarget { get; private set; }

        public List<Character> KnownEnemies = new List<Character>();
        public List<Character> KnownFriends = new List<Character>();
        public List<Character> KnownNeutrals = new List<Character>();

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