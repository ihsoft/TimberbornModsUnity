using System;
using Bindito.Core;
using Timberborn.Coordinates;
using Timberborn.Persistence;
using Timberborn.PrefabSystem;
using UnityEngine;

namespace ChooChoo
{
    public class LabeledPrefabSwitcher : MonoBehaviour
    {
        [SerializeField]
        private string _displayNameLocKey;
        [SerializeField]
        private string _descriptionLocKey;
        [SerializeField]
        private string _flavorDescriptionLocKey;
        [SerializeField]
        private Sprite _image;
        
        [SerializeField]
        private string _alternativeDisplayNameLocKey;
        [SerializeField]
        private string _alternativeDescriptionLocKey;
        [SerializeField]
        private string _alternativeFlavorDescriptionLocKey;
        [SerializeField]
        private Sprite _alternativeImage;

        private ChooChooCore _chooChooCore;

        private LabeledPrefab _labeledPrefab;
        
        [Inject]
        public void InjectDependencies(ChooChooCore chooChooCore)
        {
            _chooChooCore = chooChooCore;
        }

        void Awake()
        {
            _labeledPrefab = GetComponent<LabeledPrefab>();
        }

        public void SetOriginal()
        { 
            _chooChooCore.ChangePrivateField(_labeledPrefab, "_displayNameLocKey",  _displayNameLocKey);
            _chooChooCore.ChangePrivateField(_labeledPrefab, "_descriptionLocKey",  _descriptionLocKey);
            _chooChooCore.ChangePrivateField(_labeledPrefab, "_flavorDescriptionLocKey",  _flavorDescriptionLocKey);
            _chooChooCore.ChangePrivateField(_labeledPrefab, "_image",  _image);
        }

        public void SetAlternative()
        {
            _chooChooCore.ChangePrivateField(_labeledPrefab, "_displayNameLocKey",  _alternativeDisplayNameLocKey);
            _chooChooCore.ChangePrivateField(_labeledPrefab, "_descriptionLocKey",  _alternativeDescriptionLocKey);
            _chooChooCore.ChangePrivateField(_labeledPrefab, "_flavorDescriptionLocKey",  _alternativeFlavorDescriptionLocKey);
            _chooChooCore.ChangePrivateField(_labeledPrefab, "_image",  _alternativeImage);
        }
    }
}