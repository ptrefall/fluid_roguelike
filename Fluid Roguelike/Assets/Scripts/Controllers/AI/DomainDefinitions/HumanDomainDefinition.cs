
using Fluid.Roguelike.Character.State;
using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike.AI
{
    [CreateAssetMenu(fileName = "Human Brain", menuName = "AI/Domains/Characters/Human")]
    public class HumanDomainDefinition : CharacterDomainDefinition
    {
        public override Domain<CharacterContext> Create()
        {
            return new CharacterDomainBuilder("Human")
                .Build();
        }
    }
}