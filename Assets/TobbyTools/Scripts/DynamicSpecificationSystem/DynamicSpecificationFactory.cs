using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Timberborn.AssetSystem;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace TobbyTools.DynamicSpecificationSystem
{
    public class DynamicSpecificationFactory : ILoadableSingleton
    {
        private readonly ActiveComponentRetriever _activeComponentRetriever;
        private readonly IResourceAssetLoader _resourceAssetLoader;
        private readonly DynamicSpecificationDeserializer _dynamicSpecificationDeserializer;

        public DynamicSpecificationFactory(
            ActiveComponentRetriever activeComponentRetriever, 
            IResourceAssetLoader resourceAssetLoader, 
            DynamicSpecificationDeserializer dynamicSpecificationDeserializer)
        {
            _activeComponentRetriever = activeComponentRetriever;
            _resourceAssetLoader = resourceAssetLoader;
            _dynamicSpecificationDeserializer = dynamicSpecificationDeserializer;
        }

        public void Load()
        {
            // Create();
        }
        
        public void Create()
        {
            foreach (var monoBehaviour in _activeComponentRetriever.GetAllComponents())
            {
                var type = monoBehaviour.GetType();

                if (SkippableTypes.Types.Contains(type))
                    continue;

                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(type + "");
                
                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning("Name Monobehavior: " + type);

                var values = GetFilteredDynamicProperties(type, monoBehaviour);
                
                if (!values.Any())
                    continue;
                
                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning("Number of fields: " + values.Count);
                
                var newObject = DynamicClassFactory.CreateNewObject(values);
                var a = newObject.GetType();
                foreach (var property in values)
                {
                    a.GetProperty(property.StyledName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(newObject, property.Value);
                }

                var fileName = "DynamicSpecification." + monoBehaviour.gameObject.name + "." + monoBehaviour.GetType().Name + ".original.json";

                var filePath = "C:\\Users\\jordy\\SynologyDrive\\Unity Projecten\\TimberbornModsUnity Update 4\\ThunderKit\\Staging\\TobbyTools\\GeneratedSpecs\\" + fileName;

                if (File.Exists(filePath))
                    continue;
                
                File.WriteAllText(filePath, JsonConvert.SerializeObject(newObject, Formatting.Indented));
                
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("Written");
            }
        }

        private List<DynamicProperty> GetFilteredDynamicProperties(Type type, object instance)
        {
            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => field.IsDefined(typeof(SerializeField), true))
                .Where(ReflectionUtils.IsAllowedFieldType)
                .Select(field => new DynamicProperty(field.Name, field.GetValue(instance)))
                .ToList();
        }
    }
}