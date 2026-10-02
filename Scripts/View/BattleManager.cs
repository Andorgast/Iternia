using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
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
    [Export] public EnemyUnit Enemy1Resource { get; set; }
    [Export] public EnemyUnit Enemy2Resource { get; set; }
    [Export] public TurnOrderHUD TurnOrderHUD{ get; set; }
    [Export] public UnitInfoPanel UnitInfoPanel { get; set; }
    [Export] public ButtonManager ButtonManager;

    private BattleState _state;
    private BattleSim _sim;
    private Scripts.Models.Action _currentAction;
    private string _enemyToTarget;
    private Dictionary<string, List<TileEffect>> _unitsToReverseTileEffects = new();
    private bool _combatEnded = false;
    
    private readonly Dictionary<string, UnitView> _unitViews = new();

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
        SpawnUnitFromResource(Enemy1Resource, TargetSide.Enemy, new GridPos(0, 1));
        SpawnUnitFromResource(Enemy2Resource, TargetSide.Enemy, new GridPos(2, 2));

        TurnOrderHUD?.Setup(_state.AllUnits);

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
            view.SetHealth(battleUnit.Hp, battleUnit.MaxHp);
            _unitViews[battleUnit.Id] = view;
        }
    }

    private bool TryExecuteAction(GridPos pos, GridPos clickedPos, TargetSide clickedSide)
    {
        if (_currentAction != null)
        {
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
                (
                    clickedSide == _currentAction.TargetSide 
                    ||
                    _currentAction.TargetSide == TargetSide.Both
                )
                &&
                _currentAction.OriginSquares.GetPositions().ToList().Contains(pos)
            )
            {
                if (_currentAction.TargetType == TargetType.Tile && _state.CurrentTurnBudget.SpendMainAction())
                {
                    foreach (GridPos tilePosition in TileMask.SquaresToHit(_currentAction.AoiSquares, clickedPos).GetPositions())
                    {
                        foreach (TileEffect tileEffect in _currentAction.TileEffects)
                        {
                            switch (clickedSide)
                            {
                                case TargetSide.Ally:
                                    PlayerGrid.AddTileEffect(tilePosition, tileEffect);
                                    break;
                                case TargetSide.Enemy:
                                    EnemyGrid.AddTileEffect(tilePosition, tileEffect);
                                    break;
                            }
                        }
                    }
                    GD.Print("executed an tile action");
                    UpdateBattleVisuals();
                    return true;
                }
                string targetId = null;
                if (_currentAction.TargetSide == TargetSide.Ally || (_currentAction.TargetSide == TargetSide.Both && _enemyToTarget != null)) targetId = _state.PlayerFormation.GetUnitAt(clickedPos);
                else if (_currentAction.TargetSide == TargetSide.Enemy || (_currentAction.TargetSide == TargetSide.Both && _enemyToTarget == null)) targetId = _state.EnemyFormation.GetUnitAt(clickedPos);
                if(targetId != null && TryIsUnitAlive(targetId))
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
                        EnemyGrid.ClearHighlights();
                        EnemyGrid.HighlightTiles([clickedPos], TileColorReason.Hover);
                        PlayerGrid.ClearHighlights();
                        PlayerGrid.HighlightTiles(TileMask.Parse(allyTilesToHighlight).GetPositions(), TileColorReason.TargetForAction);
                        _enemyToTarget = targetId;
                        GD.Print("executed half an action");
                        return true;
                    }
                    else if (_currentAction.TargetType == TargetType.EnemyAndAlly && _currentAction.AllyTargetRange == AllyTargetRange.Self)
                    {
                        if (_state.CurrentTurnBudget.SpendMainAction())
                        {
                            ExecuteActionOnUnits(targetId, _currentAction.StatToChange, _currentAction.StatChangeAmount, _currentAction.Damage);
                            ExecuteActionOnUnits(_state.ActiveUnitId, _currentAction.SecondaryStatToChange, _currentAction.SecondaryStatChangeAmount, _currentAction.SecondaryDamage);
                            GD.Print("executed a self action");
                        }
                        else GD.PrintErr("No actions left!");
                        UpdateBattleVisuals();
                        return true;
                    }
                    else if (_enemyToTarget != null)
                    {
                        if(_state.CurrentTurnBudget.SpendMainAction())
                        {
                            ExecuteActionOnUnits(_enemyToTarget, _currentAction.StatToChange,
                                _currentAction.StatChangeAmount, _currentAction.Damage);
                            ExecuteActionOnUnits(targetId, _currentAction.SecondaryStatToChange,
                                _currentAction.SecondaryStatChangeAmount, _currentAction.SecondaryDamage);
                            GD.Print("executed an double action");
                        }
                        else GD.PrintErr("No actions left!");
                        UpdateBattleVisuals();
                        return true;
                    }
                    
                    if (_currentAction.AoiSquares.GetPositions().Any())
                    {
                        foreach (var position in TileMask.SquaresToHit(_currentAction.AoiSquares, clickedPos).GetPositions())
                        {
                            string secondaryTarget = null;
                            if (_currentAction.TargetSide == TargetSide.Ally || _currentAction.TargetSide == TargetSide.Both) secondaryTarget = _state.PlayerFormation.GetUnitAt(position);
                            else if (_currentAction.TargetSide == TargetSide.Enemy) secondaryTarget = _state.EnemyFormation.GetUnitAt(position);
                            if (secondaryTarget != null) ExecuteActionOnUnits(secondaryTarget, _currentAction.SecondaryStatToChange, _currentAction.SecondaryStatChangeAmount, _currentAction.SecondaryDamage);
                        }
                    }

                    if (_state.CurrentTurnBudget.SpendMainAction())
                    {
                        ExecuteActionOnUnits(targetId, _currentAction.StatToChange, _currentAction.StatChangeAmount,
                            _currentAction.Damage);
                        GD.Print("Action execution should be done here");
                    }
                    else GD.PrintErr("No actions left!");
                    UpdateBattleVisuals();
                }
                else
                {
                    GD.PrintErr("No viable target found!");
                }
            }
            else
            {
                GD.PrintErr("Conditions for the attack not met!");
            }
            UpdateBattleVisuals();
        }
        return false;
    }

    private void HandleTileClicked(TargetSide clickedSide, GridPos clickedPos)
    {
        IsCombatDone();
        GD.Print($"side: {clickedSide} position: {clickedPos.collum}, {clickedPos.row}");
        GD.Print($"  > ActiveUnitId='{_state.ActiveUnitId}'");

        if (string.IsNullOrEmpty(_state.ActiveUnitId)) return;
        if (!_state.AllUnits.TryGetValue(_state.ActiveUnitId, out var activeUnit))  return; 

        GD.Print($"  > activeUnit.Side={activeUnit.Side}, clickedSide={clickedSide}");
        
        if (_currentAction != null)
        {
            MovementRules.TryFindUnitPosition(_state, _state.ActiveUnitId, out Formation formation, out GridPos pos);
            TryExecuteAction(pos, clickedPos, clickedSide);
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

    private bool TryIsUnitAlive(string unitId)
    {
        int hp;
        try
        {
            hp = _state.AllUnits[unitId].Hp;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
        if (hp > 0) return true;
        return false;
    }

    private void UpdateBattleVisuals()
    {
        PlayerGrid.ClearHighlights();
        EnemyGrid.ClearHighlights();
        _currentAction = null;
        UpdateMovementHighlights();
        UpdateUnitInfoPanel();
        if(_state.CurrentTurnBudget.ActionPoints <= 0) ButtonManager.RemoveButtons();
        if (_state.CurrentTurnBudget.ActionPoints <= 0 && _state.CurrentTurnBudget.MoveSteps <= 0)
        {
            GD.Print($"Player {_state.ActiveUnitId} turn ended automatically.");
            var events = _sim.EndTurn(_state, _state.ActiveUnitId);
            ProcessEvents(events);
        }
    }
    
    private void HandleActionPressed(string buttonId)
    {
        IsCombatDone();
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
            EnemyGrid.ClearHighlights();
            PlayerGrid.HighlightTiles(_currentAction.OriginSquares.GetPositions().ToList(), TileColorReason.OriginForAction);
            PlayerGrid.HighlightTiles(targetSquares, TileColorReason.TargetForAction);
            
        }
        else
        {
            PlayerGrid.ClearHighlights();
            EnemyGrid.ClearHighlights();
            PlayerGrid.HighlightTiles(_currentAction.OriginSquares.GetPositions().ToList(), TileColorReason.OriginForAction);
            EnemyGrid.HighlightTiles(targetSquares, TileColorReason.TargetForAction);
        }

        if (targetSquares.Count == 0)
        {
            _currentAction = null;
            _enemyToTarget = null;
            UpdateMovementHighlights();
        }
        
    }

    private void RemoveUnitFromTurnOrder(string idToRemove)
    {
        List<string> tempQueueAsList = _state.TurnQueue.ToList();
        tempQueueAsList.Remove(idToRemove);
        _state.TurnQueue.Clear();
        foreach (string unitId in tempQueueAsList) _state.TurnQueue.Enqueue(unitId);
        tempQueueAsList.Reverse();
        tempQueueAsList.Add(_state.ActiveUnitId);
        tempQueueAsList.Reverse();
        TurnOrderHUD?.Refresh(tempQueueAsList, _state.ActiveUnitId, _state.TurnQueue.ToList());
    }

    private void ExecuteActionOnUnits(string unitId, Stat statToChange, float statChangeAmount, int damage)
    {
        ApplyStatChange(unitId, statToChange, statChangeAmount);
        int oldHp = _state.AllUnits[unitId].Hp;
        int newHp = _state.AllUnits[unitId].TakeDamage((int)MathF.Round(damage * _state.AllUnits[_state.ActiveUnitId].Attack), _currentAction.ActionElement);
        
        if(oldHp != newHp)
        {
            _unitViews[unitId].SetHealth(newHp, _state.AllUnits[unitId].MaxHp);
            GD.Print($"Health changed to: {newHp} from {oldHp}");
        }

        if (newHp <= 0)
        {
            RemoveUnitFromTurnOrder(unitId);
            _state.AllUnits.Remove(unitId);
        }
        
        IsCombatDone();
    }

    private void IsCombatDone()
    {
        if (_combatEnded) return;

        bool playerAlive = false;
        bool enemyAlive = false;
        foreach (Unit unit in _state.AllUnits.Values)
        {
            switch (unit.Side)
            {
                case TargetSide.Ally:   playerAlive = true; break;
                case TargetSide.Enemy:  enemyAlive = true;  break;
            }
        }

        if (!enemyAlive)
        {
            _combatEnded = true;
            GD.Print("=== COMBAT WON! All enemies defeated. ===");
            // TODO: toon win-scherm
        }
        else if (!playerAlive)
        {
            _combatEnded = true;
            GD.Print("=== COMBAT LOST! All players defeated. ===");
            // TODO: toon lose-scherm
        }
    }

    private void ProcessEvents(IReadOnlyList<BattleEvent> events)
    {
        if (_combatEnded) return;
        IsCombatDone();
        if (_combatEnded) return;
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
                UpdateUnitInfoPanel();
            }
            else if (evt is TurnStartedEvent turnStarted)
            {
                GD.Print($"Turn started for: {turnStarted.UnitId}");
                GD.Print($"DEBUG Budget: MoveSteps={_state.CurrentTurnBudget.MoveSteps}, AP={_state.CurrentTurnBudget.ActionPoints}");

                UpdateMovementHighlights();
                UpdateUnitInfoPanel();
                ButtonManager.GenerateButtons(_state.AllUnits[turnStarted.UnitId].Actions);

                if (_state.AllUnits.TryGetValue(turnStarted.UnitId, out var unit) && unit.Side == TargetSide.Enemy)
                {
                    if (unit is EnemyUnit enemyUnit)
                    {
                        GD.Print($"Enemy {unit.Id} executing AI turn.");
                        var aiEvents = _sim.ExecuteEnemyTurn(_state, enemyUnit);
                        ProcessEvents(aiEvents);
                    }

                    var nextEvents = _sim.EndTurn(_state, unit.Id);
                    ProcessEvents(nextEvents);
                    return;
                }
            }
            else if (evt is TurnEndedEvent turnEnded)
            {
                GD.Print($"Turn ended for: {turnEnded.UnitId}");
                _currentAction = null;
                PlayerGrid?.ClearHighlights();
                EnemyGrid?.ClearHighlights();
                TryExecuteTileAction(PlayerGrid, _state.PlayerFormation);
                TryExecuteTileAction(EnemyGrid, _state.EnemyFormation);
                ButtonManager.RemoveButtons();
                UpdateUnitInfoPanel();
            }
            else if (evt is EnemyAbilityChosenEvent abilityChosen)
            {
                GD.Print($"Enemy {abilityChosen.UnitId} uses '{abilityChosen.ActionId}' targeting ({abilityChosen.TargetPos.row},{abilityChosen.TargetPos.collum})");

                var enemyAction = _state.AllUnits.TryGetValue(abilityChosen.UnitId, out var attacker)
                    ? attacker.Actions.FirstOrDefault(a => a.Id == abilityChosen.ActionId)
                    : null;

                if (enemyAction != null)
                {
                    MovementRules.TryFindUnitPosition(_state, abilityChosen.UnitId, out Formation formation, out GridPos pos);
                    _currentAction = enemyAction;
                    if (enemyAction.TargetSide == TargetSide.Ally) TryExecuteAction(pos , abilityChosen.TargetPos, TargetSide.Enemy);
                    else if (enemyAction.TargetSide == TargetSide.Enemy) TryExecuteAction(pos, abilityChosen.TargetPos, TargetSide.Ally);
                    else
                    {
                        TryExecuteAction(pos, abilityChosen.TargetPos, TargetSide.Ally);
                        //TODO get the propper secondary target
                        GridPos secondTarget = new GridPos(0, 0);
                        TryExecuteAction(pos, secondTarget, TargetSide.Enemy);
                    };

                    _currentAction = null;
                    UpdateUnitInfoPanel();
                }
            }
            else if (evt is TurnOrderChangedEvent turnOrder)
            {
                TurnOrderHUD?.Refresh(turnOrder.OrderUnitIds, turnOrder.ActiveUnitId, turnOrder.RemainingThisRound);
            }
        }
    }

    private void TryExecuteTileAction(GridView gridView, Formation formation)
    {
        //Reverses all temporary tile effects
        foreach (KeyValuePair<string, List<TileEffect>> unitId in _unitsToReverseTileEffects)
        {
            if(TryIsUnitAlive(unitId.Key))
            {
                foreach (TileEffect tileEffect in unitId.Value)
                {
                    tileEffect.StatChangeAmount *= -1;
                    ApplyStatChange(unitId.Key, tileEffect.StatToChange, tileEffect.StatChangeAmount);
                }

                if (_state.AllUnits[unitId.Key].Hp <= 0)
                {
                    RemoveUnitFromTurnOrder(unitId.Key);
                    _state.AllUnits.Remove(unitId.Key);
                }
            }
        }
        
        //Executes all current tile effects
        Dictionary<GridPos, List<TileEffect>> tileEffectsToTry = gridView.GetTilesWEffects();
        foreach (KeyValuePair<GridPos, List<TileEffect>> tileEffectPair in tileEffectsToTry)
        {
            string unitAtPos = formation.GetUnitAt(tileEffectPair.Key);
            if (unitAtPos != null && TryIsUnitAlive(unitAtPos))
            {
                int oldHp = _state.AllUnits[unitAtPos].Hp;
                foreach (TileEffect tileEffect in tileEffectPair.Value)
                {
                    ApplyStatChange(unitAtPos, tileEffect.StatToChange, tileEffect.StatChangeAmount);
                    if (tileEffect.Type == TileEffectType.TempStatChange)
                    {
                        if (!_unitsToReverseTileEffects.TryAdd(unitAtPos, [tileEffect])) _unitsToReverseTileEffects[unitAtPos].Add(tileEffect);
                    }
                }
                int newHp = _state.AllUnits[unitAtPos].Hp;
                if (oldHp != newHp)
                {
                    _unitViews[unitAtPos].SetHealth(newHp, _state.AllUnits[unitAtPos].MaxHp);
                }
                if (_state.AllUnits[unitAtPos].Hp <= 0)
                {
                    RemoveUnitFromTurnOrder(unitAtPos);
                    _state.AllUnits.Remove(unitAtPos);
                }
            }
        }
    }

    private void ApplyStatChange(string unitId, Stat statToChange, float statChangeAmount)
    {
        switch (statToChange)
        {
            case Stat.None:
                break;
            case Stat.MaxHp:
                if (_state.AllUnits[unitId].MaxHp + (int)statChangeAmount <= 0) _state.AllUnits[unitId].MaxHp = 1;
                else _state.AllUnits[unitId].MaxHp += (int)statChangeAmount;
                if (_state.AllUnits[unitId].MaxHp < _state.AllUnits[unitId].Hp) _state.AllUnits[unitId].Hp = _state.AllUnits[unitId].MaxHp;
                break;
            case Stat.Hp:
                if (_state.AllUnits[unitId].Hp + (int)statChangeAmount > _state.AllUnits[unitId].MaxHp) _state.AllUnits[unitId].Hp = _state.AllUnits[unitId].MaxHp;
                else _state.AllUnits[unitId].Hp += (int)statChangeAmount;
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
    }

    private void UpdateMovementHighlights()
    {
        PlayerGrid?.ClearHighlights();
        EnemyGrid?.ClearHighlights();

        if (string.IsNullOrEmpty(_state.ActiveUnitId)) return;
        if (!_state.AllUnits.TryGetValue(_state.ActiveUnitId, out var activeUnit)) return;
        if (activeUnit.Side != TargetSide.Ally) return;

        var validMoves = MovementRules.GetValidMoves(_state, activeUnit.Id);
        PlayerGrid?.HighlightTiles(validMoves, TileColorReason.MovePossible);
    }

    private void UpdateUnitInfoPanel()
    {
        if (UnitInfoPanel == null) return;
        
        if (!string.IsNullOrEmpty(_state.ActiveUnitId) && _state.AllUnits.TryGetValue(_state.ActiveUnitId, out var activeUnit))
        {
            UnitInfoPanel.UpdateInfo(activeUnit, _state.CurrentTurnBudget);
        }
        else
        {
            UnitInfoPanel.UpdateInfo(null, null); // Hide panel
        }
    }
}
