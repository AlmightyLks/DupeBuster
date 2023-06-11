using DupeBuster.Core;
using System.IO.Abstractions.TestingHelpers;
using System.IO;
using DupeBuster.Core.Comparer;

namespace DupeBuster.Tests;

public class FileNameEqualityComparerTests
{
    private readonly FileNameEqualityComparer _comparer = FileNameEqualityComparer.Default;

    [Theory]
    [InlineData(Intensity.Rough)]
    [InlineData(Intensity.Precise)]
    public async Task Can_Find_Duplicates_With_Duplicates(Intensity intensity)
    {
        const string RootPath = @"C:\";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { Path.Combine(RootPath, "File1.txt"), new MockFileData(Array.Empty<byte>()) },
            { Path.Combine(RootPath, "File2.txt"), new MockFileData(Array.Empty<byte>()) },
            { Path.Combine(RootPath, "Test", "File1.txt"), new MockFileData(Array.Empty<byte>()) },
            { Path.Combine(RootPath, "Some Where Else", "File2.txt"), new MockFileData(Array.Empty<byte>()) },
            { Path.Combine(RootPath, "Test", "Some Random Name.txt"), new MockFileData(Array.Empty<byte>()) }
        });
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(RootPath, intensity)).ToList();
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
        const string RootPath = @"C:\";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { Path.Combine(RootPath, "File1.txt"), new MockFileData(Array.Empty<byte>()) },
            { Path.Combine(RootPath, "File2.txt"), new MockFileData(Array.Empty<byte>()  ) },
            { Path.Combine(RootPath, "Test", "File3.txt"), new MockFileData(Array.Empty<byte>()) },
            { Path.Combine(RootPath, "Some Where Else", "File4.txt"), new MockFileData(Array.Empty<byte>()) },
            { Path.Combine(RootPath, "Test", "Some Random Name.txt"), new MockFileData(Array.Empty<byte>()) }
        });
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(RootPath, intensity)).ToList();
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Empty(comparisonResult.DuplicateSets);
    }
}
