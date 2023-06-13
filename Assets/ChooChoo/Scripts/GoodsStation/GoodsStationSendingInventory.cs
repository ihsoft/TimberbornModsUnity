using Timberborn.BaseComponentSystem;
using Timberborn.Common;
using Timberborn.InventorySystem;

namespace ChooChoo.Scripts.GoodsStation
{
    public class GoodsStationSendingInventory : BaseComponent
    {
        public Inventory Inventory { get; set; }
        
        public void InitializeSendingInventory(Inventory inventory)
        {
            Asserts.FieldIsNull(this, Inventory, "Inventory");
            Inventory = inventory;
        }
    }
}