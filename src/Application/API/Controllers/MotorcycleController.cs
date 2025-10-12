using API.Models.Motorcycle.Request;
using Business.Features.Motorcycles.Commands;
using Business.Features.Motorcycles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.Extensions;

namespace API.Controllers;

[ApiController]
[Route("motos")]
public class MotorcycleController(ILogger<MotorcycleController> logger, ISender sender) : ControllerBase
{
    [HttpGet(Name = "ListMotorcycles")]
    public async Task<IActionResult> ListMotorcycles(
        [FromQuery(Name = "placa")] string? licensePlate,
        CancellationToken cancellationToken)
    {
        var command = new ListMotorcycles.Command(licensePlate);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id}", Name = "GetMotorcycle")]
    public async Task<IActionResult> GetMotorcycle(string id, CancellationToken cancellationToken)
    {
        var command = new GetMotorcycle.Command(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost(Name = "CreateMotorcycle")]
    public async Task<IActionResult> CreateMotorcycle([FromBody] CreateMotorcycleRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? result.ToCreatedAtRouteResult("GetMotorcycle", new { id = result.Value.Identifier })
            : result.ToActionResult();
    }

    [HttpPatch("{id}/placa", Name = "UpdateMotorcycleLicensePlate")]
    public async Task<IActionResult> UpdateMotorcycleLicensePlate(string id,
        [FromBody] UpdateMotorcycleLicensePlateRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{id}", Name = "DeleteMotorcycle")]
    public async Task<IActionResult> DeleteMotorcycle(string id, CancellationToken cancellationToken)
    {
        var command = new DeleteMotorcycle.Command(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }
}