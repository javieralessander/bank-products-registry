namespace BankProductsRegistry.Api.Configuration;

public static class DotEnvLoader
{
    public static void LoadIfExists(string filePath = ".env")
    {
        var resolvedFilePath = ResolveFilePath(filePath);
        if (resolvedFilePath is null)
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(resolvedFilePath))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) ||
                 (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string? ResolveFilePath(string filePath)
    {
        if (Path.IsPathRooted(filePath) && File.Exists(filePath))
        {
            return filePath;
        }

        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), filePath),
            Path.Combine(AppContext.BaseDirectory, filePath)
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            var found = SearchUpward(fullPath);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private static string? SearchUpward(string startingPath)
    {
        var directory = Directory.Exists(startingPath)
            ? new DirectoryInfo(startingPath)
            : new FileInfo(startingPath).Directory;

        while (directory is not null)
        {
            var envPath = Path.Combine(directory.FullName, ".env");
            if (File.Exists(envPath))
            {
                return envPath;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
