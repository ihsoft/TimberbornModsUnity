using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using Timberborn.AssetSystem;
using Timberborn.BlockSystem;
using Timberborn.PathSystem;
using Timberborn.SingletonSystem;
using Timberborn.Yielding;
using UnityEngine;
using UnityEngine.UIElements;

namespace TobbyTools.DynamicSpecificationSystem
{
    public class DynamicSpecificationFactory : ILoadableSingleton
    {
        private IResourceAssetLoader _resourceAssetLoader;
        private readonly DynamicSpecificationDeserializer _dynamicSpecificationDeserializer;
        
        private readonly Type[] SkippableTypes = new []
        {
            typeof(BinaryData),
            AccessTools.TypeByName("UniversalAdditionalLightData"),
            AccessTools.TypeByName("CanvasScaler"),
        };

        private readonly List<Type> _unallowedTypes = new()
        {
            typeof(GameObject),
            typeof(GameObject[]),
            typeof(Image),
            typeof(ParticleSystem),
            typeof(BlockObject),
            typeof(PathModelVariant),
            typeof(YielderSpecification),
            AccessTools.TypeByName("MechanicalModelVariantSpecification").MakeArrayType(),
        };

        public DynamicSpecificationFactory(IResourceAssetLoader resourceAssetLoader, DynamicSpecificationDeserializer dynamicSpecificationDeserializer)
        {
            _resourceAssetLoader = resourceAssetLoader;
            _dynamicSpecificationDeserializer = dynamicSpecificationDeserializer;
        }

        public void Load()
        {
            // Create();
        }
        
        public void Create()
        {
            var allMonoBehaviours = _resourceAssetLoader.LoadAll<MonoBehaviour>("");

            foreach (var monoBehaviour in allMonoBehaviours)
            {
                var type = monoBehaviour.GetType();
                
                if (SkippableTypes.Contains(type))
                    continue;
                
                Plugin.Log.LogWarning("Name Monobehavior: " + type);

                var values = GetFilteredDynamicProperties(type, monoBehaviour);

                Plugin.Log.LogWarning("Number of fields: " + values.Count);
                if (!values.Any())
                    continue;
                
                // try
                // {
                    // var configFilePath = Path.Combine(Plugin.Mod.DirectoryPath, "GeneratedSpecifications", type + ".json");
                    //
                    // if (!Directory.Exists(Path.GetDirectoryName(configFilePath)))
                    //     Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));

                    var newObject = DynamicClassFactory.CreateNewObject(values);
                    var a = newObject.GetType();
                    // Plugin.Log.LogInfo(a + "");
                    foreach (var property in values)
                    {
                        var b = a.GetProperty(property.StyledName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        // Plugin.Log.LogInfo(b + "");

                        
                        // if (b.PropertyType.IsValueType && !b.PropertyType.IsPrimitive && !b.PropertyType.IsEnum)
                        // {
                        //     var structInstance = Activator.CreateInstance(b.PropertyType);
                        //     var values2 = GetDynamicPropertiesWithoutValues(structInstance.GetType(), b.GetValue(newObject));
                        //     foreach (var dynamicProperty2 in values2)
                        //     {
                        //         Plugin.Log.LogInfo(dynamicProperty2.StyledName);
                        //         Plugin.Log.LogInfo(dynamicProperty2.Value + "");
                        //         b.PropertyType.GetProperty(dynamicProperty2.StyledName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(structInstance, dynamicProperty2.Value);
                        //     }
                        // }


                        b.SetValue(newObject, property.Value);
                    }

                    var fileName = "DynamicSpecification." + monoBehaviour.gameObject.name + "." + monoBehaviour.GetType().Name + ".original.json";
                    File.WriteAllText("C:\\Users\\jordy\\SynologyDrive\\Unity Projecten\\TimberbornModsUnity Update 4\\ThunderKit\\Staging\\TobbyTools\\GeneratedSpecs\\"+fileName, JsonConvert.SerializeObject(newObject, Formatting.Indented));
                    Plugin.Log.LogInfo("Written");
                // }
                // catch (Exception ex)
                // {
                //     Plugin.Log.LogError("Failed to save specification: " + ex.Message);
                // }
            }
        }

        private List<DynamicProperty> GetFilteredDynamicProperties(Type type, object instance)
        {
            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => field.IsDefined(typeof(SerializeField), true))
                .Where(IsAllowedFieldType)
                .Select(field => new DynamicProperty(field.Name, field.GetValue(instance)))
                .ToList();
        }
        
        private List<DynamicProperty> GetDynamicPropertiesWithoutValues(Type type, object instance)
        {
            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(field => new DynamicProperty(field.Name, field.GetValue(instance)))
                .ToList();
        }

        private bool IsAllowedFieldType(FieldInfo fieldInfo)
        {
            return !_unallowedTypes.Contains(fieldInfo.FieldType);
        }
    }
}