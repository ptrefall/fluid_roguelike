
using System.Collections.Generic;
using FluidHTN;
using FluidHTN.Compounds;
using FluidHTN.PrimitiveTasks;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public partial class DungeonAgent : MonoBehaviour
    {
        [SerializeField] private DungeonDomainDefinition _domainDefinition;

        private DungeonContext _context;
        private Domain<DungeonContext> _domain;

        private void Start()
        {
            _context = new DungeonContext();
            _context.Init();

            _domain = _domainDefinition.Create();
            Generate(0);
        }

        public bool Generate(int dungeonDepth)
        {
            _context.SetState(DungeonWorldState.DungeonDepth, dungeonDepth, EffectType.Permanent);

            var status = _domain.FindPlan(_context, out var plan);
            if(status == DecompositionStatus.Succeeded)
            {
                return ProcessPlan(plan);
            }

            return false;
        }

        private bool ProcessPlan(Queue<ITask> plan)
        {
            while (plan.Count > 0)
            {
                var task = plan.Dequeue();
                if (task is IPrimitiveTask action)
                {
                    while (action.Operator?.Update(_context) == TaskStatus.Continue)
                    {

                    }
                }
            }
            return true;
        }
    }
}