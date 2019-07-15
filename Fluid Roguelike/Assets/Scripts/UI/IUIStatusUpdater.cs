namespace Fluid.Roguelike.UI
{
    public interface IUIStatusUpdater
    {
        void Setup(int turns);
        void Tick();
    }
}