using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExtension
{
    public static Vector3Int OffsetCoord2CubeCoord(this Vector2Int coord)
    {
        Vector2Int axialCoord = coord.OffsetCoord2AxialCoord();
        return new Vector3Int(axialCoord.x, axialCoord.y, -axialCoord.x - axialCoord.y);
    }

    public static Vector2Int OffsetCoord2AxialCoord(this Vector2Int coord)
    {
        return new Vector2Int(coord.x - Mathf.FloorToInt(coord.y / 2.0f), coord.y);
    }

    public static Vector2Int CubeCoord2OffsetCoord(this Vector3Int coord)
    {
        return new Vector2Int(coord.x + Mathf.FloorToInt(coord.y / 2.0f), coord.y);
    }

    // This is wrong right now, fk
    //     _1_
    //  2 /   \ 0  
    //  3 \___/ 5
    //      4
    private static Vector3Int[] s_HexagonalNeighborsOffsets =
        new Vector3Int[] {
            new Vector3Int(0, 1, -1),
            new Vector3Int(1, 0, -1),
            new Vector3Int(1, -1, 0),

            new Vector3Int(0, -1, 1),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(-1, 1, 0)
        };

    public static HexagonalTileNeighborEnumerable GetHexagonalTileNeighborsFromOffsetCoord(this Vector2Int center, int depth = 1)
    {
        if (depth <= 0)
        {
            throw new System.ArgumentOutOfRangeException("depth", "Depth should be equal or greater than 1.");
        }

        return new HexagonalTileNeighborEnumerable(center, depth);
    }

    public static Vector2Int GetHexagonalTileNeighborFromOffsetCoord(this Vector2Int center, int neighborIndex)
    {
        return (s_HexagonalNeighborsOffsets[neighborIndex] + center.OffsetCoord2CubeCoord()).CubeCoord2OffsetCoord();
    }

    public class HexagonalTileNeighborEnumerable : IEnumerable<Vector2Int>
    {
        private Vector2Int m_Center;
        private float m_Depth;

        public HexagonalTileNeighborEnumerable(Vector2Int center, int depth)
        {
            m_Center = center;
            m_Depth = depth;
        }

        class VisitPair
        {
            public Vector2Int pos;
            public int depth;
        }

        public IEnumerator<Vector2Int> GetEnumerator()
        {
            if (m_Depth == 1)
            {
                for (int i = 0; i < 6; i++)
                {
                    yield return (s_HexagonalNeighborsOffsets[i] + m_Center.OffsetCoord2CubeCoord()).CubeCoord2OffsetCoord();
                }
            }
            else
            {
                var visited = new HashSet<Vector2Int>();
                var queue = new Queue<VisitPair>();

                var pair = new VisitPair();
                pair.pos = m_Center;
                pair.depth = 0;
                queue.Enqueue(pair);

                while (queue.Count > 0)
                {
                    var visitPair = queue.Dequeue();

                    if (visited.Contains(visitPair.pos))
                    {
                        continue;
                    }

                    if (m_Center != visitPair.pos)
                    {
                        yield return visitPair.pos;
                    }
                    visited.Add(visitPair.pos);

                    foreach (var neighbor in visitPair.pos.GetHexagonalTileNeighborsFromOffsetCoord(1))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            var newDepth = visitPair.depth + 1;
                            if (newDepth <= m_Depth)
                            {
                                var newPair = new VisitPair();
                                newPair.pos = neighbor;
                                newPair.depth = newDepth;
                                queue.Enqueue(newPair);
                            }
                        }
                    }
                }
                
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
