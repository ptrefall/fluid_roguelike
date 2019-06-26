using System;
using System.Collections.Generic;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Dungeon;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Character.Sensory
{
    public class FieldOfViewSensor : ICharacterSensor
    {
        public SensorTypes Type { get; }

        private static readonly int4[] OctantTransform = new int4[]
        {
            new int4(1, 0, 0, 1), // 0 E-NE
            new int4(0, 1, 1, 0), // 1 NE-N
            new int4(0, -1, 1, 0), // 2 N-NW
            new int4(-1, 0, 0, 1), // 3 NW-W
            new int4(-1, 0, 0, -1), // 4 W-SW
            new int4(0, -1, -1, 0), // 5 SW-S
            new int4(0, 1, -1, 0), // 6 S-SE
            new int4(1, 0, 0, -1), // 7 SE-E
        };

        public void Tick(CharacterContext context)
        {
            context.FieldOfView.Clear();
            ComputeVisibility(context.Dungeon, context, context.Self.Position, context.Self.Sight);
            List<int2> extensions = null;
            foreach (var kvp in context.FieldOfView)
            {
                if (context.Dungeon.ValueMap.ContainsKey(kvp.Key))
                {
                    if (context.Dungeon.ValueMap[kvp.Key].Index == DungeonTile.Index.Wall)
                    {
                        continue;
                    }
                }

                var dir = CharacterController.VecToDirection(kvp.Key - context.Self.Position);
                switch (dir)
                {
                    case MoveDirection.N:
                        ExtendFOV(context.Dungeon, context, kvp.Key, BuilderDirection.North, ref extensions);
                        break;
                    case MoveDirection.E:
                        ExtendFOV(context.Dungeon, context, kvp.Key, BuilderDirection.East, ref extensions);
                        break;
                    case MoveDirection.S:
                        ExtendFOV(context.Dungeon, context, kvp.Key, BuilderDirection.South, ref extensions);
                        break;
                    case MoveDirection.W:
                        ExtendFOV(context.Dungeon, context, kvp.Key, BuilderDirection.West, ref extensions);
                        break;
                }
            }

            if (extensions != null)
            {
                foreach (var p in extensions)
                {
                    var distSq = math.distancesq(context.Self.Position, p);
                    context.FieldOfView.Add(p, distSq);
                }
            }
        }

        private void ExtendFOV(Dungeon.Dungeon dungeon, CharacterContext context, int2 key, BuilderDirection dir, ref List<int2> extensions)
        {
            var adjacentKey = DungeonRoom.GetAdjacentKey(dungeon, key, dir);
            if (context.FieldOfView.ContainsKey(adjacentKey) || (extensions != null && extensions.Contains(adjacentKey)))
                return;

            if (dungeon.ValueMap.ContainsKey(adjacentKey))
            {
                if (dungeon.ValueMap[adjacentKey].Index == DungeonTile.Index.Wall)
                {
                    if (extensions == null)
                    {
                        extensions = new List<int2>();
                    }

                    extensions.Add(adjacentKey);
                }
            }
        }

        public void Reset(CharacterContext context)
        {
            context.FieldOfView.Clear();
        }

        public static void ComputeVisibility(Dungeon.Dungeon dungeon, CharacterContext context, int2 origin, float viewRadius)
        {
            // Viewer's cell is always visible.
            context.FieldOfView.Add(origin, 0f);

            // Cast light into cells for each of 8 octants.
            //
            // The left/right inverse slope values are initially 1 and 0, indicating a diagonal
            // and a horizontal line.  These aren't strictly correct, as the view area is supposed
            // to be based on corners, not center points.  We only really care about one side of the
            // wall at the edges of the octant though.
            //
            // NOTE: depending on the compiler, it's possible that passing the octant transform
            // values as four integers rather than an object reference would speed things up.
            // It's much tidier this way though.
            for (int txidx = 0; txidx < OctantTransform.Length; txidx++)
            {
                CastLight(dungeon, context, origin, viewRadius, 1, 1.0f, 0.0f, OctantTransform[txidx]);
            }
        }

        /// <summary>
        /// Recursively casts light into cells.  Operates on a single octant.
        /// </summary>
        /// <param name="grid">The cell grid definition.</param>
        /// <param name="gridPosn">The player's position within the grid.</param>
        /// <param name="viewRadius">The view radius; can be a fractional value.</param>
        /// <param name="startColumn">Current column; pass 1 as initial value.</param>
        /// <param name="leftViewSlope">Slope of the left (upper) view edge; pass 1.0 as
        ///   the initial value.</param>
        /// <param name="rightViewSlope">Slope of the right (lower) view edge; pass 0.0 as
        ///   the initial value.</param>
        /// <param name="txfrm">Coordinate multipliers for the octant transform.</param>
        ///
        /// Maximum recursion depth is (Ceiling(viewRadius)).
        private static void CastLight(Dungeon.Dungeon dungeon, CharacterContext context, int2 originWorld, float viewRadius,
            int startColumn, float leftViewSlope, float rightViewSlope, int4 txfrm)
        {
            //Debug.Assert(leftViewSlope >= rightViewSlope);

            // Used for distance test.
            float viewRadiusSq = viewRadius * viewRadius;

            int viewCeiling = (int) Math.Ceiling(viewRadius);

            // Set true if the previous cell we encountered was blocked.
            bool prevWasBlocked = false;

            // As an optimization, when scanning past a block we keep track of the
            // rightmost corner (bottom-right) of the last one seen.  If the next cell
            // is empty, we can use this instead of having to compute the top-right corner
            // of the empty cell.
            float savedRightSlope = -1;

            var minDim = new int2(dungeon.GetMinX(), dungeon.GetMinY());
            var maxDim = new int2(dungeon.GetMaxX(), dungeon.GetMaxY());

            // Move to [0, ->] range
            var dimLocal = maxDim - minDim;

            // Offset origin to [0, ->] range too
            var originLocal = originWorld - minDim;

            // Outer loop: walk across each column, stopping when we reach the visibility limit.
            for (int currentCol = startColumn; currentCol <= viewCeiling; currentCol++)
            {
                int xc = currentCol;

                // Inner loop: walk down the current column.  We start at the top, where X==Y.
                //
                // TODO: we waste time walking across the entire column when the view area is narrow.
                // TODO: Experiment with computing the possible range of cells from the slopes, and
                // TODO: iterate over that instead.
                for (int yc = currentCol; yc >= 0; yc--)
                {
                    // Translate local coordinates to grid coordinates.  For the various octants
                    // we need to invert one or both values, or swap X for Y.
                    var pLocal = new int2(
                        originLocal.x + xc * txfrm.x + yc * txfrm.y,
                        originLocal.y + xc * txfrm.z + yc * txfrm.w);

                    // Range-check the values.  This lets us avoid the slope division for blocks
                    // that are outside the grid.
                    //
                    // Note that, while we will stop at a solid column of blocks, we do always
                    // start at the top of the column, which may be outside the grid if we're (say)
                    // checking the first octant while positioned at the north edge of the map.
                    if (pLocal.x < 0 || pLocal.x >= dimLocal.x || pLocal.y < 0 || pLocal.y >= dimLocal.y)
                    {
                        continue;
                    }

                    // Compute slopes to corners of current block.  We use the top-left and
                    // bottom-right corners.  If we were iterating through a quadrant, rather than
                    // an octant, we'd need to flip the corners we used when we hit the midpoint.
                    //
                    // Note these values will be outside the view angles for the blocks at the
                    // ends -- left value > 1, right value < 0.
                    float leftBlockSlope = (yc + 0.5f) / (xc - 0.5f);
                    float rightBlockSlope = (yc - 0.5f) / (xc + 0.5f);

                    // Check to see if the block is outside our view area.  Note that we allow
                    // a "corner hit" to make the block visible.  Changing the tests to >= / <=
                    // will reduce the number of cells visible through a corner (from a 3-wide
                    // swath to a single diagonal line), and affect how far you can see past a block
                    // as you approach it.  This is mostly a matter of personal preference.
                    if (rightBlockSlope > leftViewSlope)
                    {
                        // Block is above the left edge of our view area; skip.
                        continue;
                    }
                    else if (leftBlockSlope < rightViewSlope)
                    {
                        // Block is below the right edge of our view area; we're done.
                        break;
                    }

                    var pWorld = pLocal + minDim;

                    // This cell is visible, given infinite vision range.  If it's also within
                    // our finite vision range, light it up.
                    //
                    // To avoid having a single lit cell poking out N/S/E/W, use a fractional
                    // viewRadius, e.g. 8.5.
                    //
                    // TODO: we're testing the middle of the cell for visibility.  If we tested
                    //  the bottom-left corner, we could say definitively that no part of the
                    //  cell is visible, and reduce the view area as if it were a wall.  This
                    //  could reduce iteration at the corners.
                    float distSq = xc * xc + yc * yc;
                    if (distSq <= viewRadiusSq)
                    {
                        if (context.FieldOfView.ContainsKey(pWorld) == false)
                        {
                            context.FieldOfView.Add(pWorld, distSq);
                        }
                    }

                    var curBlocked = (dungeon.ValueMap.ContainsKey(pWorld) ? (dungeon.ValueMap[pWorld].Index == DungeonTile.Index.Wall) : true);

                    if (prevWasBlocked)
                    {
                        if (curBlocked)
                        {
                            // Still traversing a column of walls.
                            savedRightSlope = rightBlockSlope;
                        }
                        else
                        {
                            // Found the end of the column of walls.  Set the left edge of our
                            // view area to the right corner of the last wall we saw.
                            prevWasBlocked = false;
                            leftViewSlope = savedRightSlope;
                        }
                    }
                    else
                    {
                        if (curBlocked)
                        {
                            // Found a wall.  Split the view area, recursively pursuing the
                            // part to the left.  The leftmost corner of the wall we just found
                            // becomes the right boundary of the view area.
                            //
                            // If this is the first block in the column, the slope of the top-left
                            // corner will be greater than the initial view slope (1.0).  Handle
                            // that here.
                            if (leftBlockSlope <= leftViewSlope)
                            {
                                CastLight(dungeon, context, originWorld, viewRadius, currentCol + 1,
                                    leftViewSlope, leftBlockSlope, txfrm);
                            }

                            // Once that's done, we keep searching to the right (down the column),
                            // looking for another opening.
                            prevWasBlocked = true;
                            savedRightSlope = rightBlockSlope;
                        }
                    }
                }

                // Open areas are handled recursively, with the function continuing to search to
                // the right (down the column).  If we reach the bottom of the column without
                // finding an open cell, then the area defined by our view area is completely
                // obstructed, and we can stop working.
                if (prevWasBlocked)
                {
                    break;
                }
            }
        }
    }
}