
using Fluid.Roguelike.Character.State;
using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike.AI
{
    [CreateAssetMenu(fileName = "Flee Brain", menuName = "AI/Domains/Characters/Flee")]
    public class FleeDomainDefinition : CharacterDomainDefinition
    {
        [SerializeField] private string _brainName;

        public override Domain<CharacterContext> Create()
        {
            return new CharacterDomainBuilder(_brainName)
                .FleeOrEngageSequence()
                .Build();
        }
    }
}