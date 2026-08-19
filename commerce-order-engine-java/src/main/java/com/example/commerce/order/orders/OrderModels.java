package com.example.commerce.order.orders;

import jakarta.validation.Valid;
import jakarta.validation.constraints.*;
import java.math.BigDecimal;
import java.time.Instant;
import java.util.List;

public final class OrderModels {
    private OrderModels() {}
    public record CreateItem(@Positive long productId, @NotBlank String sku, @NotBlank String productName, @DecimalMin("0") BigDecimal unitPrice, @Positive long quantity) {}
    public record CreateOrderRequest(@Positive long customerId, @Email @NotBlank String customerEmail, @NotBlank String customerName, @NotEmpty List<@Valid CreateItem> items) {}
    public record OrderItem(long id, long productId, String sku, String productName, BigDecimal unitPrice, long quantity, BigDecimal lineAmount) {}
    public record Order(long id, long customerId, String customerEmail, String customerName, String status, BigDecimal totalAmount, Instant createdAt, Instant updatedAt, List<OrderItem> items) {}
    public record OrderSummary(long id, long customerId, String customerEmail, String customerName, String status, BigDecimal totalAmount, Instant createdAt, Instant updatedAt) {}
    public record OrderPage(List<OrderSummary> items, int page, int pageSize, long totalCount) {}
    public record Payment(long id,long orderId,String transactionType,BigDecimal amount,String status,String referenceNo,Instant createdAt) {}
    public record ShipOrderRequest(@NotBlank @Size(max=100) String carrier,@NotBlank @Size(max=100) String trackingNumber) {}
    public record Shipment(long id,long orderId,String carrier,String trackingNumber,String status,Instant shippedAt,Instant deliveredAt) {}
}
