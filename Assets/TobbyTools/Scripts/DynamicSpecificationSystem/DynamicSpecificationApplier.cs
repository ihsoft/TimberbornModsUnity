using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Timberborn.AssetSystem;
using Timberborn.BlockSystem;
using Timberborn.PathSystem;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using Timberborn.Yielding;
using UnityEngine;
using UnityEngine.UI;
using ExpandoObject = System.Dynamic.ExpandoObject;

namespace TobbyTools.DynamicSpecificationSystem
{
    public class DynamicSpecificationApplier : ILoadableSingleton
    {
        private readonly ISpecificationService _specificationService;
        private readonly IResourceAssetLoader _resourceAssetLoader;
        
        private readonly List<Type> _unallowedTypes = new()
        {
            typeof(GameObject),
            typeof(GameObject[]),
            typeof(Image),
            typeof(ParticleSystem),
            typeof(BlockObject),
            typeof(PathModelVariant),
            typeof(YielderSpecification),
            AccessTools.TypeByName("MechanicalModelVariantSpecification").MakeArrayType()
        };
        
        public DynamicSpecificationApplier(ISpecificationService specificationService, IResourceAssetLoader resourceAssetLoader)
        {
            _specificationService = specificationService;
            _resourceAssetLoader = resourceAssetLoader;
        }

        public void Load()
        {
            // var test = GetDynamicPropertiesWithoutValues(GetPropertyInfos(typeof(Vector3Int)));
    
            // Plugin.Log.LogError(test.Count() + "");
            //
            // foreach (var dynamicProperty in test)
            // {
            //     Plugin.Log.LogInfo(dynamicProperty.OriginalName + "");
            // }
            
            // throw new Exception();
            
            var allMonoBehaviours = _resourceAssetLoader.LoadAll<MonoBehaviour>("");

            foreach (var monoBehaviour in allMonoBehaviours)
            {
                var type = monoBehaviour.GetType();
                
                if (type == typeof(BinaryData))
                    continue;
                
                Plugin.Log.LogWarning("Name Monobehavior: " + type);

                var fieldInfos = GetFilteredFieldInfos(type).ToArray();
                
                var values = GetDynamicProperties(fieldInfos, monoBehaviour).ToArray();

                Plugin.Log.LogWarning("Number of fields: " + values.Length);
                if (!values.Any())
                    continue;
                
                var fileName = "DynamicSpecification." + monoBehaviour.gameObject.name + "." + monoBehaviour.GetType().Name + ".original.json";
                
                Plugin.Log.LogError(fileName);

                var fileLocation = "C:\\Users\\jordy\\SynologyDrive\\Unity Projecten\\TimberbornModsUnity Update 4\\ThunderKit\\Staging\\TobbyTools\\GeneratedSpecs\\" + fileName;

                if (!File.Exists(fileLocation))
                    continue;
                
            
                var jsonString = File.ReadAllText(fileLocation);
                
                ExpandoObject objectContaingValues = JObject.Parse(jsonString).ToExpandoObject();

                foreach (var property in values)
                {
                    UpdateValues(property, objectContaingValues, monoBehaviour, objectContaingValues.GetPropertyValue(property.StyledName));
                }
            }
        }
        
        private IEnumerable<DynamicProperty> GetDynamicProperties(IEnumerable<FieldInfo> fieldInfos, object instance)
        {
            return fieldInfos.Select(field => new DynamicProperty(field.Name, field.GetValue(instance)));
        }
        
        private IEnumerable<DynamicProperty> GetDynamicPropertiesWithoutValues(IEnumerable<FieldInfo> fieldInfos)
        {
            return fieldInfos.Select(field => new DynamicProperty(field.Name));
        }
        
        private IEnumerable<DynamicProperty> GetDynamicPropertiesWithoutValues(IEnumerable<PropertyInfo> fieldInfos)
        {
            return fieldInfos.Select(field => new DynamicProperty(field.Name));
        }

        private IEnumerable<FieldInfo> GetFilteredFieldInfos(Type type)
        {
            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => field.IsDefined(typeof(SerializeField), true))
                .Where(IsAllowedFieldType);
        }
        
        private bool IsAllowedFieldType(FieldInfo fieldInfo)
        {
            return !_unallowedTypes.Contains(fieldInfo.FieldType);
        }
        
        private IEnumerable<PropertyInfo> GetFilteredPropertyInfos(Type type)
        {
            return type
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => field.IsDefined(typeof(SerializeField), true))
                .Where(IsAllowedFieldType);
        }
        
        private IEnumerable<PropertyInfo> GetPropertyInfos(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private bool IsAllowedFieldType(PropertyInfo fieldInfo)
        {
            return !_unallowedTypes.Contains(fieldInfo.PropertyType);
        }

        private void UpdateValues(DynamicProperty property, ExpandoObject objectContaingValues, object objectToBeUpdated, object newValue)
        {
            var toBeUpdatedType = objectToBeUpdated.GetType();
            var toBeUpdateFieldInfo = toBeUpdatedType.GetField(property.OriginalName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var toBeUpdatePropertyInfo = toBeUpdatedType.GetProperty(property.OriginalName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            

            // foreach (var VARIABLE in test1.GetProperties())
            // {
            //     Plugin.Log.LogWarning("value: " + VARIABLE.GetValue(objectContaingValues));
            // }

            // var test = GetValueFromJObject(objectContaingValues, property, previousLayers);
            
            // var value = test?.ToObject(toBeUpdateFieldInfo?.FieldType);

            Plugin.Log.LogError(newValue + "");
            Plugin.Log.LogError(newValue?.GetType() + "");
            // var test = GetDynamicProperties(GetFilteredFieldInfos(value.GetType()), value);
            // foreach (var dynamicProperty in test)
            // {
            //     UpdateValues(dynamicProperty, objectContaingValues, value);
            //     Plugin.Log.LogWarning(dynamicProperty.OriginalName);
            // }
            
            
            if ((toBeUpdateFieldInfo != null && toBeUpdateFieldInfo.FieldType.IsArray) || (toBeUpdatePropertyInfo != null && toBeUpdatePropertyInfo.PropertyType.IsArray))
            {
                Plugin.Log.LogInfo("ARRAY");
                Plugin.Log.LogInfo(property.StyledName);
                // Plugin.Log.LogError(toBeUpdateFieldInfo.FieldType + "");
                var elementType = toBeUpdateFieldInfo != null ? 
                    toBeUpdateFieldInfo.FieldType.GetElementType() :
                    toBeUpdatePropertyInfo.PropertyType.GetElementType();
                var dynamicProperties = GetDynamicPropertiesWithoutValues(GetFilteredFieldInfos(elementType));

                var list = new List<object>();

                IEnumerable<IDictionary<string, object>> enumerable = objectContaingValues.GetList(property.StyledName);
                foreach (IDictionary<string, object> item in enumerable)
                {
                    var newObject = Activator.CreateInstance(elementType);
                    Plugin.Log.LogWarning(newObject.GetType() + "");
                    foreach (var dynamicProperty in dynamicProperties)
                    {
                        Plugin.Log.LogWarning(dynamicProperty.StyledName);
                        Plugin.Log.LogWarning(item.GetPropertyValue(dynamicProperty.StyledName) + "");
                        UpdateValues(dynamicProperty, objectContaingValues, newObject, item.GetPropertyValue(dynamicProperty.StyledName));
                    }
                    list.Add(newObject);
                }

                var generatedArray = list.ToArray();

                // Plugin.Log.LogInfo("generatedArray length: " + generatedArray.Length);
                
                Array typedArray = Array.CreateInstance(elementType, generatedArray.Length);
                generatedArray.CopyTo(typedArray, 0);
                
                // Plugin.Log.LogInfo("typedArray length: " + typedArray.Length + "");

                
                if (toBeUpdateFieldInfo != null)
                {
                    if (newValue != null)
                    {
                        toBeUpdateFieldInfo.SetValue(objectToBeUpdated, typedArray);
                    }
                }
                if (toBeUpdatePropertyInfo != null)
                {
                    if (newValue != null && toBeUpdatePropertyInfo.GetSetMethod() != null)
                    {
                        toBeUpdatePropertyInfo.SetValue(objectToBeUpdated, typedArray);
                    }
                }

                // var test = toBeUpdateFieldInfo.GetValue(objectToBeUpdated) as Array;
                // Plugin.Log.LogInfo("test length: " + test.Length + "");
                
                return;
            }

            if (
                (toBeUpdateFieldInfo != null && toBeUpdateFieldInfo.FieldType.IsValueType && !toBeUpdateFieldInfo.FieldType.IsPrimitive && !toBeUpdateFieldInfo.FieldType.IsEnum) || 
                (toBeUpdatePropertyInfo != null && toBeUpdatePropertyInfo.PropertyType.IsValueType && !toBeUpdatePropertyInfo.PropertyType.IsPrimitive && !toBeUpdatePropertyInfo.PropertyType.IsEnum))
            {
                Plugin.Log.LogInfo("STRUCT");
                Plugin.Log.LogInfo(property.StyledName);

                var parentObjectContainingValues = (ExpandoObject)objectContaingValues.GetPropertyValue(property.StyledName);

                if (parentObjectContainingValues == null)
                {
                    return;
                }
                
                // Plugin.Log.LogInfo(test + "");
                // var asd = (ExpandoObject)test.GetPropertyValue("Size");
                // Plugin.Log.LogInfo(asd + "");
                // Plugin.Log.LogInfo(asd.GetPropertyValue("x") + "");
                // Plugin.Log.LogInfo(test.GetType() + "");
                
                // var allFields = parentObjectContainingValues.GetAllFields();
                //
                // foreach (var pair in allFields)
                // {
                //     string propertyName = pair.Key;
                //
                //     Debug.Log($"Property name: {propertyName}, Value: {parentObjectContainingValues.GetPropertyValue(propertyName)}");
                // }

                var structInstance = toBeUpdateFieldInfo != null ? 
                        Activator.CreateInstance(toBeUpdateFieldInfo.FieldType) : 
                        Activator.CreateInstance(toBeUpdatePropertyInfo.PropertyType);
                
                var dynamicProperties1 = toBeUpdateFieldInfo != null ?
                    GetDynamicPropertiesWithoutValues(GetFilteredFieldInfos(toBeUpdateFieldInfo.FieldType)) :
                    GetDynamicPropertiesWithoutValues(GetFilteredFieldInfos(toBeUpdatePropertyInfo.PropertyType));
                Plugin.Log.LogWarning(dynamicProperties1.Count() + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                Plugin.Log.LogWarning(structInstance.GetType() + "");
                foreach (var dynamicProperty in dynamicProperties1)
                {
                    Plugin.Log.LogWarning(dynamicProperty.StyledName);
                    var hgf = parentObjectContainingValues.GetPropertyValue(dynamicProperty.StyledName);
                    Plugin.Log.LogInfo(hgf + "");
                    if (hgf == null)
                    {
                        hgf = parentObjectContainingValues.GetPropertyValue(dynamicProperty.OriginalName);
                    }
                    // Plugin.Log.LogWarning(hgf.GetPropertyValue("x") + "  aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                    UpdateValues(dynamicProperty, parentObjectContainingValues, structInstance, hgf);
                }
                
                var dynamicProperties2 =  toBeUpdateFieldInfo != null ?
                    GetDynamicPropertiesWithoutValues(GetPropertyInfos(toBeUpdateFieldInfo.FieldType)) :
                    GetDynamicPropertiesWithoutValues(GetPropertyInfos(toBeUpdatePropertyInfo.PropertyType));
                
                Plugin.Log.LogWarning(dynamicProperties2.Count() + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                Plugin.Log.LogWarning(structInstance.GetType() + "");
                foreach (var dynamicProperty in dynamicProperties2)
                {
                    Plugin.Log.LogWarning(dynamicProperty.StyledName);
                    var hgf = parentObjectContainingValues.GetPropertyValue(dynamicProperty.StyledName);
                    if (hgf == null)
                    {
                        hgf = parentObjectContainingValues.GetPropertyValue(dynamicProperty.OriginalName);
                    }
                    Plugin.Log.LogInfo("Property Value: " + hgf);
                    // Plugin.Log.LogWarning(hgf.GetPropertyValue("x") + "  aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                    UpdateValues(dynamicProperty, parentObjectContainingValues, structInstance, hgf);
                }
                
                Plugin.Log.LogInfo("structInstance: " + structInstance + "");
                if (toBeUpdateFieldInfo != null)
                {
                    toBeUpdateFieldInfo.SetValue(objectToBeUpdated, structInstance);
                    var test = toBeUpdateFieldInfo.GetValue(objectToBeUpdated);
                    Plugin.Log.LogInfo("test length: " + test + "");
                }
                else
                {
                    toBeUpdatePropertyInfo.SetValue(objectToBeUpdated, structInstance);
                    var test = toBeUpdatePropertyInfo.GetValue(objectToBeUpdated);
                    Plugin.Log.LogInfo("test length: " + test + "");
                }

                return;
            }

            if (toBeUpdateFieldInfo != null)
            {
                if (newValue != null)
                {
                    toBeUpdateFieldInfo.SetValue(objectToBeUpdated, Convert.ChangeType(newValue, toBeUpdateFieldInfo.FieldType));
                }
            }
            if (toBeUpdatePropertyInfo != null)
            {
                if (newValue != null && toBeUpdatePropertyInfo.GetSetMethod() != null)
                {
                    toBeUpdatePropertyInfo.SetValue(objectToBeUpdated, Convert.ChangeType(newValue, toBeUpdatePropertyInfo.PropertyType));
                }
            }

            Plugin.Log.LogInfo(property.OriginalName);
        }
    }
}