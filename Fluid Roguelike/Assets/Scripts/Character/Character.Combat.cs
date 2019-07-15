
using System.Collections.Generic;
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

        public bool Melee(Character target)
        {
            if (PrimaryWeapon != null)
            {
                if (PrimaryWeapon.TryUse(Context, target))
                {
                    return true;
                }
            }

            //TODO: This should be a punch ability on the character (some characters might bite or claw instead).
            Debug.Log($"{Context.Self.name} punched {target.name}!");

            target.Health -= 1;

            return true;
        }

        public void Die()
        {
            Context.SetState(CharacterWorldState.IsDead, true, EffectType.Permanent);
            Reset_Sensors();
            Reset_Status();

            if (Meta != null)
            {
                View.sprite = Meta.DeathSprite;
                View.color = Meta.DeathColor;
            }
        }
    }
}