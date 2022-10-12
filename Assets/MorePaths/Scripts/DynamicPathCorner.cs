using TimberApi.DependencyContainerSystem;
using UnityEngine;

namespace MorePaths
{
    public class DynamicPathCorner : MonoBehaviour
    {
        public GameObject cornerDownLeft;
        public GameObject cornerDownRight;
        public GameObject cornerUpLeft;
        public GameObject cornerUpRight;

        public void Start()
        {
            var enabledList = DependencyContainer.GetInstance<PathCornerService>().EnableNeighbouringPaths(Vector3Int.FloorToInt(transform.position));

            if (cornerDownLeft != null)
                cornerDownLeft.SetActive(enabledList[0]);
            if (cornerUpLeft != null)
                cornerUpLeft.SetActive(enabledList[1]);
            if (cornerUpRight != null)
                cornerUpRight.SetActive(enabledList[2]);            
            if (cornerDownRight != null)
                cornerDownRight.SetActive(enabledList[3]);
        }

        public void OnDestroy()
        {
            DependencyContainer.GetInstance<PathCornerService>().DisableNeighbouringPaths(Vector3Int.FloorToInt(transform.position));
        }
    }
}
