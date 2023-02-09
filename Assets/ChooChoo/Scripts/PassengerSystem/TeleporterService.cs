using System.Collections.Generic;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using Timberborn.Navigation;
using UnityEngine;

namespace ChooChoo
{
    public class TeleporterService
    {
        private NodeIdService _nodeIdService;
        
        private ChooChooCore _chooChooCore;
        
        private readonly List<TeleporterLink> _nodeIds = new();

        TeleporterService(NodeIdService nodeIdService, ChooChooCore chooChooCore)
        {
            _nodeIdService = nodeIdService;
            _chooChooCore = chooChooCore;
        }

        public void AddTeleporterNode(GameObject gameObject)
        {
            var blockObject = gameObject.GetComponent<BlockObject>();

            var positionedEntrance = blockObject.PositionedEntrance;
            
            var originalCoordinates = positionedEntrance.DoorstepCoordinates - Direction2D.Down.ToOffset();
            var originalNodeId = (int)_chooChooCore.InvokePrivateMethod(_nodeIdService, "GridToId", new object[] { originalCoordinates });
            
            var endCoordinates = originalCoordinates + Direction2D.Down.ToOffset() * 10;
            var endNodeID = (int)_chooChooCore.InvokePrivateMethod(_nodeIdService, "GridToId", new object[] { endCoordinates });
            
            Plugin.Log.LogError(originalNodeId + "   " + originalCoordinates);
            Plugin.Log.LogWarning(endNodeID + "   " + endCoordinates);
            _nodeIds.Add(new TeleporterLink(originalNodeId, endNodeID));
        }

        public TeleporterLink IsTeleporterNode(int nodeId)
        {
            return _nodeIds.FirstOrDefault(link => link.StartNodeId == nodeId);
        }
    }
}