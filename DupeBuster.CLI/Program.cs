using DupeBuster.Core;
using System.Diagnostics;
using System.IO.Abstractions;

var path = @"C:\Users\Wholesome\Documents\Dupes_Test";

var finder = new DupeFinder(new FileSystem());
finder.AddIdentifier(new FileNameIdentifier());

var sw = Stopwatch.GetTimestamp();
var dupes = await finder.FindDuplicatesAsync(path, Intensity.Rough);
var diff = Stopwatch.GetElapsedTime(sw);

Console.WriteLine($"Took {diff.TotalMilliseconds} ms");
Console.WriteLine();

foreach (var dupe in dupes)
{
    Console.WriteLine(dupe.Type);
    foreach (var lalala in dupe.Values)
    {
        Console.WriteLine("\t" + lalala.FileInfo.FullName);
    }
    Console.WriteLine();
}

Console.WriteLine();
