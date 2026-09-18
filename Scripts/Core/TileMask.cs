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
        var lines = mask.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length != 3)
            throw new ArgumentException("TileMask string must have exactly 3 rows separated by '/'.", nameof(mask));

        for (int lane = 0; lane < 3; lane++)
        {
            if (lines[lane].Length != 3)
                throw new ArgumentException("Each row in TileMask string must have exactly 3 characters.", nameof(mask));

            for (int rank = 0; rank < 3; rank++)
            {
                if (lines[lane][rank] != '.')
                {
                    positions.Add(new GridPos(rank, lane));
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
