using DupeBuster.Core;
using DupeBuster.Core.Comparer;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace DupeBuster.Tests;

public class FileHashEqualityComparerTests : TestBase
{
    private readonly FileHashEqualityComparer _comparer = FileHashEqualityComparer.Default;
    private readonly Random _random = new Random(1234567890);

    [Theory]
    [InlineData(Intensity.Rough)]
    [InlineData(Intensity.Precise)]
    public async Task Can_Find_Duplicates_With_Duplicates(Intensity intensity)
    {
        const int Size = 1024 * 1024 * 50; // 50 MiB

        var bytes = new byte[Size];
        _random.NextBytes(bytes);

        string rootPath = GetMethodName();
        using var fileEnsurer = new FileEnsurer();
        fileEnsurer.Setup(rootPath,
            (Path.Combine("File1.txt"), bytes),
            (Path.Combine("File2.txt"), new byte[] { 1, 1, 1 }),
            (Path.Combine("Test", "File1.txt"), bytes),
            (Path.Combine("Some Where Else", "File2.txt"), new byte[] { 2, 2, 2 }),
            (Path.Combine("Test", "Some Random Name.txt"), bytes)
            );
        var fileSystem = new FileSystem();

        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(rootPath, intensity)).ToList();
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Single(comparisonResult.DuplicateSets);

        var set = comparisonResult.DuplicateSets.First();
        Assert.Equal(3, set.Count);
    }

    [Theory]
    [InlineData(Intensity.Rough)]
    [InlineData(Intensity.Precise)]
    public async Task Cannot_Find_Duplicates_Without_Duplicates(Intensity intensity)
    {
        const int Size = 1024 * 1024 * 50; // 50 MiB

        var arrayOne = new byte[Size];
        var arrayTwo = new byte[Size];
        for (int i = 0; i < arrayOne.Length; i++)
            arrayOne[i] = 1;
        for (int i = 0; i < arrayTwo.Length; i++)
            arrayTwo[i] = 2;

        string rootPath = GetMethodName();
        using var fileEnsurer = new FileEnsurer();
        fileEnsurer.Setup(rootPath,
            (Path.Combine("File1.txt"), arrayOne),
            (Path.Combine("File2.txt"), new byte[] { 1, 1 }),
            (Path.Combine("Test", "File1.txt"), new byte[] { 2, 2 }),
            (Path.Combine("Some Where Else", "File2.txt"), new byte[] { 3, 3, 3 }),
            (Path.Combine("Test", "Some Random Name.txt"), arrayTwo)
        );
        var fileSystem = new FileSystem();
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(rootPath, intensity)).ToList();
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Empty(comparisonResult.DuplicateSets);
    }

    [Theory]
    [InlineData(Intensity.Rough)]
    [InlineData(Intensity.Precise)]
    public async Task Can_Find_Duplicates_With_Duplicates_Many_255MiB(Intensity intensity)
    {
        const int Size = 1024 * 1024 * 255; // 255MiB

        var bytes = new byte[Size];
        _random.NextBytes(bytes);
        string rootPath = GetMethodName();
        using var fileEnsurer = new FileEnsurer();
        fileEnsurer.Setup(rootPath,
            (Path.Combine("Test", "File1.txt"), bytes),
            (Path.Combine("File1.txt"), bytes),
            (Path.Combine("File2.txt"), new byte[] { 1, 1, 1 }),
            (Path.Combine("File3.txt"), new byte[] { 2, 2, 2 }),
            (Path.Combine("File4.txt"), bytes),
            (Path.Combine("File5.txt"), new byte[] { 3, 3, 3 }),
            (Path.Combine("Some Where Else", "File2.txt"), new byte[0]),
            (Path.Combine("Test", "Some Random Name.txt"), bytes),
            (Path.Combine("Test", "Testing Name.txt"), new byte[] { 4, 4, 4 }),
            (Path.Combine("Test", "File1 - Copy.txt"), bytes),
            (Path.Combine("Test", "File2 - Copy.txt"), new byte[] { 5, 5, 5 }),
            (Path.Combine("Test", "File3 - Copy.txt"), bytes)
        );
        var fileSystem = new FileSystem();
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(rootPath, intensity)).ToList();
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Single(comparisonResult.DuplicateSets);

        var set = comparisonResult.DuplicateSets.First();
        Assert.Equal(6, set.Count);
    }
}
