using System;
using System.Collections.Generic;
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
            Plugin.Log.LogInfo(index);  
            
            var BehaviorManager = MyBehaviorManagers[index].BehaviorManager;   
            var ReturnToBehavior = MyBehaviorManagers[index].ReturnToBehavior;   
            var RunningBehavior = MyBehaviorManagers[index].RunningBehavior;
            var RootBehaviors =MyBehaviorManagers[index].RootBehaviors;
            
            Plugin.Log.LogFatal(BehaviorManager);   
            Plugin.Log.LogFatal(ReturnToBehavior);   
            Plugin.Log.LogFatal(RunningBehavior);   
            Plugin.Log.LogFatal(RootBehaviors);
            
            if (RunningBehavior == null)
            {
                Plugin.Log.LogInfo("RunningBehavior == null");
                return;
            }
                
            
            MethodInfo methodInfo = typeof(BehaviorManager).GetMethod("ProcessBehavior", BindingFlags.NonPublic | BindingFlags.Instance);

            if (ReturnToBehavior && (bool) (Object) RunningBehavior && (bool)methodInfo.Invoke(BehaviorManager, new object[] {RunningBehavior}))
                return;
            
            Plugin.Log.LogInfo("First check: true");
            
            try
            {
                using (List<RootBehavior>.Enumerator enumerator = RootBehaviors.GetEnumerator())
                {
                    do
                        ;
                    while (enumerator.MoveNext() && enumerator.Current != null &&
                           !(bool)methodInfo.Invoke(BehaviorManager, new object[] { enumerator.Current }));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
    }
}
