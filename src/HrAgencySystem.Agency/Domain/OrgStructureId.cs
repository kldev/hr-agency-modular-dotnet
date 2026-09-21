using System.Security.Cryptography;

namespace HrAgencySystem.Agency.Domain;

/// <summary>
/// The stream the chart lives on: one per organization, derived from the organization's id rather
/// than equal to it.
/// <para>
/// Derived, because Marten's streams share one table across every module and the
/// <c>Organization</c> aggregate already owns the stream whose id is the organization id. Two
/// aggregates cannot sit on one stream, and the collision surfaces as
/// "Stream #… already exists" on the very first write - which says nothing about what really
/// happened.
/// </para>
/// <para>
/// A hash rather than a stored lookup: the answer has to be the same on every node and every
/// replay, and a lookup document would be one read before every single command.
/// </para>
/// </summary>
public static class OrgStructureId
{
    /// <summary>Fixed namespace, so the derivation never changes meaning. Do not edit.</summary>
    private static readonly Guid Namespace = Guid.Parse("6f1d0c2e-5a2b-4f8e-9d3a-7c5b1e4a9f20");

    public static Guid For(Guid organizationId)
    {
        Span<byte> buffer = stackalloc byte[32];

        Namespace.TryWriteBytes(buffer[..16]);
        organizationId.TryWriteBytes(buffer[16..]);

        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(buffer, hash);

        return new Guid(hash[..16]);
    }
}
