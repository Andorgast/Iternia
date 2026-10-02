using Godot;
using Iternia.Scripts.Core;

namespace Iternia.Scripts.Models;

[GlobalClass]
public partial class TileEffect : Resource
{
    [Export] public string Name;
    [Export] public TileEffectType Type;
    [Export] public Stat StatToChange;
    [Export] public float StatChangeAmount;
}