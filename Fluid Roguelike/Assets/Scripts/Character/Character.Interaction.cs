﻿
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Database;
using Fluid.Roguelike.Dungeon;
using Fluid.Roguelike.Interaction;
using Fluid.Roguelike.Item;
using FluidHTN;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character
    {
        public bool TryInteract(Item.Item item)
        {
            return item.TryInteract(this);
        }

        public bool AddSpecial(string special)
        {
            var split = special.Split(':');
            if (split.Length < 2 || split.Length > 2)
            {
                Debug.LogError($"Error in syntax reading special {special} for item {Meta.Name}!");
                return false;
            }

            var key = split[0];
            var value = split[1];

            if (key.ToLower() == "loot")
            {
                AddSpecialLootDrop(value);
                return true;
            }

            return false;
        }
    }
}