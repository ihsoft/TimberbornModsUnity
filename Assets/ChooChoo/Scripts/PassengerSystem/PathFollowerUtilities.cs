using System.Collections.Generic;
using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CharacterMovementSystem;
using Timberborn.WalkingSystem;
using UnityEngine;

namespace ChooChoo
{
    public class PathFollowerUtilities : BaseComponent
    {
        private PathCornerBlockObjectRepository _pathCornerBlockObjectRepository;
        
        private PathFollower _pathFollower;
        private BlockObject[] _toBeVisitedBlockObjects;

        [Inject]
        public void InjectDependencies(PathCornerBlockObjectRepository pathCornerBlockObjectRepository)
        {
            _pathCornerBlockObjectRepository = pathCornerBlockObjectRepository;
        }
        
        private void Awake()
        {
            var walker = GetComponentFast<Walker>();
            walker.StartedNewPath += OnStartedNewPath;
            _pathFollower = (PathFollower)ChooChooCore.GetInaccessibleField(walker, "_pathFollower");
        }

        public BlockObject GetBlockObjectAtIndex(int index)
        {
            return index >= _toBeVisitedBlockObjects.Length ? null : _toBeVisitedBlockObjects[index];
        }

        private void OnStartedNewPath(object sender, StartedNewPathEventArgs e)
        {
            // Plugin.Log.LogWarning(sender + " started new path.");
            var pathCorners = (IReadOnlyList<Vector3>)ChooChooCore.GetInaccessibleField(_pathFollower, "_pathCorners");
            var list = new List<BlockObject>();
            foreach (var pathCorner in pathCorners)
            {
                var blockObject = _pathCornerBlockObjectRepository.Get(pathCorner);
                // Plugin.Log.LogInfo(pathCorner + "   " + blockObject);
                list.Add(blockObject);
            }

            _toBeVisitedBlockObjects = list.ToArray();
        }
    }
}