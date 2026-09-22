namespace HrAgencySystem.EmailTemplates.Messaging;

/// <summary>
/// One queue per source domain. A queue is a subscriber address, not a unit of scale — throughput is
/// added with more listeners on the same queue, so that a backlog in one domain cannot stall another.
/// </summary>
public static class EmailQueues
{
    public const string Recruitment = "q.emails.recruitment";
    public const string Identity = "q.emails.identity";
    public const string Sales = "q.emails.sales";
    public const string Teams = "q.emails.teams";

    /// <summary>The agency as an employer: hours, and the decisions taken about them.</summary>
    public const string Agency = "q.emails.agency";
}
