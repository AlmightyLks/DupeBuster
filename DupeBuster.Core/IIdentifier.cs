using System.IO.Abstractions;

namespace DupeBuster.Core;

public interface IIdentifier
{
    Task<IdentificationResult> CalculateAsync(IFileInfo fileInfo, Intensity intensity);
}
