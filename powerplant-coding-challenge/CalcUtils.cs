using powerplant_coding_challenge.Model;
using System.Collections.Immutable;

namespace powerplant_coding_challenge;

public static partial class CalcUtils
{
    /// <summary>
    /// The main function to compute the production of powerplants, given a load, the price of gas and kerosine, the availability of wind and the current powerplants.
    /// </summary>
    public static PowerplantProductionResult ComputeProduction(
        Load load,
        Fuel.Gas gasPrice,
        Fuel.Kerosine kerosinePrice,
        Fuel.Wind windAvailability,
        ImmutableList<Powerplant> powerplants) =>
            powerplants
            .AdjustPMaxOfWindTurbines(windAvailability)
            .SortByMeritOrder(gasPrice, kerosinePrice)
            .ComputeNecessaryProduction(load);

    internal static ImmutableList<Powerplant> AdjustPMaxOfWindTurbines(this ImmutableList<Powerplant> powerplants, Fuel.Wind windAvailability) =>
        powerplants
            .Select(powerplant =>
                powerplant is Powerplant.WindTurbine windTurbine
                    ? windTurbine with { Pmax = windTurbine.Pmax * windAvailability.AverageWind }
                    : powerplant)
            .ToImmutableList();

    // I am not super familiar with merit-order sorting, but for now I'll sort descending on efficiency * price, always favoring wind turbines
    // There is some non-determinism when sorting two equivalent (same efficiency) powerplants (ex: gasfiredbig1 and gasfiredbig2)
    internal static ImmutableList<Powerplant> SortByMeritOrder(this ImmutableList<Powerplant> powerplants, Fuel.Gas gasPrice, Fuel.Kerosine kerosinePrice) =>
        powerplants
        .OrderBy(powerplant => powerplant switch
        {
            Powerplant.WindTurbine => -1, // Force it to be on top, even if gas or kerosine cost zero
            Powerplant.GasFired gasFired => (1 - gasFired.Efficiency) * gasPrice.EurosPerMWh,
            Powerplant.TurboJet turbojet => (1 - turbojet.Efficiency) * kerosinePrice.EurosPerMWh,
        })
        .ThenByDescending(PowerplantFuncs.GetEfficiency)
        .ToImmutableList();

    // The strategy here is to assume a Merit-Ordered input, and try to satisfy the load.
    // The high-level strategy is to walk through the input, and use the necessary value up to Pmax to satisfy the load.
    // There is an edge case when the "current" powerplant doesn't have enough capacity to satisfy the load, but the "next" powerplant
    // has a Pmin higher than the required load.
    // The network should never be underloaded or overloaded. I believe it would be better to be under-loaded, since we could try to look for
    // capacity in other powerplants, but overloading the network might not be a good choice (unless there would be storage capacity, but that's
    // outside the scope). This function will try to match the load, by sacrificing the "current" powerplant's production to compensate for the
    // next one's pmin.
    // While thinking a bit more about it, I am not sure what to do for the hypothetical input where all powerstations have a pmin higher than the load?
    // Good topics to discuss further.
    internal static PowerplantProductionResult ComputeNecessaryProduction(this ImmutableList<Powerplant> powerplants, Load load) =>
        powerplants
        .Zip(powerplants.Skip(1).Append(null))
        .Aggregate(seed: new PowerplantProductionResult(
            PowerplantProductions: ImmutableList<PowerplantProduction>.Empty,
            RemainingLoad: load),
            TrySatisfyLoad);

    internal static PowerplantProductionResult TrySatisfyLoad(
        PowerplantProductionResult acc,
        (Powerplant Current, Powerplant? Next) powerplants)
    {
        // Is the load already satisfied?
        if (acc.RemainingLoad.LoadValue <= 0)
            return CurrentWithProduction(0);

        var (pmin, pmax) = (powerplants.Current.GetPmin(), powerplants.Current.GetPmax());
        var proposedProduction = Math.Clamp(value: acc.RemainingLoad.LoadValue, min: pmin, max: pmax);

        // If the current powerplant doesn't have the capability to satisfy the load, we need to check if
        // turning on the next powerplant would exceed the load. In that case, we might need to underutilize the current powerplant
        if (acc.RemainingLoad.LoadValue >= pmax
            && powerplants.Next is Powerplant next 
            && next.GetPmin() + proposedProduction > acc.RemainingLoad.LoadValue) // The current powerplant's capacity is not enough
        {
            var loadFromCurrent = Math.Clamp(acc.RemainingLoad.LoadValue - next.GetPmin(), min: pmin, max: pmax);

            return CurrentWithProduction(loadFromCurrent);
        }

        // Otherwise, let the current powerplant work at necessary capacity
        return CurrentWithProduction(proposedProduction);

        PowerplantProductionResult CurrentWithProduction(decimal production) =>
            acc with { 
                PowerplantProductions = acc.PowerplantProductions.Add(new(powerplants.Current.GetName(), Production: production)),
                RemainingLoad = new (LoadValue: acc.RemainingLoad.LoadValue - production),
            };
    }
}