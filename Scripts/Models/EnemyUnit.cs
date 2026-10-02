using Godot;
using Iternia.Scripts.Core;

namespace Iternia.Scripts.Models;

[GlobalClass]
public partial class EnemyUnit : Unit
{
    [Export] protected Basis InitialAllowedTiles;

    public TileMask AllowedTiles => TileMask.Parse(InitialAllowedTiles);
}

