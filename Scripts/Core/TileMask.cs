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
        if (mask.Column0.X != 0) positions.Add(new GridPos(0,0));
        if (mask.Column1.X != 0) positions.Add(new GridPos(1,0));
        if (mask.Column2.X != 0) positions.Add(new GridPos(2,0));
        if (mask.Column0.Y != 0) positions.Add(new GridPos(0,1));
        if (mask.Column1.Y != 0) positions.Add(new GridPos(1,1));
        if (mask.Column2.Y != 0) positions.Add(new GridPos(2,1));
        if (mask.Column0.Z != 0) positions.Add(new GridPos(0,2));
        if (mask.Column1.Z != 0) positions.Add(new GridPos(1,2));
        if (mask.Column2.Z != 0) positions.Add(new GridPos(2,2));
        return new TileMask(positions);
    }

    public static TileMask SquaresToHit(TileMask aoi, GridPos originPoint)
    {
        List<GridPos> posList = [];
        for (int collum = -1; collum < 2; collum++)
        {
            for (int row = -1; row < 2; row++)
            {
                if (originPoint.collum + collum >= 0 && originPoint.collum +collum <= 3
                    &&
                    originPoint.row + row >= 0 && originPoint.row <= 3
                    &&
                    aoi.HasPos(new GridPos(collum + 1, row + 1))
                )
                    posList.Add(new GridPos(originPoint.collum + collum, originPoint.row + row));
            }
        }
        return new TileMask(posList);
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

    public new string ToString()
    {
        string returnString = "";
        for (int collum = 0; collum < 3; collum++)
        {
            for (int row = 0; row < 3; row++)
            {
                if (_positions.Contains(new GridPos(row, collum))) returnString += "X";
                else returnString += ".";
            }
            returnString += "/";
        }
        return returnString.Remove(11);
    }
}
