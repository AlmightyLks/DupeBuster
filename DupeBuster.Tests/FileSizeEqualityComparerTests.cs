using DupeBuster.Core;
using DupeBuster.Core.Comparer;
using System.IO.Abstractions;
using System.Drawing;

namespace DupeBuster.Tests;

public class FileSizeEqualityComparerTests
{
    private readonly FileSizeEqualityComparer _comparer = FileSizeEqualityComparer.Default;

    [Theory]
    [InlineData(Intensity.Rough)]
    [InlineData(Intensity.Precise)]
    public async Task Can_Find_Duplicates_With_Duplicates(Intensity intensity)
    {
        const int Size = 1024 * 1024;

        string rootPath = GetMethodName();
        using var fileEnsurer = new FileEnsurer();
        fileEnsurer.Setup(rootPath,
            (Path.Combine("File1.txt"), new byte[Size]),
            (Path.Combine("File2.txt"), new byte[2]),
            (Path.Combine("Test", "File1.txt"), new byte[Size]),
            (Path.Combine("Some Where Else", "File2.txt"), new byte[Size]),
            (Path.Combine("Test", "Some Random Name.txt"), new byte[Size])
            );
        var fileSystem = new FileSystem();

        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(rootPath, intensity)).ToList();
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Single(comparisonResult.DuplicateSets);

        var set = comparisonResult.DuplicateSets.First();
        Assert.Equal(4, set.Count);
    }

    [Theory]
    [InlineData(Intensity.Rough)]
    [InlineData(Intensity.Precise)]
    public async Task Cannot_Find_Duplicates_Without_Duplicates(Intensity intensity)
    {
        string rootPath = GetMethodName();
        using var fileEnsurer = new FileEnsurer();
        fileEnsurer.Setup(rootPath,
            (Path.Combine("File1.txt"), new byte[1]),
            (Path.Combine("File2.txt"), new byte[2]),
            (Path.Combine("Test", "File1.txt"), new byte[3]),
            (Path.Combine("Some Where Else", "File2.txt"), new byte[4]),
            (Path.Combine("Test", "Some Random Name.txt"), new byte[5])
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
