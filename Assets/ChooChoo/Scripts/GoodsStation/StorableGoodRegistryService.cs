// using System.Linq;
// using Timberborn.Goods;
// using Timberborn.InventorySystem;
//
// namespace ChooChoo
// {
//     public class StorableGoodRegistryService
//     {
//         private readonly ChooChooCore _chooChooCore;
//
//         StorableGoodRegistryService(ChooChooCore chooChooCore)
//         {
//             _chooChooCore = chooChooCore;
//         }
//
//         public void ChangeGoodToInput(Inventory inventory, string goodId)
//         {
//             ChangeStorableGood(inventory, goodId, true);
//         }
//         
//         public void ChangeGoodToOutput(Inventory inventory, string goodId)
//         {
//             ChangeStorableGood(inventory, goodId, false);
//         }
//
//         private void ChangeStorableGood(Inventory inventory, string goodId, bool isGivable)
//         {
//             var storableGoodAmounts = inventory.AllowedGoods.ToList();
//
//             for (var i = 0; i < storableGoodAmounts.Count; i++)
//             {
//                 var storableGoodAmount = storableGoodAmounts[i];
//                 if (storableGoodAmount.StorableGood.GoodId == goodId)
//                 {
//                     var test = storableGoodAmount.StorableGood;
//                     _chooChooCore.SetPrivateProperty(test, "Takeable", !isGivable);
//                     _chooChooCore.SetPrivateProperty(test, "Givable", isGivable);
//                     storableGoodAmounts[i] = new StorableGoodAmount(, storableGoodAmount.Amount);
//                     storableGoodAmount.StorableGood.
//                 }
//             }
//             
//             inventory.AllowedGoods
//         }
//     }
// }