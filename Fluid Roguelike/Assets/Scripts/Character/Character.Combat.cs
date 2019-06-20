
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
        public bool Melee(Character target)
        {
            //TODO: Take stats and equipment into account.
            Debug.Log($"Hit {target.name}!");

            var crit = UnityEngine.Random.value < 0.1f;
            if (crit)
            {
                target.Health -= 4;
                target.AddTimedStatus(CharacterStatusType.Stunned, 2);
                Debug.Log($"{target.name} got stunned!");
            }
            else
            {
                target.Health -= 1;
            }

            return true;
        }
    }
}