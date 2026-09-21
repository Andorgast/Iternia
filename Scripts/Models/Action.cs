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
    [Export] public TargetType TargetType;
    [Export] public Element ActionElement;
    [Export] public TargetSide TargetSide;
    [Export] public Stat StatToChange;
    [Export] public float StatChangeAmount;
    [Export] public Stat SecondaryStatToChange;
    [Export] public float SecondaryStatChangeAmount;
    [Export] public int Damage;
    [Export] public int SecondaryDamage;
    public TileMask OriginSquares;
    public TileMask TargetSquares;

    public Action()
    {
        OriginSquares = TileMask.Parse(InitialOriginSquares);
        TargetSquares = TileMask.Parse(InitialTargetSquares);
    }
    
    public void ExecuteAction(List<GridPos> tileTargets, List<string> mainTargetIds, List<string> secondaryTargetIds, int attackStat)
    {
        foreach (string id in mainTargetIds)
        {
            if (Damage > 0 && TargetType != TargetType.Tile) ; //TODO deal the damage to the target
            if (StatToChange != Stat.None && TargetType != TargetType.Tile) ; //TODO change the stat value
        }
        foreach (string id in secondaryTargetIds)
        {
            if (SecondaryDamage > 0 && TargetType != TargetType.Aoi) ; //TODO deal the damage to the target
            if (SecondaryStatToChange != Stat.None && TargetType != TargetType.Aoi) ; //TODO change the stat value
        }
        foreach (GridPos position in tileTargets)
        {
            if (Damage > 0 && TargetType == TargetType.Tile) ; //TODO deal the damage to the target
            if (StatToChange != Stat.None && TargetType == TargetType.Tile) ; //TODO change the stat value
        }
    }
}