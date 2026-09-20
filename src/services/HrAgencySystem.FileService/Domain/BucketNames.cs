namespace HrAgencySystem.FileService.Domain;

public static class BucketNames
{
    /// <summary>
    /// Private, access controlled files. Separate from the feed bucket on purpose: that one holds
    /// artefacts published for anyone to read, this one holds documents nobody may read by guessing.
    /// </summary>
    public const string Documents = "documents";
}
