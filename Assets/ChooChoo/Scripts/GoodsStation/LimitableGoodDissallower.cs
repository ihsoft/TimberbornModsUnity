using System;
using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.ConstructibleSystem;

namespace Timberborn.InventorySystem
{
    public class LimitableGoodDisallower : BaseComponent, IGoodDisallower, IFinishedStateListener
    {
        private readonly Dictionary<string, int> _limits = new();

        public event EventHandler<DisallowedGoodsChangedEventArgs> DisallowedGoodsChanged;

        public void Awake() => enabled = false;

        public int AllowedAmount(string goodId)
        {
            return !_limits.TryGetValue(goodId, out var num) ? int.MaxValue : num;
        }

        public void SetAllowedAmount(string goodId, int amount)
        {
            _limits[goodId] = amount;
            EventHandler<DisallowedGoodsChangedEventArgs> disallowedGoodsChanged = DisallowedGoodsChanged;
            if (disallowedGoodsChanged == null)
                return;
            disallowedGoodsChanged(this, new DisallowedGoodsChangedEventArgs(goodId));
        }

        public void OnEnterFinishedState()
        {
            enabled = true;
        }

        public void OnExitFinishedState()
        {
            enabled = false;
        }
    }
}