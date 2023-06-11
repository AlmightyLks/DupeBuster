using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using DupeBuster.Core;
using DupeBuster.Core.Comparer;
using DupeBuster.Misc.Benchmarks;
using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace DupeBuster.Benchmarks;

[MemoryDiagnoser]
[InProcess]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class MyBenchmarks
{
    private const string TestDirectory = @"C:\";

    private readonly TestDataSets _testDataSets = new TestDataSets();
    private readonly FileNameEqualityComparer FileNameEqualityComparer = new FileNameEqualityComparer();
    private readonly FileSizeEqualityComparer FileSizeComparer = new FileSizeEqualityComparer();
    private readonly FileHashEqualityComparer FileHashEqualityComparer = new FileHashEqualityComparer();

    private DupeFinder FileNameEquality_DupeFinder = null!;
    private DupeFinder FileSize_DupeFinder = null!;
    private DupeFinder FileHashEquality_DupeFinder = null!;

    private IFileSystem _fewSmallFileSystem = null!;


    [Params(Intensity.Rough, Intensity.Precise)]
    public Intensity Intensity { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _testDataSets.Init(TestDirectory);

        _fewSmallFileSystem = new MockFileSystem(_testDataSets.FewSmallFiles);

        FileNameEquality_DupeFinder = new DupeFinder(_fewSmallFileSystem).AddComparer(FileNameEqualityComparer);
        FileSize_DupeFinder = new DupeFinder(_fewSmallFileSystem).AddComparer(FileSizeComparer);
        FileHashEquality_DupeFinder = new DupeFinder(_fewSmallFileSystem).AddComparer(FileHashEqualityComparer);
    }

    [Benchmark]
    public async Task FindDupes_FileName()
    {
        var result = await FileNameEquality_DupeFinder.FindDuplicatesAsync(TestDirectory, Intensity);
    }
    [Benchmark]
    public async Task FindDupes_FileSize()
    {
        var result = await FileSize_DupeFinder.FindDuplicatesAsync(TestDirectory, Intensity);
    }
    [Benchmark]
    public async Task FindDupes_FileHash()
    {
        var result = await FileHashEquality_DupeFinder.FindDuplicatesAsync(TestDirectory, Intensity);
    }
}
