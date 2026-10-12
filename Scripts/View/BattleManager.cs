using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using Iternia.Scripts.Core;
using Iternia.View;

using Iternia.Scripts.Models;
using Iternia.Scripts.View;
using Action = System.Action;
using Range = Godot.Range;

namespace Iternia.Manager;

public partial class BattleManager : Node
{
    [Export] public GridView PlayerGrid { get; set; }
    [Export] public GridView EnemyGrid { get; set; }
    [Export] public PackedScene UnitViewScene { get; set; }
    [Export] public Unit Player1Resource { get; set; }
    [Export] public Unit Player2Resource { get; set; }
    [Export] public Unit Player3Resource { get; set; }
    [Export] public EnemyUnit Enemy1Resource { get; set; }
    [Export] public EnemyUnit Enemy2Resource { get; set; }
    [Export] public EnemyUnit Enemy3Resource { get; set; }
    [Export] public TurnOrderHUD TurnOrderHUD{ get; set; }
    [Export] public UnitInfoPanel UnitInfoPanel { get; set; }
    [Export ] public UnitAbilityInfoPanel UnitAbilityInfoPanel { get; set; }
    [Export] public ButtonManager ButtonManager;

    private BattleState _state;
    private Scripts.Models.Action _currentAction;
    private Unit _enemyToTarget;
    private Dictionary<Unit, List<TileEffect>> _unitsToReverseTileEffects = new();
    private bool _combatEnded;
    
    private readonly Dictionary<string, Node> _unitViews = new();
    private readonly Dictionary<string, Node> _unitHealthBars = new();

    public override void _Ready()
    {
        _state = new BattleState();

        if (PlayerGrid != null) PlayerGrid.OnTileClicked += HandleTileClicked;
        if (EnemyGrid != null) EnemyGrid.OnTileClicked += HandleTileClicked;
        if (ButtonManager != null) ButtonManager.OnButtonClicked += HandleActionPressed;

        SpawnUnitFromResource(Player1Resource, TargetSide.Ally, new GridPos(1, 0));
        SpawnUnitFromResource(Player2Resource, TargetSide.Ally, new GridPos(2, 1));
        SpawnUnitFromResource(Player3Resource, TargetSide.Ally, new GridPos(2, 2));
        SpawnUnitFromResource(Enemy1Resource, TargetSide.Enemy, new GridPos(0, 1));
        SpawnUnitFromResource(Enemy2Resource, TargetSide.Enemy, new GridPos(2, 2));
        SpawnUnitFromResource(Enemy3Resource, TargetSide.Enemy, new GridPos(0, 2));

        TurnOrderHUD?.Setup(_state.AllUnits);
        
        StartTurn();
    }

    public void TryFillEmptyTurnQueue()
    {
        if (_state.TurnQueue.Count == 0)
        {
            List<Unit> allUnits = _state.AllUnits.Values.ToList();
            allUnits.Sort((a, b) => b.Speed.CompareTo(a.Speed));
            foreach (var unit in allUnits) _state.TurnQueue.Enqueue(unit);
            
            List<string> unitQueueOnlyNames = [];
            foreach (Unit unit in _state.TurnQueue) unitQueueOnlyNames.Add(unit.Name);
            _state.TurnOrder = unitQueueOnlyNames;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept"))
        {
            if (_state.ActiveUnit != null &&
                _state.ActiveUnit.Side == TargetSide.Ally)
            {
                GD.Print($"Player ended turn for {_state.ActiveUnit.Name}");
                EndTurn();
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
        
        _state.AllUnits[battleUnit.Name] = battleUnit;
        Formation formation = side == TargetSide.Ally ? _state.PlayerFormation : _state.EnemyFormation;
        bool placed = formation.PlaceUnit(battleUnit.Name, pos);
        GD.Print($"Spawning '{battleUnit.Name}' on {side} at ({pos.collum},{pos.row}) -> placed={placed}");

        if (UnitViewScene != null)
        {
            var view = UnitViewScene.Instantiate();
            AddChild(view);
            
            GridView grid = side == TargetSide.Ally ? PlayerGrid : EnemyGrid;
            Vector2 screenPos = grid.Position + grid.GetTileScreenPos(pos);
            Node healthBar = view.FindChild("HealthBar");
            if(healthBar == null)
            {
                GD.PrintErr("ERROR! healthbar wasn't found");
                return;
            }
            view.Set(Node2D.PropertyName.Position, screenPos);
            healthBar.Set(Range.PropertyName.MaxValue, battleUnit.MaxHp);
            healthBar.Set(Range.PropertyName.Value, battleUnit.Hp);
            _unitViews[battleUnit.Name] = view;
            _unitHealthBars[battleUnit.Name] = healthBar;
        }
    }

    private bool TryExecuteAction(GridPos pos, TargetSide originSide, GridPos clickedPos)
    {
        if (_currentAction != null)
        {
            TargetSide globalActionTargetSide;
            if (
                (originSide == TargetSide.Ally && _currentAction.TargetSide == TargetSide.Enemy) || 
                (originSide == TargetSide.Ally && _currentAction.TargetSide == TargetSide.Both) ||
                (originSide == TargetSide.Enemy && _currentAction.TargetSide == TargetSide.Ally)
                ) globalActionTargetSide = TargetSide.Enemy;
            else globalActionTargetSide = TargetSide.Ally;
            
            if (_currentAction.OriginSquares.Positions.Contains(pos) &&
                (
                    (_currentAction.TargetSquares.Positions.Contains(clickedPos)) || 
                    (_currentAction.AllyTargetRange == AllyTargetRange.Full) ||
                    (_currentAction.AllyTargetRange == AllyTargetRange.Self && clickedPos == pos) ||
                    (_currentAction.AllyTargetRange == AllyTargetRange.NextToSelf && TileMask.SquaresToHit(TileMask.Parse(".x./x.x/.x."), pos).Positions.Contains(clickedPos))
                )
            )
            {
                if (_currentAction.TargetType == TargetType.Tile && _state.ActiveUnit.ExecuteAction(_currentAction))
                {
                    foreach (GridPos tilePosition in TileMask.SquaresToHit(_currentAction.AoiSquares, clickedPos).Positions)
                    {
                        foreach (TileEffect tileEffect in _currentAction.TileEffects)
                        {
                            switch (globalActionTargetSide)
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
                if (globalActionTargetSide == TargetSide.Ally || (_currentAction.TargetSide == TargetSide.Both && _enemyToTarget != null)) targetId = _state.PlayerFormation.GetUnitAt(clickedPos);
                else if (globalActionTargetSide == TargetSide.Enemy || (_currentAction.TargetSide == TargetSide.Both && _enemyToTarget == null)) targetId = _state.EnemyFormation.GetUnitAt(clickedPos);
                if(targetId != null && TryIsUnitAlive(targetId))
                {
                    Unit target = _state.AllUnits[targetId];
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
                        PlayerGrid.HighlightTiles(TileMask.Parse(allyTilesToHighlight).Positions, TileColorReason.TargetForAction);
                        _enemyToTarget = target;
                        GD.Print("executed half an action");
                        return true;
                    }
                    else if (_currentAction.TargetType == TargetType.EnemyAndAlly && _currentAction.AllyTargetRange == AllyTargetRange.Self)
                    {
                        if (_state.ActiveUnit.ExecuteAction(_currentAction))
                        {
                            ExecuteActionOnUnits(target, _currentAction.StatToChange, _currentAction.StatChangeAmount, _currentAction.Damage);
                            ExecuteActionOnUnits(_state.ActiveUnit, _currentAction.SecondaryStatToChange, _currentAction.SecondaryStatChangeAmount, _currentAction.SecondaryDamage);
                            GD.Print("executed a self action");
                        }
                        else GD.PrintErr("No actions left!");
                        UpdateBattleVisuals();
                        return true;
                    }
                    else if (_enemyToTarget != null && _currentAction.TargetType == TargetType.EnemyAndAlly)
                    {
                        if(_state.ActiveUnit.ExecuteAction(_currentAction))
                        {
                            ExecuteActionOnUnits(_enemyToTarget, _currentAction.StatToChange,
                                _currentAction.StatChangeAmount, _currentAction.Damage);
                            ExecuteActionOnUnits(target, _currentAction.SecondaryStatToChange,
                                _currentAction.SecondaryStatChangeAmount, _currentAction.SecondaryDamage);
                            GD.Print("executed an double action");
                        }
                        else GD.PrintErr("No actions left!");
                        UpdateBattleVisuals();
                        return true;
                    }
                    
                    if (_currentAction.AoiSquares.Positions.Any())
                    {
                        foreach (var position in TileMask.SquaresToHit(_currentAction.AoiSquares, clickedPos).Positions)
                        {
                            string secondaryTargetId;
                            if (globalActionTargetSide == TargetSide.Ally || _currentAction.TargetSide == TargetSide.Both) secondaryTargetId = _state.PlayerFormation.GetUnitAt(position);
                            else secondaryTargetId = _state.EnemyFormation.GetUnitAt(position);
                            Unit secondaryTarget = _state.AllUnits[secondaryTargetId];
                            if (secondaryTarget != null) ExecuteActionOnUnits(secondaryTarget, _currentAction.SecondaryStatToChange, _currentAction.SecondaryStatChangeAmount, _currentAction.SecondaryDamage);
                        }
                    }

                    if (_state.ActiveUnit.ExecuteAction(_currentAction))
                    {
                        ExecuteActionOnUnits(target, _currentAction.StatToChange, _currentAction.StatChangeAmount,
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
        GD.Print($"  > ActiveUnitId='{_state.ActiveUnit}'");

        if (_state.ActiveUnit == null) return;
        if (!_state.AllUnits.TryGetValue(_state.ActiveUnit.Name, out var activeUnit))  return; 

        GD.Print($"  > activeUnit.Side={activeUnit.Side}, clickedSide={clickedSide}");
        
        if (_currentAction != null && (clickedSide == _currentAction.TargetSide || _currentAction.TargetSide == TargetSide.Both))
        {
            MovementRules.TryFindUnitPosition(_state, _state.ActiveUnit.Name, out Formation formation, out GridPos pos);
            TryExecuteAction(pos, TargetSide.Ally, clickedPos);
            return;
        }

        if (activeUnit.Side != TargetSide.Ally) return; 
        if (clickedSide != activeUnit.Side) return;
        TryMoveUnit(activeUnit, clickedPos);
        
        if(_state.ActiveUnit.ActionPoints <= 0) ButtonManager.RemoveButtons();
        if (_state.ActiveUnit.ActionPoints <= 0 && _state.ActiveUnit.Movement <= 0)
        {
            EndTurn();
            GD.Print($"Player {_state.ActiveUnit} turn ended automatically.");
        }
    }

    public bool TryIsUnitAlive(string unitId)
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
        UnitInfoPanel?.UpdateInfo(_state.ActiveUnit);
        UnitAbilityInfoPanel?.UpdateInfo(_currentAction);
        if(_state.ActiveUnit.ActionPoints <= 0) ButtonManager.RemoveButtons();
        if (_state.ActiveUnit.ActionPoints <= 0 && _state.ActiveUnit.Movement <= 0)
        {
            GD.Print($"Player {_state.ActiveUnit} turn ended automatically.");
            EndTurn();
        }
    }
    
    private void HandleActionPressed(string buttonId)
    {
        IsCombatDone();
        GD.Print($"Action {buttonId} was pressed");
        List<GridPos> targetSquares = [];
        if (_currentAction != _state.ActiveUnit.Actions[int.Parse(buttonId)])
        {
            _currentAction = _state.ActiveUnit.Actions[int.Parse(buttonId)];
            targetSquares = _currentAction.TargetSquares.Positions.ToList();
        }
        if (_currentAction.TargetSide == TargetSide.Ally)
        {
            PlayerGrid.ClearHighlights();
            EnemyGrid.ClearHighlights();
            PlayerGrid.HighlightTiles(_currentAction.OriginSquares.Positions.ToList(), TileColorReason.OriginForAction);
            PlayerGrid.HighlightTiles(targetSquares, TileColorReason.TargetForAction);
            
        }
        else
        {
            PlayerGrid.ClearHighlights();
            EnemyGrid.ClearHighlights();
            PlayerGrid.HighlightTiles(_currentAction.OriginSquares.Positions.ToList(), TileColorReason.OriginForAction);
            EnemyGrid.HighlightTiles(targetSquares, TileColorReason.TargetForAction);
        }

        if (targetSquares.Count == 0)
        {
            _currentAction = null;
            _enemyToTarget = null;
            UpdateMovementHighlights();
        }
        UnitAbilityInfoPanel?.UpdateInfo(_currentAction);
    }

    private void RemoveUnitFromTurnOrder(string idToRemove)
    {
        _state.TurnOrder.Remove(idToRemove);
        List<Unit> tempQueueAsList = _state.TurnQueue.ToList();
        tempQueueAsList.Remove(tempQueueAsList.FirstOrDefault(u => u.Name == idToRemove));
        _state.TurnQueue.Clear();
        foreach (Unit unit in tempQueueAsList) _state.TurnQueue.Enqueue(unit);
        TryFillEmptyTurnQueue();
    }

    private void ExecuteActionOnUnits(Unit unit, Stat statToChange, float statChangeAmount, int damage)
    {
        ApplyStatChange(unit, statToChange, statChangeAmount);
        int oldHp = unit.Hp;
        int newHp = unit.TakeDamage((int)MathF.Round(damage * _state.ActiveUnit.Attack), _currentAction.ActionElement);
        
        if(oldHp != newHp)
        {
            _unitHealthBars[unit.Name].Set(Range.PropertyName.MaxValue, unit.MaxHp);
            _unitHealthBars[unit.Name].Set(Range.PropertyName.Value, unit.Hp);
            GD.Print($"Health changed to: {newHp} from {oldHp}");
        }

        if (newHp <= 0)
        {
            RemoveUnitFromTurnOrder(unit.Name);
            _state.AllUnits.Remove(unit.Name);
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
            // TODO show win screen
        }
        else if (!playerAlive)
        {
            _combatEnded = true;
            GD.Print("=== COMBAT LOST! All players defeated. ===");
            // TODO show lose screen
        }
    }

    public void EndTurn()
    {
        _currentAction = null;
        PlayerGrid?.ClearHighlights();
        EnemyGrid?.ClearHighlights();
        TryExecuteTileAction(PlayerGrid, _state.PlayerFormation);
        TryExecuteTileAction(EnemyGrid, _state.EnemyFormation);
        ButtonManager.RemoveButtons();
        UnitInfoPanel?.UpdateInfo(null);
        GD.Print($"Turn ended for: {_state.ActiveUnit.Name}");
        StartTurn();
    }

    public void StartTurn()
    {
        TryFillEmptyTurnQueue();
        _state.ActiveUnit = _state.TurnQueue.Dequeue();
        _state.ActiveUnit.StartTurn();
        UpdateTurnOrder();
        GD.Print($"Turn started for: {_state.ActiveUnit.Name}");
        GD.Print($"DEBUG Budget: MoveSteps={_state.ActiveUnit.Movement}, AP={_state.ActiveUnit.ActionPoints}");
        
        UpdateMovementHighlights();
        UnitInfoPanel?.UpdateInfo(_state.ActiveUnit);
        ButtonManager.GenerateButtons(_state.AllUnits[_state.ActiveUnit.Name].Actions);
        
        if (_state.ActiveUnit.Side == TargetSide.Enemy)
        {
            if (_state.ActiveUnit is EnemyUnit enemyUnit)
            {
                GD.Print($"Enemy {_state.ActiveUnit.Name} executing AI turn.");
                //TODO execute enemy action
            }
        }
    }

    public void TryMoveUnit(Unit unitToMove, GridPos destinationSquare)
    {
        // TODO make this function
        if (_unitViews.TryGetValue(moved.UnitId, out var view))
        {
            GridView grid = PlayerGrid;
            if (_state.AllUnits.TryGetValue(moved.UnitId, out var unit) && unit.Side == TargetSide.Enemy)
            {
                grid = EnemyGrid;
            }
            Vector2 newScreenPos = grid.Position + grid.GetTileScreenPos(moved.To);
            view.Set(Node2D.PropertyName.Position, newScreenPos);
        }
        GD.Print($"SUCCESS! Unit {moved.UnitId} moved to {moved.To.row},{moved.To.collum}");
        UpdateMovementHighlights();
        UnitInfoPanel?.UpdateInfo(_state.ActiveUnit);
    }

    public void UpdateTurnOrder()
    {
        List<string> unitQueueOnlyNames = [];
        foreach (Unit unit in _state.TurnQueue) unitQueueOnlyNames.Add(unit.Name);
        TurnOrderHUD?.Refresh(_state.TurnOrder, _state.ActiveUnit.Name, unitQueueOnlyNames);
    }
    
    //         else if (evt is EnemyAbilityChosenEvent abilityChosen)
    //         {
    //             _currentAction = abilityChosen.Action;
    //             GD.Print(_currentAction);
    //             GD.Print($"Enemy {abilityChosen.UnitId} uses '{_currentAction}' targeting ({abilityChosen.TargetPos.row},{abilityChosen.TargetPos.collum})");
    //
    //             bool actionSuccess = TryExecuteAction(abilityChosen.EnemyPos, TargetSide.Enemy, abilityChosen.TargetPos);
    //             
    //             if (actionSuccess && _currentAction.TargetType == TargetType.EnemyAndAlly)
    //             {
    //                 //TODO get the propper secondary target
    //                 GridPos secondTarget = new GridPos(0, 0);
    //                 TryExecuteAction(abilityChosen.EnemyPos, TargetSide.Enemy, secondTarget);
    //             }
    //             _currentAction = null;
    //             UnitInfoPanel?.UpdateInfo(_state.ActiveUnit);
    //         }
    //         else if (evt is TurnOrderChangedEvent turnOrder)
    //         {
    //             TurnOrderHUD?.Refresh(turnOrder.OrderUnitIds, turnOrder.ActiveUnitId, turnOrder.RemainingThisRound);
    //         }

    private void TryExecuteTileAction(GridView gridView, Formation formation)
    {
        //Reverses all temporary tile effects
        foreach (KeyValuePair<Unit, List<TileEffect>> unit in _unitsToReverseTileEffects)
        {
            if(TryIsUnitAlive(unit.Key.Name))
            {
                foreach (TileEffect tileEffect in unit.Value)
                {
                    tileEffect.StatChangeAmount *= -1;
                    ApplyStatChange(unit.Key, tileEffect.StatToChange, tileEffect.StatChangeAmount);
                }

                if (unit.Key.Hp <= 0)
                {
                    RemoveUnitFromTurnOrder(unit.Key.Name);
                    _state.AllUnits.Remove(unit.Key.Name);
                }
            }
        }
        
        //Executes all current tile effects
        Dictionary<GridPos, List<TileEffect>> tileEffectsToTry = gridView.GetTilesWEffects();
        foreach (KeyValuePair<GridPos, List<TileEffect>> tileEffectPair in tileEffectsToTry)
        {
            Unit unitAtPos = _state.AllUnits[formation.GetUnitAt(tileEffectPair.Key)];
            if (unitAtPos != null && TryIsUnitAlive(unitAtPos.Name))
            {
                int oldHp = unitAtPos.Hp;
                foreach (TileEffect tileEffect in tileEffectPair.Value)
                {
                    ApplyStatChange(unitAtPos, tileEffect.StatToChange, tileEffect.StatChangeAmount);
                    if (tileEffect.Type == TileEffectType.TempStatChange)
                    {
                        if (!_unitsToReverseTileEffects.TryAdd(unitAtPos, [tileEffect])) _unitsToReverseTileEffects[unitAtPos].Add(tileEffect);
                    }
                }
                int newHp = unitAtPos.Hp;
                if (oldHp != newHp)
                {
                    GD.Print($"Hp changed by {oldHp - newHp}");
                    _unitHealthBars[unitAtPos.Name].Set(Range.PropertyName.MaxValue, unitAtPos.MaxHp);
                    _unitHealthBars[unitAtPos.Name].Set(Range.PropertyName.Value, unitAtPos.Hp);
                }
                if (unitAtPos.Hp <= 0)
                {
                    RemoveUnitFromTurnOrder(unitAtPos.Name);
                    _state.AllUnits.Remove(unitAtPos.Name);
                }
            }
        }
    }

    private void ApplyStatChange(Unit unit, Stat statToChange, float statChangeAmount)
    {
        switch (statToChange)
        {
            case Stat.None:
                break;
            case Stat.MaxHp:
                if (unit.MaxHp + (int)statChangeAmount <= 0) unit.MaxHp = 1;
                else unit.MaxHp += (int)statChangeAmount;
                if (statChangeAmount > 0) unit.Hp += (int)statChangeAmount;
                if (unit.MaxHp < unit.Hp) unit.Hp = unit.MaxHp;
                break;
            case Stat.Hp:
                if (unit.Hp + (int)statChangeAmount > unit.MaxHp) unit.Hp = unit.MaxHp;
                else unit.Hp += (int)statChangeAmount;
                break;
            case Stat.Attack:
                unit.Attack += statChangeAmount;
                break;
            case Stat.Speed:
                unit.Speed += statChangeAmount;
                break;
            case Stat.Aggro:
                unit.Aggro += statChangeAmount;
                break;
            case Stat.MaxMovement:
                unit.MaxMovement += (int)statChangeAmount;
                break;
            case Stat.Movement:
                unit.Movement += (int)statChangeAmount;
                break;
            case Stat.MaxActionPoints:
                unit.MaxActionPoints += (int)statChangeAmount;
                break;
            case Stat.ActionPoints:
                unit.ActionPoints += (int)statChangeAmount;
                break;
        }
    }

    private void UpdateMovementHighlights()
    {
        PlayerGrid?.ClearHighlights();
        EnemyGrid?.ClearHighlights();
        if (_state.ActiveUnit == null) return;
        if (_state.ActiveUnit.Side != TargetSide.Ally) return;
        var validMoves = MovementRules.GetValidMoves(_state, _state.ActiveUnit);
        PlayerGrid?.HighlightTiles(validMoves, TileColorReason.MovePossible);
    }
}
