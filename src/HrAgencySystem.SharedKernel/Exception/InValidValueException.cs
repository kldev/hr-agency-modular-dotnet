namespace HrAgencySystem.SharedKernel.Exception;

public sealed class InValidValueException(string message): System.Exception(message);