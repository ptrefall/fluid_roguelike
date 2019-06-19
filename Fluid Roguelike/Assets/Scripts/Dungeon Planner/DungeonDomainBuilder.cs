using System;
using System.Collections.Generic;
using Fluid.Roguelike.Database;
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
        private DecorationDatabaseManager _decorations;

        public DungeonDomainBuilder(string domainName, DecorationDatabaseManager decorations) : base(domainName, new DefaultFactory())
        {
            _decorations = decorations;
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

        public DungeonDomainBuilder SpawnRoom(int minSize, int maxSize, DungeonRoomShape shape, bool allowOverlap, int maxModifications = 20)
        {
            var width = UnityEngine.Random.Range(minSize, maxSize + 1);
            var height = UnityEngine.Random.Range(minSize, maxSize + 1);
            if (width <= 2)
                height = maxSize;
            else if (height <= 2)
                width = maxSize;

            return SpawnRoom(new Tuple<int, int>(width, height), shape, allowOverlap, maxModifications);
        }

        public DungeonDomainBuilder SpawnRoom(Tuple<int, int> size, DungeonRoomShape shape, bool allowOverlap, int maxModifications = 20)
        {
            var width = size.Item1;
            var height = size.Item2;

            if (shape == DungeonRoomShape.Random)
            {
                shape = (DungeonRoomShape)UnityEngine.Random.Range((int)DungeonRoomShape.Rectangular, (int)DungeonRoomShape.Round + 1);
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
                        CenterX = 0,
                        CenterY = 0,
                        Theme = context.CurrentTheme,
                        MaxModifications = maxModifications,
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
                                direction = validExits[UnityEngine.Random.Range(0, validExits.Count)];
                            }
                            else
                            {
                                direction = (BuilderDirection)UnityEngine.Random.Range((int) BuilderDirection.North, (int) BuilderDirection.West + 1);
                            }
                        }

                        var halfWidth = width / 2;
                        var halfHeight = height / 2;
                        var parentHalfWidth = parentRoom.Width / 2;
                        var parentHalfHeight = parentRoom.Height / 2;

                        switch (direction)
                        {
                            case BuilderDirection.North:
                                {
                                    room.X = parentRoom.X;
                                    room.Y = parentRoom.Y - 1;
                                    room.CenterX = parentRoom.CenterX;
                                    room.CenterY = parentRoom.CenterY - parentHalfHeight - halfHeight;
                                } break;
                            case BuilderDirection.East:
                                {
                                    room.X = parentRoom.X + 1;
                                    room.Y = parentRoom.Y;
                                    room.CenterX = parentRoom.CenterX + parentHalfWidth + halfWidth;
                                    room.CenterY = parentRoom.CenterY;
                                } break;
                            case BuilderDirection.South:
                                {
                                    room.X = parentRoom.X;
                                    room.Y = parentRoom.Y + 1;
                                    room.CenterX = parentRoom.CenterX;
                                    room.CenterY = parentRoom.CenterY + parentHalfHeight + halfHeight;
                                } break;
                            case BuilderDirection.West:
                                {
                                    room.X = parentRoom.X - 1;
                                    room.Y = parentRoom.Y;
                                    room.CenterX = parentRoom.CenterX - parentHalfWidth - halfWidth;
                                    room.CenterY = parentRoom.CenterY;
                                } break;
                        }

                        parentRoom.Connections.Add(new DungeonRoomConnectionMeta
                        {
                            From = parentRoom,
                            To = room,
                            Direction = direction,
                        });
                        room.Connections.Add(new DungeonRoomConnectionMeta
                        {
                            From = room,
                            To = parentRoom,
                            Direction = Opposite(direction),
                        });

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

        private BuilderDirection Opposite(BuilderDirection direction)
        {
            switch(direction)
            {
                case BuilderDirection.North:
                    return BuilderDirection.South;
                case BuilderDirection.East:
                    return BuilderDirection.West;
                case BuilderDirection.South:
                    return BuilderDirection.North;
                case BuilderDirection.West:
                    return BuilderDirection.East;
            }

            return BuilderDirection.Portal;
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
                    Condition("Player needs spawn point",
                        context => context.HasState(DungeonWorldState.PlayerNeedsSpawnPoint));
                    Do((context) =>
                    {
                        var parentRoom = context.RoomStack.Peek();
                        if (parentRoom != null)
                        {
                            Debug.Log($"Spawn player in room {parentRoom.Id}\n");
                            var playerSpawn = new DungeonSpawnMeta
                            {
                                SpawnRoom = parentRoom,
                            };
                            context.PlayerSpawnMeta = playerSpawn;
                            return TaskStatus.Success;
                        }

                        Debug.Log($"Error! Can't spawn player in the void!\n");
                        return TaskStatus.Failure;
                    });
                    Effect("Player spawned", EffectType.PlanAndExecute,
                        (context, type) => context.SetState(DungeonWorldState.PlayerNeedsSpawnPoint, false, type));
                }
                End();
            }
            End();
            return this;
        }

        public DungeonDomainBuilder TrySpawnNpc(string race, string name)
        {
            Optionally();
            {
                Action($"Try spawn npc");
                {
                    Do((context) =>
                    {
                        var parentRoom = context.RoomStack.Peek();
                        if (parentRoom != null)
                        {
                            Debug.Log($"Spawn npc {race}/{name} in room {parentRoom.Id}\n");
                            var npcSpawn = new DungeonSpawnNpcMeta()
                            {
                                SpawnRoom = parentRoom,
                                Race = race,
                                Name = name,
                            };
                            context.NpcSpawnMeta.Add(npcSpawn);
                            return TaskStatus.Success;
                        }

                        Debug.Log($"Error! Can't spawn npc in the void!\n");
                        return TaskStatus.Failure;
                    });
                }
                End();
            }
            End();
            return this;
        }

        public DungeonDomainBuilder Random()
        {
            return this.RandomSelect<DungeonDomainBuilder, DungeonContext>("Random");
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

        public DungeonDomainBuilder AddDecoration(DungeonTheme theme, TextAsset textAsset)
        {
            if (textAsset == null)
                return this;

            Action($"Add decoration");
            {
                Do((context) =>
                {
                    var parentRoom = context.RoomStack.Peek();
                    if (parentRoom != null)
                    {
                        Debug.Log($"Spawn decoration {textAsset.name} in room {parentRoom.Id}\n");
                        var decorationMeta = new DungeonRoomDecorationMeta();
                        decorationMeta.Generate(theme, textAsset);
                        parentRoom.DecorationMeta.Add(decorationMeta);
                        return TaskStatus.Success;
                    }

                    Debug.Log($"Error! Can't spawn decoration in the void!\n");
                    return TaskStatus.Failure;
                });
            }
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
                    Random();
                    {
                        SpawnRoom(new Tuple<int, int>(2, 8), DungeonRoomShape.Rectangular, false);
                        SpawnRoom(new Tuple<int, int>(8, 2), DungeonRoomShape.Rectangular, false);
                    }
                    End();
                    SpawnRoom(4, 8, DungeonRoomShape.Rectangular, false);
                    {
                        TrySpawnPlayer();
                    }
                    SpawnRoom(2, 6, DungeonRoomShape.Random, false);
                    SpawnRoom(2, 12, DungeonRoomShape.Random, false);
                    Repeat(10);
                    {
                        SpawnRoom(2, 8, DungeonRoomShape.Random, false);
                    }
                    EndRepeat();
                    Random();
                    {
                        SpawnRoom(new Tuple<int, int>(2, 8), DungeonRoomShape.Rectangular, false);
                        SpawnRoom(new Tuple<int, int>(8, 2), DungeonRoomShape.Rectangular, false);
                    }
                    End();
                    {
                        AddDecoration(DungeonTheme.Cave, _decorations.Find(DungeonTheme.Cave, DecorationType.Altar));
                        TrySpawnNpc("kobold", "warrior");
                    }
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
                }
            }
            End();
            return this;
        }
    }
}