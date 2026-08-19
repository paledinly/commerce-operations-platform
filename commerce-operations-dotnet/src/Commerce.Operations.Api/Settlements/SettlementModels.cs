namespace Commerce.Operations.Api.Settlements;
public sealed record DailySettlement(DateOnly SettlementDate,decimal PaymentAmount,decimal RefundAmount,decimal NetAmount,long PaymentCount,long RefundCount,DateTime CalculatedAt);
public sealed record SettlementPage(IReadOnlyList<DailySettlement> Items,DateOnly From,DateOnly To);
public sealed record RebuildResult(DateOnly From,DateOnly To,int RebuiltDays);
