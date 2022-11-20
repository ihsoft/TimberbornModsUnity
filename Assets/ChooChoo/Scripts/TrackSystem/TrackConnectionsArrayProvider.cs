using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
   public class TrackConnectionsArrayProvider
   {
      public TrackConnection[] GetConnections(string prefabName)
      {
         prefabName = prefabName.Replace("(Clone)", "");
         Plugin.Log.LogWarning("Providing Connections: " + prefabName);
         switch (prefabName)
         {
            case "TrackStraight":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Up),
                  new(new Vector3Int(0, 0, 0), Direction2D.Down),
               };
            case "TrackTIntersection":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Up),
                  new(new Vector3Int(0, 0, 0), Direction2D.Right),
                  new(new Vector3Int(0, 0, 0), Direction2D.Down),
               };
            default:
               return new TrackConnection[] { };
         }
      }
   }
}
