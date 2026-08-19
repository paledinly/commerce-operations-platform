package com.example.commerce.order.inventory;

import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import java.time.Instant;
import java.util.List;

public final class InventoryModels {
    private InventoryModels() {}
    public record Inventory(long productId, long availableQuantity, long reservedQuantity, long version, Instant createdAt, Instant updatedAt) {}
    public record Movement(long id, long productId, String movementType, long quantityDelta, long availableAfter, String reason, Instant createdAt) {}
    public record InventoryPage(List<Inventory> items, int page, int pageSize, long totalCount) {}
    public record CreateInventoryRequest(@NotNull Long productId, @Min(0) long initialQuantity) {}
    public record AdjustInventoryRequest(long quantityDelta, @NotBlank @Size(max=200) String reason) {}
}

