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
        [SerializeField] private GameObject _trackContainedSectionMesh;
        
        private ToolGroupManager _toolGroupManager;
        private EventBus _eventBus;

        private TrackPiece _trackPiece;
        
        private MeshRenderer _trackContainedSectionMeshRenderer;
        private MeshRenderer _trackSectionMeshRenderer;
        private MeshRenderer _trackEntranceSectionMeshRenderer;
        private MeshRenderer _trackExitSectionMeshRenderer;

        private GameObject _selectedGameObject;

        [Inject]
        public void InjectDependencies(
            ConstructionModeService constructionModeService, 
            NavRangeDrawingService navRangeDrawingService, 
            ToolGroupManager toolGroupManager, 
            EventBus eventBus)
        {
            _toolGroupManager = toolGroupManager;
            _eventBus = eventBus;

            var pathNavRangeDrawer = ChooChooCore.GetInaccessibleField(navRangeDrawingService, "_pathNavRangeDrawer");
            var material = ChooChooCore.GetInaccessibleField(pathNavRangeDrawer, "_material") as Material;
            _trackPiece = GetComponent<TrackPiece>();

            if (_trackContainedSectionMesh != null)
            {
                _trackContainedSectionMeshRenderer = _trackContainedSectionMesh.GetComponentInChildren<MeshRenderer>();
                _trackContainedSectionMeshRenderer.material = material;
                _trackContainedSectionMeshRenderer.material.renderQueue = material.renderQueue;
                _trackContainedSectionMesh.SetActive(false);
            }
            if (_trackEntranceSectionMesh != null)
            {
                _trackEntranceSectionMeshRenderer = _trackEntranceSectionMesh.GetComponentInChildren<MeshRenderer>();
                _trackEntranceSectionMeshRenderer.material = material;
                _trackEntranceSectionMeshRenderer.material.renderQueue = material.renderQueue;
                _trackEntranceSectionMesh.SetActive(false);
            }
            if (_trackExitSectionMesh != null)
            {
                _trackExitSectionMeshRenderer = _trackExitSectionMesh.GetComponentInChildren<MeshRenderer>();
                _trackExitSectionMeshRenderer.material = material;
                _trackExitSectionMeshRenderer.material.renderQueue = material.renderQueue;
                _trackExitSectionMesh.SetActive(false);
            }
            if(_trackSectionMesh != null)
            {
                _trackSectionMeshRenderer = _trackSectionMesh.GetComponentInChildren<MeshRenderer>();
                _trackSectionMeshRenderer.material = material;
                _trackSectionMeshRenderer.material.renderQueue = material.renderQueue;
                _trackSectionMesh.SetActive(false);
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
            SetActive(ShouldBeActive());
        }
        
        [OnEvent]
        public void OnToolEntered(ToolEnteredEvent toolEnteredEvent)
        {
            SetActive(ShouldBeActive());
        }
        
        [OnEvent]
        public void OnTrackUpdate(OnTracksUpdatedEvent onTracksUpdatedEvent)
        {
            UpdateColor();
            SetActive(ShouldBeActive());
        }

        [OnEvent]
        public void OnGameObjectSelected(GameObjectSelectedEvent gameObjectSelectedEvent)
        {
            _selectedGameObject = gameObjectSelectedEvent.GameObject;
            SetActive(ShouldBeActive());
        }
        
        [OnEvent]
        public void OnGameObjectUnselectedEvent(GameObjectUnselectedEvent gameObjectUnselectedEvent)
        {
            _selectedGameObject = null;
            SetActive(ShouldBeActive());
        }

        private bool ShouldBeActive()
        {
            if (_selectedGameObject != null && (
                    _selectedGameObject.GetComponent<TrackPiece>() || 
                    _selectedGameObject.GetComponent<Train>() || 
                    _selectedGameObject.GetComponent<TrainWagon>()))
            {
                return true;
            }
            
            var activeToolGroup = _toolGroupManager.ActiveToolGroup;

            return activeToolGroup != null && activeToolGroup.DisplayNameLocKey.ToLower().Contains("train");
        }
        
        private void UpdateColor() 
        { 
            if (_trackEntranceSectionMeshRenderer != null)
            {
                if (_trackPiece.TrackRoutes[0].Entrance.ConnectedTrackPiece != null)
                    _trackEntranceSectionMeshRenderer.material.SetColor("_BaseColor_1", _trackPiece.TrackRoutes[0].Entrance.ConnectedTrackPiece.TrackSection.Color);
                else
                    _trackEntranceSectionMeshRenderer.material.SetColor("_BaseColor_1", Color.white);
            }
            if (_trackExitSectionMeshRenderer != null)
            {
                if (_trackPiece.TrackRoutes[0].Exit.ConnectedTrackPiece != null)
                    _trackExitSectionMeshRenderer.material.SetColor("_BaseColor_1", _trackPiece.TrackRoutes[0].Exit.ConnectedTrackPiece.TrackSection.Color);
                else
                    _trackExitSectionMeshRenderer.material.SetColor("_BaseColor_1", Color.white);
            }
            if (_trackContainedSectionMeshRenderer != null)
                _trackContainedSectionMeshRenderer.material.SetColor("_BaseColor_1", _trackPiece.TrackSection.Color);
            if (_trackSectionMeshRenderer != null)
                _trackSectionMeshRenderer.material.SetColor("_BaseColor_1", _trackPiece.TrackSection.Color);
        }
        
        private void SetActive(bool active)
        {
            var flag1 = active && _trackPiece.DividesSection;
            if (_trackEntranceSectionMesh != null && _trackEntranceSectionMesh.activeSelf != flag1)
                _trackEntranceSectionMesh.SetActive(flag1);
            if (_trackExitSectionMesh != null && _trackExitSectionMesh.activeSelf != flag1)
                _trackExitSectionMesh.SetActive(flag1);
            if (_trackContainedSectionMesh != null && _trackContainedSectionMesh.activeSelf != flag1)
                _trackContainedSectionMesh.SetActive(flag1);
            var flag2 = active && !_trackPiece.DividesSection;
            if(_trackSectionMesh != null && _trackSectionMesh.activeSelf != flag2)
                _trackSectionMesh.SetActive(flag2);
        }
    }
}