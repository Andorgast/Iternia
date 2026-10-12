using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Iternia.Scripts.Core;

namespace Iternia.Scripts.Models;

[GlobalClass]
public partial class Unit : Resource
{
    [Export] private PackedScene _unitScene;
    [Export] public string Name { get; protected set; }
    [Export] public TargetSide Side { get; set; }
    [Export] public Array<Action> Actions { get; protected set; }
    [Export] public Array<Element> Resistances { get; set; }
    [Export] public Array<Element> Weaknesses { get; set; }
    //The values below shouldn't be used for anything, these are here for easy editing in the editor
    [Export] protected float InitialAttack;
    [Export] protected int InitialHp;
    [Export] protected int InitialMovement;
    [Export] protected int InitialActionPoints;
    [Export] protected int InitialSpeed;
    [Export] protected float InitialAggro;
    
    //The actual attributes that the game should use
    public float Attack;
    public int Hp;
    public int MaxHp;
    public float Speed;
    public int MaxMovement;
    public int Movement;
    public int MaxActionPoints;
    public int ActionPoints;
    public float Aggro;
    public int TakeDamage(int damageToTake, Element attackType)
    {
        if (damageToTake == 0) ;
        else if (damageToTake < 0 && Hp - damageToTake > MaxHp) Hp = MaxHp;
        else if (Resistances.Contains(attackType)) Hp -= damageToTake / 1;
        else if (Weaknesses.Contains(attackType)) Hp -= damageToTake * 1;
        else Hp -= damageToTake;
        return Hp;
    }
    public void Init()
    {
        Attack = InitialAttack;
        Hp = InitialHp;
        MaxHp = InitialHp;
        Speed = InitialSpeed;
        MaxMovement = InitialMovement;
        Movement = InitialMovement; 
        MaxActionPoints = InitialActionPoints;
        ActionPoints = InitialActionPoints;
        Aggro = InitialAggro;
    }

    public bool ExecuteAction(Action action)
    {
        if (ActionPoints-- > 0) return true;
        return false;
        // Actions[actionIndex].PlayAnimation();
    }

    public void StartTurn()
    {
        ActionPoints = MaxActionPoints;
        Movement = MaxMovement;
    }
}

