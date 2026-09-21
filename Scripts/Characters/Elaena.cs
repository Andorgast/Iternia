using System.Collections.Generic;
using Godot;
using Iternia.Scripts.Core;
using Iternia.Scripts.Models;
namespace Iternia.Scripts.Characters;

public class Elaena(string id) : Unit
{
    public override string Id { get; init; } = id;
    public override string Name { get; init; } = "Elaena";
    public override TargetSide Side { get; set; } = TargetSide.Ally;
    public new List<Action> Actions { get; set; } = [];
    public new List<Element> Resistances { get; set; } = [Element.Ice, Element.Fire, Element.Plant];
    public new List<Element> Weaknesses { get; set; } = [Element.Dark];
    //The values below shouldn't be used for anything, these are here for easy editing in the editor
    [Export] private static readonly int InitialAttack = 1;
    [Export] private static readonly int InitialHp = 18;
    [Export] private static readonly int InitialMovement = 1;
    [Export] private static readonly int InitialActionPoints = 1;
    [Export] private static readonly int InitialSpeed = 13;
    [Export] private static readonly float InitialAggro = 0.8f;
    
    //The actual attributes that the game should use
    public override int Attack { get; set; } = InitialAttack;
    public override int DefaultAttack { get; set; } = InitialAttack;
    public override int Hp { get; set; } = InitialHp;
    public override int MaxHp { get; set; } = InitialHp;
    public override int Speed { get; set; } = InitialSpeed;
    public override int DefaultSpeed { get; set; } = InitialSpeed;
    public override int Movement { get; set; } = InitialMovement;
    public override int DefaultMovement { get; set; } = InitialMovement;
    public override int ActionPoints { get; set; } = InitialActionPoints;
    public override int DefaultActionPoints { get; set; } = InitialActionPoints;
    public override float Aggro { get; set; } = InitialAggro;
    public override float DefaultAggro { get; set; } = InitialAggro;
}