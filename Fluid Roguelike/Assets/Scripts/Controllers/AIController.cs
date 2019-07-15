
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
            if (Character == null || Character.IsDead)
                return;

            Character.TickTurn_Sensors();

            if (_brain != null)
            {
                int steps = 0;
                Character.Context.SetState(CharacterWorldState.HasConsumedTurn, false, EffectType.Permanent);
                while (Character.Context.HasState(CharacterWorldState.HasConsumedTurn) == false)
                {
                    steps++;
                    _brainHandler.Tick(_brain, Character.Context);
                    if (steps > 10) break;
                }
            }

            ConsumeTurn(dungeon);
        }
    }
}