using System.Collections;
using System.Collections.Generic;
using Fluid.Roguelike.Database;
using UnityEngine;
using UnityEngine.UI;

namespace Fluid.Roguelike.UI
{
    public class UILabel : MonoBehaviour
    {
        [SerializeField] private string _label;
        [SerializeField] private UiDatabase _db;

        private List<UnityEngine.UI.Image> _characters = new List<Image>();

        public string Label => _label;

        public void SetLabel(string label)
        {
            label = label.ToLower();
            if (_label == label)
                return;

            _label = label;
            UpdateLabel();
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(_label) == false)
            {
                _label = _label.ToLower();
                UpdateLabel();
            }
        }

        [ContextMenu("Update Label")]
        private void UpdateLabel()
        {
            ClearLabel();
            _label = _label.ToLower();
            if (_characters.Count > 0)
            {
                foreach (var character in _characters)
                {
                    GameObject.Destroy(character.gameObject);
                }

                _characters.Clear();
            }

            foreach (var character in _label)
            {
                if (_db.Find(character.ToString(), out var sprite))
                {
                    var c = GameObject.Instantiate(_db.ItemUiPrefab, transform, true);
                    c.sprite = sprite;
                    c.color = Color.white;
                    _characters.Add(c);
                }
            }
        }

        [ContextMenu("Clear Label")]
        private void ClearLabel()
        {
            var children = transform.GetComponentsInChildren<Image>();
            foreach (var child in children)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
            _characters?.Clear();
        }
    }
}