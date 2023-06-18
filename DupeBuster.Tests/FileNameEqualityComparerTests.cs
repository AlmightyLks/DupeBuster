//using DupeBuster.Core;
//using System.IO;
//using DupeBuster.Core.Comparer;
//using System;

//namespace DupeBuster.Tests;

//public class FileNameEqualityComparerTests
//{
//    private readonly FileNameEqualityComparer _comparer = FileNameEqualityComparer.Default;
//    private readonly Random _random = new Random(1234567890);

//    [Theory]
//    [InlineData(Intensity.Rough)]
//    [InlineData(Intensity.Precise)]
//    public async Task Can_Find_Duplicates_With_Duplicates(Intensity intensity)
//    {
//        const int Size = 1024 * 1024 * 50; // 50 MiB

//        var bytes = new byte[Size];
//        _random.NextBytes(bytes);

//        string rootPath = GetMethodName();
//        using var fileEnsurer = new FileEnsurer();
//        fileEnsurer.Setup(rootPath,
//            (Path.Combine("File1.txt"), bytes),
//            (Path.Combine("File2.txt"), new byte[2]),
//            (Path.Combine("Test", "File1.txt"), bytes),
//            (Path.Combine("Some Where Else", "File2.txt"), new byte[0]),
//            (Path.Combine("Test", "Some Random Name.txt"), bytes)
//            );
//        var fileSystem = new FileSystem();

//        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
//        {
//            { Path.Combine("File1.txt"), new MockFileData(Array.Empty<byte>()) },
//            { Path.Combine("File2.txt"), new MockFileData(Array.Empty<byte>()) },
//            { Path.Combine("Test", "File1.txt"), new MockFileData(Array.Empty<byte>()) },
//            { Path.Combine("Some Where Else", "File2.txt"), new MockFileData(Array.Empty<byte>()) },
//            { Path.Combine("Test", "Some Random Name.txt"), new MockFileData(Array.Empty<byte>()) }
//        });
//        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

//        var results = (await duper.FindDuplicatesAsync(RootPath, intensity)).ToList();
//        Assert.Single(results);

//        var comparisonResult = results.First();
//        Assert.Equal(_comparer.Type, comparisonResult.Type);
//        Assert.Equal(2, comparisonResult.DuplicateSets.Count);
//    }

//    [Theory]
//    [InlineData(Intensity.Rough)]
//    [InlineData(Intensity.Precise)]
//    public async Task Cannot_Find_Duplicates_Without_Duplicates(Intensity intensity)
//    {
//        const string RootPath = @"C:\Users\Public\Documents";
//        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
//        {
//            { Path.Combine(RootPath, "File1.txt"), new MockFileData(Array.Empty<byte>()) },
//            { Path.Combine(RootPath, "File2.txt"), new MockFileData(Array.Empty<byte>()  ) },
//            { Path.Combine(RootPath, "Test", "File3.txt"), new MockFileData(Array.Empty<byte>()) },
//            { Path.Combine(RootPath, "Some Where Else", "File4.txt"), new MockFileData(Array.Empty<byte>()) },
//            { Path.Combine(RootPath, "Test", "Some Random Name.txt"), new MockFileData(Array.Empty<byte>()) }
//        });
//        var duper = new DupeFinder(fileSystem).AddComparer(_comparer);

//        var results = (await duper.FindDuplicatesAsync(RootPath, intensity)).ToList();
//        Assert.Single(results);

//        var comparisonResult = results.First();
//        Assert.Equal(_comparer.Type, comparisonResult.Type);
//        Assert.Empty(comparisonResult.DuplicateSets);
//    }
//}
