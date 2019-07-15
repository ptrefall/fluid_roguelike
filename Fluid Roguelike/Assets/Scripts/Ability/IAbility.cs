using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;

namespace Fluid.Roguelike.Ability
{
    public interface IAbility
    {
        bool CanUse(CharacterContext context);
        void Use(CharacterContext context);
        void Use(CharacterContext context, Character.Character target);
        void Use(CharacterContext context, IBumpTarget target);
    }
}