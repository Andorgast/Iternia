// using Iternia.Scripts.Core;
//
// namespace Iternia.Scripts.Models;
//
// public class ActionCreator
// {
//     public Action MakeDarkness()
//     {
//         string id = "thalia_1";
//         string name = "Darkness";
//         Element attackType = Element.Dark;
//         int damage = 1;
//         int secondaryDamage = 1;
//         TileMask originSquares = TileMask.Parse(".x./" +
//                                                 "..x/" +
//                                                 ".x.");
//         
//         TileMask targetSquares = TileMask.Parse("x../" +
//                                                 "xx./" +
//                                                 "x..");
//         bool mustTargetUnit = false;
//         return new Action(id, name, attackType, damage, secondaryDamage, originSquares, targetSquares, mustTargetUnit);
//     }
//     
//     public Action MakeRapier(int attackStat)
//     {
//         string id = "thalia_2";
//         string name = "Rapier";
//         Element attackType = Element.Physical;
//         int damage = 6 + attackStat;
//         int secondaryDamage = -1;
//         //this is healing yourself
//         TileMask originSquares = TileMask.Parse("xx./" +
//                                                 "x../" +
//                                                 "xx.");
//         
//         TileMask targetSquares= TileMask.Parse("x../" +
//                                                "x../" +
//                                                "x..");
//         bool mustTargetUnit = true;
//         return new Action(id, name, attackType, damage, secondaryDamage,originSquares, targetSquares, mustTargetUnit);
//     }
//     
//     public Action MakeBloodToss(int attackStat)
//     {
//         string id = "thalia_3";
//         string name = "Blood toss";
//         Element attackType = Element.Dark;
//         int damage = 12 + attackStat;
//         int secondaryDamage = 4;
//         //This is self damage;
//         TileMask originSquares = TileMask.Parse(".../" +
//                                                 ".xx/" +
//                                                 "...");
//         
//         TileMask targetSquares= TileMask.Parse(".xx/" +
//                                                "x.x/" +
//                                                ".xx");
//         bool mustTargetUnit = true;
//         Action action = new Action(id, name, attackType, damage, secondaryDamage, originSquares, targetSquares,
//              mustTargetUnit);
//         
//         return action;
//     }
// }