namespace Fluid.Roguelike.Interaction
{
    public interface IInteractible
    {
        void Setup(Item.Item item, IInteractibleMeta meta);
        bool TryInteract(Character.Character character);
        bool TryApply(string key, string value);
    }

    public interface IInteractibleMeta
    {
        IInteractible Create(Item.Item item);
    }
}