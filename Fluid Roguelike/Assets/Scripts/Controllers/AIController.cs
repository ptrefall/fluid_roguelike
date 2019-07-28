
using Fluid.Roguelike.AI;
using Fluid.Roguelike.Character.State;
using FluidHTN;
using FluidHTN.Debug;
using UnityEngine;

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

                if (Character.Context.LogDecomposition)
                {
                    UnityEngine.Debug.Log("---------------------- DECOMP LOG --------------------------");
                    while (Character.Context.DecompositionLog?.Count > 0)
                    {
                        var entry = Character.Context.DecompositionLog.Dequeue();
                        var depth = FluidHTN.Debug.Debug.DepthToString(entry.Depth);
                        //Console.ForegroundColor = entry.Color;
                        var color = ColorUtility.ToHtmlStringRGB(FromColor(entry.Color));
                        UnityEngine.Debug.Log($"<color={color}>{depth}{entry.Name}: {entry.Description}</color>");
                    }
                    //Console.ResetColor();
                    UnityEngine.Debug.Log("-------------------------------------------------------------");
                }
            }

            ConsumeTurn(dungeon);
        }

        public static Color FromColor(System.ConsoleColor c)
        {
            int cInt = (int)c;

            int brightnessCoefficient = ((cInt & 8) > 0) ? 2 : 1;
            int r = ((cInt & 4) > 0) ? 64 * brightnessCoefficient : 0;
            int g = ((cInt & 2) > 0) ? 64 * brightnessCoefficient : 0;
            int b = ((cInt & 1) > 0) ? 64 * brightnessCoefficient : 0;

            return new Color(r, g, b);
        }
    }
}