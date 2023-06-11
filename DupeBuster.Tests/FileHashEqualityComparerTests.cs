using DupeBuster.Core;
using System.IO.Abstractions.TestingHelpers;
using DupeBuster.Core.Comparer;

namespace DupeBuster.Tests;

public class FileHashEqualityComparerTests
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

        const string RootPath = @"C:\";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { Path.Combine(RootPath, "File1.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "File2.txt"), new MockFileData(new byte[2]) },
            { Path.Combine(RootPath, "Test", "File1.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "Some Where Else", "File2.txt"), new MockFileData(new byte[0]) },
            { Path.Combine(RootPath, "Test", "Some Random Name.txt"), new MockFileData(bytes) }
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
        const int Size = 1024 * 1024 * 50; // 50 MiB

        var arrayOne = new byte[Size];
        var arrayTwo = new byte[Size];
        for (int i = 0; i < arrayOne.Length; i++)
            arrayOne[i] = 1;
        for (int i = 0; i < arrayTwo.Length; i++)
            arrayTwo[i] = 2;

        const string RootPath = @"C:\";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { Path.Combine(RootPath, "File1.txt"), new MockFileData(arrayOne) },
            { Path.Combine(RootPath, "File2.txt"), new MockFileData(new byte[2]) },
            { Path.Combine(RootPath, "Test", "File1.txt"), new MockFileData(new byte[3]) },
            { Path.Combine(RootPath, "Some Where Else", "File2.txt"), new MockFileData(new byte[4]) },
            { Path.Combine(RootPath, "Test", "Some Random Name.txt"), new MockFileData(arrayTwo) }
        });
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(RootPath, intensity)).ToList();
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Empty(comparisonResult.DuplicateSets);
    }
    
    [Theory]
    [InlineData(Intensity.Rough)]
    [InlineData(Intensity.Precise)]
    public async Task Can_Find_Duplicates_With_Duplicates_Few_1GiB(Intensity intensity)
    {
        const int Size = 1024 * 1024 * 1024 * 1; // 1GiB

        var bytes = new byte[Size];
        _random.NextBytes(bytes);

        const string RootPath = @"C:\";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { Path.Combine(RootPath, "File1.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "File2.txt"), new MockFileData(new byte[2]) },
            { Path.Combine(RootPath, "Test", "File1.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "Some Where Else", "File2.txt"), new MockFileData(new byte[0]) },
            { Path.Combine(RootPath, "Test", "Some Random Name.txt"), new MockFileData(bytes) }
        });
        GC.Collect();
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
    public async Task Can_Find_Duplicates_With_Duplicates_Many_250MiB(Intensity intensity)
    {
        const int Size = 1024 * 1024 * 250; // 250MiB

        var bytes = new byte[Size];
        _random.NextBytes(bytes);

        const string RootPath = @"C:\";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { Path.Combine(RootPath, "File1.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "File2.txt"), new MockFileData(new byte[2]) },
            { Path.Combine(RootPath, "File3.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "File4.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "File5.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "Test", "File1.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "Some Where Else", "File2.txt"), new MockFileData(new byte[0]) },
            { Path.Combine(RootPath, "Test", "Some Random Name.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "Test", "Testing Name.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "Test", "File1 - Copy.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "Test", "File2 - Copy.txt"), new MockFileData(bytes) },
            { Path.Combine(RootPath, "Test", "File3 - Copy.txt"), new MockFileData(bytes) }
        });
        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

        var results = (await duper.FindDuplicatesAsync(RootPath, intensity)).ToList();
        GC.Collect(0, GCCollectionMode.Default, true, true);
        Assert.Single(results);

        var comparisonResult = results.First();
        Assert.Equal(_comparer.Type, comparisonResult.Type);
        Assert.Single(comparisonResult.DuplicateSets);

        var set = comparisonResult.DuplicateSets.First();
        Assert.Equal(10, set.Count);
    }
}
