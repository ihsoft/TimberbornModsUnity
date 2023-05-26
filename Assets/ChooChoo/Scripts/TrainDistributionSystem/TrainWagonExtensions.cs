using Timberborn.InventorySystem;

namespace ChooChoo
{
    public static class TrainWagonExtensions
    {
        public static bool IsMaximumReserved(this TrainWagon trainWagon, TrainCarryAmountCalculator trainCarryAmountCalculator)
        {
            if (trainWagon.GoodReserver.HasReservedStock)
            {
                var maxAmount = trainCarryAmountCalculator.MaxAmountToCarry(trainWagon.GoodCarrier.LiftingCapacity, trainWagon.GoodReserver.StockReservation.GoodAmount.GoodId);
                
                return maxAmount <= trainWagon.GoodReserver.StockReservation.GoodAmount.Amount;
            }

            return false;
        }
    }
}