using System.Collections.Generic;
using Timberborn.BehaviorSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultithreadedNavigation
{
    public class MyBehaviorManager : MonoBehaviour
    {
        public BehaviorManager behaviorManager;
        public bool returnToBehavior;
        public Behavior runningBehavior;
        public List<RootBehavior> rootBehaviors;

        public MyBehaviorManager(
            BehaviorManager behaviorManager,
            bool returnToBehavior,
            Behavior runningBehavior, 
            List<RootBehavior> rootBehaviors)
        {
            this.behaviorManager = behaviorManager;
            this.returnToBehavior = returnToBehavior;
            this.runningBehavior = runningBehavior;
            this.rootBehaviors = rootBehaviors;
        }
        
        public struct Data
        {
            public BehaviorManager BehaviorManager;
            public bool ReturnToBehavior;
            public Behavior RunningBehavior;
            public List<RootBehavior> RootBehaviors;

            public Data(MyBehaviorManager myBehaviorManager)
            {
                BehaviorManager = myBehaviorManager.behaviorManager;
                ReturnToBehavior = myBehaviorManager.returnToBehavior;
                RunningBehavior = myBehaviorManager.runningBehavior;
                RootBehaviors = myBehaviorManager.rootBehaviors;
            }
        }
    }
}

