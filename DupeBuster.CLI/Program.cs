using DupeBuster.Core;
using DupeBuster.Core.Comparer;
using System.Diagnostics;
using System.IO.Abstractions;

//var path = @"C:\Users\Wholesome\Documents\Dupes_Test\1";
var path = Path.Combine("Assets");

var finder = new DupeFinder(new FileSystem());
finder.AddComparer(new FileHashEqualityComparer());

var sw = Stopwatch.GetTimestamp();
var results = await finder.FindDuplicatesAsync(path, Intensity.Rough);
var diff = Stopwatch.GetElapsedTime(sw);

Console.WriteLine($"Took {diff.TotalMilliseconds} ms");
Console.WriteLine();

foreach (var result in results)
{
    Console.WriteLine(result.Type);
    Console.WriteLine();
    foreach (var duplicateSet in result.DuplicateSets)
    {
        foreach (var duplicate in duplicateSet)
        {
            Console.WriteLine("\t" + duplicate.FullName);
        }
        Console.WriteLine();
    }
    Console.WriteLine("============================");
}

Console.WriteLine();
