using System.Text.Json.Serialization;
using Business.Features.Riders.Commands;

namespace API.Models.Rider.Request;

public record UploadRiderCnhImageRequest(
    [property: JsonPropertyName("imagem_cnh")]
    string CnhImage
)
{
    public UpdateRiderCnhImage.Command ToCommand(string identifier)
    {
        return new UpdateRiderCnhImage.Command(identifier, CnhImage);
    }
}