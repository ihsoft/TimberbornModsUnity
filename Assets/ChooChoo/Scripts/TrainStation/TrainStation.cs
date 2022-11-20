using System.Collections.Generic;
using Bindito.Core;
using Timberborn.BlockSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrainStation : MonoBehaviour
    {
        private BlockService _blockService;

        private BlockObject _blockObject;
        
        private List<List<Vector3Int>> _railRoutes = new();

        [Inject]
        public void InjectDependencies(BlockService blockService)
        {
            _blockService = blockService;
        }

        // private void Start()
        // {
        //     _blockObject = GetComponent<BlockObject>();
        //     
        //     // TryGenerateRoutes();
        //     
        //     foreach (var vector3Int in _railRoutes[0])
        //     {
        //         Plugin.Log.LogWarning(vector3Int.ToString());
        //     }
        // }
        //
        // public void MarkEndOfRoute(List<Vector3Int> route)
        // {
        //     route.Add(_blockObject.Coordinates);
        // }
        
        // private void TryGenerateRoutes()
        // {
        //     var coordinateOfRail = _blockObject.Coordinates + new Vector3Int(1, 3);
        //     var railObject = _blockService.GetFloorObjectAt(coordinateOfRail);
        //     _railRoutes.Add(new List<Vector3Int>());
        //     if(railObject != null && railObject.TryGetComponent(out TrackPiece rail))
        //         rail.TryAddNextStepOfRoute(_railRoutes[0], _blockObject.Orientation.ToDirection());
        // }
    }
}
