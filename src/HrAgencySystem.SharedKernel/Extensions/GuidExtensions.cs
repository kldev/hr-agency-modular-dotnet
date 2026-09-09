namespace HrAgencySystem.SharedKernel.Extensions;

public static class GuidExtensions
{
    extension(Guid? guid)
    {
        public bool IsValid()
        {
            return guid != null && guid != Guid.Empty;
        }
        
        public bool IsInvalid()
        {
            return guid == null || guid == Guid.Empty;
        }
    }
}