// using System.Reflection;
// using HarmonyLib;
// using Timberborn.BlockSystem;
//
// namespace TobbyTools.DynamicSpecificationSystem
// {
//     [HarmonyPatch]
//     public class BlockObjectPatch
//     {
//         static MethodInfo TargetMethod()
//         {
//             return AccessTools.Method(AccessTools.TypeByName("Blocks"), "From", new []
//             {
//                 typeof(BlocksSpecification)
//             });
//         }
//     
//         static void Prefix(BlocksSpecification blocksSpecification)
//         {
//             Plugin.Log.LogInfo(blocksSpecification.BlockSpecifications.Length + "");
//             Plugin.Log.LogInfo(blocksSpecification.Size + "");
//         }
//     }
// }