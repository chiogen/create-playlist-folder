namespace CreatePlaylistFolder;

internal enum ExitCode : int
{
    Success = 0,
    Error = 1,
    InvalidArguments = 2,
    PlaylistFileNotExisting = 3,
    OutputDirectoryNotExisting = 4,
}

internal class Program
{
    static int Main(string[] args)
    {
        try
        {

            if (args.Length != 2)
            {
                Console.Error.WriteLine("Invalid arguments length.");
                PrintHelp();
                return (int)ExitCode.InvalidArguments;
            }

            string pathToPlaylistFile = args[0];
            string pathToOutputFolder = args[1];

            if (!File.Exists(pathToPlaylistFile))
            {
                Console.Error.WriteLine("Playlist file doesnt exist.");
                PrintHelp();

                return (int)ExitCode.PlaylistFileNotExisting;
            }

            FileInfo playlistFile = new(pathToPlaylistFile);
            DirectoryInfo outputFolder = new(pathToOutputFolder);

            Execute(playlistFile, outputFolder);

            return (int)ExitCode.Success;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.ToString());
            return (int)ExitCode.Error;
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("CreatePlaylistFolder.exe $PathToPlaylistFile $PathToOutputDirectory");
    }

    static void Execute(FileInfo playlistFile, DirectoryInfo outputFolder)
    {
        List<(int, FileInfo)> inventory = CollectPlaylistFiles(playlistFile);

        if (outputFolder.Exists)
            ClearFolder(outputFolder);
        else
            outputFolder.Create();

        CopyInventory(inventory, outputFolder);
    }

    static List<(int, FileInfo)> CollectPlaylistFiles(FileInfo playlistFile)
    {
        using FileStream input = playlistFile.OpenRead();
        using StreamReader reader = new StreamReader(input);

        DirectoryInfo? playlistStartFolder = playlistFile.Directory;
        List<(int, FileInfo)> inventory = [];
        string? line;
        int index = 0;

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();

            // Ignore comment lines
            if (line.StartsWith('#'))
                continue;

            string filePath = line;
            bool isRelativePath = line.StartsWith('.');

            if (isRelativePath && playlistStartFolder != null)
                filePath = Path.Join(playlistStartFolder.FullName, filePath);

            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"File {line} does not exist. Omitting.");
                continue;
            }

            (int, FileInfo) entry = (index++, new FileInfo(filePath));
            inventory.Add(entry);
        }

        return inventory;
    }

    static void ClearFolder(DirectoryInfo directory, int depth = 0)
    {
        if (depth == 0)
            Console.WriteLine("Clearing output folder");

        foreach (var file in directory.GetFiles())
            file.Delete();

        foreach (var subDirectory in directory.GetDirectories())
            ClearFolder(subDirectory, depth + 1);

        if (depth > 0)
            directory.Delete();
    }
    static void CopyInventory(List<(int, FileInfo)> inventory, DirectoryInfo outputFolder)
    {
        foreach (var (index, file) in inventory)
        {
            string prefix = GetFilePrefix(inventory.Count, index);
            string outputFilename = $"{prefix} - {file.Name}";
            string outputPath = Path.Join(outputFolder.FullName, outputFilename);

            Console.WriteLine($"Copying {file.Name}");
            file.CopyTo(outputPath);
        }
    }
    static string GetFilePrefix(int entriesCount, int index)
    {
        string prefix = index.ToString();

        int expectedPrefixLength = 1;
        if (entriesCount >= 10)
            expectedPrefixLength = 2;
        if (entriesCount >= 100)
            expectedPrefixLength = 3;
        if (entriesCount >= 1000)
            expectedPrefixLength = 4;

        while (prefix.Length < expectedPrefixLength)
            prefix = "0" + prefix;

        return prefix;
    }


}
