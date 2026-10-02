using Godot;

namespace Iternia.Scripts.View;

public partial class TurnSlot : PanelContainer
{
    [Export] private Label _nameLabel;
    private StyleBoxFlat _styleBox;

    public override void _Ready()
    {
        _styleBox = new StyleBoxFlat();

        AddThemeStyleboxOverride("panel", _styleBox);
    }

    public void Setup(string unitName, bool isEnemy)
    {
        _nameLabel.Text = unitName;
        _styleBox.BgColor = isEnemy
            ? new Color(0.7f, 0.2f, 0.2f)
            : new Color(0.2f, 0.4f, 0.7f);
    }

    public void SetActive(bool active)
    {
        _styleBox.BorderColor = active ? new Color(1f, 0.9f, 0f) : new Color(0, 0, 0, 0);
        _styleBox.BorderWidthBottom = active ? 3 : 0;
        _styleBox.BorderWidthTop    = active ? 3 : 0;
        _styleBox.BorderWidthLeft   = active ? 3 : 0;
        _styleBox.BorderWidthRight  = active ? 3 : 0;
    }
}