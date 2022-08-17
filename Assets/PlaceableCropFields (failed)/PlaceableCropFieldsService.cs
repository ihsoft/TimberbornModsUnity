using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.AreaSelectionSystem;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.Core;
using Timberborn.EntitySystem;
using Timberborn.Navigation;
using Timberborn.PlantingUI;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace PlaceableCropFields
{
    public class PlaceableCropFieldsService
    {
        public GridTraversal _gridTraversal;
        public BlockService _blockService;
        public ToolManager _toolManager;

        public PlaceableCropFieldsService(GridTraversal gridTraversal, BlockService blockService, ToolManager toolManager)
        {
            _gridTraversal = gridTraversal;
            _blockService = blockService;
            _toolManager = toolManager;
        }

        public bool Test(AreaPicker __instance, Ray startRay, Ray endRay,  TerrainPicker ____terrainPicker, AreaIterator ____areaIterator, IEnumerable<Vector3Int> __result)
        {
            
            foreach (TraversedCoordinates traversedCoordinates in _gridTraversal.TraverseRay(startRay))
            {
                Vector3Int coordinates1 = traversedCoordinates.Coordinates;
                var buildings = _blockService.GetObjectsWithComponentAt<Building>(coordinates1);

                // foreach (var building in buildings)
                // {
                //     Plugin.Log.LogFatal(building.ScienceCost);
                // }
                if (buildings.Any())
                {
                    var end = coordinates1;
                    end.z += 1;
                    // Vector3Int end = coordinates1.z + 1;

                    // Vector3Int end = ____terrainPicker.FindCoordinatesOnLevelInMap(endRay, (coordinates1.z + 1)) ?? coordinates1;
                    __result = ____areaIterator.GetRectangle(new Vector3Int(0, 0, 5 ), new Vector3Int(1, 1, 5 ), 2500);
                    return false;
                }
            }

            return true;
            // nullable = new TraversedCoordinates?();

            // return false;
            // Plugin.Log.LogInfo(ray.origin);
            // foreach (TraversedCoordinates traversedCoordinates in ____gridTraversal.TraverseRay(ray))
            // {
            //     
            //     Plugin.Log.LogFatal(traversedCoordinates.Coordinates);
            //     // Vector3Int coordinates = traversedCoordinates.Coordinates;
            //     // if (predicate(coordinates))
            //     //     return new TraversedCoordinates?(traversedCoordinates);
            // }
        }

        public TraversedCoordinates? Test2(AreaPicker __instance, Ray ray, Predicate<Vector3Int> predicate, TraversedCoordinates? __result)
        {
            if (_toolManager.ActiveTool.ToString() == "Timberborn.PlantingUI.PlantingTool")
            {
                foreach (TraversedCoordinates traversedCoordinates in _gridTraversal.TraverseRay(ray))
                {
                    Vector3Int coordinates = traversedCoordinates.Coordinates;
                
                    if (predicate(coordinates))
                        return new TraversedCoordinates?(traversedCoordinates);
                    
                    var buildings = _blockService.GetObjectsWithComponentAt<PlaceableCropField>(coordinates);
                    if (buildings.Any())
                    {
                        var coords = traversedCoordinates.Coordinates;
                        coords.z += 1;
                        traversedCoordinates.Coordinates.Set(coords.x, coords.y, coords.z);
                        
                        return new TraversedCoordinates?(traversedCoordinates);
                    }
                }
            }
            
            return new TraversedCoordinates?();
        }
        
        public bool Test3(TerrainPicker __instance, IEnumerable<Vector3Int> inputBlocks, Ray ray, IEnumerable<Vector3Int> __result)
        {
            if (_toolManager.ActiveTool.ToString() == "Timberborn.PlantingUI.PlantingTool")
            {
                return true;
            }

            return false;
        }
        
        
        public bool Test4(Vector3Int coordinates, string name)
        {
            if (_toolManager.ActiveTool.ToString() == "Timberborn.PlantingUI.PlantingTool")
            {
                return true;
            }
            return false;
            
            var buildings = _blockService.GetObjectsWithComponentAt<LabeledPrefab>(coordinates);
            if (buildings.Any())
            {
                return true;
            }

            return false;
        }

        public bool IsPlaceableCropField(Vector3Int coordinates)
        {
            var buildings = _blockService.GetObjectsWithComponentAt<PlaceableCropField>(coordinates);
            if (buildings.Any())
            {
                return true;
            }
            return false;
        }

        public bool IsPlantingTool()
        {
            if (_toolManager.ActiveTool.ToString() == "Timberborn.PlantingUI.PlantingTool")
            {
                return true;
            }
            return false;
        }
    }
}
