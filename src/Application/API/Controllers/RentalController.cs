using API.Models.Rental.Request;
using Business.Features.Rentals.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.Extensions;

namespace API.Controllers;

[ApiController]
[Route("locacao")]
public class Rental(ILogger<Rental> logger, ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetRental")]
    public async Task<IActionResult> GetRental(Guid id, CancellationToken cancellationToken)
    {
        var command = new GetRental.Command(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost(Name = "CreateRental")]
    public async Task<IActionResult> CreateRental([FromBody] CreateRentalRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? result.ToCreatedAtRouteResult("GetRental", new { id = result.Value.Id })
            : result.ToActionResult();
    }

    [HttpPost("{id:guid}/devolucao", Name = "UpdateRentalReturnDate")]
    public async Task<IActionResult> UpdateRentalReturnDate(Guid id,
        [FromBody] UpdateRentalReturnDateRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }
}