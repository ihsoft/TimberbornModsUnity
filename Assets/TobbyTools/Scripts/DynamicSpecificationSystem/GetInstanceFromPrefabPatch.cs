// using System.Collections.Generic;
// using System.Reflection;
// using HarmonyLib;
//
// namespace TobbyTools.DynamicSpecificationSystem
// {
//     [HarmonyPatch]
//     public class BlockObjectToolPatch
//     {
//         public static IEnumerable<MethodInfo> TargetMethods()
//         {
//             var methodInfoList = new List<MethodInfo>
//             {
//                 AccessTools.Method(AccessTools.TypeByName("IBindingBuilder"), "ToInstance")
//             };
//
//             return methodInfoList;
//         }
//         
//         static void Postfix()
//         {
//             // Modify the returned result here, if needed
//             // __result;
//         }
//     }
// }