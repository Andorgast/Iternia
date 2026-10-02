using System;
using System.Collections.Generic;
using System.Linq;

namespace Iternia.Scripts.Core;

public class TurnOrder
{
    public List<string> CalculateRoundOrder(BattleState state)
    {
        var units = state.AllUnits.Values.ToList();

        units.Sort((a, b) => b.Speed.CompareTo(a.Speed));
        
        var order = new List<string>();
        foreach (var unit in units)
        {
            order.Add(unit.Id);
        }
        
        return order;
    }
}

