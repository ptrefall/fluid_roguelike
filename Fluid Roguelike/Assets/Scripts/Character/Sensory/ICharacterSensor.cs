using Fluid.Roguelike.Character.State;

namespace Fluid.Roguelike.Character.Sensory
{
    public interface ICharacterSensor
    {
        SensorTypes Type { get; }

        void Tick(CharacterContext context);
        void Reset(CharacterContext context);
    }
}