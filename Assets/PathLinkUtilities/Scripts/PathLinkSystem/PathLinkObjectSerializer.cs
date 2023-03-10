using Timberborn.Persistence;

namespace PathLinkUtilities
{
  public class PathLinkObjectSerializer : IObjectSerializer<PathLink>
  {
    private static readonly PropertyKey<PathLinkPoint> StartLinkPointKey = new("StartLinkPoint");
    private static readonly PropertyKey<PathLinkPoint> EndLinkPointKey = new("EndLinkPoint");
    private static readonly PropertyKey<float> WaitingTimeInHoursKey = new("WaitingTimeInHours");

    public void Serialize(PathLink value, IObjectSaver objectSaver)
    {
      objectSaver.Set(StartLinkPointKey, value.StartLinkPoint);
      objectSaver.Set(EndLinkPointKey, value.EndLinkPoint);
      objectSaver.Set(WaitingTimeInHoursKey, value.WaitingTimeInHours);
    }

    public Obsoletable<PathLink> Deserialize(IObjectLoader objectLoader)
    {
      if (objectLoader.Has(StartLinkPointKey) && objectLoader.Has(EndLinkPointKey) && objectLoader.Has(WaitingTimeInHoursKey))
      {
        PathLinkPoint startLinkPoint = objectLoader.Get(StartLinkPointKey);
        PathLinkPoint endLinkPoint = objectLoader.Get(EndLinkPointKey);
        float waitingTimeInHours = objectLoader.Get(WaitingTimeInHoursKey);
        if (startLinkPoint != null && endLinkPoint != null)
          return (Obsoletable<PathLink>) new PathLink(startLinkPoint, endLinkPoint, waitingTimeInHours);
      }
      return new Obsoletable<PathLink>();
    }
  }
}
