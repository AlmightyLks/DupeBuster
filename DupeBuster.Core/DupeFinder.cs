using System.Collections.Concurrent;
using System.IO.Abstractions;
using System.Linq;

namespace DupeBuster.Core;

public class DupeFinder
{
    private readonly IFileSystem _fileSystem;
    private readonly HashSet<IIdentifier> _identifiers;

    public DupeFinder(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _identifiers = new HashSet<IIdentifier>();
    }

    public bool AddIdentifier(IIdentifier identifier)
        => _identifiers.Add(identifier);

    public async Task<IEnumerable<FindingResult>> FindDuplicatesAsync(string rootPath, Intensity intensity, CancellationToken? ct = null)
    {
        ct ??= CancellationToken.None;

        var files = _fileSystem.Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories);

        var tempResult = new ConcurrentBag<(IdentifierType Type, Item Item)>();

        await Parallel.ForEachAsync(_identifiers, ct.Value, async (identifier, ct) =>
        {
            await Parallel.ForEachAsync(files, ct, async (filePath, ct) =>
            {
                var fileInfo = _fileSystem.FileInfo.New(filePath);
                var value = await identifier.CalculateAsync(fileInfo, intensity, ct);
                tempResult.Add((identifier.Type, new Item(fileInfo, value)));
            });
        });

        var result = tempResult
            .GroupBy(x => x.Type)
            .Select(x => new FindingResult(x.Key, x.Select(x => x.Item).ToList()))
            .Where(x => x.Values.Count > 1);

        return result;
    }
}

public enum IdentifierType
{
    FileName,
    FileSize
}

public record struct IdentificationResult(string Value, string Reason);
public record class Item(IFileInfo FileInfo, IdentificationResult Value);
public record class FindingResult(IdentifierType Type, List<Item> Values);
