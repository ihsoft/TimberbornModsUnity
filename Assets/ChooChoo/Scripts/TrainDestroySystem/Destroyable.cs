using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.Carrying;
using Timberborn.Characters;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.RecoveredGoodSystem;
using UnityEngine;

namespace ChooChoo
{
  public class Destroyable : MonoBehaviour
  {
    private RecoveredGoodStackSpawner _recoveredGoodStackSpawner;
    private TrainYardService _trainYardService;
    private Character _character;
    private Train _train;
    private GoodReserver _goodReserver;

    private List<GoodCarrier> _goodCarriers;

    [Inject]
    public void InjectDependencies(RecoveredGoodStackSpawner recoveredGoodStackSpawner, TrainYardService trainYardService)
    {
      _recoveredGoodStackSpawner = recoveredGoodStackSpawner;
      _trainYardService = trainYardService;
    }

    public void Awake()
    {
      _character = GetComponent<Character>();
      _train = GetComponent<Train>();
      _goodReserver = GetComponent<GoodReserver>();
    }

    public void Start()
    {
      _goodCarriers = GetComponent<TrainWagonManager>().TrainWagons.Select(wagon => wagon.GetComponent<GoodCarrier>()).ToList();
    }

    public void Destroy()
    {
      // _character.KillCharacter();
      _goodReserver.UnreserveStock();
      ChooChooCore.SetPrivateProperty(_goodReserver, "CapacityReservation", new GoodReservation());
      gameObject.SetActive(false);
      _character.DestroyCharacter();
      var position = transform.position;
      var wrongPosition = new Vector3(position.x, position.z, position.y);
      // _recoveredGoodStackSpawner.AddAwaitingGoods(Vector3Int.RoundToInt(wrongPosition), GetCarriedGoods());
      ChooChooCore.InvokePublicMethod(_recoveredGoodStackSpawner, "AddAwaitingGoods", new object[]{ Vector3Int.RoundToInt(wrongPosition), GetAllGoods()});
    }

    private List<GoodAmount> GetAllGoods()
    {
      List<GoodAmount> allGoods = new List<GoodAmount>();
      allGoods.AddRange(_train.TrainCost.Select(specification => specification.ToGoodAmount()));
      allGoods.AddRange(_goodCarriers.Where(carrier => carrier.IsCarrying).Select(carrier => carrier.CarriedGoods).ToList());
      return allGoods;
    }
  }
}
