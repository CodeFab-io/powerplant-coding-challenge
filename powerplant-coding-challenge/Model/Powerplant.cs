namespace powerplant_coding_challenge.Model;

public abstract record Powerplant
{
    public sealed record GasFired(string Name, decimal Efficiency, decimal Pmin, decimal Pmax) : Powerplant;

    public sealed record TurboJet(string Name, decimal Efficiency, decimal Pmin, decimal Pmax) : Powerplant;

    public sealed record WindTurbine(string Name, decimal Efficiency, decimal Pmin, decimal Pmax) : Powerplant;

    public T Map<T>(
        Func<GasFired, T> whenGasFired,
        Func<TurboJet, T> whenTurboJet,
        Func<WindTurbine, T> whenWindturbind) => this switch 
        { 
            GasFired gasFired => whenGasFired(gasFired),
            TurboJet turboJet => whenTurboJet(turboJet),
            WindTurbine windTurbine => whenWindturbind(windTurbine),
        };
}

public static class PowerplantFuncs 
{
    public static decimal GetEfficiency(this Powerplant powerplant) => powerplant.Map(
        whenGasFired: gasFired => gasFired.Efficiency,
        whenTurboJet: turboJet => turboJet.Efficiency,
        whenWindturbind: windTurbine => windTurbine.Efficiency);

    public static string GetName(this Powerplant powerplant) => powerplant.Map(
        whenGasFired: gasFired => gasFired.Name,
        whenTurboJet: turboJet => turboJet.Name,
        whenWindturbind: windTurbine => windTurbine.Name);

    public static decimal GetPmin(this Powerplant powerplant) => powerplant.Map(
        whenGasFired: gasFired => gasFired.Pmin,
        whenTurboJet: turboJet => turboJet.Pmin,
        whenWindturbind: windTurbine => windTurbine.Pmin);

    public static decimal GetPmax(this Powerplant powerplant) => powerplant.Map(
        whenGasFired: gasFired => gasFired.Pmax,
        whenTurboJet: turboJet => turboJet.Pmax,
        whenWindturbind: windTurbine => windTurbine.Pmax);
}