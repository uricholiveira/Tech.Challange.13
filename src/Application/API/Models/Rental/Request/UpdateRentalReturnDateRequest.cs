using System.Text.Json.Serialization;
using Business.Features.Rentals.Commands;

namespace API.Models.Rental.Request;

public record UpdateRentalReturnDateRequest(
    [property: JsonPropertyName("data_devolucao")]
    DateOnly ReturnDate
)
{
    public UpdateRentalReturnDate.Command ToCommand(Guid id)
    {
        return new UpdateRentalReturnDate.Command(id, ReturnDate);
    }
}