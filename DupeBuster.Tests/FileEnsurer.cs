namespace DupeBuster.Tests;

internal class FileEnsurer : IDisposable
{
    private readonly List<string> _directories = new List<string>();

    public void Setup(string directory, params (string FilePath, byte[] Data)[] files)
    {
        directory = Path.GetFullPath(directory);

        _directories.Add(directory);
        Directory.CreateDirectory(directory);

        foreach (var file in files)
        {
            var fileDirectory = Path.GetFullPath(file.FilePath);
            fileDirectory = Directory.GetParent(fileDirectory)!.Name;
            fileDirectory = Path.Combine(directory, fileDirectory);
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            var fullFilePath = Path.GetFullPath(Path.Combine(directory, file.FilePath));
            File.WriteAllBytes(fullFilePath, file.Data);
        }
    }

    public void Dispose()
    {
        foreach (var directory in _directories)
            Directory.Delete(directory, true);
    }
}
