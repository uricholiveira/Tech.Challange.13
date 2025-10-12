namespace Shared.Core.Models.Common;

public class ObjectMetadata
{
    public string Key { get; set; }
    public long Size { get; set; }
    public DateTime? LastModified { get; set; }
    public string ETag { get; set; }
    public string ContentType { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}