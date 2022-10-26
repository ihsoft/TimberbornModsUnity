using Timberborn.ConstructionMode;
using Timberborn.Core;
using Timberborn.Debugging;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;

namespace PipetteTool
{
    public class PipetteToolInGame : PipetteTool
    {
        private readonly ConstructionModeService _constructionModeService;

        public PipetteToolInGame(EventBus eventBus, ToolManager toolManager, DevModeManager devModeManager,
            InputService inputService, ConstructionModeService constructionModeService, MapEditorMode mapEditorMode,
            CursorService cursorService, DescriptionPanel descriptionPanel, ILoc loc) : base(eventBus, toolManager, devModeManager, inputService,
            mapEditorMode, cursorService, descriptionPanel, loc)
        {
            _constructionModeService = constructionModeService;
        }

        protected override void ExitPipetteTool()
        {
            base.ExitPipetteTool();
            ExitConstructionModeMethod.Invoke(_constructionModeService, new object[] { });
        }

        protected override void SwitchToSelectedBuildingTool(Tool tool)
        {
            base.SwitchToSelectedBuildingTool(tool);
            EnterConstructionModeMethod.Invoke(_constructionModeService, new object[] { });
        }
    }
}