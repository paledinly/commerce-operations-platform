namespace Commerce.Operations.Api.Dashboard;
public sealed record EngineDashboard(long TotalOrders,decimal NetRevenue,IReadOnlyDictionary<string,long> OrdersByStatus,long AvailableQuantity,long ReservedQuantity,long LowStockProducts,long PendingEvents);
public sealed record OperationsDashboard(long TotalProducts,long ActiveProducts,long TotalCustomers,long ActiveCustomers,long TotalOrders,decimal NetRevenue,IReadOnlyDictionary<string,long> OrdersByStatus,long AvailableQuantity,long ReservedQuantity,long LowStockProducts,long PendingEvents,DateTime GeneratedAtUtc);
