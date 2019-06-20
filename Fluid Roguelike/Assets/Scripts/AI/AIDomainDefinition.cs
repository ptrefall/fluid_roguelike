using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike.AI
{
    public abstract class AIDomainDefinition<T> : ScriptableObject
        where T : IContext
    {
        public abstract Domain<T> Create();
    }
}