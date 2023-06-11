using System.Collections.Concurrent;
using System.IO.Abstractions;

namespace DupeBuster.Core.Comparer;

public class FileSizeEqualityComparer : FileComparer
{
    public static readonly FileSizeEqualityComparer Default = new FileSizeEqualityComparer();

    public FileSizeEqualityComparer()
    {
        Type = ComparisonType.FileSize;
    }

    public override Task<ComparisonResult> CompareAsync(IFileInfo[] fileInfos, Intensity intensity, CancellationToken ct)
    {
        var dupes = new ConcurrentDictionary<long, List<IFileInfo>>();
        Parallel.ForEach(fileInfos, new ParallelOptions() { MaxDegreeOfParallelism = 3 }, (fileInfo) =>
        {
            var duplicate = dupes.GetOrAdd(fileInfo.Length, new List<IFileInfo>());
            duplicate.Add(fileInfo);
        });

        var result = new ComparisonResult(Type, dupes.Values.Where(x => x.Count > 1).ToList());
        return Task.FromResult(result);
    }
}
