namespace Fluid.Roguelike
{
    public class AIController : CharacterController
    {
        public override void Tick(Dungeon.Dungeon dungeon)
        {
            ConsumeTurn(dungeon);
        }
    }
}