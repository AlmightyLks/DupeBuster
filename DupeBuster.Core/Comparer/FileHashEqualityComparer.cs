using DupeBuster.Core.Util;
using Serilog;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;

namespace DupeBuster.Core.Comparer;

public class FileHashEqualityComparer : FileComparer
{
    private static ILogger? _logger;
    public static ILogger Logger => _logger ??= Log.ForContext<FileHashEqualityComparer>();

    public static readonly FileHashEqualityComparer Default = new FileHashEqualityComparer();

    private const int SpliceThreshold = 1024 * 1024 * 250;    // 250MiB
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
        await Parallel.ForEachAsync(fileInfos, new ParallelOptions() { CancellationToken = ct, MaxDegreeOfParallelism = 1 }, async (fileInfo, ct) =>
        {
            var hash = await (fileInfo.Length > SpliceThreshold ? hashCalc(fileInfo) : CalculateEntireHashAsync(fileInfo, ct));
            var duplicate = dupes.GetOrAdd(hash, new List<IFileInfo>());
            duplicate.Add(fileInfo);
            GC.Collect();
        });

        var result = new ComparisonResult(Type, dupes.Values.Where(x => x.Count > 1 && x.DistinctBy(y => y.Length).Count() == 1).ToList());
        return result;
    }

    private async Task<byte[]> CalculateEntireHashAsync(IFileInfo fileInfo, CancellationToken ct)
    {
        using var readStream = fileInfo.OpenRead();
        var result = await MD5.HashDataAsync(readStream, ct);

        return result;
    }

    private async Task<byte[]> CalculateRoughHashAsync(IFileInfo fileInfo, CancellationToken ct)
    {
        var size = (int)Math.Min(Int32.MaxValue, fileInfo.Length / 3);
        var smallArray = Pool.Rent(size);
        var summarizeArray = Pool.Rent(MD5.HashSizeInBytes * 2);

        using var readStream = fileInfo.OpenRead();

        readStream.Position = 0;
        await readStream.ReadAsync(smallArray, ct);
        var firstHash = MD5.HashData(smallArray);

        readStream.Position = (int)(fileInfo.Length - size);
        await readStream.ReadAsync(smallArray, ct);
        var secondHash = MD5.HashData(smallArray);

        Array.Copy(firstHash, 0, summarizeArray, 0, firstHash.Length);
        Array.Copy(secondHash, 0, summarizeArray, firstHash.Length, secondHash.Length);

        var result = MD5.HashData(summarizeArray);

        Pool.Return(smallArray);
        Pool.Return(summarizeArray);
        return result;
    }
    private async Task<byte[]> CalculatePreciseHashAsync(IFileInfo fileInfo, CancellationToken ct)
    {
        var totalTime = Stopwatch.GetTimestamp();

        var amounts = (int)(fileInfo.Length / SpliceThreshold);
        var summarizeArray = Pool.Rent(MD5.HashSizeInBytes * amounts);
        using var readStream = fileInfo.OpenRead();
        byte[] tempResult;
        byte[] buffer;

        for (int i = 0; i < amounts; i++)
        {
            var iterationTime = Stopwatch.GetTimestamp();
            var from = i * SpliceThreshold;

            buffer = Pool.Rent(SpliceThreshold);

            readStream.Position = from;
            await readStream.ReadAsync(buffer, ct);
            tempResult = MD5.HashData(buffer);

            Array.Copy(tempResult, 0, summarizeArray, i * MD5.HashSizeInBytes, MD5.HashSizeInBytes);
            Pool.Return(buffer);

            var elapsed = Stopwatch.GetElapsedTime(iterationTime);
            Logger.Debug("Iteration took {ElapsedTime:ss\\.fff}", elapsed);
        }

        buffer = Pool.Rent(SpliceThreshold);
        readStream.Position = (amounts - 1) * SpliceThreshold;
        await readStream.ReadAsync(buffer, ct);
        tempResult = MD5.HashData(buffer);

        Array.Copy(tempResult, 0, summarizeArray, (amounts - 1) * MD5.HashSizeInBytes, MD5.HashSizeInBytes);

        var result = MD5.HashData(summarizeArray);

        Pool.Return(buffer);
        Pool.Return(summarizeArray);

        var totalElapsed = Stopwatch.GetElapsedTime(totalTime);
        Logger.Debug("Complete File Hashing took {ElapsedTime:ss\\.fff}", totalElapsed);

        return result;
    }
}
