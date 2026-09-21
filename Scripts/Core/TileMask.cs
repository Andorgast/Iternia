using System;
using System.Collections.Generic;
using Godot;

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
    
    public static TileMask Parse(Basis mask)
    {
        List<GridPos> positions = [];
        if (mask.Row0.X != 0) positions.Add(new GridPos(0,0));
        if (mask.Row1.X != 0) positions.Add(new GridPos(1,0));
        if (mask.Row2.X != 0) positions.Add(new GridPos(2,0));
        if (mask.Row0.Y != 0) positions.Add(new GridPos(0,1));
        if (mask.Row1.Y != 0) positions.Add(new GridPos(1,1));
        if (mask.Row2.Y != 0) positions.Add(new GridPos(2,1));
        if (mask.Row0.Z != 0) positions.Add(new GridPos(0,2));
        if (mask.Row1.Z != 0) positions.Add(new GridPos(1,2));
        if (mask.Row2.Z != 0) positions.Add(new GridPos(2,2));
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
