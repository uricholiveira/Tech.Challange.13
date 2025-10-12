using System.Text.Json.Serialization;
using Business.Features.Riders.Commands;

namespace API.Models.Rider.Request;

public record CreateRiderRequest(
    [property: JsonPropertyName("identificador")]
    string Identifier,
    [property: JsonPropertyName("nome")] string Name,
    [property: JsonPropertyName("cnpj")] string Cnpj,
    [property: JsonPropertyName("data_nascimento")]
    DateOnly BirthDate,
    [property: JsonPropertyName("numero_cnh")]
    string Cnh,
    [property: JsonPropertyName("tipo_cnh")]
    string CnhType,
    [property: JsonPropertyName("imagem_cnh")]
    string CnhImage
)
{
    public CreateRider.Command ToCommand()
    {
        return new CreateRider.Command(Identifier, Name, Cnpj, BirthDate, Cnh, CnhType, CnhImage);
    }
}