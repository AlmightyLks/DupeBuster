using DupeBuster.Core;
using System.IO;
using DupeBuster.Core.Comparer;
using System;
using System.IO.Abstractions;

namespace DupeBuster.Tests;

public class FileNameEqualityComparerTests : TestBase
{
    private readonly FileNameEqualityComparer _comparer = FileNameEqualityComparer.Default;
    private readonly Random _random = new Random(1234567890);

    [Theory]
    [InlineData(Intensity.Rough)]
    [InlineData(Intensity.Precise)]
    public async Task Can_Find_Duplicates_With_Duplicates(Intensity intensity)
    {
        string rootPath = GetMethodName();
        using var fileEnsurer = new FileEnsurer();
        fileEnsurer.Setup(rootPath,
            (Path.Combine("File1.txt"), Array.Empty<byte>()),
            (Path.Combine("File2.txt"), Array.Empty<byte>()),
            (Path.Combine("Test", "File1.txt"), Array.Empty<byte>()),
            (Path.Combine("Some Where Else", "File2.txt"), Array.Empty<byte>()),
            (Path.Combine("Test", "Some Random Name.txt"), Array.Empty<byte>())
            );
        var fileSystem = new FileSystem();
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(rootPath, intensity)).ToList();
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Equal(2, comparisonResult.DuplicateSets.Count);
    }

    [Theory]
    [InlineData(Intensity.Rough)]
    [InlineData(Intensity.Precise)]
    public async Task Cannot_Find_Duplicates_Without_Duplicates(Intensity intensity)
    {
        string rootPath = GetMethodName();
        using var fileEnsurer = new FileEnsurer();
        fileEnsurer.Setup(rootPath,
            (Path.Combine("File1.txt"), Array.Empty<byte>()),
            (Path.Combine("File2.txt"), Array.Empty<byte>()),
            (Path.Combine("Test", "File3.txt"), Array.Empty<byte>()),
            (Path.Combine("Some Where Else", "File4.txt"), Array.Empty<byte>()),
            (Path.Combine("Test", "Some Random Name.txt"), Array.Empty<byte>())
            );
        var fileSystem = new FileSystem();
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(rootPath, intensity)).ToList();
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Empty(comparisonResult.DuplicateSets);
    }
}
