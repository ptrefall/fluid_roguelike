using System;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Item;
using FluidHTN;
using FluidHTN.Conditions;

namespace Fluid.Roguelike.AI.Conditions
{
    public class CanCastAnySpellCondition : ICondition
    {
        public string Name { get; }

        public CanCastAnySpellCondition()
        {
            Name = "CanCastAnySpellCondition";
        }

        public bool IsValid(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                if (!c.Dungeon.IsInFieldOfView(c.Self.Position))
                    return false;

                bool canUse = false;
                foreach (var item in c.Self.Inventory)
                {
                    if (item.Meta.Type != ItemType.Spell)
                        continue;

                    var tempMana = c.Self.Mana;
                    c.Self.Mana = (int)c.GetState(CharacterWorldState.Mana);
                    canUse = item.CanUse(c);
                    c.Self.Mana = tempMana;

                    if (canUse)
                        break;
                }

                if (ctx.LogDecomposition) ctx.Log(Name, $"CanCastAnySpellCondition.IsValid({canUse})", ctx.CurrentDecompositionDepth, this);
                return canUse;
            }

            throw new Exception("Unexpected context type!");
        }
    }
}