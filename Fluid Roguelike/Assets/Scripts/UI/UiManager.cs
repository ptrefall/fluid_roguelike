using Fluid.Roguelike.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fluid.Roguelike.UI
{
    public class UiManager : MonoBehaviour
    {
        [SerializeField] private UiDatabase _db;
        [SerializeField] private RectTransform _healthGroup;
        [SerializeField] private List<UnityEngine.UI.Image> _maxHearts;
        [SerializeField] private List<UnityEngine.UI.Image> _hearts;

        private enum HeartStages { Full, Half, Empty }

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
