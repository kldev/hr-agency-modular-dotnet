using System.Security.Cryptography;
using System.Text;

namespace HrAgencySystem.Agency.Domain;

/// <summary>
/// Stream ids derived from what they are about, the same trick <see cref="OrgStructureId"/> plays
/// for the chart.
/// <para>
/// The point is not tidiness. "One employment record per person" and "one time sheet per person per
/// month" become <em>impossible</em> to break rather than rules somebody has to guard with a
/// reservation document and a read before every write: there is nowhere else for a second one to
/// go. It also means a command can load its aggregate without first asking which id to load.
/// </para>
/// </summary>
internal static class AgencyStreamId
{
    /// <summary>Fixed namespaces. Changing one orphans every stream derived with it.</summary>
    private static readonly Guid EmploymentNamespace = Guid.Parse(
        "b2f7a41c-6d18-4c77-9a05-3e8f21d6c4a9"
    );

    private static readonly Guid TimeSheetNamespace = Guid.Parse(
        "0c5e93b7-8a24-4f61-b0d9-27e6a3f81c5d"
    );

    internal static Guid ForEmployment(Guid organizationId, Guid userId) =>
        Derive(EmploymentNamespace, $"{organizationId:N}:{userId:N}");

    internal static Guid ForTimeSheet(Guid organizationId, Guid userId, int year, int month) =>
        Derive(TimeSheetNamespace, $"{organizationId:N}:{userId:N}:{year:D4}-{month:D2}");

    private static Guid Derive(Guid space, string value)
    {
        Span<byte> prefix = stackalloc byte[16];
        space.TryWriteBytes(prefix);

        var payload = Encoding.UTF8.GetBytes(value);
        var buffer = new byte[prefix.Length + payload.Length];

        prefix.CopyTo(buffer);
        payload.CopyTo(buffer, prefix.Length);

        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(buffer, hash);

        return new Guid(hash[..16]);
    }
}
