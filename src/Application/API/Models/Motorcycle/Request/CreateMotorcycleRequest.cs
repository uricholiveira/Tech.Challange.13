using System.Text.Json.Serialization;
using Business.Features.Motorcycles.Commands;

namespace API.Models.Motorcycle.Request;

public record CreateMotorcycleRequest(
    [property: JsonPropertyName("identificador")]
    string Identifier,
    [property: JsonPropertyName("ano")] int Year,
    [property: JsonPropertyName("modelo")] string Model,
    [property: JsonPropertyName("placa")] string LicensePlate
)
{
    public CreateMotorcycle.Command ToCommand()
    {
        return new CreateMotorcycle.Command(Identifier, Year, Model, LicensePlate);
    }
}