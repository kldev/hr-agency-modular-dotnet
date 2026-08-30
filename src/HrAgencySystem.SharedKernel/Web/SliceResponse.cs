namespace HrAgencySystem.SharedKernel.Web;

public record SliceResponse<T>(IReadOnlyList<T> Content, bool HasMore);
