namespace ChooChoo
{
    public class TeleporterLink
    {
        public readonly int StartNodeId;
        
        public readonly int GoToNodeId;

        public TeleporterLink(int startNodeId, int goToNodeId)
        {
            StartNodeId = startNodeId;
            GoToNodeId = goToNodeId;
        }
    }
}