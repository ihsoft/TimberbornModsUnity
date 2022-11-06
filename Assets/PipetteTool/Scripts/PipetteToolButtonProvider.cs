using TimberApi.DependencyContainerSystem;
using Timberborn.AssetSystem;
using Timberborn.BottomBarSystem;
using Timberborn.CoreUI;
using Timberborn.ToolSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace PipetteTool
{
    public class PipetteToolButtonProvider : IBottomBarElementProvider
    {
        private readonly VisualElementLoader _visualElementLoader;

        private readonly ToolManager _toolManager;

        private readonly IResourceAssetLoader _assetLoader;

        public PipetteToolButtonProvider(VisualElementLoader visualElementLoader, ToolManager toolManager, IResourceAssetLoader assetLoader)
        {
            _visualElementLoader = visualElementLoader;
            _toolManager = toolManager;
            _assetLoader = assetLoader;
        }

        public BottomBarElement GetElement()
        {
            VisualElement buttonVisualElement = _visualElementLoader.LoadVisualElement("Common/BottomBar/GrouplessToolButton");
            buttonVisualElement.AddToClassList("bottom-bar-button--blue");
            Sprite v = _assetLoader.Load<Sprite>("tobbert.pipettetool/tobbert_pipettetool/PipetteToolIcon");
            buttonVisualElement.Q<VisualElement>("ToolImage").style.backgroundImage = new StyleBackground(v);
            var pipetteTool = (Tool)DependencyContainer.GetInstance<IPipetteTool>();
            buttonVisualElement.Q<Button>("ToolButton").clicked += () => _toolManager.SwitchTool(pipetteTool);
            return BottomBarElement.CreateSingleLevel(buttonVisualElement);
        }
    }
}