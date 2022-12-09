using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.Goods;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class DistrictDistributionLimits : MonoBehaviour, IPersistentEntity
  {
    private static readonly int DefaultLowLimit = 0;
    private static readonly int DefaultHighLimit = 100;
    private static readonly ComponentKey DistrictDistributionLimitsKey = new ComponentKey(nameof (DistrictDistributionLimits));
    private static readonly ListKey<GoodAmount> LowLimitsKey = new ListKey<GoodAmount>("LowLimits");
    private static readonly ListKey<GoodAmount> HighLimitsKey = new ListKey<GoodAmount>("HighLimits");
    private static readonly ListKey<SavedGood> DeactivatedHighLimitsKey = new ListKey<SavedGood>("DeactivatedHighLimits");
    private GoodAmountSerializer _goodAmountSerializer;
    private SavedGoodObjectSerializer _savedGoodObjectSerializer;
    private Dictionary<string, int> _lowLimits = new Dictionary<string, int>();
    private Dictionary<string, int> _highLimits = new Dictionary<string, int>();
    private readonly HashSet<string> _deactivatedHighLimits = new HashSet<string>();

    [Inject]
    public void InjectDependencies(
      GoodAmountSerializer goodAmountSerializer,
      SavedGoodObjectSerializer savedGoodObjectSerializer)
    {
      this._goodAmountSerializer = goodAmountSerializer;
      this._savedGoodObjectSerializer = savedGoodObjectSerializer;
    }

    public void SetLowLimit(string good, int lowLimit)
    {
      this._lowLimits[good] = lowLimit;
      int highLimit;
      if (!this.TryGetHighLimit(good, out highLimit) || highLimit >= lowLimit)
        return;
      this.SetHighLimit(good, lowLimit);
    }

    public int GetLowLimit(string good)
    {
      int num;
      return !this._lowLimits.TryGetValue(good, out num) ? DistrictDistributionLimits.DefaultLowLimit : num;
    }

    public void SetHighLimit(string good, int highLimit)
    {
      this._deactivatedHighLimits.Remove(good);
      this._highLimits[good] = highLimit;
      if (this.GetLowLimit(good) <= highLimit)
        return;
      this.SetLowLimit(good, highLimit);
    }

    public bool TryGetHighLimit(string good, out int highLimit)
    {
      if (this._deactivatedHighLimits.Contains(good))
      {
        highLimit = 0;
        return false;
      }
      if (!this._highLimits.TryGetValue(good, out highLimit))
        highLimit = DistrictDistributionLimits.DefaultHighLimit;
      return true;
    }

    public void ResetHighLimit(string good) => this.SetHighLimit(good, Mathf.Max(this.GetLowLimit(good), DistrictDistributionLimits.DefaultHighLimit));

    public void DeactivateHighLimit(string good)
    {
      this._highLimits.Remove(good);
      this._deactivatedHighLimits.Add(good);
    }

    public void Save(IEntitySaver entitySaver)
    {
      IObjectSaver component = entitySaver.GetComponent(DistrictDistributionLimits.DistrictDistributionLimitsKey);
      component.Set<GoodAmount>(DistrictDistributionLimits.LowLimitsKey, (IReadOnlyCollection<GoodAmount>) DistrictDistributionLimits.LimitsToList(this._lowLimits), (IObjectSerializer<GoodAmount>) this._goodAmountSerializer);
      component.Set<GoodAmount>(DistrictDistributionLimits.HighLimitsKey, (IReadOnlyCollection<GoodAmount>) DistrictDistributionLimits.LimitsToList(this._highLimits), (IObjectSerializer<GoodAmount>) this._goodAmountSerializer);
      component.Set<SavedGood>(DistrictDistributionLimits.DeactivatedHighLimitsKey, (IReadOnlyCollection<SavedGood>) this._deactivatedHighLimits.Select<string, SavedGood>(new Func<string, SavedGood>(SavedGood.Create)).ToList<SavedGood>(), (IObjectSerializer<SavedGood>) this._savedGoodObjectSerializer);
    }

    [BackwardCompatible(2021, 5, 27)]
    public void Load(IEntityLoader entityLoader)
    {
      if (!entityLoader.HasComponent(DistrictDistributionLimits.DistrictDistributionLimitsKey))
        return;
      IObjectLoader component = entityLoader.GetComponent(DistrictDistributionLimits.DistrictDistributionLimitsKey);
      this._lowLimits = DistrictDistributionLimits.LimitsFromList(component.Get<GoodAmount>(DistrictDistributionLimits.LowLimitsKey, (IObjectSerializer<GoodAmount>) this._goodAmountSerializer));
      this._highLimits = DistrictDistributionLimits.LimitsFromList(component.Get<GoodAmount>(DistrictDistributionLimits.HighLimitsKey, (IObjectSerializer<GoodAmount>) this._goodAmountSerializer));
      if (!component.Has<SavedGood>(DistrictDistributionLimits.DeactivatedHighLimitsKey))
        return;
      foreach (SavedGood savedGood in component.Get<SavedGood>(DistrictDistributionLimits.DeactivatedHighLimitsKey, (IObjectSerializer<SavedGood>) this._savedGoodObjectSerializer))
        this._deactivatedHighLimits.Add(savedGood.Id);
    }

    private static List<GoodAmount> LimitsToList(Dictionary<string, int> limits) => limits.Select<KeyValuePair<string, int>, GoodAmount>((Func<KeyValuePair<string, int>, GoodAmount>) (pair => new GoodAmount(pair.Key, pair.Value))).ToList<GoodAmount>();

    private static Dictionary<string, int> LimitsFromList(List<GoodAmount> limits)
    {
      Dictionary<string, int> dictionary = new Dictionary<string, int>();
      foreach (GoodAmount limit in limits)
        dictionary[limit.GoodId] = limit.Amount;
      return dictionary;
    }
  }
}
