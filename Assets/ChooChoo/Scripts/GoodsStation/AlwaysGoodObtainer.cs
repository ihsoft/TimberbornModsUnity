using Timberborn.Persistence;
using Timberborn.StockpilePrioritySystem;

namespace ChooChoo
{
    public class AlwaysGoodObtainer : GoodObtainer, IPostInitializableLoadedEntity
    {
        public void PostInitializeLoadedEntity()
        {
            EnableGoodObtaining();
        }
    }
}