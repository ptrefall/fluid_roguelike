﻿
using Fluid.Roguelike.AI;
using Fluid.Roguelike.Character.State;
using FluidHTN;

namespace Fluid.Roguelike
{
    public class AIController : CharacterController
    {
        private readonly Planner<CharacterContext> _brainHandler;
        private readonly Domain<CharacterContext> _brain;

        public AIController(CharacterDomainDefinition brain)
        {
            _brainHandler = new Planner<CharacterContext>();
            _brain = brain?.Create();
        }

        public override void Tick(Dungeon.Dungeon dungeon)
        {
            if (_brain != null)
            {
                _brainHandler.Tick(_brain, Character.Context);
            }

            ConsumeTurn(dungeon);
        }
    }
}