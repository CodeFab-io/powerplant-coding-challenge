using System.Text.Json.Serialization;

namespace REST_API.Dtos;

sealed record ProductionPlanPayloadDto(
    [property: JsonPropertyName("load")] decimal Load,
    [property: JsonPropertyName("fuels")] ProductionPlanPayloadDto.FuelsDto Fuels,
    [property: JsonPropertyName("powerplants")] ProductionPlanPayloadDto.PowerplantDto[] Powerplants)
{
    public sealed record FuelsDto(
        [property: JsonPropertyName("gas(euro/MWh)")] decimal GasPrice,
        [property: JsonPropertyName("kerosine(euro/MWh)")] decimal KerosinePrice,
        [property: JsonPropertyName("wind(%)")] decimal WindPercentage);

    public sealed record PowerplantDto(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("efficiency")] decimal Efficiency,
        [property: JsonPropertyName("pmin")] decimal Pmin,
        [property: JsonPropertyName("pmax")] decimal Pmax);
}