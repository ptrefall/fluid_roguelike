using System;
using System.Collections.Generic;
using Fluid.Roguelike.Ability;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Character.Stats;
using Fluid.Roguelike.Dungeon;
using FluidHTN;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character
    {
        public bool CanCastSpell(Item.Item item)
        {
            return item.CanUse(Context);
        }

        public bool TryCastSpell(Item.Item item, int2 position)
        {
            return item.TryUse(Context, position);
        }

        public int2 FindDefaultTargetPosition(Item.Item item, int2 lastMoveDir)
        {
            return item.FindDefaultTarget(Context)?.Position ?? Position + lastMoveDir;
        }
    }
}