using System.Reflection;
using Bindito.Core;
using Timberborn.Beavers;
using Timberborn.BlockSystem;
using Timberborn.BonusSystem;
using Timberborn.EntitySystem;
using Timberborn.TickSystem;
using Timberborn.WalkingSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace MorePaths
{
    public class PathListener : TickableComponent
    {
        private BlockService _blockService;
        private BonusManager _bonusManager;
        private WalkerSpeedManager _walkerSpeedManager;
        private bool _bonusActive = false;

        [Inject]
        public void InjectDependencies(BlockService blockService)
        {
            _blockService = blockService;
        }

        new void Start()
        {
             _bonusManager = GetComponent<BonusManager>();
             _walkerSpeedManager = GetComponent<WalkerSpeedManager>();
        }
        
        public override void Tick()
        {
            var coords = Vector3Int.FloorToInt(transform.position);
            var newCoords = new Vector3Int(coords.x, coords.z, coords.y);
            var objectsAt = _blockService.GetObjectsAt(newCoords);
            foreach (var blockObject in objectsAt)
            {
                if (blockObject.GetComponent<Prefab>().PrefabName.ToLower().Contains("gravel"))
                {
                    if (!_bonusActive)
                    {
                        IncreaseMovementSpeed(500f);
                        // _bonusManager.AddBonus("MovementSpeed", 200f);
                    }
                    _bonusActive = true;
                }
                else
                {
                    if (_bonusActive)
                    {
                        // _bonusManager.RemoveBonus("MovementSpeed", 200f);
                    }
                    _bonusActive = false;
                }
            }
        }

        public void IncreaseMovementSpeed(float SpeedMultiplier)
        {
            // Plugin.Log.LogFatal("incraese bonus");
            PropertyInfo property = typeof(WalkerSpeedManager).GetProperty("Speed");
            float speed = _walkerSpeedManager.Speed;
            property.SetValue(_walkerSpeedManager, speed * SpeedMultiplier);
        }
    }
}
