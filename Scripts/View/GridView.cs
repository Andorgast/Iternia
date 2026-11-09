using System;
using Godot;
using Iternia.Scripts.Core;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using Iternia.Scripts.Models;

namespace Iternia.View;

public partial class GridView : Node2D
{
    [Export] public TargetSide Side { get; set; } = TargetSide.Ally;
    [Export] public int TileSizeX { get; set; } = 220;
    [Export] public int TileSizeY { get; set; } = 130;
    [Export] public int Spacing { get; set; } = 0;
    [Export] public int Rows { get; set; } = 3;
    [Export] public int Collums { get; set; } = 3;
    [Export] public PackedScene TileScene { get; set; }

    private System.Collections.Generic.Dictionary<GridPos, BattleTile> _tiles = new();
    
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
            _tiles[pos].UpdateVisuals(false, reason, true);
        }
    }

    public System.Collections.Generic.Dictionary<GridPos, List<TileEffect>> GetTilesWEffects()
    {
        System.Collections.Generic.Dictionary<GridPos, List<TileEffect>> tempList = [];
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
        RandomNumberGenerator rng = new RandomNumberGenerator();
        if (TileScene == null)
        {
            GD.PrintErr("TileScene is leeg! Sleep BattleTile.tscn in de inspector van GridView.");
            return;
        }

        for (int collum = 0; collum < Collums; collum++)
        {
            for (int row = 0; row < Rows; row++)
            {
                GridPos pos = new GridPos(row, collum);
                Vector2 screenPos = GridToScreen(pos);

                BattleTile tile = TileScene.Instantiate<BattleTile>();
                Array<Node> propNodes = tile.GetNode("Props").GetChildren();
                String[] animationNames = [];
                foreach (Node propNode in propNodes)
                {
                    AnimatedSprite2D propSprite = tile.AllowedProps[0].Instantiate<AnimatedSprite2D>();
                    if(animationNames.Length == 0) animationNames = propSprite.GetSpriteFrames().GetAnimationNames();
                    propSprite.Play(animationNames[rng.RandiRange(0, animationNames.Length - 1)]);
                    propNode.AddChild(propSprite);
                }
                
                
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

        float x = xIndex * (TileSizeX + Spacing);
        float y = pos.row * (TileSizeY + Spacing);
        
        return new Vector2(x, y);
    }
}
