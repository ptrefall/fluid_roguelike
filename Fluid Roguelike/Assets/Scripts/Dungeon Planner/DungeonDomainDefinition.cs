﻿using Fluid.Roguelike.Database;
using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    [CreateAssetMenu(fileName = "Dungeon Domain", menuName = "AI/Domains/Dungeon")]
    public class DungeonDomainDefinition : AIDomainDefinition<DungeonContext>
    {
        [SerializeField] private string _dungeonName;
        [SerializeField] private DecorationDatabaseManager _decorations;

        public override Domain<DungeonContext> Create()
        {
            return new DungeonDomainBuilder(_dungeonName, _decorations)
                .Sequence("First level")
                    .IsAtDepth(0)
                    .CreateTheForest()
                    .CreateTheCave()
                .End()
                .Build();
        }
    }
}