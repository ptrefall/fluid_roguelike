using Fluid.Roguelike.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Dungeon;
using Fluid.Roguelike.Item;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

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
        [SerializeField] private TMPro.TextMeshProUGUI _scraps;
        [SerializeField] private RectTransform _knownEnemiesGroup;
        [SerializeField] private RectTransform _inventoryGroup;
        [SerializeField] private RectTransform _logGroup;
        [SerializeField] private Image _map;
        [SerializeField] private Color _undiscoveredTile;
        [SerializeField] private Color _discoveredWallTile;
        [SerializeField] private Color _discoveredFloorTile;
        [SerializeField] private Color _discoveredVoidTile;
        [SerializeField] private Color _playerTile;
        [SerializeField] private Color _enemyTile;
        [SerializeField] private Color _itemTile;

        private readonly Dictionary<CharacterStatusType, GameObject> _statusesNeedRemoval = new Dictionary<CharacterStatusType, GameObject>();
        private readonly Dictionary<Character.Character, UIKnownEnemyInfo> _knownEnemyInfos = new Dictionary<Character.Character, UIKnownEnemyInfo>();
        private readonly Dictionary<Item.Item, UIInventoryItem> _inventoryInfo = new Dictionary<Item.Item, UIInventoryItem>();
        private readonly List<UILog> _logs = new List<UILog>();

        public enum HeartStages { Full, Half, Empty }

        public void AddLog(string log)
        {

        }

        public void UpdateScraps(Character.Character character)
        {
            _scraps.text = character.Scraps.ToString();
        }

        public void UpdateMap(CharacterContext context)
        {
            if (_map == null)
                return;

            if (_map.sprite == null)
            {
                var rect = _map.rectTransform.rect;
                var texture = new Texture2D((int)rect.width / 4, (int)rect.height / 4);
                texture.alphaIsTransparency = true;
                texture.filterMode = FilterMode.Point;
                var sprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), Vector2.one * 0.5f, 1);
                _map.sprite = sprite;
                _map.color = Color.white;
            }

            var center = new int2(_map.sprite.texture.width / 2, _map.sprite.texture.height / 2);
            var topLeft = new int2(0, 0);
            var bottomRight = new int2(_map.sprite.texture.width, _map.sprite.texture.height);

            for (var y = topLeft.y; y < bottomRight.y; y++)
            {
                for (var x = topLeft.x; x < bottomRight.x; x++)
                {
                    var lp = new int2(x, y);
                    var wp = context.Self.Position + (lp - center);

                    if (lp.x == center.x && lp.y == center.y)
                    {
                        _map.sprite.texture.SetPixel(x, y, _playerTile);
                        continue;
                    }

                    // If this is an undiscovered tile
                    if (context.DiscoveredTiles.Contains(wp) == false)
                    {
                        _map.sprite.texture.SetPixel(x,y, _undiscoveredTile);
                        continue;
                    }

                    bool didSetEnemy = false;
                    foreach (var enemy in context.KnownEnemies)
                    {
                        if (enemy.Position.x == wp.x && enemy.Position.y == wp.y)
                        {
                            _map.sprite.texture.SetPixel(x, y, _enemyTile);
                            didSetEnemy = true;
                            break;
                        }
                    }

                    if (didSetEnemy)
                    {
                        continue;
                    }

                    var item = context.Dungeon.GetItemAt(wp);
                    if (item != null)
                    {
                        _map.sprite.texture.SetPixel(x, y, _itemTile);
                        continue;
                    }

                    if (context.Dungeon.ValueMap.ContainsKey(wp) == false)
                    {
                        _map.sprite.texture.SetPixel(x,y, _discoveredVoidTile);
                        continue;
                    }

                    var info = context.Dungeon.ValueMap[wp];
                    switch (info.Index)
                    {
                        case DungeonTile.Index.Wall:
                            _map.sprite.texture.SetPixel(x, y, _discoveredWallTile);
                            break;
                        case DungeonTile.Index.Floor:
                            _map.sprite.texture.SetPixel(x, y, _discoveredFloorTile);
                            break;
                        default:
                            _map.sprite.texture.SetPixel(x, y, _discoveredVoidTile);
                            break;
                    }
                }
            }

            _map.sprite.texture.Apply();
        }

        public void UpdateInventory(Character.Character character)
        {
            if (character.Inventory.Count == 0)
            {
                foreach (var kvp in _inventoryInfo)
                {
                    GameObject.Destroy(kvp.Value.gameObject);
                }
                _inventoryInfo.Clear();
            }
            else
            {
                // First remove items no longer in the inventory
                List<Item.Item> removeKeys = null;
                foreach (var kvp in _inventoryInfo)
                {
                    if (character.Inventory.Contains(kvp.Key) == false)
                    {
                        if (removeKeys == null)
                        {
                            removeKeys = new List<Item.Item>();
                        }
                        
                        removeKeys.Add(kvp.Key);
                    }
                }

                if (removeKeys != null)
                {
                    foreach (var key in removeKeys)
                    {
                        var value = _inventoryInfo[key];
                        _inventoryInfo.Remove(key);
                        GameObject.Destroy(value.gameObject);
                    }
                }

                // Then add new items in inventory to ui
                int index = 0;
                foreach (var item in character.Inventory)
                {
                    index++;
                    UIInventoryItem info = null;
                    if (_inventoryInfo.ContainsKey(item) == false)
                    {
                        info = GameObject.Instantiate(_db.InventoryItemPrefab, _inventoryGroup, false);
                        info.Setup(item, _db);
                        _inventoryInfo.Add(item, info);
                    }
                    else
                    {
                        info = _inventoryInfo[item];
                    }

                    if (item.Meta.Type == ItemType.Weapon)
                    {
                        if (item != character.PrimaryWeapon)
                        {
                            if (_db.Find(index.ToString(), out var sprite))
                            {
                                info.UpdateSprite(sprite, Color.white);
                            }
                        }
                        else
                        {
                            info.SetDefaultSprite();
                        }
                    }
                }
            }
        }

        public void UpdateKnownEnemies(CharacterContext context)
        {
            if (context.KnownEnemies == null || context.KnownEnemies.Count == 0)
            {
                foreach (var kvp in _knownEnemyInfos)
                {
                    GameObject.Destroy(kvp.Value.gameObject);
                    kvp.Key.OnDeath -= OnKnownEnemyDeath;
                }
                _knownEnemyInfos.Clear();
            }
            else
            {
                List<Character.Character> pendingRemoval = null;
                foreach (var enemy in context.KnownEnemies)
                {
                    if (enemy.IsDead)
                    {
                        if (_knownEnemyInfos.ContainsKey(enemy))
                        {
                            var value = _knownEnemyInfos[enemy];
                            GameObject.Destroy(value.gameObject);
                            enemy.OnDeath -= OnKnownEnemyDeath;

                            if (pendingRemoval == null)
                            {
                                pendingRemoval = new List<Character.Character>();
                            }
                            pendingRemoval.Add(enemy);
                        }
                        continue;
                    }

                    if (_knownEnemyInfos.ContainsKey(enemy))
                    {
                        // If this enemy is no longer in our field of view, remove it from the list.
                        if (!context.FieldOfView.ContainsKey(enemy.Position))
                        {
                            var value = _knownEnemyInfos[enemy];
                            GameObject.Destroy(value.gameObject);
                            enemy.OnDeath -= OnKnownEnemyDeath;

                            if (pendingRemoval == null)
                            {
                                pendingRemoval = new List<Character.Character>();
                            }
                            pendingRemoval.Add(enemy);
                        }
                        continue;
                    }

                    if (context.FieldOfView.ContainsKey(enemy.Position))
                    {
                        var info = GameObject.Instantiate(_db.KnownEnemyInfoPrefab, _knownEnemiesGroup, false);
                        info.Setup(enemy, _db);
                        _knownEnemyInfos.Add(enemy, info);

                        enemy.OnDeath += OnKnownEnemyDeath;
                    }
                }

                if (pendingRemoval != null)
                {
                    foreach (var enemy in pendingRemoval)
                    {
                        _knownEnemyInfos.Remove(enemy);
                    }
                }
            }
        }

        private void OnKnownEnemyDeath(Character.Character character)
        {
            if (_knownEnemyInfos.ContainsKey(character))
            {
                var info = _knownEnemyInfos[character];
                GameObject.Destroy(info.gameObject);
                _knownEnemyInfos.Remove(character);
            }
        }

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
                    var updaters = fx.GetComponents<IUIStatusUpdater>();
                    foreach (var updater in updaters)
                    {
                        updater?.Setup(status.Life);
                    }
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

        public void UpdateStatuses()
        {
            foreach (var kvp in _statusesNeedRemoval)
            {
                var updaters = kvp.Value.GetComponents<IUIStatusUpdater>();
                foreach (var updater in updaters)
                {
                    updater?.Tick();
                }
            }
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
                        if (heart == null || heart.transform == null || heart.IsDestroyed())
                            continue;

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
                if (heart != null && heart.transform != null && !heart.IsDestroyed())
                {
                    heart.sprite = healthDb.Sprites[(int)UiManager.HeartStages.Half];
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
                    heart.sprite = healthDb.Sprites[(int)HeartStages.Empty];
                    _maxHearts.Add(heart);
                }
            }
        }
    }
}
