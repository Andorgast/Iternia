namespace Iternia.Scripts.Core;

public class Unit
{
    public string Id { get; set; }
    public string Name { get; set; }
    public TargetSide Side { get; set; }
    
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Speed { get; set; }

    public Unit(string id, string name, TargetSide side, int maxHp, int speed)
    {
        Id = id;
        Name = name;
        Side = side;
        MaxHp = maxHp;
        Hp = maxHp;
        Speed = speed;
    }
}

