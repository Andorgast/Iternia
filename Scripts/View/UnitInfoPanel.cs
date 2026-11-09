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

    public void UpdateInfo(Unit unit)
    {
        if (unit == null)
        {
            Visible = false;
            return;
        }

        Visible = true;
        _nameLabel.Text = $"Name: {unit.Name} ({unit.Name})";
        _hpLabel.Text = $"HP: {unit.Hp} / {unit.MaxHp}";
        _apLabel.Text = $"AP: {unit.ActionPoints} / {unit.MaxActionPoints}";
        _mpLabel.Text = $"MP: {unit.Movement} / {unit.MaxMovement}";
    }
}