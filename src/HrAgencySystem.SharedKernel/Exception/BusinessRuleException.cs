namespace HrAgencySystem.SharedKernel.Exception;

public sealed class BusinessRuleException(string message) : System.Exception(message);