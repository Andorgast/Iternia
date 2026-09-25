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
    Unit,
    Aoi,
    Tile,
    None
}