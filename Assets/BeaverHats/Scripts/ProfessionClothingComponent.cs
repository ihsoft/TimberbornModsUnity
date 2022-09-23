using System;
using Bindito.Core;
using Timberborn.WorkSystem;
using UnityEngine;

namespace BeaverHats
{
    public class ProfessionClothingComponent : MonoBehaviour
    {
        private BeaverHatsService _beaverHatsService;
        
        [Inject]
        public void InjectDependencies(BeaverHatsService beaverHatsService)
        {
            _beaverHatsService = beaverHatsService;
        }

        void Start()
        {
            Worker worker = GetComponent<Worker>();
            worker.GotUnemployed += OnGotUnemployed;
            worker.GotEmployed += OnGotEmployed;
        
            UpdateClothing();
        }

        private void UpdateClothing()
        {
            if (!TryGetComponent(out Worker worker)) return;

            foreach (var clothing in _beaverHatsService.Clothings)
            {
                var bodyPart = _beaverHatsService.FindBodyPart(transform, "__" + clothing.Name);
                if (bodyPart == null)
                    return;
                
                if (worker.Workplace != null)
                {
                    // Plugin.Log.LogError(worker.Workplace.name);
                    
                    // Removed this so only the frog hat will show un till the mod is finished.
                    // if (clothing.WorkPlaces.Contains(worker.Workplace.name.Split(".")[0]))
                    // {
                    //     bodyPart.transform.gameObject.SetActive(true);
                    // }
                    
                    bodyPart.transform.gameObject.SetActive(true);
                }
                else
                {
                    bodyPart.transform.gameObject.SetActive(false);
                }
            }
        }

        private void OnGotUnemployed(object sender, EventArgs e)
        {
            UpdateClothing();
        }

        private void OnGotEmployed(object sender, EventArgs e)
        {
            UpdateClothing();
        }
    }
}
