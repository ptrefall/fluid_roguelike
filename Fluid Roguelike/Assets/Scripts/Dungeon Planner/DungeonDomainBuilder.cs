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

                    var parentRoom = TryGetParentRoom(context, allowOverlap, out var validExits);
                    if (parentRoom != null)
                    { 
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
                                    room.X = parentRoom.X;
                                    room.Y = parentRoom.Y - 1;
                                } break;
                            case BuilderDirection.East:
                                {
                                    room.X = parentRoom.X + 1;
                                    room.Y = parentRoom.Y;
                                } break;
                            case BuilderDirection.South:
                                {
                                    room.X = parentRoom.X;
                                    room.Y = parentRoom.Y + 1;
                                } break;
                            case BuilderDirection.West:
                                {
                                    room.X = parentRoom.X - 1;
                                    room.Y = parentRoom.Y;
                                } break;
                        }

                        Debug.Log($"Spawn room[{room.Id}] ([{room.X},{room.Y}], {room.Shape}:{room.Width}x{room.Height}:{room.Theme}) to the {direction} of room[{parentRoom.Id}]\n");
                    }
                    else
                    {
                        Debug.Log($"Spawn first room[{room.Id}] ([{room.X},{room.Y}], {room.Shape}:{room.Width}x{room.Height}:{room.Theme})\n");
                    }

                    if (IsOccupied.ContainsKey(new Tuple<int, int>(room.X, room.Y)) == false)
                    {
                        IsOccupied.Add(new Tuple<int, int>(room.X, room.Y), true);
                    }

                    context.RoomStack.Push(room);
                    context.AllRooms.Add(room);
                    return TaskStatus.Success;
                });
            }
            End();
            return this;
        }

        private DungeonRoomMeta TryGetParentRoom(DungeonContext context, bool allowOverlap, out List<BuilderDirection> validExits)
        {
            validExits = null;
            if (context.RoomStack == null || context.RoomStack.Count == 0)
                return null;

            var room = context.RoomStack.Peek();
            while (room != null)
            {
                validExits = GetValidExits(context, room);
                if ((validExits == null || validExits.Count == 0) && !allowOverlap)
                {
                    // What to do?
                    context.RoomStack.Pop();
                    if (context.RoomStack.Count == 0)
                        break;

                    room = context.RoomStack.Peek();
                }
                else
                {
                    break;
                }
            }

            if ((validExits == null || validExits.Count == 0) && allowOverlap)
            {
                if(validExits == null)
                    validExits = new List<BuilderDirection>();

                validExits.Add(BuilderDirection.North);
                validExits.Add(BuilderDirection.East);
                validExits.Add(BuilderDirection.South);
                validExits.Add(BuilderDirection.West);
            }

            return room;
        }

        private List<BuilderDirection> GetValidExits(DungeonContext context, DungeonRoomMeta r)
        {
            var result = context.Factory.CreateList<BuilderDirection>();
            result.Clear();

            if(IsOccupied.ContainsKey(new Tuple<int, int>(r.X,r.Y-1)) == false)
                result.Add(BuilderDirection.North);
            if (IsOccupied.ContainsKey(new Tuple<int, int>(r.X+1, r.Y)) == false)
                result.Add(BuilderDirection.East);
            if (IsOccupied.ContainsKey(new Tuple<int, int>(r.X, r.Y+1)) == false)
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
                        var parentRoom = context.RoomStack.Peek();
                        if (parentRoom != null)
                        {
                            Debug.Log($"Spawn player in room {parentRoom.Id}\n");
                            var playerSpawn = new DungeonSpawnPlayerMeta
                            {
                                SpawnRoom = parentRoom,
                            };
                            context.PlayerSpawnMeta = playerSpawn;
                            return TaskStatus.Success;
                        }

                        Debug.Log($"Error! Can't spawn player in the void!\n");
                        return TaskStatus.Failure;
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

        public DungeonDomainBuilder Repeat(int count)
        {
            Sequence("Repeat");
            {
                Action("Set repeat count");
                {
                    Do((context => TaskStatus.Success));
                    Effect("Set repeat count", EffectType.PlanAndExecute,
                        (context, type) => context.SetState(DungeonWorldState.RepeatCount, count, type));
                }
                End();
                this.Repeat<DungeonDomainBuilder, DungeonContext>("Repeat", (uint) DungeonWorldState.RepeatCount);
            }
            return this;
        }

        public DungeonDomainBuilder EndRepeat()
        {
            End();
            End();
            return this;
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
                    Repeat(10);
                    {
                        SpawnRoom(4, 12, DungeonRoomShape.Random, false);
                    }
                    EndRepeat();
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
                    Repeat(10);
                    {
                        SpawnRoom(4, 12, DungeonRoomShape.Random, false);
                    }
                    EndRepeat();
                }
            }
            End();
            return this;
        }
    }
}