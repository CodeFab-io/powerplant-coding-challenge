using powerplant_coding_challenge.Model;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// For the purpose of this Demo, we'll enable Swagger UI also in ""production""
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/productionplan", (ProductionPlanPayloadDTO payload, HttpContext http) =>
{
    var computedProduction =
        powerplant_coding_challenge.CalcUtils.ComputeProduction(
            load: new Load(payload.Load),
            gasPrice: new Fuel.Gas(payload.Fuels.GasPrice),
            kerosinePrice: new Fuel.Kerosine(payload.Fuels.KerosinePrice),
            windAvailability: new Fuel.Wind(payload.Fuels.WindPercentage / 100M),
            powerplants: payload.Powerplants.Select(MapToModel).ToImmutableList());

    http.Response.Headers["unsatisfied-load"] = computedProduction.RemainingLoad.LoadValue.ToString();

    return computedProduction.PowerplantProductions.Select(MapToResponseDTO);

    // Helper functions to map from the HTTP payload into the domain Model

    static Powerplant MapToModel(ProductionPlanPayloadDTO.PowerplantDTO powerplantDTO) => powerplantDTO.Type switch { 
        "gasfired" => new Powerplant.GasFired(Name: powerplantDTO.Name, Efficiency: powerplantDTO.Efficiency, Pmin: powerplantDTO.Pmin, Pmax: powerplantDTO.Pmax),
        "turbojet" => new Powerplant.TurboJet(Name: powerplantDTO.Name, Efficiency: powerplantDTO.Efficiency, Pmin: powerplantDTO.Pmin, Pmax: powerplantDTO.Pmax),
        "windturbine" => new Powerplant.WindTurbine(Name: powerplantDTO.Name, Efficiency: powerplantDTO.Efficiency, Pmin: powerplantDTO.Pmin, Pmax: powerplantDTO.Pmax),
    };

    static ProductionPlanResponseDTO MapToResponseDTO(PowerplantProduction powerplantProduction) =>
        new() {
            Name = powerplantProduction.Name, 
            Production = powerplantProduction.Production 
        };
})
.WithName("ProductionPlan")
.WithOpenApi();

app.Run();

sealed class ProductionPlanPayloadDTO
{
    [JsonPropertyName("load")]
    public decimal Load { get; init; }

    [JsonPropertyName("fuels")]
    public required FuelsDTO Fuels { get; init; }

    [JsonPropertyName("powerplants")]
    public required PowerplantDTO[] Powerplants { get; init; }

    public sealed class FuelsDTO 
    {
        [JsonPropertyName("gas(euro/MWh)")]
        public decimal GasPrice { get; init; }

        [JsonPropertyName("kerosine(euro/MWh)")]
        public decimal KerosinePrice { get; init; }

        [JsonPropertyName("wind(%)")]
        public decimal WindPercentage { get; init; }
    }

    public sealed class PowerplantDTO 
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("efficiency")]
        public decimal Efficiency { get; init; }

        [JsonPropertyName("pmin")]
        public decimal Pmin { get; init; }

        [JsonPropertyName("pmax")]
        public decimal Pmax { get; init; }
    }
}

sealed class ProductionPlanResponseDTO 
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("p")]
    public required decimal Production { get; init; }
}

