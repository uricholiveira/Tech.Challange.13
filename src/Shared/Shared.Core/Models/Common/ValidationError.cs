namespace Shared.Core.Models.Common;

public sealed record ValidationError(string Message, List<ValidationErrorContent> Errors);

public sealed record ValidationErrorContent(string Property, string[] Errors);