using Timberborn.ToolSystem;
using UnityEngine;

namespace PipetteTool
{
    public interface IPipetteTool
    {
        string CursorKey { get; }
        void PostProcessInput();
        void AddToolButtonToDictionary(GameObject gameObject, ToolButton toolButton);
        void OnSelectableObjectSelected(GameObject hitObject);
    }
}