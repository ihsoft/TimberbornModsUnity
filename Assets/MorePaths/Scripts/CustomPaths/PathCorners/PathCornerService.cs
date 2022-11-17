using System.Collections.Generic;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.PathSystem;
using Timberborn.TerrainSystem;
using UnityEngine;

namespace MorePaths
{
    public class PathCornerService
    {
        private readonly BlockService _blockService;

        private readonly ITerrainService _terrainService;

        private readonly List<List<Vector3Int>> _neighboringCoordinates = new()
        {
            new List<Vector3Int>
            {
                new(0, -1 , 0),
                new(-1, -1 , 0),
                new(-1, 0 , 0),
            },
            new List<Vector3Int>
            {
                new(-1, 0 , 0),
                new(-1, 1 , 0),
                new(0, 1 , 0),
            },
            new List<Vector3Int>
            {
                new(0, 1 , 0),
                new(1, 1 , 0),
                new(1, 0 , 0),
            },
            new List<Vector3Int>
            {
                new(1, 0 , 0),
                new(1, -1 , 0),
                new(0, -1 , 0),
            },
        };

        public PathCornerService(BlockService blockService, ITerrainService terrainService)
        {
            _blockService = blockService;
            _terrainService = terrainService;
        }

        public List<bool> EnableNeighbouringPaths(Vector3Int checkingCoordinates)
        {
            var newCheckingCoordinates = new Vector3Int(checkingCoordinates.x, checkingCoordinates.z, checkingCoordinates.y);

            var flags = new List<bool>();
            
            foreach (var (quadrantList, i) in _neighboringCoordinates.Select((value, i) => ( value, i )))
            {
                var obj1 = _terrainService.OnGround(newCheckingCoordinates + quadrantList[0]) ? _blockService.GetFloorObjectComponentAt<DynamicPathCorner>(newCheckingCoordinates + quadrantList[0]) : null;
                var obj2 = _terrainService.OnGround(newCheckingCoordinates + quadrantList[1]) ? _blockService.GetFloorObjectComponentAt<DynamicPathCorner>(newCheckingCoordinates + quadrantList[1]) : null;
                var obj3 = _terrainService.OnGround(newCheckingCoordinates + quadrantList[2]) ? _blockService.GetFloorObjectComponentAt<DynamicPathCorner>(newCheckingCoordinates + quadrantList[2]) : null;

                var flag = obj1 != null && obj2 != null && obj3 != null;
                
                flags.Add(flag);

                if (!flag) continue;

                switch (i)
                {
                    case 0:
                        EnablePathCorner(obj1.CornerUpLeft);
                        EnablePathCorner(obj2.CornerUpRight);
                        EnablePathCorner(obj3.CornerDownRight);
                        break;
                    case 1:
                        EnablePathCorner(obj1.CornerUpRight);
                        EnablePathCorner(obj2.CornerDownRight);
                        EnablePathCorner(obj3.CornerDownLeft);
                        break;
                    case 2:
                        EnablePathCorner(obj1.CornerDownRight);
                        EnablePathCorner(obj2.CornerDownLeft);
                        EnablePathCorner(obj3.CornerUpLeft);
                        break;
                    case 3:
                        EnablePathCorner(obj1.CornerDownLeft);
                        EnablePathCorner(obj2.CornerUpLeft);
                        EnablePathCorner(obj3.CornerUpRight);
                        break;
                }
            }

            return flags;
        }

        public void DisableNeighbouringPaths(Vector3Int checkingCoordinates)
        {
            var newCheckingCoordinates = new Vector3Int(checkingCoordinates.x, checkingCoordinates.z, checkingCoordinates.y);

            foreach (var (quadrantList, i) in _neighboringCoordinates.Select((value, i) => ( value, i )))
            {
                foreach (var coordinate in quadrantList)
                {
                    var obj = _blockService.GetFloorObjectComponentAt<DynamicPathCorner>(newCheckingCoordinates + coordinate);

                    if (obj == null) continue;

                    switch (i)
                    {
                        case 0:
                            DisablePathCorner(obj.CornerUpRight);
                            break;
                        case 1:
                            DisablePathCorner(obj.CornerDownRight);
                            break;
                        case 2:
                            DisablePathCorner(obj.CornerDownLeft);
                            break;
                        case 3:
                            DisablePathCorner(obj.CornerUpLeft);
                            break;
                    }
                }
            }
        }

        private void EnablePathCorner(GameObject corner)
        {
            corner.SetActive(true);
        }
        
        private void DisablePathCorner(GameObject corner)
        {
            corner.SetActive(false);
        }
    }
}