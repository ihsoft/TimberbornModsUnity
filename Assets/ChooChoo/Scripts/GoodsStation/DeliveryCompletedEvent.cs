using Timberborn.Carrying;

namespace ChooChoo
{
    public class DeliveryCompletedEvent
    {
        public readonly CarryRootBehavior Agent;
        
        public DeliveryCompletedEvent(CarryRootBehavior agent)
        {
            Agent = agent;
        }
    }
}
