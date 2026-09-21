// using System;
// using System.Collections.Generic;
// using Godot;
// using Iternia.Scripts.Core;
//
// namespace Iternia.Scripts.Models;
//
// public class UnitCreator
// {
//     public static Unit CreateEric()
//     {
//         string id = "hero_6";
//         string name = "Eric";
//         int attack = 2;
//         TargetSide side = TargetSide.Ally;
//         int maxHp = 10;
//         int defaultSpeed = 20;
//         int defaultMovement = 2;
//         int defaultActionPoints = 1;
//         float defaultAggro = 1;
//         List<Action> actions = [];
//         List<Element> resistances = [];
//         List<Element> weaknesses = [];
//         return new Unit(id, name, attack, side, maxHp, defaultSpeed, defaultMovement, defaultActionPoints, defaultAggro, 
//             actions, resistances, weaknesses);
//     }
// }