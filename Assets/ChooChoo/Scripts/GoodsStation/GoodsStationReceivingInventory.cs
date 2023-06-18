using Timberborn.BaseComponentSystem;
using Timberborn.Common;
using Timberborn.InventorySystem;

namespace ChooChoo
{
    public class GoodsStationReceivingInventory : BaseComponent
    {
        public Inventory Inventory { get; set; }
        
        public void InitializeReceivingInventory(Inventory inventory)
        {
            Asserts.FieldIsNull(this, Inventory, "Inventory");
            Inventory = inventory;
        }
    }
}