// using Bindito.Core;
// using Timberborn.BehaviorSystem;
// using Timberborn.Carrying;
// using Timberborn.Goods;
// using Timberborn.InventorySystem;
// using Timberborn.TimeSystem;
// using Timberborn.WorkSystem;
// using UnityEngine;
//
// namespace ChooChoo
// {
//   internal class DistributeGoodWorkplaceBehavior : WorkplaceBehavior
//   {
//     private IDayNightCycle _dayNightCycle;
//     private DistributionRouteCalculator _distributionRouteCalculator;
//     private CarryAmountCalculator _carryAmountCalculator;
//     private GoodsStation _distributionPost;
//
//     [Inject]
//     public void InjectDependencies(
//       IDayNightCycle dayNightCycle,
//       DistributionRouteCalculator distributionRouteCalculator,
//       CarryAmountCalculator carryAmountCalculator)
//     {
//       this._dayNightCycle = dayNightCycle;
//       this._distributionRouteCalculator = distributionRouteCalculator;
//       this._carryAmountCalculator = carryAmountCalculator;
//     }
//
//     public void Awake() => this._distributionPost = this.GetComponent<GoodsStation>();
//
//     public override Decision Decide(GameObject agent)
//     {
//       DistributionRoute distributionRoute = this.PickLeastSatisfiedRoute(agent);
//       return distributionRoute == null ? Decision.ReleaseNow() : this.CompleteRoute(agent, distributionRoute);
//     }
//
//     private DistributionRoute PickLeastSatisfiedRoute(GameObject agent)
//     {
//       DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction? currentLeastSatisfiedRoute = new DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction?();
//       int liftingCapacity = agent.GetComponent<GoodCarrier>().LiftingCapacity;
//       foreach (DistributionRoute distributionRoute in this._distributionPost.DistributionRoutes)
//       {
//         if (DistributeGoodWorkplaceBehavior.CanCompleteRouteInTime(distributionRoute, agent))
//         {
//           DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction? routeSatisfaction = this.GetRouteSatisfaction(distributionRoute, liftingCapacity);
//           if (routeSatisfaction.HasValue)
//           {
//             DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction valueOrDefault = routeSatisfaction.GetValueOrDefault();
//             if (DistributeGoodWorkplaceBehavior.IsFirstOrLeastSatisfiedRoute(valueOrDefault, currentLeastSatisfiedRoute))
//               currentLeastSatisfiedRoute = new DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction?(valueOrDefault);
//           }
//         }
//       }
//       return currentLeastSatisfiedRoute?.DistributionRoute;
//     }
//
//     private static bool CanCompleteRouteInTime(
//       DistributionRoute distributionRoute,
//       GameObject agent)
//     {
//       return agent.GetComponent<IGoodDistributor>().CanCompleteRouteInTime(distributionRoute);
//     }
//
//     private DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction? GetRouteSatisfaction(
//       DistributionRoute route,
//       int liftingCapacity)
//     {
//       float? nullable = this._distributionRouteCalculator.RouteSatisfaction(route, liftingCapacity);
//       if (!nullable.HasValue)
//         return new DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction?();
//       float valueOrDefault = nullable.GetValueOrDefault();
//       return new DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction?(new DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction(route, valueOrDefault));
//     }
//
//     private static bool IsFirstOrLeastSatisfiedRoute(
//       DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction routeToCheck,
//       DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction? currentLeastSatisfiedRoute)
//     {
//       return !currentLeastSatisfiedRoute.HasValue || DistributeGoodWorkplaceBehavior.IsLeastSatisfiedRoute(routeToCheck, currentLeastSatisfiedRoute.Value);
//     }
//
//     private static bool IsLeastSatisfiedRoute(
//       DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction routeToCheck,
//       DistributeGoodWorkplaceBehavior.DistributionRouteSatisfaction currentLeastSatisfiedRoute)
//     {
//       return (double) routeToCheck.Satisfaction <= (double) currentLeastSatisfiedRoute.Satisfaction && routeToCheck.DistributionRoute.IsMoreStaleThan(currentLeastSatisfiedRoute.DistributionRoute);
//     }
//
//     private Decision CompleteRoute(GameObject agent, DistributionRoute distributionRoute)
//     {
//       int take = this.AmountToTake(agent, distributionRoute);
//       DistributeGoodWorkplaceBehavior.Reserve(agent, distributionRoute, take);
//       distributionRoute.UpdateLastCompletionTime(this._dayNightCycle.PartialDayNumber);
//       return Decision.ReleaseNextTick();
//     }
//
//     private int AmountToTake(GameObject agent, DistributionRoute distributionRoute)
//     {
//       GoodsStation start = distributionRoute.Start;
//       DropOffPoint end = distributionRoute.End;
//       return Mathf.Min(this._carryAmountCalculator.AmountToCarry(agent.GetComponent<GoodCarrier>().LiftingCapacity, distributionRoute.GoodId, (IAmountProvider) end.Inventory, (IAmountProvider) start.Inventory).Amount, this._distributionRouteCalculator.MissingAmountToHighLimit(distributionRoute));
//     }
//
//     private static void Reserve(GameObject agent, DistributionRoute distributionRoute, int amount)
//     {
//       Inventory inventory1 = distributionRoute.Start.Inventory;
//       Inventory inventory2 = distributionRoute.End.Inventory;
//       GoodReserver component = agent.GetComponent<GoodReserver>();
//       GoodAmount goodAmount = new GoodAmount(distributionRoute.GoodId, amount);
//       component.ReserveExactStockAmount(inventory1, goodAmount);
//       component.ReserveCapacity(inventory2, goodAmount);
//     }
//
//     private readonly struct DistributionRouteSatisfaction
//     {
//       public DistributionRoute DistributionRoute { get; }
//
//       public float Satisfaction { get; }
//
//       public DistributionRouteSatisfaction(DistributionRoute distributionRoute, float satisfaction)
//       {
//         this.DistributionRoute = distributionRoute;
//         this.Satisfaction = satisfaction;
//       }
//     }
//   }
// }
