using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Timberborn.BehaviorSystem;
using Unity.Collections;
using Unity.Jobs;
using Object = UnityEngine.Object;

namespace MultithreadedNavigation
{
    struct MultithreadedNavigationJob: IJobParallelFor
    {
        public NativeArray<MyBehaviorManager.Data> MyBehaviorManagers;
        
        public void Execute(int index)
        {
            var data = MyBehaviorManagers[index];
            
            var BehaviorManager = data.BehaviorManager;   
            // var ReturnToBehavior = data.ReturnToBehavior;   
            // var RunningBehavior = data.RunningBehavior;
            // var RootBehaviors = data.RootBehaviors;
            // var RunningExecutor = data.RunningExecutor;
            
            // Plugin.Log.LogFatal(BehaviorManager);   
            // Plugin.Log.LogFatal(ReturnToBehavior);   
            // Plugin.Log.LogFatal(RunningBehavior);   
            // Plugin.Log.LogFatal(RootBehaviors);

            
            // BehaviorManager.Tick();
            
            try
            {
                // Stopwatch stopwatch = Stopwatch.StartNew();
                BehaviorManager.Tick();
                // stopwatch.Stop();
                // Plugin.Log.LogInfo(stopwatch.ElapsedTicks);
                // stopwatch.Reset();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            // Plugin.Log.LogFatal("complete");
            // data.BehaviorManager = BehaviorManager;
            // MyBehaviorManagers[index] = data;
            
            
            
            
            // MethodInfo methodInfo;
            //
            // if (RunningExecutor != null)
            // {
            //     methodInfo = typeof(BehaviorManager).GetMethod("TickRunningExecutor", BindingFlags.NonPublic | BindingFlags.Instance);
            //     methodInfo.Invoke(BehaviorManager, new object[] { });
            // }
            //
            // Plugin.Log.LogFatal(index + ": First if");
            //
            // if (RunningExecutor != null)
            //     return;
            //
            // Plugin.Log.LogFatal(index + ": Second if");
            //
            // methodInfo = typeof(BehaviorManager).GetMethod("ProcessBehaviors", BindingFlags.NonPublic | BindingFlags.Instance);
            // methodInfo.Invoke(BehaviorManager, new object[] { });
            //
            // MyBehaviorManagers[index] = data;



            // bool flag1 = false;


            // if (RunningBehavior == null)
            // {
            //     Plugin.Log.LogInfo("RunningBehavior == null");
            //     // flag1 = true;
            //     // return;
            // }
            // // Plugin.Log.LogFatal(RunningBehavior.Name);
            // // Plugin.Log.LogFatal(RunningBehavior.GetType());
            //     
            //
            // MethodInfo methodInfo = typeof(BehaviorManager).GetMethod("ProcessBehavior", BindingFlags.NonPublic | BindingFlags.Instance);
            //
            // if (ReturnToBehavior && (bool)(Object)RunningBehavior && (bool)methodInfo.Invoke(BehaviorManager, new object[] {RunningBehavior}))
            //     return;
            //
            // Plugin.Log.LogInfo("First check: true");
            //
            // try
            // {
            //     using (List<RootBehavior>.Enumerator enumerator = RootBehaviors.GetEnumerator())
            //     {
            //         do
            //             ;
            //         while (enumerator.MoveNext() && !(bool)methodInfo.Invoke(BehaviorManager, new object[] { enumerator.Current }));
            //     }
            //
            //     data.BehaviorManager = BehaviorManager;
            //     MyBehaviorManagers[index] = data;
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e);
            // }

        }
    }
}
