using Bindito.Core;
using Timberborn.BuildingsNavigation;
using Timberborn.ConstructibleSystem;
using Timberborn.ConstructionMode;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrackSectionMeshShower : MonoBehaviour, IFinishedStateListener
    {
        // height in object should be 0.06
        [SerializeField] private GameObject _trackSectionMesh;
        [SerializeField] private GameObject _trackEntranceSectionMesh;
        [SerializeField] private GameObject _trackExitSectionMesh;
        
        private ToolGroupManager _toolGroupManager;
        private EventBus _eventBus;

        private TrackPiece _trackPiece;
        private MeshRenderer _trackSectionMeshRenderer;
        private MeshRenderer _trackEntranceSectionMeshRenderer;
        private MeshRenderer _trackExitSectionMeshRenderer;

        [Inject]
        public void InjectDependencies(
            ConstructionModeService constructionModeService, 
            NavRangeDrawingService navRangeDrawingService, 
            ToolGroupManager toolGroupManager, 
            ChooChooCore chooChooCore, 
            EventBus eventBus)
        {
            _toolGroupManager = toolGroupManager;
            _eventBus = eventBus;

            var pathNavRangeDrawer = chooChooCore.GetPrivateField(navRangeDrawingService, "_pathNavRangeDrawer");

            var material = chooChooCore.GetPrivateField(pathNavRangeDrawer, "_material") as Material;
            
            _trackPiece = GetComponent<TrackPiece>();
            
            if (_trackPiece.DividesSection)
            {
                _trackEntranceSectionMeshRenderer = _trackEntranceSectionMesh.GetComponentInChildren<MeshRenderer>();
                _trackEntranceSectionMeshRenderer.material = material;
                _trackEntranceSectionMeshRenderer.material.renderQueue = material.renderQueue;
            
                _trackExitSectionMeshRenderer = _trackExitSectionMesh.GetComponentInChildren<MeshRenderer>();
                _trackExitSectionMeshRenderer.material = material;
                _trackExitSectionMeshRenderer.material.renderQueue = material.renderQueue;
            }
            else
            {
                _trackSectionMeshRenderer = _trackSectionMesh.GetComponentInChildren<MeshRenderer>();
                _trackSectionMeshRenderer.material = material;
                _trackSectionMeshRenderer.material.renderQueue = material.renderQueue;
            }

            SetActive(false);
        }

        public void OnEnterFinishedState()
        {
            _eventBus.Register(this);
            
            UpdateColor();

            SetActive(!ShouldBeActive());
        }

        public void OnExitFinishedState()
        {
            _eventBus.Unregister(this);
        }
                
        [OnEvent]
        public void OnToolGroupEntered(ToolGroupEnteredEvent toolGroupEnteredEvent)
        {
            if (toolGroupEnteredEvent.ToolGroup != null && toolGroupEnteredEvent.ToolGroup.DisplayNameLocKey.ToLower().Contains("train"))
                SetActive(true);
            else
                SetActive(false);
        }
        
        [OnEvent]
        public void OnToolEntered(ToolEnteredEvent toolEnteredEvent)
        {
            if (toolEnteredEvent.Tool.ToolGroup != null && toolEnteredEvent.Tool.ToolGroup.DisplayNameLocKey.ToLower().Contains("train"))
            {
                SetActive(true);
                return;
            }

            if (ShouldBeActive())
                SetActive(false);
        }
        
        [OnEvent]
        public void OnTrackUpdate(OnTracksUpdatedEvent onTracksUpdatedEvent)
        {
            UpdateColor();
        }

        [OnEvent]
        public void OnGameObjectSelected(GameObjectSelectedEvent gameObjectSelectedEvent)
        {
            if (
                gameObjectSelectedEvent.GameObject.GetComponent<TrackPiece>() ||
                gameObjectSelectedEvent.GameObject.GetComponent<Train>() ||
                gameObjectSelectedEvent.GameObject.GetComponent<TrainWagon>())
            {
                SetActive(true);
                return;
            }

            if (ShouldBeActive())
                SetActive(false);
        }
        
        [OnEvent]
        public void OnGameObjectUnselectedEvent(GameObjectUnselectedEvent gameObjectUnselectedEvent)
        {
            SetActive(false);
        }

        private bool ShouldBeActive()
        {
            var activeToolGroup = _toolGroupManager.ActiveToolGroup;

            return activeToolGroup == null || !activeToolGroup.DisplayNameLocKey.ToLower().Contains("train");
        }
        
        private void UpdateColor() 
        {
            if (_trackPiece.DividesSection)
            {
                if (_trackPiece.TrackRoutes[0].Entrance.ConnectedTrackPiece != null)
                {
                    _trackEntranceSectionMeshRenderer.material.SetColor("_BaseColor_1", _trackPiece.TrackRoutes[0].Entrance.ConnectedTrackPiece.TrackSection.Color);
                }
                else
                {
                    _trackEntranceSectionMeshRenderer.material.SetColor("_BaseColor_1", Color.white);
                }
                
                if (_trackPiece.TrackRoutes[0].Exit.ConnectedTrackPiece != null)
                {
                    _trackExitSectionMeshRenderer.material.SetColor("_BaseColor_1", _trackPiece.TrackRoutes[0].Exit.ConnectedTrackPiece.TrackSection.Color);
                }
                else
                {
                    _trackExitSectionMeshRenderer.material.SetColor("_BaseColor_1", Color.white);
                }
            }
            else
            {
                _trackSectionMeshRenderer.material.SetColor("_BaseColor_1", _trackPiece.TrackSection.Color);
            }
        }
        
        private void SetActive(bool active)
        {
            if (_trackPiece.DividesSection)
            {
                if (_trackEntranceSectionMesh.activeSelf != active)
                    _trackEntranceSectionMesh.SetActive(active);
                
                if (_trackExitSectionMesh.activeSelf != active)
                    _trackExitSectionMesh.SetActive(active);
            }
            else
            {
                if (_trackSectionMesh.activeSelf != active)
                    _trackSectionMesh.SetActive(active);
            }
        }
    }
}