using System;
using Fluid.Roguelike.Character.State;
using FluidHTN;
using FluidHTN.Conditions;

namespace Fluid.Roguelike.AI.Conditions
{
    public class CanCastSpellCondition : ICondition
    {
        public string Name { get; }

        public CanCastSpellCondition()
        {
            Name = "CanCastSpellCondition";
        }

        public bool IsValid(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                bool result = false;
                if (c.CurrentSpell != null)
                {
                    var tempMana = c.Self.Mana;
                    c.Self.Mana = (int) c.GetState(CharacterWorldState.Mana);
                    result = c.CurrentSpell.CanUse(c);
                    c.Self.Mana = tempMana;
                }

                ctx.Log(Name, $"CanCastSpellCondition.IsValid({result})", ctx.CurrentDecompositionDepth, this);
                return result;
            }

            throw new Exception("Unexpected context type!");
        }
    }
}