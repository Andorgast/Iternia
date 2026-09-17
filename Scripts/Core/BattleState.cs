using System.Collections.Generic;

namespace Iternia.Scripts.Core;

public class BattleState
{
    public Formation PlayerFormation { get; } = new(TargetSide.Ally);
    public Formation EnemyFormation { get; } = new(TargetSide.Enemy);
    public Dictionary<string, Unit> AllUnits { get; } = new();
    public TurnBudget CurrentTurnBudget { get; } = new();
    public string ActiveUnitId { get; set; }
    public Queue<string> TurnQueue { get; } = new();
}
