namespace HrAgencySystem.SharedKernel.Extensions;

public static class GuidExtensions
{
    extension(Guid? guid)
    {
        public bool IsInvalid()
        {
            return guid == null || guid == Guid.Empty;
        }
    }
}
