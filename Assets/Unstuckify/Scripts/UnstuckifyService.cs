using System;
using System.Linq;
using System.Linq.Expressions;
using PipetteTool;
using Timberborn.BlockSystem;
using Timberborn.GameDistricts;
using Timberborn.WalkingSystem;
using UnityEngine;

namespace Unstuckify.Scripts
{
    public class UnstuckifyService
    {
        private readonly DistrictCenterRegistry _districtCenterRegistry;
        private readonly WalkerService _walkerService;

        UnstuckifyService(DistrictCenterRegistry districtCenterRegistry, WalkerService walkerService)
        {
            _districtCenterRegistry = districtCenterRegistry;
            _walkerService = walkerService;
        }
        
        public Vector3 ClosestDistrictCenterPosition(Vector3 currentPosition)
        {
            try
            {
                var closestDistrictCenterPosition = _districtCenterRegistry.AllDistrictCenters
                    .Select(districtCenter =>
                    {
                        var coordinates = districtCenter.GetComponentFast<BlockObject>().PositionedEntrance.DoorstepCoordinates;
                        return new Vector3(coordinates.x, coordinates.z, coordinates.y);
                    })
                    .OrderBy(coordinates => Vector3.Distance(coordinates, currentPosition))
                    .First();

                var closestPositionOnNavMesh = _walkerService.ClosestPositionOnNavMesh(closestDistrictCenterPosition);
                // Plugin.Log.LogInfo("Trying to Unstuckify from " + currentPosition + " to " + closestPositionOnNavMesh);
                return closestPositionOnNavMesh;
            }
            catch (Exception)
            {
                Plugin.Log.LogError("You do not have any District Centers.");
                return currentPosition;
            }
        }
    }
}