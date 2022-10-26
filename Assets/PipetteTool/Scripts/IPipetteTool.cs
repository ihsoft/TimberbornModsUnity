using Timberborn.SelectionSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace PipetteTool
{
    public interface IPipetteTool
    {
        string CursorKey { get; }
        void AddToolButtonToDictionary(GameObject gameObject, ToolButton toolButton);
        void OnSelectableObjectSelected(SelectableObject selectableObject);
    }
}