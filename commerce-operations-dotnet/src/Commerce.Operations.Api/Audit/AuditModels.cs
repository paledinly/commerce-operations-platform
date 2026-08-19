namespace Commerce.Operations.Api.Audit;
public sealed record AuditLog(long Id,long? OperatorId,string OperatorEmail,string HttpMethod,string RequestPath,string ResourceType,int StatusCode,long DurationMs,string? IpAddress,string? UserAgent,DateTime CreatedAt);
public sealed record AuditLogPage(IReadOnlyList<AuditLog> Items,int Page,int PageSize,long TotalCount);
public sealed record AuditQuery(DateTime? From,DateTime? To,string? ResourceType,string? Result,int Page=1,int PageSize=20);
