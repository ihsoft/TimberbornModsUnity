using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using BepInEx;
using Timberborn.SingletonSystem;

namespace CustomNameList
{
    class CustomNameService : ILoadableSingleton
    {
        private static readonly Random Random = new();
        private static string _textFile = $"{Path.GetDirectoryName(Paths.ExecutablePath)}{Path.DirectorySeparatorChar}names.txt";
        private List<String> _allNames;
        private Stack<String> _nextNames;


        public void Load() 
        {
            if (!File.Exists(_textFile)) {
                Plugin.Log.LogError($"Could not find names file at: {_textFile}");
                Plugin.Log.LogWarning($"Will use standard game names instead.");
                return;
            }

            _allNames = File.ReadAllLines(_textFile).ToList().Select(e => e.Trim().Replace("\r", "")).ToList();

            Plugin.Log.LogInfo($"Read {_allNames.Count()} names from {_textFile}");

            RefillNames();
        }

        public String NextName() 
        {
            if(_nextNames.Count == 0)
                RefillNames();

            var nextName = _nextNames.Pop();

            return nextName;
        }

        private void RefillNames() 
        {
            _nextNames = new Stack<String>(_allNames.OrderBy(a => Random.Next()));
        }
    }
}