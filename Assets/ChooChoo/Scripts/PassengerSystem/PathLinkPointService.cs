using System.Collections.Generic;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using Timberborn.Navigation;
using UnityEngine;

namespace ChooChoo
{
    public class PathLinkPointService
    {
        private NodeIdService _nodeIdService;
        
        private ChooChooCore _chooChooCore;
        
        private readonly List<TeleporterLink> _nodeIds = new();

        PathLinkPointService(NodeIdService nodeIdService, ChooChooCore chooChooCore)
        {
            _nodeIdService = nodeIdService;
            _chooChooCore = chooChooCore;
        }

        public void AddTeleporterNode(GameObject gameObject, GameObject end)
        {
            var blockObject = gameObject.GetComponent<BlockObject>();

            var positionedEntrance = blockObject.PositionedEntrance;
            
            var originalCoordinates = positionedEntrance.DoorstepCoordinates - Direction2D.Down.ToOffset();
            var originalNodeId = (int)_chooChooCore.InvokePrivateMethod(_nodeIdService, "GridToId", new object[] { originalCoordinates });
            
            var endCoordinates = originalCoordinates + Direction2D.Down.ToOffset() * 10;
            var endNodeID = (int)_chooChooCore.InvokePrivateMethod(_nodeIdService, "GridToId", new object[] { endCoordinates });
            
            endCoordinates = new Vector3Int(endCoordinates.x, endCoordinates.z, endCoordinates.y);
            end.transform.position = endCoordinates;

            Plugin.Log.LogError(originalNodeId + "   " + originalCoordinates);
            Plugin.Log.LogWarning(endNodeID + "   " + endCoordinates);
            _nodeIds.Add(new TeleporterLink(originalNodeId, endNodeID));
        }

        public TeleporterLink GetTeleporterLink(int nodeId)
        {
            return _nodeIds.FirstOrDefault(link => link.StartNodeId == nodeId);
        }
    }
}