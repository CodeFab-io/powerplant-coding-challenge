using System.Text.Json.Serialization;

namespace REST_API.Dtos;

sealed record ProductionPlanResponseDto(
    [property: JsonPropertyName("name")] string Name, 
    [property: JsonPropertyName("p")] decimal Production);
