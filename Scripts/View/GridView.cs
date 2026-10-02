using Godot;
using Iternia.Scripts.Core;
using System.Collections.Generic;
using System.Linq;
using Iternia.Scripts.Models;

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
            tile.UpdateVisuals(true, TileColorReason.Reset, false);
        }
    }

    public void HighlightTiles(IEnumerable<GridPos> positions, TileColorReason reason)
    {
        foreach (var pos in positions)
        {
            if (_tiles.TryGetValue(pos, out var tile))
            {
                tile.UpdateVisuals(false, reason, true);
            }
        }
    }

    public Dictionary<GridPos, List<TileEffect>> GetTilesWEffects()
    {
        Dictionary<GridPos, List<TileEffect>> tempList = [];
        for (int i = 0; i < _tiles.Count; i++)
        {
            if (_tiles.Values.ToList()[i].TileEffectList.Any()) tempList.Add(_tiles.Keys.ToList()[i], _tiles.Values.ToList()[i].TileEffectList);
        }
        return tempList;
    }

    public void AddTileEffect(GridPos position, TileEffect tileEffectToApply)
    {
        _tiles[position].TileEffectList.Add(tileEffectToApply);
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
        int xIndex = pos.collum;
        
        if (Side == TargetSide.Ally)
        {
            xIndex = 2 - pos.collum;
        }

        float x = xIndex * (TileSize + Spacing);
        float y = pos.row * (TileSize + Spacing);
        
        return new Vector2(x, y);
    }
}
