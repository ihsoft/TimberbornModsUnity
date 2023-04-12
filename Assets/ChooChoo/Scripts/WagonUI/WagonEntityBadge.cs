using Bindito.Core;
using ChooChoo;
using Timberborn.AssetSystem;
using Timberborn.Characters;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using Timberborn.SelectionSystem;
using UnityEngine;

namespace ChooChoo
{
  public class WagonEntityBadge : MonoBehaviour, IEntityBadge
  {
    private const string TrainDisplayNameLocKey = "Tobbert.Train.Name";
    
    private const string AgeLocKey = "Beaver.Age";
    
    private ILoc _loc;
    
    private IResourceAssetLoader _resourceAssetLoader;
    
    private SelectionManager _selectionManager;

    private TrainWagon _trainWagon;
    
    private Character _character;

    private Sprite _sprite;

    [Inject]
    public void InjectDependencies(
      ILoc loc,
      IResourceAssetLoader resourceAssetLoader,
      SelectionManager selectionManager)
    {
      _loc = loc;
      _resourceAssetLoader = resourceAssetLoader;
      _selectionManager = selectionManager;
    }

    public int EntityBadgePriority => 1;

    public void Awake()
    {
      _character = GetComponent<Character>();
      _sprite = _resourceAssetLoader.Load<Sprite>("tobbert.choochoo/tobbert_choochoo/ToolGroupIcon");
    }

    public string GetEntityName() => "<b>" + _character.FirstName + "</b>";

    public string GetEntitySubtitle()
    {
      return Age();
    }

    public ClickableSubtitle GetEntityClickableSubtitle()
    {
      GameObject train = _trainWagon.Train;
      return ClickableSubtitle.Create(() => _selectionManager.SelectAndFocusOn(train), _loc.T(TrainDisplayNameLocKey));
    }

    public Sprite GetEntityAvatar() => _sprite;
    
    private string Age() => _loc.T(AgeLocKey, _character.Age);
  }
}
