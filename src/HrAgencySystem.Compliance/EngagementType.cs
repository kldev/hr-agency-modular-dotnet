namespace HrAgencySystem.Compliance;

/// <summary>
/// How people are put to work for the client. This is not decoration: together with the country it
/// decides which formal obligations arise, and the cases differ sharply. Posting an IT specialist to
/// a German client triggers almost nothing; hiring the same person out to a German user undertaking
/// triggers a licence, a notification and document duties; employing them under German law triggers
/// neither, because nobody is being posted anywhere.
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

    /// <summary>
    /// We owe the client a result, not people: our own staff carry out a service under our own
    /// direction. Still a posting under Directive 96/71/EC, so the posting duties apply - plus the
    /// one duty the other modes do not have, which is being able to show that the client is not in
    /// fact directing the work. If they are, this was hiring out all along, without the licence.
    /// </summary>
    Outsourcing,

    /// <summary>
    /// A real employment contract under the law of the country where the work is done. Nobody is
    /// posted, so the whole posting machinery - A1, host state notifications, an authorised
    /// recipient - simply does not apply. Local labour and social security law does instead.
    /// </summary>
    LocalEmployment,
}
