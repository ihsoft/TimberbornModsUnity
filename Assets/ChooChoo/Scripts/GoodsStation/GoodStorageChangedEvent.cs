namespace ChooChoo
{
    public class GoodStorageChangedEvent
    {
        public string GoodId;
        
        public GoodStorageChangedEvent(string goodId)
        {
            GoodId = goodId;
        }
    }
}
