namespace HrAgencySystem.EmailTemplates.UnitTests;

public static class TestDirectoryHelper
{
    public static string GetTempDirectory()
    {
        var testProjectPath = AppContext.BaseDirectory;
        var parentFullName = Directory
            .GetParent(testProjectPath)!
            .Parent!.Parent?.Parent?.Parent?.FullName;
        if (parentFullName == null)
            throw new Exception("Temp directory parent directory not found");
        var tempDirectory = Path.Combine(parentFullName, ".temp");

        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        return tempDirectory;
    }
}
