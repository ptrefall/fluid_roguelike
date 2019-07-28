using System;
using Fluid.Roguelike.Ability;
using Fluid.Roguelike.Character.State;
using FluidHTN;

namespace Fluid.Roguelike.AI.Effects
{
    public class CastSpellEffect : IEffect
    {
        public string Name { get; }
        public EffectType Type { get; }

        public CastSpellEffect(EffectType type)
        {
            Name = "CastSpellEffect";
            Type = type;
        }

        public void Apply(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                if (c.CurrentSpell != null)
                {
                    c.CurrentSpell.ApplyUseCost(c, Type);
                    if (ctx.LogDecomposition) ctx.Log(Name, $"CastSpellEffect.Apply({Type})", ctx.CurrentDecompositionDepth, this);
                }

                return;
            }

            throw new Exception("Unexpected context type!");
        }
    }
}