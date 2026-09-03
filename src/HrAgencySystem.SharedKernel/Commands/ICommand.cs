namespace HrAgencySystem.SharedKernel.Commands;

public interface ICommand
{
}

public interface ICreateCommand : ICommand
{
    Guid CreatedBy { get; }
}

public interface IUpdateCommand : ICommand
{
    Guid ModifiedBy { get; }
}