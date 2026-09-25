using Godot;
using System;
using Iternia.Scripts.Models;
using Action = Iternia.Scripts.Models.Action;

public partial class Button : TextureButton
{
    public event Action<string> ButtonClicked;
    // public Sprite2D Sprite;
    public override void _Pressed() { ButtonClicked?.Invoke(Name); }
}
