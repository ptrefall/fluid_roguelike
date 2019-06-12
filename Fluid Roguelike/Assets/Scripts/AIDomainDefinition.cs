using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike
{
    public abstract class AIDomainDefinition<T> : ScriptableObject
        where T : IContext
    {
        public abstract Domain<T> Create();
    }
}