using System.IO.Abstractions.TestingHelpers;

namespace DupeBuster.Misc.Benchmarks;

public class TestDataSets
{
    private static readonly Random Random = new Random(1234567890); // Deterministic Pseudo Randomness
    private string _testDirectory = null!;

    public void Init(string testDirectory)
    {
        _testDirectory = testDirectory;

        InitFewSmallFiles();
        InitLotsSmallFiles();
        InitFewBigFiles();
        InitLotsBigFiles();

        GC.Collect(); // We've just created a shit ton of strings... Get rid of them first
    }
    private void InitFewSmallFiles()
    {
        const int Count = 25;

        var fileContents = Enumerable.Range(0, Count * 100).Select(GenerateRandomString).ToList();
        var folders = Enumerable.Range(0, Count / 3).Select(x => x.ToString()).ToList();

        for (int i = 0; i < Count; i++)
        {
            var folder = folders[Random.Next(folders.Count)];
            var fileContent = fileContents[Random.Next(fileContents.Count)];
            var fullPath = Path.Combine(_testDirectory, folder, $"{i}.txt");

            FewSmallFiles.Add(fullPath, new MockFileData(fileContent));
        }
    }
    private void InitLotsSmallFiles()
    {
        const int Count = 20_000;

        var fileContents = Enumerable.Range(0, Count * 100).Select(GenerateRandomString).ToList();
        var folders = Enumerable.Range(0, Count / 3).Select(x => x.ToString()).ToList();

        for (int i = 0; i < Count; i++)
        {
            var folder = folders[Random.Next(folders.Count)];
            var fileContent = fileContents[Random.Next(fileContents.Count)];
            var fullPath = Path.Combine(_testDirectory, folder, $"{i}.txt");

            LotsSmallFiles.Add(fullPath, new MockFileData(fileContent));
        }
    }
    private void InitFewBigFiles()
    {
        const int Count = 25;

        var fileContents = Enumerable.Range(0, Count * 20_000).Select(GenerateRandomString).ToList();
        var folders = Enumerable.Range(0, Count / 3).Select(x => x.ToString()).ToList();

        for (int i = 0; i < Count; i++)
        {
            var folder = folders[Random.Next(folders.Count)];
            var fileContent = fileContents[Random.Next(fileContents.Count)];
            var fullPath = Path.Combine(_testDirectory, folder, $"{i}.txt");

            FewBigFiles.Add(fullPath, new MockFileData(fileContent));
        }
    }
    private void InitLotsBigFiles()
    {
        const int Count = 20_000;

        var fileContents = Enumerable.Range(0, Count * 20_000).Select(GenerateRandomString).ToList();
        var folders = Enumerable.Range(0, Count / 3).Select(x => x.ToString()).ToList();

        for (int i = 0; i < Count; i++)
        {
            var folder = folders[Random.Next(folders.Count)];
            var fileContent = fileContents[Random.Next(fileContents.Count)];
            var fullPath = Path.Combine(_testDirectory, folder, $"{i}.txt");

            LotsBigFiles.Add(fullPath, new MockFileData(fileContent));
        }
    }

    public readonly Dictionary<string, MockFileData> FewSmallFiles = new Dictionary<string, MockFileData>();
    public readonly Dictionary<string, MockFileData> LotsSmallFiles = new Dictionary<string, MockFileData>();
    public readonly Dictionary<string, MockFileData> FewBigFiles = new Dictionary<string, MockFileData>();
    public readonly Dictionary<string, MockFileData> LotsBigFiles = new Dictionary<string, MockFileData>();

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}