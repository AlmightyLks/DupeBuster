using System.Collections.Concurrent;
using System.IO.Abstractions;

namespace DupeBuster.Core.Comparer;

public class FileNameEqualityComparer : FileComparer
{
    public static readonly FileNameEqualityComparer Default = new FileNameEqualityComparer();

    public FileNameEqualityComparer()
    {
        Type = ComparisonType.FileNameEquality;
    }

    public override Task<ComparisonResult> CompareAsync(IFileInfo[] fileInfos, Intensity intensity, CancellationToken ct)
    {
        Func<string, string> modifier = intensity switch
        {
            Intensity.Rough => (x) => x.ToLower(),
            Intensity.Precise => (x) => x,
            _ => throw new InvalidOperationException($"Invalid value for ({nameof(intensity)})")
        };

        var dupes = new ConcurrentDictionary<string, List<IFileInfo>>();
        Parallel.ForEach(fileInfos, new ParallelOptions() { CancellationToken = ct, MaxDegreeOfParallelism = 3 }, (fileInfo) =>
        {
            var value = modifier(fileInfo.Name);
            var duplicate = dupes.GetOrAdd(value, new List<IFileInfo>());
            duplicate.Add(fileInfo);
        });

        var result = new ComparisonResult(Type, dupes.Values.Where(x => x.Count > 1).ToList());
        return Task.FromResult(result);
    }
}
