using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TimberApi.Common.SingletonSystem;
using Timberborn.PathSystem;
using Timberborn.Persistence;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MorePaths
{
    public class MorePathsCore : ITimberApiLoadableSingleton
    {
        public IEnumerable<Object> PathObjects;

        private readonly ISpecificationService _specificationService;
        private readonly PathSpecificationObjectDeserializer _pathSpecificationObjectDeserializer;

        private MethodInfo _methodInfo;
        public ImmutableArray<PathSpecification> PathsSpecifications;
        public List<CustomPath> CustomPaths;
        private readonly Dictionary<string, FieldInfo> _fieldInfos = new();
        private readonly Dictionary<string, MethodInfo> _methodInfos = new();

        public MorePathsCore(ISpecificationService specificationService, PathSpecificationObjectDeserializer pathSpecificationObjectDeserializer)
        {
            _specificationService = specificationService;
            _pathSpecificationObjectDeserializer = pathSpecificationObjectDeserializer;
        }

        public void Load()
        {
            LoadPathSpecifications();
        }

        private void LoadPathSpecifications()
        {
            var list = _specificationService.GetSpecifications(_pathSpecificationObjectDeserializer).Where(specification => specification.Enabled).ToList();
            var orderedList = list.OrderBy(specification => specification.ToolOrder);
            PathsSpecifications = orderedList.ToImmutableArray();
        }

        private void LoadAllPathObjects()
        {
            var timberbornPathObjects = Resources.LoadAll("", typeof(DynamicPathModel));
            PathObjects = timberbornPathObjects.Concat(CustomPaths.Select(path => path.PathGameObject));
        }

        public void AddFakePathsToObjectsPatch(ref IEnumerable<Object> result)
        {
            if (CustomPaths == null)
            {
                TimberApi.DependencyContainerSystem.DependencyContainer.GetInstance<CustomPathFactory>().CreatePathsFromSpecification();
                LoadAllPathObjects();
            }
            
            var pathGameObject = result.First(o => o.name.Split(".")[0] == "Path");
            if (pathGameObject == null) return;

            var resultList = result.ToList();
            resultList.Remove(pathGameObject);

            var newObjectsList = resultList.Concat(CustomPaths.Select(path => path.PathGameObject));

            result = newObjectsList;
        }
        
        public Texture2D TryLoadTexture(string pathName, string fileName, int width = 1024, int height = 1024)
        {
            var texture2D =  new Texture2D(width, height);
            if (pathName == null || fileName == null)
                return texture2D;
            var filePath = Path.Combine(Plugin.path, "Paths", pathName, fileName);
            if (File.Exists(filePath))
                texture2D.LoadImage(File.ReadAllBytes(filePath));
            return texture2D;
        }

        public object InvokePrivateMethod(object instance, string methodName)
        {
            if (!_methodInfos.ContainsKey(methodName))
            {
                _methodInfos.Add(methodName, AccessTools.TypeByName(instance.GetType().Name).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            
            return _methodInfos[methodName].Invoke(instance, new object[]{});
        }

        public void ChangePrivateField(object instance, string fieldName, object newValue)
        {
            if (!_fieldInfos.ContainsKey(fieldName))
            {
                _fieldInfos.Add(fieldName, AccessTools.TypeByName(instance.GetType().Name).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            
            _fieldInfos[fieldName].SetValue(instance, newValue);
        }

        public object GetPrivateField(object instance, string fieldName)
        {
            if (!_fieldInfos.ContainsKey(fieldName))
            {
                _fieldInfos.Add(fieldName, AccessTools.TypeByName(instance.GetType().Name).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            
            return _fieldInfos[fieldName].GetValue(instance);
        }
    }
}