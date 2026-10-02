using Godot;
using System;

public partial class Button : TextureButton
{
    public event Action<string> ButtonClicked;
    public event Action<string> ButtonHovered;
    public event Action<string> ButtonUnhovered;

    public override void _Ready()
    {
        MouseEntered += () => ButtonHovered?.Invoke(Name);
        MouseExited += () => ButtonUnhovered?.Invoke(Name);
    }

    public override void _Pressed() { ButtonClicked?.Invoke(Name); }
}

