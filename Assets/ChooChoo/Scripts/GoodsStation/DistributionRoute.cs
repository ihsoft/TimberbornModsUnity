using Timberborn.Buildings;

namespace ChooChoo
{
  public class DistributionRoute
  {
    public GoodsStation Start { get; }

    public DropOffPoint End { get; }

    public string GoodId { get; }

    public float LastCompletionTimestamp { get; private set; }

    public float Length { get; private set; }

    public bool HasProblem { get; private set; }

    public bool HasProblemInstant { get; private set; }

    public DistributionRoute(
      GoodsStation start,
      DropOffPoint end,
      string goodId,
      float lastCompletionTimestamp = 0.0f)
    {
      this.Start = start;
      this.End = end;
      this.GoodId = goodId;
      this.LastCompletionTimestamp = lastCompletionTimestamp;
    }

    public bool IsPaused => !this.Start.GetComponent<BlockableBuilding>().IsUnblocked || !this.End.GetComponent<BlockableBuilding>().IsUnblocked;

    public void UpdateLastCompletionTime(float day) => this.LastCompletionTimestamp = day;

    public bool IsMoreStaleThan(DistributionRoute distributionRoute) => (double) this.LastCompletionTimestamp < (double) distributionRoute.LastCompletionTimestamp;

    public void UpdateLength(float length) => this.Length = length;

    public void MarkAsHavingProblem() => this.HasProblem = true;

    public void UnmarkAsHavingProblem() => this.HasProblem = false;

    public void MarkAsHavingProblemInstant() => this.HasProblemInstant = true;

    public void UnmarkAsHavingProblemInstant() => this.HasProblemInstant = false;
  }
}
