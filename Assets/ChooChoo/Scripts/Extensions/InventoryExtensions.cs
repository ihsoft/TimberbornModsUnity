using Timberborn.InventorySystem;

namespace ChooChoo
{
    public static class InventoryExtensions
    {
        public static int UnreservedAmountInStockAndIncoming(this Inventory @inventory, string goodId)
        {
            return @inventory.UnreservedAmountInStock(goodId) + @inventory.ReservedCapacity(goodId);
        }
    }
}