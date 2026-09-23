using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Iternia.Scripts.Core;
using Iternia.Scripts.Models;

namespace Iternia.Scripts.View;

public partial class ButtonManager : Node2D
{
    [Export] private PackedScene _buttonScene;
    [Export] private int _buttonWidth;
    [Export] private int _spacing;
    [Export] private int _previousXpos = 0;

    public ButtonManager()
    {
        _previousXpos -= _buttonWidth;
    }
    public void GenerateButtons(Array<Action> actions)
    {
        if (_buttonScene == null)
        {
            GD.PrintErr("TileScene is leeg! Sleep Button.tscn in de inspector van GridView.");
            return;
        }
        
        foreach (Action action in actions)
        {
            Button newButton = _buttonScene.Instantiate<Button>();
            int newXpos = _previousXpos + _buttonWidth + _spacing;
            _previousXpos = newXpos;
            newButton.Position = new Vector2(newXpos, Position.Y);
        }
    }

    public void RemoveButtons()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
    }
}