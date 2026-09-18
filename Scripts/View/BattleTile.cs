using Godot;
using Iternia.Scripts.Core;

namespace Iternia.View;

public partial class BattleTile : Area2D
{
    [Export] public Color NormalColor { get; set; } = Colors.White;
    [Export] public Color HighlightColor { get; set; } = Colors.RoyalBlue;
    [Export] public Color HoverColor { get; set; } = Colors.Cyan;

    private Sprite2D _sprite;
    public GridPos LogicalPos { get; private set; }
    
    private bool _isHighlighted;
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
        UpdateVisuals();
    }

    public void SetHighlight(bool active)
    {
        _isHighlighted = active;
        UpdateVisuals();
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
        _isHovered = true;
        UpdateVisuals();
    }

    private void OnMouseExited()
    {
        _isHovered = false;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_sprite == null) return;

        if (_isHovered)
        {
            _sprite.Modulate = HoverColor;
        }
        else if (_isHighlighted)
        {
            _sprite.Modulate = HighlightColor;
        }
        else
        {
            _sprite.Modulate = NormalColor;
        }
    }
}

