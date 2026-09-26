using System.Collections.Generic;

namespace Iternia.Scripts.Core;

public class Formation
{
    private readonly Dictionary<GridPos, string> _slots = new();
    
    public TargetSide Side { get; }

    public Formation(TargetSide side)
    {
        Side = side;
    }

    public bool PlaceUnit(string unitId, GridPos pos)
    {
        if (pos.collum < 0 || pos.collum > 2 || pos.row < 0 || pos.row > 2)
            return false;
            
        if (_slots.ContainsKey(pos))
            return false;

        _slots[pos] = unitId;
        return true;
    }

    public void RemoveUnit(GridPos pos)
    {
        _slots.Remove(pos);
    }

    public string GetUnitAt(GridPos pos)
    {
        return _slots.TryGetValue(pos, out var unitId) ? unitId : null;
    }

    public bool MoveUnit(GridPos from, GridPos to)
    {
        string unitId = GetUnitAt(from);
        
        if (unitId == null || GetUnitAt(to) != null)
            return false;

        RemoveUnit(from);
        return PlaceUnit(unitId, to);
    }
}

