using System;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Effects;
using FluidHTN;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Ability
{
    [CreateAssetMenu(fileName = "Spell Ability", menuName = "Content/Abilities/Spell")]
    public class SpellAbility : ScriptableObject, IAbility
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private int _radius = 1;
        [SerializeField] private int _range = 1;
        [SerializeField] private int _manaCost = 1;
        [SerializeField] private string _hitVerb = "magically touched";
        [SerializeField] private string _chargeVerb = "starts mumbling strange incantations";
        [SerializeField] private AbilityEffect _chargeEffectPrefab;
        [SerializeField] private AbilityEffect _hitEffectPrefab;
        [SerializeField] private AbilityEffect _hitCharacterEffectPrefab;

        public string Info => $"{_manaCost}mana, {_damage}dmg, {(_radius > 1 ? $"{_radius}aoe" : "")}, {_range}range";

        public bool CanUse(CharacterContext context)
        {
            if (context.Self.Mana < _manaCost)
            {
                return false;
            }

            if (_range <= 0 && context.HasState(CharacterWorldState.HasEnemyTargetInMeleeRange) == false)
            {
                return false;
            }

            return true;
        }

        public void Use(CharacterContext context)
        {
            Use(context, context.CurrentEnemyTarget);
        }

        public void Use(CharacterContext context, Character.Character target)
        {
            var dir = (target.Position - context.Self.Position);

            Debug.Log($"{context.Self.name} {_hitVerb} {target.name}!");
            target.Health -= _damage;

            if (_hitCharacterEffectPrefab != null)
            {
                var fx = GameObject.Instantiate(_hitCharacterEffectPrefab, new Vector3(target.Position.x, target.Position.y, 0),
                    Quaternion.identity);

                fx.Setup(dir);
            }
            else if (_hitEffectPrefab != null)
            {
                var fx = GameObject.Instantiate(_hitEffectPrefab, new Vector3(target.Position.x, target.Position.y, 0),
                    Quaternion.identity);

                fx.Setup(dir);
            }
        }

        public void Use(CharacterContext context, IBumpTarget target)
        {
            Debug.Log($"Hit {target}, but it had no effect!");
        }

        public void Use(CharacterContext context, int2 position)
        {
            var dir = (position - context.Self.Position);
            Debug.Log($"{context.Self.name} {_chargeVerb}.");
            ApplyUseCost(context, EffectType.Permanent);

            if (_chargeEffectPrefab != null)
            {
                var fx = GameObject.Instantiate(_chargeEffectPrefab, new Vector3(context.Self.Position.x, context.Self.Position.y, 0),
                    Quaternion.identity);

                fx.Setup(dir);
            }

            if (_radius <= 0)
            {
                var character = context.Dungeon.GetCharacterAt(position);
                if (character != null)
                {
                    Use(context, character);
                }
            }
            else
            {
                for (var y = position.y - _radius; y <= position.y + _radius; y++)
                {
                    for (var x = position.x - _radius; x <= position.x + _radius; x++)
                    {
                        var character = context.Dungeon.GetCharacterAt(new int2(x,y));
                        if (character != null)
                        {
                            Use(context, character);
                        }
                        else
                        {
                            if (_hitEffectPrefab != null)
                            {
                                var fx = GameObject.Instantiate(_hitEffectPrefab, new Vector3(x, y, 0),
                                    Quaternion.identity);

                                fx.Setup(dir);
                            }
                        }
                    }
                }
            }
        }

        // TODO: This could be a little smarter.
        // TODO: E.g. if damage is negative, it implies healing, which we'd want to apply to ourself.
        // TODO: E.g. if spell has AOE, ensure we pick a target that does not implicate ourself in the damage radius.
        public Character.Character FindDefaultTarget(CharacterContext context)
        {
            float sqClosest = float.MaxValue;
            Character.Character bestCharacter = null;

            foreach (var character in context.Dungeon.Characters)
            {
                if (context.FieldOfView.ContainsKey(character.Position))
                {
                    var sqDist = math.distancesq(character.Position, context.Self.Position);
                    if (sqDist < sqClosest)
                    {
                        sqClosest = sqDist;
                        bestCharacter = character;
                    }
                }
            }

            return bestCharacter;
        }

        public void ApplyUseCost(CharacterContext context, EffectType type)
        {
            if (context.ContextState == ContextState.Planning)
            {
                var currentMana = (int)context.GetState(CharacterWorldState.Mana);
                var newMana = currentMana - _manaCost;
                if (type == EffectType.Permanent)
                {
                    context.Self.Mana = newMana;
                }
                else
                {
                    context.SetState(CharacterWorldState.Mana, (byte)newMana, type);
                }
            }
            else
            {
                context.Self.Mana -= _manaCost;
            }
        }
    }
}