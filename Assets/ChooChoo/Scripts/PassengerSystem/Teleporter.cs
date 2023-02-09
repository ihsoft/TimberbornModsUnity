using Bindito.Core;
using UnityEngine;

namespace ChooChoo
{
    public class Teleporter : MonoBehaviour
    {
        private TeleporterService _teleporterService;
        
        [Inject]
        public void InjectDependencies(TeleporterService teleporterService)
        {
            _teleporterService = teleporterService;
        }

        private void Start()
        {
            _teleporterService.AddTeleporterNode(gameObject);
        }
    }
}