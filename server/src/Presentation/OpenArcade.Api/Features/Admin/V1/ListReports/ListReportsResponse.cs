namespace OpenArcade.Api.Features.Admin.V1.ListReports;

/// <param name="TargetType">Game or Comment.</param>
public sealed record ListReportsResponse(Guid Id, Guid ReporterId, string TargetType, Guid TargetId, string Reason, DateTimeOffset CreatedAt);
