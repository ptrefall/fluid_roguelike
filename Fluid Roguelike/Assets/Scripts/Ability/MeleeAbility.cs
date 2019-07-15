using System;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using UnityEngine;

namespace Fluid.Roguelike.Ability
{
    [CreateAssetMenu(fileName = "Melee Ability", menuName = "Content/Abilities/Melee")]
    public class MeleeAbility : ScriptableObject, IAbility
    {
        [SerializeField] private int _damage;
        [SerializeField] private float _critChance = 0.1f;
        [SerializeField] private string _attackVerb = "punched";

        public string Info => $"{_damage}dmg";

        public bool CanUse(CharacterContext context)
        {
            return context.HasState(CharacterWorldState.HasEnemyTargetInMeleeRange);
        }

        public void Use(CharacterContext context)
        {
            Use(context, context.CurrentEnemyTarget);
        }

        public void Use(CharacterContext context, Character.Character target)
        {
            Debug.Log($"{context.Self.name} {_attackVerb} {target.name}!");

            var crit = UnityEngine.Random.value < _critChance;
            if (crit)
            {
                target.Health -= _damage * 2;
                target.AddTimedStatus(CharacterStatusType.Stunned, 2);
                Debug.Log($"{target.name} got stunned!");
            }
            else
            {
                target.Health -= _damage;
            }
        }

        public void Use(CharacterContext context, IBumpTarget target)
        {
            Debug.Log($"Hit {target}, but it had no effect!");
        }
    }
}