using Fluid.Roguelike.Character.State;
using FluidHTN;
using FluidHTN.Factory;

namespace Fluid.Roguelike.AI
{
    public class CharacterDomainBuilder : BaseDomainBuilder<CharacterDomainBuilder, CharacterContext>
    {
        public CharacterDomainBuilder(string domainName) : base(domainName, new DefaultFactory())
        {
        }
    }
}