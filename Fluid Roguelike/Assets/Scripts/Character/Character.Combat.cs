
using System;
using System.Collections.Generic;
using Fluid.Roguelike.Ability;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Dungeon;
using FluidHTN;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character
    {
        public bool IsDead => Context.HasState(CharacterWorldState.IsDead);

        public Action<Character> OnDeath { get; set; }

        public bool GodMode { get; set; } = false;

        public bool Melee(Character target)
        {
            if (target.GodMode)
            {
                return true;
            }

            if (PrimaryWeapon != null)
            {
                if (PrimaryWeapon.TryUse(Context, target))
                {
                    return true;
                }
            }

            foreach (var meta in Meta.Abilities)
            {
                if (meta is IAbility ability)
                {
                    if (ability.CanUse(Context))
                    {
                        ability.Use(Context, target);
                    }
                }
            }

            return true;
        }

        public void Die()
        {
            Context.SetState(CharacterWorldState.IsDead, true, EffectType.Permanent);
            OnDeath?.Invoke(this);

            Reset_Sensors();
            Reset_Status();

            SpawnLoot();

            Context.Dungeon.Destroy(this);
        }
    }
}