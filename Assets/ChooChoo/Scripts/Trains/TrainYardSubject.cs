using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
    public class TrainYardSubject : MonoBehaviour, IPersistentEntity
    {
        private static readonly ComponentKey TrainYardSubjectKey = new(nameof(TrainYardSubject));

        private static readonly PropertyKey<GameObject> LinkedTrainYardKey = new(nameof(LinkedGlobalMarket));

        private static readonly PropertyKey<Vector3> LinkedTrainYardPositionKey = new(nameof(LinkedGlobalMarketPosition));

        public GameObject LinkedGlobalMarket { get; set; }

        public Vector3 LinkedGlobalMarketPosition { get; set; }

        public void Save(IEntitySaver entitySaver)
        {
            entitySaver.GetComponent(TrainYardSubjectKey).Set(LinkedTrainYardKey, LinkedGlobalMarket);
            entitySaver.GetComponent(TrainYardSubjectKey).Set(LinkedTrainYardPositionKey, LinkedGlobalMarketPosition);
        }

        public void Load(IEntityLoader entityLoader)
        {
            LinkedGlobalMarket = entityLoader.GetComponent(TrainYardSubjectKey).Get(LinkedTrainYardKey);
            LinkedGlobalMarketPosition = entityLoader.GetComponent(TrainYardSubjectKey).Get(LinkedTrainYardPositionKey);
        }
    }
}
