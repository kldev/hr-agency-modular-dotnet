namespace HrAgencySystem.Agency.Domain;

/// <summary>
/// Who answers for whom. The one rule the whole module exists to serve, kept pure so it can be
/// read and tested without a database.
/// <para>
/// A supervisor is never written down. It follows from where somebody sits: the head of their
/// unit, and if they are that head, the head of the unit above. Walking up handles two cases in
/// one sentence, which is why it is one sentence — the head who reports a floor up, and the unit
/// with no head of its own that the department above answers for. Requiring a head at every level
/// would mean naming the same person as head of three units, and then moving a department costs
/// three corrections.
/// </para>
/// </summary>
public static class SupervisorPolicy
{
    /// <summary>
    /// The supervisor of a person, or null when nobody is above them - the head of the root unit
    /// has no supervisor, and that is an answer rather than a missing value.
    /// </summary>
    public static Guid? SupervisorOf(IReadOnlyList<OrgUnit> units, Guid userId)
    {
        var unit = units.FirstOrDefault(candidate => candidate.HasMember(userId));

        if (unit is null)
            return null;

        return WalkUp(units, unit, userId);
    }

    /// <summary>
    /// Everybody this person answers for: the members of every unit they head, and - when the
    /// whole subtree is asked for - of everything under those units. Themselves excluded, because
    /// nobody is their own subordinate.
    /// </summary>
    public static IReadOnlyList<Guid> SubordinatesOf(
        IReadOnlyList<OrgUnit> units,
        Guid userId,
        bool wholeSubtree
    )
    {
        var headed = units.Where(unit => unit.HeadUserId == userId && !unit.IsArchived).ToList();

        if (headed.Count == 0)
            return [];

        var reached = new List<OrgUnit>();

        foreach (var unit in headed)
        {
            reached.Add(unit);

            if (wholeSubtree)
                reached.AddRange(Descendants(units, unit));
        }

        // A unit reached through two heads would list its people twice; a head who is also a member
        // of a unit they head would list themselves.
        return
        [
            .. reached
                .DistinctBy(unit => unit.UnitId)
                .SelectMany(unit => unit.Members.Select(member => member.UserId))
                .Where(id => id != userId)
                .Distinct(),
        ];
    }

    /// <summary>
    /// Everything under a unit. Used by the move rule as well: a unit cannot be moved under its own
    /// descendant, which is the only way a tree of parents can be made to bite its own tail.
    /// </summary>
    public static IReadOnlyList<OrgUnit> Descendants(IReadOnlyList<OrgUnit> units, OrgUnit unit)
    {
        var found = new List<OrgUnit>();
        var queue = new Queue<Guid>();

        queue.Enqueue(unit.UnitId);

        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();

            foreach (var child in units.Where(candidate => candidate.ParentId == parentId))
            {
                found.Add(child);
                queue.Enqueue(child.UnitId);
            }
        }

        return found;
    }

    /// <summary>How deep a unit sits, counting the root as zero. Straight from the parent chain.</summary>
    public static int DepthOf(IReadOnlyList<OrgUnit> units, OrgUnit unit)
    {
        var depth = 0;
        var current = unit;

        while (current.ParentId is { } parentId)
        {
            var parent = units.FirstOrDefault(candidate => candidate.UnitId == parentId);

            if (parent is null)
                break;

            depth++;
            current = parent;
        }

        return depth;
    }

    /// <summary>The units from the root down to this one, this one last.</summary>
    public static IReadOnlyList<Guid> PathOf(IReadOnlyList<OrgUnit> units, OrgUnit unit)
    {
        var path = new List<Guid> { unit.UnitId };
        var current = unit;

        while (current.ParentId is { } parentId)
        {
            var parent = units.FirstOrDefault(candidate => candidate.UnitId == parentId);

            if (parent is null)
                break;

            path.Insert(0, parent.UnitId);
            current = parent;
        }

        return path;
    }

    /// <summary>
    /// Whether one person stands above another anywhere in the chart - their direct supervisor, or
    /// anybody further up the same line.
    /// <para>
    /// Asked at the moment of the action rather than frozen onto the sheet, and that is deliberate:
    /// a month is closed once and whoever is the supervisor <em>now</em> is the one who should be
    /// closing it. A leave request is the opposite case and freezes its approver, because a request
    /// waiting for an answer must not change hands underneath the person deciding.
    /// </para>
    /// </summary>
    public static bool IsAbove(IReadOnlyList<OrgUnit> units, Guid supervisorId, Guid userId)
    {
        if (supervisorId == userId)
            return false;

        var unit = units.FirstOrDefault(candidate => candidate.HasMember(userId));

        if (unit is null)
            return false;

        var current = unit;

        while (true)
        {
            // Their own unit's head counts only when it is somebody else - otherwise the walk would
            // stop at the person asking and never reach whoever is actually above them.
            if (current.HeadUserId is { } head && head != userId && head == supervisorId)
                return true;

            if (current.ParentId is not { } parentId)
                return false;

            var parent = units.FirstOrDefault(candidate => candidate.UnitId == parentId);

            if (parent is null)
                return false;

            current = parent;
        }
    }

    private static Guid? WalkUp(IReadOnlyList<OrgUnit> units, OrgUnit unit, Guid userId)
    {
        var current = unit;

        while (true)
        {
            if (current.HeadUserId is { } head && head != userId)
                return head;

            if (current.ParentId is not { } parentId)
                return null;

            var parent = units.FirstOrDefault(candidate => candidate.UnitId == parentId);

            if (parent is null)
                return null;

            current = parent;
        }
    }
}
