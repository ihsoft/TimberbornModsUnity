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
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Up),
                  new(new Vector3Int(0, 0, 0), Direction2D.Down),
               };
            case "TrackCorner1x1":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Up),
                  new(new Vector3Int(0, 0, 0), Direction2D.Right),
               };
            case "TrackCorner2x2":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Up),
                  new(new Vector3Int(1, 1, 0), Direction2D.Right),
               };
            case "TrackTIntersection":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Up),
                  new(new Vector3Int(0, 0, 0), Direction2D.Right),
                  new(new Vector3Int(0, 0, 0), Direction2D.Down),
               };
            case "TrainYard":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Down),
               };
            case "TrainStation":
               return new TrackConnection[]
               {
                  new(new Vector3Int(0, 0, 0), Direction2D.Up),
                  new(new Vector3Int(0, 0, 0), Direction2D.Right),
                  new(new Vector3Int(0, 0, 0), Direction2D.Left),
                  new(new Vector3Int(0, 0, 0), Direction2D.Down),
               };
            default:
               throw new ArgumentOutOfRangeException($"Unexpected Track object: {prefabName}");
         }
      }
      
      public Vector3[] GetPathCorners(string prefabName)
      {
         prefabName = FixPrefabName(prefabName);
         Plugin.Log.LogWarning("Providing Path Corners: " + prefabName);
         switch (prefabName)
         {
            case "TrackStraight":
               return new Vector3[]
               {
                  new(0.5f, 0, 0.5f)
               };
            case "TrackCorner1x1":
               return new Vector3[]
               {
                  new(0.5f, 0, 0.5f)
               };
            case "TrackCorner2x2":
               return new Vector3[]
               {
                  new(0.5f, 0, 0.5f)
               };
            case "TrackTIntersection":
               return new Vector3[]
               {
                  new(0.5f, 0, 0.5f)
               };
            case "TrainYard":
               return new Vector3[]
               {
                  new(0.5f, 0, 0.5f)
               };
            case "TrainStation":
               return new Vector3[]
               {
                  new(0.5f, 0, 0.5f)
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
