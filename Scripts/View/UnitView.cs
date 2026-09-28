using Godot;

namespace Iternia.View;

public partial class UnitView : Node2D
{
    public string UnitId { get; private set; }

    [Export] private ProgressBar _healthBar;

    public void Setup(string unitId, Vector2 startScreenPos)
    {
        UnitId = unitId;
        Position = startScreenPos;
    }

    public void SetHealth(int current, int max)
    {
        if (_healthBar == null) return;
        _healthBar.MaxValue = max;
        _healthBar.Value = current;
    }

    public void MoveTo(Vector2 screenPos)
    {
        Tween tween = CreateTween();
        
        tween.TweenProperty(this, "position", screenPos, 0.3f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.Out);
    }
}
