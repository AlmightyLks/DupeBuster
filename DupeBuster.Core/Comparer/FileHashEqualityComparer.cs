using DupeBuster.Core.Util;
using FastHashes;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;

namespace DupeBuster.Core.Comparer;

public class FileHashEqualityComparer : FileComparer
{
    public static readonly FileHashEqualityComparer Default = new FileHashEqualityComparer();

    private static readonly Hash Hash = new FarmHash128();
    private const int BufferSize = 1024 * 1024 * 250;       // 250MiB
    private const int SplicingDistance = BufferSize * 2;    // 500MiB
    private readonly ArrayPool<byte> Pool = ArrayPool<byte>.Create();

    public FileHashEqualityComparer()
    {
        Type = ComparisonType.FileHashCheck;
    }

    public override async Task<ComparisonResult> CompareAsync(IFileInfo[] fileInfos, Intensity intensity, CancellationToken ct)
    {
        Func<IFileInfo, Task<byte[]>> hashCalc = intensity switch
        {
            Intensity.Rough => async (fileInfo) => await CalculateRoughHashAsync(fileInfo, ct),
            Intensity.Precise => async (fileInfo) => await CalculatePreciseHashAsync(fileInfo, ct),
            _ => throw new InvalidOperationException($"Invalid value for ({nameof(intensity)})")
        };

        var dupes = new ConcurrentDictionary<byte[], List<IFileInfo>>(ByteArrayComparer.Default);
        await Parallel.ForEachAsync(fileInfos, new ParallelOptions() { CancellationToken = ct, MaxDegreeOfParallelism = 3 }, async (fileInfo, ct) =>
        {
            var hash = await (fileInfo.Length > SplicingDistance ? hashCalc(fileInfo) : CalculateEntireHashAsync(fileInfo, ct));
            var duplicate = dupes.GetOrAdd(hash, new List<IFileInfo>());
            duplicate.Add(fileInfo);
        });

        var result = new ComparisonResult(Type, dupes.Values.Where(x => x.Count > 1 && x.DistinctBy(y => y.Length).Count() == 1).ToList());
        return result;
    }

    private async Task<byte[]> CalculateEntireHashAsync(IFileInfo fileInfo, CancellationToken ct)
    {
        var summarizeArray = Pool.Rent((int)fileInfo.Length);

        using var readStream = fileInfo.OpenRead();
        await readStream.ReadAsync(summarizeArray, ct);
        var result = Hash.ComputeHash(summarizeArray);

        Pool.Return(summarizeArray);
        return result;
    }
    private async Task<byte[]> CalculateRoughHashAsync(IFileInfo fileInfo, CancellationToken ct)
    {
        var amounts = (int)(fileInfo.Length / SplicingDistance);
        var remainder = (int)(fileInfo.Length % SplicingDistance);
        var summarizeArray = Pool.Rent(BufferSize * amounts);

        await Parallel.ForAsync(0, amounts, new ParallelOptions() { CancellationToken = ct, MaxDegreeOfParallelism = 3 }, async (i, ct) =>
        {
            var from = i * SplicingDistance;
            var size = (i == amounts ? BufferSize + remainder : BufferSize);

            var buffer = Pool.Rent(size);

            using var readStream = fileInfo.OpenRead();
            readStream.Position = from;
            await readStream.ReadAsync(buffer, ct);

            Array.Copy(buffer, 0, summarizeArray, i * BufferSize, size);
            Pool.Return(buffer);
        });

        var result = Hash.ComputeHash(summarizeArray);

        Pool.Return(summarizeArray);
        return result;
    }
    private async Task<byte[]> CalculatePreciseHashAsync(IFileInfo fileInfo, CancellationToken ct)
    {
        var amounts = (int)(fileInfo.Length / SplicingDistance);
        var remainder = (int)(fileInfo.Length % SplicingDistance);
        var summarizeArray = Pool.Rent(BufferSize * amounts);

        await Parallel.ForAsync(0, amounts, new ParallelOptions() { CancellationToken = ct, MaxDegreeOfParallelism = 3 }, async (i, ct) =>
        {
            var from = i * SplicingDistance;
            var size = (i == amounts ? BufferSize + remainder : BufferSize);

            var buffer = Pool.Rent(size);

            using var readStream = fileInfo.OpenRead();
            readStream.Position = from;
            await readStream.ReadAsync(buffer, ct);

            Array.Copy(buffer, 0, summarizeArray, i * BufferSize, size);
            Pool.Return(buffer);
        });

        var result = Hash.ComputeHash(summarizeArray);

        Pool.Return(summarizeArray);
        return result;
    }
}
