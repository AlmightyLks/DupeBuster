using System.Collections.Concurrent;
using System.IO.Abstractions;
using DupeBuster.Core.Comparer;

namespace DupeBuster.Core;

public class DupeFinder
{
    private readonly IFileSystem _fileSystem;
    private readonly HashSet<FileComparer> _identifiers;

    public DupeFinder(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _identifiers = new HashSet<FileComparer>();
    }

    public DupeFinder AddComparer(FileComparer identifier)
    {
        _identifiers.Add(identifier);
        return this;
    }

    public async Task<IEnumerable<ComparisonResult>> FindDuplicatesAsync(string rootPath, Intensity intensity, CancellationToken? ct = null)
    {
        ct ??= CancellationToken.None;

        var fileInfos = _fileSystem.Directory
            .GetFiles(rootPath, "*", SearchOption.AllDirectories)
            .Select(_fileSystem.FileInfo.New)
            .ToArray();

        var result = new ConcurrentBag<ComparisonResult>();

        await Parallel.ForEachAsync(_identifiers, new ParallelOptions() { CancellationToken = ct.Value, MaxDegreeOfParallelism = 3 }, async (identifier, ct) =>
        {
            var value = await identifier.CompareAsync(fileInfos, intensity, ct);
            result.Add(value);
        });

        return result;
    }
}
