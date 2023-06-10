using FastHashes;
using System.IO.Abstractions;

namespace DupeBuster.Core;

public class FileNameIdentifier : IIdentifier
{
    public IdentifierType Type
        => IdentifierType.FileName;

    public Task<IdentificationResult> CalculateAsync(IFileInfo fileInfo, Intensity intensity, CancellationToken ct)
    {
        var value = intensity switch
        {
            Intensity.Rough => fileInfo.Name.ToLower(),
            Intensity.Intense => fileInfo.Name,
            _ => throw new InvalidOperationException($"Invalid value for ({nameof(intensity)})")
        };

        var result = new IdentificationResult(value, "Identical Filename");
        return Task.FromResult(result);
    }
}

public class FileSizeIdentifier : IIdentifier
{
    public IdentifierType Type
        => IdentifierType.FileSize;

    public Task<IdentificationResult> CalculateAsync(IFileInfo fileInfo, Intensity intensity, CancellationToken ct)
    {
        var value = intensity switch
        {
            Intensity.Rough => fileInfo.Length,
            Intensity.Intense => fileInfo.Length,
            _ => throw new InvalidOperationException($"Invalid value for ({nameof(intensity)})")
        };

        var result = new IdentificationResult(value.ToString(), "Identical Filename");
        return Task.FromResult(result);
    }
}
