using System.Security.Cryptography;
using System.Text;

namespace HrAgencySystem.Forms.Domain;

/// <summary>
/// Ids derived from what they are about - the <c>AgencyStreamId</c> trick. "One catalogue per
/// organization", "one version document per published version" and "one response per person" for
/// a form that asks for exactly one become impossible to break rather than rules guarded by a read
/// before every write: there is nowhere for a second one to go, and two concurrent starts collide on
/// the stream instead of both succeeding.
/// </summary>
public static class FormsStreamId
{
    /// <summary>Fixed namespaces. Changing one orphans everything derived with it.</summary>
    private static readonly Guid CatalogueNamespace = Guid.Parse("4a8f2d61-93c7-4e0b-8b15-6f2e9c7d1a34");

    private static readonly Guid VersionNamespace = Guid.Parse("d17c5e2a-0b46-4f98-a3e1-5c8b27f60d19");

    private static readonly Guid ResponseNamespace = Guid.Parse("8e3b6f14-2c9d-47a5-b0f7-91d4e6a2c583");

    private static readonly Guid ProfileNamespace = Guid.Parse("b5d92c07-6e1f-4a38-9c4b-2f7a80e13d66");

    public static Guid ForCatalogue(Guid organizationId) =>
        Derive(CatalogueNamespace, $"{organizationId:N}");

    public static Guid ForVersion(Guid formId, int version) =>
        Derive(VersionNamespace, $"{formId:N}:{version}");

    public static Guid ForSingleResponse(Guid organizationId, Guid formId, SubjectRef subject) =>
        Derive(ResponseNamespace, $"{organizationId:N}:{formId:N}:{subject.Kind}:{subject.Id:N}");

    public static Guid ForProfile(Guid organizationId, SubjectRef subject) =>
        Derive(ProfileNamespace, $"{organizationId:N}:{subject.Kind}:{subject.Id:N}");

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
