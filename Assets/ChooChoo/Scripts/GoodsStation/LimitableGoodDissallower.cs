﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.ConstructibleSystem;
using Timberborn.Goods;
using Timberborn.Persistence;

namespace Timberborn.InventorySystem
{
    public class LimitableGoodDisallower : BaseComponent, IFinishedStateListener, IPersistentEntity, IGoodDisallower
    {
        private static readonly ComponentKey LimitedGoodAllowerKey = new(nameof(LimitableGoodDisallower));
        private static readonly ListKey<GoodAmount> LimitsKey = new(nameof(_limits));
        private GoodAmountSerializer _goodAmountSerializer;
      
        private readonly Dictionary<string, int> _limits = new();

        public event EventHandler<DisallowedGoodsChangedEventArgs> DisallowedGoodsChanged;

        [Inject]
        public void InjectDependencies(GoodAmountSerializer savedGoodObjectSerializer)
        {
            _goodAmountSerializer = savedGoodObjectSerializer;
        }
        
        public void Awake()
        {
            enabled = false;
        }

        public int AllowedAmount(string goodId)
        {
            return !_limits.TryGetValue(goodId, out var num) ? int.MaxValue : num;
        }

        public void SetAllowedAmount(string goodId, int amount)
        {
            _limits[goodId] = amount;
            InvokeDisallowedGoodsChangedEvent(goodId);
        }

        public void OnEnterFinishedState()
        {
            enabled = true;
        }

        public void OnExitFinishedState()
        {
            enabled = false;
        }

        public void Save(IEntitySaver entitySaver)
        {
          IObjectSaver component = entitySaver.GetComponent(LimitedGoodAllowerKey);
          List<GoodAmount> savedGoods = _limits.Select(good => new GoodAmount(good.Key, good.Value)).ToList();
          component.Set(LimitsKey, savedGoods, _goodAmountSerializer);
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.HasComponent(LimitedGoodAllowerKey))
                return;
            IObjectLoader component = entityLoader.GetComponent(LimitedGoodAllowerKey);
            if (!component.Has(LimitsKey))
                return;
            foreach (var limit in component.Get(LimitsKey, _goodAmountSerializer))
            {
              _limits.Add(limit.GoodId, limit.Amount);
            }
        }

        private void InvokeDisallowedGoodsChangedEvent(string goodId)
        {
          EventHandler<DisallowedGoodsChangedEventArgs> disallowedGoodsChanged = DisallowedGoodsChanged;
          if (disallowedGoodsChanged == null)
            return;
          disallowedGoodsChanged(this, new DisallowedGoodsChangedEventArgs(goodId));
        }
    }
}