using System.Collections.Generic;
using System.Linq;
using Iternia.Scripts.Models;

namespace Iternia.Scripts.Core;

public class BattleSim
{
    private readonly TurnOrder _turnOrder;
    private EnemyAI _enemyAi = new();

    public BattleSim(TurnOrder turnOrder)
    {
        _turnOrder = turnOrder;
    }

    public IReadOnlyList<BattleEvent> StartBattle(BattleState state)
    {
        var events = new List<BattleEvent>();
        AdvanceTurn(state, events);
        return events;
    }

    public IReadOnlyList<BattleEvent> TryMove(BattleState state, Unit unit, GridPos targetPos)
    {
        var events = new List<BattleEvent>();

        if (!MovementRules.CanMove(state, unit, targetPos))
        {
            events.Add(new CommandFailedEvent("Invalid move: destination occupied, out of bounds, or out of moves."));
            return events;
        }

        MovementRules.TryFindUnitPosition(state, unit.Name, out Formation formation, out GridPos currentPos);
        
        if (state.ActiveUnit.Movement - 1 >= 0) state.ActiveUnit.Movement--;
        else if (state.ActiveUnit.ActionPoints - 1 >= 0) state.ActiveUnit.ActionPoints--;
        else
        {
            events.Add(new CommandFailedEvent("Invalid move: destination occupied, out of bounds, or out of moves."));
            return events;
        }
        
        formation.MoveUnit(currentPos, targetPos);
        events.Add(new UnitMovedEvent(unit.Name, currentPos, targetPos));
        return events;
    }

    public IReadOnlyList<BattleEvent> ExecuteEnemyTurn(BattleState state, Models.EnemyUnit enemy)
    {
        var events = new List<BattleEvent>();

        var target = EnemyAI.PickMoveTarget(state, enemy);
        if (target.HasValue)
        {
            var moveEvents = TryMove(state, enemy, target.Value);
            foreach (var evt in moveEvents)
                events.Add(evt);
        }

        var abilityPick = _enemyAi.PickAbility(state, enemy);
        if (abilityPick.HasValue)
        {
            MovementRules.TryFindUnitPosition(state, enemy.Name, out Formation formation, out GridPos enemyPos);
            events.Add(new EnemyAbilityChosenEvent(enemy.Name, abilityPick.Value.Action, enemyPos,
                abilityPick.Value.TargetPos));
        }
        else events.AddRange(EndTurn(state, enemy.Name).ToList());

        return events;
    }
}
