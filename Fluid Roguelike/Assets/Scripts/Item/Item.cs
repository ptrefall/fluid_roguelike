using Fluid.Roguelike.Ability;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Database;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Item
{
    public enum ItemType { Weapon, Equipment, Food, Scraps, Other }
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

    public class Item
    {
        private Dungeon.Dungeon _dungeon;
        private ItemDbEntry _meta;
        private SpriteRenderer _worldView;

        public ItemDbEntry Meta => _meta;
        public SpriteRenderer WorldView => _worldView;

        public int2 WorldPosition
        {
            get
            {
                return _worldView != null
                    ? new int2((int) _worldView.transform.position.x, (int) _worldView.transform.position.y)
                    : int2.zero;
            }
            set
            {
                if (_worldView != null)
                {
                    _worldView.transform.position = new Vector3(value.x, value.y, 0);
                }
            }
        }

        public void Setup(Dungeon.Dungeon dungeon, ItemDbEntry meta, bool spawnInWorld)
        {
            _dungeon = dungeon;
            _meta = meta;

            if (spawnInWorld)
            {
                _worldView = dungeon.SpawnItemInWorld(this, meta);
            }
        }

        public void Destroy()
        {
            if (_worldView != null)
            {
                GameObject.Destroy(_worldView.gameObject);
            }
        }

        public void Pickup()
        {
            if (_worldView != null)
            {
                _worldView.gameObject.SetActive(false);
            }
        }

        public void Drop(int2 position)
        {
            _worldView = _dungeon.SpawnItemInWorld(this, _meta);
            WorldPosition = position;
        }

        public bool TryUse(CharacterContext context)
        {
            switch (_meta.Type)
            {
                case ItemType.Weapon:
                    return TryUse(context, context.CurrentEnemyTarget);
                default:
                    return TryUse(context, context.CurrentBumpTarget);
            }
        }

        public bool TryUse(CharacterContext context, Character.Character target)
        {
            //TODO: Run utility selection on abilities to pick the best rather than the first?
            foreach (var abilityMeta in _meta.Abilities)
            {
                if (abilityMeta is IAbility ability && ability.CanUse(context))
                {
                    ability.Use(context, target);
                    return true;
                }
            }

            return false;
        }

        public bool TryUse(CharacterContext context, IBumpTarget target)
        {
            //TODO: Run utility selection on abilities to pick the best rather than the first?
            foreach (var abilityMeta in _meta.Abilities)
            {
                if (abilityMeta is IAbility ability && ability.CanUse(context))
                {
                    ability.Use(context, target);
                    return true;
                }
            }

            return false;
        }
    }
}