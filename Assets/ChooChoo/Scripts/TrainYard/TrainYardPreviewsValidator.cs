using System.Collections.Generic;
using System.Linq;
using ChooChoo;
using Timberborn.Localization;
using Timberborn.PreviewSystem;

namespace ChooChoo
{
  internal class TrainYardPreviewsValidator : IPreviewsValidator
  {
    private static readonly string ErrorMessageLocKey = "Tobbert.TrainYard.PreviewErrorMessage";
    private readonly TrainYardService _trainYardService;
    private readonly ILoc _loc;

    public TrainYardPreviewsValidator(TrainYardService trainYardService, ILoc loc)
    {
      _trainYardService = trainYardService;
      _loc = loc;
    }

    public bool PreviewsAreValid(IReadOnlyList<Preview> previews, out string errorMessage)
    {
      if (TrainYardAlreadyPlaced(previews))
      {
        errorMessage = _loc.T(ErrorMessageLocKey);
        return false;
      }
      errorMessage = null;
      return true;
    }

    private bool TrainYardAlreadyPlaced(IEnumerable<Preview> previews) 
    {
      var list = previews.Select(preview => preview.GetComponent<TrainYard>()).Where(trainYard => (bool)(UnityEngine.Object)trainYard).ToList();
      if (list.Count > 0 && _trainYardService.CurrentTrainYard != null)
      {
        return true;
      }

      return false;
    }
  }
}
