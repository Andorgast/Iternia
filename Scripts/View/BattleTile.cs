using System.Collections.Generic;
using Godot;
using Iternia.Scripts.Core;

namespace Iternia.View;

public partial class BattleTile : Area2D
{
    [Export] public Color NormalColor { get; set; } = Colors.White;
    [Export] public Color MoveToColor { get; set; } = Colors.Red;
    [Export] public Color OriginColor { get; set; } = Colors.Yellow;
    [Export] public Color TargetColor { get; set; } = Colors.Blue;
    [Export] public Color HoverColor { get; set; } = Colors.Cyan;

    private Sprite2D _sprite;
    public GridPos LogicalPos { get; private set; }
    
    private bool _isHovered;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        
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

    public void UpdateVisuals(bool resetColor, TileColorReason tileColorReason, bool addingColor)
    {
        if (_sprite == null) return;
        
        if (resetColor) _sprite.Modulate = NormalColor;

        if (!addingColor && _sprite.Modulate == NormalColor)
        {
            return;
        }
        
        if (addingColor)
        {
            switch (tileColorReason)
            {
                case TileColorReason.MovePossible:
                    _sprite.Modulate += MoveToColor;
                    break;
                case TileColorReason.OriginForAction:
                    _sprite.Modulate += OriginColor;
                    break;
                case TileColorReason.TargetForAction:
                    _sprite.Modulate += TargetColor;
                    break;
                case TileColorReason.Hover:
                    _sprite.Modulate += HoverColor;
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
                    _sprite.Modulate -= MoveToColor;
                    break;
                case TileColorReason.OriginForAction:
                    _sprite.Modulate -= OriginColor;
                    break;
                case TileColorReason.TargetForAction:
                    _sprite.Modulate -= TargetColor;
                    break;
                case TileColorReason.Hover:
                    _sprite.Modulate -= HoverColor;
                    break;
            }
        }
        
        
    }
}

