using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Item;
using FluidHTN;
using FluidHTN.Operators;

namespace Fluid.Roguelike.AI.Operators
{
    public class FindSpellToCastOperator : IOperator
    {
        // TODO: Let us find the best spell for the situation, not just the first valid spell.
        public TaskStatus Update(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                foreach (var item in c.Self.Inventory)
                {
                    if (item.Meta.Type != ItemType.Spell)
                    {
                        continue;
                    }

                    // We need to temporarily apply the mana stored in context, that we can support effects
                    // changing the mana during planning.
                    var tempMana = c.Self.Mana;
                    c.Self.Mana = (int)c.GetState(CharacterWorldState.Mana);
                    var canCastSpell = c.Self.CanCastSpell(item);
                    c.Self.Mana = tempMana;
                    if (canCastSpell == false)
                        continue;

                    c.CurrentSpell = item;
                    return TaskStatus.Success;
                }
            }

            return TaskStatus.Failure;
        }

        public void Stop(IContext ctx)
        {
            
        }
    }
}