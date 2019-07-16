using System.Collections;
using System.Collections.Generic;
using Fluid.Roguelike.Ability;
using Fluid.Roguelike.Database;
using UnityEngine;
using UnityEngine.UI;

namespace Fluid.Roguelike.UI
{
    public class UIInventoryItem : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _name;
        [SerializeField] private TMPro.TextMeshProUGUI _info;
        [SerializeField] private Image _icon;

        private Item.Item _item;
        private UiDatabase _db;

        public string Name
        {
            get => _name.text;
            set => _name.text = value;
        }

        public string Info
        {
            get => _info.text;
            set => _info.text = value;
        }

        public void Setup(Item.Item item, UiDatabase db)
        {
            _item = item;
            _db = db;

            Name = item.Meta.DisplayName;
            _icon.sprite = item.Meta.Sprite;
            _icon.color = item.Meta.Color;

            var info = "";
            for(var i = 0; i < item.Meta.Abilities.Count; i++)
            { 
                if (item.Meta.Abilities[i] is IAbility ability)
                {
                    info += ability.Info;
                    if (i < item.Meta.Abilities.Count - 1)
                    {
                        info += " ";
                    }
                }
            }

            Info = info;
        }

        public void UpdateSprite(Sprite sprite, Color color)
        {
            _icon.sprite = sprite;
            _icon.color = color;
        }

        public void SetDefaultSprite()
        {
            _icon.sprite = _item.Meta.Sprite;
            _icon.color = _item.Meta.Color;
        }
    }
}
