namespace Iternia.Scripts.Core;

public enum TargetSide
{
    Ally,
    Enemy
}

public enum Element
{
    Fire,
    Water,
    Plant,
    Ice,
    Physical,
    Dark
}

public enum ActionType
{
    Attack,
    Buff,
    Debuff,
    TileEffect,
    AoiAttack,
    AoiTileEffect,
    BuffAndDebuff,
}

public enum Stat
{
    None,
    MaxHp,
    Attack,
    Speed,
    Aggro,
    MaxMovement,
    Movement,
    MaxActionPoints,
    ActionPoints
}

public enum TargetType
{
    Tile,
    TileAoi,
    Ally,
    AllyAoi,
    Enemy,
    EnemyAoi,
    SelfDamage,
    SelfDamageAoi,
    AllyAndEnemy
}

public enum AllyTargetRange
{
    Full,
    Self,
    NextToSelf,
    DefinedByTargetGrid,
    DoesntTargetAlly
}