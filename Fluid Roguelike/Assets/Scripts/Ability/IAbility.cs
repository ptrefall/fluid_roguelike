using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using FluidHTN;
using Unity.Mathematics;

namespace Fluid.Roguelike.Ability
{
    public interface IAbility
    {
        string Info { get; }
        bool CanUse(CharacterContext context);
        void Use(CharacterContext context);
        void Use(CharacterContext context, Character.Character target);
        void Use(CharacterContext context, IBumpTarget target);
        void Use(CharacterContext context, int2 position);
        void ApplyUseCost(CharacterContext context, EffectType type);

        Character.Character FindDefaultTarget(CharacterContext context);
    }
}