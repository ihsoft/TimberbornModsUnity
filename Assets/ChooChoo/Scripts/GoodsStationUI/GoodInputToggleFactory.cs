// using Timberborn.CoreUI;
// using Timberborn.Localization;
// using Timberborn.SliderToggleSystem;
// using UnityEngine.UIElements;
//
// namespace ChooChoo
// {
//   public class GoodInputToggleFactory
//   {
//     private readonly SliderToggleFactory _sliderToggleFactory;
//     private readonly ILoc _loc;
//
//     public GoodInputToggleFactory(SliderToggleFactory sliderToggleFactory, ILoc loc)
//     {
//       _sliderToggleFactory = sliderToggleFactory;
//       _loc = loc;
//     }
//
//     public GoodInputToggle Create(VisualElement parent, string goodId)
//     {
//       GoodInputToggle goodInputToggle = new GoodInputToggle(_sliderToggleFactory, _loc);
//       goodInputToggle.Initialize(parent, goodId);
//       return goodInputToggle;
//     }
//   }
// }
