// using System;
// using System.Collections.Generic;
// using Godot;
// using Iternia.Scripts.Core;
//
// namespace Iternia.Scripts.Models;
//
// public class UnitCreator
// {
//     public static Unit CreateAster()
//     {
//         string id = "hero_2";
//         string name = "Aster";
//         int attack = 1;
//         TargetSide side = TargetSide.Ally;
//         int maxHp = 25;
//         int defaultSpeed = 15;
//         int defaultMovement = 1;
//         int defaultActionPoints = 1;
//         float defaultAggro = 1.5f;
//         List<Action> actions = [];
//         List<Element> resistances = [Element.Fire];
//         List<Element> weaknesses = [];
//         return new Unit(id, name, attack, side, maxHp, defaultSpeed, defaultMovement, defaultActionPoints, defaultAggro, 
//             actions, resistances, weaknesses);
//     }
//     
//     public static Unit CreateFaber()
//     {
//         var rng = new RandomNumberGenerator();
//         string id = "hero_3";
//         string name = "Faber";
//         int attack = rng.RandiRange(0, 3);
//         TargetSide side = TargetSide.Ally;
//         int maxHp = rng.RandiRange(5, 20);
//         int defaultSpeed = rng.RandiRange(5, 20);
//         int defaultMovement = rng.RandiRange(0, 2);
//         int defaultActionPoints = rng.RandiRange(0, 2);
//         float defaultAggro = rng.RandfRange(0.1f, 5);
//         List<Action> actions = [];
//         List<Element> resistances = [];
//         List<Element> weaknesses = [];
//         return new Unit(id, name, attack, side, maxHp, defaultSpeed, defaultMovement, defaultActionPoints, defaultAggro, 
//             actions, resistances, weaknesses);
//     }
//     
//     public static Unit CreateElaena()
//     {
//         string id = "hero_4";
//         string name = "Elaena";
//         int attack = 1;
//         TargetSide side = TargetSide.Ally;
//         int maxHp = 18;
//         int defaultSpeed = 13;
//         int defaultMovement = 1;
//         int defaultActionPoints = 1;
//         float defaultAggro = 0.8f;
//         List<Action> actions = [];
//         List<Element> resistances = [Element.Ice, Element.Fire, Element.Plant];
//         List<Element> weaknesses = [Element.Dark];
//         return new Unit(id, name, attack, side, maxHp, defaultSpeed, defaultMovement, defaultActionPoints, defaultAggro, 
//             actions, resistances, weaknesses);
//     }
//     
//     public static Unit CreateFrederic()
//     {
//         string id = "hero_5";
//         string name = "Frederic";
//         int attack = 2;
//         TargetSide side = TargetSide.Ally;
//         int maxHp = 28;
//         int defaultSpeed = 7;
//         int defaultMovement = 2;
//         int defaultActionPoints = 1;
//         float defaultAggro = 2f;
//         List<Action> actions = [];
//         List<Element> resistances = [Element.Physical, Element.Dark];
//         List<Element> weaknesses = [];
//         return new Unit(id, name, attack, side, maxHp, defaultSpeed, defaultMovement, defaultActionPoints, defaultAggro, 
//             actions, resistances, weaknesses);
//     }
//     
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