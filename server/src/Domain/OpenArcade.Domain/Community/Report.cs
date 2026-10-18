using OpenArcade.Domain.Common;

namespace OpenArcade.Domain.Community;

public enum ReportTarget { Game, Comment }
public enum ReportStatus { Open, Resolved, Dismissed }

/// <summary>A player's report of a game or comment, handled in the admin queue.</summary>
public sealed class Report
{
    public const int MaxReasonLength = 500;

    public Guid Id { get; internal set; }
    public Guid ReporterId { get; internal set; }
    public ReportTarget TargetType { get; internal set; }
    public Guid TargetId { get; internal set; }
    public string Reason { get; internal set; } = "";
    public ReportStatus Status { get; internal set; }
    public Guid? HandledBy { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }

    internal Report() { }

    public static Report File(Guid reporterId, ReportTarget targetType, Guid targetId, string reason, DateTimeOffset now)
    {
        var text = reason?.Trim() ?? "";
        if (text.Length == 0) throw new DomainRuleException("A reason is required.");

        return new Report
        {
            Id = Guid.CreateVersion7(),
            ReporterId = reporterId,
            TargetType = targetType,
            TargetId = targetId,
            Reason = text[..Math.Min(text.Length, MaxReasonLength)],
            Status = ReportStatus.Open,
            CreatedAt = now,
        };
    }

    public void Close(ReportStatus outcome, Guid adminId)
    {
        if (outcome == ReportStatus.Open) throw new ArgumentException("Close with Resolved or Dismissed.", nameof(outcome));
        if (Status != ReportStatus.Open) throw new DomainRuleException($"The report is already {Status}.");
        Status = outcome;
        HandledBy = adminId;
    }
}

public interface IReportRepository
{
    Task<Report?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Report>> ListOpenAsync(int take, CancellationToken ct = default);
    void Add(Report report);
    void Update(Report report);
}
