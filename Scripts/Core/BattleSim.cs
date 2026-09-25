using System.Collections.Generic;
using System.Linq;

namespace Iternia.Scripts.Core;

public class BattleSim
{
    private readonly TurnOrder _turnOrder;

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

    public IReadOnlyList<BattleEvent> TryMove(BattleState state, string unitId, GridPos targetPos)
    {
        var events = new List<BattleEvent>();

        if (!MovementRules.CanMove(state, unitId, targetPos))
        {
            events.Add(new CommandFailedEvent("Invalid move: destination occupied, out of bounds, or out of moves."));
            return events;
        }

        MovementRules.TryFindUnitPosition(state, unitId, out Formation formation, out GridPos currentPos);
        formation.MoveUnit(currentPos, targetPos);
        state.CurrentTurnBudget.SpendMove();

        events.Add(new UnitMovedEvent(unitId, currentPos, targetPos));
        
        return events;
    }

    public IReadOnlyList<BattleEvent> EndTurn(BattleState state, string unitId)
    {
        var events = new List<BattleEvent>();
        events.Add(new TurnEndedEvent(unitId));
        AdvanceTurn(state, events);
        return events;
    }

    public IReadOnlyList<BattleEvent> ExecuteEnemyTurn(BattleState state, Models.EnemyUnit enemy)
    {
        var events = new List<BattleEvent>();

        var target = EnemyAI.PickMoveTarget(state, enemy);
        if (target.HasValue)
        {
            var moveEvents = TryMove(state, enemy.Id, target.Value);
            foreach (var evt in moveEvents)
                events.Add(evt);
        }

        // TODO: ability uitvoeren

        return events;
    }

    private void AdvanceTurn(BattleState state, List<BattleEvent> events)
    {
        if (state.TurnQueue.Count == 0)
        {
            var newOrder = _turnOrder.CalculateRoundOrder(state);
            state.CurrentRoundOrder = newOrder;
            foreach (var id in newOrder)
            {
                state.TurnQueue.Enqueue(id);
            }
        }

        if (state.TurnQueue.Count > 0)
        {
            string nextUnitId = state.TurnQueue.Dequeue(); 
            state.ActiveUnitId = nextUnitId;

            if (state.AllUnits.TryGetValue(nextUnitId, out var activeUnit))
            {
                state.CurrentTurnBudget.Reset(activeUnit);
            }

            events.Add(new TurnStartedEvent(nextUnitId));
            var remaining = new List<string>(state.TurnQueue);
            events.Add(new TurnOrderChangedEvent(state.CurrentRoundOrder, nextUnitId, remaining));
        }
        else
        {
            state.ActiveUnitId = null;
        }
    }
}
