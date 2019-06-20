
using Fluid.Roguelike.Character.State;
using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike.AI
{
    [CreateAssetMenu(fileName = "Kobold Brain", menuName = "AI/Domains/Characters/Kobold")]
    public class KoboldDomainDefinition : CharacterDomainDefinition
    {
        public override Domain<CharacterContext> Create()
        {
            return new CharacterDomainBuilder("Kobold")
                .EngageEnemyMeleeSequence()
                .Build();
        }
    }
}