using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class Dungeon : MonoBehaviour
    {
        [SerializeField] private DungeonAgent _agent;
        [SerializeField] private DungeonRoom _dungeonRoomPrefab;
        [SerializeField] private Transform _playerCharacter;

        private PlayerController _playerController;

        public Dictionary<Tuple<int, int>, DungeonTile> Tiles { get; } = new Dictionary<Tuple<int, int>, DungeonTile>();
        public Dictionary<Tuple<int, int>, int> ValueMap { get; } = new Dictionary<Tuple<int, int>, int>();

        private void Start()
        {
            _agent.Generate(0);
            foreach (var meta in _agent.Context.AllRooms)
            {
                var room = GameObject.Instantiate(_dungeonRoomPrefab);
                room.SetMeta(meta);
                room.GenerateTiles(this);
            }

            _playerController = new PlayerController();
            _playerController.Set(_playerCharacter);
        }

        private void Update()
        {
            _playerController?.Tick();
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (ValueMap != null)
            {
                foreach (var value in ValueMap)
                {
                    UnityEditor.Handles.Label(new Vector3(value.Key.Item1, value.Key.Item2, 0), value.Value.ToString());
                }
            }
#endif
        }
    }
}