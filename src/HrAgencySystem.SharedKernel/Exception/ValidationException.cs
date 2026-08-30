namespace HrAgencySystem.SharedKernel.Exception;

public class ValidationException(List<string> errors) : System.Exception(string.Join(Environment.NewLine, errors))
{
    public ValidationException(string error) : this([error]){}
    
    public IReadOnlyCollection<string> Errors { get; } = errors;
}