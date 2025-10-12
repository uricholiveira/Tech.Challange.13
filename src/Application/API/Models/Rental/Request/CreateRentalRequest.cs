using System.Text.Json.Serialization;
using Business.Features.Rentals.Commands;

namespace API.Models.Rental.Request;

public record CreateRentalRequest(
    [property: JsonPropertyName("entregador_id")]
    string RiderIdentifier,
    [property: JsonPropertyName("moto_id")]
    string MotorcycleIdentifier,
    [property: JsonPropertyName("data_inicio")]
    DateOnly StartDate,
    [property: JsonPropertyName("data_termino")]
    DateOnly EndDate,
    [property: JsonPropertyName("data_previsao_termino")]
    DateOnly ExpectedEndDate,
    [property: JsonPropertyName("plano")] int RentalPlanDays
)
{
    public CreateRental.Command ToCommand()
    {
        return new CreateRental.Command(
            RiderIdentifier,
            MotorcycleIdentifier,
            StartDate,
            EndDate,
            ExpectedEndDate,
            RentalPlanDays
        );
    }
}