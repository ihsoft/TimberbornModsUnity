using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.Carrying;
using Timberborn.Characters;
using Timberborn.Goods;
using Timberborn.RecoveredGoodSystem;
using UnityEngine;

namespace ChooChoo
{
  public class Destroyable : MonoBehaviour
  {
    private RecoveredGoodStackSpawner _recoveredGoodStackSpawner;
    private ChooChooCore _chooChooCore;
    private Character _character;

    private List<GoodCarrier> _goodCarriers;

    [Inject]
    public void InjectDependencies(RecoveredGoodStackSpawner recoveredGoodStackSpawner, ChooChooCore chooChooCore)
    {
      _recoveredGoodStackSpawner = recoveredGoodStackSpawner;
      _chooChooCore = chooChooCore;
    }

    public void Awake()
    {
      _character = GetComponent<Character>();
    }

    public void Start()
    {
      _goodCarriers = GetComponent<TrainWagonManager>().TrainWagons.Select(wagon => wagon.GetComponent<GoodCarrier>()).ToList();
    }

    public void Destroy()
    {
      _character.KillCharacter();
      gameObject.SetActive(false);
      _character.DestroyCharacter();
      var position = transform.position;
      var wrongPosition = new Vector3(position.x, position.z, position.y);
      // _recoveredGoodStackSpawner.AddAwaitingGoods(Vector3Int.RoundToInt(wrongPosition), GetCarriedGoods());
      _chooChooCore.InvokePublicMethod(_recoveredGoodStackSpawner, "AddAwaitingGoods", new object[]{ Vector3Int.RoundToInt(wrongPosition), GetCarriedGoods()});
    }

    private List<GoodAmount> GetCarriedGoods() => _goodCarriers.Where(carrier => carrier.IsCarrying).Select(carrier => carrier.CarriedGoods).ToList();
  }
}
