using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Iternia.Scripts.Core;
using Iternia.Scripts.Models;

namespace Iternia.View;

public partial class BattleTile : Area2D
{
    [Export] public CpuParticles2D Walkable { get; set; }
    [Export] public CpuParticles2D Origin { get; set; }
    [Export] public CpuParticles2D Target { get; set; }
    [Export] public CpuParticles2D Hover { get; set; }

    // [Export] public Array<AnimatedSprite2D> testthingg;
    [Export] public Array<PackedScene> AllowedProps;
    public GridPos LogicalPos { get; private set; }
    public List<TileEffect> TileEffectList { get; set; } = [];

    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public void Setup(GridPos pos, Vector2 screenPos)
    {
        LogicalPos = pos;
        Position = screenPos;
        UpdateVisuals(true, TileColorReason.Reset, false );
    }

    public event System.Action<GridPos> TileClicked;

    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseBtn)
        {
            if (mouseBtn.ButtonIndex == MouseButton.Left && mouseBtn.Pressed)
            {
                TileClicked?.Invoke(LogicalPos);
            }
        }
    }

    private void OnMouseEntered()
    {
        UpdateVisuals(false, TileColorReason.Hover, true);
    }

    private void OnMouseExited()
    {
        UpdateVisuals(false, TileColorReason.Hover, false);
    }

    public void UpdateVisuals(bool reset, TileColorReason tileColorReason, bool addingColor)
    {
        if (reset)
        {
            Walkable.Emitting = false;
            Origin.Emitting = false;
            Target.Emitting = false;
            Hover.Emitting = false;
        }
        
        if (addingColor)
        {
            switch (tileColorReason)
            {
                case TileColorReason.MovePossible:
                    Walkable.Emitting = true;
                    break;
                case TileColorReason.OriginForAction:
                    Origin.Emitting = true;
                    break;
                case TileColorReason.TargetForAction:
                    Target.Emitting = true;
                    break;
                case TileColorReason.Hover:
                    Hover.Emitting = true;
                    break;
            }
        }
        else
        {
            switch (tileColorReason)
            {
                case TileColorReason.Reset:
                    break;
                case TileColorReason.MovePossible:
                    Walkable.Emitting = false;
                    break;
                case TileColorReason.OriginForAction:
                    Origin.Emitting = false;
                    break;
                case TileColorReason.TargetForAction:
                    Target.Emitting = false;
                    break;
                case TileColorReason.Hover:
                    Hover.Emitting = false;
                    break;
            }
        }
        
        
    }
}

