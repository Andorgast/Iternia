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

    public void ClearHighlights()
    {
        foreach (var tile in _tiles.Values)
        {
            tile.SetHighlight(false);
        }
    }

    public void HighlightTiles(IEnumerable<GridPos> positions)
    {
        ClearHighlights();
        foreach (var pos in positions)
        {
            if (_tiles.TryGetValue(pos, out var tile))
            {
                tile.SetHighlight(true);
            }
        }
    }

    private void GenerateGrid()
    {
        if (TileScene == null)
        {
            GD.PrintErr("TileScene is leeg! Sleep BattleTile.tscn in de inspector van GridView.");
            return;
        }

        for (int collum = 0; collum < 3; collum++)
        {
            for (int row = 0; row < 3; row++)
            {
                GridPos pos = new GridPos(row, collum);
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
