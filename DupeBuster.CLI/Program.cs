using DupeBuster.Core;
using System.IO.Abstractions;

var path = @"C:\Users\Wholesome\Documents\Dupes_Test";

var finder = new DupeFinder(new FileSystem());

var dupes = await finder.FindDuplicatesAsync(path, new FileNameIdentifier(), Intensity.Rough);

foreach (var dupe in dupes)
{
    Console.WriteLine(dupe.Key);
    foreach (var lalala in dupe)
    {
        Console.WriteLine("\t" + lalala.FileInfo.FullName);
    }
    Console.WriteLine();
}

Console.WriteLine();