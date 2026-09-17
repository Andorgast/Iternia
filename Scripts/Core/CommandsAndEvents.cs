namespace Iternia.Scripts.Core;

public record ValidationResult(bool IsValid, string ErrorMessage = "");

// COMMANDS
public abstract record BattleCommand;

public record MoveCommand(string UnitId, GridPos TargetPos) : BattleCommand;
public record EndTurnCommand(string UnitId) : BattleCommand;


// EVENTS
public abstract record BattleEvent;

public record UnitMovedEvent(string UnitId, GridPos From, GridPos To) : BattleEvent;
public record TurnEndedEvent(string UnitId) : BattleEvent;
public record TurnStartedEvent(string UnitId) : BattleEvent;
public record CommandFailedEvent(string Reason) : BattleEvent;
