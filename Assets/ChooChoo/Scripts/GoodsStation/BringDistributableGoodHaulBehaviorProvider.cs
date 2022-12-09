// Decompiled with JetBrains decompiler
// Type: Timberborn.DistributionSystem.BringDistributableGoodHaulBehaviorProvider
// Assembly: Timberborn.DistributionSystem, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C616A16C-F6F1-4897-8555-D530DF676979
// Assembly location: C:\Users\Tobbert\SynologyDrive\C# Projecten\TobbertMods\TobbertMods\Timberborn\Timberborn_Data\Managed\Timberborn.DistributionSystem.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.Buildings;
using Timberborn.Common;
using Timberborn.Goods;
using Timberborn.Hauling;
using Timberborn.WorkSystem;
using UnityEngine;

namespace ChooChoo
{
  internal class BringDistributableGoodHaulBehaviorProvider : MonoBehaviour, IHaulBehaviorProvider
  {
    private GoodsStation _distributionPost;
    private BringDistributableGoodWorkplaceBehavior _bringDistributableGoodWorkplaceBehavior;
    private BlockableBuilding _blockableBuilding;
    private readonly List<GoodAmount> _lackingGoods = new List<GoodAmount>();

    public void Awake()
    {
      this._distributionPost = this.GetComponent<GoodsStation>();
      this._bringDistributableGoodWorkplaceBehavior = this.GetComponent<BringDistributableGoodWorkplaceBehavior>();
      this._blockableBuilding = this.GetComponent<BlockableBuilding>();
    }

    public IEnumerable<WeightedBehavior> GetWeightedBehaviors()
    {
      if ((bool) (UnityEngine.Object) this._distributionPost && this._distributionPost.Inventory.enabled && this._blockableBuilding.IsUnblocked)
      {
        float lackingPercentage = this.GetLackingPercentage();
        if ((double) lackingPercentage > 0.0)
          yield return new WeightedBehavior(lackingPercentage, (WorkplaceBehavior) this._bringDistributableGoodWorkplaceBehavior);
      }
    }

    private float GetLackingPercentage()
    {
      this._lackingGoods.Clear();
      this._distributionPost.LackingGoods(this._lackingGoods);
      return !this._lackingGoods.IsEmpty<GoodAmount>() ? this._lackingGoods.Max<GoodAmount>(new Func<GoodAmount, float>(this.GoodLackingPercentage)) : 0.0f;
    }

    private float GoodLackingPercentage(GoodAmount goodAmount) => (float) goodAmount.Amount / (float) this._distributionPost.MaxAllowedAmount(goodAmount.GoodId);
  }
}
