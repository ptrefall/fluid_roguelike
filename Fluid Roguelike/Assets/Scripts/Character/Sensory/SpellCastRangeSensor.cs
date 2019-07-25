using Fluid.Roguelike.Character.State;
using Unity.Mathematics;

namespace Fluid.Roguelike.Character.Sensory
{
    public class SpellCastRangeSensor : ICharacterSensor
    {
        public SensorTypes Type => SensorTypes.SpeelCastRange;

        public void Tick(CharacterContext context)
        {
            if (context.CurrentEnemyTarget == null)
                return;

            var dir = (context.CurrentEnemyTarget.Position - context.Self.Position);
            context.SetState(
                CharacterWorldState.HasEnemyTargetAtSpellCastRange, 
                math.lengthsq(dir) >= 2, 
                FluidHTN.EffectType.Permanent);
        }

        public void Reset(CharacterContext context)
        {
            context.SetState(
                CharacterWorldState.HasEnemyTargetAtSpellCastRange,
                true,
                FluidHTN.EffectType.Permanent);
        }
    }
}