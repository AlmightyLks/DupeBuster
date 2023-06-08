using System.Collections.Concurrent;
using System.IO.Abstractions;

namespace DupeBuster.Core;

public class DupeFinder
{
    private readonly IFileSystem _fileSystem;

    public DupeFinder(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task<IEnumerable<IGrouping<string, Item>>> FindDuplicatesAsync(string rootPath, IIdentifier identifier, Intensity intensity)
    {
        var files = _fileSystem.Directory
            .GetFiles(rootPath, "*", SearchOption.AllDirectories)
            .Select(_fileSystem.FileInfo.New)
            .ToList();

        var result = new ConcurrentBag<Item>();

        await Parallel.ForEachAsync(files, async (fileInfo, _) =>
        {
            var value = await identifier.CalculateAsync(fileInfo, intensity);
            result.Add(new Item(fileInfo, value));
        });

        return result.GroupBy(x => x.Value.Value);
    }
}
