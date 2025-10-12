using API.Models.Rider.Request;
using Business.Features.Riders.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.Extensions;

namespace API.Controllers;

[ApiController]
[Route("entregadores")]
public class RiderController(ILogger<RiderController> logger, ISender sender) : ControllerBase
{
    [HttpGet("{id}", Name = "GetRider")]
    public async Task<IActionResult> GetRider(string id, CancellationToken cancellationToken)
    {
        var command = new GetRider.Command(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost(Name = "CreateRider")]
    public async Task<IActionResult> CreateRider([FromBody] CreateRiderRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? result.ToCreatedAtRouteResult("GetRider", new { id = result.Value.Identifier })
            : result.ToActionResult();
    }

    [HttpPost("{id}/cnh", Name = "UploadRiderCnhImage")]
    public async Task<IActionResult> UploadRiderCnhImage(string id, [FromBody] UploadRiderCnhImageRequest imageRequest,
        CancellationToken cancellationToken)
    {
        var command = imageRequest.ToCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }
}