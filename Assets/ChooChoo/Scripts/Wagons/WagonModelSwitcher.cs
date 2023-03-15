using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChooChoo
{
    public class WagonModelSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject BoxModel;
        [SerializeField] private GameObject LiquidModel;
        [SerializeField] private GameObject FlatModel;
        [SerializeField] private GameObject GondolaModel;
        
        private TrainWagon _trainWagon;
        
        [NonSerialized] 
        public readonly List<string> WagonTypes = new() { "Tobbert.WagonType.Box", "Tobbert.WagonType.Liquid", "Tobbert.WagonType.Flat", "Tobbert.WagonType.Gondola" };

        private void Awake()
        {
            _trainWagon = GetComponent<TrainWagon>();
        }

        public void RefreshModel()
        {
            var activeType = _trainWagon.ActiveWagonType;
            BoxModel.SetActive(activeType == WagonTypes[0]);
            LiquidModel.SetActive(activeType == WagonTypes[1]);
            FlatModel.SetActive(activeType == WagonTypes[2]);
            GondolaModel.SetActive(activeType == WagonTypes[3]);
        }
    }
}