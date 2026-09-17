using System.Collections.Generic;

namespace Iternia.Scripts.Core;

public class BattleSim
{
    private readonly TurnOrder _turnOrder;

    public BattleSim(TurnOrder turnOrder)
    {
        _turnOrder = turnOrder;
    }

    public ValidationResult Validate(BattleState state, BattleCommand cmd)
    {
        switch (cmd)
        {
            case MoveCommand moveCmd:
                if (!MovementRules.CanMove(state, moveCmd.UnitId, moveCmd.TargetPos))
                {
                    return new ValidationResult(false, "Invalid move: destination occupied, out of bounds, or out of moves.");
                }
                return new ValidationResult(true);

            case EndTurnCommand endTurnCmd:
                return new ValidationResult(true);

            default:
                return new ValidationResult(false, "Unknown command.");
        }
    }

    public IReadOnlyList<BattleEvent> Execute(BattleState state, BattleCommand cmd)
    {
        var events = new List<BattleEvent>();
        
        var validation = Validate(state, cmd);
        if (!validation.IsValid)
        {
            events.Add(new CommandFailedEvent(validation.ErrorMessage));
            return events;
        }

        switch (cmd)
        {
            case MoveCommand moveCmd:
                ExecuteMove(state, moveCmd, events);
                break;

            case EndTurnCommand endTurnCmd:
                ExecuteEndTurn(state, endTurnCmd, events);
                break;
        }

        return events;
    }

    private void ExecuteMove(BattleState state, MoveCommand cmd, List<BattleEvent> events)
    {
        MovementRules.TryFindUnitPosition(state, cmd.UnitId, out Formation formation, out GridPos currentPos);
        
        formation.MoveUnit(currentPos, cmd.TargetPos);
        state.CurrentTurnBudget.SpendMove();

        events.Add(new UnitMovedEvent(cmd.UnitId, currentPos, cmd.TargetPos));
    }

    private void ExecuteEndTurn(BattleState state, EndTurnCommand cmd, List<BattleEvent> events)
    {
        events.Add(new TurnEndedEvent(cmd.UnitId));
        
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
