using Godot;
using Iternia.Scripts.Models;
using Iternia.Scripts.Core;
using System.Linq;

namespace Iternia.Scripts.View;

public partial class UnitAbilityInfoPanel : PanelContainer
{
    [Export] private Label _abilityName;
    [Export] private Label InitialAoiSquares;
    [Export] private Label TargetType;
    [Export] private Label AllyTargetRange;
    [Export] public Label TileEffects;
    [Export] public Label ActionElement;
    [Export] public Label TargetSide;
    [Export] public Label StatToChange;
    [Export] public Label StatChangeAmount;
    [Export] public Label SecondaryStatToChange;
    [Export] public Label SecondaryStatChangeAmount;
    [Export] public Label Damage;
    [Export] public Label SecondaryDamage;

    public void UpdateInfo(Action action)
    {
        if (action == null)
        {
            Visible = false;
            return;
        }

        Visible = true;
        _abilityName.Text = $"Name: {action.Name} ({action.Id})";
        InitialAoiSquares.Text = $"AOI Squares: {action.AoiSquares.ToString()}";
        TargetType.Text = $"Target Type: {action.TargetType}";
        AllyTargetRange.Text = $"Ally Target Range: {action.AllyTargetRange}";
        string tileEffectsStr = action.TileEffects != null && action.TileEffects.Count > 0 
            ? string.Join(", ", action.TileEffects.Select(t => t.Name)) 
            : "None";
        TileEffects.Text = $"Tile Effects: {tileEffectsStr}";
        ActionElement.Text = $"Action Element: {action.ActionElement}";
        TargetSide.Text = $"Target Side: {action.TargetSide}";
        StatToChange.Text = $"Stat To Change: {action.StatToChange}";
        StatChangeAmount.Text = $"Stat Change Amount: {action.StatChangeAmount}";
        SecondaryStatToChange.Text = $"Secondary Stat To Change: {action.SecondaryStatToChange}";
        SecondaryStatChangeAmount.Text = $"Secondary Stat Change Amount: {action.SecondaryStatChangeAmount}";
        Damage.Text = $"Damage: {action.Damage}";
        SecondaryDamage.Text = $"Secondary Damage: {action.SecondaryDamage}";
    }
}