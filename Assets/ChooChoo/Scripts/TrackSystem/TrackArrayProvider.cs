using System;
using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
   public class TrackArrayProvider
   {
      public TrackRoute[] GetConnections(string prefabName)
      {
         prefabName = FixPrefabName(prefabName);
         // Plugin.Log.LogWarning("Providing Connections: " + prefabName);
         switch (prefabName)
         {
            case "TrackStraight":
               return new TrackRoute[]
               {
                  new(
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Up),
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Down), 
                     new[] { Vector3.zero }),
                  new(
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Down),
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Up), 
                     new[] { Vector3.zero })
               };
            case "TrackHill1x2":
               return new TrackRoute[]
               {
                  new(
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Up),
                     new TrackConnection(new Vector3Int(0, 1, 1), Direction2D.Down), 
                     new Vector3[]
                     {
                        new(0, 0, -1),
                        new(0, 1, 1)
                     }),
                  new(
                     new TrackConnection(new Vector3Int(0, 1, 1), Direction2D.Down),
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Up), 
                     new Vector3[]
                     {
                        new(0, 1, 1),
                        new(0, 0, -1),
                     })
               };
            case "TrackCorner1x1":
               return new TrackRoute[]
               {
                  new(
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Up), 
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Right), 
                     new[] { Vector3.zero }),
                  new(
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Right),
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Up), 
                     new[] { Vector3.zero })
               };
            case "TrackCorner2x2":
               return new TrackRoute[]
               {
                  new(
                     new(new Vector3Int(1, 0, 0), Direction2D.Up), 
                     new(new Vector3Int(0, 1, 0), Direction2D.Right), new Vector3[]
                     {
                        new(0.5f, 0, -1f),
                        new(0.4f, 0, -0.35f),
                        new(0.1f, 0, 0.1f),
                        new(-0.35f, 0, 0.4f),
                        new(-1f, 0, 0.5f),
                     }),
                  new(
                     new(new Vector3Int(0, 1, 0), Direction2D.Right),
                     new(new Vector3Int(1, 0, 0), Direction2D.Up),
                     
                     new Vector3[]
                     {
                        new(-1f, 0, 0.5f),
                        new(-0.35f, 0, 0.4f),
                        new(0.1f, 0, 0.1f),
                        new(0.4f, 0, -0.35f),
                        new(0.5f, 0, -1f),
                     })
               };
            case "TrackTIntersection1x1":
                return new TrackRoute[]
               {
                  new(
                     new(new Vector3Int(0, 0, 0), Direction2D.Up),
                     new(new Vector3Int(0, 0, 0), Direction2D.Right), 
                     new[] { Vector3.zero }),
                  new(
                     new(new Vector3Int(0, 0, 0), Direction2D.Right),
                     new(new Vector3Int(0, 0, 0), Direction2D.Up), 
                     new[] { Vector3.zero }),
                  new(
                     new(new Vector3Int(0, 0, 0), Direction2D.Up),
                     new(new Vector3Int(0, 0, 0), Direction2D.Down), 
                     new[] { Vector3.zero }),
                  new(
                     new(new Vector3Int(0, 0, 0), Direction2D.Down),
                     new(new Vector3Int(0, 0, 0), Direction2D.Up), 
                     new[] { Vector3.zero }),
                  new(
                     new(new Vector3Int(0, 0, 0), Direction2D.Down),
                     new(new Vector3Int(0, 0, 0), Direction2D.Right), 
                     new[] { Vector3.zero }),
                  new(
                     new(new Vector3Int(0, 0, 0), Direction2D.Right),
                     new(new Vector3Int(0, 0, 0), Direction2D.Down), 
                     new[] { Vector3.zero })
               };
            case "TrackTIntersection2x3":
               return new TrackRoute[]
               {
                  new(
                     new(new Vector3Int(1, 0, 0), Direction2D.Up), 
                     new(new Vector3Int(0, 1, 0), Direction2D.Right), 
                     new Vector3[]
                     {
                        new(0.5f, 0, -1.5f),
                        new(0.4f, 0, -0.85f),
                        new(0.1f, 0, -0.4f),
                        new(-0.35f, 0, -0.1f),
                        new(-1f, 0, 0f),
                     }),
                  new(
                     new(new Vector3Int(0, 1, 0), Direction2D.Right),
                     new(new Vector3Int(1, 0, 0), Direction2D.Up),
                     new Vector3[]
                     {
                        new(-1f, 0, 0f),
                        new(-0.35f, 0, -0.1f),
                        new(0.1f, 0, -0.4f),
                        new(0.4f, 0, -0.85f),
                        new(0.5f, 0, -1.5f),
                     }),
                  new(
                     new(new Vector3Int(1, 0, 0), Direction2D.Up),
                     new(new Vector3Int(1, 2, 0), Direction2D.Down), 
                     new Vector3[] { new(0.5f, 0, 0), }),
                  new(
                     new(new Vector3Int(1, 2, 0), Direction2D.Down),
                     new(new Vector3Int(1, 0, 0), Direction2D.Up), 
                     new Vector3[] { new(0.5f, 0, 0), }),
                  new(
                     new(new Vector3Int(1, 2, 0), Direction2D.Down),
                     new(new Vector3Int(0, 1, 0), Direction2D.Right), 
                     new Vector3[]
                     {
                        new(0.5f, 0, 1.5f),
                        new(0.4f, 0, 0.85f),
                        new(0.1f, 0, 0.4f),
                        new(-0.35f, 0, 0.1f),
                        new(-1f, 0, 0f),
                     }),
                  new(
                     new(new Vector3Int(0, 1, 0), Direction2D.Right),
                     new(new Vector3Int(1, 2, 0), Direction2D.Down), 
                     new Vector3[]
                     {
                        new(-1f, 0, 0f),
                        new(-0.35f, 0, 0.1f),
                        new(0.1f, 0, 0.4f),
                        new(0.4f, 0, 0.85f),
                        new(0.5f, 0, 1.5f),
                     }),
               };
            case "TrainYard":
               return new TrackRoute[]
               {
                  new(
                     new TrackConnection(new Vector3Int(0, 2, 0), Direction2D.Down), 
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Up), 
                     new Vector3[] { new(0, 0, -1.1f) }),
                  new(
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Up),
                     new TrackConnection(new Vector3Int(0, 2, 0), Direction2D.Down), 
                     new Vector3[] { new(0, 0, 1.1f) })
               };
            case "TrainStation":
               return new TrackRoute[]
               {
                  new(
                     new TrackConnection(new Vector3Int(2, 0, 0), Direction2D.Left),
                     new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Right),
                     new Vector3[]
                     {
                        new(1.2f, 0, -1f),
                        new(-1.2f, 0, -1f)
                     }),
                  new(new TrackConnection(new Vector3Int(0, 0, 0), Direction2D.Right),
                     new TrackConnection(new Vector3Int(2, 0, 0), Direction2D.Left),
                     new Vector3[]
                     {
                        new(-1.2f, 0, -1f),
                        new(1.2f, 0, -1f)
                     })
               };
            case "GoodsStation":
               return new TrackRoute[]
               {
                  new(
                     new TrackConnection(new Vector3Int(2, 2, 0), Direction2D.Left), 
                     new TrackConnection(new Vector3Int(0, 2, 0), Direction2D.Right), 
                     new Vector3[] { new(-1.2f, 0, 1f) }),
                  new(
                     new TrackConnection(new Vector3Int(0, 2, 0), Direction2D.Right), 
                     new TrackConnection(new Vector3Int(2, 2, 0), Direction2D.Left), 
                     new Vector3[] { new(1.2f, 0, 1f) })
               };
            case "WaitingStation":
               return new TrackRoute[]
               {
                  new(
                     new(new Vector3Int(0, 0, 0), Direction2D.Up),
                     new(new Vector3Int(0, 2, 0), Direction2D.Down), 
                     new Vector3[] { new(0, 0, 1.5f), }),
                  new(
                     new(new Vector3Int(0, 2, 0), Direction2D.Down),
                     new(new Vector3Int(0, 0, 0), Direction2D.Up), 
                     new Vector3[] { new(0, 0, -1.5f), }),
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
