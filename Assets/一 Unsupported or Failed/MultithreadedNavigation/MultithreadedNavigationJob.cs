using System;
using System.Diagnostics;
using HarmonyLib;
using Unity.Collections;
using Unity.Jobs;

namespace MultithreadedNavigation
{
    struct MultithreadedNavigationJob: IJobParallelFor
    {
        public NativeArray<MyBehaviorManager> MyBehaviorManagers;
        
        public void Execute(int index)
        {
            var data = MyBehaviorManagers[index];
            var behaviorManager = data.BehaviorManager;   
            
            try
            {
                // Stopwatch stopwatch = Stopwatch.StartNew();
                behaviorManager.Tick();
                // stopwatch.Stop();
                // var time = stopwatch.ElapsedTicks;
                // if (time > 1000)
                // {
                //     Plugin.Log.LogFatal(time);
                // }
                // stopwatch.Reset();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            // Plugin.Log.LogFatal("complete");
            // data.BehaviorManager = BehaviorManager;
            // MyBehaviorManagers[index] = data;
        }
    }
}
