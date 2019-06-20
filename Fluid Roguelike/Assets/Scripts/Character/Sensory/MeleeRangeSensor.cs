using Fluid.Roguelike.Character.State;
using Unity.Mathematics;

namespace Fluid.Roguelike.Character.Sensory
{
    public class MeleeRangeSensor : ICharacterSensor
    {
        public SensorTypes Type => SensorTypes.MeleeRange;

        public void Tick(CharacterContext context)
        {
            if (context.CurrentEnemyTarget == null)
                return;

            var dir = (context.CurrentEnemyTarget.Position - context.Self.Position);
            context.SetState(
                CharacterWorldState.HasEnemyTargetInMeleeRange, 
                math.lengthsq(dir) <= 1, 
                FluidHTN.EffectType.Permanent);
        }
    }
}