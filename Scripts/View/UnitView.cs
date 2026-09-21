using Godot;

namespace Iternia.View;

public partial class UnitView : Sprite2D
{
    public string UnitId { get; private set; }
    

    public void Setup(string unitId, Vector2 startScreenPos)
    {
        UnitId = unitId;
        Position = startScreenPos;
    }

    public void MoveTo(Vector2 screenPos)
    {
        Tween tween = CreateTween();
        
        tween.TweenProperty(this, "position", screenPos, 0.3f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.Out);
    }
}

