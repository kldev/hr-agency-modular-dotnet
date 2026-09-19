namespace HrAgencySystem.SharedKernel.Extensions;

public static class EnsureNotDefaultExtensions
{
    extension(Guid value)
    {
        public Guid EnsureNotDefault(string paramName)
        {
            return value == Guid.Empty
                ? throw new ArgumentException($"{paramName} cannot be default.", paramName)
                : value;
        }
    }
}
