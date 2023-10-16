namespace powerplant_coding_challenge.Model;

public abstract record Fuel
{
    public sealed record Gas(decimal EurosPerMWh) : Fuel;

    public sealed record Kerosine(decimal EurosPerMWh) : Fuel;

    // Percentage of wind. If there is on average 25% wind during an hour, a wind-turbine
    // with a Pmax of 4 MW will generate 1MWh of energy
    public sealed record Wind(decimal AverageWind) : Fuel;
}
