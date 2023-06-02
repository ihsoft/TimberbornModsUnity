using Timberborn.Persistence;

namespace ChooChoo
{
  public class PassengerStationLinkObjectSerializer : IObjectSerializer<PassengerStationLink>
  {
    private static readonly PropertyKey<PassengerStation> StartLinkPointKey = new("StartLinkPoint");
    private static readonly PropertyKey<PassengerStation> EndLinkPointKey = new("EndLinkPoint");
    private static readonly PropertyKey<float> WaitingTimeInHoursKey = new("WaitingTimeInHours");

    public void Serialize(PassengerStationLink value, IObjectSaver objectSaver)
    {
      objectSaver.Set(StartLinkPointKey, value.StartLinkPoint);
      objectSaver.Set(EndLinkPointKey, value.EndLinkPoint);
      objectSaver.Set(WaitingTimeInHoursKey, value.WaitingTimeInHours);
    }

    public Obsoletable<PassengerStationLink> Deserialize(IObjectLoader objectLoader)
    {
      if (objectLoader.Has(StartLinkPointKey) && objectLoader.Has(EndLinkPointKey) && objectLoader.Has(WaitingTimeInHoursKey))
      {
        PassengerStation startLinkPoint = objectLoader.Get(StartLinkPointKey);
        PassengerStation endLinkPoint = objectLoader.Get(EndLinkPointKey);
        float waitingTimeInHours = objectLoader.Get(WaitingTimeInHoursKey);
        if (startLinkPoint != null && endLinkPoint != null)
          return (Obsoletable<PassengerStationLink>) new PassengerStationLink(startLinkPoint, endLinkPoint, waitingTimeInHours);
      }
      return new Obsoletable<PassengerStationLink>();
    }
  }
}
