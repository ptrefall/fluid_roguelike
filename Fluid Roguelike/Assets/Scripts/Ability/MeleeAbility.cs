using System;
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Effects;
using FluidHTN;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Ability
{
    [CreateAssetMenu(fileName = "Melee Ability", menuName = "Content/Abilities/Melee")]
    public class MeleeAbility : ScriptableObject, IAbility
    {
        [SerializeField] private int _damage;
        [SerializeField] private float _critChance = 0.1f;
        [SerializeField] private string _attackVerb = "punched";
        [SerializeField] private AbilityEffect _hitEffectPrefab;
        [SerializeField] private AbilityEffect _criticalHitEffectPrefab;
        [SerializeField] private AbilityEffect _missedEffectPrefab;

        public string Info => $"{_damage}dmg";

        public AbilityShape Shape => AbilityShape.Line;

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
            var dir = (target.Position - context.Self.Position);
            var targetDodgeChance = target.Dodge * 0.01f;
            if (UnityEngine.Random.value < targetDodgeChance)
            {
                Debug.Log($"{target.name} dodged {context.Self.name}'s attack!");
                if (_missedEffectPrefab != null)
                {
                    var fx = GameObject.Instantiate(_missedEffectPrefab, new Vector3(target.Position.x, target.Position.y, 0), Quaternion.identity);
                    fx.Setup(dir);
                }
                return;
            }

            Debug.Log($"{context.Self.name} {_attackVerb} {target.name}!");

            var crit = UnityEngine.Random.value < _critChance;
            if (crit)
            {
                target.Health -= _damage * 2;
                target.AddTimedStatus(CharacterStatusType.Stunned, 2);
                Debug.Log($"{target.name} got stunned!");
                if (_criticalHitEffectPrefab != null)
                {
                    var fx = GameObject.Instantiate(_criticalHitEffectPrefab,
                        new Vector3(target.Position.x, target.Position.y, 0), Quaternion.identity);

                    fx.Setup(dir);
                }
                else if (_hitEffectPrefab != null)
                {
                    var fx = GameObject.Instantiate(_hitEffectPrefab, new Vector3(target.Position.x, target.Position.y, 0),
                        Quaternion.identity);

                    fx.Setup(dir);
                }
            }
            else
            {
                target.Health -= _damage;
                if (_hitEffectPrefab != null)
                {
                    var fx = GameObject.Instantiate(_hitEffectPrefab, new Vector3(target.Position.x, target.Position.y, 0),
                        Quaternion.identity);

                    fx.Setup(dir);
                }
            }
        }

        public void Use(CharacterContext context, IBumpTarget target)
        {
            Debug.Log($"Hit {target}, but it had no effect!");
        }

        public void Use(CharacterContext context, int2 position)
        {
            Debug.Log($"Hits nothing but air!");
        }

        public Character.Character FindDefaultTarget(CharacterContext context)
        {
            return context.CurrentEnemyTarget;
        }

        public void ApplyUseCost(CharacterContext context, EffectType type)
        {

        }

        public int GetLocalImpactRadius(CharacterContext context)
        {
            return 0;
        }
    }
}