using System.Collections.Generic;
using Godot;
using Iternia.Scripts.Core;

namespace Iternia.Scripts.Models;

[GlobalClass]
public partial class Action : Resource
{
    [Export] public string Id;
    [Export] public string Name;
    [Export] protected Basis InitialOriginSquares;
    [Export] protected Basis InitialTargetSquares;
    [Export] protected Basis InitialAoiSquares;
    [Export] public TargetType TargetType;
    [Export] public AllyTargetRange AllyTargetRange = AllyTargetRange.DoesntTargetAlly;
    [Export] public Element ActionElement;
    [Export] public TargetSide TargetSide;
    [Export] public Stat StatToChange;
    [Export] public float StatChangeAmount;
    [Export] public Stat SecondaryStatToChange;
    [Export] public float SecondaryStatChangeAmount;
    [Export] public int Damage;
    [Export] public int SecondaryDamage;
    public TileMask OriginSquares => TileMask.Parse(InitialOriginSquares);
    public TileMask TargetSquares => TileMask.Parse(InitialTargetSquares);
    public TileMask AoiSquares => TileMask.Parse(InitialAoiSquares);
}