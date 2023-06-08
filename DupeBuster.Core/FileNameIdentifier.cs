using System.IO.Abstractions;

namespace DupeBuster.Core;

public class FileNameIdentifier : IIdentifier
{
    public Task<IdentificationResult> CalculateAsync(IFileInfo fileInfo, Intensity intensity)
    {
        var value = intensity switch
        {
            Intensity.Rough => fileInfo.Name.ToLower(),
            Intensity.Intense => fileInfo.Name,
            _ => fileInfo.Name
        };

        var result = new IdentificationResult(value, "Identical Filename");
        return Task.FromResult(result);
    }
}
