using System;
using System.Collections.Generic;

namespace Iternia.Scripts.Core;

public class TileMask
{
    private readonly HashSet<GridPos> _positions;

    public TileMask(IEnumerable<GridPos> positions)
    {
        _positions = new HashSet<GridPos>(positions);
    }

    public static TileMask Parse(string mask)
    {
        var positions = new List<GridPos>();
        var rows = mask.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (rows.Length != 3)
            throw new ArgumentException("TileMask string must have exactly 3 rows separated by '/'.", nameof(mask));

        for (int collum = 0; collum < 3; collum++)
        {
            if (rows[collum].Length != 3)
                throw new ArgumentException("Each row in TileMask string must have exactly 3 characters.", nameof(mask));

            for (int rank = 0; rank < 3; rank++)
            {
                if (rows[collum][rank] != '.')
                {
                    positions.Add(new GridPos(rank, collum));
                }
            }
        }
        
        return new TileMask(positions);
    }

    public bool HasPos(GridPos pos)
    {
        return _positions.Contains(pos);
    }

    public IEnumerable<GridPos> GetPositions()
    {
        return _positions;
    }

    public TileMask Intersect(TileMask other)
    {
        var combined = new HashSet<GridPos>(_positions);
        combined.IntersectWith(other._positions);
        return new TileMask(combined);
    }
}
