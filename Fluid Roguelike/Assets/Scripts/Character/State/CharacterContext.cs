using Fluid.Roguelike.Actions;
using FluidHTN;
using FluidHTN.Contexts;

namespace Fluid.Roguelike.Character.State
{
    public partial class CharacterContext : BaseContext
    {
        public IBumpTarget CurrentBumpTarget { get; private set; }

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