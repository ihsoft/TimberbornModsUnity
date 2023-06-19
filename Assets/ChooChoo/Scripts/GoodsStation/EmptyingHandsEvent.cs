using Timberborn.Carrying;

namespace ChooChoo
{
    public class EmptyingHandsEvent
    {
        public readonly CarryRootBehavior Agent;
        
        public EmptyingHandsEvent(CarryRootBehavior agent)
        {
            Agent = agent;
        }
    }
}
