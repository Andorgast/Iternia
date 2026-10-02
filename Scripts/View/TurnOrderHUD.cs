using Godot;
using System.Collections.Generic;
using System.Linq;
using Iternia.Scripts.Models;
using Iternia.Scripts.Core;

namespace Iternia.Scripts.View;

public partial class TurnOrderHUD : HBoxContainer
{
    [Export] public PackedScene TurnSlotScene { get; set; }
    [Export] public int LookaheadCount { get; set; } = 5;

    private Dictionary<string, Unit> _units = new();

    public void Setup(Dictionary<string, Unit> units)
    {
        _units = units;
    }

    public void Refresh(IReadOnlyList<string> fullRoundOrder, string activeId, IReadOnlyList<string> remainingThisRound)
    {
        if (TurnSlotScene == null)
        {
            GD.PrintErr("TurnOrderHUD: TurnSlotScene is not assigned!");
            return;
        }

        var sequence = new List<string>();
        sequence.Add(activeId);
        sequence.AddRange(remainingThisRound);

        if (fullRoundOrder.Count > 0)
        {
            while (sequence.Count < LookaheadCount)
            {
                foreach (var id in fullRoundOrder)
                {
                    sequence.Add(id);
                    if (sequence.Count >= LookaheadCount) break;
                }
            }
        }

        sequence = sequence.Take(LookaheadCount).ToList();

        foreach (var child in GetChildren())
            child.QueueFree();

        for (int i = 0; i < sequence.Count; i++)
        {
            var id = sequence[i];
            if (!_units.TryGetValue(id, out var unit)) continue;

            var slot = TurnSlotScene.Instantiate<TurnSlot>();
            AddChild(slot);
            slot.Setup(unit.Name, unit.Side == TargetSide.Enemy);
            slot.SetActive(i == 0);
        }
    }
}