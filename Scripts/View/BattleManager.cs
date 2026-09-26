using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using Iternia.Scripts.Core;
using Iternia.View;

using Iternia.Scripts.Models;
using Iternia.Scripts.View;
using Action = System.Action;

namespace Iternia.Manager;

public partial class BattleManager : Node
{
    [Export] public GridView PlayerGrid { get; set; }
    [Export] public GridView EnemyGrid { get; set; }
    [Export] public PackedScene UnitViewScene { get; set; }
    [Export] public Unit Player1Resource { get; set; }
    [Export] public Unit Player2Resource { get; set; }
    [Export] public Unit EnemyResource { get; set; }

    [Export] public ButtonManager ButtonManager;

    private BattleState _state;
    private BattleSim _sim;
    private Iternia.Scripts.Models.Action _currentAction;
    private string _enemyToTarget;
    
    private readonly System.Collections.Generic.Dictionary<string, UnitView> _unitViews = new();

    public override void _Ready()
    {
        _state = new BattleState();
        var turnOrder = new TurnOrder();
        _sim = new BattleSim(turnOrder);

        if (PlayerGrid != null) PlayerGrid.OnTileClicked += HandleTileClicked;
        if (EnemyGrid != null) EnemyGrid.OnTileClicked += HandleTileClicked;
        if (ButtonManager != null) ButtonManager.OnButtonClicked += HandleActionPressed;

        SpawnUnitFromResource(Player1Resource, TargetSide.Ally, new GridPos(1, 1));
        SpawnUnitFromResource(Player2Resource, TargetSide.Ally, new GridPos(2, 1));
        SpawnUnitFromResource(EnemyResource, TargetSide.Enemy, new GridPos(0, 1));

        var startEvents = _sim.StartBattle(_state);
        ProcessEvents(startEvents);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept"))
        {
            if (!string.IsNullOrEmpty(_state.ActiveUnitId) &&
                _state.AllUnits.TryGetValue(_state.ActiveUnitId, out var activeUnit) &&
                activeUnit.Side == TargetSide.Ally)
            {
                GD.Print($"Player ended turn for {activeUnit.Id}");
                var events = _sim.EndTurn(_state, activeUnit.Id);
                ProcessEvents(events);
            }
        }
    }

    private void SpawnUnitFromResource(Unit battleUnit, TargetSide side, GridPos pos)
    {
        if (battleUnit == null) return;
        
        battleUnit.Init();

        GD.Print($"Battle Unit: {battleUnit.Movement}");


        // Unit battleUnit = new Unit();
        // battleUnit.Side = side;
        
        _state.AllUnits[battleUnit.Id] = battleUnit;
        Formation formation = side == TargetSide.Ally ? _state.PlayerFormation : _state.EnemyFormation;
        bool placed = formation.PlaceUnit(battleUnit.Id, pos);
        GD.Print($"Spawning '{battleUnit.Id}' on {side} at ({pos.collum},{pos.row}) -> placed={placed}");

        if (UnitViewScene != null)
        {
            var view = UnitViewScene.Instantiate<UnitView>();
            AddChild(view);
            
            GridView grid = side == TargetSide.Ally ? PlayerGrid : EnemyGrid;
            Vector2 screenPos = grid.Position + grid.GetTileScreenPos(pos);
            
            view.Setup(battleUnit.Id, screenPos);
            _unitViews[battleUnit.Id] = view;
        }
    }

    private void HandleTileClicked(TargetSide clickedSide, GridPos clickedPos)
    {
        GD.Print($"side: {clickedSide} position: {clickedPos.collum}, {clickedPos.row}");
        GD.Print($"  > ActiveUnitId='{_state.ActiveUnitId}'");

        if (string.IsNullOrEmpty(_state.ActiveUnitId)) return;
        if (!_state.AllUnits.TryGetValue(_state.ActiveUnitId, out var activeUnit))  return; 

        GD.Print($"  > activeUnit.Side={activeUnit.Side}, clickedSide={clickedSide}");

        if (_currentAction != null)
        {
            MovementRules.TryFindUnitPosition(_state, _state.ActiveUnitId, out Formation formation, out GridPos pos);
            if (
                (
                    _currentAction.TargetSquares.GetPositions().ToList().Contains(clickedPos) 
                    ||
                    _currentAction.AllyTargetRange == AllyTargetRange.Full 
                    || 
                    (
                        _currentAction.AllyTargetRange == AllyTargetRange.Self 
                        && 
                        clickedPos == pos
                    ) 
                    ||
                    (
                        _currentAction.AllyTargetRange == AllyTargetRange.NextToSelf 
                        && 
                        TileMask.SquaresToHit(TileMask.Parse(".x./x.x/.x."), pos).HasPos(clickedPos)
                    )
                ) 
                && 
                clickedSide == _currentAction.TargetSide 
                &&
                _currentAction.OriginSquares.GetPositions().ToList().Contains(pos)
            )
            {
                if (_currentAction.TargetType == TargetType.Tile)
                {
                    //TODO apply a tile effect
                    GD.Print("executed an tile action");
                    return;
                }
                string targetId = null;
                if (_currentAction.TargetSide == TargetSide.Ally) targetId = _state.PlayerFormation.GetUnitAt(clickedPos);
                else if (_currentAction.TargetSide == TargetSide.Enemy) targetId = _state.EnemyFormation.GetUnitAt(clickedPos);
                if(targetId != null)
                {
                    if (_currentAction.TargetType == TargetType.EnemyAndAlly && _currentAction.AllyTargetRange != AllyTargetRange.Self && _enemyToTarget != null)
                    {
                        string allyTilesToHighlight = null;
                        switch (_currentAction.AllyTargetRange)
                        {
                            case AllyTargetRange.Full:
                                allyTilesToHighlight = "xxx/xxx/xxx";
                                break;
                            case AllyTargetRange.DefinedByTargetGrid:
                                allyTilesToHighlight = _currentAction.TargetSquares.ToString();
                                break;
                            case AllyTargetRange.NextToSelf:
                                allyTilesToHighlight = TileMask.SquaresToHit(TileMask.Parse(".x./x.x/.x."), pos).ToString();
                                break;
                        }
                        PlayerGrid.HighlightTiles(TileMask.Parse(allyTilesToHighlight).GetPositions());
                        _enemyToTarget = targetId;
                        GD.Print("executed half an action");
                        return;
                    }
                    else if (_currentAction.TargetType == TargetType.EnemyAndAlly && _currentAction.AllyTargetRange == AllyTargetRange.Self)
                    {
                        ExecuteActionOnUnits(targetId, _currentAction.StatToChange, _currentAction.StatChangeAmount);
                        ExecuteActionOnUnits(_state.ActiveUnitId, _currentAction.SecondaryStatToChange, _currentAction.SecondaryStatChangeAmount);
                        _state.CurrentTurnBudget.SpendMainAction();
                        GD.Print("executed a self action");
                        return;
                    }
                    else if (_enemyToTarget != null)
                    {
                        ExecuteActionOnUnits(_enemyToTarget, _currentAction.StatToChange, _currentAction.StatChangeAmount);
                        ExecuteActionOnUnits(targetId, _currentAction.SecondaryStatToChange, _currentAction.SecondaryStatChangeAmount);
                        _state.CurrentTurnBudget.SpendMainAction();
                        GD.Print("executed an double action");
                        return;
                    }
                    
                    if (_currentAction.AoiSquares.GetPositions().Any())
                    {
                        foreach (var position in TileMask.SquaresToHit(_currentAction.AoiSquares, clickedPos).GetPositions())
                        {
                            string secondaryTarget = null;
                            if (_currentAction.TargetSide == TargetSide.Ally || _currentAction.TargetSide == TargetSide.Both) secondaryTarget = _state.PlayerFormation.GetUnitAt(position);
                            else if (_currentAction.TargetSide == TargetSide.Enemy) secondaryTarget = _state.EnemyFormation.GetUnitAt(position);
                            if (secondaryTarget != null) ExecuteActionOnUnits(secondaryTarget, _currentAction.SecondaryStatToChange, _currentAction.SecondaryStatChangeAmount);
                        }
                    }
                    ExecuteActionOnUnits(targetId, _currentAction.StatToChange, _currentAction.StatChangeAmount);
                    _state.CurrentTurnBudget.SpendMainAction();
                }
                GD.Print("Action execution should be done here");
                PlayerGrid.ClearHighlights();
                EnemyGrid.ClearHighlights();
                _currentAction = null;
                UpdateMovementHighlights();
                if(_state.CurrentTurnBudget.ActionPoints <= 0) ButtonManager.RemoveButtons();
                if (_state.CurrentTurnBudget.ActionPoints <= 0 && _state.CurrentTurnBudget.MoveSteps <= 0)
                {
                    GD.Print($"Player {_state.ActiveUnitId} turn ended automatically.");
                    var events = _sim.EndTurn(_state, _state.ActiveUnitId);
                    ProcessEvents(events);
                }
            }
            else
            {
                GD.PrintErr("Conditions for the attack not met!");
            }
            return;
        }

        if (activeUnit.Side != TargetSide.Ally) return; 
        if (clickedSide != activeUnit.Side) return; 

        IReadOnlyList<BattleEvent> events2 = _sim.TryMove(_state, activeUnit.Id, clickedPos);
        IReadOnlyList<BattleEvent> events3 = [];
        if(_state.CurrentTurnBudget.ActionPoints <= 0) ButtonManager.RemoveButtons();
        if (_state.CurrentTurnBudget.ActionPoints <= 0 && _state.CurrentTurnBudget.MoveSteps <= 0)
        {
            GD.Print($"Player {_state.ActiveUnitId} turn ended automatically.");
            events3 = _sim.EndTurn(_state, _state.ActiveUnitId);
        }
        ProcessEvents(events2);
        ProcessEvents(events3);
    }

    private void HandleActionPressed(string buttonId)
    {
        GD.Print($"Action {buttonId} was pressed");
        List<GridPos> targetSquares = [];
        if (_currentAction != _state.AllUnits[_state.ActiveUnitId].Actions[int.Parse(buttonId)])
        {
            _currentAction = _state.AllUnits[_state.ActiveUnitId].Actions[int.Parse(buttonId)];
            targetSquares = _currentAction.TargetSquares.GetPositions().ToList();
        }
        if (_currentAction.TargetSide == TargetSide.Ally)
        {
            PlayerGrid.ClearHighlights();
            PlayerGrid.HighlightTiles(targetSquares);
        }
        else
        {
            PlayerGrid.ClearHighlights();
            EnemyGrid.ClearHighlights();
            EnemyGrid.HighlightTiles(targetSquares);
        }

        if (targetSquares.Count == 0)
        {
            _currentAction = null;
            _enemyToTarget = null;
            UpdateMovementHighlights();
        }
        
    }

    private void ExecuteActionOnUnits(string unitId, Stat statToChange, float statChangeAmount)
    {
        switch (statToChange)
        {
            case Stat.None:
                break;
            case Stat.MaxHp:
                _state.AllUnits[unitId].MaxHp += (int)statChangeAmount;
                break;
            case Stat.Attack:
                _state.AllUnits[unitId].Attack += statChangeAmount;
                break;
            case Stat.Speed:
                _state.AllUnits[unitId].Speed += statChangeAmount;
                break;
            case Stat.Aggro:
                _state.AllUnits[unitId].Aggro += statChangeAmount;
                break;
            case Stat.MaxMovement:
                _state.AllUnits[unitId].MaxMovement += (int)statChangeAmount;
                break;
            case Stat.Movement:
                _state.AllUnits[unitId].Movement += (int)statChangeAmount;
                break;
            case Stat.MaxActionPoints:
                _state.AllUnits[unitId].MaxActionPoints += (int)statChangeAmount;
                break;
            case Stat.ActionPoints:
                _state.AllUnits[unitId].ActionPoints += (int)statChangeAmount;
                break;
        }
        _state.AllUnits[unitId].Hp += (int)MathF.Round(_currentAction.Damage * _state.AllUnits[_state.ActiveUnitId].Attack);
    }

    private void ProcessEvents(IReadOnlyList<BattleEvent> events)
    {
        foreach (var evt in events)
        {
            if (evt is CommandFailedEvent fail)
            {
                GD.PrintErr($"Failed: {fail.Reason}");
            }
            else if (evt is UnitMovedEvent moved)
            {
                GD.Print($"SUCCESS! Unit {moved.UnitId} moved to {moved.To.collum},{moved.To.row}");
                
                if (_unitViews.TryGetValue(moved.UnitId, out var view))
                {
                    GridView grid = PlayerGrid;
                    if (_state.AllUnits.TryGetValue(moved.UnitId, out var unit) && unit.Side == TargetSide.Enemy)
                    {
                        grid = EnemyGrid;
                    }

                    Vector2 newScreenPos = grid.Position + grid.GetTileScreenPos(moved.To);
                    view.MoveTo(newScreenPos);
                }

                UpdateMovementHighlights();
            }
            else if (evt is TurnStartedEvent turnStarted)
            {
                GD.Print($"Turn started for: {turnStarted.UnitId}");
                GD.Print($"DEBUG Budget: MoveSteps={_state.CurrentTurnBudget.MoveSteps}, AP={_state.CurrentTurnBudget.ActionPoints}");

                UpdateMovementHighlights();
                ButtonManager.GenerateButtons(_state.AllUnits[turnStarted.UnitId].Actions);

                if (_state.AllUnits.TryGetValue(turnStarted.UnitId, out var unit) && unit.Side == TargetSide.Enemy)
                {
                    GD.Print($"Enemy {unit.Id} turn skipped automatically.");
                    var nextEvents = _sim.EndTurn(_state, unit.Id);
                    ProcessEvents(nextEvents);
                }
            }
            else if (evt is TurnEndedEvent turnEnded)
            {
                GD.Print($"Turn ended for: {turnEnded.UnitId}");
                _currentAction = null;
                PlayerGrid?.ClearHighlights();
                EnemyGrid?.ClearHighlights();
                ButtonManager.RemoveButtons();
            }
        }
    }

    private void UpdateMovementHighlights()
    {
        PlayerGrid?.ClearHighlights();
        EnemyGrid?.ClearHighlights();

        if (string.IsNullOrEmpty(_state.ActiveUnitId)) return;
        if (!_state.AllUnits.TryGetValue(_state.ActiveUnitId, out var activeUnit)) return;
        if (activeUnit.Side != TargetSide.Ally) return;

        var validMoves = MovementRules.GetValidMoves(_state, activeUnit.Id);
        PlayerGrid?.HighlightTiles(validMoves);
    }

    // private async void SkipEnemyTurn(string unitId)
    // {
    //     // Wacht 0.5 sec zodat de beurt zichtbaar overgeslagen wordt
    //     await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
    //     var nextEvents = _sim.EndTurn(_state, unitId);
    //     ProcessEvents(nextEvents);
    // }
}
