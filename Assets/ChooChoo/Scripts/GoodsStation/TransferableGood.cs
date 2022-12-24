namespace ChooChoo
{
    public class TransferableGood
    {
        public string GoodId;

        public bool Enabled;

        public bool CanReceiveGoods;

        public TransferableGood(string goodId, bool enabled, bool isInput)
        {
            GoodId = goodId;
            Enabled = enabled;
            CanReceiveGoods = isInput;
        }
    }
}