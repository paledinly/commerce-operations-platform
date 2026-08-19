using Commerce.Operations.Api.Products;

namespace Commerce.Operations.Api.Inventories;

public sealed record InventoryRecord(long ProductId, long AvailableQuantity, long ReservedQuantity, long Version, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record InventoryPage(IReadOnlyList<InventoryRecord> Items, int Page, int PageSize, long TotalCount);
public sealed record InventoryView(long ProductId, string? Sku, string? ProductName, long AvailableQuantity, long ReservedQuantity, long Version, DateTime UpdatedAt);
public sealed record InventoryViewPage(IReadOnlyList<InventoryView> Items, int Page, int PageSize, long TotalCount);
public sealed record InventoryMovement(long Id, long ProductId, string MovementType, long QuantityDelta, long AvailableAfter, string Reason, DateTime CreatedAt);
public sealed record CreateInventoryRequest(long ProductId, long InitialQuantity);
public sealed record AdjustInventoryRequest(long QuantityDelta, string Reason);

