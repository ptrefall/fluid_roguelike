using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    [CreateAssetMenu(fileName = "Dungeon Domain", menuName = "AI/Domains/Dungeon")]
    public class DungeonDomainDefinition : AIDomainDefinition<DungeonContext>
    {
        [SerializeField] private string _dungeonName;

        public override Domain<DungeonContext> Create()
        {
            return new DungeonDomainBuilder(_dungeonName)
                .Sequence("First level")
                    .IsAtDepth(0)
                    .CreateTheCave()
                    .CreateTheForest()
                .End()
                .Build();
        }
    }
}