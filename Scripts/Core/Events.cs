using System.Collections.Generic;

namespace Iternia.Scripts.Core;

public abstract record BattleEvent;

public record UnitMovedEvent(string UnitId, GridPos From, GridPos To) : BattleEvent;
public record TurnEndedEvent(string UnitId) : BattleEvent;
public record TurnStartedEvent(string UnitId) : BattleEvent;
public record CommandFailedEvent(string Reason) : BattleEvent;
public record TurnOrderChangedEvent(List<string> OrderUnitIds, string ActiveUnitId, List<string> RemainingThisRound) : BattleEvent;
public record EnemyAbilityChosenEvent(string UnitId, string ActionId, GridPos EnemyPos) : BattleEvent;