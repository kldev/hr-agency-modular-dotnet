namespace HrAgencySystem.SharedKernel.Exception;

public sealed class NotFoundException(string message) : System.Exception(message)
{
    public NotFoundException(string domain, Guid key) : this($"{domain} not found by {key}") {}
    public NotFoundException(string domain, string key) : this($"{domain} not found by {key}") {}
}