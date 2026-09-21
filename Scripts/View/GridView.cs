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
    
    public event System.Action<TargetSide, GridPos> OnTileClicked;

    public override void _Ready()
    {
        GenerateGrid();
    }

    public Vector2 GetTileScreenPos(GridPos pos)
    {
        return GridToScreen(pos);
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

                tile.TileClicked += (clickedPos) => OnTileClicked?.Invoke(Side, clickedPos);

                _tiles[pos] = tile;
            }
        }
    }

    private Vector2 GridToScreen(GridPos pos)
    {
        int xIndex = pos.row;
        
        if (Side == TargetSide.Ally)
        {
            xIndex = 2 - pos.row;
        }

        float x = xIndex * (TileSize + Spacing);
        float y = pos.collum * (TileSize + Spacing);
        
        return new Vector2(x, y);
    }
}
