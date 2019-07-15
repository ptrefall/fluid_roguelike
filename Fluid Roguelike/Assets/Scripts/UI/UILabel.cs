using System.Collections;
using System.Collections.Generic;
using Fluid.Roguelike.Database;
using UnityEngine;

namespace Fluid.Roguelike.UI
{
    public class UILabel : MonoBehaviour
    {
        [SerializeField] private string _label;
        [SerializeField] private UiDatabase _db;

        private void Start()
        {
            foreach (var character in _label)
            {
                if (_db.Find(character.ToString(), out var sprite))
                {
                    var c = GameObject.Instantiate(_db.ItemUiPrefab, transform, true);
                    c.sprite = sprite;
                    c.color = Color.white;
                }
            }
        }
    }
}