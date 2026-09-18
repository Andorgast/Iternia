using System.Collections.Generic;

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

    public IReadOnlyList<BattleEvent> TrySpawnTileEffect(BattleState state, TargetSide side, GridPos pos, TileEffectType effectType, int durationInTurns)
    {
        var events = new List<BattleEvent>();

        state.ActiveTileEffects.Add(new TileEffect(side, pos, effectType, durationInTurns));

        events.Add(new TileEffectSpawnedEvent(side, pos, effectType));

        return events;
    }

    public IReadOnlyList<BattleEvent> EndTurn(BattleState state, string unitId)
    {
        var events = new List<BattleEvent>();
        events.Add(new TurnEndedEvent(unitId));
        AdvanceTurn(state, events);
        return events;
    }

    private void AdvanceTurn(BattleState state, List<BattleEvent> events)
    {
        if (state.TurnQueue.Count == 0)
        {
            var newOrder = _turnOrder.CalculateRoundOrder(state);
            foreach (var id in newOrder)
            {
                state.TurnQueue.Enqueue(id);
            }
        }

        if (state.TurnQueue.Count > 0)
        {
            string nextUnitId = state.TurnQueue.Dequeue(); 
            
            state.ActiveUnitId = nextUnitId;
            state.CurrentTurnBudget.Reset();
            
            events.Add(new TurnStartedEvent(nextUnitId));
        }
    }
}
