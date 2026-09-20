namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// How people are put to work for the client. This is not decoration: together with the country it
/// decides which formal obligations the project carries, and the two cases differ sharply.
/// Posting an IT specialist to a German client triggers almost nothing; hiring the same person out
/// to a German user undertaking triggers a licence, a notification and document duties.
/// <para>
/// There is no <c>Recruitment</c> value - placing a candidate the client then employs is a different
/// business and nobody has asked for it here. It would arrive with an empty requirement catalogue.
/// </para>
/// </summary>
public enum EngagementType
{
    /// <summary>We remain the employer and send our own people to work for the client.</summary>
    PostingOfWorkers,

    /// <summary>We remain the employer and the client directs the work.</summary>
    TemporaryAgencyWork,
}
