using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Timers;
using Timberborn.TickSystem;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Analytics;

namespace MultithreadedNavigation
{
    public class MultithreadedNavigationService : ITickableSingleton
    {
        public readonly RunMethodsOnMainThread RunMethodsOnMainThread;

        MultithreadedNavigationService(RunMethodsOnMainThread runMethodsOnMainThread)
        {
            RunMethodsOnMainThread = runMethodsOnMainThread;
        }
        
        // public readonly Dictionary<int, MyBehaviorManager> BehaviorManagers = new();
        public readonly List<MyBehaviorManager> MyBehaviorManagers = new();

        Stopwatch stopwatch = Stopwatch.StartNew();
        private List<long> times = new();

        public void Tick()
        {
            // var time = BehaviorManagerPatch.Time;
            // times.Add(time);
            // if (times.Count>3)
            // {
            //     Plugin.Log.LogFatal(times.GetRange(2, times.Count-3).Average());
            // }
           
            // BehaviorManagerPatch.Time = 0;
            Plugin.Log.LogInfo("Tick");
            
            
            stopwatch.Start();
            for (int i = 0; i < MyBehaviorManagers.Count; i++)
            {
                var newObject = MyBehaviorManagers[i];
                if (newObject.BehaviorManager == null)
                {
                    MyBehaviorManagers.Remove(newObject);
                }
            }
            var behaviorManagers = new NativeArray<MyBehaviorManager>(MyBehaviorManagers.ToArray(), Allocator.TempJob);
            var job = new MultithreadedNavigationJob { MyBehaviorManagers = behaviorManagers };
            var jobHandle = job.Schedule(behaviorManagers.Length, behaviorManagers.Length / 10);
            jobHandle.Complete();
            behaviorManagers.Dispose();
            
            RunMethodsOnMainThread.UpdateAllVisibilities();

            stopwatch.Stop();
            times.Add(stopwatch.ElapsedTicks);
            if (times.Count>3)
            {
                Plugin.Log.LogFatal(times.GetRange(2, times.Count-3).Average());
            }
            stopwatch.Reset();
        }
        
        private readonly Dictionary<string, bool> _secondCalls = new();
        // private readonly Dictionary<string, object> _locks = new();
        // private static readonly object LocksLock = new object();
        private static readonly object LockObject = new object();
        private static readonly object SecondCallsLock = new object();
        
        
        static readonly ConcurrentDictionary<string, object> LockObjects = new();

        private static readonly object NonVoidFunctionLock = new object();
        private static readonly List<string> PlantingMethods = new() {"FindClosest", "GetClosestOrDefault", "GetNeighboring", "GetReachable", "ReserveCoordinates"};

        public Tuple<object, bool> LockedNonVoidFunction(object __instance, object[] __args, MethodBase __originalMethod)
        {
            var methodName = __originalMethod.Name;
            var lockObject = LockObjects.GetOrAdd(methodName, new object());

            if (PlantingMethods.Contains(methodName))
            {
                lockObject = LockObjects.GetOrAdd("Planting", new object());
            }
            
            lock (lockObject)
            {
                if (!_secondCalls.ContainsKey(methodName))
                {
                    _secondCalls.Add(methodName, false);
                }
                if (_secondCalls[methodName])
                {
                    _secondCalls[methodName] = false;
                    return Tuple.Create(default(object), true);
                }
                _secondCalls[methodName] = true;
                return Tuple.Create(__originalMethod.Invoke(__instance, __args), false);
            }
        }
        
        // private static readonly object VoidFunctionLock = new object();
        public bool LockedVoidFunction(object __instance, object[] __args, MethodBase __originalMethod)
        {
            var methodName = __originalMethod.Name;
            var lockObject = LockObjects.GetOrAdd(methodName, new object());
            
            if (PlantingMethods.Contains(methodName))
            {
                lockObject = LockObjects.GetOrAdd("Planting", new object());
            }
            
            lock (lockObject)
            {
                methodName = __originalMethod.Name;
                if (!_secondCalls.ContainsKey(methodName))
                {
                    _secondCalls.Add(methodName, false);
                }
                if (_secondCalls[methodName])
                {
                    _secondCalls[methodName] = false;
                    return true;
                }
                _secondCalls[methodName] = true;
                __originalMethod.Invoke(__instance, __args);
                return false;
            }
        }
    }
}
