using TimberApi.DependencyContainerSystem;
using UnityEngine;

namespace MorePaths
{
    public class DynamicPathCorner : MonoBehaviour
    {
        public GameObject CornerDownLeft;
        public GameObject CornerDownRight;
        public GameObject CornerUpLeft;
        public GameObject CornerUpRight;

        public void Start()
        {
            var enabledList = DependencyContainer.GetInstance<PathCornerService>().EnableNeighbouringPaths(Vector3Int.FloorToInt(transform.position));

            if (CornerDownLeft != null)
                CornerDownLeft.SetActive(enabledList[0]);
            if (CornerUpLeft != null)
                CornerUpLeft.SetActive(enabledList[1]);
            if (CornerUpRight != null)
                CornerUpRight.SetActive(enabledList[2]);            
            if (CornerDownRight != null)
                CornerDownRight.SetActive(enabledList[3]);
        }

        public void OnDestroy()
        {
            DependencyContainer.GetInstance<PathCornerService>().DisableNeighbouringPaths(Vector3Int.FloorToInt(transform.position));
        }
    }
}
