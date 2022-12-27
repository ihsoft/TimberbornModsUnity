using System;
using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
   public class TrackArrayProvider
   {
      public TrackConnection[] GetConnections(string prefabName)
      {
         prefabName = FixPrefabName(prefabName);
         Plugin.Log.LogWarning("Providing Connections: " + prefabName);
         switch (prefabName)
         {
            case "TrackStraight":
               return new[]
               {
                  new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Up, new[] {Vector3.zero}),
                  new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Down, new[] {Vector3.zero}),
               };
            case "TrackCorner1x1":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Up, new[] {Vector3.zero}),
                  new(new Vector3Int(0, 0, 0), Direction2D.Right, new[] {Vector3.zero}),
               };
            case "TrackCorner2x2":
               return new TrackConnection[]
               {
                  new(new Vector3Int(1, 0, 0), Direction2D.Up, new Vector3[] {
                     new(-1f, 0, 0.5f),
                     new(-0.35f, 0, 0.4f),
                     new(0.1f, 0, 0.1f),
                     new(0.4f, 0, -0.35f),
                     new(0.5f, 0, -1f),
                  }),
                  new(new Vector3Int(0, 1, 0), Direction2D.Right, new Vector3[] {
                     new(0.5f, 0, -1f),
                     new(0.4f, 0, -0.35f),
                     new(0.1f, 0, 0.1f),
                     new(-0.35f, 0, 0.4f),
                     new(-1f, 0, 0.5f),
                  }),
               };
            case "TrackTIntersection":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Up, new[] {Vector3.zero}),
                  new(new Vector3Int(0, 0, 0), Direction2D.Right, new[] {Vector3.zero}),
                  new(new Vector3Int(0, 0, 0), Direction2D.Down, new[] {Vector3.zero}),
               };
            case "TrainYard":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 2, 0), Direction2D.Down, new Vector3[] { new(0, 0, -1.1f) }),
                  new(new Vector3Int(0, 0, 0), Direction2D.Up, new Vector3[] { new(0, 0, 1.1f) }),
               };
            case "TrainStation":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Right, new Vector3[]
                  {
                     new(-1.2f, 0, -1f),
                     new(1.2f, 0, -1f)
                  }),
                  new(new Vector3Int(2, 0, 0), Direction2D.Left, new Vector3[]
                  {
                     new(1.2f, 0, -1f),
                     new(-1.2f, 0, -1f)
                  }),
               };
            case "GoodsStation":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 2, 0), Direction2D.Right, new Vector3[] { new(1.2f, 0, 1f) }),
                  new(new Vector3Int(2, 2, 0), Direction2D.Left, new Vector3[] { new(-1.2f, 0, 1f) }),
               };
            default:
               throw new ArgumentOutOfRangeException($"Unexpected Track object: {prefabName}");
         }
      }

      private string FixPrefabName(string prefabName)
      {
         return prefabName
            .Replace("(Clone)", "")
            .Replace(".Folktails", "")
            .Replace(".IronTeeth", "");
      }
   }
}
