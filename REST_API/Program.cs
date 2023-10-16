using powerplant_coding_challenge.Model;
using REST_API.Dtos;
using System.Collections.Immutable;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// For the purpose of this Demo, we'll enable Swagger UI also in ""production""
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/productionplan", (ProductionPlanPayloadDto payload, HttpContext http) =>
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

    static Powerplant MapToModel(ProductionPlanPayloadDto.PowerplantDto powerplantDTO) => powerplantDTO.Type switch { 
        "gasfired" => new Powerplant.GasFired(Name: powerplantDTO.Name, Efficiency: powerplantDTO.Efficiency, Pmin: powerplantDTO.Pmin, Pmax: powerplantDTO.Pmax),
        "turbojet" => new Powerplant.TurboJet(Name: powerplantDTO.Name, Efficiency: powerplantDTO.Efficiency, Pmin: powerplantDTO.Pmin, Pmax: powerplantDTO.Pmax),
        "windturbine" => new Powerplant.WindTurbine(Name: powerplantDTO.Name, Efficiency: powerplantDTO.Efficiency, Pmin: powerplantDTO.Pmin, Pmax: powerplantDTO.Pmax),
    };

    static ProductionPlanResponseDto MapToResponseDTO(PowerplantProduction powerplantProduction) =>
        new(Name: powerplantProduction.Name, 
            Production: powerplantProduction.Production);
})
.WithName("ProductionPlan")
.WithOpenApi();

app.Run();
