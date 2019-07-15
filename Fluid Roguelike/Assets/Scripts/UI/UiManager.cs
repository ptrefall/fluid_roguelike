using Fluid.Roguelike.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character;
using UnityEngine;

namespace Fluid.Roguelike.UI
{
    public class UiManager : MonoBehaviour
    {
        [SerializeField] private UiDatabase _db;
        [SerializeField] private RectTransform _healthGroup;
        [SerializeField] private List<UnityEngine.UI.Image> _maxHearts;
        [SerializeField] private List<UnityEngine.UI.Image> _hearts;
        [SerializeField] private RectTransform _equipmentGroup;
        [SerializeField] private UnityEngine.UI.Image _primaryWeapon;

        private readonly Dictionary<CharacterStatusType, GameObject> _statusesNeedRemoval = new Dictionary<CharacterStatusType, GameObject>();

        private enum HeartStages { Full, Half, Empty }

        public void AddStatus(Status status)
        {
            if (_statusesNeedRemoval.ContainsKey(status.Type))
                return;

            if (_db.Find(status.Type, out var effect))
            {
                var fx = GameObject.Instantiate(effect.Effect, transform, false);
                if (effect.NeedsRemoval)
                {
                    _statusesNeedRemoval.Add(effect.Type, fx);
                }
            }
        }

        public void RemoveStatus(Status status)
        {
            if (_statusesNeedRemoval.ContainsKey(status.Type))
            {
                var fx = _statusesNeedRemoval[status.Type];
                GameObject.Destroy(fx);
                _statusesNeedRemoval.Remove(status.Type);
            }
        }

        public void ResetStatuses()
        {
            foreach (var kvp in _statusesNeedRemoval)
            {
                GameObject.Destroy(kvp.Value);
            }
            _statusesNeedRemoval.Clear();
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

        public void SetHealth(int value)
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
                if (healthDb.Sprites.Count >= (int)HeartStages.Empty)
                {
                    var diff = _hearts.Count - heartCount;
                    for(var i = 0; i < diff; i++)
                    {
                        var heart = _hearts[_hearts.Count - 1];
                        _hearts.RemoveAt(_hearts.Count - 1);
                        heart.sprite = healthDb.Sprites[(int)HeartStages.Empty];
                    }
                }
            }
            else if(_hearts.Count < heartCount)
            {
                if (healthDb.Sprites.Count >= (int)HeartStages.Full)
                {
                    var diff = heartCount - _hearts.Count;
                    for (var i = 0; i < diff; i++)
                    {
                        var heart = _maxHearts[_hearts.Count];
                        _hearts.Add(heart);
                        heart.sprite = healthDb.Sprites[(int)HeartStages.Full];
                    }
                }
            }

            if (addHalfHeart)
            {
                var heart = _hearts[_hearts.Count - 1];
                heart.sprite = healthDb.Sprites[(int)HeartStages.Half];
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
                    heart.sprite = healthDb.Sprites[(int)HeartStages.Empty];
                    _maxHearts.Add(heart);
                }
            }
        }
    }
}
