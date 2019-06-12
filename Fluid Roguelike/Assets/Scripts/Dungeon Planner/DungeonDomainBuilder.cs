using System;
using System.Collections.Generic;
using FluidHTN;
using FluidHTN.Compounds;
using FluidHTN.Factory;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonDomainBuilder : BaseDomainBuilder<DungeonDomainBuilder, DungeonContext>
    {
        private Dictionary<Tuple<int, int>, bool> IsOccupied { get; set; } = new Dictionary<Tuple<int, int>, bool>();

        public DungeonDomainBuilder(string domainName) : base(domainName, new DefaultFactory())
        {
        }

        public void ClearCache()
        {
            IsOccupied.Clear();
        }

        public DungeonDomainBuilder IsAtDepth(int depth)
        {
            Condition($"Is at depth {depth}",
                context => context.GetState(DungeonWorldState.DungeonDepth) == (byte) depth);

            return this;
        }

        public DungeonDomainBuilder ChangeBuilderDirection(BuilderDirection direction)
        {
            Action($"Change builder direction");
            {
                Do((context) =>
                {
                    Debug.Log($"Change build direction {direction}\n");
                    context.CurrentBuilderDirection = direction;
                    return TaskStatus.Success;
                });
            }
            End();
            return this;
        }

        public DungeonDomainBuilder SetTheme(DungeonTheme theme)
        {
            Action($"Set theme");
            {
                Do((context) =>
                {
                    Debug.Log($"Set theme {theme}\n");
                    context.CurrentTheme = theme;
                    return TaskStatus.Success;
                });
            }
            End();
            return this;
        }

        public DungeonDomainBuilder SpawnRoom(int minSize, int maxSize, DungeonRoomShape shape, bool allowOverlap)
        {
            var width = Random.Range(minSize, maxSize+1);
            var height = Random.Range(minSize, maxSize + 1);
            if (shape == DungeonRoomShape.Random)
            {
                shape = (DungeonRoomShape)Random.Range((int)DungeonRoomShape.Rectangular, (int)DungeonRoomShape.Round + 1);
            }
            Action($"Spawn room ({shape}:{width}x{height})");
            {
                Do((context) =>
                {
                    var room = new DungeonRoomMeta
                    {
                        Width = width,
                        Height = height,
                        Shape = shape,
                        X = 0,
                        Y = 0,
                        Theme = context.CurrentTheme,
                    };

                    var lastRoom = context.LastRoomMeta;
                    if (lastRoom != null)
                    {
                        var validExits = GetValidExits(context);
                        if ((validExits == null || validExits.Count == 0) && !allowOverlap)
                        {
                            // What to do?
                            return TaskStatus.Failure;
                        }

                        var direction = context.CurrentBuilderDirection;
                        if (validExits.Contains(direction) == false && !allowOverlap)
                        {
                            direction = BuilderDirection.Random;
                        }

                        if (direction == BuilderDirection.Random)
                        {
                            if (validExits.Count > 0)
                            {
                                direction = validExits[Random.Range(0, validExits.Count)];
                            }
                            else
                            {
                                direction = (BuilderDirection) Random.Range((int) BuilderDirection.North, (int) BuilderDirection.West + 1);
                            }
                        }

                        switch (direction)
                        {
                            case BuilderDirection.North:
                                {
                                    room.X = lastRoom.X;
                                    room.Y = lastRoom.Y - 1;
                                } break;
                            case BuilderDirection.East:
                                {
                                    room.X = lastRoom.X + 1;
                                    room.Y = lastRoom.Y;
                                } break;
                            case BuilderDirection.South:
                                {
                                    room.X = lastRoom.X;
                                    room.Y = lastRoom.Y + 1;
                                } break;
                            case BuilderDirection.West:
                                {
                                    room.X = lastRoom.X - 1;
                                    room.Y = lastRoom.Y;
                                } break;
                        }

                        Debug.Log($"Spawn room[{room.Id}] ([{room.X},{room.Y}], {room.Shape}:{room.Width}x{room.Height}:{room.Theme}) to the {direction} of room[{lastRoom.Id}]\n");
                    }
                    else
                    {
                        Debug.Log($"Spawn first room[{room.Id}] ([{room.X},{room.Y}], {room.Shape}:{room.Width}x{room.Height}:{room.Theme})\n");
                    }

                    IsOccupied.Add(new Tuple<int, int>(room.X, room.Y), true);

                    context.LastRoomMeta = room;
                    return TaskStatus.Success;
                });
            }
            End();
            return this;
        }

        private List<BuilderDirection> GetValidExits(DungeonContext context)
        {
            var result = context.Factory.CreateList<BuilderDirection>();
            result.Clear();

            var r = context.LastRoomMeta;
            if(IsOccupied.ContainsKey(new Tuple<int, int>(r.X,r.Y+1)) == false)
                result.Add(BuilderDirection.North);
            if (IsOccupied.ContainsKey(new Tuple<int, int>(r.X + 1, r.Y)) == false)
                result.Add(BuilderDirection.East);
            if (IsOccupied.ContainsKey(new Tuple<int, int>(r.X, r.Y-1)) == false)
                result.Add(BuilderDirection.South);
            if (IsOccupied.ContainsKey(new Tuple<int, int>(r.X-1, r.Y)) == false)
                result.Add(BuilderDirection.West);

            return result;
        }

        public DungeonDomainBuilder TrySpawnPlayer()
        {
            Optionally();
            {
                Action($"Try spawn player");
                {
                    Condition("Player not already spawned",
                        context => context.HasState(DungeonWorldState.PlayerNeedsSpawnPoint));
                    Do((context) =>
                    {
                        Debug.Log($"Spawn player in room {context.LastRoomMeta?.Id ?? 0}\n");
                        var playerSpawn = new DungeonSpawnPlayerMeta
                        {
                            SpawnRoom = context.LastRoomMeta,
                        };
                        context.PlayerSpawnMeta = playerSpawn;
                        return TaskStatus.Success;
                    });
                }
                End();
            }
            End();
            return this;
        }

        public DungeonDomainBuilder Optionally()
        {
            return this.AlwaysSucceedSelect<DungeonDomainBuilder, DungeonContext>("Optionally");
        }

        // ------------------------------------------- BUILDER PREFABS

        public DungeonDomainBuilder CreateTheCave()
        {
            Sequence("The cave");
            {
                SetTheme(DungeonTheme.Cave);
                {
                    ChangeBuilderDirection(BuilderDirection.Random);
                    SpawnRoom(4, 8, DungeonRoomShape.Rectangular, false);
                    {
                        TrySpawnPlayer();
                    }
                    SpawnRoom(8, 8, DungeonRoomShape.Random, false);
                    SpawnRoom(4, 6, DungeonRoomShape.Random, false);
                }
            }
            End();
            return this;
        }

        public DungeonDomainBuilder CreateTheForest()
        {
            Sequence("The forest");
            {
                SetTheme(DungeonTheme.Forest);
                {
                    ChangeBuilderDirection(BuilderDirection.Random);
                    SpawnRoom(8, 12, DungeonRoomShape.Round, false);
                    {
                        TrySpawnPlayer();
                    }
                    SpawnRoom(4, 8, DungeonRoomShape.Random, false);
                    SpawnRoom(8, 12, DungeonRoomShape.Random, false);
                }
            }
            End();
            return this;
        }
    }
}