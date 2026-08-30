namespace HrAgencySystem.SharedKernel.Exception;

public sealed class NotFoundException(string message) : System.Exception(message);