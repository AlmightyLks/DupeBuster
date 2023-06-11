using System.IO.Abstractions;

namespace DupeBuster.Core.Comparer;

public abstract class FileComparer
{
    public ComparisonType Type { get; init; }

    public abstract Task<ComparisonResult> CompareAsync(IFileInfo[] fileInfos, Intensity intensity, CancellationToken ct);
}

public record class ComparisonResult(ComparisonType Type, List<List<IFileInfo>> DuplicateSets);
