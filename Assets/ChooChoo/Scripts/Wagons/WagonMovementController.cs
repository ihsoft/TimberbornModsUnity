using System.Collections.Generic;
using UnityEngine;

namespace ChooChoo
{
    public class WagonMovementController : MonoBehaviour
    {
        private TrainWagonManager _trainWagonManager;
        
        private void Awake()
        {
            _trainWagonManager = GetComponent<TrainWagonManager>();
        }
        
        public void SetNewPathConnections(ITrackFollower trackFollower, List<TrackRoute> pathConnections)
        {
            var trainWagons = _trainWagonManager.TrainWagons;
            trainWagons[0].StartMoving(trackFollower, pathConnections);
            for (var index = 1; index < trainWagons.Count; index++)
            {
                var trainWagon = trainWagons[index];
                trainWagon.StartMoving(trainWagons[index - 1].ObjectFollower, pathConnections);
            }
        }

        public void MoveWagons()
        {
            foreach (var trainWagon in _trainWagonManager.TrainWagons)
            {
                trainWagon.Move();
            }
        }
        
        public void StopWagons()
        {
            foreach (var trainWagon in _trainWagonManager.TrainWagons)
            {
                trainWagon.GetComponent<TrainWagon>().Stop();
            }
        }
    }
}