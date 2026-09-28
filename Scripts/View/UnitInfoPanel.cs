using Godot;
using Iternia.Scripts.Models;
using Iternia.Scripts.Core;

namespace Iternia.Scripts.View;

public partial class UnitInfoPanel : PanelContainer
{
    [Export] private Label _nameLabel;
    [Export] private Label _hpLabel;
    [Export] private Label _apLabel;
    [Export] private Label _mpLabel;

    public void UpdateInfo(Unit unit, TurnBudget budget)
    {
        if (unit == null)
        {
            Visible = false;
            return;
        }

        Visible = true;
        _nameLabel.Text = $"Name: {unit.Name} ({unit.Id})";
        _hpLabel.Text = $"HP: {unit.Hp} / {unit.MaxHp}";
        
        if (budget != null)
        {
            _apLabel.Text = $"AP: {budget.ActionPoints} / {unit.MaxActionPoints}";
            _mpLabel.Text = $"MP: {budget.MoveSteps} / {unit.MaxMovement}";
        }
    }
}