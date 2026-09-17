using Godot;
using Iternia.Scripts.Core;
using System.Collections.Generic;

namespace Iternia.View;

public partial class GridView : Node2D
{
    [Export] public TargetSide Side { get; set; } = TargetSide.Ally;
    [Export] public int TileSize { get; set; } = 64;
    [Export] public int Spacing { get; set; } = 4;
    [Export] public PackedScene TileScene { get; set; }

    private readonly Dictionary<GridPos, BattleTile> _tiles = new();
    
    private TileMask _testMask;

    public override void _Ready()
    {
        _testMask = TileMask.Parse(
            "X../" +
            "XX./" +
            "X.."
        );

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (TileScene == null)
        {
            GD.PrintErr("TileScene is leeg! Sleep BattleTile.tscn in de inspector van GridView.");
            return;
        }

        for (int lane = 0; lane < 3; lane++)
        {
            for (int rank = 0; rank < 3; rank++)
            {
                GridPos pos = new GridPos(rank, lane);
                Vector2 screenPos = GridToScreen(pos);

                BattleTile tile = TileScene.Instantiate<BattleTile>();
                
                AddChild(tile); 
                
                tile.Setup(pos, screenPos);
                
                tile.SetHighlight(_testMask.HasPos(pos));

                _tiles[pos] = tile;
            }
        }
    }

    private Vector2 GridToScreen(GridPos pos)
    {
        int xIndex = pos.Rank;
        
        if (Side == TargetSide.Ally)
        {
            xIndex = 2 - pos.Rank;
        }

        float x = xIndex * (TileSize + Spacing);
        float y = pos.Lane * (TileSize + Spacing);
        
        return new Vector2(x, y);
    }
}
