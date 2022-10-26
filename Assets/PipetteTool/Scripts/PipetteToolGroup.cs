// using Timberborn.AssetSystem;
// using Timberborn.SingletonSystem;
// using Timberborn.ToolSystem;
// using UnityEngine;
//
// namespace PipetteTool
// {
//     public class PipetteToolGroup : ToolGroup, ILoadableSingleton
//     {
//         private readonly IResourceAssetLoader _resourceAssetLoader;
//         
//         public PipetteToolGroup(IResourceAssetLoader resourceAssetLoader)
//         {
//             _resourceAssetLoader = resourceAssetLoader;
//         }
//
//         public override string IconName => "PipetteToolIcon";
//
//         public override string DisplayNameLocKey => "Kyp.ToolGroups.DraggableUtils";
//         
//         public void Load()
//         {
//             Icon = _resourceAssetLoader.Load<Sprite>("tobbert.pipettetool/tobbert_pipettetool/PipetteToolIcon");
//         }
//     }
// }