namespace HrAgencySystem.SharedKernel.Web;

public interface IPagedQuery
{
    int Page { get; }
    int PageSize { get; }
}