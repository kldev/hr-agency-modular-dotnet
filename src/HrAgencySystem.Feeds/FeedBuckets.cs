namespace HrAgencySystem.Feeds;

/// <summary>
/// Where the generated feed files live. Owned by this module rather than by the storage library: the
/// library knows how to put an object in a bucket, not which buckets this product has.
/// </summary>
public static class FeedBuckets
{
    public const string Jobs = "jobs-feed";
}
