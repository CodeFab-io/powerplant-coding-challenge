using FluentAssertions;
using powerplant_coding_challenge;
using powerplant_coding_challenge.Model;
using System.Collections.Immutable;

namespace Tests;

public class CalcUtilsTests
{
    [Theory]
    [MemberData(nameof(AdjustPMaxOfWindTurbines_Tests_Data))]
    public void AdjustPMaxOfWindTurbines_Tests(Powerplant[] powerplants, decimal windAvailability, Powerplant[] expected) =>
        CalcUtils.AdjustPMaxOfWindTurbines(
            powerplants.ToImmutableList(),
            windAvailability: new Fuel.Wind(AverageWind: windAvailability))
        .Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());

    [Theory]
    [MemberData(nameof(SortByMeritOrder_Tests_Data))]
    public void SortByMeritOrder_Tests(Powerplant[] powerplants, Fuel.Gas gasPrice, Fuel.Kerosine kerosinePrice, Powerplant[] expected) =>
        CalcUtils.SortByMeritOrder(
            powerplants.ToImmutableList(),
            gasPrice,
            kerosinePrice)
        .Should().BeEquivalentTo(expected);

    [Theory]
    [MemberData(nameof(ComputeProduction_Tests_Data))]
    public void ComputeProduction_Tests(Load load, Powerplant[] powerplants, PowerplantProductionResult expected) =>
        CalcUtils.ComputeNecessaryProduction(
            powerplants.ToImmutableList(),
            load)
        .Should().BeEquivalentTo(expected);

    public static IEnumerable<object[]> AdjustPMaxOfWindTurbines_Tests_Data =>
        new List<object[]>
        {
            // Empty arrays don't break it
            new object[] { Array.Empty<Powerplant>(), 0M, Array.Empty<Powerplant>() },
            new object[] { Array.Empty<Powerplant>(), 1M, Array.Empty<Powerplant>() },
            
            // Non-wind powerplants aren't modified
            new object[] { new Powerplant[] { GasFiredBig1, }, 0M, new Powerplant[] { GasFiredBig1, } },
            new object[] { new Powerplant[] { GasFiredBig1, }, 1M, new Powerplant[] { GasFiredBig1, } },
            new object[] { new Powerplant[] { TJ1, }, 1M, new Powerplant[] { TJ1, } },

            // Zero wind reduces Pmax to zero
            new object[] { new Powerplant[] { WindPark1, }, 0M, new Powerplant[] { WindPark1 with { Pmax = 0 } }, },
            // Half wind means half Pmax
            new object[] { new Powerplant[] { WindPark1, }, 0.5M, new Powerplant[] { WindPark1 with { Pmax = WindPark1.Pmax / 2 } }, },
            // Full wind has no impact on Pmax
            new object[] { new Powerplant[] { WindPark1, }, 1M, new Powerplant[] { WindPark1, } },
            // The order of the powerplants is preserved
            new object[] { new Powerplant[] { GasFiredBig1, WindPark1, }, 1M, new Powerplant[] { GasFiredBig1, WindPark1, } },
            new object[] { new Powerplant[] { WindPark1, GasFiredBig1, }, 1M, new Powerplant[] { WindPark1, GasFiredBig1, } },
        };

    public static IEnumerable<object[]> SortByMeritOrder_Tests_Data =>
        new List<object[]> 
        {
            // Empty arrays don't break it
            new object[] { Array.Empty<Powerplant>(), new Fuel.Gas(13.4M), new Fuel.Kerosine(50.8M), Array.Empty<Powerplant>() },
            
            // Most efficient powerplants should always come first
            new object[] { new Powerplant[] { GasFiredBig1, WindPark1, }, new Fuel.Gas(50M), new Fuel.Kerosine(50M), 
                           new Powerplant[] { WindPark1, GasFiredBig1, } },
            
            // When kerosine is much more expensive than gas, the turbojet plants get more merit
            new object[] { new Powerplant[] { GasFiredBig1, TJ1, WindPark1, }, new Fuel.Gas(1000M), new Fuel.Kerosine(0M), 
                           new Powerplant[] { WindPark1, TJ1, GasFiredBig1, } },
            
            // When gas is much more expensive than kerosine, the gas fired plants get more merit
            new object[] { new Powerplant[] { GasFiredBig1, TJ1, WindPark1, }, new Fuel.Gas(0M), new Fuel.Kerosine(1000M), 
                           new Powerplant[] { WindPark1, GasFiredBig1, TJ1, } },

            // More efficient powerplants get more merit
            new object[] { new Powerplant[] { GasFiredBig1, MoreEfficientGasFiredBig1, TJ1, MoreEfficientTJ1, WindPark1, }, new Fuel.Gas(0M), new Fuel.Kerosine(0M), 
                           new Powerplant[] { WindPark1, MoreEfficientGasFiredBig1, GasFiredBig1, MoreEfficientTJ1, TJ1, } },

            // Check the order of example3 (response3.json)
            new object[] { new Powerplant[] { GasFiredBig1, GasFiredBig2, GasFiredSomewhatSmaller, TJ1, WindPark1, WindPark2, }, new Fuel.Gas(13.4M), new Fuel.Kerosine(50.8M),
                           new Powerplant[] { WindPark1, WindPark2, GasFiredBig1, GasFiredBig2, GasFiredSomewhatSmaller, TJ1, } },
        };

    public static IEnumerable<object[]> ComputeProduction_Tests_Data =>
        new List<object[]> 
        {
            // Empty arrays don't break it
            new object[] { 
                new Load(0), 
                Array.Empty<Powerplant>(), 
                new PowerplantProductionResult(
                    PowerplantProductions: ImmutableList<PowerplantProduction>.Empty, 
                    RemainingLoad: new Load(0M)) 
            },
            
            // Single powerplant with no pmin and sufficient pmax is put to use
            new object[] {
                new Load(100),
                new Powerplant[] { WindPark1 },
                new PowerplantProductionResult(
                    PowerplantProductions: new PowerplantProduction[] { 
                        new(Name: WindPark1.Name, Production: 100M), 
                    }.ToImmutableList(),
                    RemainingLoad: new Load(0M))
            },
            
            // Single powerplant with pmin > load has production = pmin
            new object[] {
                new Load(10),
                new Powerplant[] { WindPark1 with { Pmin = 100M } },
                new PowerplantProductionResult(
                    PowerplantProductions: new PowerplantProduction[] { 
                        new(Name: WindPark1.Name, Production: 100M), 
                    }.ToImmutableList(),
                    RemainingLoad: new Load(-90M))
            },
            
            // Single powerplant with pmax < load has production = pmax
            new object[] {
                new Load(150),
                new Powerplant[] { WindPark1 with { Pmax = 100M } },
                new PowerplantProductionResult(
                    PowerplantProductions: new PowerplantProduction[] { 
                        new(Name: WindPark1.Name, Production: 100M), 
                    }.ToImmutableList(),
                    RemainingLoad: new Load(50M))
            },
            
            // Single powerplant with pmin > 0, but load = 0, should not start
            new object[] {
                new Load(0),
                new Powerplant[] { WindPark1 with { Pmin = 100M } },
                new PowerplantProductionResult(
                    PowerplantProductions: new PowerplantProduction[] { 
                        new(Name: WindPark1.Name, Production: 0M), 
                    }.ToImmutableList(),
                    RemainingLoad: new Load(0M))
            },

            // Two powerplants, first one has capacity, should take all the load
            new object[] {
                new Load(50),
                new Powerplant[] {
                    WindPark1,
                    WindPark2,
                },
                new PowerplantProductionResult(
                    PowerplantProductions: new PowerplantProduction[] {
                        new(Name: WindPark1.Name, Production: 50M),
                        new(Name: WindPark2.Name, Production: 0M),
                    }.ToImmutableList(), 
                    RemainingLoad: new Load(0M)) 
            },

            // Two powerplants, load higher that the first's pmax, the load should spill over to the second
            new object[] {
                new Load(150),
                new Powerplant[] {
                    WindPark1 with { Pmax = 100M },
                    WindPark2 with { Pmax = 100M },
                },
                new PowerplantProductionResult(
                    PowerplantProductions: new PowerplantProduction[] {
                        new(Name: WindPark1.Name, Production: 100M),
                        new(Name: WindPark2.Name, Production: 50M),
                    }.ToImmutableList(), 
                    RemainingLoad: new Load(0M))
            },

            // Two powerplants, the first one doesn't have enough pmax, and the second one has a pmin higher than the load
            // The first one should not start, because the second one will be able to cope
            new object[] {
                new Load(150),
                new Powerplant[] {
                    WindPark1 with { Pmax = 75M },
                    WindPark2 with { Pmin = 100M, Pmax = 200M },
                },
                new PowerplantProductionResult(
                    PowerplantProductions: new PowerplantProduction[] {
                    new(Name: WindPark1.Name, Production: 50M),
                    new(Name: WindPark2.Name, Production: 100M),
                }.ToImmutableList(), 
                RemainingLoad: new Load(0M))
            },

            // The example 3 
            new object[] {
                new Load(910),
                new Powerplant[] {
                    WindPark1 with { Pmax = 0.6M * WindPark1.Pmax },
                    WindPark2 with { Pmax = 0.6M * WindPark2.Pmax },
                    GasFiredBig1,
                    GasFiredBig2,
                    GasFiredSomewhatSmaller,
                    TJ1,
                },
                new PowerplantProductionResult(
                    PowerplantProductions: new PowerplantProduction[] {
                        new(Name: WindPark1.Name, Production: 90.0M),
                        new(Name: WindPark2.Name, Production: 21.6M),
                        new(Name: GasFiredBig1.Name, Production: 460.0M),
                        new(Name: GasFiredBig2.Name, Production: 338.4M),
                        new(Name: GasFiredSomewhatSmaller.Name, Production: 0.0M),
                        new(Name: TJ1.Name, Production: 0.0M),
                    }.ToImmutableList(), 
                    RemainingLoad: new Load(0M))
            },
        };

    internal static Powerplant.GasFired GasFiredBig1 = new(Name: "gasfiredbig1", Efficiency: 0.53M, Pmin: 100, Pmax: 460);
    internal static Powerplant.GasFired GasFiredBig2 = GasFiredBig1 with { Name = "gasfiredbig2" };
    internal static Powerplant.GasFired GasFiredSomewhatSmaller = new(Name: "gasfiredsomewhatsmaller", Efficiency: 0.37M, Pmin: 40, Pmax: 210);

    internal static Powerplant.WindTurbine WindPark1 = new(Name: "windpark1", Efficiency: 1, Pmin: 0, Pmax: 150);
    internal static Powerplant.WindTurbine WindPark2 = new(Name: "windpark2", Efficiency: 1, Pmin: 0, Pmax: 36);
    
    internal static Powerplant.TurboJet TJ1 = new(Name: "tj1", Efficiency: 0.3M, Pmin: 0, Pmax: 16);

    internal static Powerplant.GasFired MoreEfficientGasFiredBig1 = GasFiredBig1 with { Efficiency = 0.75M };
    internal static Powerplant.TurboJet MoreEfficientTJ1 = TJ1 with { Efficiency = 0.5M };
}