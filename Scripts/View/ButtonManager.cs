using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Iternia.Scripts.Core;
using Action = Iternia.Scripts.Models.Action;

namespace Iternia.Scripts.View;

public partial class ButtonManager : Node2D
{
    [Export] private PackedScene _buttonScene;
    [Export] private int _buttonWidth;
    [Export] private int _spacing;
    [Export] private int _previousXPos = 0;

    public event Action<string> OnButtonClicked;
    public ButtonManager()
    {
        _previousXPos -= _buttonWidth;
    }
    public void GenerateButtons(Array<Action> actions)
    {
        if (_buttonScene == null)
        {
            GD.PrintErr("TileScene is leeg! Sleep Button.tscn in de inspector van GridView.");
            return;
        }

        if (actions.Count <= 0)
        {
            GD.PrintErr("Er zijn geen actions gevonden, voeg actions toe");
            return;
        }

        int i = 0;
        foreach (Action action in actions)
        {
            Button newButton = _buttonScene.Instantiate<Button>();
            int newXPos = _previousXPos + _buttonWidth + _spacing;
            _previousXPos = newXPos;
            newButton.Position = new Vector2(newXPos, Position.Y);
            newButton.ButtonClicked += (name) => OnButtonClicked(name);
            newButton.Name = i.ToString();
            AddChild(newButton);
            i++;
        }
    }

    public void RemoveButtons()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
        _previousXPos = 0;
    }
}