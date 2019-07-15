using Fluid.Roguelike.Character.State;
using Unity.Mathematics;

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
                context.OnKnownEnemiesUpdated?.Invoke(context);
                return;
            }

            var sight = context.Self.Sight;
            foreach (var character in context.Dungeon.Characters)
            {
                if (character == context.Self)
                    continue;

                if (character.IsDead)
                    continue;

                var dir = (character.Position - context.Self.Position);
                var distSq = math.lengthsq(dir);
                if (distSq <= sight * sight)
                {
                    context.KnownEnemies.Add(character);
                }
            }

            context.OnKnownEnemiesUpdated?.Invoke(context);
        }

        public void Reset(CharacterContext context)
        {
            context.KnownEnemies.Clear();
            context.OnKnownEnemiesUpdated?.Invoke(context);
        }
    }
}