using System.IO.Abstractions;

namespace DupeBuster.Core;

public interface IIdentifier
{
    IdentifierType Type { get; }
Task<IdentificationResult> CalculateAsync(IFileInfo fileInfo, Intensity intensity, CancellationToken ct);
}
