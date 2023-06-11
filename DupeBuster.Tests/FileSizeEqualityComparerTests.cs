using DupeBuster.Core;
using System.IO.Abstractions.TestingHelpers;
using DupeBuster.Core.Comparer;

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

        const string RootPath = @"C:\";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { Path.Combine(RootPath, "File1.txt"), new MockFileData(new byte[Size]) },
            { Path.Combine(RootPath, "File2.txt"), new MockFileData(new byte[2]) },
            { Path.Combine(RootPath, "Test", "File1.txt"), new MockFileData(new byte[Size]) },
            { Path.Combine(RootPath, "Some Where Else", "File2.txt"), new MockFileData(new byte[0]) },
            { Path.Combine(RootPath, "Test", "Some Random Name.txt"), new MockFileData(new byte[Size]) }
        });
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(RootPath, intensity)).ToList();
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
        const string RootPath = @"C:\";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { Path.Combine(RootPath, "File1.txt"), new MockFileData(new byte[1]) },
            { Path.Combine(RootPath, "File2.txt"), new MockFileData(new byte[2]) },
            { Path.Combine(RootPath, "Test", "File1.txt"), new MockFileData(new byte[3]) },
            { Path.Combine(RootPath, "Some Where Else", "File2.txt"), new MockFileData(new byte[4]) },
            { Path.Combine(RootPath, "Test", "Some Random Name.txt"), new MockFileData(new byte[5]) }
        });
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(RootPath, intensity)).ToList();
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Empty(comparisonResult.DuplicateSets);
    }
}
