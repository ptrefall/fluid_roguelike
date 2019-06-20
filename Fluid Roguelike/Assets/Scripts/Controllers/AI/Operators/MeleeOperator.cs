using System.Diagnostics;
using FluidHTN;
using FluidHTN.Operators;

namespace Fluid.Roguelike.AI.Operators
{
    public class MeleeOperator : IOperator
    {
        public enum TargetType
        {
            Enemy,
        }

        private TargetType _type;

        public MeleeOperator(TargetType type)
        {
            _type = type;
        }

        public TaskStatus Update(IContext ctx)
        {
            UnityEngine.Debug.Log("Hit target!");
            return TaskStatus.Success;
        }

        public void Stop(IContext ctx)
        {
        }
    }
}