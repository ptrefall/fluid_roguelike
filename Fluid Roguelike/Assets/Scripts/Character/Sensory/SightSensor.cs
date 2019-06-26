using Fluid.Roguelike.Character.State;

namespace Fluid.Roguelike.Character.Sensory
{
    public class SightSensor : ICharacterSensor
    {
        public SensorTypes Type => SensorTypes.Sight;

        public void Tick(CharacterContext context)
        {
            context.KnownEnemies.Clear();

            // Ensure we're not blind
            if (context.Self.Sight == 0)
            {
                return;
            }

            var sight = context.Self.Sight;
            foreach (var character in context.Dungeon.Characters)
            {
                if (character == context.Self)
                    continue;

                context.KnownEnemies.Add(character);
            }
        }

        public void Reset(CharacterContext context)
        {
            context.KnownEnemies.Clear();
        }
    }
}