using System;
using System.Collections.Generic;
using Iternia.Scripts.Models;

namespace Iternia.Scripts.Core;

public class BattleState
{
    public Formation PlayerFormation { get; } = new(TargetSide.Ally);
    public Formation EnemyFormation { get; } = new(TargetSide.Enemy);
    public Dictionary<string, Unit> AllUnits { get; } = new();
    public Unit ActiveUnit { get; set; }
    public Queue<Unit> TurnQueue { get; } = new();
    public List<string> TurnOrder { get; set; } = new();
}
