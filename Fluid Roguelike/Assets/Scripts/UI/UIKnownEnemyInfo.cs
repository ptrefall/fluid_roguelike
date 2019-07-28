using System.Collections;
using System.Collections.Generic;
using Fluid.Roguelike.Character.Stats;
using Fluid.Roguelike.Database;
using UnityEngine;
using UnityEngine.UI;

namespace Fluid.Roguelike.UI
{
    public class UIKnownEnemyInfo : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _name;

        [SerializeField] private RectTransform _healthGroup;
        [SerializeField] private List<UnityEngine.UI.Image> _maxHearts;
        [SerializeField] private List<UnityEngine.UI.Image> _hearts;

        [SerializeField] private RectTransform _manaGroup;
        [SerializeField] private List<UnityEngine.UI.Image> _maxMana;
        [SerializeField] private List<UnityEngine.UI.Image> _mana;

        [SerializeField] private RectTransform _equipmentGroup;
        [SerializeField] private UnityEngine.UI.Image _primaryWeapon;

        private Character.Character _character;
        private UiDatabase _db;

        public string Name
        {
            get => _name.text;
            set => _name.text = value;
        }

        public void Setup(Character.Character character, UiDatabase db)
        {
            _character = character;
            _db = db;

            character.OnPrimaryWeaponChanged += OnPrimaryWeaponChanged;
            if (character.PrimaryWeapon != null)
            {
                OnPrimaryWeaponChanged(character.PrimaryWeapon, null);
            }

            var health = character.GetStat(Roguelike.Character.Stats.StatType.Health);
            if (health != null)
            {
                health.OnValueChanged += OnHealthChanged;
                health.OnMaxValueChanged += OnMaxHealthChanged;

                OnMaxHealthChanged(health, 0);
                OnHealthChanged(health, 0);
            }

            var mana = character.GetStat(Roguelike.Character.Stats.StatType.Mana);
            if (mana != null)
            {
                mana.OnValueChanged += OnManaChanged;
                mana.OnMaxValueChanged += OnMaxManaChanged;

                OnMaxManaChanged(mana, 0);
                OnManaChanged(mana, 0);
            }

            Name = character.Meta.DisplayName;
        }

        private void OnPrimaryWeaponChanged(Item.Item item, Item.Item oldItem)
        {
            SetPrimaryWeapon(item.Meta);
        }

        private void OnHealthChanged(Character.Stats.Stat health, int oldHealth)
        {
            SetHealth(health.Value);
        }

        private void OnMaxHealthChanged(Character.Stats.Stat health, int oldMaxHealth)
        {
            SetMaxHealth(health.MaxValue);
        }

        private void OnManaChanged(Character.Stats.Stat mana, int oldMana)
        {
            SetMana(mana.Value);
        }

        private void OnMaxManaChanged(Character.Stats.Stat mana, int oldMaxMana)
        {
            SetMaxMana(mana.MaxValue);
        }

        public void SetPrimaryWeapon(ItemDbEntry itemMeta)
        {
            if (_primaryWeapon == null)
            {
                _primaryWeapon = GameObject.Instantiate(_db.ItemUiPrefab, _equipmentGroup, false);
            }

            if (itemMeta != null)
            {
                _primaryWeapon.sprite = itemMeta.Sprite;
                _primaryWeapon.color = itemMeta.Color;
            }
            else
            {
                _primaryWeapon.sprite = null;
                _primaryWeapon.color = Color.white;
            }
        }

        /*public void SetHealth(int value)
        {
            UiDbEntry healthDb;
            if (_db.Find(Character.Stats.StatType.Health, out healthDb) == false)
                return;

            var numHearts = value * 0.5f;
            var numWholeHearts = (int)numHearts;
            var addHalfHeart = (numHearts - numWholeHearts) > 0;
            var heartCount = numWholeHearts + (addHalfHeart ? 1 : 0);

            if (_hearts.Count > heartCount)
            {
                if (healthDb.Sprites.Count >= (int)UiManager.HeartStages.Empty)
                {
                    var diff = _hearts.Count - heartCount;
                    for (var i = 0; i < diff; i++)
                    {
                        var heart = _hearts[_hearts.Count - 1];
                        _hearts.RemoveAt(_hearts.Count - 1);
                        if (heart == null || heart.transform == null || heart.IsDestroyed())
                            continue;

                        heart.sprite = healthDb.Sprites[(int)UiManager.HeartStages.Empty];
                    }
                }
            }
            else if (_hearts.Count < heartCount)
            {
                if (healthDb.Sprites.Count >= (int)UiManager.HeartStages.Full)
                {
                    var diff = heartCount - _hearts.Count;
                    for (var i = 0; i < diff; i++)
                    {
                        var heart = _maxHearts[_hearts.Count];
                        _hearts.Add(heart);
                        heart.sprite = healthDb.Sprites[(int)UiManager.HeartStages.Full];
                    }
                }
            }

            if (addHalfHeart)
            {
                var heart = _hearts[_hearts.Count - 1];
                if (heart != null && heart.transform != null && !heart.IsDestroyed())
                {
                    heart.sprite = healthDb.Sprites[(int) UiManager.HeartStages.Half];
                }
            }
        }

        public void SetMaxHealth(int value)
        {
            var numHearts = value * 0.5f;
            var numWholeHearts = (int)numHearts;
            var addHalfHeart = (numHearts - numWholeHearts) > 0;
            var heartCount = numWholeHearts + (addHalfHeart ? 1 : 0);

            if (_maxHearts.Count > heartCount)
            {
                var diff = _maxHearts.Count - heartCount;
                for (var i = 0; i < diff; i++)
                {
                    var heart = _maxHearts[_maxHearts.Count - 1];
                    _maxHearts.RemoveAt(_maxHearts.Count - 1);
                    _hearts.Remove(heart);
                }
            }
            else if (_maxHearts.Count < heartCount)
            {
                UiDbEntry healthDb;
                if (_db.Find(Character.Stats.StatType.Health, out healthDb) == false || healthDb.Prefab == null)
                    return;

                var diff = heartCount - _maxHearts.Count;
                for (var i = 0; i < diff; i++)
                {
                    var heart = GameObject.Instantiate(healthDb.Prefab);
                    heart.transform.SetParent(_healthGroup, false);
                    heart.sprite = healthDb.Sprites[(int)UiManager.HeartStages.Empty];
                    _maxHearts.Add(heart);
                }
            }
        }*/

        public void SetHealth(int value)
        {
            SetStat(value, StatType.Health, _hearts, _maxHearts);
        }

        public void SetMaxHealth(int value)
        {
            SetMaxStat(value, StatType.Health, _hearts, _maxHearts, _healthGroup);
        }

        public void SetMana(int value)
        {
            SetStat(value, StatType.Mana, _mana, _maxMana);
        }

        public void SetMaxMana(int value)
        {
            SetMaxStat(value, StatType.Mana, _mana, _maxMana, _manaGroup);
        }

        public void SetStat(int value, Character.Stats.StatType statType, List<Image> cache, List<Image> maxCache)
        {
            UiDbEntry statDb;
            if (_db.Find(statType, out statDb) == false)
                return;

            var numHearts = value * 0.5f;
            var numWholeHearts = (int)numHearts;
            var addHalfHeart = (numHearts - numWholeHearts) > 0;
            var heartCount = numWholeHearts + (addHalfHeart ? 1 : 0);

            if (cache.Count > heartCount)
            {
                if (statDb.Sprites.Count >= (int)UiManager.HeartStages.Empty)
                {
                    var diff = cache.Count - heartCount;
                    for (var i = 0; i < diff; i++)
                    {
                        var heart = cache[cache.Count - 1];
                        cache.RemoveAt(cache.Count - 1);
                        if (heart == null || heart.transform == null || heart.IsDestroyed())
                            continue;

                        heart.sprite = statDb.Sprites[(int)UiManager.HeartStages.Empty];
                    }
                }
            }
            else if (cache.Count < heartCount)
            {
                if (statDb.Sprites.Count >= (int)UiManager.HeartStages.Full)
                {
                    var diff = heartCount - cache.Count;
                    for (var i = 0; i < diff; i++)
                    {
                        var heart = maxCache[cache.Count];
                        cache.Add(heart);
                        heart.sprite = statDb.Sprites[(int)UiManager.HeartStages.Full];
                    }
                }
            }

            if (addHalfHeart)
            {
                var heart = cache[cache.Count - 1];
                if (heart != null && heart.transform != null && !heart.IsDestroyed())
                {
                    heart.sprite = statDb.Sprites[(int)UiManager.HeartStages.Half];
                }
            }
        }

        public void SetMaxStat(int value, Character.Stats.StatType statType, List<Image> cache, List<Image> maxCache,
            RectTransform group)
        {
            var numHearts = value * 0.5f;
            var numWholeHearts = (int) numHearts;
            var addHalfHeart = (numHearts - numWholeHearts) > 0;
            var heartCount = numWholeHearts + (addHalfHeart ? 1 : 0);

            if (maxCache.Count > heartCount)
            {
                var diff = maxCache.Count - heartCount;
                for (var i = 0; i < diff; i++)
                {
                    var heart = maxCache[maxCache.Count - 1];
                    maxCache.RemoveAt(maxCache.Count - 1);
                    cache.Remove(heart);
                }
            }
            else if (maxCache.Count < heartCount)
            {
                UiDbEntry statDb;
                if (_db.Find(statType, out statDb) == false || statDb.Prefab == null)
                    return;

                var diff = heartCount - maxCache.Count;
                for (var i = 0; i < diff; i++)
                {
                    var heart = GameObject.Instantiate(statDb.Prefab);
                    heart.transform.SetParent(group, false);
                    heart.sprite = statDb.Sprites[(int) UiManager.HeartStages.Empty];
                    maxCache.Add(heart);
                }
            }
        }
    }
}
