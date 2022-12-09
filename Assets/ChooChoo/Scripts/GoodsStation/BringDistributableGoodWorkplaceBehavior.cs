using Timberborn.BehaviorSystem;
using Timberborn.WorkSystem;
using UnityEngine;

namespace ChooChoo
{
  internal class BringDistributableGoodWorkplaceBehavior : WorkplaceBehavior
  {
    private GoodsStation _distributionPost;

    public void Awake() => this._distributionPost = this.GetComponent<GoodsStation>();

    public override Decision Decide(GameObject agent) => !agent.GetComponent<DistributableGoodBringer>().BringDistributableGoods(this._distributionPost) ? Decision.ReleaseNow() : Decision.ReleaseNextTick();
  }
}
