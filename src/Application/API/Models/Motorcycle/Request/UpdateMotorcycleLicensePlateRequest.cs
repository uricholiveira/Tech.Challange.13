using System.Text.Json.Serialization;
using Business.Features.Motorcycles.Commands;

namespace API.Models.Motorcycle.Request;

public record UpdateMotorcycleLicensePlateRequest(
    [property: JsonPropertyName("placa")] string LicensePlate
)
{
    public UpdateMotorcycleLicensePlate.Command ToCommand(string identifier)
    {
        return new UpdateMotorcycleLicensePlate.Command(identifier, LicensePlate);
    }
}