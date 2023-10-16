using System.Collections.Immutable;

namespace powerplant_coding_challenge.Model;

public sealed record PowerplantProductionResult(
    ImmutableList<PowerplantProduction> PowerplantProductions,
    Load RemainingLoad);