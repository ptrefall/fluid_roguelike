
using Cinemachine;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Effects;
using Fluid.Roguelike.Item;
using Fluid.Roguelike.UI;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike
{
    public class PlayerController : CharacterController
    {
        private bool forceKeyDown = false;
        private float nextSameKeyTime = 0f;
        private KeyCode lastKey = KeyCode.Escape;

        private UiManager _uiManager;

        private const float keyPause = 0.15f;

        private string oldCharacterName;
        private int _lastFieldOfViewEnemyCount = 0;

        private int2 _lastMoveDir = int2.zero;

        public void Set(UiManager uiManager)
        {
            _uiManager = uiManager;
        }

        public override void Set(Character.Character character)
        {
            if (Character != null)
            {
                Unset(Character);
            }

            base.Set(character);

            oldCharacterName = character.name;
            character.name = "Player";
            character.IsPlayerControlled = true;
            character.OnPrimaryWeaponChanged += OnPrimaryWeaponChanged;
            character.OnStatusAdded += OnStatusAdded;
            character.OnStatusRemoved += OnStatusRemoved;
            character.OnStatusReset += OnStatusReset;
            character.Context.OnKnownEnemiesUpdated += OnKnownEnemiesUpdated;

            var health = character.GetStat(Roguelike.Character.Stats.StatType.Health);
            if (health != null)
            {
                health.OnValueChanged += OnHealthChanged;
                health.OnMaxValueChanged += OnMaxHealthChanged;

                OnMaxHealthChanged(health, 0);
                OnHealthChanged(health, 0);
            }

            if (Character.PrimaryWeapon != null)
            {
                OnPrimaryWeaponChanged(Character.PrimaryWeapon, null);
            }

            var cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (cameraBrain != null && cameraBrain.ActiveVirtualCamera != null)
            {
                cameraBrain.ActiveVirtualCamera.LookAt = character.transform;
                cameraBrain.ActiveVirtualCamera.Follow = character.transform;
            }
        }

        public override void Unset(Character.Character character)
        {
            base.Unset(character);

            if (!string.IsNullOrEmpty(oldCharacterName))
            {
                character.name = oldCharacterName;
                character.GodMode = false;
            }
            character.IsPlayerControlled = false;
            character.OnPrimaryWeaponChanged -= OnPrimaryWeaponChanged;
            character.OnStatusAdded -= OnStatusAdded;
            character.OnStatusRemoved -= OnStatusRemoved;
            character.OnStatusReset -= OnStatusReset;
            character.Context.OnKnownEnemiesUpdated -= OnKnownEnemiesUpdated;

            var health = character.GetStat(Roguelike.Character.Stats.StatType.Health);
            if (health != null)
            {
                health.OnValueChanged -= OnHealthChanged;
                health.OnMaxValueChanged -= OnMaxHealthChanged;
            }

            var cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (cameraBrain != null && cameraBrain.ActiveVirtualCamera != null)
            {
                cameraBrain.ActiveVirtualCamera.LookAt = null;
                cameraBrain.ActiveVirtualCamera.Follow = null;
            }
        }

        private void OnHealthChanged(Character.Stats.Stat health, int oldHealth)
        {
            _uiManager?.SetHealth(health.Value);
        }

        private void OnMaxHealthChanged(Character.Stats.Stat health, int oldMaxHealth)
        {
            _uiManager?.SetMaxHealth(health.MaxValue);
        }

        private void OnPrimaryWeaponChanged(Item.Item item, Item.Item oldItem)
        {
            _uiManager?.SetPrimaryWeapon(item.Meta);
        }

        private void OnStatusAdded(Status status)
        {
            _uiManager?.AddStatus(status);
        }

        private void OnStatusRemoved(Status status)
        {
            _uiManager?.RemoveStatus(status);
        }

        private void OnStatusReset()
        {
            _uiManager?.ResetStatuses();
        }

        private void OnKnownEnemiesUpdated(CharacterContext context)
        {
            _uiManager?.UpdateKnownEnemies(context);

            int inFieldOfViewCount = 0;
            foreach (var enemy in context.KnownEnemies)
            {
                if (context.FieldOfView.ContainsKey(enemy.Position))
                {
                    inFieldOfViewCount++;
                }
            }

            if (inFieldOfViewCount > _lastFieldOfViewEnemyCount)
            {
                forceKeyDown = true;
                CameraShake.ShakeStatic();
            }

            _lastFieldOfViewEnemyCount = inFieldOfViewCount;
        }

        private bool GetInventoryKeyDown(out int index)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                index = 9;
                return true;
            }

            for(var i = (int)KeyCode.Alpha1; i <= (int)KeyCode.Alpha9; i++)
            {
                if (Input.GetKeyDown((KeyCode) i))
                {
                    index = i - (int)KeyCode.Alpha1;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public override void Tick(Dungeon.Dungeon dungeon)
        {
            if (Character == null || Character.IsDead)
                return;

            // Cheats
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
            {
                Character.AddTimedStatus(CharacterStatusType.Stunned, 2);
                Debug.Log($"{Character.name} got stunned!");
                return;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2))
            {
                Character.GodMode = !Character.GodMode;
                return;
            }

            // Logic

            if (Input.GetKeyDown(KeyCode.E))
            {
                var item = dungeon.GetItemAt(Character.Position);
                if (item == null)
                {
                    item = dungeon.GetItemAt(Character.Position + _lastMoveDir);
                    if (item == null)
                    {
                        return;
                    }
                }

                if (Character.TryInteract(item) == false)
                    return;
            }
            else if (GetInventoryKeyDown(out var index))
            {
                if (index >= Character.Inventory.Count)
                    return;

                var item = Character.Inventory[index];
                if (item.Meta.Type == ItemType.Weapon && Character.PrimaryWeapon != item)
                {
                    Character.SetPrimaryWeapon(item);
                }
                else
                {
                    return;
                }
            }
            else
            {
                MoveResult result = MoveResult.None;
                MoveDirection dir = CheckMoveInput();

                if (dir != MoveDirection.None)
                {
                    result = Move(dungeon, dir, true);
                    _lastMoveDir = DirectionToVec(dir);
                }

                // We did not consume a turn
                if (result == MoveResult.None)
                {
                    return;
                }

                if (result == MoveResult.Collided)
                {
                    forceKeyDown = true;
                    return;
                }

                if (result == MoveResult.Bump)
                {
                    if (Character.Context.CurrentBumpTarget != null)
                    {
                        if (Character.Context.CurrentBumpTarget is Character.Character c)
                        {
                            Character.Melee(c);
                        }

                        forceKeyDown = true;
                    }
                    else
                    {
                        Debug.LogError("This should never happen");
                    }
                }
            }

            // We tick the AI first, so that we're not updating status effects until NPCs have had the chance to add new ones.
            dungeon.TickAI();

            if (Character == null || Character.IsDead)
            {
                return;
            }

            ConsumeTurn(dungeon);
            Character.TickTurn_Sensors();

            UpdateVisibility(dungeon);

            UpdateMap();
            UpdateInventory();
            UpdateScraps();
            UpdateStatuses();
        }

        public void UpdateMap()
        {
            _uiManager?.UpdateMap(Character?.Context);
        }

        public void UpdateInventory()
        {
            _uiManager?.UpdateInventory(Character);
        }

        public void UpdateScraps()
        {
            _uiManager?.UpdateScraps(Character);
        }

        public void UpdateStatuses()
        {
            _uiManager?.UpdateStatuses();
        }

        public void UpdateVisibility(Dungeon.Dungeon dungeon)
        {
            if (Character != null)
            {
                foreach (var tile in dungeon.Tiles)
                {
                    if (Character.Context.FieldOfView.ContainsKey(tile.Key))
                    {
                        tile.Value.Visibility(true);
                    }
                    else
                    {
                        tile.Value.Visibility(false);
                    }
                }

                foreach (var character in dungeon.Characters)
                {
                    if (character == Character)
                        continue;

                    if (Character.Context.FieldOfView.ContainsKey(character.Position))
                    {
                        character.Visibility(true);
                    }
                    else
                    {
                        character.Visibility(false);
                    }
                }

                foreach (var item in dungeon.WorldItems)
                {
                    if (Character.Context.FieldOfView.ContainsKey(item.WorldPosition))
                    {
                        item.Visibility(true);
                    }
                    else
                    {
                        item.Visibility(false);
                    }
                }
            }
        }

        private MoveDirection CheckMoveInput()
        {
            if (forceKeyDown)
            {
                return CheckMoveInputKeyDown();
            }

            var dir = MoveDirection.None;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                if (lastKey == KeyCode.W)
                {
                    if (Time.time < nextSameKeyTime)
                        return MoveDirection.None;
                }

                lastKey = KeyCode.W;
                nextSameKeyTime = Time.time + keyPause;
                dir = MoveDirection.N;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                if (lastKey == KeyCode.D)
                {
                    if (Time.time < nextSameKeyTime)
                        return MoveDirection.None;
                }

                lastKey = KeyCode.D;
                nextSameKeyTime = Time.time + keyPause;
                dir = MoveDirection.E;
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                if (lastKey == KeyCode.S)
                {
                    if (Time.time < nextSameKeyTime)
                        return MoveDirection.None;
                }

                lastKey = KeyCode.S;
                nextSameKeyTime = Time.time + keyPause;
                dir = MoveDirection.S;
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                if (lastKey == KeyCode.A)
                {
                    if (Time.time < nextSameKeyTime)
                        return MoveDirection.None;
                }

                lastKey = KeyCode.A;
                nextSameKeyTime = Time.time + keyPause;
                dir = MoveDirection.W;
            }

            return dir;
        }

        private MoveDirection CheckMoveInputKeyDown()
        {
            var dir = MoveDirection.None;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                lastKey = KeyCode.W;
                nextSameKeyTime = Time.time + keyPause;
                forceKeyDown = false;
                dir = MoveDirection.N;
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                lastKey = KeyCode.D;
                nextSameKeyTime = Time.time + keyPause;
                forceKeyDown = false;
                dir = MoveDirection.E;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                lastKey = KeyCode.S;
                nextSameKeyTime = Time.time + keyPause;
                forceKeyDown = false;
                dir = MoveDirection.S;
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                lastKey = KeyCode.A;
                nextSameKeyTime = Time.time + keyPause;
                forceKeyDown = false;
                dir = MoveDirection.W;
            }

            return dir;
        }
    }
}
