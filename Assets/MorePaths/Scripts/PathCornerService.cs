using System.Collections.Generic;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.PathSystem;
using UnityEngine;

namespace MorePaths
{
    public class PathCornerService
    {
        private readonly BlockService _blockService;

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

        public PathCornerService(BlockService blockService)
        {
            _blockService = blockService;
        }

        public List<bool> EnableNeighbouringPaths(Vector3Int checkingCoordinates)
        {
            var newCheckingCoordinates = new Vector3Int(checkingCoordinates.x, checkingCoordinates.z, checkingCoordinates.y);

            var flags = new List<bool>();
            
            foreach (var (quadrantList, i) in _neighboringCoordinates.Select((value, i) => ( value, i )))
            {
                var obj1 = _blockService.GetFloorObjectComponentAt<DynamicPathCorner>(newCheckingCoordinates + quadrantList[0]);
                var obj2 = _blockService.GetFloorObjectComponentAt<DynamicPathCorner>(newCheckingCoordinates + quadrantList[1]);
                var obj3 = _blockService.GetFloorObjectComponentAt<DynamicPathCorner>(newCheckingCoordinates + quadrantList[2]);

                var flag = obj1 != null && obj2 != null && obj3 != null;
                
                flags.Add(flag);

                if (!flag) continue;
                
                // Plugin.Log.LogInfo("");
                //
                // Plugin.Log.LogInfo(obj1.gameObject.name);
                // Plugin.Log.LogInfo(obj2.gameObject.name);
                // Plugin.Log.LogInfo(obj3.gameObject.name);
                
                
                switch (i)
                {
                    case 0:
                        EnablePathCorner(obj1.cornerUpLeft);
                        EnablePathCorner(obj2.cornerUpRight);
                        EnablePathCorner(obj3.cornerDownRight);
                        break;
                    case 1:
                        EnablePathCorner(obj1.cornerUpRight);
                        EnablePathCorner(obj2.cornerDownRight);
                        EnablePathCorner(obj3.cornerDownLeft);
                        break;
                    case 2:
                        EnablePathCorner(obj1.cornerDownRight);
                        EnablePathCorner(obj2.cornerDownLeft);
                        EnablePathCorner(obj3.cornerUpLeft);
                        break;
                    case 3:
                        EnablePathCorner(obj1.cornerDownLeft);
                        EnablePathCorner(obj2.cornerUpLeft);
                        EnablePathCorner(obj3.cornerUpRight);
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
                            DisablePathCorner(obj.cornerUpRight);
                            break;
                        case 1:
                            DisablePathCorner(obj.cornerDownRight);
                            break;
                        case 2:
                            DisablePathCorner(obj.cornerDownLeft);
                            break;
                        case 3:
                            DisablePathCorner(obj.cornerUpLeft);
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