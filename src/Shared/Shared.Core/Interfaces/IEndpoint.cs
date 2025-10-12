using Microsoft.AspNetCore.Routing;

namespace Shared.Core.Interfaces;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}