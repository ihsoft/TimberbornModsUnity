using System.Collections.Generic;
using Timberborn.BehaviorSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultithreadedNavigation
{
    public class MyBehaviorManager : MonoBehaviour
    {
        public BehaviorManager behaviorManager;
        // public bool returnToBehavior;
        // public Behavior runningBehavior;
        // public List<RootBehavior> rootBehaviors;
        // public IExecutor runningExecutor;

        public MyBehaviorManager(
            BehaviorManager behaviorManager
            // ref bool returnToBehavior,
            // ref Behavior runningBehavior, 
            // ref List<RootBehavior> rootBehaviors,
            // ref IExecutor runningExecutor
            )
        {
            this.behaviorManager = behaviorManager;
            // this.returnToBehavior = returnToBehavior;
            // this.runningBehavior = runningBehavior;
            // this.rootBehaviors = rootBehaviors;
            // this.runningExecutor = runningExecutor;
        }
        
        public struct Data
        {
            public BehaviorManager BehaviorManager;
            // public bool ReturnToBehavior;
            // public Behavior RunningBehavior;
            // public List<RootBehavior> RootBehaviors;
            // public IExecutor RunningExecutor;

            public Data(
                MyBehaviorManager myBehaviorManager
                // ref bool returnToBehavior,
                // ref Behavior runningBehavior, 
                // ref List<RootBehavior> rootBehaviors,
                // ref IExecutor runningExecutor
                )
            {
                BehaviorManager = myBehaviorManager.behaviorManager;
                // ReturnToBehavior = returnToBehavior;
                // RunningBehavior = runningBehavior;
                // RootBehaviors = rootBehaviors;
                // RunningExecutor = runningExecutor;
            }
        }
    }
}

