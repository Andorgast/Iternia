using Godot;
using System;

public partial class Button : TextureButton
{
    public event Action<string> ButtonClicked;

    public override void _Pressed() { ButtonClicked?.Invoke(Name); }
}
