using Godot;
using System.Collections.Generic;
using Iternia.Scripts.Characters;
using Iternia.Scripts.Core;
using Iternia.Scripts.Models;
using Iternia.View;

namespace Iternia.Manager;

public partial class BattleManager : Node
{
    [Export] public GridView PlayerGrid { get; set; }
    [Export] public GridView EnemyGrid { get; set; }
    [Export] public PackedScene UnitViewScene { get; set; }

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

        SpawnTestUnit("player_1", 1, TargetSide.Ally, new GridPos(1, 1), 12);
        SpawnTestUnit("player_2", 1,TargetSide.Ally, new GridPos(2, 1), 11);
        SpawnTestUnit("enemy_1", 1,TargetSide.Enemy, new GridPos(0, 1), 10);

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

    private void SpawnTestUnit(string id, int attack, TargetSide side, GridPos pos, int speed)
    {
        Unit unit = new TestUnit(id, side, speed);
        _state.AllUnits[id] = unit;

        Formation formation;
        if (side == TargetSide.Ally)
        {
            formation = _state.PlayerFormation;
        }
        else
        {
            formation = _state.EnemyFormation;
        }
        formation.PlaceUnit(id, pos);

        if (UnitViewScene != null)
        {
            var view = UnitViewScene.Instantiate<UnitView>();
            AddChild(view);
            
            GridView grid;
            if (side == TargetSide.Ally)
            {
                grid = PlayerGrid;
            }
            else
            {
                grid = EnemyGrid;
            }
            
            Vector2 screenPos = grid.Position + grid.GetTileScreenPos(pos);
            
            view.Setup(id, screenPos);
            _unitViews[id] = view;
        }
    }

    private void HandleTileClicked(TargetSide clickedSide, GridPos clickedPos)
    {
        GD.Print($"side: {clickedSide} position: {clickedPos.Rank}, {clickedPos.Lane}");

        if (string.IsNullOrEmpty(_state.ActiveUnitId)) return;
        if (!_state.AllUnits.TryGetValue(_state.ActiveUnitId, out var activeUnit)) return;

        if (activeUnit.Side != TargetSide.Ally)
        {
            GD.PrintErr("Not your turn!");
            return;
        }

        if (clickedSide != activeUnit.Side)
        {
            GD.PrintErr("Invalid move: cannot move to enemy board");
            return;
        }

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
                GD.Print($"SUCCESS! Unit {moved.UnitId} moved to {moved.To.Rank},{moved.To.Lane}");
                
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
            }
            else if (evt is TurnStartedEvent turnStarted)
            {
                GD.Print($"Turn started for: {turnStarted.UnitId}");

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
            }
        }
    }
}
