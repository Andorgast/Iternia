using Godot;
using System.Collections.Generic;
using Iternia.Scripts.Core;
using Iternia.View;

using Iternia.Scripts.Models;

namespace Iternia.Manager;

public partial class BattleManager : Node
{
    [Export] public GridView PlayerGrid { get; set; }
    [Export] public GridView EnemyGrid { get; set; }
    [Export] public PackedScene UnitViewScene { get; set; }
    [Export] public Unit Player1Resource { get; set; }
    [Export] public Unit Player2Resource { get; set; }
    [Export] public Unit EnemyResource { get; set; }

    private BattleState _state;
    private BattleSim _sim;
    
    private readonly Dictionary<string, UnitView> _unitViews = new();

    public override void _Ready()
    {
        _state = new BattleState();
        var turnOrder = new TurnOrder();
        _sim = new BattleSim(turnOrder);

        if (PlayerGrid != null) PlayerGrid.OnTileClicked += HandleTileClicked;
        if (EnemyGrid != null) EnemyGrid.OnTileClicked += HandleTileClicked;

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
        GD.Print($"Spawning '{battleUnit.Id}' on {side} at ({pos.row},{pos.collum}) -> placed={placed}");

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
        GD.Print($"side: {clickedSide} position: {clickedPos.row}, {clickedPos.collum}");
        GD.Print($"  > ActiveUnitId='{_state.ActiveUnitId}'");

        if (string.IsNullOrEmpty(_state.ActiveUnitId)) return;
        if (!_state.AllUnits.TryGetValue(_state.ActiveUnitId, out var activeUnit))  return; 

        GD.Print($"  > activeUnit.Side={activeUnit.Side}, clickedSide={clickedSide}");

        if (activeUnit.Side != TargetSide.Ally) return; 
        if (clickedSide != activeUnit.Side) return; 

        var events = _sim.TryMove(_state, activeUnit.Id, clickedPos);
        
        ProcessEvents(events);
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
                GD.Print($"SUCCESS! Unit {moved.UnitId} moved to {moved.To.row},{moved.To.collum}");
                
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
                PlayerGrid?.ClearHighlights();
                EnemyGrid?.ClearHighlights();
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
